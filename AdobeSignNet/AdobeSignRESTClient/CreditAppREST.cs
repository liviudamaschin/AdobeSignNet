using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;

namespace AdobeSignRESTClient
{
    public class CreditAppREST : ICreditAppREST
    {
        private readonly RestClient _restClient;

        public CreditAppREST(string apiUrl)
        {
            _restClient = new RestClient(apiUrl);
        }

        public void UpdateCreditApplicationStatus()
        {
            //var request = new RestRequest($"{_apiEndpointVer}/agreements/{agreementId}", Method.GET);
            //request.AddHeader("Content-Type", "application/json");
            //request.AddHeader("Authorization", $"Bearer {AccessToken}");
            //var result = _restClient.Execute(request);
            //var returnVal = result.Headers.FirstOrDefault(x => x.Name == "ETag")?.Value.ToString();

            //return returnVal;
        }
    }
}
