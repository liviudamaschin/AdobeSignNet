using System.Collections.Generic;

namespace AdobeSignRESTClient.Models
{
    public class WebHookDocumentsInfo
    {
        public List<WebHookDocument> documents { get; set; }
        public List<WebHookDocument> supportingDocuments { get; set; }
    }
}
