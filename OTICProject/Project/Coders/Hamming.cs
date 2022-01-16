using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Coders
{
    public class Hamming: ICoder
    {
        public byte[] Decode(byte[] file, byte[] info, int oldSize)
        {
            var sizeOfTable = BitConverter.ToInt32(new byte[] { info[0], info[1], info[2], info[3] });
            var rows = BitConverter.ToInt32(new byte[] { info[4], info[5], info[6], info[7] });
            var bitsArr = new BitArray(file);

            var len = sizeOfTable / rows;
            var inputBits = new bool[rows, len];
            int q = 0;
            foreach (var bit in bitsArr)
            {
                inputBits[q / len, q % len]=(bool)bit;
                q++;
            }

            var inputFile = new List<bool>();
            foreach (var bit in new BitArray(file))
            {
                inputFile.Add((bool)bit);
            }

            var controlLine = new List<bool>();
            for (int i = 0; i < rows; i++)
            {
                var res = 0;
                for (int j = 0; j < len; j++)
                {
                    res += (inputFile[j] ? 1 : 0) * (inputBits[i, j] ? 1 : 0);
                }
                controlLine.Add(res % 2 == 1 ? true : false);
            }

            var bitTable = new BitArray(controlLine.ToArray());
            byte[] byteTable = new byte[bitTable.Length / 8 + 1];
            bitTable.CopyTo(byteTable, 0);

            if (controlLine.Contains(true))
            {
                var ind = BitConverter.ToInt32(byteTable);
                inputFile[ind - 1] = !inputFile[ind - 1];
            }

            for (int i = rows; i >= 1; i--)
            {
                inputFile.RemoveAt((int)(Math.Pow(2, i - 1) - 1));
            }
            bitTable = new BitArray(inputFile.ToArray());
            byteTable = new byte[bitTable.Length / 8 + 1];
            bitTable.CopyTo(byteTable, 0);

            var blob = new List<byte>();
            blob.AddRange(byteTable);

            return blob.ToArray();
        }

        public (byte[] blob, byte[] info) Encode(byte[] file)
        {
            List<byte> blob = new List<byte>();
            List<byte> info = new List<byte>() { 0x00, 0x00 };

            var bitsArr = new BitArray(file);
            var inputBits = new List<bool>();
            foreach (var bit in bitsArr)
            {
                inputBits.Add((bool)bit);
            }

            var controlCount = (int)Math.Ceiling(Math.Log2(inputBits.Count + 1));
            for (int i = 1; i <= controlCount; i++)
            {
                inputBits.Insert((int)(Math.Pow(2, i - 1) - 1), false);
            }

            var table = new bool[controlCount, inputBits.Count];

            for (int i = 0; i < controlCount; i++)
            {
                var id = 0;
                while (id < (int)Math.Pow(2, i) - 1)
                {
                    table[i, id] = false;
                    id++;
                }

                var doing = true;
                while (doing)
                {
                    for (int k = 0; k < (int)Math.Pow(2, i); k++)
                    {
                        if (id >= inputBits.Count)
                        {
                            doing = false;
                            break;
                        }
                        table[i, id] = true;
                        id++;
                    }
                    for (int k = 0; k < (int)Math.Pow(2, i); k++)
                    {
                        if (id >= inputBits.Count)
                        {
                            doing = false;
                            break;
                        }
                        table[i, id] = false;
                        id++;
                    }
                }
            }

            var controlLine = new List<bool>();

            for (int i = 0; i < controlCount; i++)
            {
                var res = 0;
                for (int j = 0; j < inputBits.Count; j++)
                {
                    res += (inputBits[j] ? 1 : 0) * (table[i, j] ? 1 : 0);
                }
                controlLine.Add(res % 2 == 1 ? true : false);
            }

            for (int i = 1; i <= controlCount; i++)
            {
                inputBits[(int)(Math.Pow(2, i - 1) - 1)] = controlLine[i - 1];
            }

            var listTable = new List<bool>();
            for(int i = 0; i < table.Length; i++)
            {
                listTable.Add(table[i / inputBits.Count, i % inputBits.Count]);
            }
            var bitTable = new BitArray(listTable.ToArray());
            byte[] byteTable = new byte[bitTable.Length / 8 + 1];
            bitTable.CopyTo(byteTable, 0);

            var bitInput = new BitArray(inputBits.ToArray());
            byte[] byteInput = new byte[bitInput.Length / 8 + 1];
            bitInput.CopyTo(byteInput, 0);

            var sizeOfTable = BitConverter.GetBytes(bitTable.Length);
            var rows = BitConverter.GetBytes(controlCount);

            info.AddRange(sizeOfTable);
            info.AddRange(rows);
            info.AddRange(byteTable);
            var infoSize = info.Count - 2;
            var infoSizeBytes = BitConverter.GetBytes(infoSize);
            info[0] = infoSizeBytes[0];
            info[1] = infoSizeBytes[1];

            return (byteInput.ToArray(), info.ToArray());
        }
    }
}
