namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using GraphQL;
    using GraphQL.Client.Http;
    using GraphQL.Client.Serializer.Newtonsoft;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;

    /// <summary>
    /// Client for graphQl api...
    /// </summary>
    public class BotGraphQlClient
    {
        /// <summary>
        /// Save message...
        /// </summary>
        /// <param name="userId">userId...</param>
        /// <param name="userName">userName...</param>
        /// <param name="type">type...</param>
        /// <param name="text">text...</param>
        /// <param name="replyToId">replyToId...</param>
        /// <param name="responseText">reponsetext...</param>
        /// <returns>boolean result...</returns>
        public async Task<bool> SaveMessage(string userId, string userName, string type, string text, string replyToId, string responseText)
        {
            var saveRequest = new GraphQLRequest
            {
                Query = @"mutation($message: MessageInputDto!, $user: UserInputDto!) {	saveMessage	(message: $message, user:$user)}",
                Variables = new
                {
                    message = new
                    {
                        id = Guid.NewGuid().ToString(),
                        userId,
                        replyToId,
                        responseText,
                        text,
                        type
                    },
                    user = new
                    {
                        id = userId,
                        name = userName
                    }
                }
            };

            using (var graphQLClient = new GraphQLHttpClient("https://botgraphqlapi.azurewebsites.net/graphql", new NewtonsoftJsonSerializer()))
            {
                try
                {
                    var mutationResponse = await graphQLClient.SendMutationAsync<MutationResponse>(saveRequest);
                    return mutationResponse.Data.SaveMessage;
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Get users...
        /// </summary>
        /// <returns>User List...</returns>
        public async Task<IList<User>> GetUsers()
        {
            var usersRequest = new GraphQLRequest
            {
                Query = @"
                    {
                        users{
                            id,
                            name,
                            messages{
                                replyToId,
                                text,
                                type
                            }
                        }
                    }"
            };
            using (var graphQLClient = new GraphQLHttpClient("https://botgraphqlapi.azurewebsites.net/graphql", new NewtonsoftJsonSerializer()))
            {
                try
                {
                    var graphQLResponse = await graphQLClient.SendQueryAsync<GraphQlUserResponse>(usersRequest);

                    return graphQLResponse.Data.Users;
                }
                catch
                {
                    return new List<User>();
                }
            }
        }
    }
}
