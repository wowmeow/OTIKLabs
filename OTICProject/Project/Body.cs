using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Project
{
    class Body
    {
        private readonly List<byte> _blob;
        private readonly byte[] _coders;

        public Body(byte[] coders)
        {
            _blob = new List<byte>();
            _coders = coders;
        }

        public byte[] Blob => _blob.ToArray();

        public void AddBodyFile(FileInfo fileInfo, string path)
        {
            _blob.AddRange(new BodyFile(fileInfo, path, _coders).Blob);
        }
    }
}
