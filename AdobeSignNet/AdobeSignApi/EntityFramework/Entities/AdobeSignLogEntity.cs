using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdobeSignApi.EntityFramework.Entities
{
    [Table("AdobeSignLog")]
    public class AdobeSignLogEntity
    {
        [Key]
        public int Id { get; set; }
        public int? CreditDataId { get; set; }
        public string Action { get; set; }
        public string Request { get; set; }
        public string Response { get; set; }
        public string AgreementStatus { get; set; }

    }
}