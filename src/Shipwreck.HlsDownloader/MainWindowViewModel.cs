using Gecko;
using Gecko.Net;
using Gecko.Observers;
using Microsoft.Win32;
using Shipwreck.HlsDownloader.Properties;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

namespace Shipwreck.HlsDownloader
{
    public class MainWindowViewModel : ViewModelBase
    {
        internal MainWindow Window
            => App.Current?.MainWindow is MainWindow mw && mw.DataContext == this ? mw : null;

        #region WebView

        private class ResponseObserver : BaseHttpRequestResponseObserver
        {
            private readonly MainWindowViewModel _MainWindow;

            internal ResponseObserver(MainWindowViewModel mainWindow)
            {
                _MainWindow = mainWindow;
            }

            protected override void Request(HttpChannel channel)
            {
                var rvm = new RequestViewModel(channel, false);
                App.Current?.Dispatcher?.BeginInvoke((Action)(() => _MainWindow.RequestList.Add(rvm)));
            }

            protected override void Response(HttpChannel channel)
            {
                var rvm = new RequestViewModel(channel, true);
                base.Response(channel);

                App.Current?.Dispatcher?.BeginInvoke((Action)(() =>
                {
                    var l = _MainWindow.RequestList.LastOrDefault(e => e.Url == rvm.Url);
                    if (l != null)
                    {
                        l.StatusCode = rvm.StatusCode;
                        l.ContentType = rvm.ContentType;
                        l.ContentLength = rvm.ContentLength;
                        l.ResponseHeaders = rvm.ResponseHeaders;
                        _MainWindow._Requests?.Refresh();
                        return;
                    }
                    _MainWindow.RequestList.Add(rvm);
                }));
            }
        }

        private class ModifyObserver : BaseHttpModifyRequestObserver
        {
            private readonly MainWindowViewModel _MainWindow;

            internal ModifyObserver(MainWindowViewModel mainWindow)
            {
                _MainWindow = mainWindow;
            }

            protected override void ObserveRequest(HttpChannel channel)
            {
                base.ObserveRequest(channel);

                var url = channel.Uri;

                var tc = channel.CastToTraceableChannel();
                var s = new StreamListenerTee();
                s.Completed += (sender, e) =>
                {
                    if (sender is StreamListenerTee slt)
                    {
                        var data = slt.GetCapturedData();

                        if (data?.Length <= 1024)
                        {
                            App.Current?.Dispatcher?.BeginInvoke((Action)(() =>
                            {
                                var l = _MainWindow.RequestList.LastOrDefault(r => r.Url == url);
                                if (l != null)
                                {
                                    l.Data = data;
                                    return;
                                }
                            }));
                        }
                    }
                };
                tc.SetNewListener(s);
            }
        }

        public void AddObserver()
        {
            ObserverService.AddObserver(new ModifyObserver(this));
            ObserverService.AddObserver(new ResponseObserver(this));
        }

        #region Url

        private string _Url = Settings.Default.CurrentUrl;

        public string Url
        {
            get => _Url;
            set => SetProperty(ref _Url, value);
        }

        #endregion Url

        #region Requests

        private ObservableCollection<RequestViewModel> _RequestList;

        internal ObservableCollection<RequestViewModel> RequestList
        {
            get
            {
                return _RequestList ?? (_RequestList = new ObservableCollection<RequestViewModel>());
            }
        }

        #endregion Requests

        #region Requests

        private ICollectionView _Requests;

        public ICollectionView Requests
        {
            get
            {
                if (_Requests == null)
                {
                    _Requests = CollectionViewSource.GetDefaultView(RequestList);
                    OnContentTypeFilterChanged();
                }
                return _Requests;
            }
        }

        #endregion Requests

        #region SelectedRequest

        private RequestViewModel _SelectedRequest;

        public RequestViewModel SelectedRequest
        {
            get => _SelectedRequest;
            set => SetProperty(ref _SelectedRequest, value);
        }

        #endregion SelectedRequest

        #region ContentTypeFilter

        private string _ContentTypeFilter = "application/x-mpegurl";

        public string ContentTypeFilter
        {
            get => _ContentTypeFilter;
            set
            {
                if (SetProperty(ref _ContentTypeFilter, value))
                {
                    OnContentTypeFilterChanged();
                }
            }
        }

        private void OnContentTypeFilterChanged()
        {
            if (_Requests != null)
            {
                var t = _ContentTypeFilter;
                if (string.IsNullOrEmpty(t))
                {
                    _Requests.Filter = _ => true;
                }
                else
                {
                    _Requests.Filter = o => o is RequestViewModel m
                        && m.ContentType?.StartsWith(t, StringComparison.InvariantCultureIgnoreCase) == true;
                }
                _Requests.Refresh();
            }
        }

        #endregion ContentTypeFilter

        public string[] KnownTypes
            => new[] {
                "application/x-mpegurl", "video/MP2T", "audio/MP2T", "application/vnd.apple.mpegURL",
                "text/html", "text/css", "application/javascript", "application/json", "application/octet-stream"
            };

        #region SetRequestToDownloaderCommand

        private Command _SetRequestToDownloaderCommand;

        public ICommand SetRequestToDownloaderCommand
            => _SetRequestToDownloaderCommand
            ?? (_SetRequestToDownloaderCommand = new Command(() =>
            {
                if (_SelectedRequest != null)
                {
                    M3u8Url = _SelectedRequest.Url.ToString();
                    IsDownloaderShown = true;
                }
            }));

        #endregion SetRequestToDownloaderCommand

        internal void OnFrameLoadStart(string url)
        {
        }

        internal void OnFrameLoadEnd(string url)
        {
            if (url != _Url)
            {
                var sd = Settings.Default;
                sd.CurrentUrl = Url = url;
                sd.Save();
            }
        }

        #endregion WebView

        #region Downloader

        #region IsDownloaderShown

        private bool _IsDownloaderShown;

        public bool IsDownloaderShown
        {
            get => _IsDownloaderShown;
            set => SetProperty(ref _IsDownloaderShown, value);
        }

        #endregion IsDownloaderShown

        #region M3u8Url

        private string _M3u8Url;

        public string M3u8Url
        {
            get => _M3u8Url;
            set => SetProperty(ref _M3u8Url, value);
        }

        #endregion M3u8Url

        #region DestinationM3u8

        private string _DestinationM3u8 = Settings.Default.DestinationM3u8;

        public string DestinationM3u8
        {
            get => _DestinationM3u8;
            set => SetProperty(ref _DestinationM3u8, value);
        }

        #endregion DestinationM3u8

        #region IsDownloaderShown

        private ObservableCollection<string> _DownloaderLog;

        public ObservableCollection<string> DownloaderLog
            => _DownloaderLog ?? (_DownloaderLog = new ObservableCollection<string>());

        #endregion IsDownloaderShown

        #region BrowseDestinationCommand

        private Command _BrowseDestinationCommand;

        public ICommand BrowseDestinationCommand
            => _BrowseDestinationCommand
            ?? (_BrowseDestinationCommand = new Command(async () =>
            {
                var fd = new SaveFileDialog();
                fd.Filter = "HLS Playlist|*.m3u8";
                fd.FileName = DestinationM3u8;
                fd.InitialDirectory = string.IsNullOrEmpty(DestinationM3u8) ? fd.InitialDirectory : Path.GetDirectoryName(Path.Combine(DestinationM3u8));

                if (fd.ShowDialog(Window) == true)
                {
                    DestinationM3u8 = fd.FileName;
                }
            }));

        #endregion IsDownloaderShown

        #region DownloadCommand

        private Command _DownloadCommand;

        public ICommand DownloadCommand
            => _DownloadCommand
            ?? (_DownloadCommand = new Command(async () =>
            {
                var src = _M3u8Url;
                var dest = _DestinationM3u8;

                if (!string.IsNullOrEmpty(src) && !string.IsNullOrEmpty(dest))
                {
                    try
                    {
                        dest = Path.GetFullPath(dest);

                        var sd = Settings.Default;
                        sd.DestinationM3u8 = dest;
                        sd.Save();

                        _DownloaderLog?.Clear();

                        await DownloadAsync(src, dest);
                    }
                    catch (Exception ex)
                    {
                        DownloaderLog.Add(ex.ToString());
                    }
                }
            }));

        #endregion DownloadCommand

        private async Task<bool> DownloadAsync(string src, string dest)
        {
            var dir = Path.GetDirectoryName(dest);

            if (!Directory.Exists(dir))
            {
                DownloaderLog.Add(string.Format("Creating directory \"{0}\".", dir));
                Directory.CreateDirectory(dir);
            }

            async Task<byte[]> getDataAsync(HttpClient client, Uri u)
            {
                var lr1 = RequestList.LastOrDefault(e => e.Url == u)?.Data;
                if (lr1 != null)
                {
                    DownloaderLog.Add(string.Format("Read {0} from cache.", u));
                    return lr1;
                }

                var req = new HttpRequestMessage(HttpMethod.Get, u);
                var lr = RequestList.LastOrDefault(e => e.Url.Host == u.Host);
                if (lr != null)
                {
                    foreach (var kv in lr.RequestHeaders)
                    {
                        if ("origin".Equals(kv.Key, StringComparison.InvariantCultureIgnoreCase)
                            || "referer".Equals(kv.Key, StringComparison.InvariantCultureIgnoreCase))
                        {
                            req.Headers.Add(kv.Key, kv.Value);
                        }
                    }
                    req.Headers.Add("Cookie", lr.GetCookie());
                }

                DownloaderLog.Add(string.Format("Getting {0}.", u));

                using (var tr = await client.SendAsync(req))
                {
                    DownloaderLog[DownloaderLog.Count - 1] = string.Format("GET {0} ended in {1:D} {1:G}.", u, tr.StatusCode);
                    tr.EnsureSuccessStatusCode();

                    return await tr.Content.ReadAsByteArrayAsync();
                }
            }

            using (var hc = new HttpClient())
            {
                var lu = new Uri(src);

                var ts = 0;
                var key = 0;

                using (var sr = new StringReader(Encoding.UTF8.GetString(await getDataAsync(hc, lu))))
                using (var sw = new StringWriter())
                {
                    for (var l = sr.ReadLine(); l != null; l = sr.ReadLine())
                    {
                        if (!string.IsNullOrEmpty(l))
                        {
                            if (l[0] != '#')
                            {
                                // ts

                                var tu = new Uri(lu, l);

                                var data = await getDataAsync(hc, tu);

                                var fn = (ts++).ToString("000000\".ts\"");

                                var fp = Path.Combine(dir, fn);
                                DownloaderLog.Add(string.Format("Writing \"{0}\".", fp));

                                File.WriteAllBytes(fp, data);
                                sw.WriteLine(fn);
                            }
                            else
                            {
                                if (l.StartsWith("#EXT-X-KEY:"))
                                {
                                    var m = Regex.Match(l, "([\\s,:]?)URI=(\"(?<u>[^\"]+)\"|(?<u>[^\"][^,]*))");
                                    if (m.Success)
                                    {
                                        var tu = new Uri(lu, m.Groups["u"].Value);
                                        var data = await getDataAsync(hc, tu);

                                        var fn = (key++).ToString("\"key\"0\".bin\"");

                                        var fp = Path.Combine(dir, fn);
                                        DownloaderLog.Add(string.Format("Writing \"{0}\".", fp));

                                        File.WriteAllBytes(fp, data);
                                        sw.WriteLine(l.Substring(0, m.Index) + m.Groups[1].Value + $"URI=\"{fn}\"" + l.Substring(m.Index + m.Length));
                                        continue;
                                    }
                                }

                                sw.WriteLine(l);
                            }
                        }
                    }

                    DownloaderLog.Add(string.Format("Writing \"{0}\".", dest));
                    File.WriteAllText(dest, sw.ToString());
                }
            }

            return true;
        }

        #endregion Downloader
    }
}