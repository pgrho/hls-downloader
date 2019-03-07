using Gecko.Net;

namespace Shipwreck.HlsDownloader
{
    public sealed class RequestViewModel : ViewModelBase
    {
        internal RequestViewModel(HttpChannel channel)
        {
            Method = channel.RequestMethod;
            Url = channel.Uri.ToString();
            StatusCode = (int)channel.ResponseStatus;
            ContentType = channel.ContentType;
            ContentLength = channel.ContentLength >= 0 ? channel.ContentLength : (long?)null;
        }

        public string Method { get; }
        public string Url { get; }
        public int StatusCode { get; }
        public string ContentType { get; }
        public long? ContentLength { get; }
    }
}