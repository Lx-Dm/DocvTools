using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using iText.IO.Font;
using Swan.Logging;

namespace DocvTools
{
    class Program
    {
        static void Main(string[] args)
        {
            var url = "http://localhost:9696/";
            if (args.Length > 0)
                url = args[0];

            FontProgramFactory.RegisterSystemFontDirectories();

            using (var server = CreateWebServer(url))
            {
                server.RunAsync();
                Console.ReadKey(true);
            }
        }


        private static WebServer CreateWebServer(string url)
        {
            WebServer server = new WebServer(o => o
            .WithUrlPrefix(url)
            .WithMode(HttpListenerMode.EmbedIO))
            .WithCors()
            .WithLocalSessionManager()
            .WithWebApi("/api", m => m.WithController<SignController>());

            // Listen for state changes.
            server.StateChanged += (s, e) => $"WebServer New State - {e.NewState}".Info();

            return server;
        }
    }

    public class SignController : WebApiController
    {
        [Route(HttpVerbs.Post, "/pdf")]

        public async Task<List<Document>> Sign()
        {
            var data = await HttpContext.GetRequestDataAsync<Request>();

            if (data.Documents != null) {
                if (data.Stamps != null)
                {
                    for (int i = 0; i < data.Documents.Count; i++)
                    {
                        byte[] doc = Util.Base64ToByteArray(data.Documents[i].base64);
                        if (doc.Length > 0)
                        {
                            PdfTools pt = new PdfTools();
                            if (pt.Stamp(doc, data.Stamps) == 0 && pt.SignedPdf != null)
                            {
                                byte[] signedDoc = pt.SignedPdf;
                                data.Documents[i].base64 = Util.ByteArrayToBase64(signedDoc);
                            }
                        }
                    }
                }

                if (data.SignatureParametrs != null) {

                    for (int i = 0; i < data.Documents.Count; i++)
                    {
                        byte[] doc = Util.Base64ToByteArray(data.Documents[i].base64);
                        if (doc.Length > 0)
                        {
                            PdfTools pt = new PdfTools();
                            if (pt.Sign(doc, data.SignatureParametrs) == 0 && pt.SignedPdf != null)
                            {
                                byte[] signedDoc = pt.SignedPdf;
                                data.Documents[i].base64 = Util.ByteArrayToBase64(signedDoc);
                            }
                        }
                    }
                }
                
                return data.Documents;
            }
            return [];
        }
    }
}
