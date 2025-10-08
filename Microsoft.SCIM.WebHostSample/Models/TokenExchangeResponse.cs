using System.ComponentModel.DataAnnotations;

namespace Microsoft.SCIM.WebHostSample.Models
{
    public class TokenExchangeResponse
    {
        public string access_token { get; set; }
        public string issued_token_type { get; set;  }
        public string token_type { get; set;  }
    }
}
