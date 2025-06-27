using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NoDev.Infinity.Network
{
    internal static class Extensions
    {
        internal static async Task<T> ToObjectAsync<T>(this WebResponse response)
        {
            if (response.ContentType != "application/json")
            {
                throw new ArgumentException(@"Web response must be of type application/json.", "response");
            }

            return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(await response.ToByteArrayAsync()));
        }

        internal static async Task<byte[]> ToByteArrayAsync(this WebResponse response)
        {
            var ms = new MemoryStream();

            var stream = response.GetResponseStream();

            if (stream == null)
            {
                throw new Exception("Server did not respond with a body.");
            }

            await stream.CopyToAsync(ms);

            stream.Close();
            ms.Close();

            return ms.ToArray();
        }

        internal static string ToUrlEncodedString(this IDictionary<string, string> dict)
        {
            var sb = new StringBuilder();

            foreach (var pair in dict)
            {
                sb.Append(WebUtility.UrlEncode(pair.Key));
                sb.Append('=');
                sb.Append(WebUtility.UrlEncode(pair.Value));
            }

            sb.Remove(sb.Length - 1, 1);

            return sb.ToString();
        }
    }
}
