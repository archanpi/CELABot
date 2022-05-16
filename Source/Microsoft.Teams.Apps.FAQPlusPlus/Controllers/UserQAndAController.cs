using Microsoft.AspNetCore.Mvc;
using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
using Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Teams.Apps.FAQPlusPlus.Controllers
{
    [Route("/userqanda")]
    [ApiController]
    public class UserQAndAController : Controller
    {
        private readonly ICelaBotMemoryCache celBotMemoryCache;
        public UserQAndAController(ICelaBotMemoryCache celBotMemoryCache)
        {
            this.celBotMemoryCache = celBotMemoryCache;
        }

        public async Task<ActionResult> Index()
        {
            var userQuestions = celBotMemoryCache.GetCache<List<QandAResponse>>("CelBot");
            
            return this.View(userQuestions);
        }
    }
}
