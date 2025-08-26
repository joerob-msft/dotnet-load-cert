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
}
