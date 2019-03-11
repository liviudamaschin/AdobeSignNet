using System;

namespace AdobeSignApi.Models
{
    public class AppStatusResponse
    {
        public int DistributorID { get; set; }
        public int RetailerID { get; set; }
        public int UserID { get; set; }
        public DateTime? LastUpdate { get; set; }
        public string Status { get; set; }
        public string Comments { get; set; }
    }
}