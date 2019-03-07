using Gecko;
using Gecko.Net;
using Gecko.Observers;
using Shipwreck.HlsDownloader.Properties;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;

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

                App.Current?.Dispatcher?.BeginInvoke((Action)(() => _MainWindow.RequestList.Add(rvm)));
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