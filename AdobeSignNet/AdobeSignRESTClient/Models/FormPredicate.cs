using Newtonsoft.Json;

namespace AdobeSignRESTClient.Models
{
    public class FormPredicate
    {
        public string fieldLocationIndex { get; set; }
        public string fieldName { get; set; }
        [JsonProperty(PropertyName = "operator")]
        public string Operator { get; set; }
        public string value { get; set; }
    }
}
