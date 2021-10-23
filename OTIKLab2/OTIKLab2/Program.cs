using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Excel = Microsoft.Office.Interop.Excel;
using Word = Microsoft.Office.Interop.Word;

namespace OTIKLAb2
{
    class Program
    {
        static void Main(string[] args)
        {
            WorkFile workFile = new WorkFile();
            workFile.getfile();
            workFile.Counter();
            workFile.WriteExel();
            WorkFileBite workFileByte = new WorkFileBite();
            workFileByte.getfile();
            workFileByte.Counter();
            workFileByte.WriteExel();
            CloseProcess();
            Console.ReadLine();
        }
        public static void CloseProcess()
        {
            Process[] List;
            List = Process.GetProcessesByName("EXCEL");
            foreach (Process proc in List)
            {
                proc.Kill();
            }
        }

    }
    class WorkFile
    {
        public List<string> file = new List<string>();
        private List<Dictionary<string, int>> ElemCounter = new List<Dictionary<string, int>>();
        private List<long> SumElem = new List<long>();
        public void ReadText(string filepath)
        {
            string res = File.ReadAllText(filepath);
            file.Add(res);
        }
        public void getfile()
        {
            DirectoryInfo directory = new DirectoryInfo(@"C:\Users\Acer\Desktop\текст");
            foreach (var item in directory.GetFiles())
            {
                ReadText(item.FullName);
            }
        }
        public void Counter()
        {
            for (int k = 0; k < file.Count; k++)
            {
                if (file.Count > 10000)
                {
                    ElemCounter.Add(new Dictionary<string, int>());
                    long cntr = 0;
                    for (int i = 0; i < file[k].Length; i++)
                    {
                        if (ElemCounter[k].ContainsKey(file[k][i].ToString()))
                        {
                            ElemCounter[k][file[k][i].ToString()] += 1;
                            cntr++;
                        }
                        else
                        {
                            ElemCounter[k].Add(file[k][i].ToString(), 1);
                            cntr++;
                        }
                    }
                    SumElem.Add(cntr);
                }
                else
                {
                    ElemCounter.Add(new Dictionary<string, int>());
                    long cntr = 0;
                    for (int i = 0; i < file[k].Length; i++)
                    {
                        if (ElemCounter[k].ContainsKey(file[k][i].ToString().ToLower()))
                        {
                            ElemCounter[k][file[k][i].ToString().ToLower()] += 1;
                            cntr++;
                        }
                        else
                        {
                            ElemCounter[k].Add(file[k][i].ToString().ToLower(), 1);
                            cntr++;
                        }
                    }
                    SumElem.Add(cntr);
                }
            }
        }
        public void WriteExel()
        {
            Excel.Application ex = new Excel.Application();
            ex.SheetsInNewWorkbook = file.Count;
            Excel.Workbook workBook = ex.Workbooks.Add(Type.Missing);
            for (int k = 1; k < file.Count+1; k++)
            {
                Excel.Worksheet sheet = (Excel.Worksheet)ex.Worksheets.get_Item(k);
                sheet.Name = "Текст" + k;
                string[] arr = new string[ElemCounter[k-1].Count];
                ElemCounter[k-1].Keys.CopyTo(arr, 0);
                sheet.Cells[1, 1] = "Символ";
                sheet.Cells[1, 2] = "Количество появлений";
                sheet.Cells[1, 3] = "Нормированная относительная частота";
                sheet.Cells[1, 4] = "Количество информации на символ";
                sheet.Cells[1, 5] = "Средняя информация на символ";
                double buf1 = 0;
                for (int i = 2; i < ElemCounter[k-1].Count + 2; i++)
                {
                    sheet.Cells[i, 1] = arr[i-2];
                    sheet.Cells[i, 2] = ElemCounter[k-1][arr[i-2]];
                    double buf = Convert.ToDouble(ElemCounter[k - 1][arr[i - 2]]) / SumElem[k - 1];
                    sheet.Cells[i, 3] = buf;
                    sheet.Cells[i, 4] = Math.Log(buf,2) *- 1;
                    buf1 += Math.Log(buf, 2) * -1 * buf;
                }

                sheet.Cells[2, 5] = buf1;
            }
            ex.Application.ActiveWorkbook.SaveAs(@"C:\Users\Acer\Desktop\OTIKLab2doc\doc.xlsx");
            Console.WriteLine("good");
            workBook.Close(0);
            ex.Quit();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(ex);
        }
    }

    class WorkFileBite
    {
        public List<byte[]> file = new List<byte[]>();
        private List<Dictionary<byte, int>> ElemCounter = new List<Dictionary<byte, int>>();
        private List<long> SumElem = new List<long>();
        public void ReadText(string filepath)
        {
            byte[] res = File.ReadAllBytes(filepath);
            file.Add(res);
        }
        public void getfile()
        {
            DirectoryInfo directory = new DirectoryInfo(@"C:\Users\Acer\Desktop\текст");
            foreach (var item in directory.GetFiles())
            {
                ReadText(item.FullName);
            }
        }
        public void Counter()
        {
            for (int k = 0; k < file.Count; k++)
            {
                ElemCounter.Add(new Dictionary<byte, int>());
                long cntr = 0;
                for (int i = 0; i < file[k].Length; i++)
                {
                    if (ElemCounter[k].ContainsKey(file[k][i]))
                    {
                        ElemCounter[k][file[k][i]] += 1;
                        cntr++;
                    }
                    else
                    {
                        ElemCounter[k].Add(file[k][i], 1);
                        cntr++;
                    }
                }
                SumElem.Add(cntr);
            }
        }
        public void WriteExel()
        {
            Excel.Application ex = new Excel.Application();
            ex.SheetsInNewWorkbook = file.Count;
            Excel.Workbook workBook = ex.Workbooks.Add();
            for (int k = 1; k < file.Count + 1; k++)
            {
                Excel.Worksheet sheet = (Excel.Worksheet)ex.Worksheets.get_Item(k);
                sheet.Name = "Текст" + k;
                byte[] arr = new byte[ElemCounter[k - 1].Count];
                ElemCounter[k - 1].Keys.CopyTo(arr, 0);
                sheet.Cells[1, 1] = "Символ";
                sheet.Cells[1, 2] = "Количество появлений";
                sheet.Cells[1, 3] = "Нормированная относительная частота";
                sheet.Cells[1, 4] = "Количество информации на символ";
                sheet.Cells[1, 5] = "Средняя информация на символ";
                double buf1 = 0;
                for (int i = 2; i < ElemCounter[k - 1].Count + 2; i++)
                {
                    sheet.Cells[i, 1] = arr[i - 2];
                    sheet.Cells[i, 2] = ElemCounter[k - 1][arr[i - 2]];
                    double buf = Convert.ToDouble(ElemCounter[k - 1][arr[i - 2]]) / SumElem[k - 1];
                    sheet.Cells[i, 3] = buf;
                    sheet.Cells[i, 4] = Math.Log(buf, 2) * -1;
                    buf1 += Math.Log(buf, 2) * -1 * buf;
                }
                sheet.Cells[2, 5] = buf1;
            }
            ex.Application.ActiveWorkbook.SaveAs(@"C:\Users\Acer\Desktop\OTIKLab2doc\docbyte.xlsx");
            Console.WriteLine("good");
            workBook.Close(true);
            ex.Quit();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(ex);
        }
    }
}
