using Microsoft.AspNetCore.Mvc;
using Microsoft.SCIM.WebHostSample.Provider;

namespace Microsoft.SCIM.WebHostSample.Controllers
{
    public class DataDisplayController : Controller
    {
        public IActionResult Index()
        {
            string summaryu = InMemoryProvider.Singleton.UsersToHtml();
            string summaryg = InMemoryProvider.Singleton.GroupsToHtml();
            string summaryai = InMemoryProvider.Singleton.AgenticIdentitiesToHtml();
            ViewData["summaryu"] = summaryu;
            ViewData["summaryg"] = summaryg;
            ViewData["summaryai"] = summaryai;

            return View();
        }
    }
}
