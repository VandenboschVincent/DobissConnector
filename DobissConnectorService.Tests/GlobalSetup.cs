// Here you could define global logic that would affect all tests
// You can use attributes at the assembly level to apply to all tests in the assembly
using DobissConnectorService.Dobiss.Interfaces;
using DobissConnectorService.Dobiss.Models;
using DobissConnectorService.Services;
using Microsoft.Extensions.Caching.Memory;
using Moq;

[assembly: Retry(3)]
[assembly: System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]

namespace DobissConnectorService.Tests;

public class GlobalHooks
{
    public static Mock<IDobissClient> dobissClientMock = new();
    public static Mock<IMemoryCache> memoryCacheMock = new();
    public static DobissService dobissService;
    protected Dictionary<int, ModuleType> moduleTypeMap = new()
    {
        { 1, ModuleType.DIMMER },
        { 2, ModuleType.RELAY }
    };

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
        dobissClientMock.Reset();
        memoryCacheMock.Reset();
        dobissService = new DobissService(dobissClientMock.Object, moduleTypeMap, new LightCacheService(memoryCacheMock.Object));
    }
}