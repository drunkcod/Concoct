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
    }
}
