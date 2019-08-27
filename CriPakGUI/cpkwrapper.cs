using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibCPK;
using System.IO;

namespace CriPakGUI
{
    public enum PackageEncodings
    {
        UTF_8 = 65001,
        SHIFT_JIS = 932,
        
    }
    public class MyPackage
    {
        public CPK CpkContent { get; set; }
        public string BasePath { get; set; }
        public string CpkContentName { get; set; }
        public string BaseName { get; set; }
        public string FileName { get; set; }
        public Encoding EncodingPage = Encoding.GetEncoding((int)PackageEncodings.UTF_8);
    }
    public class CPKTable
    {
        public int TableId { get; set; }
        public string FileName { get; set; }
        public string LocalName { get; set; }
        //public string DirName;
        public UInt64 FileOffset { get; set; }
        public int FileSize { get; set; }
        public int ExtractSize { get; set; }
        public string FileType { get; set; }
        public float Pt { get; set; }
    }


    public class CpkWrapper
    {

        public int nums = 0;
        public List<CPKTable> table;
        public bool IsVaidCPK { get; private set; } = false;

        private bool CheckValid(string path)
        {
            bool result = false;
            using (FileStream fs = File.OpenRead(path))
            {
                using (BinaryReader br = new BinaryReader(fs))
                {
                    br.BaseStream.Seek(0, SeekOrigin.Begin);
                    uint magic = br.ReadUInt32();
                    if (magic == 0x204b5043)
                    {
                        result = true;
                    }
                    else
                    {
                        result = false;
                    }
                }
            }
            return result;
        }

        public CpkWrapper(string inFile)
        {
            IsVaidCPK = CheckValid(inFile);
            if (IsVaidCPK == false)
            {
                return;
            }
            string cpk_name = inFile;
            table = new List<CPKTable>();
            MainApp.Instance.currentPackage.CpkContent = new CPK();
            MainApp.Instance.currentPackage.CpkContent.ReadCPK(cpk_name, MainApp.Instance.currentPackage.EncodingPage);
            MainApp.Instance.currentPackage.CpkContentName = cpk_name;

            BinaryReader oldFile = new BinaryReader(File.OpenRead(cpk_name));
            List<FileEntry> entries = MainApp.Instance.currentPackage.CpkContent.fileTable.OrderBy(x => x.FileOffset).ToList();
            int i = 0;
            bool bFileRepeated = Tools.CheckListRedundant(entries);
            while (i < entries.Count)
            {
                /*
                Console.WriteLine("FILE ID:{0},File Name:{1},File Type:{5},FileOffset:{2:x8},Extract Size:{3:x8},Chunk Size:{4:x8}", entries[i].ID,
                                                            (((entries[i].DirName != null) ? entries[i].DirName + "/" : "") + entries[i].FileName),
                                                            entries[i].FileOffset,
                                                            entries[i].ExtractSize,
                                                            entries[i].FileSize,
                                                            entries[i].FileType);
                */
                
                
                if (entries[i].FileType != null)
                {
                    nums += 1;

                    CPKTable t = new CPKTable();
                    if (entries[i].ID == null)
                    {
                        t.TableId = -1;
                    }
                    else
                    {
                        t.TableId = Convert.ToInt32(entries[i].ID);
                    }
                    if (t.TableId >= 0 && bFileRepeated)
                    {
                        t.FileName = (((entries[i].DirName != null) ? 
                                        entries[i].DirName + "/" : "") + string.Format("[{0}]",t.TableId.ToString()) + entries[i].FileName);
                    }
                    else
                    {
                        t.FileName = (((entries[i].DirName != null) ?
                                        entries[i].DirName + "/" : "") +  entries[i].FileName);
                    }
                    t.LocalName = entries[i].FileName.ToString();

                    t.FileOffset = Convert.ToUInt64(entries[i].FileOffset);
                    t.FileSize = Convert.ToInt32(entries[i].FileSize);
                    t.ExtractSize = Convert.ToInt32(entries[i].ExtractSize);
                    t.FileType = entries[i].FileType;
                    if (entries[i].FileType == "FILE")
                    {
                        t.Pt = (float)Math.Round((float)t.FileSize / (float)t.ExtractSize, 2) * 100f;
                    }
                    else
                    {
                        t.Pt = (float)1f * 100f;
                    }
                    table.Add(t);
                }
                i += 1;

            }
            oldFile.Close();

        }
    }
}
