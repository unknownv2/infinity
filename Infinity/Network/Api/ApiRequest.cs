using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NoDev.Infinity.Security;

namespace NoDev.Infinity.Network.Api
{
    internal class ApiRequest
    {
        internal readonly ApiState State;

        internal WebHeaderCollection Headers
        {
            get { return HttpRequest.Headers; }
        }

        protected readonly HttpWebRequest HttpRequest;

        private readonly IDictionary<string, string> _urlParams; 

        internal ApiRequest(ApiState state, string method, string url, IDictionary<string, string> urlParams = null)
        {
            _urlParams = urlParams;

            if (urlParams != null)
            {
                url += "?" + urlParams.ToUrlEncodedString();
            }

            HttpRequest = WebRequest.CreateHttp(url);
            HttpRequest.Method = method;

#if DEBUG
            HttpRequest.Headers["X-Inf-Client"] = "0.0.0.0";
            HttpRequest.Timeout = 60 * 60 * 1000;
#else
            HttpRequest.Headers["X-Inf-Client"] = Program.Version.ToString();
#endif

            State = state;
        }

        internal async Task<HttpWebResponse> SendUrlEncodedFormAsync(IDictionary<string, string> formData)
        {
            HttpRequest.ContentType = "application/x-www-form-urlencoded";

            if (formData.Count == 0)
                return await SendAsync();

            return await SendAsync(Encoding.UTF8.GetBytes(formData.ToUrlEncodedString()));
        }

        internal async Task<HttpWebResponse> SendJsonAsync(object body)
        {
            if (body == null)
            {
                throw new ArgumentNullException("body");
            }

            HttpRequest.ContentType = "application/json";

            return await SendAsync(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(body)));
        }

        internal virtual async Task<HttpWebResponse> SendAsync(byte[] body = null)
        {
            if (body == null)
            {
                body = new byte[0];
            }

            HttpRequest.ContentLength = body.Length;
            HttpRequest.Date = DateTime.UtcNow;

            if (State.SessionId != null)
            {
                HttpRequest.Headers["X-Inf-Session-ID"] = State.SessionId;
            }

            HttpRequest.Headers["X-Inf-Content-Hash"] = Convert.ToBase64String(CalculateContentHash(body));

            HttpRequest.Headers["X-Inf-Signature"] = Convert.ToBase64String(
                new RequestSignatureCalculator(State.AuthKey).CalculateRequestSignature(HttpRequest, _urlParams)
            );

            if (body.Length != 0)
            {
                var requestStream = await HttpRequest.GetRequestStreamAsync();

                await requestStream.WriteAsync(body, 0, body.Length);
                await requestStream.FlushAsync();

                requestStream.Close();
            }
            
            /* --- RESPONSE --- */

            HttpWebResponse httpResponse;

            try
            {
                httpResponse = (HttpWebResponse)await HttpRequest.GetResponseAsync();
            }
            catch (WebException e)
            {
                httpResponse = (HttpWebResponse)e.Response;
            }

            if (httpResponse.Headers["X-Inf-Session-ID"] != null)
            {
                State.SessionId = httpResponse.Headers["X-Inf-Session-ID"];
            }

            return httpResponse;
        }

        private static byte[] CalculateContentHash(byte[] body)
        {
            var sha = new InternalSHA256Managed();
            sha.TransformFinalBlock(body, 0, body.Length);
            return sha.Hash;
        }
    }
}
