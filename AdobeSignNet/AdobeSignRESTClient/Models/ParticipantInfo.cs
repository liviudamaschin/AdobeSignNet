using System.Collections.Generic;

namespace AdobeSignRESTClient.Models
{
    public class ParticipantInfo
    {
        /// <summary>
        ///  Information about the members of the recipient set
        /// </summary>
        public List<MemberInfo> memberInfos { get; set; }
        /// <summary>
        /// ['SIGNER' or 'APPROVER' or 'DELEGATE_TO_SIGNER' or 'DELEGATE_TO_APPROVER']: Specify the role of recipient set
        /// </summary>
        public string role { get; set; }
        /// <summary>
        /// Specify the name of Recipient set. Maximum no of characters in recipient set name is restricted to 255
        /// </summary>
        public string name { get; set; }

        public int? order { get; set; }
    }
}
