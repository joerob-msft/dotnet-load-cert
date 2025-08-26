using AzureCertInventory.Models;
using AzureCertInventory.Services;
using Microsoft.AspNetCore.Mvc;

namespace AzureCertInventory.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class CertificatesController : ControllerBase
    {
        private readonly ICertificateService _certificateService;
        private readonly ILogger<CertificatesController> _logger;

        public CertificatesController(ICertificateService certificateService, ILogger<CertificatesController> logger)
        {
            _certificateService = certificateService;
            _logger = logger;
        }

        /// <summary>
        /// Get private certificates
        /// </summary>
        /// <returns>List of private certificates from the current user's certificate store</returns>
        /// <response code="200">Returns the list of private certificates</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpGet("private")]
        [ProducesResponseType(typeof(IEnumerable<CertificateInfo>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public ActionResult<IEnumerable<CertificateInfo>> GetPrivateCertificates()
        {
            try
            {
                _logger.LogInformation("Retrieving private certificates");
                var certificates = _certificateService.GetPrivateCertificates();
                return Ok(certificates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving private certificates");
                return StatusCode(500, new ErrorResponse 
                { 
                    Error = "Internal server error", 
                    Details = ex.Message 
                });
            }
        }

        /// <summary>
        /// Get public certificates
        /// </summary>
        /// <returns>List of public certificates (currently returns empty list)</returns>
        /// <response code="200">Returns the list of public certificates</response>
        [HttpGet("public")]
        [ProducesResponseType(typeof(IEnumerable<CertificateInfo>), StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<CertificateInfo>> GetPublicCertificates()
        {
            try
            {
                _logger.LogInformation("Retrieving public certificates");
                var certificates = _certificateService.GetPublicCertificates();
                return Ok(certificates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving public certificates");
                return StatusCode(500, new ErrorResponse 
                { 
                    Error = "Internal server error", 
                    Details = ex.Message 
                });
            }
        }
    }
}
