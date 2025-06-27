using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace NoDev.Infinity.Network.Api
{
    internal class ValidatedApiRequest : ApiRequest
    {
        private const int RequiresValidationStatusCode = 433;

        internal ValidatedApiRequest(ApiState state, string method, string url, IDictionary<string, string> urlParams = null) : base(state, method, url, urlParams)
        {

        }

        internal override async Task<HttpWebResponse> SendAsync(byte[] body = null)
        {
            var result = await base.SendAsync(body);

            if ((int)result.StatusCode != RequiresValidationStatusCode)
            {
                return result;
            }

            await ClientValidator.ValidateAsync();

            // resend the original request
            result = await base.SendAsync(body);

            if ((int)result.StatusCode == RequiresValidationStatusCode)
            {
                throw new Exception("Failed to validate client.");
            }

            return result;
        }
    }
}
