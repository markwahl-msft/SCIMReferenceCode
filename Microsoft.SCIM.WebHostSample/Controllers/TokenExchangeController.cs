using Microsoft.AspNetCore.Mvc;
using Microsoft.SCIM.WebHostSample.Models;

namespace Microsoft.SCIM.WebHostSample.Controllers
{
    [Route("token_exchange")]
    [ApiController]
    public class TokenExchangeController : ControllerBase
    {
        [HttpPost]
        public IActionResult Index()
        {
            TokenExchangeResponse response = new TokenExchangeResponse();

            return this.Ok(response);
        }
    }
}
