using DobissConnectorService.Dobiss.Interfaces;
using DobissConnectorService.Dobiss.Models;
using DobissConnectorService.Dobiss.Utils;
using System.Text;

namespace DobissConnectorService.Dobiss
{
    public class DobissFetchGroupsRequest(IDobissClient client) : IDobissRequest<List<DobissGroupData>>
    {
        private const int GROUP_NAME_LENGTH = 32;

        // TODO: Find dobiss documentation. Is this always the same request or are there dynamic values?
        private const string FETCH_GROUPS_REQUEST = "af1020a0a000202020ffffffffffffaf";

        public byte[] GetRequestBytes()
        {
            return ConversionUtils.HexToBytes(FETCH_GROUPS_REQUEST);
        }

        public int GetMaxOutputLines()
        {
            return 0;
        }

        public async Task<List<DobissGroupData>> Execute(CancellationToken cancellationToken)
        {
            string groupsString = Encoding.UTF8.GetString(await client.SendRequest(GetRequestBytes(), GetMaxOutputLines(), cancellationToken));
            var groups = new List<DobissGroupData>();

            for (int i = 0; i < groupsString.Length / GROUP_NAME_LENGTH; i++)
            {
                string name = groupsString.Substring(i * GROUP_NAME_LENGTH, GROUP_NAME_LENGTH).Trim();
                groups.Add(new DobissGroupData(i, name));
            }

            return groups;
        }

        public async Task<string> ExecuteHex(CancellationToken cancellationToken)
        {
            return ConversionUtils.BytesToHex(await client.SendRequest(GetRequestBytes(), GetMaxOutputLines(), cancellationToken));
        }
    }
}
