using System;
using Foundation;
using UIKit;
using WebKit;

namespace BlackIOS
{
    public class WebAppInterface : NSObject, IWKScriptMessageHandler
    {
        private UIViewController _controller;

        public WebAppInterface(UIViewController controller)
        {
            _controller = controller;
        }

        public void DidReceiveScriptMessage(WKUserContentController userContentController, WKScriptMessage message)
        {
            // O JavaScript vai mandar mensagens na forma:
            // window.webkit.messageHandlers.Android.postMessage({ method: "playVod", args: ["url"] });
            // Mas, no app original, o JS chama `Android.playVod()`.
            // Para mantermos a mesma estrutura, o JS original não precisa ser alterado, 
            // basta injetar um script no webview que converte chamadas Android.XYZ para postMessage.
            // Aqui nós tratamos o recebimento dessas chamadas convertidas.
            
            try
            {
                var dict = message.Body as NSDictionary;
                if (dict == null) return;
                
                var method = dict["method"]?.ToString();
                
                switch (method)
                {
                    case "playVod":
                        var args = dict["args"] as NSArray;
                        if (args != null && args.Count > 0)
                        {
                            var urlString = args.GetItem<NSString>(0).ToString();
                            var nsUrl = NSUrl.FromString(urlString);
                            if (nsUrl != null)
                            {
                                var player = new AVFoundation.AVPlayer(nsUrl);
                                var playerViewController = new AVKit.AVPlayerViewController
                                {
                                    Player = player
                                };
                                _controller.PresentViewController(playerViewController, true, () =>
                                {
                                    player.Play();
                                });
                            }
                        }
                        break;
                    case "getAppVersion":
                        // Tratado sincronicamente no JS injetado
                        break;
                    case "exitApp":
                        Environment.Exit(0);
                        break;
                    // Os outros métodos (favoritos, etc) precisarão de implementação com NSUserDefaults
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro no JS bridge: " + ex.Message);
            }
        }
    }
}
