using AzureCertInventory.Models;
using AzureCertInventory.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;
using System.Net;

namespace AzureCertInventory.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ICertificateService _certificateService;

        public IndexModel(ICertificateService certificateService)
        {
            _certificateService = certificateService;
        }

        public IEnumerable<CertificateInfo> PublicCertificates { get; private set; } = Enumerable.Empty<CertificateInfo>();
        public IEnumerable<CertificateInfo> PrivateCertificates { get; private set; } = Enumerable.Empty<CertificateInfo>();
        public string CurrentTime { get; private set; } = "2025-04-24 19:12:03"; // Updated time
        public string CurrentUser { get; private set; } = "joerob-msft"; // Using provided username
        public string Hostname { get; private set; } = "Unknown";
        public string CertificateEnvironmentVariable { get; private set; } = "Not set";
        public string AppServicePlan { get; private set; } = "Unknown";

        public void OnGet()
        {
            PublicCertificates = _certificateService.GetPublicCertificates();
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