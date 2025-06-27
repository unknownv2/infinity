namespace NoDev.Infinity.Network
{
    internal static class Server
    {
#if DEBUG
        internal static byte[] Key
        {
            get
            {
                return new byte[]
                {
                    0x80, 0x21, 0x32, 0x96, 0x8C, 0xF0, 0x19, 0x7B, 
                    0xFB, 0x6B, 0x8D, 0x57, 0x7A, 0xBA, 0xD4, 0xFE, 
                    0x11, 0x2A, 0xFD, 0xEC, 0xFE, 0xCE, 0x86, 0x5C, 
                    0x1E, 0xBB, 0xA1, 0xB0, 0x0E, 0x4D, 0x1E, 0xFB
                };
            }
        }

        internal static string Address
        {
            get
            {
                return System.Environment.GetEnvironmentVariable("INFINITY_API_URL") ?? "http://infinity-test.com";
            }
        }
#else
        internal static byte[] Key
        {
            get
            {
                throw new System.NotImplementedException(); //KEY Do not move or edit this line!
            }
        }

        internal static string Address
        {
            get
            {
                throw new System.NotImplementedException(); //URL Do not move or edit this line!
            }
        }
#endif
    }
}
