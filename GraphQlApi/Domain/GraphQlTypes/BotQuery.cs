using Domain.Models;
using GraphQL;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore;

namespace Domain.GraphQlTypes
{
    public class BotQuery : ObjectGraphType
    {
        const string userIdParamName = "userId";

        public BotQuery(IDbContextFactory<CelaHackContext> factory)
        {
            FieldAsync<NonNullGraphType<ListGraphType<NonNullGraphType<UserType>>>>(
                "Users",
                resolve: async ctx =>
                {
                    using var context = await factory.CreateDbContextAsync();

                    return await context.Users.ToListAsync();
                });

            FieldAsync<NonNullGraphType<ListGraphType<NonNullGraphType<UserType>>>>(
                "User",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = userIdParamName }),
                resolve: async ctx => {
                    var id = ctx.GetArgument<string>(userIdParamName);
                    using var context = await factory.CreateDbContextAsync();

                    return await context.Users.FindAsync(id);
                });

            FieldAsync<NonNullGraphType<ListGraphType<NonNullGraphType<GraphQLClrOutputTypeReference<Message>>>>>(
                "Messages",
                resolve: async ctx => {
                    using var context = await factory.CreateDbContextAsync();

                    return await context.Messages.ToListAsync();
                }
                );
        }
    }
}
