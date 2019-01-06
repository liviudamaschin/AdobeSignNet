using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AdobeSignApi.EntityFramework.Entities;

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

        public void AddAdobeSignLog(int? creditDataId, string action, string request, string response)
        {
            using (var context = new CreditAppContext())
            {
                AdobeSignLogEntity entity = new AdobeSignLogEntity
                {
                    CreditDataId = creditDataId,
                    Action = action,
                    Request = request,
                    Response = response
                };

                context.AdobeSignLogs.Add(entity);
                context.SaveChanges();
            }
        }

    }
}