using iText.Kernel.Geom;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Data;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using System.Text;

namespace DocvTools
{
    public class CustomLocationTextExtractionStrategy(string stringForSearch, bool sa) : LocationTextExtractionStrategy
    {
        internal List<Area> AreaList = [];
        private int CurrentPageNumber = 0;
        public bool SearchAll = sa;
        private Rectangle? Rect;
        private readonly StringBuilder strBld = new();
        private readonly string searchedString = stringForSearch;

        public override void EventOccurred(IEventData data, EventType type)
        {
            if (type == EventType.RENDER_TEXT && strBld.ToString() != searchedString)
            {
                var renderInfo = (TextRenderInfo)data;

                for (int i = 0; i < renderInfo.GetCharacterRenderInfos().Count && strBld.ToString() != searchedString; i++) {
                    var t = renderInfo.GetCharacterRenderInfos()[i];

                    if (t.GetText() == searchedString.Substring(strBld.Length, 1))
                    {
                        strBld.Append(t.GetText());

                        var start = t.GetDescentLine().GetStartPoint();
                        var end = t.GetAscentLine().GetEndPoint();

                        if (strBld.Length > 1 && Rect != null)
                        {
                            Rect.SetWidth(end.Get(0) - Rect.GetX());
                        }
                        else
                        {
                            Rect = new Rectangle(
                                        start.Get(0),
                                        start.Get(1),
                                        end.Get(0) - start.Get(0),
                                        end.Get(1) - start.Get(1));
                        }
                    }
                    else
                    {
                        strBld.Clear();
                        Rect = null;
                    }

                    if (strBld.ToString() == searchedString && Rect != null) {
                        AreaList.Add(new Area(Rect.GetX(), Rect.GetY(), Rect.GetWidth(), Rect.GetHeight(), CurrentPageNumber));
                        if (SearchAll) {
                            strBld.Clear();
                            Rect = null;
                        }
                    }
                }
            }
        }

        public void SetPageNumber(int pn) {
            CurrentPageNumber = pn;
        }
    }
}
