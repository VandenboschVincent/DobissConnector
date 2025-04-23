using DobissConnectorService.Dobiss.Utils;

namespace DobissConnectorService.Dobiss
{
    public class DobissSendActionRequest : IDobissRequest<object>
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

        private readonly DobissClient dobissClient;

        private readonly int module;
        private readonly int address;
        private readonly ActionType? actionType;
        private readonly int? value;
        private readonly int? delayOn;
        private readonly int? delayOff;
        private readonly int? softDim;
        private readonly int? cond;

        public DobissSendActionRequest(DobissClient client, int module, int address, ActionType? actionType = null, int? value = null, int? delayOn = null, int? delayOff = null, int? softDim = null, int? cond = null)
        {
            this.dobissClient = client;
            this.module = module;
            this.address = address;
            this.actionType = actionType;
            this.value = value;
            this.delayOn = delayOn;
            this.delayOff = delayOff;
            this.softDim = softDim;
            this.cond = cond;
        }

        public byte[] GetRequestBytes()
        {
            var byteArray = (byte[])BASE_SEND_ACTION_REQUEST.Clone();

            byteArray[INDEX_MODULE_HEADER] = (byte)module;
            byteArray[INDEX_MODULE] = (byte)module;
            byteArray[INDEX_ADDRESS] = (byte)address;
            byteArray[INDEX_ACTION_TYPE] = (byte)(actionType?.GetValue() ?? DEFAULT_ACTION_TYPE.GetValue());
            byteArray[INDEX_DELAY_ON] = (byte)(delayOn ?? DEFAULT_DELAY_ON);
            byteArray[INDEX_VALUE] = (byte)(value ?? DEFAULT_ACTION_VALUE);
            byteArray[INDEX_SOFT_DIM] = (byte)(softDim ?? DEFAULT_SOFT_DIM);
            byteArray[INDEX_COND] = (byte)(cond ?? DEFAULT_COND);

            return byteArray;
        }

        public int GetMaxOutputLines() => 2;

        public async Task<object> Execute(CancellationToken cancellationToken)
        {
            await dobissClient.SendRequest(this, cancellationToken);
            return null;
        }

        public async Task<string> ExecuteHex(CancellationToken cancellationToken)
        {
            await dobissClient.SendRequest(this, cancellationToken);
            return null;
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
