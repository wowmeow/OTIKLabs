using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Project
{
    public class Header
    {
        public static readonly int[] PositionSignature = {0, 1, 2, 3, 4};
        public static readonly int PositionVersion = 5;
        public static readonly int PositionSubversion = 6;
        public static readonly int PositionFilesCount = 11;
        public static readonly int[] PositionOfAlghorithms = { 8,9 };
        public static readonly int[] PositionArchiveSize = {12, 13, 14, 15};

        private readonly List<byte> _blob;

        public static int HeaderSize => 16;

        public Header(byte[] coders)
        {
            _blob = new List<byte>();
            this.AddSignature()
                .AddVersion()
                .AddSubversion()
                .AddReserve(1)
                .AddCoders(coders)
                .AddReserve(1)
                .AddFilesCount()
                .AddArchiveSize();
        }

        public byte[] Blob => _blob.ToArray();

        private Header AddSignature()
        {
            _blob.AddRange(Project.Signature);
            return this;
        }

        private Header AddVersion()
        {
            _blob.Add(Project.CurrentVersion);
            return this;
        }

        private Header AddSubversion()
        {
            _blob.Add(Project.CurrentSubversion);
            return this;
        }

        private Header AddReserve(int count)
        {
            for (int i = 0; i < count; i++)
            {
                _blob.Add(0x00);
            }
            return this;
        }

        private Header AddCoders(byte[] coders)
        {
            _blob.Add(coders[0]);
            _blob.Add(coders[1]);
            return this;
        }

        /// <summary>
        /// code without context
        /// </summary>
        /// <returns></returns>
        private Header AddCWC()
        {
            _blob.Add(0x00);
            return this;
        }

        /// <summary>
        /// code with context
        /// </summary>
        /// <returns></returns>
        private Header AddCC()
        {
            _blob.Add(0x00);
            return this;
        }

        /// <summary>
        /// anti-interference protection
        /// </summary>
        /// <returns></returns>
        private Header AddAIP()
        {
            _blob.Add(0x00);
            return this;
        }

        private Header AddFilesCount()
        {
            _blob.Add(0x00);
            return this;
        }

        private Header AddArchiveSize()
        {
            _blob.AddRange(new byte[] { 0x00, 0x00, 0x00, 0x00 });
            return this;
        }

        public void IncFilesCount()
        {
            _blob[PositionFilesCount] += 0x01;
        }
    }
}
