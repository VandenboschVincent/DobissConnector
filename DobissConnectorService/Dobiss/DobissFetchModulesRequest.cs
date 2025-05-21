using DobissConnectorService.Dobiss.Interfaces;
using DobissConnectorService.Dobiss.Models;

namespace DobissConnectorService.Dobiss
{
    public class DobissFetchModulesRequest(IDobissClient client) : IDobissRequest<List<DobissModule>>
    {
        private const string FETCH_MODULES_REQUEST = "AF0B00003000100110FFFFFFFFFFFFFFAF";

        public byte[] GetRequestBytes()
        {
            return Convert.FromHexString(FETCH_MODULES_REQUEST);
        }

        public int GetMaxOutputLines()
        {
            return 16;
        }

        public async Task<List<DobissModule>> Execute(CancellationToken cancellationToken)
        {
            List<int> modules = [];
            List<DobissModule> foundModules = [];
            byte[] installationData = await client.SendRequest(GetRequestBytes(), GetMaxOutputLines(), cancellationToken);
            for (int i = 0; i < 82; i++)
            {
                int byteNum = i / 8;
                int bitNum = i % 8;
                bool hasModule = ((installationData[byteNum] >> bitNum) & 1) == 1;
                if (hasModule)
                {
                    modules.Add(i + 1);
                }
            }

            foreach (int moduleAddr in modules)
            {
                foundModules.Add(await ImportModuleAsync(moduleAddr, cancellationToken));
            }
            return foundModules;
        }

        public async Task<DobissModule> ImportModuleAsync(int moduleAddr, CancellationToken cancellationToken)
        {
            byte[] data = Convert.FromHexString($"AF10FF{moduleAddr:X2}0000100110FFFFFFFFFFFFAF");
            byte[] moduleData = await client.SendRequest(data, GetMaxOutputLines(), cancellationToken);

            if (moduleData.Length != GetMaxOutputLines())
            {
                throw new ArgumentException($"Invalid module data length: {moduleData.Length}");
            }

            byte address = moduleData[0];
            ModuleType type = (ModuleType)moduleData[14];
            bool isMaster = (moduleData[2] & 1) == 1;
            int outputCount = type == ModuleType.RELAY ? 12 : 4;

            return new DobissModule(address, type, isMaster, outputCount);
        }

        public async Task<string> ExecuteHex(CancellationToken cancellationToken)
        {
            return Convert.ToHexString(await client.SendRequest(GetRequestBytes(), GetMaxOutputLines(), cancellationToken));
        }
    }
}
