using System.ComponentModel.DataAnnotations;

namespace Microsoft.SCIM.WebHostSample.Models
{
    public class Tour
    {
        public int Id { get; set;  }
        public string? Title { get; set; }

        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; }
    }
}
