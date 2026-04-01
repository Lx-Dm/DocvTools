using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using iText.IO.Font;
using Swan.Logging;

namespace DocvTools
{
    class Program
    {
        private static readonly Mutex mutex = new(true, "bc97e66a-d51c-4dd1-8ede-0eca63a969b7");
        [STAThread]
        static void Main(string[] args)
        {
            if (!Directory.Exists("logs"))
            {
                Directory.CreateDirectory("logs");
            }

            var logPath = "logs/application.log";
            Logger.RegisterLogger(new FileLogger(logPath, true));

            if (mutex.WaitOne(TimeSpan.Zero, true))
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
                mutex.ReleaseMutex(); // Освобождаем при закрытии
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
                                $"STAMP: {data.Documents[i].name}".Info();
                                byte[] signedDoc = pt.SignedPdf;
                                data.Documents[i].base64 = Util.ByteArrayToBase64(signedDoc);
                            }
                        }
                    }
                }

                if (data.SignatureParameters != null) {

                    for (int i = 0; i < data.Documents.Count; i++)
                    {
                        byte[] doc = Util.Base64ToByteArray(data.Documents[i].base64);
                        if (doc.Length > 0)
                        {
                            PdfTools pt = new PdfTools();
                            if (pt.Sign(doc, data.SignatureParameters) == 0 && pt.SignedPdf != null)
                            {
                                $"SIGN: {data.Documents[i].name}".Info();
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
