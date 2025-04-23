using DobissConnectorService.Dobiss.Models;
using DobissConnectorService.Dobiss.Utils;
using static DobissConnectorService.Dobiss.DobissSendActionRequest;
using System.Reflection.Emit;

namespace DobissConnectorService.Dobiss
{
    public class DobissRequestStatusRequest : IDobissRequest<List<DobissOutput>>
    {
        private const int MAX_OUTPUTS_PER_MODULE = 8;
        private const byte EMPTY_BYTE = 0xFF;

        private readonly DobissClient dobissClient;
        private readonly ModuleType type;
        private readonly int module;

        public DobissRequestStatusRequest(DobissClient dobissClient, ModuleType type, int module)
        {
            this.dobissClient = dobissClient;
            this.type = type;
            this.module = module;
        }

        public byte[] GetRequestBytes()
        {
            return
            [
                0xAF,
                0x01, //Poll=0x01, Action=0x02
                (byte)type,
                (byte)module,
                0x00,
                0x00,
                0x08,
                0x01,
                0,
                0xFF,
                0xFF,
                0xFF,
                0xFF,
                0xFF,
                0xFF,
                0xAF
            ];
        }

        public int GetMaxOutputLines()
        {
            return 4;
        }

        public async Task<List<DobissOutput>> Execute(CancellationToken cancellationToken)
        {
            byte[] result = await dobissClient.SendRequest(this, cancellationToken);
            if (result == null || result.Length == 0)
            {
                return [];
            }

            Array.Resize(ref result, MAX_OUTPUTS_PER_MODULE);

            var resultList = new List<DobissOutput>();
            for (int i = 0; i < result.Length; i++)
            {
                if (result[i] != EMPTY_BYTE)
                {
                    resultList.Add(new DobissOutput(i,  Convert.ToInt32(result[i])));
                }
            }

            return resultList;
        }

        public async Task<string> ExecuteHex(CancellationToken cancellationToken)
        {
            byte[] result = await dobissClient.SendRequest(this, cancellationToken);
            if (result == null || result.Length == 0)
            {
                return string.Empty;
            }

            Array.Resize(ref result, MAX_OUTPUTS_PER_MODULE);

            return ConversionUtils.BytesToHex(ConversionUtils.TrimBytes(result, EMPTY_BYTE));
        }
    }
}
