using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Project;
using System.Collections;
using Project.Coders;

namespace ProjectGui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<string> filenames;
        private List<string> chosedCoders;
        Dictionary<string, string> coders;
        public MainWindow()
        {
            filenames = new List<string>();
            chosedCoders = new List<string>();
            coders = new Dictionary<string, string>();
            coders.Add("001", "q кодирование");
            coders.Add("010", "Шеннон-Фано");
            coders.Add("011", "Арифметичский");
            coders.Add("100", "RLE");
            coders.Add("101", "Hamming");
            InitializeComponent();
        }

        public void ClickDirArchiving(object sender, RoutedEventArgs e)
        {

            l_error.Content = "";
            l_inf.Content = "";
            try
            {
                var dialog = new CommonOpenFileDialog();
                dialog.InitialDirectory = "D:";
                dialog.Title = "Выбор папки для архивирования";
                dialog.IsFolderPicker = true;
                dialog.Multiselect = true;
                /*dialog.EnsurePathExists = true;
                dialog.EnsureFileExists = false;
                dialog.AllowNonFileSystemItems = false;*/
                CommonFileDialogResult result = dialog.ShowDialog();

                foreach (string file in dialog.FileNames)
                {
                    filenames.Add(file);
                }

                dialog.Title = "Выбор пути сохранения архива";
                result = dialog.ShowDialog();
                if (filenames.Count == 0)
                {
                    throw new Exception("Не выбраны директории");
                }
                if (dialog.FileName == null)
                {
                    throw new Exception("Не выбран путь сохранения");
                }
                l_inf.Content = "Архивация началась";
                ProjectArchivator koticArchivator = new ProjectArchivator(filenames, GetCodeCoders());
                koticArchivator.GenerateArchive(dialog.FileName);
                l_inf.Content = "Архивация прошла успешно";
            }
            catch (Exception ex)
            {
                l_inf.Content = "Архивация не прошла";
                l_error.Content = ex.Message;
                if (ex is InvalidOperationException)
                {
                    l_error.Content = "Не выбран путь сохранения";
                }
            }
               
        }
        public void ClickFileArchiving(object sender, RoutedEventArgs e)
        {
            l_error.Content = "";
            l_inf.Content = "";
            try
            {
                var dialog = new CommonOpenFileDialog();
                dialog.InitialDirectory = "D:";
                dialog.Title = "Выбор файла для архивирования";             
                dialog.Multiselect = true;
                /*dialog.EnsurePathExists = true;
                dialog.EnsureFileExists = false;
                dialog.AllowNonFileSystemItems = false;*/
                CommonFileDialogResult result = dialog.ShowDialog();

                foreach (string file in dialog.FileNames)
                {
                    filenames.Add(file);
                }

                dialog.Title = "Выбор пути сохранения архива";
                dialog.IsFolderPicker = true;
                result = dialog.ShowDialog();
                if (filenames.Count == 0)
                {
                    throw new Exception("Не выбраны файлы");
                }
                if (dialog.FileName == null)
                {
                    throw new Exception("Не выбран путь сохранения");
                }
              
                l_inf.Content = "Архивация началась";
                ProjectArchivator koticArchivator = new ProjectArchivator(filenames, GetCodeCoders());
                koticArchivator.GenerateArchive(dialog.FileName);
                l_inf.Content = "Архивация прошла успешно";
                filenames = new List<string>();
            }
            catch (Exception ex)
            {
                l_inf.Content = "Архивация не прошла";
                l_error.Content = ex.Message;
                if (ex is InvalidOperationException)
                {
                    l_error.Content = "Не выбран путь сохранения";
                }
            }

        }


        public void ClickDearchiving(object sender, RoutedEventArgs e)
        {
           
            l_error.Content = "";
            l_inf.Content = "";
           // try
            //{
                var dialog = new CommonOpenFileDialog();
                dialog.InitialDirectory = "D:";
                dialog.Title = "Выбор архива";
                CommonFileDialogResult result = dialog.ShowDialog();
                ProjectDearchivator koticDearchivator = new ProjectDearchivator(dialog.FileName);
                dialog.Title = "Путь разархивирования"; dialog.IsFolderPicker = true;
                result = dialog.ShowDialog();
                koticDearchivator.GenerateFilesFromArchive(dialog.FileName);
                l_inf.Content = "Разархивация прошла успешно";


            //}
          /*  catch (Exception ex)
            {
                l_error.Content = ex.Message;
                l_inf.Content = "Разархивация не прошла";
            }*/
        }  
        
        public void CheckBoxChangedTrue(object sender, RoutedEventArgs e)
        {
            chosedCoders.Add(((CheckBox)sender).Content.ToString());
            txtCoders.Text = "";
            string text = "";
            foreach(String str in chosedCoders)
            {
                text += str;
                text += "\n";
            }
            txtCoders.Text = text;
        }
        public void CheckBoxChangedFalse(object sender, RoutedEventArgs e)
        {
            chosedCoders.Remove(((CheckBox)sender).Content.ToString());
            txtCoders.Text = "";
            string text = "";
            foreach (String str in chosedCoders)
            {
                text += str;
                text += "\n";
            }
            txtCoders.Text = text;
        }

        public  byte[] GetCodeCoders()
        {
            byte[] codersInfo = new byte[2];

            List<BitArray> codersInBitList = new List<BitArray>();

            int j = 0;
            foreach(string str in chosedCoders)
            {
                var myKey = coders.FirstOrDefault(x => x.Value == str).Key;
                BitArray tempCode = new BitArray(3);
                foreach(char sym in myKey)
                {
                    if (sym == '1')
                        tempCode[j] = true;
                    else
                        tempCode[j] = false;
                    j++;
                }
                codersInBitList.Add(tempCode);
                j = 0;
            }

            bool[] boolCode = new bool[16];
           int k = 0;
           foreach(BitArray item in codersInBitList)
            {
                foreach(bool var in item)
                {
                    boolCode[k] = var;
                    k++;
                }
            }

            BitArray codersInBit = new BitArray(boolCode);
            for(int i = codersInBit.Length - 1; i < 16; i++)
            {
                codersInBit[i] = false;
            }
            codersInBit.CopyTo(codersInfo, 0);
            return codersInfo;
        }

       
    }
}
