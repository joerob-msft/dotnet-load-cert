namespace AzureCertInventory.Models
{
    public class CertificateInfo
    {
        public string Name { get; set; } = string.Empty;
        public string? StoreName { get; set; }
        public string? StoreLocation { get; set; }
        public string? Subject { get; set; }
        public string? Issuer { get; set; }
        public string? SerialNumber { get; set; }
        public string? ValidFrom { get; set; }
        public string? ValidUntil { get; set; }
        public string Status { get; set; } = string.Empty;
        public int? DaysLeft { get; set; }
        public string? Thumbprint { get; set; }
        public bool? HasPrivateKey { get; set; }
        public string? Error { get; set; }
    }
}