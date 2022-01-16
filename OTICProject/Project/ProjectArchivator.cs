using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace Project
{
    public class ProjectArchivator
    {
        private readonly Header _header;
        private readonly Body _body;

        public ProjectArchivator(List<string> filenames, byte[] coders)
        {
            _header = new Header(coders);
            _body = new Body(coders);

            foreach (var filename in filenames)
            {
                AddFile(filename, "");
            }
        }

        public string Extension => "kotic";

        public byte[] Blob()
        {
            List<byte> bytes = new List<byte>();
            bytes.AddRange(_header.Blob);
            bytes.AddRange(_body.Blob);
            return bytes.ToArray();
        } 

        public void GenerateArchive(string generatePath)
        {
            string filename = $"{generatePath}\\{Guid.NewGuid()}.{Extension}";

            var blob = Blob();
            UpdateArchiveSize(ref blob);

            File.WriteAllBytes(filename, blob);
        }

        private void UpdateArchiveSize(ref byte[] blob)
        {
            var blobSize = blob.Length;

            List<byte> size = new List<byte>(new byte[] { 0x00, 0x00, 0x00, 0x00 });
            var archiveSize = BitConverter.GetBytes(blobSize);
            Array.Reverse(archiveSize);
            size.AddRange(archiveSize);
            for (int i = 0; i < Header.PositionArchiveSize.Length; i++)
            {
                blob[Header.PositionArchiveSize[i]] = archiveSize[i];
            }
        }

        private void AddFile(string filename, string path)
        {
            FileInfo fileInfo = new FileInfo(filename);

            if (fileInfo.Attributes.HasFlag(FileAttributes.Directory))
            {
                var directory = new DirectoryInfo(filename);
                foreach (var file in directory.GetFiles())
                {
                    var filepath = Path.Combine(path, directory.Name);
                    AddFile(file.FullName, filepath);
                }
                foreach (var file in directory.GetDirectories())
                {
                    var filepath = Path.Combine(path, directory.Name);
                    AddFile(file.FullName, filepath);
                }
            }
            else
            {
                _body.AddBodyFile(fileInfo, path);
                _header.IncFilesCount();
            }
        }
    }
}
