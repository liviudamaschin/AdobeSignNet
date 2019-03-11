
namespace AdobeSignRESTClient.Models
{
    public class RefreshTokenRequest
    {
        public string refresh_token { get; set; }
        public string client_id { get; set; }
        public string grant_type { get; set; }
        public string client_secret { get; set; }
    }
}
