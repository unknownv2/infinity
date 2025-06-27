using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace NoDev.Infinity.Network.Api
{
    internal class RequestSignatureCalculator : IRequestSignatureCalculator
    {
        private static readonly string[] SignableHeaders =
        {
            "Content-Type",
            "Date"
        };

        static RequestSignatureCalculator()
        {
            for (var x = 0; x < SignableHeaders.Length; x++)
            {
                SignableHeaders[x] = SignableHeaders[x].ToLowerInvariant();
            }
        }

        private readonly byte[] _key;

        internal RequestSignatureCalculator(byte[] key)
        {
            _key = key;
        }

        public byte[] CalculateRequestSignature(HttpWebRequest request, IDictionary<string, string> urlParams)
        {
            var sb = new StringBuilder(256);
            sb.Append(request.Method);
            sb.Append("\n");
            sb.Append(request.RequestUri.AbsolutePath);
            sb.Append("\n");

            AppendHeaders(sb, request.Headers);

            if (urlParams != null)
            {
                AppendQueryString(sb, urlParams);
            }

#if DEBUG
            var str = sb.ToString().Replace("\n", "\r\n");
#endif

            var bytes = Encoding.ASCII.GetBytes(sb.ToString());

            var sha = new HMACSHA256(_key);
            sha.TransformFinalBlock(bytes, 0, bytes.Length);
            var hash = sha.Hash;
            sha.Clear();

            return hash;
        }

        private static void AppendHeaders(StringBuilder sb, WebHeaderCollection headers)
        {
            var keys = headers.AllKeys;

            Array.Sort(keys, string.CompareOrdinal);

            foreach (var k in keys)
            {
                var low = k.ToLowerInvariant();

                // x-inf-
                if (SignableHeaders.Contains(low) || (low.Length > 6 && low.Substring(0, 6) == "x-inf-"))
                {
                    sb.Append(low);
                    sb.Append(':');
                    sb.Append(headers[k]);
                    sb.Append("\n");
                }
            }
        }

        private static void AppendQueryString(StringBuilder sb, IDictionary<string, string> urlParams)
        {
            if (urlParams.Count == 0)
            {
                return;
            }

            var keys = urlParams.Keys.ToList();
            keys.Sort(string.CompareOrdinal);

            foreach (var key in keys)
            {
                sb.Append(WebUtility.UrlEncode(key));
                sb.Append('=');
                sb.Append(WebUtility.UrlEncode(urlParams[key]));
                sb.Append('&');
            }

            sb.Remove(sb.Length - 1, 1);
        }
    }
}
