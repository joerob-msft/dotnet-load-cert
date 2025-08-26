using AzureCertInventory.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace AzureCertInventory.Controllers
{
    [ApiController]
    [Route("api/system")]
    [Produces("application/json")]
    public class SystemController : ControllerBase
    {
        private readonly ILogger<SystemController> _logger;

        public SystemController(ILogger<SystemController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Get system information
        /// </summary>
        /// <returns>System information including hostname, certificate environment variables, and app service plan details</returns>
        /// <response code="200">Returns the system information</response>
        [HttpGet("info")]
        [ProducesResponseType(typeof(SystemInfo), StatusCodes.Status200OK)]
        public ActionResult<SystemInfo> GetSystemInfo()
        {
            try
            {
                _logger.LogInformation("Retrieving system information");

                var systemInfo = new SystemInfo();

                // Get hostname
                try
                {
                    systemInfo.Hostname = Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME")
                        ?? Dns.GetHostName();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to get hostname");
                    systemInfo.Hostname = "Unknown";
                }

                // Get certificate environment variable
                systemInfo.CertificateEnvironmentVariable = Environment.GetEnvironmentVariable("WEBSITE_LOAD_CERTIFICATES") ?? "Not set";

                // Get app service plan info
                try
                {
                    systemInfo.AppServicePlan = Environment.GetEnvironmentVariable("WEBSITE_SKU") ?? "Unknown";
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to get app service plan information");
                    systemInfo.AppServicePlan = "Unknown";
                }

                return Ok(systemInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving system information");
                return StatusCode(500, new ErrorResponse 
                { 
                    Error = "Internal server error", 
                    Details = ex.Message 
                });
            }
        }
    }
}
