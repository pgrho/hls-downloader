using Gecko;
using Gecko.Net;
using Gecko.Observers;
using Shipwreck.HlsDownloader.Properties;
using System;
using System.Collections.ObjectModel;

namespace Shipwreck.HlsDownloader
{
    public class MainWindowViewModel : ViewModelBase
    {
        private class Observer : BaseHttpRequestResponseObserver
        {
            private readonly MainWindowViewModel _MainWindow;

            internal Observer(MainWindowViewModel mainWindow)
            {
                _MainWindow = mainWindow;
            }

            protected override void Response(HttpChannel channel)
            {
                var rvm = new RequestViewModel(channel);
                base.Response(channel);

                App.Current?.Dispatcher?.BeginInvoke((Action)(() => _MainWindow.Requests.Add(rvm)));
            }
        }

        public void AddObserver()
            => ObserverService.AddObserver(new Observer(this));

        #region Url

        private string _Url = Settings.Default.CurrentUrl;

        public string Url
        {
            get => _Url;
            set => SetProperty(ref _Url, value);
        }

        #endregion Url

        #region Requests

        private ObservableCollection<RequestViewModel> _Requests;

        public ObservableCollection<RequestViewModel> Requests
            => _Requests ?? (_Requests = new ObservableCollection<RequestViewModel>());

        #endregion Requests

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
    }
}