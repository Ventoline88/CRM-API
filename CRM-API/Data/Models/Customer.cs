using Newtonsoft.Json;

namespace CRM_API.Data.Models
{
    public class Customer
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        public string Name { get; set; }
        public string Title { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }

        public Salesperson ResponsibleSalesPerson { get; set; }
    }
}
