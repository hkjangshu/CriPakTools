using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using System.Windows.Navigation;
using System.Diagnostics;
using Ookii.Dialogs.Wpf;
using System.Threading;
using System.Windows.Threading;
using LibCPK;

namespace CriPakGUI
{
    /// <summary>
    /// CpkPatcher.xaml 的交互逻辑
    /// </summary>
    public partial class CpkPatcher : Window
    {
        public CpkPatcher(double x, double y)
        {
            InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.Manual;
            this.Top = x;
            this.Left = y;
        }

        private void button_selPatchPath_Click(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog saveFilesDialog = new VistaFolderBrowserDialog();
            saveFilesDialog.SelectedPath = MainApp.Instance.currentPackage.BasePath + "/";
            if (saveFilesDialog.ShowDialog().Value)
            {
                Debug.Print(saveFilesDialog.SelectedPath);
                textbox_patchDir.Text = saveFilesDialog.SelectedPath;
            }

        }



        private void button_selDstCPK_Click(object sender, RoutedEventArgs e)
        {
            VistaSaveFileDialog saveDialog = new VistaSaveFileDialog();
            saveDialog.InitialDirectory = MainApp.Instance.currentPackage.BasePath;
            saveDialog.RestoreDirectory = true;
            saveDialog.Filter = "CPK File（*.cpk）|*.cpk";
            if (saveDialog.ShowDialog() == true)
            {
                string saveFileName = saveDialog.FileName;
                textbox_cpkDir.Text = saveFileName;
            }

        }
        private delegate void textblockDelegate(string text);
        private void updateTextblock(string text)
        {
            textblock0.Text += string.Format("Updating ... {0}\n", text);
            scrollview0.ScrollToEnd();
        }

        private delegate void progressbarDelegate(float no);
        private void updateprogressbar(float no)
        {
            progressbar1.Value = no;
        }
        public class CPKPatchInfo
        {
            public string cpkDir { get; set; }
            public string patchDir { get; set; }
            public bool bForceCompress { get; set; }
            public Dictionary<string, string> batch_file_list { get; set; }
        }

        private void button_PatchCPK_Click(object sender, RoutedEventArgs e)
        {
            string cpkDir = textbox_cpkDir.Text;
            string patchDir = textbox_patchDir.Text;
            Dictionary<string, string> batch_file_list = new Dictionary<string, string>();
            List<string> ls = new List<string>();
            if ((MainApp.Instance.currentPackage.CpkContent != null) && (Directory.Exists(patchDir)))
            {

                GetFilesFromPath(patchDir, ref ls);
                Debug.Print(string.Format("GOT {0} Files.", ls.Count));
                foreach (string s in ls)
                {
                    string name = s.Remove(0, patchDir.Length + 1);
                    name = name.Replace("\\", @"/");
                    if (!name.Contains(@"/"))
                    {
                        name = @"/" + name;
                    }
                    batch_file_list.Add(name, s);
                }
                CPKPatchInfo t = new CPKPatchInfo();
                t.cpkDir = cpkDir;
                t.patchDir = patchDir;
                if (checkbox_donotcompress.IsChecked == true)
                {
                    t.bForceCompress = false;
                }
                else
                {
                    t.bForceCompress = true;
                }
                t.batch_file_list = batch_file_list;
                ThreadPool.QueueUserWorkItem(new WaitCallback(PatchCPK), t);
            }
            else
            {
                MessageBox.Show("Error, cpkdata or patchdata not found.");

            }
        }

        private void PatchCPK(object t)
        {
            CPKPatchInfo v = (CPKPatchInfo)t;
            PatchCPK patcher = new PatchCPK(MainApp.Instance.currentPackage.CpkContent, MainApp.Instance.currentPackage.CpkContentName);
            patcher.SetListener(
                (float value) =>
            {
                UI_SetProgess(value);
            }, 
                (string msg)=> 
                {
                UI_SetTextBlock(msg);
                }, 
                ()=> {
                MessageBox.Show("CPK Patched.");
                this.UI_SetProgess(0f);
            });
            patcher.Patch(v.cpkDir, v.bForceCompress, v.batch_file_list);

        }

        public void UI_SetProgess(float value)
        {
            this.Dispatcher.Invoke(new progressbarDelegate(updateprogressbar), new object[] { (float)value });
        }

        public void UI_SetTextBlock(string msg)
        {
            this.Dispatcher.Invoke(new textblockDelegate(updateTextblock), new object[] { msg });
        }

        private void GetFilesFromPath(string directoryname, ref List<string> ls)
        {
            FileInfo[] fi = new DirectoryInfo(directoryname).GetFiles();
            DirectoryInfo[] di = new DirectoryInfo(directoryname).GetDirectories();
            if (fi.Length != 0)
            {
                foreach (FileInfo v in fi)
                {
                    ls.Add(v.FullName);
                }
            }
            if (di.Length != 0)
            {
                foreach (DirectoryInfo v in di)
                {
                    GetFilesFromPath(v.FullName, ref ls);

                }
            }
        }
    }
}
