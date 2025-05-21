using System.Text;

namespace DobissConnectorService.Dobiss.Utils
{
    public static class ConversionUtils
    {
        private static readonly char[] HexArray = "0123456789abcdef".ToCharArray();

        /// <summary>
        /// Converts a byte array to its hexadecimal string representation.
        /// </summary>
        public static string BytesToHex(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
                return string.Empty;

            var sb = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes)
            {
                sb.Append(ByteToHex(b));
            }
            return sb.ToString();
        }

        /// <summary>
        /// Converts a single byte to a 2-character hexadecimal string.
        /// </summary>
        public static string ByteToHex(byte b)
        {
            int v = b & 0xFF;
            char high = HexArray[v >> 4];
            char low = HexArray[v & 0x0F];
            return new string([high, low]);
        }

        /// <summary>
        /// Converts a hexadecimal string to a byte array.
        /// </summary>
        public static byte[] HexToBytes(string hex)
        {
            if (string.IsNullOrEmpty(hex))
                return [];

            int len = hex.Length;
            if (len % 2 != 0)
                throw new ArgumentException("Hex string must have an even length", nameof(hex));

            var bytes = new byte[len / 2];
            for (int i = 0; i < len; i += 2)
            {
                int high = Convert.ToInt32(hex[i].ToString(), 16);
                int low = Convert.ToInt32(hex[i + 1].ToString(), 16);
                bytes[i / 2] = (byte)((high << 4) + low);
            }
            return bytes;
        }

        /// <summary>
        /// Converts a byte to an unsigned integer (0-255).
        /// </summary>
        public static int ByteToUnsignedInt(byte b)
        {
            return b & 0xFF;
        }

        /// <summary>
        /// Trims trailing bytes equal to the specified trimByte from the end of the array.
        /// </summary>
        public static byte[] TrimBytes(byte[] bytes, byte trimByte)
        {
            if (bytes == null || bytes.Length == 0)
                return Array.Empty<byte>();

            int index = bytes.Length;
            while (index > 0 && bytes[index - 1] == trimByte)
            {
                index--;
            }

            if (index == bytes.Length)
            {
                // No trimming needed
                return bytes;
            }

            var result = new byte[index];
            Array.Copy(bytes, 0, result, 0, index);
            return result;
        }
    }
}
