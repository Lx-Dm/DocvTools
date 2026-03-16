namespace DocvTools
{
    internal class Request
    {
        public SignatureParameters? SignatureParametrs { get; set; }
        public List<Document>? Documents { get; set; }
        public List<Stamp>? Stamps { get; set; }
    }
}
