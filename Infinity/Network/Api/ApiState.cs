
namespace NoDev.Infinity.Network.Api
{
    internal class ApiState
    {
        private readonly byte[] _initialKey;

        internal string SessionId;
        internal byte[] AuthKey;

        internal ApiState(byte[] initialKey)
        {
            _initialKey = initialKey;
            Reset();
        }

        internal void Reset()
        {
            AuthKey = _initialKey;
        }
    }
}
