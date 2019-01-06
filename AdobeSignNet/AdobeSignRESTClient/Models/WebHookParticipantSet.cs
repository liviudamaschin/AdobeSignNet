using System.Collections.Generic;

namespace AdobeSignRESTClient.Models
{
    public class WebHookParticipantSet
    {
        public List<WebHookMemberInfo> memberInfos { get; set; }
        public string order { get; set; }
        public string role { get; set; }
        public string status { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string privateMessage { get; set; }
    }
}
