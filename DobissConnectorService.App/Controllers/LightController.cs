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
        [HttpGet]
        public ActionResult<IEnumerable<Light>> GetAll()
        {
            logger.LogDebug("Performing Get");
            var lights = lightCacheService.GetAll();
            return Ok(lights);
        }

        [HttpGet("{module}/{key}")]
        public ActionResult<Light> Get(int module, int key)
        {
            logger.LogDebug("Performing Get for {Module} and {Key}", module, key);
            var light = lightCacheService.Get(module, key);
            if (light == null)
                return NotFound($"No light found with key {key} and module {module}");
            return Ok(light);
        }

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

        [HttpPost("{module}/{key}/set/{value}")]
        public async Task<IActionResult> Set(int module, int key, int value)
        {
            logger.LogDebug("Performing Set for {Module} and {Key} to {Value}", module, key, value);
            var light = lightCacheService.Get(module, key);
            if (light == null)
                return NotFound($"No light found with key {key} and module {module}");

            await mediator.Send(new ToggleLightMessage(light, value));

            return NoContent();
        }
    }
}
