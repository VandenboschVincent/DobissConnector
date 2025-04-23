using DobissConnectorService.Dobiss.Models;
using DobissConnectorService.Dobiss.Utils;
using System.Text;

namespace DobissConnectorService.Dobiss
{
    public class DobissFetchOutputsRequest : IDobissRequest<List<DobissGroupData>>
    {
        private static readonly byte[] BASE_FETCH_OUTPUTS_REQUEST = ConversionUtils.HexToBytes("af10ffff0100200c20ffffffffffffaf");

        private const int INDEX_TYPE = 2;
        private const int INDEX_MODULE = 3;
        private static readonly char[] INVALID_CHAR = ['\u0001', '\0', '\u0002', '\uFFFD'];
        private const int OUTPUT_NAME_LENGTH = 32;

        private readonly DobissClient dobissClient;
        private readonly ModuleType type;
        private readonly int module;

        public DobissFetchOutputsRequest(DobissClient client, ModuleType type, int module)
        {
            this.dobissClient = client;
            this.type = type;
            this.module = module;
        }

        public byte[] GetRequestBytes()
        {
            byte[] byteArray = (byte[])BASE_FETCH_OUTPUTS_REQUEST.Clone();

            byteArray[INDEX_TYPE] = (byte)type;
            byteArray[INDEX_MODULE] = (byte)module;

            return byteArray;
        }

        public int GetMaxOutputLines()
        {
            return 26;
        }

        public async Task<List<DobissGroupData>> Execute(CancellationToken cancellationToken)
        {
            string groupsString = Encoding.UTF8.GetString(await this.dobissClient.SendRequest(this, cancellationToken));
            var groups = new List<DobissGroupData>();

            for (int i = 0; i < groupsString.Length / OUTPUT_NAME_LENGTH; i++)
            {
                string name = groupsString.Substring(i * OUTPUT_NAME_LENGTH, OUTPUT_NAME_LENGTH)
                    .Trim(INVALID_CHAR).Trim();
                if (!string.IsNullOrWhiteSpace(name))
                {
                    groups.Add(new DobissGroupData(i, name));
                }
            }

            return groups;
        }

        public async Task<string> ExecuteHex(CancellationToken cancellationToken)
        {
            return ConversionUtils.BytesToHex(await this.dobissClient.SendRequest(this, cancellationToken));
        }
    }
}
