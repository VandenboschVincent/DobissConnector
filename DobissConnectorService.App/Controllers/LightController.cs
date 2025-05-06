using DobissConnectorService.Dobiss.Models;
using DobissConnectorService.Handlers.Messages;
using Mediator;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SlimMessageBus;
using System.Threading;

namespace DobissConnectorService.App.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LightController(ILogger<LightController> logger, LightCacheService lightCacheService, IMediator mediator) : ControllerBase
    {
        /// <summary>
        /// Get all lights
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult<IEnumerable<Light>> GetAll()
        {
            logger.LogDebug("Performing Get");
            var lights = lightCacheService.GetAll();
            return Ok(lights);
        }

        /// <summary>
        /// Get a specific light
        /// </summary>
        /// <param name="module"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        [HttpGet("{module}/{key}")]
        public ActionResult<Light> Get(int module, int key)
        {
            logger.LogDebug("Performing Get for {Module} and {Key}", module, key);
            var light = lightCacheService.Get(module, key);
            if (light == null)
                return NotFound($"No light found with key {key} and module {module}");
            return Ok(light);
        }

        /// <summary>
        /// Toggle the light on or off
        /// </summary>
        /// <param name="module"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        [HttpPost("{module}/{key}/toggle")]
        public async Task<IActionResult> Toggle(int module, int key)
        {
            logger.LogDebug("Performing Toggle for {Module} and {Key}", module, key);
            var light = lightCacheService.Get(module, key);
            if (light == null)
                return NotFound($"No light found with key {key} and module {module}");

            await mediator.Send(new ToggleLightMessage(light, null));

            return NoContent();
        }

        /// <summary>
        /// Set the light to a specific value
        /// </summary>
        /// <param name="module"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        [HttpPost("{module}/{key}/set/{value}")]
        public async Task<IActionResult> Set(int module, int key, bool value)
        {
            logger.LogDebug("Performing Set for {Module} and {Key} to {Value}", module, key, value);
            var light = lightCacheService.Get(module, key);
            if (light == null)
                return NotFound($"No light found with key {key} and module {module}");

            await mediator.Send(new ToggleLightMessage(light, Convert.ToInt32(value) * 100));

            return NoContent();
        }

        /// <summary>
        /// Set the light to a specific value, 0-100
        /// </summary>
        /// <param name="module"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        [HttpPost("{module}/{key}/dim/{value}")]
        public async Task<IActionResult> Dim(int module, int key, int value)
        {
            logger.LogDebug("Performing Set for {Module} and {Key} to {Value}", module, key, value);
            var light = lightCacheService.Get(module, key);
            if (light == null)
                return NotFound($"No light found with key {key} and module {module}");
            if (value < 0 || value > 100)
                return BadRequest($"Value must be between 0 and 100");

            await mediator.Send(new DimLightMessage(light, value));

            return NoContent();
        }
    }
}
