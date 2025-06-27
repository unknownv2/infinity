using System.Collections.Generic;
using System.Net;

namespace NoDev.Infinity.Network.Api
{
    internal interface IRequestSignatureCalculator
    {
        byte[] CalculateRequestSignature(HttpWebRequest request, IDictionary<string, string> urlParams);
    }
}
