using System.Collections.Generic;

namespace AdobeSignRESTClient.Models
{
    public class SigningUrlSetInfo
    {
        public List<SigningUrl> signingUrls { get; set; }
        public string signingUrlSetName { get; set; }
    }
}
