using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdobeSignApi.EntityFramework.Entities
{
    [Table("CreditData")]
    public partial class CreditDataEntity
    {
        [Key]
        public int? Id { get; set; }
        public string DistributorId { get; set; }
        public int? RetailerId { get; set; }
        public int? UserId { get; set; }
        public string Token { get; set; }
        [MaxLength(100)]
        public string BusinessName { get; set; }
        [MaxLength(100)]
        public string TradeName { get; set; }
        [MaxLength(50)]
        public string LicenseNumber { get; set; }
        public DateTime? LicenseExpirationDate { get; set; }
        //MaxLength(50)]
        //public string Ein { get; set; }
        [MaxLength(50)]
        public string NystateTaxId { get; set; }
        public string DeliveryTime { get; set; }
        public string CompanyType { get; set; }
        public string Phone { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public string PrincipalName { get; set; }
        public string PrincipalTitle { get; set; }
        public string PrincipalEmail { get; set; }
        public string PrincipalPhone { get; set; }
        //public string PrincipalSsn { get; set; }
        public string PrincipalAddress1 { get; set; }
        public string PrincipalAddress2 { get; set; }
        public string PrincipalCity { get; set; }
        public string PrincipalState { get; set; }
        public string PrincipalZipCode { get; set; }
        public bool? PropertyOwned { get; set; }
        public string PropertyType { get; set; }
        public string PropertyAddress1 { get; set; }
        public string PropertyAddress2 { get; set; }
        public string PropertyCity { get; set; }
        public string PropertyState { get; set; }
        public string PropertyZipCode { get; set; }
        public bool? PriorBusiness { get; set; }
        public string PriorBusinessAddress1 { get; set; }
        public string PriorBusinessAddress2 { get; set; }
        public string PriorBusinessCity { get; set; }
        public string PriorBusinessState { get; set; }
        public string PriorBusinessZipCode { get; set; }
        public string BillingContactName { get; set; }
        public string BillingContactEmail { get; set; }
        public string BillingContactPhone { get; set; }
        public string BillingContactAddress1 { get; set; }
        public string BillingContactAddress2 { get; set; }
        public string BillingContactCity { get; set; }
        public string BillingContactState { get; set; }
        public string BillingContactZipCode { get; set; }
        public string BankReferenceName { get; set; }
        public string BankReferencePhone { get; set; }
        public string BankReferenceAccountType { get; set; }
        public string BankReferenceAccountNumber { get; set; }
        public string BankReferenceRoutingNumber { get; set; }
        public string BankReferenceAddress1 { get; set; }
        public string BankReferenceAddress2 { get; set; }
        public string BankReferenceCity { get; set; }
        public string BankReferenceState { get; set; }
        public string BankReferenceZipCode { get; set; }
        public string TradeReference1Name { get; set; }
        public string TradeReference1Phone { get; set; }
        public string TradeReference1AccountNumber { get; set; }
        public string TradeReference1Address1 { get; set; }
        public string TradeReference1Address2 { get; set; }
        public string TradeReference1City { get; set; }
        public string TradeReference1State { get; set; }
        public string TradeReference1ZipCode { get; set; }
        public string TradeReference2Name { get; set; }
        public string TradeReference2Phone { get; set; }
        public string TradeReference2AccountNumber { get; set; }
        public string TradeReference2Address1 { get; set; }
        public string TradeReference2Address2 { get; set; }
        public string TradeReference2City { get; set; }
        public string TradeReference2State { get; set; }
        public string TradeReference2ZipCode { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? LastUpdate { get; set; }
        public string Status { get; set; }
        public string SigningUrl { get; set; }
        public string AdobeSignAgreementId { get; set; }
        
    }
}