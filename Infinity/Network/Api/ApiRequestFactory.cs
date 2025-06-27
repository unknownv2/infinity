
using System.Collections.Generic;

namespace NoDev.Infinity.Network.Api
{
    internal static class ApiRequestFactory
    {
        private static readonly ApiState State = new ApiState(Server.Key);

        internal static ApiRequest Create(string method, string uri, IDictionary<string, string> urlParams = null)
        {
            return new ValidatedApiRequest(State, method, Server.Address + uri, urlParams);
        }

        internal static ApiRequest CreateUnvalidated(string method, string uri, IDictionary<string, string> urlParams = null)
        {
            return new ApiRequest(State, method, Server.Address + uri, urlParams);
        }
    }
}
