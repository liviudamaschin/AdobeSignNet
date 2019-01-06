using System.Collections.Generic;
using Newtonsoft.Json;

namespace AdobeSignRESTClient.Models
{
    public class WebHookInfo
    {
        public string webHookId { get; set; }
        public string webHookName { get; set; }
        public string webHookNotificationId { get; set; }
        public List<WebHookNotificationApplicableUser> webHookNotificationApplicableUsers { get; set; }
        public WebHookUrlInfo webHookUrlInfo { get; set; }
        public string webHookScope { get; set; }
        [JsonProperty(PropertyName = "event")]
        public string Event { get; set; }
        public string eventDate { get; set; }
        public string eventResourceParentType { get; set; }
        public string eventResourceParentId { get; set; }
        public string subEvent { get; set; }
        public string eventResourceType { get; set; }
        public string participantRole { get; set; }
        public string sectionType { get; set; }
        public string participantUserId { get; set; }
        public string participantUserEmail { get; set; }
        public string actingUserId { get; set; }
        public string actingUserEmail { get; set; }
        public string initiatingUserId { get; set; }
        public string initiatingUserEmail { get; set; }
        public string actingUserIpAddress { get; set; }
        public WebHookAgreement agreement { get; set; }
    }
}
