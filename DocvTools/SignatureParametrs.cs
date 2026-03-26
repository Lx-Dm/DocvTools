using iText.IO.Font.Constants;
using System.Drawing;

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
            Scale = 1.0f;
            Visible = false;
            PageNum = 0;
            Offset = new();
        }
        public bool Visible { get; set; }
        public float Scale { get; set; }
        public Offset Offset { get; set; }
        public Marker? Marker { get; set; }
        public Area? Area { get; set; }
        public int PageNum { get; set; }
        public Layout? Layout { get; set; }
    }
    public class Layout
    {
        public Layout() {
            Elements = [];
            BorderColor = [0, 0, 0];
        }
        public float Width { get; set; }
        public float Height { get; set; }
        public float Border { get; set; }
        public float BorderRadius { get; set; }
        public int[] BorderColor { get; set; }
        public List<LayoutElement> Elements { get; set; }
    }
    public class LayoutElement {
        public LayoutElement() {
            Type = "text";
            Text = "";
            FontFamily = "Times New Roman";
            BackgroundColor = [255, 255, 255];
            FontColor = [0, 0, 0];
        }
        public string Type { get; set; }
        public string Text { get; set; }
        public string FontFamily { get; set; }
        public int FontSize { get; set; }
        public int HorizontalAlignment { get; set; }
        public float FixedPositionLeft { get; set; }
        public float FixedPositionBottom { get; set; }
        public float Width { get; set; }
        public int[] BackgroundColor { get; set; }
        public int[] FontColor { get; set; }
    }
}
