using System;

namespace AdobeSignApi.Models
{
    public class TokenInfo
    {
        public string Token { get; set; }
        public string DistributorID { get; set; }
        public string OrgID { get; set; }
        public string UserID { get; set; }
        public string UserType { get; set; }
        public DateTime Endtamp { get; set; }
    }
}