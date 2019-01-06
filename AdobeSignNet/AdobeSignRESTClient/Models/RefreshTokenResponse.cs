using System;
using System.Collections.Generic;
using System.Text;

namespace AdobeSignRESTClient.Models
{
    public class RefreshTokenResponse
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
    }
}
