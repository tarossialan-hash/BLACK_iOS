using System.Net.Http;
using System.Net.Security;

namespace BlackIOS.Data.Remote
{
    public static class HttpClientFactory
    {
        // Identifica o app de verdade pro painel 
        public const string UserAgent = "BlackPro/v1.0.5.7-Exo/2";

        public static HttpClient CreateUnsafeClient()
        {
            var handler = new HttpClientHandler
            {
                // Ignorar verificação de certificado SSL (como no NetworkModule.kt do Android)
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
            };

            var client = new HttpClient(handler);
            client.DefaultRequestHeaders.Add("User-Agent", UserAgent);
            return client;
        }
    }
}
