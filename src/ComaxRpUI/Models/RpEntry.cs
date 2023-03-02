using System.Text.Json.Serialization;

namespace ComaxRpUI.Models
{
    public class RpEntry
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string IngressHost { get; set; }
        public string ForwardAddress { get; set; }
        public string IngressCertManager { get; set; }
        public string IngressCertSecret { get; set; }
        public bool UseHttps { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public bool Managed { get; set; }
        public bool Active { get; set; }
    }
}
