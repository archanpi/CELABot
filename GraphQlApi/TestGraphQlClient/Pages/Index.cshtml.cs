using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TestGraphQlClient.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        [BindProperty]
        public List<User> Users { get; set; }




        public async Task<IActionResult> OnGet()
        {
            var usersRequest = new GraphQLRequest
            {
                Query = @"
                    {
                        users{
                            id,
                            name,
                            messages{
                                id,
                                type,
                                text
                            }
                        }
                    }"
            };
            var saveRequest = new GraphQLRequest
            {
                Query = @"mutation($message: MessageInputDto!, $user: UserInputDto!) {	saveMessage	(message: $message, user:$user)}",
                Variables = new { 
                    message = new
                    {
                        id = Guid.NewGuid().ToString(),
                        userId = "@asdasdasd33",
                        replyToId = "",
                        responseText = "",
                        text = "bla bla",
                        type = "3rd type"
                    },
                    user = new
                    {
                        id = "@asdasdasd33",
                        name = "test test"
                    }
                }
            };
            var graphQLClient = new GraphQLHttpClient("https://botgraphqlapi.azurewebsites.net/graphql", new NewtonsoftJsonSerializer());
            var mutationResponse = await graphQLClient.SendMutationAsync<MutationResponse>(saveRequest);
            var graphQLResponse = await graphQLClient.SendQueryAsync<GraphQlUserResponse>(usersRequest);
            
            Users = graphQLResponse.Data.Users;

            return Page();
        }
    }
}