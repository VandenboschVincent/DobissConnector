using DobissConnectorService.Dobiss.Interfaces;

namespace DobissConnectorService.Dobiss
{
    public class DobissSendActionRequest(IDobissClient client, int moduleIndex, int outputIndex, int value, DobissSendActionRequest.ActionType actionType = DobissSendActionRequest.ActionType.TOGGLE, int delayOn = -1, int delayOff = -1, int softDim = -1, int red = -1) : IDobissRequest<bool>
    {
        public byte[] GetRequestBytes()
        {
            return Convert.FromHexString($"AF02FF{moduleIndex:X2}0000080108FFFFFFFFFFFFAF");
        }

        public int GetMaxOutputLines() => 0;

        public async Task<bool> Execute(CancellationToken cancellationToken)
        {
            await client.SendRequest(GetRequestBytes(), GetMaxOutputLines(), cancellationToken);

            byte[] requestData = [(byte)moduleIndex, (byte)outputIndex, (byte)actionType, (byte)delayOn, (byte)delayOff, (byte)value, (byte)softDim, (byte)red];
            await client.SendRequest(requestData, GetMaxOutputLines(), cancellationToken);
            return true;
        }

        public enum ActionType : byte
        {
            OFF = 0x00,
            ON = 0x01,
            TOGGLE = 0x02
        }
    }
}
