namespace DocvTools
{
    internal class Util
    {
        public static byte[] Base64ToByteArray(string base64)
        {
            try
            {
                return Convert.FromBase64String(base64);
            }
            catch (FormatException e)
            {
                Console.WriteLine("Invalid Base64 string: " + e.Message);
                return [];
            }
        }

        public static byte[] StreamToByteArray(Stream sourceStream)
        {
            // If the source is already a MemoryStream, we can potentially use ToArray directly for efficiency, 
            // but CopyTo is more general for all stream types.
            using (MemoryStream memoryStream = new())
            {
                // Use CopyTo to transfer data from the source stream to the MemoryStream
                sourceStream.CopyTo(memoryStream);
                // Return the byte array from the MemoryStream
                return memoryStream.ToArray();
            }
        }

        public static string ByteArrayToBase64(byte[] bytes)
        {
            return Convert.ToBase64String(bytes);
        }

        public static bool IsByteArrayPdf(byte[] byteArray)
        {
            // PDF файл должен иметь первые 4 байта сигнатуры
            if (byteArray == null || byteArray.Length < 4)
            {
                return false;
            }

            // Проверка первых 4 байт на соответствие "%PDF"
            // % = 0x25
            // P = 0x50
            // D = 0x44
            // F = 0x46
            if (byteArray[0] == 0x25 &&
                byteArray[1] == 0x50 &&
                byteArray[2] == 0x44 &&
                byteArray[3] == 0x46)
            {
                return true;
            }

            return false;
        }
    }
}
