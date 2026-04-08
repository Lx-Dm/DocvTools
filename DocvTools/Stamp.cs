using iText.Kernel.Geom;

namespace DocvTools
{
    internal class Stamp
    {
        public Stamp() {
            FontName  = "Times New Roman";
            FontSize = 12;
            Areas = [];
        }
        public required string Type { get; set; }
        public required string Text { get; set; }
        public string FontName { get; set; }
        public int FontSize { get; set; }
        
        public Marker? Marker { get; set; }
        public List<Area> Areas { get; set; }
    }

    internal class Area 
    {
        public Area(float x, float y, float width, float height, int pn)
        { 
            X = x;
            Y = y;
            W = width;
            H = height;
            PageNumber = pn;
        }

        public float X { get; set; }
        public float Y { get; set; }
        public float H { get; set; }
        public float W { get; set; }
        public int PageNumber { get; set; }
    }
    internal class Offset
    {
        public Offset()
        {
            X = 0;
            Y = 0;
        }
        public int X { get; set; }
        public int Y { get; set; }
    }

    internal class Marker
    {
        public Marker()
        {
            ReverseSearch = false;
            SearchAll = false;
            Offset = new();
        }
        public string? Text { get; set; }
        public Offset Offset { get; set; }
        public bool ReverseSearch { get; set; }
        public bool SearchAll { get; set; }
    }
}
