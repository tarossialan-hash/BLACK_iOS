using System;
using Foundation;
using UIKit;
using WebKit;

namespace BlackIOS
{
    public class ViewController : UIViewController
    {
        private WKWebView _webView;
        private WebAppInterface _webAppInterface;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var userContentController = new WKUserContentController();
            _webAppInterface = new WebAppInterface(this);
            userContentController.AddScriptMessageHandler(_webAppInterface, "AndroidApp");

            var jsCode = @"
                window.AndroidApp = {
                    playVod: function(url, title) { window.webkit.messageHandlers.AndroidApp.postMessage({ method: 'playVod', args: [url, title] }); },
                    playLiveTv: function(url, title) { window.webkit.messageHandlers.AndroidApp.postMessage({ method: 'playLiveTv', args: [url, title] }); },
                    login: function(u, p) { window.webkit.messageHandlers.AndroidApp.postMessage({ method: 'login', args: [u, p] }); },
                    logout: function() { window.webkit.messageHandlers.AndroidApp.postMessage({ method: 'logout' }); },
                    getAppVersion: function() { return '1.0.5.7 (iOS)'; },
                    isTv: function() { return false; },
                    addFavorite: function(j) { window.webkit.messageHandlers.AndroidApp.postMessage({ method: 'addFavorite', args: [j] }); return true; },
                    isFavorite: function(id, t) { return false; },
                    getFavoriteMovies: function() { window.webkit.messageHandlers.AndroidApp.postMessage({ method: 'getFavoriteMovies' }); },
                    getFavoriteSeries: function() { window.webkit.messageHandlers.AndroidApp.postMessage({ method: 'getFavoriteSeries' }); },
                    getFavoriteChannels: function() { window.webkit.messageHandlers.AndroidApp.postMessage({ method: 'getFavoriteChannels' }); },
                    searchContent: function(q) { window.webkit.messageHandlers.AndroidApp.postMessage({ method: 'searchContent', args: [q] }); },
                    getEpg: function(id) { window.webkit.messageHandlers.AndroidApp.postMessage({ method: 'getEpg', args: [id] }); },
                    baixarEInstalarApk: function(url) { /* não aplicável no ios */ },
                    exitApp: function() { window.webkit.messageHandlers.AndroidApp.postMessage({ method: 'exitApp' }); }
                };
            ";
            var script = new WKUserScript(new NSString(jsCode), WKUserScriptInjectionTime.AtDocumentStart, true);
            userContentController.AddUserScript(script);

            var config = new WKWebViewConfiguration
            {
                UserContentController = userContentController
            };

            config.Preferences.SetValueForKey(NSObject.FromObject(true), new NSString("allowFileAccessFromFileURLs"));

            _webView = new WKWebView(View.Bounds, config)
            {
                AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight
            };

            View.AddSubview(_webView);

            // Carregar o index.html dos assets
            var url = NSBundle.MainBundle.GetUrlForResource("Resources/assets/index", "html");
            if (url != null)
            {
                _webView.LoadFileUrl(url, url.AbsoluteUrl);
            }
            else
            {
                Console.WriteLine("Erro: index.html não encontrado!");
            }
        }
    }
}
