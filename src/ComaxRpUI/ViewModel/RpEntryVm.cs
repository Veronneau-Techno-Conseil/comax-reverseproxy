using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ComaxRpUI.Models
{
    public class RpEntryVm
    {
        [Required]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required]
        [MinLength(5)]
        public string Name { get; set; }
        [Required]
        public string IngressHost { get; set; }
        [Required]
        [Url]
        public string ForwardAddress { get; set; }
        [Required]
        public string IngressCertManager { get; set; }
        public string IngressCertSecret { get; set; }
        public bool UseHttps { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public bool Managed { get; set; }
        public bool Active { get; set; }
    }


    public class RpEntryMessages
    {
        public string[] Name { get; set; }
        public string[] IngressHost { get; set; }
        public string[] ForwardAddress { get; set; }
        public string[] IngressCertManager { get; set; }
        public string[] IngressCertSecret { get; set; }
        public string[] UseHttps { get; set; }
        public string[] CreatedDate { get; set; }
        public string[] ModifiedDate { get; set; }
        public string[] Managed { get; set; }
        public string[] Active { get; set; }
    }
}
