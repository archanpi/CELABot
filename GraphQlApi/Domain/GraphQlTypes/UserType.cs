using Domain.Models;
using GraphQL.DataLoader;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore;

namespace Domain.GraphQlTypes
{
    public class UserType: AutoRegisteringObjectGraphType<User>
    {
        public UserType(IDbContextFactory<CelaHackContext> factory, IDataLoaderContextAccessor accessor)
        {
            Field<ListGraphType<NonNullGraphType<GraphQLClrOutputTypeReference<Message>>>>(
                "Messages", 
                resolve: ctx => accessor.Context.GetOrAddCollectionBatchLoader<string, Message>(
                    ctx.FieldAst.Location.ToString(),
                    async (items) =>
                    {
                        using var context = await factory.CreateDbContextAsync();

                        var result = (await context.Messages
                                .Where(x => items.Contains(x.UserId))
                                .ToListAsync())
                                .ToLookup(x => x.UserId);
                        return result;
                    }).LoadAsync(ctx.Source.Id));
        }

        
    }
}
