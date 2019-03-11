using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

using System.Web.Http;
//using System.Web.Mvc;
//using System.Web.Mvc;
using AdobeSignApi.EntityFramework;
using AdobeSignApi.Enums;
using AdobeSignApi.Extensions;
using AdobeSignApi.Models;
using AdobeSignRESTClient;
using AdobeSignRESTClient.Models;
using Newtonsoft.Json;
using RestSharp;

namespace AdobeSignApi.Controllers
{
    [Route("api/AdobeSign")]
    public class AdobeSignController : ApiController
    {
        private const string RefreshTokenKey = "RefreshToken";
        private const string AdobeSignApiUrlKey = "AdobeSignApiUrl";
        private const string AdobeClientIdKey = "AdobeClientId";
        private const string AdobeSecretCodeKey = "AdobeSecretCode";
        private readonly string verifyTokenKey = "BMGVerifyTokenUrl";
        //private const string AdobeSignProxyApiUrlKey = "AdobeSignProxyApiUrlKey";


        private static AdobeSignREST client;
        private readonly CreditAppRepository repository = new CreditAppRepository();
        public AdobeSignController()
        {

            var adobeSignApiUrl = repository.GetKeyValue(AdobeSignApiUrlKey);
            var adobeClientIdKey = repository.GetKeyValue(AdobeClientIdKey);
            var adobeSecretCode = repository.GetKeyValue(AdobeSecretCodeKey);
            client = new AdobeSignREST(adobeSignApiUrl, adobeClientIdKey, adobeSecretCode);
        }

        // GET api/AdobeSign/RefreshToken
        //[HttpGet]
        //[Route("api/AdobeSign/RefreshToken")]
        public void RefreshToken(int? creditDataId)
        {
            var refreshToken = this.repository.GetKeyValue(RefreshTokenKey);
            try
            {

                var response = client.Authorize(refreshToken);
                repository.AddAdobeSignLog(creditDataId, "RefreshToken", $"refreshToken={refreshToken}", response);
            }
            catch (Exception e)
            {
                repository.AddAdobeSignLog(creditDataId, "RefreshToken", $"refreshToken={refreshToken}", e);
                Console.WriteLine(e);
                throw;
            }

        }

        //[HttpPost]
        //[Route("api/AdobeSign/PostTransientDocument")]
        public TransientDocument PostTransientDocument([FromUri]string fileName)
        {
            var httpRequest = HttpContext.Current.Request;
            var postedFile = httpRequest.Files[0];

            var fileStream = postedFile.InputStream;
            byte[] fileByte;

            using (BinaryReader br = new BinaryReader(fileStream))
            {
                fileByte = br.ReadBytes((int)fileStream.Length);
            }
            this.RefreshToken(null);
            var transientDocument = client.UploadTransientDocument(null, fileName, fileByte).Result;
            return transientDocument;
        }

        private TransientDocument PostDocument(int creditDataId, string fileName, byte[] fileBytes)
        {
            try
            {
                var transientDocument = client.UploadTransientDocument(creditDataId, fileName, fileBytes).Result;
                repository.AddAdobeSignLog(creditDataId, "PostDocument", $"fileName={fileName}", transientDocument);
                return transientDocument;
            }
            catch (Exception e)
            {
                repository.AddAdobeSignLog(creditDataId, "PostDocument", $"fileName={fileName}", e);
                Console.WriteLine(e);
                throw;
            }

        }

        [HttpPost]
        [Route("api/AdobeSign/SendDocumentForSignature")]
        public SigningDocumentResponse SendDocumentForSignature([FromUri] string filename, string creditDataId, string recipientEmail, string agreementName)
        {

            var httpRequest = HttpContext.Current.Request;
            var postedFile = httpRequest.Files[0];

            var fileStream = postedFile.InputStream;
            byte[] fileByte;

            using (BinaryReader br = new BinaryReader(fileStream))
            {
                fileByte = br.ReadBytes((int)fileStream.Length);
            }

            int creditId = Convert.ToInt32(creditDataId);
            this.RefreshToken(creditId);
            var transientDocument = this.PostDocument(creditId, filename, fileByte);
            var agreement = this.CreateAgreement(creditId, transientDocument.transientDocumentId, recipientEmail, agreementName);
            SigningUrlResponse signingUrls = new SigningUrlResponse();
            int interval = 1500; //1.5 sec
            var timeOutSettings = repository.GetKeyValue("RequestTimeout");
            int timeout = 10000; // default 10 sec
            if (timeOutSettings != null)
            {
                timeout = Convert.ToInt32(timeOutSettings) * 1000;
            }


            int retries = 5;

            retries = timeout / interval;
            int i = 1;
            while (signingUrls.signingUrlSetInfos == null && i <= retries)
            {
                i++;
                signingUrls = this.GetSigningUrl(creditId, agreement.id);
                if (signingUrls.signingUrlSetInfos == null)
                    Thread.Sleep(interval);
            }

            var adobeSigningUrl = string.Empty;
            if (signingUrls.signingUrlSetInfos != null)
            {
                adobeSigningUrl = signingUrls.signingUrlSetInfos[0].signingUrls[0].esignUrl;
            }
            else
            {
                this.CancelAgreement(creditId, agreement.id);
            }

            return new SigningDocumentResponse
            {
                agreementId = agreement.id,
                signingUrl = adobeSigningUrl
            };

        }

        [HttpPost]
        [Route("api/AdobeSign/GetAgreementDocumentUrl")]
        public DocumentUrl GetAgreementDocumentUrl([FromUri]string agreementId, string creditDataId)
        {
            int creditId = Convert.ToInt32(creditDataId);
            this.RefreshToken(creditId);
            var response = client.GetAgreementDocumentUrl(creditId, agreementId);
            repository.AddAdobeSignLog(creditId, "GetAgreementDocumentUrl", $"agreementId={agreementId}", response);

            return response;
        }

        private AgreementCreationResponse CreateAgreement(int? creditDataId, string transientDocumentId, string recipientEmail, string agreementName)
        {
            var agreementRequest = new AgreementMinimalRequest
            {
                fileInfos = new List<AdobeSignRESTClient.Models.FileInfo>
                {
                    new AdobeSignRESTClient.Models.FileInfo
                    {
                        transientDocumentId = transientDocumentId
                    }
                },
                name = agreementName,
                participantSetsInfo = new List<ParticipantInfo>
                {
                    new ParticipantInfo
                    {
                        memberInfos = new List<MemberInfo>
                        {
                            new MemberInfo
                            {
                                email = recipientEmail
                                //email = string.Empty
                            }
                        },
                        order = 1,
                        role = "SIGNER"
                    }
                },
                emailOption = new EmailOption
                {
                    sendOptions = new SendOption
                    {
                        completionEmails = "NONE",
                        inFlightEmails = "NONE",
                        initEmails = "NONE"
                    }
                },

                externalId = new ExternalId
                {
                    id = creditDataId.Value.ToString()
                },
                signatureType = "ESIGN",
                state = "IN_PROCESS"
                //state="AUTHORING"
            };
            try
            {

                var response = client.CreateAgreement(creditDataId, agreementRequest).Result;
                repository.AddAdobeSignLog(creditDataId, "CreateAgreement", agreementRequest.ToJson(), response);
                return response;
            }
            catch (Exception e)
            {
                repository.AddAdobeSignLog(creditDataId, "CreateAgreement", agreementRequest.ToJson(), e);
                Console.WriteLine(e);
                throw;
            }
        }

        [HttpGet]
        [Route("api/AppStatus")]
        public IHttpActionResult GetAppStatus([FromUri] string token)
        {
            AppStatusResponse response = new AppStatusResponse
            {

                Comments = "",
                LastUpdate = null,
                Status = "NONE"
            };

            TokenInfo tokenInfo = VerifyToken(token, out string tokenErrorMessage);
            if (string.IsNullOrWhiteSpace(tokenErrorMessage))
            {
                if (string.IsNullOrWhiteSpace(tokenInfo.DistributorID) || string.IsNullOrWhiteSpace(tokenInfo.UserID))
                {
                    return BadRequest("Missing user id.");
                }
                else
                {
                    var creditApp = repository.GetCreditApp(tokenInfo.DistributorID, tokenInfo.OrgID);
                    response.DistributorID = Convert.ToInt32(tokenInfo.DistributorID);
                    response.UserID = Convert.ToInt32(tokenInfo.UserID);
                    response.RetailerID = Convert.ToInt32(tokenInfo.OrgID);
                    if (creditApp != null)
                    {
                        response.LastUpdate = creditApp.LastUpdate;
                        response.RetailerID = creditApp.RetailerId.Value;
                        if (creditApp.Status == CreditAppStatusEnum.APPROVED.ToString() || creditApp.Status == CreditAppStatusEnum.DENIED.ToString())
                        {
                            response.LastUpdate = creditApp.LastUpdate;
                            response.Status = creditApp.Status;
                            response.Comments = this.GetComments(creditApp.Id);
                        }
                        else
                        {
                            if (!string.IsNullOrWhiteSpace(creditApp.AdobeSignAgreementId))
                            {
                                this.RefreshToken(null);
                                var agreement = this.GetAgreement(creditApp.AdobeSignAgreementId, creditApp.Id).Result;
                                // update CreditData status
                                repository.UpdateCreditAppStatus(creditApp.Id.Value, agreement.status);
                                response.Status = agreement.status;
                            }
                            else
                            {
                                response.Status = creditApp.Status;
                                response.Comments = this.GetComments(creditApp.Id);
                            }
                        }
                    }
                    return Ok(response);
                }

            }
            else
            {
                return BadRequest(tokenErrorMessage);

            }

        }

        private TokenInfo VerifyToken(string token, out string errorMessage)
        {
            TokenInfo tokenInfo = default(TokenInfo);
            errorMessage = "";

            string url = repository.GetKeyValue(verifyTokenKey);
            var client = new RestClient(url);
            var request = new RestRequest(Method.POST);
            request.AddHeader("Token", token);
            IRestResponse response = client.Execute(request);
            try
            {
                tokenInfo = JsonConvert.DeserializeObject<TokenInfo>(response.Content);
            }
            catch (Exception)
            {
                errorMessage = response.Content;
            }
            return tokenInfo;
        }
        [HttpPut]
        [Route("api/AdobeSign/CancelAgreement")]
        public AgreementCancelResponse CancelCreditAgreement(int? creditDataId, string agreementId)
        {
            int creditId = Convert.ToInt32(creditDataId);
            this.RefreshToken(creditId);
            var cancelInfo = this.CancelAgreement(creditId, agreementId);
            string retVal;
            return new AgreementCancelResponse
            {
                status = cancelInfo
            };
        }

        private string CancelAgreement(int? creditDataId, string agreementId)
        {
            try
            {
                var prevAgreement = client.GetAgreement(creditDataId, agreementId);
                client.UpdateAgreementStatus(creditDataId, agreementId, "CANCELED");

                repository.AddAdobeSignLog(creditDataId, "CancelAgreement", new
                {
                    AgreementId = agreementId,
                    PreviousStatus = prevAgreement.status
                }.ToJson()
                , new { status = "CANCELED" });
                var agreementResponse = client.GetAgreement(creditDataId, agreementId);
                repository.AddAdobeSignLog(creditDataId, "GetAgreement", $"AgreementId={agreementId}", agreementResponse);

                return agreementResponse.status;
            }
            catch (Exception e)
            {
                repository.AddAdobeSignLog(creditDataId, "CancelAgreement", $"AgreementId={agreementId}", e);
                Console.WriteLine(e);
                throw;
            }
        }

        //[HttpPost]
        //[Route("api/AdobeSign/CreateAgreement")]
        public async Task<AgreementCreationResponse> CreateAgreement([FromBody]AgreementMinimalRequest agreement)
        {
            Task<AgreementCreationResponse> response;
            try
            {
                this.RefreshToken(null);
                response = Task.FromResult(client.CreateAgreement(null, agreement).Result);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            return await response;
        }

        [HttpGet]
        [Route("api/AdobeSign/GetAgreement")]
        public Task<AgreementResponse> GetAgreement([FromUri] string agreementId, int? creditDataId)
        {
            AgreementResponse response;
            try
            {
                this.RefreshToken(creditDataId);
                response = client.GetAgreement(creditDataId, agreementId);
                repository.AddAdobeSignLog(creditDataId, "GetAgreement", $"AgreementId={agreementId}", response);
            }
            catch (Exception e)
            {
                repository.AddAdobeSignLog(creditDataId, "GetAgreement", $"AgreementId={agreementId}", e);
                Console.WriteLine(e);
                throw;
            }

            return Task.FromResult(response);
        }

        private SigningUrlResponse GetSigningUrl(int? creditDataId, string agreementId)
        {
            SigningUrlResponse response;
            try
            {
                //this.RefreshToken(creditDataId);
                response = client.GetAgreementSigningUrl(creditDataId, agreementId).Result;
                repository.AddAdobeSignLog(creditDataId, "GetAgreementSigningUrl", $"agreementId={agreementId}", response);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                repository.AddAdobeSignLog(creditDataId, "GetAgreementSigningUrl", $"agreementId={agreementId}", e);
                throw;
            }

            //response.signingUrlSetInfos = null;
            return response;
        }

        [HttpGet]
        [Route("api/AdobeSign/GetAgreementSigningUrl")]
        public SigningUrlResponse GetAgreementSigningUrl([FromUri]string agreementId, string creditDataId)
        {
            SigningUrlResponse response;
            try
            {
                int creditId = Convert.ToInt32(creditDataId);
                this.RefreshToken(creditId);
                response = this.GetSigningUrl(creditId, agreementId);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }


            return response;
            //client.CreateAgreement();
        }

        //[HttpPut]
        //[Route("api/AdobeSign/PutAgreementSigningPosition")]
        public async Task<AgreementSigningPositionResponse> AgreementSigningPosition([FromUri]string agreementId, float height, float left, float top, float width)
        {

            Task<AgreementSigningPositionResponse> response;
            try
            {
                this.RefreshToken(null);
                FormFieldPutInfo field = new FormFieldPutInfo();
                field.fields = new List<FormField>();
                field.fields.Add(new FormField
                {
                    name = "sigBlock1",
                    locations = new List<FormLocation>
                    {
                        new FormLocation
                        {
                            height = height,
                            left = left,
                            top = top,
                            width = width,
                            pageNumber = "1"
                        }
                    }
                });

                response = Task.FromResult(client.AgreementSigningPosition(null, agreementId, field).Result);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }


            return await response;
        }

        [HttpPost]
        [Route("api/AdobeSign/DocEvents")]
        public void CallBack([FromBody] WebHookInfo webHookInfo)
        {
            try
            {
                //using (var context = new CreditAppContext())
                //{
                //    var creditDataEntity = context.CreditData.SingleOrDefault(x =>
                //        x.AdobeSignAgreementId == webHookInfo.agreement.id);
                //    if (creditDataEntity != null)
                //    {
                //        creditDataEntity.Status = webHookInfo.agreement.status;
                //        context.SaveChanges();
                //    }
                //}
                //log

                repository.AddAdobeSignLog(null, "UpdateCreditApplicationStatus", $"AgreementId={webHookInfo.agreement.id}", webHookInfo);
                ////repository.AddAdobeSignLog(null, "UpdateCreditApplicationStatus",null , webHookInfo);
                UpdateAllAgreementsStatuses();
            }
            catch (Exception e)
            {
                repository.AddAdobeSignLog(null, "UpdateCreditApplicationStatus", $"AgreementId={webHookInfo.agreement.id}", e);
                throw;
            }
        }

        [HttpGet]
        [Route("api/AdobeSign/DocEvents")]
        // api/AdobeSign/CallBack
        public IHttpActionResult CallBack()
        {
            var hasHeader = Request.Headers.TryGetValues("X-ADOBESIGN-CLIENTID", out var clientIds);

            //return response;
            HttpContext.Current.Response.Headers.Add("X-ADOBESIGN-CLIENTID", clientIds.FirstOrDefault());
            //if (hasHeader)
            //    return Json(new { xAdobeSignClientId = clientIds.FirstOrDefault() });
            //else
            //{
            //    return Json(new { xAdobeSignClientId = "Unreached" });
            //}
            //var clientId = repository.GetKeyValue("AdobeClientId");
            //return Json(new { xAdobeSignClientId = clientId });
            //var clientid = Request.Headers["X-ADOBESIGN-CLIENTID"];
            //if (clientid == "CBJCHBCAABAAShWitqkQgjhBRXFQH7zuOHJsdG-Vi4GS")
            //    return Json(new { xAdobeSignClientId = "PZaD8TnKTzl0XSwdF6orBzGbPx6OcBwr" });
            //else
            //{
            //    return new BadRequestResult();
            return Ok();
        }

        private void UpdateAllAgreementsStatuses()
        {
            using (var context = new CreditAppContext())
            {
                var creditDataEntities = context.CreditData.Where(x => x.Status != "SIGNED" && x.AdobeSignAgreementId != null && x.AdobeSignAgreementId != "").ToList();
                if (creditDataEntities.Any())
                {
                    this.RefreshToken(null);
                    foreach (var creditDataEntity in creditDataEntities)
                    {
                        var response = client.GetAgreement(creditDataEntity.Id, creditDataEntity.AdobeSignAgreementId);
                        repository.AddAdobeSignLog(creditDataEntity.Id, "UpdateAgreementStatus", $"agreementId={creditDataEntity.AdobeSignAgreementId}", response);
                        if (response.status == "CANCELLED")
                        {
                            creditDataEntity.Status = null;
                            creditDataEntity.SigningUrl = null;
                            creditDataEntity.AdobeSignAgreementId = null;
                        }
                        else
                        {
                            creditDataEntity.Status = response.status;
                        }
                        context.SaveChanges();
                    }

                }
            }
        }

        private string GetComments(int? creditDataId)
        {
            string retVal = string.Empty;
            if (creditDataId.HasValue)
            {
                CreditAppRepository repository = new CreditAppRepository();
                retVal = repository.GetCreditAppComments(creditDataId.Value);
            }
            return retVal;
        }
    }
}
