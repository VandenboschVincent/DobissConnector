using Moq;
using Microsoft.AspNetCore.Mvc;
using DobissConnectorService.App.Controllers;
using DobissConnectorService.Dobiss.Models;
using Shouldly;
using DobissConnectorService.Dobiss.Interfaces;
using DobissConnectorService.Tests.Base;

namespace DobissConnectorService.Tests
{
    public class DobissControlerTests : BaseTestClass
    {
        [Test]
        public async Task GetAll_Should_ReturnAllLights()
        {
            // Arrange
            List<Light> lights = [ new(1, 1, ModuleType.RELAY, "Test", LightType.Light) ];
            Mocker.GetMock<ILightCacheService>()
                .Setup(s => s.GetAll())
                .ReturnsAsync(lights);
            LightController controller = Mocker.CreateInstance<LightController>();

            // Act
            ActionResult<IEnumerable<Light>> result = await controller.GetAll();

            // Assert
            OkObjectResult okResult = result.Result.ShouldBeOfType<OkObjectResult>();
            okResult.Value.ShouldBe(lights);
        }
    }
}
