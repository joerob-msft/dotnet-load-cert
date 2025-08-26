using AzureCertInventory.Models;

namespace AzureCertInventory.Services
{
    /// <summary>
    /// Service interface for certificate operations
    /// </summary>
    public interface ICertificateService
    {
        /// <summary>
        /// Gets all public certificates
        /// </summary>
        /// <returns>Collection of public certificate information</returns>
        IEnumerable<CertificateInfo> GetPublicCertificates();
        
        /// <summary>
        /// Gets all private certificates from the current user's certificate store
        /// </summary>
        /// <returns>Collection of private certificate information</returns>
        IEnumerable<CertificateInfo> GetPrivateCertificates();

        /// <summary>
        /// Gets certificates loaded via App Service (WEBSITE_LOAD_CERTIFICATES)
        /// </summary>
        /// <returns>Collection of App Service loaded certificates</returns>
        IEnumerable<CertificateInfo> GetAppServiceCertificates();

        /// <summary>
        /// Loads a certificate into application memory (App Service compatible)
        /// </summary>
        /// <param name="certificateData">Base64 encoded certificate data</param>
        /// <param name="password">Certificate password (optional)</param>
        /// <param name="friendlyName">Friendly name for the certificate</param>
        /// <returns>Certificate information if successful</returns>
        Task<CertificateInfo> LoadCertificateAsync(string certificateData, string? password = null, string? friendlyName = null);

        /// <summary>
        /// Validates a certificate by thumbprint
        /// </summary>
        /// <param name="thumbprint">Certificate thumbprint</param>
        /// <returns>Validation result</returns>
        Task<CertificateValidationResult> ValidateCertificateAsync(string thumbprint);

        /// <summary>
        /// Gets certificates currently loaded in application memory
        /// </summary>
        /// <returns>Collection of in-memory certificates</returns>
        IEnumerable<CertificateInfo> GetLoadedCertificates();

        /// <summary>
        /// Removes a certificate from application memory
        /// </summary>
        /// <param name="thumbprint">Certificate thumbprint</param>
        /// <returns>True if removed successfully</returns>
        bool RemoveLoadedCertificate(string thumbprint);
    }
}