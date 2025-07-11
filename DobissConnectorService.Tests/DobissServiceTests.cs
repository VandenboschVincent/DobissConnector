using DobissConnectorService.Dobiss.Interfaces;
using DobissConnectorService.Tests.Base;
using Moq;
using Shouldly;

namespace DobissConnectorService.Tests;

public class DobissServiceTests : BaseTestClass
{
    [Test]
    [Arguments(1, 1, 100, "AF02FF010000080108FFFFFFFFFFFFAF")]
    [Arguments(1, 1, 100, "010101FFFF64FFFF")]
    public async Task SendAction_Should_TranslateToHex(int module, int address, int value, string hex)
    {
        byte[] inputByte = Convert.FromHexString(hex);
        Mocker.GetMock<IDobissClient>().Setup(x => x.SendRequest(inputByte, 0, It.IsAny<CancellationToken>()))
            .ReturnsAsync([0x00])
            .Verifiable();
        Func<Task> act = () => DobissService.DimOutput(module, address, value);
        await act.ShouldNotThrowAsync();
        Mocker.Verify();
    }
}