using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.Numerics;

namespace Project.Coders
{
    public class FrequancyOfByte
    {
        private byte _byte;
        private int _freq;       
        public FrequancyOfByte(byte symbol, int freq)
        {
            this._byte = symbol;
            this._freq = freq;
        }
        public byte GetByte()
        {
            return _byte;
        }
       

        public int GetFreq()
        {
            return _freq;
        }
        

        public static void AddingFreq(byte serchingSymbol, ref List<FrequancyOfByte> frequancyOfBytes)
        {
            bool isFound = false;
            for (int i = 0; i < frequancyOfBytes.Count(); i++)
            {
                if (frequancyOfBytes[i]._byte.Equals(serchingSymbol))
                {
                    frequancyOfBytes[i]._freq += 1;
                    isFound = true;
                    break;
                }
            }
            if (!isFound)
            {
                frequancyOfBytes.Add(new FrequancyOfByte(serchingSymbol, 1));
            }
        }

        public static void Sort(ref List<FrequancyOfByte> frequancyOfBytes)
        {
            for (int i = 0; i < frequancyOfBytes.Count(); i++)
            {
                for (int j = i + 1; j < frequancyOfBytes.Count() ; j++)
                {
                    if (frequancyOfBytes[i]._freq < frequancyOfBytes[j]._freq)
                    {
                        FrequancyOfByte temp = frequancyOfBytes[i];
                        frequancyOfBytes[i] = frequancyOfBytes[j];
                        frequancyOfBytes[j] = temp;
                    }
                    else if (frequancyOfBytes[i]._freq == frequancyOfBytes[j]._freq)
                    {
                        if (frequancyOfBytes[i]._byte > frequancyOfBytes[j]._byte)
                        {
                            FrequancyOfByte temp = frequancyOfBytes[i];
                            frequancyOfBytes[i] = frequancyOfBytes[j];
                            frequancyOfBytes[j] = temp;
                        }
                    }
                }
            }

        }

        public static FrequancyOfByte SearchFreq(List<FrequancyOfByte> frequancyOfBytes, byte searchingSymbol)
        {
            foreach (FrequancyOfByte item in frequancyOfBytes)
            {
                if (item._byte == searchingSymbol)
                    return item;
            }
            return null;
        }

        
    }

    public class Shannon_FanoCoder : ICoder
    {

        
        public class CodeOfSymbol
        {
            private byte _byte;
            private string _code;

            public CodeOfSymbol(byte _byte)
            {
                this._byte = _byte;
                _code = "";
            }

            public CodeOfSymbol(byte _byte, string _code)
            {
                this._byte = _byte;
                this._code = _code;
            }
            public string GetCode()
            {
                return _code;
            }
            public byte GetByte()
            {
                return _byte;
            }

            public static void UpdateCode(List<FrequancyOfByte> frequancyOfBytes, ref List<CodeOfSymbol> codeOfSymbols, string putCode)
            {
                foreach(CodeOfSymbol item in codeOfSymbols)
                {
                    foreach (FrequancyOfByte var in frequancyOfBytes)
                    {
                        if (var.GetByte().Equals(item._byte))
                        {
                            item._code += putCode;
                        }
                    }
                }
            }
           
            

        }
        public byte[] Decode(byte[] file, byte[] info, int oldSize)
        {
            List<byte> newFile = new List<byte>();
            byte[] btOldSize = new byte[4];
            for(int i = 0; i < 4; i++)
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


            List<CodeOfSymbol> codeOfSymbols = new List<CodeOfSymbol>();
            List<byte> byteStr = new List<byte>();
            for (int i = 6 + count ; i < info.Count(); i++)
            {
                byteStr.Add(info[i]);
            }
            string[] arrStr = System.Text.Encoding.ASCII.GetString(byteStr.ToArray()).Split(" ", StringSplitOptions.RemoveEmptyEntries);
            int j = 0;
            for (int i = 6; i < count + 6 ; i++) 
            {
                codeOfSymbols.Add(new CodeOfSymbol(info[i], arrStr[j]));
                j++;
            }

            BitArray fileInBits = new BitArray(file);

            string tempStringCode = "";
            for(int i = 0; i < fileInBits.Length; i++)
            {
                if (fileInBits[i] == true)
                    tempStringCode += "1";
                else
                    tempStringCode += "0";
                
                CodeOfSymbol tempCode = SearchByteCodeByCode(codeOfSymbols, tempStringCode);
                if(tempCode != null)
                {
                    newFile.Add(tempCode.GetByte());
                    tempStringCode = "";
                }
                else
                {
                    continue;
                }
                
            }

            byte[] byteFile = new byte[oldSize];
            for(int i = 0; i < oldSize; i++)
            {
                byteFile[i] = newFile[i];
            }
            return byteFile;
        }

        public (byte[] blob, byte[] info) Encode(byte[] file)
        {
            List<byte> info = new List<byte>() { 0x00, 0x00, 0x00, 0x00 };
            List<FrequancyOfByte> frequancyOfBytes = new List<FrequancyOfByte>();
            foreach(byte item in file)
            {
                FrequancyOfByte.AddingFreq(item, ref frequancyOfBytes);
            }
            FrequancyOfByte.Sort(ref frequancyOfBytes);

            //подсчет теоретического количества информации
            double theorySize = 0;
            foreach(FrequancyOfByte item in frequancyOfBytes)
            {
                theorySize += -(Math.Log2(Convert.ToDouble(item.GetFreq())/file.Length)*Convert.ToDouble(item.GetFreq()));
            }



            List<CodeOfSymbol> codeOfSymbols = new List<CodeOfSymbol>();
            foreach(FrequancyOfByte item in frequancyOfBytes)
            {
                codeOfSymbols.Add(new CodeOfSymbol(item.GetByte()));
            }
            //деление пополам и кодирование
            List<List<FrequancyOfByte>> dividedBytes = new List<List<FrequancyOfByte>>();
            dividedBytes.Add(frequancyOfBytes);
            int maxCountOfTables = 0;
            while (maxCountOfTables != 1)
            {
                maxCountOfTables = 0;
                dividedBytes = BisectionAndCoding(dividedBytes, ref codeOfSymbols);
                foreach (List<FrequancyOfByte> var in dividedBytes)
                {
                    if (maxCountOfTables < var.Count())
                    {
                        maxCountOfTables = var.Count();
                    }
                }
            }
        
            
            List<bool> fileInBitsBoolean = new List<bool>();

           
            foreach(byte item in file)
            {
                
                CodeOfSymbol tempCodeOfSymbol = SearchByteCodeByByte(codeOfSymbols, item);
               
                foreach(char var in tempCodeOfSymbol.GetCode())
                {
                    if (var == '0')
                        fileInBitsBoolean.Add(false);
                    else
                        fileInBitsBoolean.Add(true);
                   
                }
            }
            int countBitsToAdd = Math.Abs((fileInBitsBoolean.Count() / 8 + 1) * 8 - fileInBitsBoolean.Count()) ;
            for(int i = 0; i < countBitsToAdd; i++)
            {
                fileInBitsBoolean.Add(false);
            }

            BitArray fileInBits = new BitArray(fileInBitsBoolean.ToArray());
            byte[] newFile = new byte[(fileInBits.Length) / 8];
            fileInBits.CopyTo(newFile,0);


            string codeString = "";
            foreach (CodeOfSymbol item in codeOfSymbols)
            {
                codeString += item.GetCode();
                codeString += " ";
            }

            var countOfCode = BitConverter.GetBytes(codeOfSymbols.Count());
            info[2] = (BitConverter.GetBytes(codeOfSymbols.Count())[0]);
            if (codeOfSymbols.Count() == 256)
                info[3]= (0x01);
            else
                info[3] = (0x00);
            byte[] oldSize = BitConverter.GetBytes(file.Length);
            foreach (byte it in oldSize)
                info.Add(it);
            foreach (CodeOfSymbol item in codeOfSymbols)
            {
                info.Add(item.GetByte());
            }           
            byte[] byteString = System.Text.Encoding.ASCII.GetBytes(codeString);
            foreach (byte item in byteString)
            {
                info.Add(item);
            }

            var infoSize = info.Count - 2;
            var infoSizeBytes = BitConverter.GetBytes(infoSize);

            info[0] = infoSizeBytes[0];
            info[1] = infoSizeBytes[1];

            int sizeOfNewCode = 0;
            foreach(CodeOfSymbol item in codeOfSymbols)
            {
                sizeOfNewCode += item.GetCode().Length*(FrequancyOfByte.SearchFreq(frequancyOfBytes, item.GetByte()).GetFreq());
            }

           
            
            return (newFile, info.ToArray());
        }

        public CodeOfSymbol SearchByteCodeByByte(List<CodeOfSymbol> codeOfSymbols, byte searchingByte)
        {
            foreach(CodeOfSymbol item in codeOfSymbols)
            {
                if(item.GetByte() == searchingByte)
                {
                    return item;
                }
            }
            return null;
        }
        public CodeOfSymbol SearchByteCodeByCode(List<CodeOfSymbol> codeOfSymbols, string code)
        {
            foreach (CodeOfSymbol item in codeOfSymbols)
            {
                if (item.GetCode().Equals(code))
                {
                    return item;
                }
            }
            return null;
        }

        public List<List<FrequancyOfByte>> BisectionAndCoding( List<List<FrequancyOfByte>> oldFrequancyOfBytes, ref List<CodeOfSymbol> codeOfSymbols)
        {
            List<List<FrequancyOfByte>> newFrequanceOfBytes = new List<List<FrequancyOfByte>>();
            foreach (List<FrequancyOfByte> item in oldFrequancyOfBytes)
            {
                if(item.Count() == 1)
                {
                    newFrequanceOfBytes.Add(item);
                    continue;
                }    
                int frequancySum = 0;
                foreach (FrequancyOfByte var in item)
                {
                    frequancySum += var.GetFreq();
                }
                int middle = frequancySum / 2;
                int tempFreq = 0;
                
                //index - индекс последнего элемента, входящего в первую половину по изначальному предположению
                int index = 0;
                while (tempFreq <= middle)
                {
                    tempFreq += item[index].GetFreq();
                    index++;
                }
                index--;
                if (index == 0)
                {

                    List<FrequancyOfByte> frequancyOfBytesForAdding = new List<FrequancyOfByte>();
                    frequancyOfBytesForAdding.Add(item[0]);
                    CodeOfSymbol.UpdateCode(frequancyOfBytesForAdding, ref codeOfSymbols, "0");
                    newFrequanceOfBytes.Add(frequancyOfBytesForAdding);
                    frequancyOfBytesForAdding = new List<FrequancyOfByte>();
                    for (int i = 1; i < item.Count(); i++)
                    {
                        frequancyOfBytesForAdding.Add(item[i]);
                    }
                    CodeOfSymbol.UpdateCode(frequancyOfBytesForAdding, ref codeOfSymbols, "1");
                    newFrequanceOfBytes.Add(frequancyOfBytesForAdding);
                    
                    
                }
                else if (index == item.Count() - 1)
                {
                    List<KeyValuePair<int, int>> freqOfParts = new List<KeyValuePair<int, int>>();
                    freqOfParts.Add(new KeyValuePair<int, int>(tempFreq - item[index].GetFreq(), frequancySum - (tempFreq - item[index].GetFreq())));
                    freqOfParts.Add(new KeyValuePair<int, int>(tempFreq, frequancySum - tempFreq));

                    int indexOfMin = 0;
                    int min = int.MaxValue;
                    foreach (KeyValuePair<int, int> var in freqOfParts)
                    {
                        if (min > Math.Abs(var.Key - var.Value))
                        {
                            min = Math.Abs(var.Key - var.Value);
                            indexOfMin = freqOfParts.IndexOf(var);
                        }
                    }
                    switch (indexOfMin)
                    {
                        case 0:
                            List<FrequancyOfByte> frequancyOfBytesForAdding = new List<FrequancyOfByte>();
                            for (int i = 0; i < index; i++)
                            {
                                frequancyOfBytesForAdding.Add(item[i]);
                            }
                            CodeOfSymbol.UpdateCode(frequancyOfBytesForAdding, ref codeOfSymbols, "0");
                            newFrequanceOfBytes.Add(frequancyOfBytesForAdding);
                            frequancyOfBytesForAdding = new List<FrequancyOfByte>();
                            for (int i = index; i < item.Count(); i++)
                            {
                                frequancyOfBytesForAdding.Add(item[i]);
                            }
                            CodeOfSymbol.UpdateCode(frequancyOfBytesForAdding, ref codeOfSymbols, "1");
                            newFrequanceOfBytes.Add(frequancyOfBytesForAdding);
                            break;
                        case 1:
                            frequancyOfBytesForAdding = new List<FrequancyOfByte>();
                            for (int i = 0; i <= index; i++)
                            {
                                frequancyOfBytesForAdding.Add(item[i]);
                            }
                            CodeOfSymbol.UpdateCode(frequancyOfBytesForAdding, ref codeOfSymbols, "0");
                            newFrequanceOfBytes.Add(frequancyOfBytesForAdding);
                            frequancyOfBytesForAdding = new List<FrequancyOfByte>();
                            for (int i = index + 1; i < item.Count(); i++)
                            {
                                frequancyOfBytesForAdding.Add(item[i]);
                            }
                            CodeOfSymbol.UpdateCode(frequancyOfBytesForAdding, ref codeOfSymbols, "1");
                            newFrequanceOfBytes.Add(frequancyOfBytesForAdding);
                            break;
                    }
                                       
                }
                else
                {
                    List<KeyValuePair<int, int>> freqOfParts = new List<KeyValuePair<int, int>>();
                    freqOfParts.Add(new KeyValuePair<int, int>(tempFreq - item[index].GetFreq(), frequancySum - (tempFreq - item[index].GetFreq())));
                    freqOfParts.Add(new KeyValuePair<int, int>(tempFreq, frequancySum - tempFreq));
                    freqOfParts.Add(new KeyValuePair<int, int>(tempFreq + item[index + 1].GetFreq(), frequancySum - (tempFreq + item[index + 1].GetFreq())));

                    int indexOfMin = 0;
                    int min = int.MaxValue;
                    foreach (KeyValuePair<int, int> var in freqOfParts)
                    {
                        if (min > Math.Abs(var.Key - var.Value))
                        {
                            min = Math.Abs(var.Key - var.Value);
                            indexOfMin = freqOfParts.IndexOf(var);
                        }
                    }
                    switch (indexOfMin)
                    {
                        case 0:
                            List<FrequancyOfByte> frequancyOfBytesForAdding = new List<FrequancyOfByte>();
                            for (int i = 0; i < index; i++)
                            {
                                frequancyOfBytesForAdding.Add(item[i]);
                            }
                            CodeOfSymbol.UpdateCode(frequancyOfBytesForAdding, ref codeOfSymbols, "0");
                            newFrequanceOfBytes.Add(frequancyOfBytesForAdding);
                            frequancyOfBytesForAdding = new List<FrequancyOfByte>();
                            for (int i = index; i < item.Count(); i++)
                            {
                                frequancyOfBytesForAdding.Add(item[i]);
                            }
                            CodeOfSymbol.UpdateCode(frequancyOfBytesForAdding, ref codeOfSymbols, "1");
                            newFrequanceOfBytes.Add(frequancyOfBytesForAdding);
                            break;
                        case 1:
                            frequancyOfBytesForAdding = new List<FrequancyOfByte>();
                            for (int i = 0; i <= index; i++)
                            {
                                frequancyOfBytesForAdding.Add(item[i]);
                            }
                            CodeOfSymbol.UpdateCode(frequancyOfBytesForAdding, ref codeOfSymbols, "0");
                            newFrequanceOfBytes.Add(frequancyOfBytesForAdding);
                            frequancyOfBytesForAdding = new List<FrequancyOfByte>();
                            for (int i = index + 1; i < item.Count(); i++)
                            {
                                frequancyOfBytesForAdding.Add(item[i]);
                            }
                            CodeOfSymbol.UpdateCode(frequancyOfBytesForAdding, ref codeOfSymbols, "1");
                            newFrequanceOfBytes.Add(frequancyOfBytesForAdding);
                            break;
                        case 2:
                            frequancyOfBytesForAdding = new List<FrequancyOfByte>();
                            for (int i = 0; i <= index + 1; i++)
                            {
                                frequancyOfBytesForAdding.Add(item[i]);
                            }
                            CodeOfSymbol.UpdateCode(frequancyOfBytesForAdding, ref codeOfSymbols, "0");
                            newFrequanceOfBytes.Add(frequancyOfBytesForAdding);
                            frequancyOfBytesForAdding = new List<FrequancyOfByte>();
                            for (int i = index + 2; i < item.Count(); i++)
                            {
                                frequancyOfBytesForAdding.Add(item[i]);
                            }
                            CodeOfSymbol.UpdateCode(frequancyOfBytesForAdding, ref codeOfSymbols, "1");
                            newFrequanceOfBytes.Add(frequancyOfBytesForAdding);
                            break;
                    }
                    
                }                                           
            }
            return newFrequanceOfBytes;
        }
        
    }
}
