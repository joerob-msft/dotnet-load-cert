using AzureCertInventory.Models;
using AzureCertInventory.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;
using System.Net;

namespace AzureCertInventory.Pages
{
    /// <summary>
    /// Page model for the main index page displaying certificate information
    /// </summary>
    public class IndexModel : PageModel
    {
        private readonly ICertificateService _certificateService;

        /// <summary>
        /// Initializes a new instance of the IndexModel
        /// </summary>
        /// <param name="certificateService">The certificate service</param>
        public IndexModel(ICertificateService certificateService)
        {
            _certificateService = certificateService;
        }

        /// <summary>
        /// Gets the collection of private certificates
        /// </summary>
        public IEnumerable<CertificateInfo> PrivateCertificates { get; private set; } = Enumerable.Empty<CertificateInfo>();
        
        /// <summary>
        /// Gets the current hostname
        /// </summary>
        public string Hostname { get; private set; } = "Unknown";
        
        /// <summary>
        /// Gets the certificate environment variable value
        /// </summary>
        public string CertificateEnvironmentVariable { get; private set; } = "Not set";
        
        /// <summary>
        /// Gets the App Service plan information
        /// </summary>
        public string AppServicePlan { get; private set; } = "Unknown";

        /// <summary>
        /// Handles GET requests to load certificate and system information
        /// </summary>
        public void OnGet()
        {
            PrivateCertificates = _certificateService.GetPrivateCertificates();

            // Get hostname
            try
            {
                Hostname = Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME")
                    ?? Dns.GetHostName();
            }
            catch
            {
                // Ignore errors, keep default value
            }

            // Get certificate environment variable
            CertificateEnvironmentVariable = Environment.GetEnvironmentVariable("WEBSITE_LOAD_CERTIFICATES") ?? "Not set";

            // Get app service plan info
            try
            {
                AppServicePlan = Environment.GetEnvironmentVariable("WEBSITE_SKU") ?? "Unknown";
            }
            catch
            {
                // Ignore errors, keep default value
            }
        }
    }
}