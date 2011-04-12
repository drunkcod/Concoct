using System;
using System.IO;

namespace Concoct.IO
{
    public static class StreamExtensions
    {
        const int DefaultBufferSize = 4096;

        public static void CopyTo(this Stream self, Stream target) {
            var buffer = new byte[DefaultBufferSize];

            for(int bytes; (bytes = self.Read(buffer, 0, buffer.Length)) != 0;)
                target.Write(buffer, 0, bytes);
        }

        public static void CopyTo(this Stream self, Action<byte[], int, int> target) {
            var buffer = new byte[DefaultBufferSize];

            for(int bytes; (bytes = self.Read(buffer, 0, buffer.Length)) != 0;)
                target(buffer, 0, bytes);
        }

        public static void CopyTo(this Stream self, Action<byte[]> target) {
            var buffer = new byte[DefaultBufferSize];

            for(int bytes; (bytes = self.Read(buffer, 0, buffer.Length)) != 0;)
                target(buffer);
        }
}
}
