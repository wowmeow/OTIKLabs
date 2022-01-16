using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Coders
{
    public class DefaultCoder: ICoder
    {
        public byte[] Decode(byte[] file, byte[] info, int oldSize)
        {
            if(info.Length != 0)
            {
                int mult = 0;
                List<byte> newFile = new List<byte>(file);
                newFile.RemoveAt((int)info[0]);
                for (int i = 1; i < info.Length; i++)
                { 
                    if((int)info[i] <= (int)info[i-1])
                    {
                        mult++;
                    }
                    newFile.RemoveAt((int)info[i] - i + mult*256);
                }

                return newFile.ToArray();
            }
            return file;
        }

        public (byte[] blob, byte[] info) Encode(byte[] file)
        {
            List<byte> blob = new List<byte>();
            List<byte> info = new List<byte>() {0x00, 0x00};
            
            int count = 0;
            int last = 0;
            byte index = 0x00;
                        
            for (int i = 0; i < file.Length; i++)
            {
                if (count == 0)
                {
                    var rnd = new Random((int)DateTime.Now.Ticks);
                    count = rnd.Next(1, 16);
                }

                if (i - last == count)
                {
                    info.Add((byte)blob.Count);
                    info.Add((byte) ((blob.Count + 1) % 256));
                    blob.Add(index);
                    blob.Add(BitConverter.GetBytes('q')[0]);
                    index += 0x01;
                    last = i;
                    count = 0;
                }

                blob.Add(file[i]);
            }
            //эти 4 строчки вставить в конце кода
            var infoSize = info.Count - 2;
            var infoSizeBytes = BitConverter.GetBytes(infoSize);

            info[0] = infoSizeBytes[0];
            info[1] = infoSizeBytes[1];

            return (blob.ToArray(), info.ToArray());
        }
    }
}
