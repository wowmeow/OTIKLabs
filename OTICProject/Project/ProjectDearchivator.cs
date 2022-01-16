using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project.Coders;

namespace Project
{
    public class ProjectDearchivator
    {
        private readonly byte[] _archive;
        private readonly List<(byte[] blob, string filename)> _files;
        
        public ProjectDearchivator(string fileName)
        {
            _archive = File.ReadAllBytes(fileName);

            List<byte> signature = new List<byte>();
            foreach (var octet in Header.PositionSignature)
            {
                signature.Add(_archive[octet]);
            }
            Project.CheckSignature(signature.ToArray());
            Project.CheckVersion(_archive[Header.PositionVersion], _archive[Header.PositionSubversion]);

            _files = new List<(byte[] blob, string filename)>();
        }

        public void GenerateFilesFromArchive(string generatePath)
        {
            int offset = Header.HeaderSize;
            for (int _ = 0; _ < _archive[Header.PositionFilesCount]; _++)
            {
                List<byte> oldSizeBytes = new List<byte>();
                foreach (var octet in BodyFileHeader.PositionOldSize)
                {
                    oldSizeBytes.Add(_archive[offset + octet]);
                }

                while (oldSizeBytes.Count < sizeof(int))
                {
                    oldSizeBytes.Add(0x00);
                }

                offset += BodyFileHeader.PositionOldSize.Length;

                List<byte> newSizeBytes = new List<byte>();
                foreach (var octet in BodyFileHeader.PositionNewSize)
                {
                    newSizeBytes.Add(_archive[offset + octet - BodyFileHeader.PositionNewSize.Length]);
                }
                while (newSizeBytes.Count < sizeof(int))
                {
                    newSizeBytes.Add(0x00);
                }

                offset += BodyFileHeader.PositionNewSize.Length;

                int fileNameSize = _archive[offset];
                offset += 1;

                List<byte> fileName = new List<byte>();
                for (int i = 0; i < fileNameSize; i++)
                {
                    fileName.Add(_archive[offset + i]);
                }

                offset += fileNameSize;

                byte[] byteCoders = new byte[Header.PositionOfAlghorithms.Length];
                for(int i = 0; i < Header.PositionOfAlghorithms.Length; i++)
                {
                    byteCoders[i] = _archive[Header.PositionOfAlghorithms[i]];
                }
                List<ICoder> listCoders = Project.GetCoders(byteCoders);
                bool firstly = true;
                List<byte[]> infos = new List<byte[]>(); 
                foreach (var coder in listCoders)
                {
                    var infoSizeArr = new List<byte>();
                    for (int i = 0; i < BodyFileHeader.CoderInfoSize; i++)
                    {
                        infoSizeArr.Add(_archive[offset + i]);
                    }

                    offset += BodyFileHeader.CoderInfoSize;

                    var infoSize = new List<byte>();

                    while (infoSizeArr.Count < sizeof(int))
                    {
                        infoSizeArr.Add(0x00);
                    }
                    for (int i = 0; i < BitConverter.ToInt32(infoSizeArr.ToArray()); i++)
                    {
                        infoSize.Add(_archive[offset + i]);
                    }

                    offset += BitConverter.ToInt32(infoSizeArr.ToArray());

                    /*List<byte> blob = new List<byte>();
                    for (int i = offset; i < offset + BitConverter.ToInt32(newSizeBytes.ToArray()); i++)
                    {
                        blob.Add(_archive[i]);
                    }

                    offset += BitConverter.ToInt32(newSizeBytes.ToArray());

                    var decodedFile = coder.Decode(blob.ToArray(), infoSize.ToArray(), BitConverter.ToInt32(oldSizeBytes.ToArray()));
                    _files.Add((decodedFile, Encoding.UTF8.GetString(fileName.ToArray())));*/
                    infos.Add(infoSize.ToArray());
                }

                List<byte> blob = new List<byte>();
                for (int i = offset; i < offset + BitConverter.ToInt32(newSizeBytes.ToArray()); i++)
                {
                    blob.Add(_archive[i]);
                }

                offset += BitConverter.ToInt32(newSizeBytes.ToArray());

                for (int i = infos.Count - 1; i >= 0; i--)
                {
                    blob = listCoders[i].Decode(blob.ToArray(), infos[i], BitConverter.ToInt32(oldSizeBytes.ToArray())).ToList();
                }
                _files.Add((blob.ToArray(), Encoding.UTF8.GetString(fileName.ToArray())));

                foreach (var file in _files)
                {
                    var dirs = file.filename.Split(new char[] { '\\', '/' });

                    var path = generatePath;
                    for (int i = 0; i < dirs.Length - 1; i++)
                    {
                        path = Path.Combine(path, dirs[i]);
                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }
                    }
                    File.WriteAllBytes(Path.Combine(path, dirs.Last()), file.blob);
                }
            }
        }
    }
}
