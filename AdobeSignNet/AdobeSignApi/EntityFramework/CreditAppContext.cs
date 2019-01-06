using System.Data.Entity;
using AdobeSignApi.EntityFramework.Entities;

namespace AdobeSignApi.EntityFramework
{
    public class CreditAppContext : DbContext
    {
        public CreditAppContext() : base("name=CreditAppContext")
        {
            this.Database.Log = s => System.Diagnostics.Debug.WriteLine(s);
        }
      
        public DbSet<ApplicationConfigurationEntity> ApplicationConfigurations { get; set; }
        public DbSet<CreditDataEntity> CreditData { get; set; }
        public DbSet<AdobeSignLogEntity> AdobeSignLogs { get; set; }
        
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }
        
    }
}