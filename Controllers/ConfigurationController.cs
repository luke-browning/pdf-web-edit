using Microsoft.AspNetCore.Mvc;
using PDFWebEdit.Helpers;
using PDFWebEdit.Models.Config;
using PDFWebEdit.Services;

namespace PDFWebEdit.Controllers
{
    /// <summary>
    /// A controller for handling configurations.
    /// </summary>
    /// <seealso cref="ControllerBase"/>
    [Route("api/configuration")]
    [ApiController]
    public class ConfigurationController : ControllerBase
    {
        /// <summary>
        /// The configuration service.
        /// </summary>
        private readonly ConfigService _configService;

        /// <summary>
        /// Initialises a new instance of the
        /// <see cref="PDFWebEdit.Controllers.ConfigurationController"/> class.
        /// </summary>
        /// <param name="configService">The configuration service.</param>
        public ConfigurationController(ConfigService configService)
        {
            _configService = configService;
        }

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <returns>
        /// The configuration.
        /// </returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Config))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
        [ProducesErrorResponseType(typeof(ObjectResult))]
        public IActionResult GetConfiguration()
        {
            try
            {
                return Ok(_configService.Settings);
            }
            catch (Exception x)
            {
                return ExceptionHelpers.GetErrorObjectResult("GetConfiguration", HttpContext, x);
            }
        }

        /// <summary>
        /// Saves a configuration.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <returns>
        /// The updated config
        /// </returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Config))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
        [ProducesErrorResponseType(typeof(ObjectResult))]
        public IActionResult SaveConfiguration([FromBody] Config config)
        {
            try
            {
                _configService.UpdateConfiguration(config);

                return Ok(_configService.Settings);
            }
            catch (Exception x)
            {
                return ExceptionHelpers.GetErrorObjectResult("UpdateConfiguration", HttpContext, x);
            }
        }

        /// <summary>
        /// Reload configuration.
        /// </summary>
        /// <returns>
        /// A status code indicating success or failure.
        /// </returns>
        [HttpPatch]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Config))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
        [ProducesErrorResponseType(typeof(ObjectResult))]
        public IActionResult ReloadConfiguration()
        {
            try
            {
                _configService.ReloadConfiguration();

                return Ok(_configService.Settings);
            }
            catch (Exception x)
            {
                return ExceptionHelpers.GetErrorObjectResult("GetConfiguration", HttpContext, x);
            }
        }
    }
}
