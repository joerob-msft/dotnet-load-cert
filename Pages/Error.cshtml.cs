using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;

namespace AzureCertInventory.Pages
{
    /// <summary>
    /// Page model for error handling and display
    /// </summary>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [IgnoreAntiforgeryToken]
    public class ErrorModel : PageModel
    {
        /// <summary>
        /// Gets or sets the request ID for tracking errors
        /// </summary>
        public string? RequestId { get; set; }

        /// <summary>
        /// Gets a value indicating whether the request ID should be displayed
        /// </summary>
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        private readonly ILogger<ErrorModel> _logger;

        /// <summary>
        /// Initializes a new instance of the ErrorModel
        /// </summary>
        /// <param name="logger">The logger instance</param>
        public ErrorModel(ILogger<ErrorModel> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Handles GET requests to the error page
        /// </summary>
        public void OnGet()
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        }
    }
}