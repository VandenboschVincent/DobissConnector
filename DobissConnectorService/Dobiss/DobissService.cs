using DobissConnectorService.Dobiss.Models;
using DobissConnectorService.Dobiss;
using DobissConnectorService.Dobiss.Interfaces;

namespace DobissConnectorService.Services
{
    public class DobissService(IDobissClient dobissClient, Dictionary<int, ModuleType> moduleTypeMap, LightCacheService lightCacheService)
    {
        private const int HEX_OUTPUT_STATUS_LENGTH = 2;

        public async Task ToggleOutput(int module, int address, CancellationToken cancellationToken = default)
        {
            DobissSendActionRequest request = new(dobissClient, module, address);
            await request.Execute(cancellationToken);
        }

        public async Task DimOutput(int module, int address, int value, CancellationToken cancellationToken = default)
        {
            DobissSendActionRequest request = new(dobissClient
                , module
                , address
                , value == 0 ? DobissSendActionRequest.ActionType.TOGGLE : DobissSendActionRequest.ActionType.ON
                , value == 0 ? null : value);
            await request.Execute(cancellationToken);
        }

        public async Task<string> RequestModuleStatusAsHex(int module, CancellationToken cancellationToken = default)
        {
            return await RequestStatusHex(module, cancellationToken);
        }

        public async Task<string> RequestOutputStatusAsHex(int module, int address, CancellationToken cancellationToken = default)
        {
            string result = await RequestStatusHex(module, cancellationToken);
            int offset = address * HEX_OUTPUT_STATUS_LENGTH;

            if (string.IsNullOrEmpty(result) || result.Length < offset + HEX_OUTPUT_STATUS_LENGTH)
            {
                return string.Empty;
            }

            return result.Substring(offset, HEX_OUTPUT_STATUS_LENGTH);
        }

        public async Task<DobissOutput?> RequestOutputStatusAsObject(int module, int address, CancellationToken cancellationToken = default)
        {
            var moduleStatuses = await RequestStatus(module, cancellationToken);

            if (moduleStatuses == null || moduleStatuses.Count == 0)
            {
                return null;
            }

            return moduleStatuses[address];
        }

        public async Task<List<DobissModule>> RequestAllStatus(CancellationToken cancellationToken = default)
        {
            var modules = new List<DobissModule>();
            foreach(var module in moduleTypeMap.Select(t => t.Key))
            {
                var outputs = await RequestStatus(module, cancellationToken);
                if (outputs.Count != 0)
                {
                    modules.Add(new DobissModule(module, outputs));
                }
            }
            return modules;
        }

        public async Task<List<DobissOutput>> RequestStatus(int module, CancellationToken cancellationToken = default)
        {
            if (moduleTypeMap.TryGetValue(module, out ModuleType knownType))
            {
                return await new DobissRequestStatusRequest(dobissClient, knownType, module, lightCacheService.GetAll().Where(t => t.ModuleKey == module)?.Count()).Execute(cancellationToken);
            }
            return [];
        }

        private async Task<string> RequestStatusHex(int module, CancellationToken cancellationToken = default)
        {
            if (moduleTypeMap.TryGetValue(module, out ModuleType knownType))
            {
                return await new DobissRequestStatusRequest(dobissClient, knownType, module, lightCacheService.GetAll().Where(t => t.ModuleKey == module)?.Count())
                    .ExecuteHex(cancellationToken);
            }

            return string.Empty;
        }

        public async Task<List<DobissGroupData>> FetchGroupsData(CancellationToken cancellationToken = default)
        {
            return await new DobissFetchGroupsRequest(dobissClient).Execute(cancellationToken);
        }

        public async Task<List<DobissGroupData>> FetchMoodsData(CancellationToken cancellationToken = default)
        {
            return await new DobissFetchMoodsRequest(dobissClient).Execute(cancellationToken);
        }

        public async Task<List<DobissGroupData>> FetchOutputsData(int module, CancellationToken cancellationToken = default)
        {
            if (moduleTypeMap.TryGetValue(module, out ModuleType value))
            {
                // Use known module type
                return await new DobissFetchOutputsRequest(dobissClient, value, module).Execute(cancellationToken);
            }

            return [];
        }
    }
}
