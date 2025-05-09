﻿using DobissConnectorService.Dobiss.Models;
using DobissConnectorService.Dobiss.Utils;
using System.Text;

namespace DobissConnectorService.Dobiss
{
    public class DobissFetchMoodsRequest(DobissClient client) : IDobissRequest<List<DobissGroupData>>
    {
        private const int MOODS_NAME_LENGTH = 32;

        // TODO: Find dobiss documentation. Is this always the same request or are there dynamic values?
        private const string FETCH_MOODS_REQUEST = "af1020a0c000204020ffffffffffffaf";

        public byte[] GetRequestBytes()
        {
            return ConversionUtils.HexToBytes(FETCH_MOODS_REQUEST);
        }

        public int GetMaxOutputLines()
        {
            return 0;
        }

        public async Task<List<DobissGroupData>> Execute(CancellationToken cancellationToken)
        {
            string moodsString = Encoding.UTF8.GetString(await client.SendRequest(this, cancellationToken));
            var groups = new List<DobissGroupData>();

            for (int i = 0; i < moodsString.Length / MOODS_NAME_LENGTH; i++)
            {
                string name = moodsString.Substring(i * MOODS_NAME_LENGTH, MOODS_NAME_LENGTH).Trim();
                groups.Add(new DobissGroupData(i, name));
            }

            return groups;
        }

        public async Task<string> ExecuteHex(CancellationToken cancellationToken)
        {
            return ConversionUtils.BytesToHex(await client.SendRequest(this, cancellationToken));
        }
    }
}
