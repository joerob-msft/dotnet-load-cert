using AzureCertInventory.Models;
using System.Security.Cryptography.X509Certificates;

namespace AzureCertInventory.Services
{
    /// <summary>
    /// Service for managing certificate operations and retrieval
    /// </summary>
    public class CertificateService : ICertificateService
    {
        private readonly ILogger<CertificateService> _logger;

        /// <summary>
        /// Initializes a new instance of the CertificateService
        /// </summary>
        /// <param name="logger">The logger instance</param>
        public CertificateService(ILogger<CertificateService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Return empty list for public certificates as requested
        /// </summary>
        /// <returns>Empty collection of certificate information</returns>
        public IEnumerable<CertificateInfo> GetPublicCertificates()
        {
            return new List<CertificateInfo>();
        }

        /// <summary>
        /// Gets all private certificates from the current user's certificate store
        /// </summary>
        /// <returns>Collection of private certificate information</returns>
        public IEnumerable<CertificateInfo> GetPrivateCertificates()
        {
            var certificates = new List<CertificateInfo>();

            try
            {
                // Changed to CurrentUser MY store instead of LocalMachine
                using var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
                store.Open(OpenFlags.ReadOnly);

                foreach (var cert in store.Certificates)
                {
                    certificates.Add(GetCertificateDetails(cert, StoreName.My.ToString(), StoreLocation.CurrentUser.ToString()));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving personal certificates");
                certificates.Add(new CertificateInfo
                {
                    Name = "Error",
                    Error = $"Failed to access certificate store: {ex.Message}"
                });
            }

            return certificates;
        }

        /// <summary>
        /// Gets certificates loaded via App Service (WEBSITE_LOAD_CERTIFICATES) - delegates to base implementation
        /// </summary>
        /// <returns>Collection of App Service loaded certificates</returns>
        public IEnumerable<CertificateInfo> GetAppServiceCertificates()
        {
            return GetPrivateCertificates(); // Base implementation uses same store
        }

        /// <summary>
        /// Loads a certificate into application memory (not supported in base implementation)
        /// </summary>
        /// <param name="certificateData">Base64 encoded certificate data</param>
        /// <param name="password">Certificate password (optional)</param>
        /// <param name="friendlyName">Friendly name for the certificate</param>
        /// <returns>Certificate information indicating operation not supported</returns>
        public Task<CertificateInfo> LoadCertificateAsync(string certificateData, string? password = null, string? friendlyName = null)
        {
            return Task.FromResult(new CertificateInfo
            {
                Name = "Not Supported",
                Error = "Certificate loading not supported in base implementation. Use AppServiceCertificateService for App Service deployment.",
                Status = "Error"
            });
        }

        /// <summary>
        /// Validates a certificate by thumbprint (basic implementation)
        /// </summary>
        /// <param name="thumbprint">Certificate thumbprint</param>
        /// <returns>Basic validation result</returns>
        public Task<CertificateValidationResult> ValidateCertificateAsync(string thumbprint)
        {
            var result = new CertificateValidationResult
            {
                IsValid = false,
                Message = "Certificate validation not supported in base implementation. Use AppServiceCertificateService for full validation.",
                ChainValid = false,
                ExpirationStatus = "Unknown"
            };

            return Task.FromResult(result);
        }

        /// <summary>
        /// Gets certificates currently loaded in application memory (not supported in base implementation)
        /// </summary>
        /// <returns>Empty collection</returns>
        public IEnumerable<CertificateInfo> GetLoadedCertificates()
        {
            return new List<CertificateInfo>();
        }

        /// <summary>
        /// Removes a certificate from application memory (not supported in base implementation)
        /// </summary>
        /// <param name="thumbprint">Certificate thumbprint</param>
        /// <returns>False - operation not supported</returns>
        public bool RemoveLoadedCertificate(string thumbprint)
        {
            return false;
        }

        private CertificateInfo GetCertificateDetails(X509Certificate2 cert, string storeName, string storeLocation)
        {
            try
            {
                // Calculate days until expiration
                var now = DateTime.UtcNow;
                var daysLeft = (cert.NotAfter - now).Days;

                // Determine certificate status
                string status;
                if (now > cert.NotAfter)
                {
                    status = "Expired";
                }
                else if (daysLeft < 30)
                {
                    status = "Warning";
                }
                else
                {
                    status = "Valid";
                }

                // Get friendly name or subject if friendly name is empty
                var name = !string.IsNullOrEmpty(cert.FriendlyName)
                    ? cert.FriendlyName
                    : cert.Subject.Split(',').FirstOrDefault()?.Replace("CN=", "") ?? "Unknown";

                return new CertificateInfo
                {
                    Name = name,
                    StoreName = storeName,
                    StoreLocation = storeLocation,
                    Subject = cert.Subject,
                    Issuer = cert.Issuer,
                    SerialNumber = cert.SerialNumber,
                    ValidFrom = cert.NotBefore.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss UTC"),
                    ValidUntil = cert.NotAfter.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss UTC"),
                    Status = status,
                    DaysLeft = daysLeft,
                    Thumbprint = cert.Thumbprint,
                    HasPrivateKey = cert.HasPrivateKey
                };
            }
            catch (Exception ex)
            {
                return new CertificateInfo
                {
                    Name = cert.Subject.Split(',').FirstOrDefault()?.Replace("CN=", "") ?? "Unknown Certificate",
                    StoreName = storeName,
                    StoreLocation = storeLocation,
                    Error = $"Error processing certificate: {ex.Message}"
                };
            }
        }
    }
}