using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Project.Coders;

namespace Project
{
    public class BodyFileHeader
    {
        public static readonly int PositionFileNameSize = 6;
        public static readonly int PositionFileName = 7;
        public static readonly int CoderInfoSize = 2;
        public static readonly int[] PositionOldSize = new[] { 0, 1, 2};
        public static readonly int[] PositionNewSize = new[] { 3, 4, 5 };

        private readonly List<byte> _blob;

        public BodyFileHeader(int oldSize, int newSize, string fileName)
        {
            _blob = new List<byte>();
            this.AddOldSize(oldSize)
                .AddNewSize(newSize)
                .AddFileNameSize(fileName)
                .AddFileName(fileName);
        }

        public byte[] Blob => _blob.ToArray();

        private BodyFileHeader AddOldSize(int oldSize)
        {
            List<byte> bytes = new List<byte>();
            bytes.AddRange(BitConverter.GetBytes(oldSize));

            for (int i = 0; i < PositionOldSize.Length; i++)
            {
                _blob.Add(bytes[i]);
            }
            return this;
        }

        private BodyFileHeader AddNewSize(int newSize)
        {
            List<byte> bytes = new List<byte>();
            bytes.AddRange(BitConverter.GetBytes(newSize));

            for (int i = 0; i < PositionNewSize.Length; i++)
            {
                _blob.Add(bytes[i]);
            }
            return this;
        }

        private BodyFileHeader AddFileNameSize(string fileName)
        {
            var fileNameSize = BitConverter.GetBytes(fileName.Length);
            _blob.Add(fileNameSize.First());
            return this;
        }

        private BodyFileHeader AddFileName(string fileName)
        {
            _blob.AddRange(Encoding.UTF8.GetBytes(fileName));
            return this;
        }
    }

    class BodyFile
    {
        private readonly List<byte> _blob;

        public BodyFile(FileInfo fileInfo, string path, byte[] byteCoders)
        {
            _blob = new List<byte>();

            //ICoder coder = new ArithmeticCoder();

            var file = File.ReadAllBytes(fileInfo.FullName);

            List<ICoder> listCoders = Project.GetCoders(byteCoders);

            /*var (encodedFile, coderInfo) = coder.Encode(file);
            var fileName = Path.Combine(path, fileInfo.Name);

            this.AddFileHeader(file.Length, encodedFile.Length, fileName)
                .AddFileInfo(coderInfo)
                .AddFileBlob(encodedFile);*/

            var fileBlob = file;
            var fileName = Path.Combine(path, fileInfo.Name);
            List<byte[]> coderInfos = new List<byte[]>();
            foreach (var coder in listCoders)
            {
                var (encodedFile, coderInfo) = coder.Encode(fileBlob);
                coderInfos.Add(coderInfo);
                fileBlob = encodedFile;
                /*if (addHeader)
                {
                    this.AddFileHeader(file.Length, encodedFile.Length, fileName);
                    addHeader = false;
                }

                this.AddFileInfo(coderInfo);
                f
                    //.AddFileBlob(encodedFile);*/
            }
            this.AddFileHeader(file.Length, fileBlob.Length, fileName);
            foreach(var coderInfo in coderInfos)
            {
                this.AddFileInfo(coderInfo);
            }
            this.AddFileBlob(fileBlob);
        }

        public byte[] Blob => _blob.ToArray();

        private BodyFile AddFileHeader(int oldSize, int newSize, string fileName)
        {
            _blob.AddRange(new BodyFileHeader(oldSize, newSize, fileName).Blob);
            return this;
        }

        private BodyFile AddFileBlob(byte[] fileBlob)
        {
            _blob.AddRange(fileBlob);
            return this;
        }

        private BodyFile AddFileInfo(byte[] info)
        {
            /*List<byte> bytes = new List<byte>();
            bytes.AddRange(BitConverter.GetBytes(info.Length));

            for (int i = 0; i < BodyFileHeader.CoderInfoSize; i++)
            {
                _blob.Add(bytes[i]);
            }*/
            
            _blob.AddRange(info);

            return this;
        }
    }
}
