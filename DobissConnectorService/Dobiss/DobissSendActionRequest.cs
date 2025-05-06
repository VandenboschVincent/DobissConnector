using DobissConnectorService.Dobiss.Utils;

namespace DobissConnectorService.Dobiss
{
    public class DobissSendActionRequest(DobissClient client, int module, int address, DobissSendActionRequest.ActionType? actionType = null, int? value = null, int? delayOn = null, int? delayOff = null, int? softDim = null, int? cond = null) : IDobissRequest<object>
    {
        private static readonly byte[] BASE_SEND_ACTION_REQUEST = ConversionUtils.HexToBytes("af02ff000000080108ffffffffffffaf0000000000000000");

        // Default values
        private const int DEFAULT_ACTION_VALUE = 100;
        private const ActionType DEFAULT_ACTION_TYPE = ActionType.TOGGLE;
        private const int DEFAULT_DELAY_ON = -1;
        private const int DEFAULT_DELAY_OFF = -1;
        private const int DEFAULT_SOFT_DIM = -1;
        private const int DEFAULT_COND = -1;

        // Indexes in the request array
        private const int INDEX_MODULE_HEADER = 3;
        private const int INDEX_MODULE = 16;
        private const int INDEX_ADDRESS = 17;
        private const int INDEX_ACTION_TYPE = 18;
        private const int INDEX_DELAY_ON = 19;
        private const int INDEX_DELAY_OFF = 20;
        private const int INDEX_VALUE = 21;
        private const int INDEX_SOFT_DIM = 22;
        private const int INDEX_COND = 23;

        public byte[] GetRequestBytes()
        {
            var byteArray = (byte[])BASE_SEND_ACTION_REQUEST.Clone();

            byteArray[INDEX_MODULE_HEADER] = (byte)module;
            byteArray[INDEX_MODULE] = (byte)module;
            byteArray[INDEX_ADDRESS] = (byte)address;
            byteArray[INDEX_ACTION_TYPE] = actionType?.GetValue() ?? DEFAULT_ACTION_TYPE.GetValue();
            byteArray[INDEX_DELAY_ON] = (byte)(delayOn ?? DEFAULT_DELAY_ON);
            byteArray[INDEX_VALUE] = (byte)(value ?? DEFAULT_ACTION_VALUE);
            byteArray[INDEX_SOFT_DIM] = (byte)(softDim ?? DEFAULT_SOFT_DIM);
            byteArray[INDEX_COND] = (byte)(cond ?? DEFAULT_COND);

            return byteArray;
        }

        public int GetMaxOutputLines() => 2;

        public async Task<object> Execute(CancellationToken cancellationToken)
        {
            return await client.SendRequest(this, cancellationToken);
        }

        public async Task<string> ExecuteHex(CancellationToken cancellationToken)
        {
            return (await client.SendRequest(this, cancellationToken)).ToString() ?? string.Empty;
        }

        public enum ActionType : byte
        {
            OFF = 0,
            ON = 1,
            TOGGLE = 2
        }
    }

    // Extension method for ActionType to match Java-style .getValue()
    public static class ActionTypeExtensions
    {
        public static byte GetValue(this DobissSendActionRequest.ActionType actionType)
        {
            return (byte)actionType;
        }
    }
}
