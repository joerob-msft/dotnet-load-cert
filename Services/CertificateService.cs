using AzureCertInventory.Models;
using System.Security.Cryptography.X509Certificates;

namespace AzureCertInventory.Services
{
    public class CertificateService : ICertificateService
    {
        private readonly ILogger<CertificateService> _logger;

        public CertificateService(ILogger<CertificateService> logger)
        {
            _logger = logger;
        }

        // Return empty list for public certificates as requested
        public IEnumerable<CertificateInfo> GetPublicCertificates()
        {
            return new List<CertificateInfo>();
        }

        public IEnumerable<CertificateInfo> GetPrivateCertificates()
        {
            var certificates = new List<CertificateInfo>();

            try
            {
                // Get certificates ONLY from the My store (personal certificates)
                using var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
                store.Open(OpenFlags.ReadOnly);

                foreach (var cert in store.Certificates)
                {
                    certificates.Add(GetCertificateDetails(cert, StoreName.My.ToString(), StoreLocation.LocalMachine.ToString()));
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