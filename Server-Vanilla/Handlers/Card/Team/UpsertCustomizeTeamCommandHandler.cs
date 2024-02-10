﻿using MediatR;
using Microsoft.EntityFrameworkCore;
using ServerVanilla.Models.Cards.Team;
using ServerVanilla.Persistence;
using WebUIVanilla.Shared.Dto.Request;
using WebUIVanilla.Shared.Dto.Response;

namespace ServerVanilla.Handlers.Card.Team;

public record UpsertCustomizeTeamCommand(UpsertTeamsRequest Request) : IRequest<BasicResponse>;

public class UpsertCustomizeTeamCommandHandler : IRequestHandler<UpsertCustomizeTeamCommand, BasicResponse>
{
    private readonly ServerDbContext _context;
    
    public UpsertCustomizeTeamCommandHandler(ServerDbContext context)
    {
        _context = context;
    }

    public Task<BasicResponse> Handle(UpsertCustomizeTeamCommand request, CancellationToken cancellationToken)
    {
        var updateRequest = request.Request;
        
        var cardProfile = _context.CardProfiles
            .Include(x => x.OnlinePairs)
            .FirstOrDefault(x => x.AccessCode == updateRequest.AccessCode && x.ChipId == updateRequest.ChipId);
        
        if (cardProfile == null)
        {
            throw new NullReferenceException("Card Profile is invalid");
        }
        
        if (updateRequest.Teams.Count == 0)
        {
            return Task.FromResult(new BasicResponse
            {
                Success = true
            });
        }

        cardProfile.OnlinePairs.Clear();
        _context.SaveChanges();

        var requestedTeams = updateRequest.Teams;
        var updatedTeamIds = updateRequest.Teams
            .Where(team => team.Id != 0)
            .Select(team => team.Id)
            .ToList();
            
        var currentTeams = _context.TagTeamData
            .Where(tagTeam => tagTeam.CardId == cardProfile.Id || tagTeam.TeammateCardId == cardProfile.Id)
            .ToList();
        
        // Delete Logic
        var removedTeams = currentTeams
            .Where(team => !updatedTeamIds.Contains((uint)team.Id))
            .ToList();
        currentTeams.RemoveAll(team => !updatedTeamIds.Contains((uint) team.Id));
        _context.TagTeamData.RemoveRange(removedTeams);
        _context.SaveChanges();
        
        // Update Logic
        requestedTeams.ForEach(team =>
        {
            if (team.Id == 0)
            {
                return;
            }

            var updateTeam = currentTeams.FirstOrDefault(innerTeam => innerTeam.Id == team.Id);

            if (updateTeam is null)
            {
                return;
            }

            updateTeam.TeamName = team.Name;
            updateTeam.BackgroundPartsId = team.BackgroundPartsId;
            updateTeam.EmblemId = team.EmblemId;
            updateTeam.EffectId = team.EffectId;
            updateTeam.NameColorId = team.NameColorId;
            updateTeam.BgmId = team.BgmId;
            
            if (team.OnlineTag)
            {
                cardProfile.OnlinePairs.Add(new OnlinePair()
                {
                    TeamId = (int) team.Id
                });
            }
        });
        
        _context.SaveChanges();
        
        return Task.FromResult(new BasicResponse
        {
            Success = true
        });
    }
}