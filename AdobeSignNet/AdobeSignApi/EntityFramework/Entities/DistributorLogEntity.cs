using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdobeSignApi.EntityFramework.Entities
{
    [Table("DistributorLog")]
    public class DistributorLogEntity
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int? Id { get; set; }
        public int CreditDataId { get; set; }
        public string Status { get; set; }
        public string Comments { get; set; }
        public DateTime? LastUpdate { get; set; }
    }
}