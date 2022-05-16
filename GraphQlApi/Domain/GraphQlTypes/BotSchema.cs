using GraphQL.Types;
using Microsoft.EntityFrameworkCore;

namespace Domain.GraphQlTypes
{
    public class BotSchema : Schema
    {
        public BotSchema(IServiceProvider provider, IDbContextFactory<CelaHackContext> factory, BotQuery query, BotMutation mutation)
            : base(provider)
        {
            Query = query;
            Mutation = mutation;

            using var context = factory.CreateDbContext();

            context.Database.EnsureCreated();
        }
    }
}
