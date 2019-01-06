using System.Collections.Generic;

namespace AdobeSignRESTClient.Models
{
    public class FormConditionalAction
    {
        public string action { get; set; }
        public string anyOrAll { get; set; }
        public List<FormPredicate> predicates { get; set; }
    }
}
