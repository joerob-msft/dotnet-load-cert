namespace AzureCertInventory.Models
{
    /// <summary>
    /// System information and environment details
    /// </summary>
    public class SystemInfo
    {
        /// <summary>
        /// Current hostname
        /// </summary>
        public string Hostname { get; set; } = string.Empty;

        /// <summary>
        /// Value of WEBSITE_LOAD_CERTIFICATES environment variable
        /// </summary>
        public string CertificateEnvironmentVariable { get; set; } = string.Empty;

        /// <summary>
        /// Azure App Service plan tier
        /// </summary>
        public string AppServicePlan { get; set; } = string.Empty;
    }

    /// <summary>
    /// Error response
    /// </summary>
    public class ErrorResponse
    {
        /// <summary>
        /// Error message
        /// </summary>
        public string Error { get; set; } = string.Empty;

        /// <summary>
        /// Additional error details
        /// </summary>
        public string? Details { get; set; }
    }

    /// <summary>
    /// Request model for importing certificates
    /// </summary>
    public class CertificateImportRequest
    {
        /// <summary>
        /// Base64 encoded certificate data
        /// </summary>
        public string CertificateData { get; set; } = string.Empty;

        /// <summary>
        /// Optional password for encrypted certificates
        /// </summary>
        public string? Password { get; set; }

        /// <summary>
        /// Optional friendly name for the certificate
        /// </summary>
        public string? FriendlyName { get; set; }
    }

    /// <summary>
    /// Request model for certificate validation
    /// </summary>
    public class CertificateValidationRequest
    {
        /// <summary>
        /// Certificate thumbprint to validate
        /// </summary>
        public string Thumbprint { get; set; } = string.Empty;

        /// <summary>
        /// Whether to validate the certificate chain
        /// </summary>
        public bool ValidateChain { get; set; } = true;

        /// <summary>
        /// Whether to check certificate revocation
        /// </summary>
        public bool CheckRevocation { get; set; } = false;

        /// <summary>
        /// Optional URL to test certificate against
        /// </summary>
        public string? TestUrl { get; set; }
    }

    /// <summary>
    /// Result of certificate validation
    /// </summary>
    public class CertificateValidationResult
    {
        /// <summary>
        /// Whether the certificate is valid
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Validation message or error details
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Chain validation status
        /// </summary>
        public bool ChainValid { get; set; }

        /// <summary>
        /// Revocation check status
        /// </summary>
        public bool? RevocationValid { get; set; }

        /// <summary>
        /// URL test result if applicable
        /// </summary>
        public string? UrlTestResult { get; set; }

        /// <summary>
        /// Certificate expiration status
        /// </summary>
        public string ExpirationStatus { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response for successful certificate operations
    /// </summary>
    public class CertificateOperationResponse
    {
        /// <summary>
        /// Success status
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Operation message
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Certificate thumbprint if applicable
        /// </summary>
        public string? Thumbprint { get; set; }
    }
}
