using System.Net.Http;
using System.Threading.Tasks;
using AdobeSignRESTClient.Models;

namespace AdobeSignRESTClient
{
    interface IAdobeSignREST
    {
        //string AccessToken { get; set; }
        //int AccessTokenExpires { get; }
        //string ApiEndpointVer { get; set; }
        //string RefreshToken { get; }

        /// <summary>
        /// Refresh existing Access Token with the Refresh Token.
        /// </summary>
        /// <param name="refreshToken">Refresh Token used to get a new Access Token.</param>
        /// <returns></returns>
        HttpResponseMessage Authorize(string refreshToken);

        /// <summary>
        /// Obtain Access Token for the AdobeSign REST API (use only if you don't already have a Refresh Token, or if it is expired)
        /// </summary>
        /// <param name="authCode">Authorization code received from the authorization request</param>
        /// <param name="redirectUri">Redirect URI matching the one specified in the authorization request</param>
        /// <returns></returns>
        HttpResponseMessage Authorize(string authCode, string redirect_uri);

        Task<SigningUrlResponse> GetAgreementSigningUrl(int? creditDataId, string agreementId);

        Task<TransientDocument> UploadTransientDocument(int? creditDataId, string fileName, byte[] file, string mimeType = null);
        
        /// <summary>
        /// Creates an agreement. Sends it out for signatures, and returns the agreementID in the response to the client
        /// </summary>
        /// <param name="newAgreement">Information about the agreement</param>
        /// <returns>AgreementCreationResponse</returns>
        Task<AgreementCreationResponse> CreateAgreement(int? creditDataId, AgreementMinimalRequest newAgreement);

        Task<AgreementSigningPositionResponse> AgreementSigningPosition(int? creditDataId, string agreementId, FormFieldPutInfo formField);

        DocumentUrl GetAgreementDocumentUrl(int? creditDataId, string agreementId);

        bool GetAgreementDocuments(int? creditDataId, string agreementId);

        AgreementResponse GetAgreement(int? creditDataId, string agreementId);

        void UpdateAgreementStatus(int? creditDataId, string agreementId, string agreementStatus);

        //Task<AlternateParticipantResponse> AddParticipant(string agreementId, string participantSetId, string participantId, AlternateParticipantInfo participantInfo);
        //Task<AgreementStatusUpdateResponse> CancelAgreement(string agreementId, string comment, bool notifySigner);
        //Task<WidgetCreationResponse> CreateWidget(WidgetMinimalRequest newWidget);
        //Task DeleteAgreement(string agreementId);
        //Task<AgreementInfo> GetAgreement(string agreementId);
        //Task<UserAgreements> GetAgreements();
        //Task<Stream> GetAgreementDocument(string agreementId, string documentId);
        //Task<AgreementDocuments> GetAgreementDocuments(string agreementId);
        //Task<WidgetPersonalizedResponse> PersonalizedWidget(string widgetId, string email);
        //Task Revoke(string token);
        //Task<WidgetStatusUpdateResponse> UpdateWidgetStatus(string widgetId, WidgetStatusUpdateInfo info);
        //Task<ReminderCreationResult> SendReminders(ReminderCreationInfo info);

        void Dispose();
    }
}
