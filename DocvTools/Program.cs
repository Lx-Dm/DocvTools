// See https://aka.ms/new-console-template for more information

namespace DocvTools
{

    using EmbedIO;
    using EmbedIO.Routing;
    using EmbedIO.WebApi;

    using Swan.Logging;
    using System;

    class Program
    {
        //Logger.RegisterLogger(new ConsoleLogger());
        static void Main(string[] args)
        {
            var url = "http://localhost:9696/";
            if (args.Length > 0)
                url = args[0];

            // Our web server is disposable.
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
                //data.Documents.First().ToBase64String(File.ReadAllBytes(@"D:\Documents\test_doc.pdf")); //Закомментировать (использовалось для теста)
                if (data.Stamps != null)
                {
                    for (int i = 0; i < data.Documents.Count; i++)
                    {
                        if (PdfTools.Stamp(data.Documents[i].ByteArray(), data.Stamps) == 0 && PdfTools.SignedPdf != null)
                        {
                            byte[] signedDoc = PdfTools.SignedPdf;
                            data.Documents[i].ToBase64String(signedDoc);
                        }
                    }
                }

                if (data.SignatureParametrs != null) {
                    

                    for (int i = 0; i < data.Documents.Count; i++)
                    {
                        if (PdfTools.Sign(data.Documents[i].ByteArray(), data.SignatureParametrs) == 0 && PdfTools.SignedPdf != null)
                        {
                            byte[] signedDoc = PdfTools.SignedPdf;
                            data.Documents[i].ToBase64String(signedDoc);
                            //File.WriteAllBytes(@"D:\Documents\test_doc_signed.pdf", signedDoc);         //Закомментировать (использовалось для теста)
                        }
                    }
                }
                
                return data.Documents;
            }
            return [];
        }
    }
}
