using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers;

namespace Microsoft.Teams.Apps.FAQPlusPlus.Controllers
{
    [Route("/messagelog")]
    [ApiController]
    public class UserMessageLogController : Controller
    {
        private readonly BotGraphQlClient graphQlClient;

        public UserMessageLogController(BotGraphQlClient graphQlClient)
        {
            this.graphQlClient = graphQlClient;
        }

        public async Task<ActionResult> Index()
        {
            var userMessages = await graphQlClient.GetUsers();

            return this.View(userMessages);
        }
    }
}
