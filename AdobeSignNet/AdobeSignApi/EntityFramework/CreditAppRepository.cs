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
            return this._context.ApplicationConfigurations.SingleOrDefault(x => x.ConfigKey == key && x.IsActive).ConfigValue;
        }

        public void AddAdobeSignLog(int? creditDataId, string action, string request, object response)
        {
            object agreementStatus = null;
            if (response!=null)
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

    }
}