using AzureCertInventory.Models;
using AzureCertInventory.Services;
using Microsoft.AspNetCore.Mvc;

namespace AzureCertInventory.Controllers
{
    /// <summary>
    /// Controller for managing certificate operations
    /// </summary>
    [ApiController]
    [Route("api/certinventory/certificates")]
    [Produces("application/json")]
    public class CertificatesController : ControllerBase
    {
        private readonly ICertificateService _certificateService;
        private readonly ILogger<CertificatesController> _logger;

        /// <summary>
        /// Initializes a new instance of the CertificatesController
        /// </summary>
        /// <param name="certificateService">The certificate service</param>
        /// <param name="logger">The logger instance</param>
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

        /// <summary>
        /// Get App Service certificates (loaded via WEBSITE_LOAD_CERTIFICATES)
        /// </summary>
        /// <returns>List of certificates loaded by App Service</returns>
        /// <response code="200">Returns the list of App Service certificates</response>
        [HttpGet("appservice")]
        [ProducesResponseType(typeof(IEnumerable<CertificateInfo>), StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<CertificateInfo>> GetAppServiceCertificates()
        {
            try
            {
                _logger.LogInformation("Retrieving App Service certificates");
                var certificates = _certificateService.GetAppServiceCertificates();
                return Ok(certificates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving App Service certificates");
                return StatusCode(500, new ErrorResponse 
                { 
                    Error = "Internal server error", 
                    Details = ex.Message 
                });
            }
        }

        /// <summary>
        /// Get certificates loaded in application memory
        /// </summary>
        /// <returns>List of certificates currently loaded in memory</returns>
        /// <response code="200">Returns the list of loaded certificates</response>
        [HttpGet("loaded")]
        [ProducesResponseType(typeof(IEnumerable<CertificateInfo>), StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<CertificateInfo>> GetLoadedCertificates()
        {
            try
            {
                _logger.LogInformation("Retrieving loaded certificates");
                var certificates = _certificateService.GetLoadedCertificates();
                return Ok(certificates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving loaded certificates");
                return StatusCode(500, new ErrorResponse 
                { 
                    Error = "Internal server error", 
                    Details = ex.Message 
                });
            }
        }

        /// <summary>
        /// Load a certificate into application memory (App Service compatible)
        /// </summary>
        /// <param name="request">Certificate import request</param>
        /// <returns>Certificate information if successful</returns>
        /// <response code="200">Certificate loaded successfully</response>
        /// <response code="400">Invalid certificate data or format</response>
        [HttpPost("load")]
        [ProducesResponseType(typeof(CertificateInfo), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CertificateInfo>> LoadCertificate([FromBody] CertificateImportRequest request)
        {
            try
            {
                _logger.LogInformation("Loading certificate into memory");
                
                if (string.IsNullOrEmpty(request.CertificateData))
                {
                    return BadRequest(new ErrorResponse 
                    { 
                        Error = "Certificate data is required" 
                    });
                }

                var certificateInfo = await _certificateService.LoadCertificateAsync(
                    request.CertificateData, 
                    request.Password, 
                    request.FriendlyName);

                if (!string.IsNullOrEmpty(certificateInfo.Error))
                {
                    return BadRequest(new ErrorResponse 
                    { 
                        Error = "Failed to load certificate", 
                        Details = certificateInfo.Error 
                    });
                }

                return Ok(certificateInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading certificate");
                return StatusCode(500, new ErrorResponse 
                { 
                    Error = "Internal server error", 
                    Details = ex.Message 
                });
            }
        }

        /// <summary>
        /// Validate a certificate by thumbprint
        /// </summary>
        /// <param name="request">Certificate validation request</param>
        /// <returns>Validation result</returns>
        /// <response code="200">Validation completed</response>
        /// <response code="400">Invalid thumbprint</response>
        [HttpPost("validate")]
        [ProducesResponseType(typeof(CertificateValidationResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CertificateValidationResult>> ValidateCertificate([FromBody] CertificateValidationRequest request)
        {
            try
            {
                _logger.LogInformation("Validating certificate: {Thumbprint}", request.Thumbprint);
                
                if (string.IsNullOrEmpty(request.Thumbprint))
                {
                    return BadRequest(new ErrorResponse 
                    { 
                        Error = "Certificate thumbprint is required" 
                    });
                }

                var result = await _certificateService.ValidateCertificateAsync(request.Thumbprint);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating certificate");
                return StatusCode(500, new ErrorResponse 
                { 
                    Error = "Internal server error", 
                    Details = ex.Message 
                });
            }
        }

        /// <summary>
        /// Remove a certificate from application memory
        /// </summary>
        /// <param name="thumbprint">Certificate thumbprint</param>
        /// <returns>Operation result</returns>
        /// <response code="200">Certificate removed successfully</response>
        /// <response code="404">Certificate not found</response>
        [HttpDelete("loaded/{thumbprint}")]
        [ProducesResponseType(typeof(CertificateOperationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public ActionResult<CertificateOperationResponse> RemoveLoadedCertificate(string thumbprint)
        {
            try
            {
                _logger.LogInformation("Removing certificate from memory: {Thumbprint}", thumbprint);
                
                if (string.IsNullOrEmpty(thumbprint))
                {
                    return BadRequest(new ErrorResponse 
                    { 
                        Error = "Certificate thumbprint is required" 
                    });
                }

                var removed = _certificateService.RemoveLoadedCertificate(thumbprint);
                
                if (removed)
                {
                    return Ok(new CertificateOperationResponse 
                    { 
                        Success = true, 
                        Message = "Certificate removed successfully",
                        Thumbprint = thumbprint
                    });
                }
                else
                {
                    return NotFound(new ErrorResponse 
                    { 
                        Error = "Certificate not found or could not be removed" 
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing certificate");
                return StatusCode(500, new ErrorResponse 
                { 
                    Error = "Internal server error", 
                    Details = ex.Message 
                });
            }
        }
    }
}
