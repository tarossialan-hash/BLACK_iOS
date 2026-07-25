using System;
using System.Linq;
using System.Collections.Generic;
using Foundation;
using UIKit;
using WebKit;
using Newtonsoft.Json;
using BlackIOS.Data.Remote;
using BlackIOS.Data.Local;
using Newtonsoft.Json.Serialization;

namespace BlackIOS
{
    public class WebAppInterface : NSObject, IWKScriptMessageHandler
    {
        private readonly UIViewController _controller;
        public WKWebView WebView { get; set; }
        private readonly IptvApiClient _apiClient;
        private readonly DatabaseRepository _db;
        private readonly SyncManager _syncManager;

        public WebAppInterface(UIViewController controller)
        {
            _controller = controller;
            
            _apiClient = new IptvApiClient(HttpClientFactory.CreateUnsafeClient());
            _db = new DatabaseRepository();
            _syncManager = new SyncManager(_apiClient, _db);
        }

        private void EvaluateJavascript(string js)
        {
            InvokeOnMainThread(() => {
                WebView?.EvaluateJavaScript(js, null);
            });
        }
        
        private string EscapeJs(string str)
        {
            if (string.IsNullOrEmpty(str)) return "";
            return str.Replace("\\", "\\\\").Replace("'", "\\'").Replace("\n", "\\n").Replace("\r", "");
        }
        
        private string ToJson(object obj)
        {
            return JsonConvert.SerializeObject(obj, new JsonSerializerSettings 
            { 
                ContractResolver = new DefaultContractResolver { NamingStrategy = new SnakeCaseNamingStrategy() }
            });
        }

        public void DidReceiveScriptMessage(WKUserContentController userContentController, WKScriptMessage message)
        {
            try
            {
                var dict = message.Body as NSDictionary;
                if (dict == null) return;
                
                var method = dict["method"]?.ToString();
                var args = dict["args"] as NSArray;
                
                switch (method)
                {
                    case "login":
                        if (args != null && args.Count >= 2)
                        {
                            var user = args.GetItem<NSString>(0).ToString();
                            var pass = args.GetItem<NSString>(1).ToString();
                            
                            // Em .NET iOS o async void em event handler é permitido para fire and forget
                            System.Threading.Tasks.Task.Run(async () => 
                            {
                                await _syncManager.SyncWithServerAsync(user, pass, 
                                    onProgress: (progress, status) => {
                                        var percent = (int)(progress * 100);
                                        EvaluateJavascript($"javascript:updateSyncProgress({percent}, '{EscapeJs(status)}')");
                                    },
                                    onError: (errorMsg) => {
                                        EvaluateJavascript($"javascript:onLoginError('{EscapeJs(errorMsg)}')");
                                    },
                                    onComplete: () => {
                                        NSUserDefaults.StandardUserDefaults.SetString(user, "username");
                                        NSUserDefaults.StandardUserDefaults.SetString(pass, "password");
                                        NSUserDefaults.StandardUserDefaults.Synchronize();
                                        EvaluateJavascript($"javascript:onLoginSuccess('{EscapeJs(user)}')");
                                    });
                            });
                        }
                        break;
                        
                    case "getUsername":
                        var u = NSUserDefaults.StandardUserDefaults.StringForKey("username") ?? "";
                        EvaluateJavascript($"javascript:window.AndroidAppCallback_getUsername('{EscapeJs(u)}')");
                        break;
                        
                    case "getPassword":
                        var p = NSUserDefaults.StandardUserDefaults.StringForKey("password") ?? "";
                        EvaluateJavascript($"javascript:window.AndroidAppCallback_getPassword('{EscapeJs(p)}')");
                        break;

                    case "getBannerItems":
                        EvaluateJavascript("javascript:window.AndroidAppCallback_getBannerItems('[]')");
                        break;
                        
                    case "getLiveCategories":
                        System.Threading.Tasks.Task.Run(async () => {
                            var cats = await _db.GetCategoriesByType("live");
                            EvaluateJavascript($"javascript:window.AndroidAppCallback_getLiveCategories('{EscapeJs(ToJson(cats))}')");
                        });
                        break;
                        
                    case "getVodCategories":
                        System.Threading.Tasks.Task.Run(async () => {
                            var cats = await _db.GetCategoriesByType("movie");
                            EvaluateJavascript($"javascript:window.AndroidAppCallback_getVodCategories('{EscapeJs(ToJson(cats))}')");
                        });
                        break;
                        
                    case "getSeriesCategories":
                        System.Threading.Tasks.Task.Run(async () => {
                            var cats = await _db.GetCategoriesByType("series");
                            EvaluateJavascript($"javascript:window.AndroidAppCallback_getSeriesCategories('{EscapeJs(ToJson(cats))}')");
                        });
                        break;

                    case "getLiveChannels":
                        if (args != null && args.Count > 0)
                        {
                            var catId = args.GetItem<NSString>(0).ToString();
                            System.Threading.Tasks.Task.Run(async () => {
                                var streams = await _db.GetLiveStreamsByCategory(catId);
                                EvaluateJavascript($"javascript:window.AndroidAppCallback_getLiveChannels('{EscapeJs(ToJson(streams))}')");
                            });
                        }
                        break;
                        
                    case "getVodList":
                        if (args != null && args.Count > 0)
                        {
                            var catId = args.GetItem<NSString>(0).ToString();
                            System.Threading.Tasks.Task.Run(async () => {
                                var movies = await _db.GetMoviesByCategory(catId);
                                EvaluateJavascript($"javascript:window.AndroidAppCallback_getVodList('{EscapeJs(ToJson(movies))}')");
                            });
                        }
                        break;
                        
                    case "getSeriesList":
                        if (args != null && args.Count > 0)
                        {
                            var catId = args.GetItem<NSString>(0).ToString();
                            System.Threading.Tasks.Task.Run(async () => {
                                var series = await _db.GetSeriesByCategory(catId);
                                EvaluateJavascript($"javascript:window.AndroidAppCallback_getSeriesList('{EscapeJs(ToJson(series))}')");
                            });
                        }
                        break;

                    case "getRecentMovies":
                        System.Threading.Tasks.Task.Run(async () => {
                            var movies = await _db.GetRecentMovies();
                            EvaluateJavascript($"javascript:window.AndroidAppCallback_getRecentMovies('{EscapeJs(ToJson(movies))}')");
                        });
                        break;

                    case "getRecentSeries":
                        System.Threading.Tasks.Task.Run(async () => {
                            var series = await _db.GetRecentSeries();
                            EvaluateJavascript($"javascript:window.AndroidAppCallback_getRecentSeries('{EscapeJs(ToJson(series))}')");
                        });
                        break;

                    case "searchContent":
                        if (args != null && args.Count > 0)
                        {
                            var query = args.GetItem<NSString>(0).ToString();
                            System.Threading.Tasks.Task.Run(async () => {
                                var movies = await _db.SearchMovies(query);
                                var series = await _db.SearchSeries(query);
                                var combined = new { movies, series };
                                EvaluateJavascript($"javascript:window.AndroidAppCallback_searchContent('{EscapeJs(ToJson(combined))}')");
                            });
                        }
                        break;

                    case "getFormatoLive":
                        EvaluateJavascript("javascript:window.AndroidAppCallback_getFormatoLive('ts')");
                        break;

                    case "isTv":
                        EvaluateJavascript("javascript:window.AndroidAppCallback_isTv(false)");
                        break;

                    case "getAppVersion":
                        EvaluateJavascript("javascript:window.AndroidAppCallback_getAppVersion('1.0.0 (iOS)')");
                        break;

                    case "getStreamUrl":
                        if (args != null && args.Count > 0)
                        {
                            var id = args.GetItem<NSNumber>(0).Int32Value;
                            var u2 = NSUserDefaults.StandardUserDefaults.StringForKey("username");
                            var p2 = NSUserDefaults.StandardUserDefaults.StringForKey("password");
                            var url = $"http://bkpac.cc/live/{u2}/{p2}/{id}.ts";
                            EvaluateJavascript($"javascript:window.AndroidAppCallback_getStreamUrl('{EscapeJs(url)}')");
                        }
                        break;

                    case "getVodStreamUrl":
                        if (args != null && args.Count > 1)
                        {
                            var id = args.GetItem<NSNumber>(0).Int32Value;
                            var ext = args.GetItem<NSString>(1).ToString();
                            var u3 = NSUserDefaults.StandardUserDefaults.StringForKey("username");
                            var p3 = NSUserDefaults.StandardUserDefaults.StringForKey("password");
                            var url = $"http://bkpac.cc/movie/{u3}/{p3}/{id}.{ext}";
                            EvaluateJavascript($"javascript:window.AndroidAppCallback_getVodStreamUrl('{EscapeJs(url)}')");
                        }
                        break;

                    case "getSeriesStreamUrl":
                        if (args != null && args.Count > 1)
                        {
                            var id = args.GetItem<NSNumber>(0).Int32Value;
                            var ext = args.GetItem<NSString>(1).ToString();
                            var u4 = NSUserDefaults.StandardUserDefaults.StringForKey("username");
                            var p4 = NSUserDefaults.StandardUserDefaults.StringForKey("password");
                            var url = $"http://bkpac.cc/series/{u4}/{p4}/{id}.{ext}";
                            EvaluateJavascript($"javascript:window.AndroidAppCallback_getSeriesStreamUrl('{EscapeJs(url)}')");
                        }
                        break;

                    case "getSeriesInfo":
                        if (args != null && args.Count > 0)
                        {
                            var seriesId = args.GetItem<NSNumber>(0).Int32Value;
                            var u5 = NSUserDefaults.StandardUserDefaults.StringForKey("username");
                            var p5 = NSUserDefaults.StandardUserDefaults.StringForKey("password");
                            System.Threading.Tasks.Task.Run(async () => {
                                try {
                                    var info = await _apiClient.GetSeriesInfoAsync(u5, p5, seriesId);
                                    EvaluateJavascript($"javascript:window.AndroidAppCallback_getSeriesInfo('{EscapeJs(ToJson(info))}')");
                                } catch {
                                    EvaluateJavascript($"javascript:window.AndroidAppCallback_getSeriesInfo('{{}}')");
                                }
                            });
                        }
                        break;

                    case "getEpg":
                        if (args != null && args.Count > 0)
                        {
                            var streamId = args.GetItem<NSNumber>(0).Int32Value;
                            var u6 = NSUserDefaults.StandardUserDefaults.StringForKey("username");
                            var p6 = NSUserDefaults.StandardUserDefaults.StringForKey("password");
                            System.Threading.Tasks.Task.Run(async () => {
                                try {
                                    var epg = await _apiClient.GetShortEpgAsync(u6, p6, streamId);
                                    EvaluateJavascript($"javascript:window.AndroidAppCallback_getEpg('{EscapeJs(ToJson(epg))}')");
                                } catch {
                                    EvaluateJavascript($"javascript:window.AndroidAppCallback_getEpg('{{}}')");
                                }
                            });
                        }
                        break;

                    case "playVod":
                        if (args != null && args.Count > 0)
                        {
                            var urlString = args.GetItem<NSString>(0).ToString();
                            InvokeOnMainThread(() => {
                                var nsUrl = NSUrl.FromString(urlString);
                                if (nsUrl != null)
                                {
                                    var player = new AVFoundation.AVPlayer(nsUrl);
                                    var playerViewController = new AVKit.AVPlayerViewController { Player = player };
                                    _controller.PresentViewController(playerViewController, true, () => player.Play());
                                }
                            });
                        }
                        break;
                        
                    case "tocarVideoNativo":
                        if (args != null && args.Count > 0)
                        {
                            var urlString = args.GetItem<NSString>(0).ToString();
                            InvokeOnMainThread(() => {
                                var nsUrl = NSUrl.FromString(urlString);
                                if (nsUrl != null)
                                {
                                    var player = new AVFoundation.AVPlayer(nsUrl);
                                    var playerViewController = new AVKit.AVPlayerViewController { Player = player };
                                    _controller.PresentViewController(playerViewController, true, () => player.Play());
                                }
                            });
                        }
                        break;

                    case "logout":
                        NSUserDefaults.StandardUserDefaults.RemoveObject("username");
                        NSUserDefaults.StandardUserDefaults.RemoveObject("password");
                        NSUserDefaults.StandardUserDefaults.Synchronize();
                        System.Threading.Tasks.Task.Run(async () => {
                            await _db.ClearAll();
                        });
                        break;

                    case "exitApp":
                        Environment.Exit(0);
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro no JS bridge: " + ex.Message);
            }
        }
    }
}
