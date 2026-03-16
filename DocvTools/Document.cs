namespace DocvTools
{
    public class Document
    {
        public string? name { get; set; }
        public string? base64 { get; set; }

        public byte[] ByteArray() {
            try
            {
                // Convert the Base64 string to a byte array
                byte[] decodedBytes = Convert.FromBase64String(base64);

                return decodedBytes;
            }
            catch (FormatException e)
            {
                // Handle invalid Base64 strings (e.g., incorrect padding or characters)
                Console.WriteLine("Invalid Base64 string: " + e.Message);
                return [];
            }
        }

        public string ToBase64String(byte[] bytes)
        {
            base64 = Convert.ToBase64String(bytes);
            return base64;

        }
    }

}
