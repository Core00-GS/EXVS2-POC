﻿using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using nue.protocol.exvs;
using Server.Mappers;
using Server.Persistence;
using WebUI.Shared.Dto.Common;
using WebUI.Shared.Dto.Enum;
using WebUI.Shared.Dto.Request;
using WebUI.Shared.Dto.Response;
using WebUI.Shared.Exception;

namespace Server.Handlers.Card.Message;

public record UpsertCustomMessagesCommand(UpsertCustomMessagesRequest Request) : IRequest<BasicResponse>;

public class UpsertCustomMessagesCommandHandler : IRequestHandler<UpsertCustomMessagesCommand, BasicResponse>
{
    private readonly ServerDbContext context;
    
    public UpsertCustomMessagesCommandHandler(ServerDbContext context)
    {
        this.context = context;
    }

    public Task<BasicResponse> Handle(UpsertCustomMessagesCommand request, CancellationToken cancellationToken)
    {
        var updateRequest = request.Request;
        
        var cardProfile = context.CardProfiles
            .Include(x => x.UserDomain)
            .FirstOrDefault(x => x.AccessCode == updateRequest.AccessCode && x.ChipId == updateRequest.ChipId);

        if (cardProfile is null)
        {
            throw new NullReferenceException("Card Profile is invalid");
        }
        
        var mobileUserGroup = JsonConvert.DeserializeObject<Response.LoadCard.MobileUserGroup>(cardProfile.UserDomain.MobileUserGroupJson);
        
        if (mobileUserGroup is null)
        {
            throw new InvalidCardDataException("Card Data is invalid");
        }

        var messageSetting = updateRequest.MessageSetting;
        
        UpsertCommandMessageGroup(messageSetting.StartGroup, mobileUserGroup.OpeningMessages);
        UpsertCommandMessageGroup(messageSetting.InBattleGroup, mobileUserGroup.PlayingMessages);
        UpsertCommandMessageGroup(messageSetting.ResultGroup, mobileUserGroup.ResultMessages);

        cardProfile.UserDomain.MobileUserGroupJson = JsonConvert.SerializeObject(mobileUserGroup);
        
        context.SaveChanges();

        return Task.FromResult(new BasicResponse
        {
            Success = true
        });
    }

    void UpsertCommandMessageGroup(CustomMessageGroup? customMessageGroup, List<Response.LoadCard.MobileUserGroup.CommandMessageGroup> commandMessageGroups)
    {
        if (customMessageGroup is null)
        {
            return;
        }

        List<Response.LoadCard.MobileUserGroup.CommandMessageGroup> newCommandMessageGroup = new ();

        if (customMessageGroup.UpMessage is not null)
        {
            newCommandMessageGroup.Add(ToWithDirection(customMessageGroup.UpMessage, Command.Up));
        } 
        
        if (customMessageGroup.DownMessage is not null)
        {
            newCommandMessageGroup.Add(ToWithDirection(customMessageGroup.DownMessage, Command.Down));
        } 
        
        if (customMessageGroup.LeftMessage is not null)
        {
            newCommandMessageGroup.Add(ToWithDirection(customMessageGroup.LeftMessage, Command.Left));
        } 
        
        if (customMessageGroup.RightMessage is not null)
        {
            newCommandMessageGroup.Add(ToWithDirection(customMessageGroup.RightMessage, Command.Right));
        }

        if (newCommandMessageGroup.Count == 0)
        {
            return;
        }
        
        commandMessageGroups.Clear();
        commandMessageGroups.AddRange(newCommandMessageGroup);
    }

    Response.LoadCard.MobileUserGroup.CommandMessageGroup ToWithDirection(CustomMessage customMessage, Command direction)
    {
        var commandMessageGroup = customMessage.ToCommandMessageGroup();
        commandMessageGroup.Command = (uint) direction;
        return commandMessageGroup;
    }
}