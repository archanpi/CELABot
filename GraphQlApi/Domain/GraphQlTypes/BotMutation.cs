using Domain.GraphQlTypes.InputDtos;
using Domain.Models;
using GraphQL;
using GraphQL.Types;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.GraphQlTypes
{
    public class BotMutation : ObjectGraphType
    {
        const string messageParamName = "message";
        const string userParamName = "user";

        public BotMutation(IDbContextFactory<CelaHackContext> factory)
        {
            FieldAsync<NonNullGraphType<BooleanGraphType>>(
                "saveMessage",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<GraphQLClrInputTypeReference<MessageInputDto>>> { Name = messageParamName },
                    new QueryArgument<NonNullGraphType<GraphQLClrInputTypeReference<UserInputDto>>> { Name = userParamName }),
                resolve: async ctx =>
                {
                    using var context = await factory.CreateDbContextAsync();
                    var user = ctx.GetArgument<UserInputDto>(userParamName);
                    var tempUser = await context.Users.FindAsync(user.Id);
                    if (tempUser == null)
                    {
                        await context.Users.AddAsync(new Models.User { Id = user.Id, Name = user.Name });
                    }

                    var message = ctx.GetArgument<MessageInputDto>(messageParamName);
                    await context.Messages.AddAsync(new Message { 
                        Id = message.Id,
                        UserId = message.UserId, 
                        ReplyToId = message.ReplyToId, 
                        Type = message.Type, 
                        Text = message.Text, 
                        ResponseText = message.ResponseText 
                    });

                    var count = await context.SaveChangesAsync();
                    return count > 0;
                });
        }
    }
}
