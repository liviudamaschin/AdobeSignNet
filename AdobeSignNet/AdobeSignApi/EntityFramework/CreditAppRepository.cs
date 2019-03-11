using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AdobeSignApi.EntityFramework.Entities;
using AdobeSignApi.Extensions;

namespace AdobeSignApi.EntityFramework
{
    public class CreditAppRepository
    {
        private CreditAppContext _context;

        public CreditAppRepository()
        {
            this._context = new CreditAppContext();
        }

        public string GetKeyValue(string key)
        {
            string retValue = null;

            var applicationConfiguration = this._context.ApplicationConfigurations.SingleOrDefault(x => x.ConfigKey == key && x.IsActive);
            if (applicationConfiguration != null)
                retValue = applicationConfiguration.ConfigValue;
            return retValue;
        }

        public void AddAdobeSignLog(int? creditDataId, string action, string request, object response)
        {
            object agreementStatus = null;
            if (response != null)
                agreementStatus = response.GetPropertyValue("status");

            using (var context = new CreditAppContext())
            {
                AdobeSignLogEntity entity = new AdobeSignLogEntity
                {
                    CreditDataId = creditDataId,
                    Action = action,
                    Request = request,
                    Response = response.ToJson(),
                    AgreementStatus = agreementStatus?.ToString()
                };
               
                context.AdobeSignLogs.Add(entity);
                context.SaveChanges();
            }
        }

        public CreditDataEntity GetCreditApp(string distributorId, string retailerId)
        {
            int retId = Convert.ToInt32(retailerId);
            return _context.CreditData.Where(x => x.DistributorId == distributorId && x.RetailerId == retId).FirstOrDefault();
        }

        public void UpdateCreditAppStatus(int creditDataId, string status)
        {
            var creditDataEntity = this._context.CreditData.SingleOrDefault(x => x.Id == creditDataId);
            if (creditDataEntity != null)
            {
                creditDataEntity.Status = status;
                _context.SaveChanges();
            }
        }

        public string GetCreditAppComments(int creditDataId)
        {
            return this._context.DistributorLogs.OrderByDescending(o=>o.Id).FirstOrDefault(x => x.CreditDataId == creditDataId)?.Comments;
        }
    }
}