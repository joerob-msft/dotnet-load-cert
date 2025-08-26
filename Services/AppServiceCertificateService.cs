using AzureCertInventory.Models;
using System.Collections.Concurrent;
using System.Security.Cryptography.X509Certificates;
using System.Net.Http;

namespace AzureCertInventory.Services
{
    /// <summary>
    /// App Service optimized certificate service that works with in-memory certificates
    /// </summary>
    public class AppServiceCertificateService : ICertificateService
    {
        private readonly ILogger<AppServiceCertificateService> _logger;
        private readonly ConcurrentDictionary<string, X509Certificate2> _loadedCertificates = new();
        private readonly ICertificateService _baseService;

        /// <summary>
        /// Initializes a new instance of the AppServiceCertificateService
        /// </summary>
        /// <param name="logger">The logger instance</param>
        /// <param name="baseService">The base certificate service</param>
        public AppServiceCertificateService(ILogger<AppServiceCertificateService> logger, ICertificateService baseService)
        {
            _logger = logger;
            _baseService = baseService;
        }

        /// <summary>
        /// Gets all public certificates
        /// </summary>
        /// <returns>Collection of public certificate information</returns>
        public IEnumerable<CertificateInfo> GetPublicCertificates()
        {
            return _baseService.GetPublicCertificates();
        }

        /// <summary>
        /// Gets all private certificates from the current user's certificate store
        /// </summary>
        /// <returns>Collection of private certificate information</returns>
        public IEnumerable<CertificateInfo> GetPrivateCertificates()
        {
            return _baseService.GetPrivateCertificates();
        }

        /// <summary>
        /// Gets certificates loaded via App Service (WEBSITE_LOAD_CERTIFICATES)
        /// </summary>
        /// <returns>Collection of App Service loaded certificates</returns>
        public IEnumerable<CertificateInfo> GetAppServiceCertificates()
        {
            var certificates = new List<CertificateInfo>();

            try
            {
                var loadCertificates = Environment.GetEnvironmentVariable("WEBSITE_LOAD_CERTIFICATES");
                
                if (string.IsNullOrEmpty(loadCertificates))
                {
                    _logger.LogInformation("WEBSITE_LOAD_CERTIFICATES environment variable not set");
                    return certificates;
                }

                _logger.LogInformation("WEBSITE_LOAD_CERTIFICATES is set to: {LoadCertificates}", loadCertificates);

                // In App Service, certificates are automatically loaded into the certificate store
                // when WEBSITE_LOAD_CERTIFICATES is set
                using var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
                store.Open(OpenFlags.ReadOnly);

                foreach (var cert in store.Certificates)
                {
                    certificates.Add(GetCertificateDetails(cert, "AppService", "CurrentUser"));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving App Service certificates");
                certificates.Add(new CertificateInfo
                {
                    Name = "Error",
                    Error = $"Failed to access App Service certificates: {ex.Message}"
                });
            }

            return certificates;
        }

        /// <summary>
        /// Loads a certificate into application memory (App Service compatible)
        /// </summary>
        /// <param name="certificateData">Base64 encoded certificate data</param>
        /// <param name="password">Certificate password (optional)</param>
        /// <param name="friendlyName">Friendly name for the certificate</param>
        /// <returns>Certificate information if successful</returns>
        public Task<CertificateInfo> LoadCertificateAsync(string certificateData, string? password = null, string? friendlyName = null)
        {
            try
            {
                _logger.LogInformation("Loading certificate into memory");

                var certBytes = Convert.FromBase64String(certificateData);
                var cert = string.IsNullOrEmpty(password) 
                    ? new X509Certificate2(certBytes) 
                    : new X509Certificate2(certBytes, password);

                // Store in memory
                _loadedCertificates.TryAdd(cert.Thumbprint, cert);

                var certInfo = GetCertificateDetails(cert, "Memory", "Application");
                
                if (!string.IsNullOrEmpty(friendlyName))
                {
                    certInfo.Name = friendlyName;
                }

                _logger.LogInformation("Certificate loaded successfully: {Thumbprint}", cert.Thumbprint);
                return Task.FromResult(certInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load certificate");
                return Task.FromResult(new CertificateInfo
                {
                    Name = "Error",
                    Error = $"Failed to load certificate: {ex.Message}",
                    Status = "Error"
                });
            }
        }

        /// <summary>
        /// Validates a certificate by thumbprint
        /// </summary>
        /// <param name="thumbprint">Certificate thumbprint</param>
        /// <returns>Validation result</returns>
        public Task<CertificateValidationResult> ValidateCertificateAsync(string thumbprint)
        {
            try
            {
                _logger.LogInformation("Validating certificate: {Thumbprint}", thumbprint);

                // Try to find certificate in loaded certificates first
                if (!_loadedCertificates.TryGetValue(thumbprint, out var cert))
                {
                    // Try to find in system stores
                    cert = FindCertificateInStores(thumbprint);
                }

                if (cert == null)
                {
                    return Task.FromResult(new CertificateValidationResult
                    {
                        IsValid = false,
                        Message = "Certificate not found",
                        ChainValid = false,
                        ExpirationStatus = "Unknown"
                    });
                }

                var result = new CertificateValidationResult();

                // Check expiration
                var now = DateTime.UtcNow;
                if (now < cert.NotBefore)
                {
                    result.ExpirationStatus = "Not yet valid";
                    result.IsValid = false;
                }
                else if (now > cert.NotAfter)
                {
                    result.ExpirationStatus = "Expired";
                    result.IsValid = false;
                }
                else
                {
                    var daysLeft = (cert.NotAfter - now).Days;
                    result.ExpirationStatus = daysLeft < 30 ? $"Expires in {daysLeft} days" : "Valid";
                    result.IsValid = true;
                }

                // Validate chain
                try
                {
                    using var chain = new X509Chain();
                    chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck; // App Service friendly
                    result.ChainValid = chain.Build(cert);
                    
                    if (!result.ChainValid)
                    {
                        result.Message = string.Join("; ", chain.ChainStatus.Select(s => s.StatusInformation));
                    }
                }
                catch (Exception ex)
                {
                    result.ChainValid = false;
                    result.Message = $"Chain validation failed: {ex.Message}";
                }

                if (result.IsValid && result.ChainValid)
                {
                    result.Message = "Certificate is valid";
                }

                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating certificate");
                return Task.FromResult(new CertificateValidationResult
                {
                    IsValid = false,
                    Message = $"Validation error: {ex.Message}",
                    ChainValid = false,
                    ExpirationStatus = "Error"
                });
            }
        }

        /// <summary>
        /// Gets certificates currently loaded in application memory
        /// </summary>
        /// <returns>Collection of in-memory certificates</returns>
        public IEnumerable<CertificateInfo> GetLoadedCertificates()
        {
            try
            {
                return _loadedCertificates.Values.Select(cert => GetCertificateDetails(cert, "Memory", "Application"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving loaded certificates");
                return new List<CertificateInfo>
                {
                    new CertificateInfo
                    {
                        Name = "Error",
                        Error = $"Failed to retrieve loaded certificates: {ex.Message}",
                        Status = "Error"
                    }
                };
            }
        }

        /// <summary>
        /// Removes a certificate from application memory
        /// </summary>
        /// <param name="thumbprint">Certificate thumbprint</param>
        /// <returns>True if removed successfully</returns>
        public bool RemoveLoadedCertificate(string thumbprint)
        {
            try
            {
                var removed = _loadedCertificates.TryRemove(thumbprint, out var cert);
                if (removed)
                {
                    cert?.Dispose();
                    _logger.LogInformation("Certificate removed from memory: {Thumbprint}", thumbprint);
                }
                return removed;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing certificate from memory");
                return false;
            }
        }

        /// <summary>
        /// Finds a certificate in system stores by thumbprint
        /// </summary>
        /// <param name="thumbprint">Certificate thumbprint</param>
        /// <returns>Certificate if found, null otherwise</returns>
        private X509Certificate2? FindCertificateInStores(string thumbprint)
        {
            var storeLocations = new[] { StoreLocation.CurrentUser, StoreLocation.LocalMachine };
            var storeNames = new[] { StoreName.My, StoreName.Root, StoreName.CertificateAuthority };

            foreach (var location in storeLocations)
            {
                foreach (var storeName in storeNames)
                {
                    try
                    {
                        using var store = new X509Store(storeName, location);
                        store.Open(OpenFlags.ReadOnly);
                        
                        var cert = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false)
                            .Cast<X509Certificate2>()
                            .FirstOrDefault();
                            
                        if (cert != null)
                        {
                            return cert;
                        }
                    }
                    catch
                    {
                        // Ignore access errors and continue searching
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets detailed information about a certificate
        /// </summary>
        /// <param name="cert">The certificate</param>
        /// <param name="storeName">Store name</param>
        /// <param name="storeLocation">Store location</param>
        /// <returns>Certificate information</returns>
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
                    Error = $"Error processing certificate: {ex.Message}",
                    Status = "Error"
                };
            }
        }
    }
}
