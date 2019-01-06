using System.Collections.Generic;

namespace AdobeSignRESTClient.Models
{
    public class WebHookParticipantSetsInfo
    {
        public List<WebHookParticipantSet> participantSets { get; set; }
        public List<WebHookParticipantSet> nextParticipantSets { get; set; }
    }
}
