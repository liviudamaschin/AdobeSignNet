using System.Collections.Generic;

namespace AdobeSignRESTClient.Models
{
    public class AgreementMinimalRequest
    {
        /// <summary>
        /// A list of one or more files (or references to files) that will be sent out for signature. 
        /// If more than one file is provided, they will be combined into one PDF before being sent out. 
        /// Note: Only one of the four parameters in every FileInfo object must be specified
        /// </summary>
        public List<FileInfo> fileInfos { get; set; }

        /// <summary>
        /// The name of the agreement that will be used to identify it, in emails and on the website
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// A list of one or more recipient sets. A recipient set may have one or more recipients. 
        /// If any member of the recipient set signs, the agreement is considered signed by the recipient set.
        /// Note: If signatureFlow is set to SENDER_SIGNS_ONLY, this parameter is optional
        /// </summary>
        public List<ParticipantInfo> participantSetsInfo { get; set; }

        /// <summary>
        /// ['ESIGN' or 'WRITTEN']: Specifies the type of signature you would like to request - written or e-signature
        /// </summary>
        public string signatureType { get; set; }

        public string state { get; set; }

    }
}
