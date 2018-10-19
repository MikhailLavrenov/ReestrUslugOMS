using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;

namespace ReestrUslugOMS.Classes_and_structures
{
    class ImportDbf
    {

        List<DbfFile> ImportList { get; set; }
        public DateTime Period { get; set; }



        public ImportDbf(DateTime period, List<enImportItems> importItems)
        {
            ImportList = new List<DbfFile>();
            Period = period;
            BuildDbfFile(importItems);
        }


        private void BuildDbfFile(List<enImportItems> importEntities)
        {
            string periodPath = $"{Config.Instance.RelaxPath}OUTS{Period:YYYY}\\PERIOD{Period:MM}\\";

            foreach (var item in importEntities)
            {
                if (item == enImportItems.УслугиПациентыДоРазложения)
                {
                    ImportList.Add(new DbfFile(nameof(), $"{periodPath}PAT.DBF"));
                    ImportList.Add(new DbfFile(nameof(), $"{periodPath}PATU.DBF"));
                }
                else if (item == enImportItems.УслугиПациентыОшибкиПослеРазложения)
                {
                    foreach (var smoFolder in Config.Instance.SmoFolders)
                    {
                        ImportList.Add(new DbfFile(nameof(), $"{periodPath}{smoFolder}\\P{Config.Instance.LpuCode}.DBF"));
                        ImportList.Add(new DbfFile(nameof(), $"{periodPath}{smoFolder}\\S{Config.Instance.LpuCode}.DBF"));
                        ImportList.Add(new DbfFile(nameof(), $"{periodPath}{smoFolder}\\E{Config.Instance.LpuCode}.DBF"));
                    }
                    ImportList.Add(new DbfFile(nameof(), $"{periodPath}{Config.Instance.InoFolder}\\I{Config.Instance.LpuCode}.DBF"));
                    ImportList.Add(new DbfFile(nameof(), $"{periodPath}{Config.Instance.InoFolder}\\C{Config.Instance.LpuCode}.DBF"));
                    ImportList.Add(new DbfFile(nameof(), $"{periodPath}{Config.Instance.InoFolder}\\M{Config.Instance.LpuCode}.DBF"));
                }
                else if (item == enImportItems.МедПерсонал)
                    ImportList.Add(new DbfFile(nameof(), $"{Config.Instance.RelaxPath}BASE\\DESCR\\MEDPERS.DBF"));

                else if (item == enImportItems.КлассификаторУслуг)
                    ImportList.Add(new DbfFile(nameof(), $"{Config.Instance.RelaxPath}BASE\\COMMON\\KMU.DBF"));

                else if (item == enImportItems.СРЗ)
                    ImportList.Add(new DbfFile(nameof(), $"{Config.Instance.RelaxPath}SRZ\\SRZ.DBF"));
            }



        }



        public class DbfFile
        {
            public string RelatedClassName { get; set; }
            public string FilePath { get; set; }
            public bool Exist { get; set; }
            public DataSet Data { get; set; }


            public DbfFile(string name, string path)
            {
                Data = new DataSet();
                RelatedClassName = name;
                FilePath = path;
                Exist = File.Exists(FilePath);
            }
            public  void ReadFromDbf()
            {
                if (Exist == false)
                    return;

                //устанавливаем байт с кодовой страницей 866 чтобы драйвер читал правильно
                using (FileStream fs = new FileStream(FilePath, FileMode.Open))
                {
                    fs.Seek(29, SeekOrigin.Begin);
                    if (fs.ReadByte() != 101)
                    {
                        fs.Seek(-1, SeekOrigin.Current);
                        fs.WriteByte(101);
                    }
                }
                
                string connectionString = $"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={Path.GetDirectoryName(FilePath)};Extended Properties = dBASE IV; User ID = Admin; Password =; ";
                using (var connection = new OleDbConnection(connectionString))
                {
                    var req = $"select * from {Path.GetFileName(FilePath)}";
                    var command = new OleDbCommand(req, connection);
                    connection.Open();

                    var da = new OleDbDataAdapter(command);
                    da.Fill(Data);
                }

                
            }
        }
        
    }
}
