namespace NoDev.__Server
{
    internal sealed class InternalSHA256Managed
    {
        private readonly byte[] _buffer;
        private long            _count; // Number of bytes in the hashed message
        private readonly uint[] _stateSHA256;
        private readonly uint[] _w;
  
        //
        // internal constructors
        //
 
        internal InternalSHA256Managed()
        {
            _stateSHA256 = new uint[8];
            _buffer = new byte[64];
            _w = new uint[64];
 
            InitializeState();
        }
 
        //
        // internal methods
        //
  
        /*internal void Initialize() {
            InitializeState();
  
            // Zeroize potentially sensitive information.
            Clear(_buffer, 0, _buffer.Length);
            Clear(_w, 0, _w.Length);
        }*/

        //
        // private methods
        //
 
        private void InitializeState() {
            _count = 0;
 
            _stateSHA256[0] = 0x6a09e667;
            _stateSHA256[1] = 0xbb67ae85;
            _stateSHA256[2] = 0x3c6ef372;
            _stateSHA256[3] = 0xa54ff53a;
            _stateSHA256[4] = 0x510e527f;
            _stateSHA256[5] = 0x9b05688c;
            _stateSHA256[6] = 0x1f83d9ab;
            _stateSHA256[7] = 0x5be0cd19;
        }
 
        /* SHA256 block update operation. Continues an SHA message-digest
           operation, processing another message block, and updating the
           context.
           */
 
        private unsafe void _HashData(byte[] partIn, int ibStart, int cbSize)
        {
            var partInLen = cbSize;
            var partInBase = ibStart;
 
            /* Compute length of buffer */
            var bufferLen = (int) (_count & 0x3f);
 
            /* Update number of bytes */
            _count += partInLen;
 
            fixed (uint* stateSHA256 = _stateSHA256) {
                fixed (byte* buffer = _buffer) {
                    fixed (uint* expandedBuffer = _w) {
                        if ((bufferLen > 0) && (bufferLen + partInLen >= 64)) {
                            InternalBlockCopy(partIn, partInBase, _buffer, bufferLen, 64 - bufferLen);
                            partInBase += (64 - bufferLen);
                            partInLen -= (64 - bufferLen);
                            SHATransform(expandedBuffer, stateSHA256, buffer);
                            bufferLen = 0;
                        }
  
                        /* Copy input to temporary buffer and hash */
                        while (partInLen >= 64) {
                            InternalBlockCopy(partIn, partInBase, _buffer, 0, 64);
                            partInBase += 64;
                            partInLen -= 64;
                            SHATransform(expandedBuffer, stateSHA256, buffer);
                        }
 
                        if (partInLen > 0) {
                            InternalBlockCopy(partIn, partInBase, _buffer, bufferLen, partInLen);
                        }
                    }
                }
            }
        }
 
        /* SHA256 finalization. Ends an SHA256 message-digest operation, writing
           the message digest.
           */
  
        private byte[] _EndHash()
        {
            /* Compute padding: 80 00 00 ... 00 00 <bit count="">
             */
  
            var padLen = 64 - (int)(_count & 0x3f);
            if (padLen <= 8)
                padLen += 64;
 
            var pad = new byte[padLen];
            pad[0] = 0x80;
 
            //  Convert count to bit count
            var bitCount = _count * 8;
 
            pad[padLen-8] = (byte) ((bitCount >> 56) & 0xff);
            pad[padLen-7] = (byte) ((bitCount >> 48) & 0xff);
            pad[padLen-6] = (byte) ((bitCount >> 40) & 0xff);
            pad[padLen-5] = (byte) ((bitCount >> 32) & 0xff);
            pad[padLen-4] = (byte) ((bitCount >> 24) & 0xff);
            pad[padLen-3] = (byte) ((bitCount >> 16) & 0xff);
            pad[padLen-2] = (byte) ((bitCount >> 8) & 0xff);
            pad[padLen-1] = (byte) ((bitCount >> 0) & 0xff);
 
            /* Digest padding */
            _HashData(pad, 0, pad.Length);

            var hash = new byte[32]; // HashSizeValue = 256
 
            /* Store digest */
            DwordToBigEndian(hash, _stateSHA256, 8);
 
            _hashValue = hash;
            return hash;
        }
  
        private readonly static uint[] K = {
            0x428a2f98, 0x71374491, 0xb5c0fbcf, 0xe9b5dba5,
            0x3956c25b, 0x59f111f1, 0x923f82a4, 0xab1c5ed5,
            0xd807aa98, 0x12835b01, 0x243185be, 0x550c7dc3,
            0x72be5d74, 0x80deb1fe, 0x9bdc06a7, 0xc19bf174,
            0xe49b69c1, 0xefbe4786, 0x0fc19dc6, 0x240ca1cc,
            0x2de92c6f, 0x4a7484aa, 0x5cb0a9dc, 0x76f988da,
            0x983e5152, 0xa831c66d, 0xb00327c8, 0xbf597fc7,
            0xc6e00bf3, 0xd5a79147, 0x06ca6351, 0x14292967,
            0x27b70a85, 0x2e1b2138, 0x4d2c6dfc, 0x53380d13,
            0x650a7354, 0x766a0abb, 0x81c2c92e, 0x92722c85,
            0xa2bfe8a1, 0xa81a664b, 0xc24b8b70, 0xc76c51a3,
            0xd192e819, 0xd6990624, 0xf40e3585, 0x106aa070,
            0x19a4c116, 0x1e376c08, 0x2748774c, 0x34b0bcb5,
            0x391c0cb3, 0x4ed8aa4a, 0x5b9cca4f, 0x682e6ff3,
            0x748f82ee, 0x78a5636f, 0x84c87814, 0x8cc70208,
            0x90befffa, 0xa4506ceb, 0xbef9a3f7, 0xc67178f2
        };

        private static void DwordToBigEndian(byte[] block, uint[] x, int digits)
        {
            var index1 = 0;
            var index2 = 0;
            while (index1 < digits)
            {
                block[index2] = (byte)(x[index1] >> 24 & 0xff);
                block[index2 + 1] = (byte)(x[index1] >> 16 & 0xff);
                block[index2 + 2] = (byte)(x[index1] >> 8 & 0xff);
                block[index2 + 3] = (byte)(x[index1] & 0xff);
                ++index1;
                index2 += 4;
            }
        }

        private static unsafe void DwordFromBigEndian(uint* x, int digits, byte* block)
        {
            var index1 = 0;
            var index2 = 0;
            while (index1 < digits)
            {
                x[index1] = (uint)(block[index2] << 24 | block[index2 + 1] << 16 | block[index2 + 2] << 8) | block[index2 + 3];
                ++index1;
                index2 += 4;
            }
        }
 
        private static unsafe void SHATransform(uint* expandedBuffer, uint* state, byte* block)
        {
            var a = state[0];
            var b = state[1];
            var c = state[2];
            var d = state[3];
            var e = state[4];
            var f = state[5];
            var g = state[6];
            var h = state[7];
  
            // fill in the first 16 bytes of W.
            DwordFromBigEndian(expandedBuffer, 16, block);
            SHA256Expand(expandedBuffer);
 
            /* Apply the SHA256 compression function */
            // We are trying to be smart here and avoid as many copies as we can
            // The perf gain with this method over the straightforward modify and shift
            // forward is >= 20%, so it's worth the pain
            for (var j = 0; j < 64; ) {
                var t1 = h + Sigma_1(e) + Ch(e,f,g) + K[j] + expandedBuffer[j];
                var ee = d + t1;
                var aa = t1 + Sigma_0(a) + Maj(a,b,c);
                j++;
 
                t1 = g + Sigma_1(ee) + Ch(ee,e,f) + K[j] + expandedBuffer[j];
                var ff = c + t1;
                var bb = t1 + Sigma_0(aa) + Maj(aa,a,b);
                j++;
 
                t1 = f + Sigma_1(ff) + Ch(ff,ee,e) + K[j] + expandedBuffer[j];
                var gg = b + t1;
                var cc = t1 + Sigma_0(bb) + Maj(bb,aa,a);
                j++;
  
                t1 = e + Sigma_1(gg) + Ch(gg,ff,ee) + K[j] + expandedBuffer[j];
                var hh = a + t1;
                var dd = t1 + Sigma_0(cc) + Maj(cc,bb,aa);
                j++;
  
                t1 = ee + Sigma_1(hh) + Ch(hh,gg,ff) + K[j] + expandedBuffer[j];
                h = aa + t1;
                d = t1 + Sigma_0(dd) + Maj(dd,cc,bb);
                j++;
 
                t1 = ff + Sigma_1(h) + Ch(h,hh,gg) + K[j] + expandedBuffer[j];
                g = bb + t1;
                c = t1 + Sigma_0(d) + Maj(d,dd,cc);
                j++;
 
                t1 = gg + Sigma_1(g) + Ch(g,h,hh) + K[j] + expandedBuffer[j];
                f = cc + t1;
                b = t1 + Sigma_0(c) + Maj(c,d,dd);
                j++;
  
                t1 = hh + Sigma_1(f) + Ch(f,g,h) + K[j] + expandedBuffer[j];
                e = dd + t1;
                a = t1 + Sigma_0(b) + Maj(b,c,d);
                j++;
            }
 
            state[0] += a;
            state[1] += b;
            state[2] += c;
            state[3] += d;
            state[4] += e;
            state[5] += f;
            state[6] += g;
            state[7] += h;
        }

        private static uint RotateRight(uint x, int n)
        {
            return (((x) >> (n)) | ((x) << (32-(n))));
        }
 
        private static uint Ch(uint x, uint y, uint z) {
            return ((x & y) ^ ((x ^ 0xffffffff) & z));
        }
 
        private static uint Maj(uint x, uint y, uint z) {
            return ((x & y) ^ (x & z) ^ (y & z));
        }
  
        private static uint sigma_0(uint x) {
            return (RotateRight(x,7) ^ RotateRight(x,18) ^ (x >> 3));
        }
 
        private static uint sigma_1(uint x) {
            return (RotateRight(x,17) ^ RotateRight(x,19) ^ (x >> 10));
        }
  
        private static uint Sigma_0(uint x) {
            return (RotateRight(x,2) ^ RotateRight(x,13) ^ RotateRight(x,22));
        }
 
        private static uint Sigma_1(uint x) {
            return (RotateRight(x,6) ^ RotateRight(x,11) ^ RotateRight(x,25));
        }
 
        /* This function creates W_16,...,W_63 according to the formula
           W_j <- sigma_1(W_{j-2}) + W_{j-7} + sigma_0(W_{j-15}) + W_{j-16};
        */
  
        private static unsafe void SHA256Expand(uint* x)
        {
            for (int i = 16; i < 64; i++) {
                x[i] = sigma_1(x[i-2]) + x[i-7] + sigma_0(x[i-15]) + x[i-16];
            }
        }


        // HashAlgorithm

        private bool _bDisposed;
        private byte[] _hashValue;
        private int _state;

        private static void InternalBlockCopy(byte[] src, int srcOffset, byte[] dst, int dstOffset, int byteCount)
        {
            for (var x = 0; x < byteCount; x++)
                dst[dstOffset + x] = src[srcOffset + x];
        }

        private static void Clear(byte[] array, int index, int length)
        {
            for (var x = 0; x < length; x++)
                array[index + x] = 0;
        }

        /*private static void Clear(uint[] array, int index, int length)
        {
            for (var x = 0; x < length; x++)
                array[index + x] = 0;
        }*/

        //
        // internal properties
        //

        internal byte[] Hash
        {
            get
            {
                if (_bDisposed)
                    return null;
                if (_state != 0)
                    return null;
                return (byte[])_hashValue.Clone();
            }
        }

        /*internal byte[] ComputeHash(Stream inputStream)
        {
            if (_bDisposed)
                return null;

            // Default the buffer size to 4K.
            var buffer = new byte[4096];
            int bytesRead;
            do
            {
                bytesRead = inputStream.Read(buffer, 0, 4096);
                if (bytesRead > 0)
                {
                    HashCore(buffer, 0, bytesRead);
                }
            } while (bytesRead > 0);

            HashValue = HashFinal();
            var tmp = (byte[])HashValue.Clone();
            Initialize();
            return (tmp);
        }*/

        /*internal byte[] ComputeHash(byte[] buffer)
        {
            if (_bDisposed)
                return null;

            // Do some validation
            if (buffer == null) return null;

            HashCore(buffer, 0, buffer.Length);
            _hashValue = HashFinal();
            var tmp = (byte[])_hashValue.Clone();
            Initialize();
            return (tmp);
        }*/

        /*internal byte[] ComputeHash(byte[] buffer, int offset, int count)
        {
            if (_bDisposed)
                return null;

            // Do some validation
            if (buffer == null) return null;
            if (offset < 0) return null;
            if (count < 0 || (count > buffer.Length)) return null;
            if ((buffer.Length - count) < offset) return null;

            HashCore(buffer, offset, count);
            _hashValue = HashFinal();
            var tmp = (byte[])_hashValue.Clone();
            Initialize();
            return (tmp);
        }*/

        // We implement TransformBlock and TransformFinalBlock here
        internal int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount)
        {
            if (_bDisposed)
                return -1;

            // Do some validation, we let BlockCopy do the destination array validation
            if (inputBuffer == null) return -1;
            if (inputOffset < 0) return -1;
            if (inputCount < 0 || (inputCount > inputBuffer.Length)) return -1;
            if ((inputBuffer.Length - inputCount) < inputOffset) return -1;

            // Change the State value
            _state = 1;
            _HashData(inputBuffer, inputOffset, inputCount);
            return inputCount;
        }

        internal int TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
        {
            if (_bDisposed)
                return -1;

            // Do some validation
            if (inputBuffer == null) return -1;
            if (inputOffset < 0) return -1;
            if (inputCount < 0 || (inputCount > inputBuffer.Length)) return -1;
            if ((inputBuffer.Length - inputCount) < inputOffset) return -1;

            _HashData(inputBuffer, inputOffset, inputCount);
            _hashValue = _EndHash();
            // reset the State value
            _state = 0;
            return inputCount;
        }

        internal void Clear()
        {
            if (_hashValue != null)
                Clear(_hashValue, 0, _hashValue.Length);
            _hashValue = null;
            _bDisposed = true;
            //GC.SuppressFinalize(this);
        }
    }
}
