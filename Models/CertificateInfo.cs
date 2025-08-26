namespace AzureCertInventory.Models
{
    /// <summary>
    /// Information about a certificate
    /// </summary>
    public class CertificateInfo
    {
        /// <summary>
        /// Friendly name of the certificate or extracted from subject
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Name of the certificate store
        /// </summary>
        public string? StoreName { get; set; }
        
        /// <summary>
        /// Location of the certificate store
        /// </summary>
        public string? StoreLocation { get; set; }
        
        /// <summary>
        /// Certificate subject
        /// </summary>
        public string? Subject { get; set; }
        
        /// <summary>
        /// Certificate issuer
        /// </summary>
        public string? Issuer { get; set; }
        
        /// <summary>
        /// Certificate serial number
        /// </summary>
        public string? SerialNumber { get; set; }
        
        /// <summary>
        /// Certificate valid from date in UTC
        /// </summary>
        public string? ValidFrom { get; set; }
        
        /// <summary>
        /// Certificate valid until date in UTC
        /// </summary>
        public string? ValidUntil { get; set; }
        
        /// <summary>
        /// Certificate status (Valid, Warning, or Expired)
        /// </summary>
        public string Status { get; set; } = string.Empty;
        
        /// <summary>
        /// Number of days until certificate expires
        /// </summary>
        public int? DaysLeft { get; set; }
        
        /// <summary>
        /// Certificate thumbprint
        /// </summary>
        public string? Thumbprint { get; set; }
        
        /// <summary>
        /// Whether the certificate has a private key
        /// </summary>
        public bool? HasPrivateKey { get; set; }
        
        /// <summary>
        /// Error message if certificate processing failed
        /// </summary>
        public string? Error { get; set; }
    }
}