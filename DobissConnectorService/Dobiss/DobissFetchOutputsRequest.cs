using DobissConnectorService.Dobiss.Interfaces;
using DobissConnectorService.Dobiss.Models;
using System.Text;

namespace DobissConnectorService.Dobiss
{
    public class DobissFetchOutputsRequest : IDobissRequest<List<DobissOutput>>
    {
        private const string BASE_FETCH_OUTPUTS_REQUEST = "AF10FFFF0100200C20FFFFFFFFFFFFAF";

        private const int INDEX_TYPE = 2;
        private const int INDEX_MODULE = 3;
        private const int INDEX_OUTPUTS = 7;
        private static readonly char[] INVALID_CHAR = ['\u0000', '\u0001', '\0', '\u0002', '\u0003', '\uFFFD'];

        private readonly IDobissClient dobissClient;
        private readonly DobissModule module;

        public DobissFetchOutputsRequest(IDobissClient client, DobissModule module)
        {
            this.dobissClient = client;
            this.module = module;
        }

        public byte[] GetRequestBytes()
        {
            byte[] byteArray = Convert.FromHexString(BASE_FETCH_OUTPUTS_REQUEST);

            byteArray[INDEX_TYPE] = (byte)module.Type;
            byteArray[INDEX_MODULE] = (byte)module.Index;
            byteArray[INDEX_OUTPUTS] = (byte)module.OutputCount;

            return byteArray;
        }

        public int GetMaxOutputLines()
        {
            return 32 * module.OutputCount;
        }

        public async Task<List<DobissOutput>> Execute(CancellationToken cancellationToken)
        {
            var outputsData = await dobissClient.SendRequest(GetRequestBytes(), GetMaxOutputLines(), cancellationToken);
            var Outputs = new List<DobissOutput>();

            if (outputsData.Length != 32 * module.OutputCount)
            {
                throw new InvalidDataException("Invalid data received trying to import outputs");
            }

            for (int outputIndex = 0; outputIndex < module.OutputCount; outputIndex++)
            {
                int offset = outputIndex * 32;
                byte[] line = [.. outputsData.Skip(offset).Take(32)];

                string name = Encoding.ASCII.GetString(line, 0, 30).Trim(INVALID_CHAR).Trim();
                LightType type = (LightType)line[30];
                byte groupIndex = line[31];

                DobissOutput outputInfo = new(outputIndex, module.Index, type, groupIndex, name);
                Outputs.Add(outputInfo);
            }

            return Outputs;
        }

        public async Task<string> ExecuteHex(CancellationToken cancellationToken)
        {
            return Convert.ToHexString(await dobissClient.SendRequest(GetRequestBytes(), GetMaxOutputLines(), cancellationToken));
        }
    }
}
