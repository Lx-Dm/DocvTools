namespace DocvTools
{
    public class Stamp
    {
        public Stamp() {
            FontName  = "Times-Roman";
            FontSize = 12;
            PageNum = 0;
        }
        public required string Text { get; set; }
        public string FontName { get; set; }
        public int FontSize { get; set; }
        
        public Marker? Marker { get; set; }
        public Area? Area { get; set; }
        public int PageNum { get; set; }
    }

    public class Area 
    {
        public required int X { get; set; }
        public required int Y { get; set; }
        public required int H { get; set; }
        public required int W { get; set; }
    }
    public class Offset
    {
        public Offset()
        {
            X = 0;
            Y = 0;
        }

        public int X { get; set; }
        public int Y { get; set; }
    }

    public class Marker
    {
        public Marker()
        {
            ReverseSearch = false;
            Offset = new();
        }
        public string? Text { get; set; }
        public Offset Offset { get; set; }
        public bool ReverseSearch { get; set; }
    }
}
