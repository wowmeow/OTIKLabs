using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Collections;

namespace Project.Coders
{
    public class ByteInterval
    {
        public byte symbol;
        public BigInteger bottomLine;
        public BigInteger upperLine;
        public ByteInterval(byte outSymbol, BigInteger outBottomLine, BigInteger outUpperLine)
        {
            this.symbol = outSymbol;
            this.bottomLine = outBottomLine;
            this.upperLine = outUpperLine;
        }

        public static ByteInterval SearchByte(byte searchingSymbol, List<ByteInterval> byteIntervals)
        {
            foreach (ByteInterval item in byteIntervals)
            {
                if (item.symbol == searchingSymbol)
                    return item;
            }
            return null;
        }
    }

    public class ArithmeticCoder : ICoder
    {
        public byte[] Decode(byte[] file, byte[] info, int oldSize)
        {
            byte[] btOldSize = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                btOldSize[i] = info[2 + i];
            }
            oldSize = BitConverter.ToInt32(btOldSize);
            int count = (int)info[0];
            if (count == 0)
            {
                if (info[1] == 0x01)
                    count = 256;
            }
            List<byte> byteStr = new List<byte>();
            for (int i = 6 + count; i < info.Count(); i++)
            {
                byteStr.Add(info[i]);
            }
            string[] arrStr = System.Text.Encoding.ASCII.GetString(byteStr.ToArray()).Split(" ", StringSplitOptions.RemoveEmptyEntries);
            List<FrequancyOfByte> frequancyOfBytes = new List<FrequancyOfByte>();           
            for(int i = 0; i < count; i++)
            {
                frequancyOfBytes.Add(new FrequancyOfByte(info[i+6], int.Parse(arrStr[i])));
            }
            FrequancyOfByte.Sort(ref frequancyOfBytes);
            BigInteger range = 1;
            for(int i = 0; i < oldSize; i++)
            {
                range *= oldSize;
            }
            List<ByteInterval> byteIntervals = new List<ByteInterval>();
            BigInteger tempLine = new BigInteger(0);
            foreach (FrequancyOfByte item in frequancyOfBytes)
            {
                BigInteger tempBigInt1 = BigInteger.Parse(item.GetFreq().ToString());
                if (tempLine == 0)
                {
                    byteIntervals.Add(new ByteInterval(item.GetByte(), tempLine, item.GetFreq()));
                    tempLine = item.GetFreq();
                }
                else
                {
                    byteIntervals.Add(new ByteInterval(item.GetByte(), tempLine, tempLine + item.GetFreq()));
                    tempLine = tempLine + item.GetFreq();
                }
            }

             BitArray fileBitsCode = new BitArray(file);
                string fileCode = "";
                for(int j = 0; j < fileBitsCode.Length; j++)
                {
                    if(fileBitsCode[j] == true)
                    {
                        fileCode += "1";
                    }
                    else
                    {
                        fileCode += "0";
                    }
                }
            
            List<ByteInterval> byteIntervalsForWork = new List<ByteInterval>();
            foreach (ByteInterval item in byteIntervals)
            {
                byteIntervalsForWork.Add(new ByteInterval(item.symbol, item.bottomLine, item.upperLine));
            }
            BigInteger oldLowLine = 0;
            BigInteger oldUpperLine = 0;

            List<ByteInterval> byteIntervalsBinary = new List<ByteInterval>();
            BigInteger leftSide = 0;
            BigInteger rightSide = range;
            range /= oldSize;
            foreach(ByteInterval item in byteIntervalsForWork)
            {
                item.bottomLine *= range;
                item.upperLine *= range;               
            }
            List<byte> oldFile = new List<byte>();

            for(int i = 0; i < oldSize; i++)
            {
                foreach (ByteInterval item in byteIntervalsForWork)
                {
                    string leftCode = GetCode(item.bottomLine, leftSide, rightSide, fileCode.Length, 1);
                    string rightCode = GetCode(item.upperLine, leftSide, rightSide, fileCode.Length, 1);
                    byteIntervalsBinary.Add(new ByteInterval(item.symbol, BigInteger.Parse(leftCode), BigInteger.Parse(rightCode)));
                }
                              
                    byte currentSymbol;
                foreach(ByteInterval item in byteIntervalsBinary)
                {
                    if(BigInteger.Parse(fileCode) >= item.bottomLine && BigInteger.Parse(fileCode) <= item.upperLine)
                    {
                        oldFile.Add(item.symbol);
                        currentSymbol = item.symbol;
                        oldLowLine = ByteInterval.SearchByte(item.symbol, byteIntervalsForWork).bottomLine;
                        oldUpperLine = ByteInterval.SearchByte(item.symbol, byteIntervalsForWork).upperLine;
                        break;
                    }
                }
                int k = 0;
                foreach (ByteInterval bt in byteIntervalsForWork)
                {                   
                    bt.bottomLine = oldLowLine  + byteIntervals[k].bottomLine * (oldUpperLine-oldLowLine) / oldSize ;
                    bt.upperLine = oldLowLine + byteIntervals[k].upperLine * (oldUpperLine - oldLowLine) / oldSize ;
                    k++;
                }
                byteIntervalsBinary.Clear();

            }        
           return oldFile.ToArray();
        }

        public (byte[] blob, byte[] info) Encode(byte[] file)
        {

            
            
            List<byte> info = new List<byte>() { 0x00, 0x00, 0x00, 0x00 };
            List<FrequancyOfByte> frequancyOfBytes = new List<FrequancyOfByte>();
            foreach (byte item in file)
            {
                FrequancyOfByte.AddingFreq(item, ref frequancyOfBytes);
            }
            FrequancyOfByte.Sort(ref frequancyOfBytes);

            List<ByteInterval> byteIntervals = new List<ByteInterval>();
            BigInteger tempLine = new BigInteger(0);

            foreach (FrequancyOfByte item in frequancyOfBytes)
            {
                BigInteger tempBigInt1 = BigInteger.Parse(item.GetFreq().ToString());
                if (tempLine == 0)
                {
                    byteIntervals.Add(new ByteInterval(item.GetByte(), tempLine, item.GetFreq()));
                    tempLine = item.GetFreq();
                }
                else
                {
                    byteIntervals.Add(new ByteInterval(item.GetByte(), tempLine, tempLine + item.GetFreq()));
                    tempLine = tempLine + item.GetFreq();
                }
            }

            List<ByteInterval> byteIntervalsForWork = new List<ByteInterval>();
            foreach (ByteInterval item in byteIntervals)
            {
                byteIntervalsForWork.Add(new ByteInterval(item.symbol, item.bottomLine, item.upperLine));
            }
            BigInteger oldLowLine = 0;
            BigInteger oldUpperLine = 0;          
            int countOfIteraions = 0;
            BigInteger range = 1;
            foreach (byte item in file)
            {
                range *= file.Length;
                ByteInterval tempByteInterval = ByteInterval.SearchByte(item, byteIntervalsForWork);
                countOfIteraions++;
                if (countOfIteraions == file.Length)
                {                   
                    oldLowLine = tempByteInterval.bottomLine;
                    oldUpperLine = tempByteInterval.upperLine;
                    break;
                }               
                oldLowLine = tempByteInterval.bottomLine;
                oldUpperLine = tempByteInterval.upperLine;
                int i = 0;
                foreach (ByteInterval bt in byteIntervalsForWork)
                {
                    //oldLowLine = bt.bottomLine;
                    //oldUpperLine = bt.upperLine;
                    bt.bottomLine = oldLowLine * file.Length + byteIntervals[i].bottomLine * (oldUpperLine - oldLowLine);
                    bt.upperLine = oldLowLine * file.Length + byteIntervals[i].upperLine * (oldUpperLine - oldLowLine);
                    i++;
                }
                
               
            }
           
          
            string leftSideStr = GetCode(oldLowLine, new BigInteger(0), range, null, 1);
            string rightSideStr = GetCode(oldUpperLine, new BigInteger(0), range, null, 1);
            string ans = "";
            int index = 0;
            while(leftSideStr[index] >= rightSideStr[index])
            {
                index++;
            }
            ans = rightSideStr.Substring(0, index + 1);
            List<bool> boolCode = new List<bool>();
            foreach(char sym in ans)
            {
                if (sym == '0')
                    boolCode.Add(false);
                else
                    boolCode.Add(true);
            }
            int countBitsToAdd = Math.Abs((boolCode.Count() / 8 + 1) * 8 - boolCode.Count());
            if (countBitsToAdd < 8)
            {
                for (int i = 0; i < countBitsToAdd; i++)
                {
                    boolCode.Add(false);
                }
            }
            BitArray fileInBits = new BitArray(boolCode.ToArray());
            byte[] newFile = new byte[(fileInBits.Length) / 8];
            fileInBits.CopyTo(newFile, 0);
           

           
            info[2] = BitConverter.GetBytes(frequancyOfBytes.Count())[0];
            if (frequancyOfBytes.Count() == 256)
                info[3] = (0x01);
            else
                info[3] = (0x00);
            byte[] oldSize = BitConverter.GetBytes(file.Length);
            foreach (byte it in oldSize)
                info.Add(it);
            foreach (FrequancyOfByte item in frequancyOfBytes)
            {
                info.Add(item.GetByte());
            }
            string freqs = "";
           
            foreach (FrequancyOfByte item in frequancyOfBytes)
            {
                freqs += item.GetFreq().ToString() + " ";
            }
            
            byte[] byteString = System.Text.Encoding.ASCII.GetBytes(freqs);
            foreach(byte bt in byteString)
            {
                info.Add(bt);
            }
            var infoSize = info.Count - 2;
            var infoSizeBytes = BitConverter.GetBytes(infoSize);
            info[0] = infoSizeBytes[0];
            info[1] = infoSizeBytes[1];
            double theorySize = 0;
            foreach (FrequancyOfByte item in frequancyOfBytes)
            {
                theorySize += -(Math.Log2(Convert.ToDouble(item.GetFreq()) / file.Length) * Convert.ToDouble(item.GetFreq()));
            }
            return (newFile, info.ToArray());
        }

        public static string GetCode(BigInteger number, BigInteger leftSide, BigInteger rightSide, int? countOfNumbers, int scaling)
        {
            string outPutCode  = "";
            BigInteger middle;
            BigInteger temp = 1;
            for(int i = 0; i < scaling; i++)
            {
                temp *= 2;
            }
            temp *= scaling;
            //if(rightSide % 2 != 0)
            //{
                number *= temp;
                leftSide *= temp;
                rightSide *= temp;
           // }
            if (countOfNumbers != null)
            {
                while (countOfNumbers > 0)
                {
                    middle = (rightSide - leftSide) / 2 + leftSide;
                    if (number >= middle)
                    {
                        outPutCode += "1";
                        leftSide = middle;
                    }
                    else
                    {
                        outPutCode += "0";
                        rightSide = middle;
                    }
                    countOfNumbers--;
                }            
            }
            else
            {
                while(rightSide - leftSide > 1)
                {
                    middle = (rightSide - leftSide) / 2 + leftSide;
                    if (number >= middle)
                    {
                        outPutCode += "1";
                        leftSide = middle;
                    }
                    else
                    {
                        outPutCode += "0";
                        rightSide = middle;
                    }
                }
            }
            return outPutCode;
        }
    }
}
