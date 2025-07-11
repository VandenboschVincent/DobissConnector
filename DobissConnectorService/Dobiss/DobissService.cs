using DobissConnectorService.Dobiss.Models;
using DobissConnectorService.Dobiss.Interfaces;
using System.Runtime.CompilerServices;

namespace DobissConnectorService.Dobiss
{
    public class DobissService(IDobissClient dobissClient, ILightCacheService lightCacheService)
    {
        public IDobissClient DobissClient => dobissClient;

        public async Task ToggleOutput(int module, int address, CancellationToken cancellationToken = default)
        {
            DobissSendActionRequest request = new(dobissClient, module, address, 100);
            await request.Execute(cancellationToken);
        }

        public async Task DimOutput(int module, int address, int value, CancellationToken cancellationToken = default)
        {
            DobissSendActionRequest request = new(dobissClient
                , module
                , address
                , value
                , value == 0 ? DobissSendActionRequest.ActionType.OFF : DobissSendActionRequest.ActionType.ON);
            await request.Execute(cancellationToken);
        }

        public async IAsyncEnumerable<(int moduleIndex, int index, int value)> RequestAllStatus(List<DobissModule> modules, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            foreach(var module in modules)
            {
                var outputs = await RequestStatus(module, cancellationToken);
                if (outputs.Count != 0)
                {
                    foreach (var (index, value) in outputs)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        yield return (module.Index, index, value);
                    }
                }
            }
        }

        public async Task<List<(int index, int value)>> RequestStatus(DobissModule module, CancellationToken cancellationToken = default)
        {
            var lights = await lightCacheService.GetAll();
            return await new DobissRequestStatusRequest(dobissClient, module.Type, module.Index, lights.Count(t => t.ModuleKey == module.Index))
                .Execute(cancellationToken);
        }

        public async Task<List<DobissGroupData>> FetchGroupsData(CancellationToken cancellationToken = default)
        {
            return await new DobissFetchGroupsRequest(dobissClient).Execute(cancellationToken);
        }

        public async Task<List<DobissModule>> FetchModules(CancellationToken cancellationToken = default)
        {
            return await new DobissFetchModulesRequest(dobissClient).Execute(cancellationToken);
        }

        public async Task<List<DobissGroupData>> FetchMoodsData(CancellationToken cancellationToken = default)
        {
            return await new DobissFetchMoodsRequest(dobissClient).Execute(cancellationToken);
        }

        public async Task<List<DobissOutput>> FetchOutputsData(DobissModule module, CancellationToken cancellationToken = default)
        {
            // Use known module type
            return await new DobissFetchOutputsRequest(dobissClient, module).Execute(cancellationToken);
        }
    }
}
