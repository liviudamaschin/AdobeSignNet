using AdobeSignRESTClient.Models;
using System;

namespace AdobeSignRESTClient.ErrorHandling
{
    class AdobeSignBadRequestException : Exception
    {
        public AdobeSignErrorCode Error { get; set; }

        public AdobeSignBadRequestException(AdobeSignErrorCode error)
        {
            this.Error = error;
        }
    }
}
