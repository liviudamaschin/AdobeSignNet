using AdobeSignRESTClient.Models;
using System;

namespace AdobeSignRESTClient.ErrorHandling
{
    public class AdobeSignOAuthException: Exception
    {
        public AdobeSignError Error { get; set; }
        public AdobeSignOAuthException(AdobeSignError error)
        {
            this.Error = error;
        }
    }
}
