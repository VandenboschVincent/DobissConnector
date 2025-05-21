using DobissConnectorService.Dobiss;
using DobissConnectorService.Dobiss.Interfaces;
using DobissConnectorService.Dobiss.Utils;
using Moq;

namespace DobissConnectorService.Tests;

public class Tests
{
    [Test]
    public async Task DobissFetchGroupsRequest_Tests()
    {
        //TODO
        await Task.CompletedTask;
    }

    [Test]
    public async Task DobissFetchMoodsRequest_Tests()
    {
        //TODO
        await Task.CompletedTask;
    }

    [Test]
    //[Arguments(1, 2, 3)]
    public async Task DobissFetchOutputsRequest_Tests()
    {
        //TODO
        await Task.CompletedTask;
    }

    [Test]
    public async Task DobissRequestStatusRequest_Tests()
    {
        //TODO
        await Task.CompletedTask;
    }

    [Test]
    [Arguments(1, 1, 100, "")]
    public async Task DobissSendActionRequest_Tests(int module, int address, int value, string hex)
    {
        byte[] inputByte = Convert.FromHexString(hex);
        GlobalHooks.dobissClientMock.Setup(x => x.SendRequest(inputByte, 2, It.IsAny<CancellationToken>()))
            .ReturnsAsync([0x00, 0x01, 0x02, 0x03])
            .Verifiable();
        await GlobalHooks.dobissService.DimOutput(module, address, value);
        GlobalHooks.dobissClientMock.Verify();
    }
}