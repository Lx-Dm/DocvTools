namespace DocvTools
{
    public class SignatureParameters
    {
        public SignatureParameters() {
            Apperance = new SignatureApperance();
        }
        public string? Certificate_dn { get; set; }
        public string? Password { get; set; }
        public string? ContactInfo { get; set; }
        
        public string? Reason { get; set; }
        public string? Location { get; set; }
        public SignatureApperance Apperance { get; set; }
    }

    public class SignatureApperance
    {
        public SignatureApperance() {
            Template = "Серийный номер {0}\nВладелец {1}\nДействителен с {2} по {3}";
            Ratio = 1.0f;
            Visible = false;
            FontName = "Times New Roman";
            FontSize = 12;
            PageNum = 0;
            Offset = new();
            ImgPath = "blank.png";
            AddCertInfo = true;
        }
        public string Template { get; set; }
        public string ImgPath { get; set; }
        public bool AddCertInfo { get; set; }
        public bool Visible { get; set; }
        public float Ratio { get; set; }
        public string FontName { get; set; }
        public int FontSize { get; set; }
        public Offset Offset { get; set; }
        public Marker? Marker { get; set; }
        public Area? Area { get; set; }
        public int PageNum { get; set; }
    }
}
