using DobissConnectorService.Dobiss;
using DobissConnectorService.Dobiss.Interfaces;
using DobissConnectorService.Dobiss.Models;
using Moq;
using Moq.AutoMock;

[assembly: Retry(3)]
[assembly: System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]

namespace DobissConnectorService.Tests.Base;

public class GlobalHooks
{

    private static AutoMocker mocker = new();
    public static AutoMocker Mocker => mocker;

    [Before(TestSession)]
    public static void SetUp()
    {
        // This runs before all tests
    }

    [After(TestSession)]
    public static void CleanUp()
    {
        // This runs after all tests
    }

    [Before(Test)]
    public void BeforeEachTest()
    {
        mocker = new AutoMocker();
    }
}