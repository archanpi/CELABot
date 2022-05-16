using Domain.Models;
using GraphQL.DataLoader;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore;

namespace Domain.GraphQlTypes
{
    public class MessageType : AutoRegisteringObjectGraphType<Message>
    {
        public MessageType(IDbContextFactory<CelaHackContext> factory, IDataLoaderContextAccessor accessor)
        {
            Field<NonNullGraphType<UserType>>(
                "User",
                resolve: ctx => accessor.Context.GetOrAddBatchLoader<string, User>(
                    ctx.FieldAst.Location.ToString(),
                    async (items) =>
                    {
                        using var context = await factory.CreateDbContextAsync();

                        var itemList = items.Distinct().ToList();
                        var result = (await context.Users
                                .Where(x => itemList.Contains(x.Id))
                                .ToListAsync())
                                .ToDictionary(x => x.Id);
                        return result;
                    }).LoadAsync(ctx.Source.UserId));
        }
    }
}
