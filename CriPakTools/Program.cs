using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using LibCPK;
namespace CriPakTools
{
    public class SystemEncoding
    {
        public static Encoding Codecs = Encoding.UTF8;
    }
    class Program
    {
        [DllImport("user32.dll", EntryPoint = "MessageBox")]
        public static extern int MsgBox(IntPtr hwnd, string text, string caption, uint type);
        public static void ShowMsgBox(string msg)
        {
            MsgBox(IntPtr.Zero, msg, "CriPakTools", 1);
        }


        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("This console program is all dead. Use CriPakGUI.exe instead.");
                ShowMsgBox("This console program is all dead. Use CriPakGUI.exe instead.");
                return;
            }

        }
    }
}
