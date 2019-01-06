using AdobeSignRESTClient.ErrorHandling;
using AdobeSignRESTClient.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using RestSharp;

namespace AdobeSignRESTClient
{
    public class AdobeSignREST : IAdobeSignREST
    {
        private readonly HttpClient _client;
        private readonly RestClient _restClient;

        private readonly string _clientId;
        private readonly string _secretId;
        private string _apiUrl;
        private readonly string _apiEndpointVer = "api/rest/v6";

        public string AccessToken { get; set; }
        public int AccessTokenExpires { get; set; }
        public string ApiEndpointVer { get; set; }
        public string RefreshToken { get; set; }
        //private readonly CreditAppRepository repository = new CreditAppRepository();
        /// <summary>
        /// Initialize AdobeSignREST without Access Token. Must call Authorize() after initialization to acquire Access Token.
        /// </summary>
        /// <param name="apiUrl">API url returned from the authorization request URL</param>
        /// <param name="clientId">Application/Client ID</param>
        /// <param name="secretId">Client Secret</param>
        public AdobeSignREST(string apiUrl, string clientId, string secretId)
        {
            _apiUrl = apiUrl;
            _clientId = clientId;
            _secretId = secretId;

            _client = new HttpClient {BaseAddress = new Uri(apiUrl)};
            _restClient = new RestClient(apiUrl);
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        /// <inheritdoc />
        public HttpResponseMessage Authorize(string refreshToken)
        {
            using (HttpContent content = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                new KeyValuePair<string, string>("client_id", _clientId),
                new KeyValuePair<string, string>("client_secret", _secretId),
                new KeyValuePair<string, string>("refresh_token", refreshToken)
            }))
            {
                HttpResponseMessage result = _client.PostAsync("oauth/refresh", content).Result;
                if (result.IsSuccessStatusCode)
                {
                    string response = result.Content.ReadAsStringAsync().Result;
                    RefreshTokenResponse tokenObj = JsonConvert.DeserializeObject<RefreshTokenResponse>(response);

                    AccessToken = tokenObj.access_token;
                    AccessTokenExpires = tokenObj.expires_in;

                    //_client.DefaultRequestHeaders.Remove("Authorization");
                    //_client.DefaultRequestHeaders.Add("Authorization", $"Bearer {AccessToken}");
                    _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
                }
                else
                {
                    string response = result.Content.ReadAsStringAsync().Result;
                    HandleError(result.StatusCode, response, true);
                }

                return result;
            }
        }

        /// <inheritdoc />
        public HttpResponseMessage Authorize(string authCode, string redirectUri)
        {
            using (HttpContent content = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("client_id", _clientId),
                new KeyValuePair<string, string>("client_secret", _secretId),
                new KeyValuePair<string, string>("code", authCode),
                new KeyValuePair<string, string>("redirect_uri", redirectUri)
            }))
            {
                HttpResponseMessage result = _client.PostAsync("oauth/token", content).Result;
                if (result.IsSuccessStatusCode)
                {
                    string response = result.Content.ReadAsStringAsync().Result;
                    TokenResponse tokenObj = JsonConvert.DeserializeObject<TokenResponse>(response);

                    AccessToken = tokenObj.access_token;
                    AccessTokenExpires = tokenObj.expires_in;
                    RefreshToken = tokenObj.refresh_token;

                    _client.DefaultRequestHeaders.Remove("Authorization");
                    _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {AccessToken}");
                }
                else
                {
                    string response = result.Content.ReadAsStringAsync().Result;
                    HandleError(result.StatusCode, response, true);
                }

                return result;
            }
        }

        /// <inheritdoc />
        public Task<SigningUrlResponse> GetAgreementSigningUrl(int? creditDataId, string agreementId)
        {
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"{_apiEndpointVer}/agreements/{agreementId}/signingUrls"))
            {
                try
                {
                    var responseMessage = _client.SendAsync(requestMessage).Result;
                    string response = responseMessage.Content.ReadAsStringAsync().Result;
                    SigningUrlResponse signingUrlResponse = JsonConvert.DeserializeObject<SigningUrlResponse>(response);
                    return Task.FromResult(signingUrlResponse);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
                
            }
           
        }

        /// <inheritdoc />
        public Task<TransientDocument> UploadTransientDocument(int? creditDataId, string fileName, byte[] file,
            string mimeType = null)
        {
            //using (var fs = new FileStream(@"d:\a.pdf", FileMode.Create, FileAccess.Write))
            //{
            //    fs.Write(file, 0, file.Length);
            //    //return true;
            //}

            var request = new RestRequest($"{_apiEndpointVer}/transientDocuments", Method.POST);
            request.AddHeader("Authorization", $"Bearer {AccessToken}");
            request.AddHeader("Content-Type", "multipart/form-data");

            request.AddFile("File", file, fileName, "application/pdf");
            request.AddParameter("File-Name", fileName);
            //var response = _restClient.Execute(request, Method.POST);
            //TransientDocument document = JsonConvert.DeserializeObject<TransientDocument>(response);
            IRestResponse<TransientDocument> result = _restClient.Execute<TransientDocument>(request);
            //_client.DefaultRequestHeaders.Add("Authorization", $"Bearer {AccessToken}");

            //repository.AddAdobeSignLog(Convert.ToInt32(creditDataId), "transientDocuments", $"filename={filename}", transientDocument.ToJson());
            return Task.FromResult(result.Data);
        }

        /// <inheritdoc />
        public async Task<AgreementCreationResponse> CreateAgreement(int? creditDataId, AgreementMinimalRequest newAgreement)
        {
            string serializedObject = JsonConvert.SerializeObject(newAgreement);

            using (StringContent content = new StringContent(serializedObject, Encoding.UTF8))
            {
                content.Headers.Remove("Content-Type");
                content.Headers.Add("Content-Type", "application/json");

                HttpResponseMessage result = await _client.PostAsync(_apiEndpointVer + "/agreements", content).ConfigureAwait(false);
                if (result.IsSuccessStatusCode)
                {
                    string response = await result.Content.ReadAsStringAsync().ConfigureAwait(true);
                    AgreementCreationResponse agreement = JsonConvert.DeserializeObject<AgreementCreationResponse>(response);

                    return agreement;
                }
                else
                {
                    string response = await result.Content.ReadAsStringAsync().ConfigureAwait(true);
                    HandleError(result.StatusCode, response, false);

                    return null;
                }
            }
        }

        private string GetAgreementETag(int? creditDataId, string agreementId)
        {
            var request = new RestRequest($"{_apiEndpointVer}/agreements/{agreementId}", Method.GET);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", $"Bearer {AccessToken}");
            var result = _restClient.Execute(request);
            var returnVal = result.Headers.FirstOrDefault(x => x.Name == "ETag")?.Value.ToString();

            return returnVal;
        }

        public AgreementResponse GetAgreement(int? creditDataId, string agreementId)
        {

            var request = new RestRequest($"{_apiEndpointVer}/agreements/{agreementId}", Method.GET);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", $"Bearer {AccessToken}");
            var result = _restClient.Execute(request);
            AgreementResponse agreementResponse = JsonConvert.DeserializeObject<AgreementResponse>(result.Content);

            return agreementResponse;
        }

        /// <inheritdoc />
        public async Task<AgreementSigningPositionResponse> AgreementSigningPosition(int? creditDataId, string agreementId, FormFieldPutInfo formField)
        {
            var eTag = GetAgreementETag(creditDataId, agreementId);

            //string serializedObject = JsonConvert.SerializeObject(formField);

            //using (StringContent content = new StringContent(serializedObject, Encoding.UTF8))
            //{
            //    content.Headers.Remove("Content-Type");
            //    content.Headers.Add("Content-Type", "application/json");
            //    content.Headers.Remove("If-Match");
            //    content.Headers.Add("If-Match", eTag);

            //    HttpResponseMessage result = await _client.PutAsync($"{_apiEndpointVer}/agreements/{agreementId}/formFields", content).ConfigureAwait(false);
            //    if (result.IsSuccessStatusCode)
            //    {
            //        string response = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
            //        AgreementSigningPositionResponse agreementSigningPosition = JsonConvert.DeserializeObject<AgreementSigningPositionResponse>(response);

            //        return agreementSigningPosition;
            //    }
            //    else
            //    {
            //        string response = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
            //        HandleError(result.StatusCode, response, false);

            //        return null;
            //    }
            //}

            var request = new RestRequest($"{_apiEndpointVer}/agreements/{agreementId}/formFields", Method.PUT);
            request.AddHeader("Authorization", $"Bearer {AccessToken}");
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("If-Match", eTag);

            //request.AddParameter("FormFieldPutInfo", formField, ParameterType.RequestBody);
            var jsonString = JsonConvert.SerializeObject(formField);
            request.AddBody(jsonString);
            IRestResponse<AgreementSigningPositionResponse> result = _restClient.Execute<AgreementSigningPositionResponse>(request);

            return await Task.FromResult(result.Data);
        }

        ///// <summary>
        ///// Creates a new alternate participant
        ///// </summary>
        ///// <param name="agreementId">The agreement identifier, as returned by the agreement creation API or retrieved from the API to fetch agreements</param>
        ///// <param name="participantSetId">The participant set identifier</param>
        ///// <param name="participantId">The participant identifier</param>
        ///// <param name="participantInfo">Information about the alternate participant</param>
        ///// <returns>AlternateParticipantResponse</returns>
        //public async Task<AlternateParticipantResponse> AddParticipant(string agreementId, string participantSetId, string participantId, AlternateParticipantInfo participantInfo)
        //{
        //    string serializedObject = JsonConvert.SerializeObject(participantInfo);

        //    using (StringContent content = new StringContent(serializedObject, Encoding.UTF8))
        //    {
        //        content.Headers.Remove("Content-Type");
        //        content.Headers.Add("Content-Type", "application/json");

        //        HttpResponseMessage result = await _client.PostAsync(_apiEndpointVer + "/agreements/" + agreementId + "/participantSets/" +
        //                                                participantSetId + "/participants/" + participantId + "/alternateParticipants", content)
        //                                                .ConfigureAwait(false);
        //        if (result.IsSuccessStatusCode)
        //        {
        //            string response = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
        //            AlternateParticipantResponse agreement = JsonConvert.DeserializeObject<AlternateParticipantResponse>(response);

        //            return agreement;
        //        }
        //        else
        //        {
        //            string response = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
        //            HandleError(result.StatusCode, response, false);

        //            return null;
        //        }
        //    }
        //}

        ///// <summary>
        ///// Cancels an agreement
        ///// </summary>
        ///// <param name="agreementId">The agreement identifier, as returned by the agreement creation API or retrieved from the API to fetch agreements</param>
        ///// <param name="comment">An optional comment describing to the recipient why you want to cancel the transaction</param>
        ///// <param name="notifySigner">Whether or not you would like the recipient to be notified that the transaction has been cancelled. The notification is mandatory if any party has already signed this document. The default value is false</param>
        ///// <returns>AgreementStatusUpdateResponse</returns>
        //public async Task<AgreementStatusUpdateResponse> CancelAgreement(string agreementId, string comment, bool notifySigner)
        //{
        //    AgreementStatusUpdateInfo info = new AgreementStatusUpdateInfo();
        //    info.value = "CANCEL";
        //    info.notifySigner = notifySigner;
        //    info.comment = comment;

        //    string serializedObject = JsonConvert.SerializeObject(info);

        //    using (HttpContent content = new StringContent(serializedObject))
        //    {
        //        content.Headers.Remove("Content-Type");
        //        content.Headers.Add("Content-Type", "application/json");

        //        HttpResponseMessage result = await _client.PutAsync(_apiEndpointVer + "/agreements/" + agreementId + "/status", content)
        //                                                .ConfigureAwait(false);
        //        if (result.IsSuccessStatusCode)
        //        {
        //            string response = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
        //            AgreementStatusUpdateResponse agreement = JsonConvert.DeserializeObject<AgreementStatusUpdateResponse>(response);

        //            return agreement;
        //        }
        //        else
        //        {
        //            string response = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
        //            HandleError(result.StatusCode, response, false);

        //            return null;
        //        }
        //    }
        //}

        ///// <summary>
        ///// Creates a widget and returns the Javascript snippet and URL to access the widget and widgetID in response to the client
        ///// </summary>
        ///// <param name="newWidget">Information about the widget that you want to create</param>
        ///// <returns>WidgetCreationResponse</returns>
        //public async Task<WidgetCreationResponse> CreateWidget(WidgetMinimalRequest newWidget)
        //{
        //    string serializedObject = JsonConvert.SerializeObject(newWidget);

        //    using (StringContent content = new StringContent(serializedObject, Encoding.UTF8))
        //    {
        //        content.Headers.Remove("Content-Type");
        //        content.Headers.Add("Content-Type", "application/json");

        //        HttpResponseMessage result = await _client.PostAsync(_apiEndpointVer + "/widgets", content).ConfigureAwait(false);
        //        if (result.IsSuccessStatusCode)
        //        {
        //            string response = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
        //            WidgetCreationResponse widget = JsonConvert.DeserializeObject<WidgetCreationResponse>(response);

        //            return widget;
        //        }
        //        else
        //        {
        //            string response = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
        //            HandleError(result.StatusCode, response, false);

        //            return null;
        //        }
        //    }
        //}

        ///// <summary>
        ///// Deletes an agreement. Agreement will no longer be visible in the user's Manage Page
        ///// </summary>
        ///// <param name="agreementId">The agreement identifier, as returned by the agreement creation API or retrieved from the API to fetch agreements</param>
        ///// <returns>void</returns>
        //public async Task DeleteAgreement(string agreementId)
        //{
        //    HttpResponseMessage result = await _client.DeleteAsync(_apiEndpointVer + "/agreements/" + agreementId).ConfigureAwait(false);
        //    if (!result.IsSuccessStatusCode)
        //    {
        //        string response = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
        //        HandleError(result.StatusCode, response, false);
        //    }
        //}

        ///// <summary>
        ///// Retrieves the latest status of an agreement
        ///// </summary>
        ///// <param name="agreementId">The agreement identifier, as returned by the agreement creation API or retrieved from the API to fetch agreements</param>
        ///// <returns>AgreementInfo</returns>
        //public async Task<AgreementInfo> GetAgreement(string agreementId)
        //{
        //    HttpResponseMessage result = await _client.GetAsync(_apiEndpointVer + "/agreements/" + agreementId).ConfigureAwait(false);
        //    if (result.IsSuccessStatusCode)
        //    {
        //        string response = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
        //        AgreementInfo agreement = JsonConvert.DeserializeObject<AgreementInfo>(response);

        //        return agreement;
        //    }
        //    else
        //    {
        //        string response = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
        //        HandleError(result.StatusCode, response, false);

        //        return null;
        //    }
        //}

        ///// <summary>
        ///// Retrieves agreements for the user
        ///// </summary>
        ///// <returns>UserAgreements</returns>
        //public async Task<UserAgreements> GetAgreements()
        //{
        //    HttpResponseMessage result = await _client.GetAsync(_apiEndpointVer + "/agreements").ConfigureAwait(false);
        //    if (result.IsSuccessStatusCode)
        //    {
        //        string response = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
        //        UserAgreements agreements = JsonConvert.DeserializeObject<UserAgreements>(response);

        //        return agreements;
        //    }
        //    else
        //    {
        //        string response = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
        //        HandleError(result.StatusCode, response, false);

        //        return null;
        //    }
        //}

        ///// <summary>
        ///// Retrieves the file stream of a document of an agreement
        ///// </summary>
        ///// <param name="agreementId">The agreement identifier, as returned by the agreement creation API or retrieved from the API to fetch agreements</param>
        ///// <param name="documentId">The document identifier, as retrieved from the API which fetches the documents of a specified agreement</param>
        ///// <returns>AgreementInfo</returns>
        //public async Task<Stream> GetAgreementDocument(string agreementId, string documentId)
        //{
        //    HttpResponseMessage result = await _client.GetAsync(_apiEndpointVer + "/agreements/" + agreementId + "/documents/" + documentId)
        //        .ConfigureAwait(false);
        //    if (result.IsSuccessStatusCode)
        //    {
        //        Stream response = await result.Content.ReadAsStreamAsync().ConfigureAwait(false);

        //        return response;
        //    }
        //    else
        //    {
        //        string response = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
        //        HandleError(result.StatusCode, response, false);

        //        return null;
        //    }
        //}

        ///// <summary>
        ///// Retrieves the IDs of all the main and supporting documents of an agreement identified by agreementid
        ///// </summary>
        ///// <param name="agreementId">The agreement identifier, as returned by the agreement creation API or retrieved from the API to fetch agreements</param>
        ///// <returns>AgreementInfo</returns>
        //public async Task<AgreementDocuments> GetAgreementDocuments(string agreementId)
        //{
        //    HttpResponseMessage result = await _client.GetAsync(_apiEndpointVer + "/agreements/" + agreementId + "/documents").ConfigureAwait(false);
        //    if (result.IsSuccessStatusCode)
        //    {
        //        string response = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
        //        AgreementDocuments agreement = JsonConvert.DeserializeObject<AgreementDocuments>(response);

        //        return agreement;
        //    }
        //    else
        //    {
        //        string response = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
        //        HandleError(result.StatusCode, response, false);

        //        return null;
        //    }
        //}

        ///// <summary>
        ///// Personalize the widget to a signable document for a specific known user
        ///// </summary>
        ///// <param name="widgetId">The widget identifier, as returned by the widget creation API or retrieved from the API to fetch widgets</param>
        ///// <param name="email">The email address of the person who will be receiving this widget</param>
        ///// <returns></returns>
        //public async Task<WidgetPersonalizedResponse> PersonalizedWidget(string widgetId, string email)
        //{
        //    WidgetPersonalizationInfo info = new WidgetPersonalizationInfo();
        //    info.email = email;
        //    string serializedObject = JsonConvert.SerializeObject(info);

        //    using (StringContent content = new StringContent(serializedObject, Encoding.UTF8))
        //    {
        //        content.Headers.Remove("Content-Type");
        //        content.Headers.Add("Content-Type", "application/json");

        //        HttpResponseMessage result = await _client.PutAsync(_apiEndpointVer + "/widgets/" + widgetId + "/personalize", content)
        //            .ConfigureAwait(false);
        //        if (result.IsSuccessStatusCode)
        //        {
        //            string response = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
        //            WidgetPersonalizedResponse widget = JsonConvert.DeserializeObject<WidgetPersonalizedResponse>(response);

        //            return widget;
        //        }
        //        else
        //        {
        //            string response = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
        //            HandleError(result.StatusCode, response, false);

        //            return null;
        //        }
        //    }
        //}

        ///// <summary>
        ///// Revoke Access and Refresh tokens so they cannot be used again until next authorization.
        ///// </summary>
        ///// <param name="token">Access or Refresh token.</param>
        ///// <returns></returns>
        //public async Task Revoke(string token)
        //{
        //    using (HttpContent content = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
        //    {
        //        new KeyValuePair<string, string>("token", token)
        //    }))
        //    {
        //        HttpResponseMessage result = await _client.PostAsync("oauth/revoke", content).ConfigureAwait(false);
        //        if (!result.IsSuccessStatusCode)
        //        {
        //            string response = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
        //            HandleError(result.StatusCode, response, true);
        //        }
        //    }
        //}

        ///// <summary>
        ///// Enables or Disables a widget
        ///// </summary>
        ///// <param name="widgetId">The widget identifier, as returned by the widget creation API or retrieved from the API to fetch widgets</param>
        ///// <param name="info">Widget status update information object</param>
        ///// <returns>WidgetStatusUpdateResponse</returns>
        //public async Task<WidgetStatusUpdateResponse> UpdateWidgetStatus(string widgetId, WidgetStatusUpdateInfo info)
        //{
        //    string serializedObject = JsonConvert.SerializeObject(info);

        //    using (StringContent content = new StringContent(serializedObject, Encoding.UTF8))
        //    {
        //        content.Headers.Remove("Content-Type");
        //        content.Headers.Add("Content-Type", "application/json");

        //        HttpResponseMessage result = await _client.PutAsync(_apiEndpointVer + "/widgets/" + widgetId + "/status", content);
        //        if (result.IsSuccessStatusCode)
        //        {
        //            string response = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
        //            WidgetStatusUpdateResponse widget = JsonConvert.DeserializeObject<WidgetStatusUpdateResponse>(response);

        //            return widget;
        //        }
        //        else
        //        {
        //            string response = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
        //            HandleError(result.StatusCode, response, false);

        //            return null;
        //        }
        //    }
        //}

        ///// <summary>
        ///// Uploads a document and obtains the document's ID to use in an Agreement.
        ///// </summary>
        ///// <param name="fileName">The name for the Transient Document</param>
        ///// <param name="file">The document file</param>
        ///// <param name="mimeType">(Optional) The mime type for the document</param>
        ///// <returns>Returns the uploaded document ID</returns>
        //public async Task<TransientDocument> UploadTransientDocument2(string fileName, byte[] file, string mimeType = null)
        //{
        //    Dictionary<string, string> parameters = new Dictionary<string, string>();
        //    parameters.Add("File-Name",fileName);

        //    MultipartFormDataContent form = new MultipartFormDataContent();
        //    HttpContent content = new StringContent("File");
        //    HttpContent DictionaryItems = new FormUrlEncodedContent(parameters);
        //    form.Add(content, "File");
        //    form.Add(DictionaryItems, "File-Name");

        //    //var stream = new FileStream("c:\\TemporyFiles\\test.jpg", FileMode.Open);
        //    MemoryStream stream = new MemoryStream();

        //    stream.Write(file, 0, file.Length);

        //    content = new StreamContent(stream);
        //    content.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
        //    {
        //        Name = "File",
        //        FileName = fileName
        //    };
        //    form.Add(content);

        //    HttpResponseMessage result = await _client.PostAsync(_apiEndpointVer + "/transientDocuments", form);
        //    if (result.IsSuccessStatusCode)
        //    {
        //        string response = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
        //        TransientDocument document = JsonConvert.DeserializeObject<TransientDocument>(response);

        //        return document;
        //    }
        //    else
        //    {
        //        string response = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
        //        HandleError(result.StatusCode, response);

        //        return null;
        //    }


        //    //using (MultipartFormDataContent content = new MultipartFormDataContent())
        //    //{
        //    //    content.Headers.Remove("Content-Type");
        //    //    content.Headers.Add("Content-Type", "application/x-www-form-urlencoded");

        //    //    //content.Add(new StreamContent(new FileStream(file)), "File", fileName);
        //    //    //content.Add(new StringContent(fileName), "File-Name");

        //    //    if (mimeType != null)
        //    //        content.Add(new StringContent(mimeType), "Mime-Type");

        //    //    HttpResponseMessage result = await _client.PostAsync(_apiEndpointVer + "/transientDocuments", content);
        //    //    if (result.IsSuccessStatusCode)
        //    //    {
        //    //        string response = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
        //    //        TransientDocument document = JsonConvert.DeserializeObject<TransientDocument>(response);

        //    //        return document;
        //    //    }
        //    //    else
        //    //    {
        //    //        string response = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
        //    //        HandleError(result.StatusCode, response);

        //    //        return null;
        //    //    }
        //    //}
        //}

        ///// <summary>
        ///// Sends a reminder for an agreement.
        ///// </summary>
        ///// <param name="agreementId">The agreement identifier, as returned by the agreement creation API or retrieved from the API to fetch agreements</param>
        ///// <returns>AgreementInfo</returns>
        //public async Task<ReminderCreationResult> SendReminders(ReminderCreationInfo info)
        //{
        //    string serializedObject = JsonConvert.SerializeObject(info);

        //    using (StringContent content = new StringContent(serializedObject, Encoding.UTF8))
        //    {
        //        content.Headers.Remove("Content-Type");
        //        content.Headers.Add("Content-Type", "application/json");

        //        HttpResponseMessage result = await _client.PostAsync(_apiEndpointVer + "/reminders", content).ConfigureAwait(false);
        //        if (result.IsSuccessStatusCode)
        //        {
        //            string response = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
        //            ReminderCreationResult agreement = JsonConvert.DeserializeObject<ReminderCreationResult>(response);

        //            return agreement;
        //        }
        //        else
        //        {
        //            string response = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
        //            HandleError(result.StatusCode, response, false);

        //            return null;
        //        }
        //    }
        //}

        private void HandleError(HttpStatusCode statusCode, string response, bool isOAuthError = false)
        {
            switch (statusCode)
            {
                case HttpStatusCode.Unauthorized:
                    AdobeSignError error = JsonConvert.DeserializeObject<AdobeSignError>(response);
                    throw new AdobeSignOAuthException(error);
                case HttpStatusCode.BadRequest:
                    if (!isOAuthError) // AdobeSign returns different json for oAuth calls
                    {
                        AdobeSignErrorCode errorCode = JsonConvert.DeserializeObject<AdobeSignErrorCode>(response);
                        throw new AdobeSignBadRequestException(errorCode);
                    }
                    else
                    {
                        AdobeSignError oAuthError = JsonConvert.DeserializeObject<AdobeSignError>(response);
                        throw new AdobeSignOAuthException(oAuthError);
                    }
                default:
                    AdobeSignErrorCode defaultError = JsonConvert.DeserializeObject<AdobeSignErrorCode>(response);
                    throw new AdobeSignBadRequestException(defaultError);
            }
        }

        public void Dispose()
        {
            _client?.Dispose();
        }

    }
}
