using System.IO;

namespace Concoct.IO
{
    public static class StreamExtensions
    {
        const int DefaultBufferSize = 4096;

        public static void CopyTo(this Stream self, Stream target) {
            CopyTo(self, target, DefaultBufferSize);
        }

        public static void CopyTo(this Stream self, Stream target, int bufferSize) {
            var buffer = new byte[bufferSize];
            for(int bytes; (bytes = self.Read(buffer, 0, bufferSize)) != 0;)
                target.Write(buffer, 0, bytes);
        }

        public static int ReadBlock(this Stream self, byte[] buffer, int offset, int count) {
            var bytesRead = 0;
            while(bytesRead != count) {
                var block = self.Read(buffer, offset + bytesRead, count - bytesRead);
                if(block == 0)
                    break;
                bytesRead += block;
            }
            return bytesRead;
        }
    }
}
