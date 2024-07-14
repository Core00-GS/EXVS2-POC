﻿using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using nue.protocol.exvs;
using Server.Commands.SaveBattle;
using Server.Mappers.Context;
using Server.Models.Cards;
using Server.Persistence;
using WebUI.Shared.Dto.Enum;

namespace Server.Handlers.Game;

public record SaveVsmResultCommand(Request Request) : IRequest<Response>;

public class SaveVsmResultCommandHandler : IRequestHandler<SaveVsmResultCommand, Response>
{
    private readonly ILogger<SaveVsmResultCommandHandler> _logger;
    private readonly ServerDbContext _context;

    public SaveVsmResultCommandHandler(ILogger<SaveVsmResultCommandHandler> logger, ServerDbContext context)
    {
        _logger = logger;
        _context = context;
    }
    
    public Task<Response> Handle(SaveVsmResultCommand request, CancellationToken cancellationToken)
    {
        var sessionId = request.Request.save_vsm_result.SessionId;
        
        var cardProfile = _context.CardProfiles
            .Include(x => x.PilotDomain)
            .Include(x => x.UserDomain)
            .FirstOrDefault(x => x.SessionId == sessionId);

        if (cardProfile == null)
        {
            return Task.FromResult(new Response
            {
                Type = request.Request.Type,
                RequestId = request.Request.RequestId,
                Error = Error.ErrServer,
                save_vsm_result = new Response.SaveVsmResult()
            });
        }
        
        var pilotId = request.Request.save_vsm_result.PilotId;
        
        var loadPlayer = JsonConvert.DeserializeObject<Response.PreLoadCard.LoadPlayer>(cardProfile.PilotDomain.LoadPlayerJson);
        var user = JsonConvert.DeserializeObject<Response.PreLoadCard.MobileUserGroup>(cardProfile.UserDomain.UserJson);
        var pilotData = JsonConvert.DeserializeObject<Response.LoadCard.PilotDataGroup>(cardProfile.PilotDomain.PilotDataGroupJson);
        var mobileUser = JsonConvert.DeserializeObject<Response.LoadCard.MobileUserGroup>(cardProfile.UserDomain.MobileUserGroupJson);

        if (loadPlayer == null || user == null || pilotData == null || mobileUser == null)
        {
            return Task.FromResult(new Response
            {
                Type = request.Request.Type,
                RequestId = request.Request.RequestId,
                Error = Error.ErrServer,
                save_vsm_result = new Response.SaveVsmResult()
            });
        }
        
        
        var resultFromRequest = request.Request.save_vsm_result.Result;
        
        var battleResultContext = resultFromRequest.ToBattleResultContext();
        battleResultContext.CommonDomain.BattleMode = request.Request.save_vsm_result.ShuffleFlag ? BattleModeConstant.OfflineSolo : BattleModeConstant.OfflineTeam;
        
        var favouriteMsList = user.FavoriteMobileSuits;
        
        var oldEchelonId = loadPlayer.EchelonId;

        var pvpBattleResult = new OfflinePvpBattleResult()
        {
            Mode = "OfflinePvp",
            OfflineBattleMode = request.Request.save_vsm_result.ShuffleFlag ? "Shuffle" : "Team",
            WinFlag = resultFromRequest.WinFlag,
            Score = resultFromRequest.ResultScore,
            UsedMsId = resultFromRequest.MstMobileSuitId,
            UsedBurstType = resultFromRequest.BurstType,
            ElapsedSecond = resultFromRequest.VsElapsedTime,
            PastEchelonId = oldEchelonId,
            EchelonExpChange = resultFromRequest.EchelonExp,
            EchelonIdAfterBattle = resultFromRequest.EchelonId,
            TotalEchelonExp = resultFromRequest.EchelonExp + loadPlayer.EchelonExp,
            SEchelonFlag = resultFromRequest.SEchelonFlag,
            SEchelonProgress = resultFromRequest.SEchelonProgress,
            
            FullBattleResultJson = JsonConvert.SerializeObject(resultFromRequest)
        };

        SavePartnerAndFoeData(resultFromRequest, pvpBattleResult);
        
        user.Gp += resultFromRequest.Gp;

        UpdateWinLossCount(request, resultFromRequest, loadPlayer);
        UpdateNaviFamiliarity(resultFromRequest, user);
        UpsertMsUsedNum(resultFromRequest, pilotData, favouriteMsList);
        
        UpdateTagTeam(resultFromRequest, pilotId);
        
        var saveBattleDataCommands = new List<ISaveBattleCommand>()
        {
            new SaveEchelonCommand(_context)
        };
        
        saveBattleDataCommands.ForEach(command => command.Save(cardProfile, loadPlayer, user, pilotData, mobileUser, battleResultContext));
        
        cardProfile.OfflinePvpBattleResults.Add(pvpBattleResult);
        cardProfile.PilotDomain.LoadPlayerJson = JsonConvert.SerializeObject(loadPlayer);
        cardProfile.PilotDomain.PilotDataGroupJson = JsonConvert.SerializeObject(pilotData);
        cardProfile.UserDomain.UserJson = JsonConvert.SerializeObject(user);
        cardProfile.UserDomain.MobileUserGroupJson = JsonConvert.SerializeObject(mobileUser);
        
        _context.SaveChanges();
        
        return Task.FromResult(new Response
        {
            Type = request.Request.Type,
            RequestId = request.Request.RequestId,
            Error = Error.Success,
            save_vsm_result = new Response.SaveVsmResult()
        });
    }

    void SavePartnerAndFoeData(Request.SaveVsmResult.PlayResultGroup resultFromRequest, OfflinePvpBattleResult pvpBattleResult)
    {
        
        
        if (resultFromRequest.Partner is not null)
        {
            if (resultFromRequest.Partner.CpuFlag == 0)
            {
                pvpBattleResult.PartnerIndicator = PlayerIndicator.Player;
                pvpBattleResult.PartnerPilotId = resultFromRequest.Partner.PilotId;
                pvpBattleResult.PartnerMsId = resultFromRequest.Partner.MstMobileSuitId;
                pvpBattleResult.PartnerEchelonId = resultFromRequest.Partner.EchelonId;
                pvpBattleResult.PartnerBurstType = resultFromRequest.Partner.BurstType;
            }
            else
            {
                pvpBattleResult.PartnerIndicator = PlayerIndicator.Cpu;
            }
        }
        else
        {
            pvpBattleResult.PartnerIndicator = PlayerIndicator.Discarded;
        }

        if (resultFromRequest.Foes is null)
        {
            return;
        }

        if (resultFromRequest.Foes.Count == 0)
        {
            return;
        }

        var foe1 = resultFromRequest.Foes.ElementAt(0);

        if (foe1 is not null)
        {
            if (foe1.CpuFlag == 0)
            {
                pvpBattleResult.Foe1Indicator = PlayerIndicator.Player;
                pvpBattleResult.Foe1PilotId = foe1.PilotId;
                pvpBattleResult.Foe1MsId = foe1.MstMobileSuitId;
                pvpBattleResult.Foe1EchelonId = foe1.EchelonId;
                pvpBattleResult.Foe1BurstType = foe1.BurstType;
            }
            else
            {
                pvpBattleResult.Foe1Indicator = PlayerIndicator.Cpu;
            }
        }
        
        if (resultFromRequest.Foes.Count == 1)
        {
            pvpBattleResult.Foe2Indicator = PlayerIndicator.Discarded;
            return;
        }

        var foe2 = resultFromRequest.Foes.ElementAt(1);

        if (foe2 is not null)
        {
            if (foe2.CpuFlag == 0)
            {
                pvpBattleResult.Foe2Indicator = PlayerIndicator.Player;
                pvpBattleResult.Foe2PilotId = foe2.PilotId;
                pvpBattleResult.Foe2MsId = foe2.MstMobileSuitId;
                pvpBattleResult.Foe2EchelonId = foe2.EchelonId;
                pvpBattleResult.Foe2BurstType = foe2.BurstType;
            }
            else
            {
                pvpBattleResult.Foe2Indicator = PlayerIndicator.Cpu;
            }
        }
    }

    void UpdateWinLossCount(SaveVsmResultCommand request, Request.SaveVsmResult.PlayResultGroup resultFromRequest,
        Response.PreLoadCard.LoadPlayer loadPlayer)
    {
        if (resultFromRequest.WinFlag)
        {
            loadPlayer.TotalWin++;
            if (request.Request.save_vsm_result.ShuffleFlag)
            {
                loadPlayer.ShuffleWin++;
            }
            else
            {
                loadPlayer.TeamWin++;
            }
        }
        else
        {
            loadPlayer.TotalLose++;
            if (request.Request.save_vsm_result.ShuffleFlag)
            {
                loadPlayer.ShuffleLose++;
            }
            else
            {
                loadPlayer.TeamLose++;
            }
        }
    }

    void UpdateNaviFamiliarity(Request.SaveVsmResult.PlayResultGroup resultFromRequest, Response.PreLoadCard.MobileUserGroup mobileUserGroup)
    {
        var uiNaviId = resultFromRequest.GuestNavId;
        var uiNavi = mobileUserGroup.GuestNavs
            .FirstOrDefault(guestNavi => guestNavi.GuestNavId == uiNaviId);

        if (uiNavi != null)
        {
            uiNavi.GuestNavFamiliarity += 1;
        }

        var battleNaviId = resultFromRequest.BattleNavId;
        var battleNavi = mobileUserGroup.GuestNavs
            .FirstOrDefault(guestNavi => guestNavi.GuestNavId == battleNaviId);

        if (battleNavi != null)
        {
            battleNavi.GuestNavFamiliarity += 1;
        }
    }
    
    void UpsertMsUsedNum(Request.SaveVsmResult.PlayResultGroup resultFromRequest, Response.LoadCard.PilotDataGroup pilotDataGroup,
        List<Response.PreLoadCard.MobileUserGroup.FavoriteMSGroup> favouriteMsList)
    {
        var msId = resultFromRequest.SkillPointMobileSuitId;

        var msSkillGroup = pilotDataGroup.MsSkills
            .FirstOrDefault(msSkillGroup => msSkillGroup.MstMobileSuitId == msId);
        uint msSkillGroupUsedNum = 1;

        if (msSkillGroup == null)
        {
            pilotDataGroup.MsSkills.Add(new Response.LoadCard.PilotDataGroup.MSSkillGroup
            {
                MstMobileSuitId = msId,
                MsUsedNum = msSkillGroupUsedNum,
                CostumeId = 0,
                TriadBuddyPoint = 0
            });
        }
        else
        {
            msSkillGroup.MsUsedNum++;
            msSkillGroupUsedNum = msSkillGroup.MsUsedNum;
        }

        favouriteMsList
            .FindAll(favouriteMs => favouriteMs.MstMobileSuitId == msId)
            .ForEach(favouriteMs => favouriteMs.MsUsedNum = msSkillGroupUsedNum);
    }
    
    private void UpdateTagTeam(Request.SaveVsmResult.PlayResultGroup resultFromRequest, uint pilotId)
    {
        if (resultFromRequest.TagTeamId == 0)
        {
            return;
        }

        var tagTeam = _context.TagTeamData.FirstOrDefault(dbTagTeam => dbTagTeam.CardId == pilotId && dbTagTeam.Id == resultFromRequest.TagTeamId);

        if (tagTeam is null)
        {
            return;
        }
        
        _logger.LogInformation("Team ID = {teamId}, Skill Point Increment = {increment}", 
            resultFromRequest.TagTeamId,
            resultFromRequest.TagSkillPoint  
        );
        
        tagTeam.SkillPoint += resultFromRequest.TagSkillPoint;
    }
}
