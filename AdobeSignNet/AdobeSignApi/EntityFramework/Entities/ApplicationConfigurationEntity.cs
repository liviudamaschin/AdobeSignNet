using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdobeSignApi.EntityFramework.Entities
{
    [Table("ApplicationConfiguration")]
    public class ApplicationConfigurationEntity
    {
        public int Id { get; set; }

        [Required]
        public string ConfigKey { get; set; }

        [Required]
        public string ConfigValue { get; set; }

        public bool IsActive { get; set; }
    }
}
