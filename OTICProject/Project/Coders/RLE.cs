using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Coders
{
    public class RLE : ICoder
    {
        public byte[] Decode(byte[] file, byte[] info, int oldSize)
        {
            List<byte> blob = new List<byte>();
            int i = -1;
            int? cur = null;
            while (true)
            {
                i++;
                if(i == file.Length)
                {
                    break;
                }
                cur = file[i];

                if(cur > 127)
                {
                    cur = cur - 256;
                    i++;
                    for (int j = 0; j < -cur; j++)
                    {
                        blob.Add(file[i+j]);
                    }
                    i = i - (cur ?? 0) - 1;
                }
                else
                {
                    i++;
                    for(int j = 0; j < cur; j++)
                    {
                        blob.Add(file[i]);
                    }
                }
            }
            return blob.ToArray();
        }

        public (byte[] blob, byte[] info) Encode(byte[] file)
        {
            List<byte> blob = new List<byte>();
            List<byte> info = new List<byte>() { 0x00, 0x00 };

            List<FrequancyOfByte> frequancyOfBytes = new List<FrequancyOfByte>();
            foreach (byte item in file)
            {
                FrequancyOfByte.AddingFreq(item, ref frequancyOfBytes);
            }
            FrequancyOfByte.Sort(ref frequancyOfBytes);
            var count = 0;
            byte? value = 0x00;
            var disvalue = new List<byte>();
            int i = -1;
            while (true)
            {
                i++;
                if(i == file.Length)
                {
                    if(value == null)
                    {
                        blob.Add((byte)(0 - count));
                        blob.AddRange(disvalue);
                    }
                    else
                    {
                        blob.Add((byte)(count));
                        blob.Add(value ?? 0);
                    }
                    break;
                }

                if(count == 127)
                {
                    if (disvalue.Count != 0)
                    {
                        blob.Add((byte)(0 - count));
                        blob.AddRange(disvalue);
                    }
                    else
                    {
                        blob.Add((byte)(count));
                        blob.Add(value ?? 0);
                    }
                    count = 0;
                    value = null;
                    disvalue.Clear();

                }

                if(count == 0)
                {
                    value = file[i];
                    disvalue.Add(file[i]);
                    count++;
                    continue;
                }

                if(file[i] == value)
                {
                    count++;
                    disvalue.Clear();
                    continue;
                }

                if (disvalue.Count != 0)
                {
                    if (i + 1 != file.Length && file[i] != file[i + 1])
                    {
                        value = null;
                        disvalue.Add(file[i]);
                        count++;
                        continue;
                    }
                    else
                    {
                        blob.Add((byte)(0 - count));
                        blob.AddRange(disvalue);
                        value = file[i];
                        disvalue.Add(file[i]);
                        count = 1;
                        continue;
                    }
                }
                else{
                    if(file[i] != value)
                    {
                        blob.Add((byte)(count));
                        blob.Add(value ?? 0);
                        value = file[i];
                        disvalue.Add(file[i]);
                        count = 1;
                        continue;
                    }
                }
            }
            double theorySize = 0;
            foreach (FrequancyOfByte item in frequancyOfBytes)
            {
                theorySize += -(Math.Log2(Convert.ToDouble(item.GetFreq()) / file.Length) * Convert.ToDouble(item.GetFreq()));
            }
            return (blob.ToArray(), info.ToArray());
        }
    }
}
