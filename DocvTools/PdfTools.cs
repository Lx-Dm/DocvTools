using CryptoPro.Security.Cryptography;
using CryptoPro.Security.Cryptography.X509Certificates;
using iText.Forms.Form.Element;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Layout;
using iText.Layout.Element;
using iText.Signatures;
using Swan.Logging;
using System.Security;
using System.Security.Cryptography.X509Certificates;

namespace DocvTools
{
    public class PdfTools
    {
        public static byte[]? SignedPdf { get; set; }

        public static int Sign(byte[] doc, SignatureParameters sp)
        {
            //Очищаем массив с подписанным pdf
            SignedPdf = null;

            //Проверяем сигнатуру файла на соответствие PDF
            if (!Util.IsByteArrayPdf(doc)) {
                "Файл не является PDF".Info();
                return 1;
            }

            // Находим секретный ключ по сертификату в хранилище MY
            CpX509Store store = new(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.OpenExistingOnly | OpenFlags.ReadOnly);

            CpX509Certificate2Collection found = store.Certificates.Find(
                X509FindType.FindBySubjectName, sp.Certificate_dn, true);

            if (found.Count == 0)
            {
                "Секретный ключ не найден".Info();
                return 1;
            }
            if (found.Count > 1)
            {
                "Найдено более одного секретного ключа".Info();
                return 1;
            }

            CpX509Certificate2 certificate = found.First();

            //Задаем пароль для секретного ключа
            if (sp.Password != null)
            {
                SecureString secureString = new();
                foreach (char walk in sp.Password)
                {
                    secureString.AppendChar(walk);
                }

                Gost3410_2012_256CryptoServiceProvider? privateKey = certificate.GetGost3410_2012_256PrivateKey() as Gost3410_2012_256CryptoServiceProvider;
                privateKey?.SetContainerPassword(secureString);
            }

            using (PdfReader reader = new(new MemoryStream(doc))) {
                using MemoryStream outputStream = new();

                StampingProperties properties = new StampingProperties().UseAppendMode(); // Use append mode to add signature to existing PDF

                PdfSigner signer = new(reader, outputStream, properties);

                SignatureFieldAppearance appearance = new SignatureFieldAppearance("sig").SetBackgroundColor(ColorConstants.WHITE);

                if (sp.Apperance.Visible)
                {
                    Rectangle? rect = null;
                    int pageNumber = 0;
                    if (sp.Apperance.Area != null && sp.Apperance.PageNum != 0 && Math.Abs(sp.Apperance.PageNum) <= signer.GetDocument().GetNumberOfPages())
                    {
                        rect = new(sp.Apperance.Area.X, sp.Apperance.Area.Y, sp.Apperance.Area.W, sp.Apperance.Area.H);
                        if (sp.Apperance.PageNum > 0)
                        {
                            pageNumber = sp.Apperance.PageNum;
                        }
                        else if (sp.Apperance.PageNum < 0)
                        {
                            pageNumber = (int)(signer.GetDocument().GetNumberOfPages() + sp.Apperance.PageNum + 1);
                        }
                    }
                    else if (sp.Apperance.Marker != null && sp.Apperance.Marker.Text != null)
                    {
                        (rect, pageNumber) = ExtractLocation(signer.GetDocument(), sp.Apperance.Marker.Text, sp.Apperance.Marker.ReverseSearch);
                        if (rect != null) {
                            rect.SetX(rect.GetX() + sp.Apperance.Marker.Offset.X);
                            rect.SetY(rect.GetY() + sp.Apperance.Marker.Offset.Y);
                        }
                    }

                    if (rect != null && pageNumber > 0)
                    {
                        string text = string.Format(sp.Apperance.Template,
                                certificate.GetSerialNumberString(),
                                certificate.GetNameInfo(X509NameType.SimpleName, false),
                                certificate.NotBefore.ToShortDateString(),
                                certificate.NotAfter.ToShortDateString());

                        ImageData img = sp.Apperance.AddCertInfo ? 
                            Util.GenerateSignatureImage(sp.Apperance.ImgPath, text, sp.Apperance.FontName, sp.Apperance.FontSize, sp.Apperance.Offset) :
                            Util.GenerateSignatureImage(sp.Apperance.ImgPath);

                        Util.CalculateRect(rect, img.GetWidth() / img.GetHeight(), sp.Apperance.Ratio);

                        $"Серийный номер {certificate.GetSerialNumberString()} Владелец {certificate.GetNameInfo(X509NameType.SimpleName, false)} Действителен с {certificate.NotBefore.ToShortDateString()} по {certificate.NotAfter.ToShortDateString()}".Info();

                        signer.GetSignerProperties()
                                    .SetPageRect(rect)
                                    .SetPageNumber(pageNumber);

                        appearance.SetContent(img);
                    }
                }

                signer.GetSignerProperties()
                    .SetReason(sp.Reason)
                    .SetContact(sp.ContactInfo)
                    .SetLocation(sp.Location)
                    .SetSignatureAppearance(appearance);

                IExternalSignatureContainer external = new SignatureContainer(certificate);

                signer.SignExternalContainer(external, 8192);

                reader.Close();

                SignedPdf = outputStream.ToArray();
            } ;

            $"Документ успешно подписан на ключе {certificate.Subject}".Info();

            return 0;
        }

        public static int Stamp(byte[] doc, List<Stamp> st) {

            using MemoryStream stream = new(doc);
            using MemoryStream outputStream = new();

            PdfReader reader = new(stream); // Use the stream for reader
            PdfWriter writer = new(outputStream); // Use a new stream/path for writer

            PdfDocument pdfDoc = new(reader, writer);

            $"Count stamps: {st.Count}".Debug();
            foreach (var stp in st) {
                PdfFont font = PdfFontFactory.CreateFont(stp.FontName);

                int pageNumber = 0;
                Rectangle? stampRect = null;

                if (stp.Area != null && stp.PageNum != 0 && Math.Abs(stp.PageNum) <= pdfDoc.GetNumberOfPages()) { //проверяем наличие абсолютного положения
                    $"Stamp: {stp.Text}".Debug();
                    stampRect = new(stp.Area.X, stp.Area.Y, stp.Area.W, stp.Area.H);

                    if (stp.PageNum > 0)
                    {
                        pageNumber = (int)stp.PageNum;
                    } else if (stp.PageNum < 0)
                    {
                        pageNumber = (int)(pdfDoc.GetNumberOfPages() + stp.PageNum + 1);
                    }

                } else if (stp.Marker != null && stp.Marker.Text != null) {    //проверяем наличие маркера относительного положения

                    //пробуем получить координаты маркера и номер страницы
                    (stampRect, pageNumber) = ExtractLocation(pdfDoc, stp.Marker.Text, stp.Marker.ReverseSearch); 

                    //проверяем координаты маркера и номер страницы
                    if (stampRect != null) 
                    {
                        stampRect.SetWidth(font.GetWidth(stp.Text, stp.FontSize));

                        if (stp.Marker.Offset != null) stampRect.SetX(stampRect.GetX() + stp.Marker.Offset.X)
                                .SetY(stampRect.GetY() + stp.Marker.Offset.Y);
                    }
                }

                if (stampRect != null && pageNumber > 0) {
                    PdfPage pageDoc = pdfDoc.GetPage(pageNumber);

                    Paragraph paragraph = new Paragraph(stp.Text)
                                .SetFont(font)
                                .SetFontSize(stp.FontSize); // Optional: set font size

                    Canvas canvas = new(pageDoc, stampRect);
                    canvas.Add(paragraph);

                    // 6. Close the canvas and document
                    canvas.Close();
                }
            }

            pdfDoc.Close();
            SignedPdf = outputStream.ToArray();
            return 0;
        }

        private static (Rectangle? rect, int page) ExtractLocation(PdfDocument pdfDoc, string searchedString, bool reverseSearch = false)
        {
            int startPage = 1;
            int numPage = 0;

            //Создаем объект стратегии поиска и извлечения координат
            var strategy = new CustomLocationTextExtractionStrategy(searchedString);

            while (strategy.Rect == null && startPage <= pdfDoc.GetNumberOfPages())
            {
                numPage = reverseSearch ? (pdfDoc.GetNumberOfPages() - startPage + 1) : startPage;
                var page = pdfDoc.GetPage(numPage);
                PdfTextExtractor.GetTextFromPage(page, strategy);
                startPage++;
            }

            if (strategy.Rect != null)
            {
                $"Marker {searchedString} location:, X: {strategy.Rect.GetX()}, Y: {strategy.Rect.GetY()}, H: {strategy.Rect.GetHeight()}, W: {strategy.Rect.GetWidth()}, Page: {numPage}".Debug();
            }
            else {
                $"Marker {searchedString} not found".Debug();
            }

            return (strategy.Rect, numPage);
        }
    }
}