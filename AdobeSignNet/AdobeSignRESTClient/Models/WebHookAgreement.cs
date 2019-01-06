using System.Collections.Generic;

namespace AdobeSignRESTClient.Models
{
    public class WebHookAgreement
    {
        public string id { get; set; }
        public string name { get; set; }
        public string signatureType { get; set; }
        public string status { get; set; }
        public List<Cc> ccs { get; set; }
        public DeviceInfo deviceInfo { get; set; }
        public string documentVisibilityEnabled { get; set; }
        public string createdDate { get; set; }
        public string expirationTime { get; set; }
        public ExternalId externalId { get; set; }
        public PostSignOption postSignOption { get; set; }
        public string firstReminderDelay { get; set; }
        public string locale { get; set; }
        public string message { get; set; }
        public string reminderFrequency { get; set; }
        public string senderEmail { get; set; }
        public VaultingInfo vaultingInfo { get; set; }
        public string workflowId { get; set; }
        public WebHookParticipantSetsInfo participantSetsInfo { get; set; }
        public WebHookDocumentsInfo documentsInfo { get; set; }
    }
}
