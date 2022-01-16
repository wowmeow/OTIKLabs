using Project.Coders;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project
{
    public static class Project
    {
        public static readonly byte CurrentVersion = 0x01;
        public static readonly byte CurrentSubversion = 0x06;
        public static readonly byte[] Signature = new byte[] { 0x6b, 0x6f, 0x74, 0x69, 0x63 };

        public static Dictionary<string, string> coders;

        static Project()
        {
            coders = new Dictionary<string, string>();
            coders.Add("001", "q кодирование");
            coders.Add("010", "Шеннон-Фано");
            coders.Add("011", "Арифметичский");
            coders.Add("100", "RLE");
            coders.Add("101", "Hamming");
        }

        public static void CheckSignature(byte[] signature)
        {
            bool check = signature.Length == Signature.Length;
            if (check)
            {
                for (int i = 0; i < Signature.Length; i++)
                {
                    check &= signature[i] == Signature[i];
                }
            }

            if (!check)
            {
                throw new Exception("Not kotic file");
            }
        }

        public static void CheckVersion(byte version, byte subversion)
        {
            if (version != CurrentVersion || subversion != CurrentSubversion)
            {
                throw new Exception("Bad version");
            }
        }

        public static List<ICoder> GetCoders(byte[] codesInBytes)
        {
            List<ICoder> codersList = new List<ICoder>();
            BitArray codeInBits = new BitArray(codesInBytes);

            string strCode = "";
            List<string> strCoders = new List<string>();


            foreach (bool item in codeInBits)
            {
                if (item == true)
                    strCode += '1';
                else
                    strCode += '0';
            }

            string tempCode = "";
            int j = 0;
            for (int i = 0; i < 16; i++)
            {
                if (j == 3)
                {
                    if (tempCode == "000")
                        break;
                    else
                    {
                        tempCode = coders[tempCode];
                        switch (tempCode)
                        {
                            case "q кодирование":
                                codersList.Add(new DefaultCoder());
                                break;
                            case "Шеннон-Фано":
                                codersList.Add(new Shannon_FanoCoder());
                                break;
                            case "Арифметичский":
                                codersList.Add(new ArithmeticCoder());
                                break;
                            case "RLE":
                                codersList.Add(new RLE());
                                break;
                            case "Hamming":
                                codersList.Add(new Hamming());
                                break;
                        }
                    }
                    j = 0;
                    tempCode = "";
                    //codersList.Add(coders[code]);
                }
                tempCode += strCode[i];
                j++;
            }
            return codersList;
        }
    }
}
