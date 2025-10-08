using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.SCIM.WebHostSample.Models;
using Microsoft.SCIM.WebHostSample.Provider;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.SCIM.WebHostSample.Controllers
{
    [Route("tour")]
    [Authorize]
    [ApiController]
    public class TourController : ControllerBase
    {
        private static string GroupName = "Tour Guides";

        private string ParseTokenForClaim(string accessToken,string claimType)
        {
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(accessToken) as JwtSecurityToken;
            if (jsonToken == null || jsonToken.Claims == null) return null;
            
            foreach (Claim c in jsonToken.Claims)
            {
                if (c.Type.Equals(claimType))
                {
                    return c.Value;
                }
            }
            return null;
        }

        private string ParseTokenForDemoClaim(string accessToken)
        {
            return ParseTokenForClaim(accessToken, "demoid");
        }

        [HttpGet]
        public async Task<ActionResult> Get()
        {

            var accessToken = await HttpContext.GetTokenAsync("access_token");
            var demoid = ParseTokenForDemoClaim(accessToken);
            if (demoid == null)
            {
                return this.Unauthorized("no agent ID provided");
            }
            AgenticIdentity ai = InMemoryProvider.Singleton.InMemoryAgegenticIdentityProvider.GetById(demoid);
            if (ai == null)
            {
                return this.Unauthorized("agent ID not found");
            }
            Core2Group g = InMemoryProvider.Singleton.InMemoryGroupProvider.GetByName(GroupName);
            if (g == null)
            {
                return this.Unauthorized("group " + GroupName + " not found");
            }
            if (g.ContainsMemberById(ai.Identifier)) {
                // ok
            } else
            {
                return this.Unauthorized("agent " + demoid + " not authorized as not member of group " + g.DisplayName + " id " + g.Identifier);
            } 

            Tour t1 = new Tour();
            t1.Id = 1;
            t1.Title = "Regular";
            //t1.Description = demoid != null ? demoid : "";
            t1.Currency = "USD";
            t1.Price = 10;

            Tour t2 = new Tour();
            t2.Id = 2;
            t2.Title = "VIP";
            t2.Currency = "USD";
            t2.Price = 20;

            List<Tour> tours = new List<Tour>();
            tours.Add(t1);
            tours.Add(t2);
            
            return this.Ok(tours);
        }

        /*

        // GET: TourController
        public ActionResult Index()
        {
            return View();
        }

        // GET: TourController/Details/5
        public ActionResult Details(int id)
        {
            Tour mv = new Tour();
            mv.Id = id; // XXX
            return View(mv);
        }

        // GET: TourController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: TourController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: TourController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: TourController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: TourController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: TourController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
        */
    }
}
