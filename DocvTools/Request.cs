namespace DocvTools
{
    internal class Request
    {
        public SignatureParameters? SignatureParameters { get; set; }
        public List<Document>? Documents { get; set; }
        public List<Stamp>? Stamps { get; set; }
    }
}
