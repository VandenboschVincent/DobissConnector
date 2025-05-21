using DobissConnectorService.Dobiss.Interfaces;
using DobissConnectorService.Dobiss.Models;

namespace DobissConnectorService.Dobiss
{
    public class DobissRequestStatusRequest(IDobissClient dobissClient, ModuleType type, int module, int outputs) : IDobissRequest<List<(int index, int value)>>
    {
        private readonly string BASE_REQUEST_STATUS = $"AF01{(byte)type:X2}{module:X2}0000000100FFFFFFFFFFFFAF";

        public byte[] GetRequestBytes()
        {
            return Convert.FromHexString(BASE_REQUEST_STATUS);
        }

        public int GetMaxOutputLines()
        {
            return 16;
        }

        public async Task<List<(int index, int value)>> Execute(CancellationToken cancellationToken)
        {
            List<(int index, int value)> finalResult = new();
            byte[] result = await dobissClient.SendRequest(GetRequestBytes(), GetMaxOutputLines(), cancellationToken);

            if (result.Length != 16)
            {
               throw new InvalidDataException("Invalid data received trying to request status");
            }

            for (int i = 0; i < outputs; i++)
            {
                finalResult.Add(new(i, result[i]));
            }
            return finalResult;
        }
    }
}
