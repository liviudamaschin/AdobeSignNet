using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

using System.Web.Http;
using System.Web.Http.Results;
using AdobeSignApi.EntityFramework;
using AdobeSignApi.Extensions;
using AdobeSignApi.Models;
using AdobeSignRESTClient;
using AdobeSignRESTClient.Models;

namespace AdobeSignApi.Controllers
{
    [Route("api/AdobeSign")]
    public class AdobeSignController : ApiController
    {
        private const string RefreshTokenKey = "RefreshToken";
        private const string AdobeSignApiUrlKey = "AdobeSignApiUrl";
        private const string AdobeClientIdKey = "AdobeClientId";
        private const string AdobeSecretCodeKey = "AdobeSecretCode";
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
                repository.AddAdobeSignLog(creditDataId, "RefreshToken", $"refreshToken={refreshToken}", response.ToJson());
            }
            catch (Exception e)
            {
                repository.AddAdobeSignLog(creditDataId, "RefreshToken", $"refreshToken={refreshToken}", e.ToJson());
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
            var transientDocument = client.UploadTransientDocument(null,fileName, fileByte).Result;
            return transientDocument;
        }

        private TransientDocument PostDocument(int creditDataId, string fileName, byte[] fileBytes)
        {
            try
            {
                var transientDocument = client.UploadTransientDocument(creditDataId, fileName, fileBytes).Result;
                repository.AddAdobeSignLog(creditDataId, "PostDocument", $"fileName={fileName}", transientDocument.ToJson());
                return transientDocument;
            }
            catch (Exception e)
            {
                repository.AddAdobeSignLog(creditDataId, "PostDocument", $"fileName={fileName}", e.ToJson());
                Console.WriteLine(e);
                throw;
            }
            
        }

        [HttpPost]
        [Route("api/AdobeSign/SendDocumentForSignature")]
        public SigningDocumentResponse SendDocumentForSignature([FromUri] string filename, string creditDataId, string recipientEmail)
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
            var transientDocument=this.PostDocument(creditId, filename, fileByte);
            //repository.AddAdobeSignLog(creditId, "PostDocument", filename, transientDocument.ToJson());
            var agreement = this.CreateAgreement(creditId, transientDocument.transientDocumentId, recipientEmail);
            //repository.AddAdobeSignLog(creditId, "CreateAgreement", $"transientDocumentId={transientDocument.transientDocumentId}, recipientEmail={recipientEmail}", agreement.ToJson());
            SigningUrlResponse signingUrls = new SigningUrlResponse();
            int retries = 5;
            int i = 1;
            while (signingUrls.signingUrlSetInfos == null && i <= retries)
            {
                i++;
                signingUrls = this.GetSigningUrl(creditId, agreement.id);
                if (signingUrls.signingUrlSetInfos == null)
                    Thread.Sleep(1000);
            }

            var adobeSigningUrl = string.Empty;
            if (signingUrls.signingUrlSetInfos != null)
            {
                adobeSigningUrl = signingUrls.signingUrlSetInfos[0].signingUrls[0].esignUrl;
            }

            return new SigningDocumentResponse
                {
                    agreementId = agreement.id, signingUrl = adobeSigningUrl
            };
         
        }

        private AgreementCreationResponse CreateAgreement(int? creditDataId, string transientDocumentId, string recipientEmail)
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
                name = "agreementName",
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
                signatureType = "ESIGN",
                state = "IN_PROCESS"
                //state="AUTHORING"
            };
            try
            {
                
                var response = client.CreateAgreement(creditDataId, agreementRequest).Result;
                repository.AddAdobeSignLog(creditDataId, "CreateAgreement", agreementRequest.ToJson(), response.ToJson());
                return response;
            }
            catch (Exception e)
            {
                repository.AddAdobeSignLog(creditDataId, "CreateAgreement", agreementRequest.ToJson(), e.ToJson());
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
                 response = Task.FromResult(client.CreateAgreement(null,agreement).Result);
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
        public Task<AgreementResponse> GetAgreement([FromUri] string agreementId)
        {
            AgreementResponse response;
            try
            {
                this.RefreshToken(null);
                response =  client.GetAgreement(null,agreementId);

            }
            catch (Exception e)
            {
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
                repository.AddAdobeSignLog(creditDataId, "GetAgreementSigningUrl", $"agreementId={agreementId}", response.ToJson());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                repository.AddAdobeSignLog(creditDataId, "GetAgreementSigningUrl", $"agreementId={agreementId}", e.ToJson());
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

                response = Task.FromResult(client.AgreementSigningPosition(null,agreementId, field).Result);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }


            return await response;
        }


        //[HttpGet]
        //[Route("api/AdobeSign/UpdateAgreementStatus")]
        public IHttpActionResult UpdateAgreementStatus()
        {

            //var clientid = Request.Headers["X-ADOBESIGN-CLIENTID"];
            //if (clientid == "CBJCHBCAABAAShWitqkQgjhBRXFQH7zuOHJsdG-Vi4GS")
            //    return Json(new { xAdobeSignClientId = "PZaD8TnKTzl0XSwdF6orBzGbPx6OcBwr" });
            //else
            //{
            //    return new BadRequestResult();
            //}
            return null;
        }

        [HttpPost]
        [Route("api/AdobeSign/DocEvents")]
        public void CallBack([FromBody] WebHookInfo webHookInfo)
        {
            try
            {
                using (var context = new CreditAppContext())
                {
                    var creditDataEntity = context.CreditData.SingleOrDefault(x =>
                        x.AdobeSignAgreementId == webHookInfo.agreement.id);
                    if (creditDataEntity != null)
                    {
                        creditDataEntity.Status = webHookInfo.Event;
                        context.SaveChanges();
                    }
                }
                //log
                repository.AddAdobeSignLog(null,"UpdateCreditApplicationStatus", $"AgreementId={webHookInfo.agreement.id}", webHookInfo.ToJson());
                UpdateAllAgreementsStatuses();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
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
                var creditDataEntities = context.CreditData.Where(x => x.Status != "SIGNED" && !string.IsNullOrWhiteSpace(x.AdobeSignAgreementId)).ToList();
                if (creditDataEntities.Any())
                {
                    foreach (var creditDataEntity in creditDataEntities)
                    {
                        var response = client.GetAgreement(creditDataEntity.Id, creditDataEntity.AdobeSignAgreementId);
                        repository.AddAdobeSignLog(creditDataEntity.Id, "UpdateAllAgreementsStatuses", $"agreementId={creditDataEntity.AdobeSignAgreementId}", response.ToJson());
                        creditDataEntity.Status = response.status;
                        context.SaveChanges();
                    }
                   
                }
            }
        }
    }
}
