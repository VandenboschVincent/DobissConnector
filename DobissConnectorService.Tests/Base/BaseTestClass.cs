using DobissConnectorService.Dobiss.Interfaces;
using DobissConnectorService.Dobiss;
using Microsoft.Extensions.Caching.Memory;
using Moq.AutoMock;
using Moq;

namespace DobissConnectorService.Tests.Base
{
    public class BaseTestClass
    {
        protected static AutoMocker Mocker => GlobalHooks.Mocker;

        private DobissService? _dobissService;
        public DobissService DobissService => _dobissService ??= new(Mocker.Get<IDobissClient>(), Mocker.Get<ILightCacheService>());

    }
}
