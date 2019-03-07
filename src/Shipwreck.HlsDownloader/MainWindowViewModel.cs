using Shipwreck.HlsDownloader.Properties;

namespace Shipwreck.HlsDownloader
{
    public class MainWindowViewModel : ViewModelBase
    {
        #region Url

        private string _Url = Settings.Default.CurrentUrl;

        public string Url
        {
            get => _Url;
            set => SetProperty(ref _Url, value);
        }

        #endregion Url
    }
}