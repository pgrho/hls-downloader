using Gecko;
using Gecko.Net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Shipwreck.HlsDownloader
{
    public sealed class RequestViewModel : ViewModelBase
    {
        private readonly WeakReference<nsIHttpChannel> _Channel;
        internal RequestViewModel(HttpChannel channel, bool isResponse)
        {
            _Channel = new WeakReference<nsIHttpChannel>(channel.Instance);
            Method = channel.RequestMethod;
            Url = channel.Uri;

            RequestHeaders = Array.AsReadOnly(
                channel.GetRequestHeadersDict()
                .SelectMany(e => e.Value.Select(s => new KeyValuePair<string, string>(e.Key, s)))
                .ToArray());

            if (isResponse)
            {
                _StatusCode = (int)channel.ResponseStatus;
                _ContentType = channel.ContentType;
                _ContentLength = channel.ContentLength >= 0 ? channel.ContentLength : (long?)null;

                ResponseHeaders = Array.AsReadOnly(
                    channel.GetResponseHeadersDict()
                    .SelectMany(e => e.Value.Select(s => new KeyValuePair<string, string>(e.Key, s)))
                    .ToArray());
            }
        }

        internal nsIHttpChannel Channel
            => _Channel.TryGetTarget(out var r) ? r : null;

        public string Method { get; }
        public Uri Url { get; }

        private int? _StatusCode;
        private string _ContentType;
        private long? _ContentLength;

        public int? StatusCode
        {
            get => _StatusCode;
            internal set => SetProperty(ref _StatusCode, value);
        }
        public string ContentType
        {
            get => _ContentType;
            internal set => SetProperty(ref _ContentType, value);
        }
        public long? ContentLength
        {
            get => _ContentLength;
            internal set => SetProperty(ref _ContentLength, value);
        }

        internal ReadOnlyCollection<KeyValuePair<string, string>> RequestHeaders { get; }
        internal ReadOnlyCollection<KeyValuePair<string, string>> ResponseHeaders { get; set; }

        internal byte[] Data { get; set; }

        public string GetCookie()
        {
            var dic = new Dictionary<string, string>();

            if (ResponseHeaders != null)
            {
                foreach (var kv in ResponseHeaders)
                {
                    if (kv.Key.Equals("Set-Cookie", StringComparison.InvariantCultureIgnoreCase))
                    {
                        var c = kv.Value.Split(';')?.FirstOrDefault();
                        if (c?.Length > 0)
                        {
                            var cv = c.Split(new[] { '=' }, 2);
                            if (cv.Length == 2)
                            {
                                dic[cv[0].Trim()] = cv[1].Trim();
                            }
                        }
                    }
                }
            }
            if (dic.Count == 0)
            {
                foreach (var kv in RequestHeaders)
                {
                    if (kv.Key.Equals("Cookie", StringComparison.InvariantCultureIgnoreCase))
                    {
                        foreach (var c in kv.Value.Split(';'))
                        {
                            var cv = c.Split(new[] { '=' }, 2);
                            if (cv.Length == 2)
                            {
                                dic[cv[0].Trim()] = cv[1].Trim();
                            }
                        }
                    }
                }
            }

            return string.Join("; ", dic.Select(e => $"{e.Key}={e.Value }"));
        }
    }
}