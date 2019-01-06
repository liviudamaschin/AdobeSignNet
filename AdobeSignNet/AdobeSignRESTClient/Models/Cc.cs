using System.Collections.Generic;

namespace AdobeSignRESTClient.Models
{
    public class Cc
    {
        public string email { get; set; }
        public string label { get; set; }
        public List<string> visiblePages { get; set; }
    }
}
