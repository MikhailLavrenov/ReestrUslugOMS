using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using ReestrUslugOMS;


namespace ReestrUslugOMS.Classes_and_structures
{
    /// <summary>
    /// Класс, описывает импортируемые сущности из dbf файлов в sql таблицы и выполняет импорт данных. Записывает ход выполнения в sql таблицу.
    /// </summary>
    class ImportDbf
    {
        /// <summary>
        /// Список сущностей, подлежащий импорту.
        /// </summary>
        public List<Item> ImportList { get; private set; }
        /// <summary>
        /// Период, за который импортируются данные.
        /// </summary>
        public DateTime Period { get; private set; }

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="period">Задает свойство Period</param>
        /// <param name="importItems">Список перечислений, соотвествующий названиям импортируемых сущностей.</param>
        public ImportDbf(DateTime period, List<enImportItems> importItems)
        {
            ImportList = new List<Item>();
            Period = period.AddDays(15 - period.Day);

            string periodPath = $"{Config.Instance.RelaxPath}OUTS{Period:yyyy}\\PERIOD{Period:MM}\\";
            foreach (var item in importItems)
            {
                if (item == enImportItems.УслугиПациентыДоРазложения)
                {
                    ImportList.Add(new Item(enImportTableNames.Patient, $"{periodPath}PAT.DBF", Period));
                    ImportList.Add(new Item(enImportTableNames.Service, $"{periodPath}PATU.DBF", Period));
                }
                else if (item == enImportItems.УслугиПациентыОшибкиПослеРазложения)
                {
                    foreach (var smoFolder in Config.Instance.SmoFolders)
                    {
                        ImportList.Add(new Item(enImportTableNames.Patient, $"{periodPath}{smoFolder}\\P{Config.Instance.LpuCode}.DBF", Period));
                        ImportList.Add(new Item(enImportTableNames.Service, $"{periodPath}{smoFolder}\\S{Config.Instance.LpuCode}.DBF", Period));
                        ImportList.Add(new Item(enImportTableNames.Error, $"{periodPath}{smoFolder}\\E{Config.Instance.LpuCode}.DBF", Period));
                    }
                    ImportList.Add(new Item(enImportTableNames.Patient, $"{periodPath}{Config.Instance.InoFolder}\\I{Config.Instance.LpuCode}.DBF", Period));
                    ImportList.Add(new Item(enImportTableNames.Service, $"{periodPath}{Config.Instance.InoFolder}\\C{Config.Instance.LpuCode}.DBF", Period));
                    ImportList.Add(new Item(enImportTableNames.Error, $"{periodPath}{Config.Instance.InoFolder}\\M{Config.Instance.LpuCode}.DBF", Period));
                }
                else if (item == enImportItems.МедПерсонал)
                    ImportList.Add(new Item(enImportTableNames.Doctor, $"{Config.Instance.RelaxPath}BASE\\DESCR\\MEDPERS.DBF"));

                else if (item == enImportItems.КлассификаторУслуг)
                    ImportList.Add(new Item(enImportTableNames.ServiceList, $"{Config.Instance.RelaxPath}BASE\\COMMON\\KMU.DBF"));

                else if (item == enImportItems.СРЗ)
                    ImportList.Add(new Item(enImportTableNames.PreventiveExam, $"{Config.Instance.RelaxPath}SRZ\\SRZ.DBF", encoded: true));
            }
        }

        /// <summary>
        /// Выполняет импорт каждой сущности из dbf файла в sql таблицу. Записывает ход выполнения в sql таблицу.
        /// </summary>
        public void Import()
        {
            dbtImportHistory history;
            int insertedCount;

            foreach (var item in ImportList)
            {

                history = new dbtImportHistory
                {
                    Table = item.SqlTable,
                    DateTime = DateTime.Now,
                    Period = item.Period,
                    Status = enImportStatus.Begin,
                    Count = 0,
                };
                Config.Instance.Runtime.dbContext.dbtImportHistory.Add(history);
                Config.Instance.Runtime.dbContext.SaveChanges();

                insertedCount = 0;
                try
                {
                    insertedCount = item.Import();
                }
                catch (Exception) { }
                finally
                {
                    history.DateTime = DateTime.Now;
                    history.Status = insertedCount == 0 ? enImportStatus.Failed : enImportStatus.End;
                    history.Count = insertedCount;
                    Config.Instance.Runtime.dbContext.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Класс, описывает импортируемую  из dbf файла в sql таблицу сущность, и выполняет импорт.
        /// </summary>
        public class Item
        {
            /// <summary>
            /// Данные из dbf файла.
            /// </summary>
            private DataTable DbfData;
            /// <summary>
            /// Совпадающие по названию поля между dbf файлом и sql таблицей. Соотвествие типов не проверяется.
            /// </summary>
            private List<string> CommonFields;
            /// <summary>
            /// Данные для sql таблицы.
            /// </summary>
            private DataTable SqlData;

            /// <summary>
            /// Полный путь к dbf файлу.
            /// </summary>
            public string DbfPath { get; private set; }
            /// <summary>
            /// Dbf файл доступен.
            /// </summary>
            public bool DbfExist;
            /// <summary>
            /// Название sql таблицы.
            /// </summary>
            public enImportTableNames SqlTable { get; private set; }
            /// <summary>
            /// Период, за который импортируются данные. Если нет - null.
            /// </summary>
            public DateTime? Period { get; private set; }
            /// <summary>
            /// Содержит зашифрованные строки - true, иначе - false.
            /// </summary>
            public bool Encoded { get; set; }

            /// <summary>
            /// Конструктор
            /// </summary>
            /// <param name="sqlName">Название sql таблицы</param>
            /// <param name="dbfPath">Полный путь к dbf файлу</param>
            /// <param name="period">Период импорта. Если нет null.</param>
            public Item(enImportTableNames sqlTable, string dbfPath, DateTime? period = null, bool encoded = false)
            {
                DbfData = new DataTable();
                DbfPath = dbfPath;
                DbfExist = File.Exists(DbfPath);
                SqlTable = sqlTable;
                Period = period;
                Encoded = encoded;
            }

            /// <summary>
            /// Преобразует неверно кодированную строку из dbf файла в нормальную
            /// </summary>
            /// <param name="str">Неверно кодированная строка</param>
            /// <returns>Нормальную строка</returns>
            private static string FixEncoding(string str)
            {
                byte[] strBytes = Encoding.GetEncoding(1252).GetBytes(str);
                byte[] resultBytes = Encoding.Convert(Encoding.GetEncoding(866), Encoding.Default, strBytes);
                return Encoding.Default.GetString(resultBytes);
            }
            /// <summary>
            /// Расшифровывает строки, путем перестановки букв по примитивному алгоритму.
            /// </summary>
            /// <param name="str">Зашифрованная строка.</param>
            /// <returns>Расшифрованная строка.</returns>
            private static string Decode(string str)
            {
                var result = new StringBuilder();

                if (str.Length > 0)
                    for (int i = str.Length - 1; i >= 0; i--)
                        result.Append((char)(str[i] - str.Length + i));

                return result.ToString();
            }
            /// <summary>
            /// Заполняет свойство DbfData.
            /// </summary>
            private void LoadDbfData()
            {
                if (DbfExist == false)
                    return;

                //Если в заголовке указано наличие файла индекса, но его нет - исправляем заголовок, иначе возникнет ошибка драйвера
                if (File.Exists(Path.ChangeExtension(DbfPath, ".cdx")) == false)
                    using (var fs = new FileStream(DbfPath, FileMode.Open))
                    {
                        fs.Seek(28, SeekOrigin.Begin);
                        if (fs.ReadByte() != 0)
                        {
                            fs.Seek(-1, SeekOrigin.Current);
                            fs.WriteByte(0);
                        }
                    }

                string connectionString = $"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={Path.GetDirectoryName(DbfPath)};Extended Properties = dBASE IV; User ID = Admin; Password =; ";
                using (var connection = new OleDbConnection(connectionString))
                {
                    var req = $"select * from {Path.GetFileName(DbfPath)}";
                    var command = new OleDbCommand(req, connection);
                    connection.Open();

                    var da = new OleDbDataAdapter(command);
                    DbfData = new DataTable();
                    da.Fill(DbfData);
                }

                var columns = DbfData.Columns.Cast<DataColumn>().Where(x => x.DataType == typeof(string)).Select(x => x.ColumnName).ToList();
                foreach (var column in columns)
                    foreach (var row in DbfData.AsEnumerable())
                        row[column] = FixEncoding(row[column].ToString());

                if (Encoded)
                    foreach (var column in columns.Select(x => x.ToUpper()).Where(x => x == "FAM" || x == "IM" || x == "OT"))
                        foreach (var row in DbfData.AsEnumerable())
                            row[column] = Decode(row[column].ToString());
            }
            /// <summary>
            /// Создает и заполняет CommonFields.
            /// </summary>
            private void SetCommonFields()
            {
                CommonFields = DbfData.Columns
                    .Cast<DataColumn>()
                    .Select(x => x.ColumnName)
                    .Intersect(SqlData.Columns.Cast<DataColumn>().Select(x => x.ColumnName))
                    .ToList();
            }
            /// <summary>
            /// Заполняет свойство SqlData.
            /// </summary>
            private void SetSqlData()
            {
                DataRow dr;

                foreach (var row in DbfData.AsEnumerable())
                {
                    dr = SqlData.NewRow();
                    if (Period != null)
                        dr["Period"] = Period;

                    foreach (var field in CommonFields)
                        dr[field] = row[field];

                    SqlData.Rows.Add(dr);
                }
            }
            /// <summary>
            /// Записывает данные из свойства SqlData в sql таблицу.
            /// </summary>
            /// <returns>Количество импортированных строк.</returns>
            private int SaveSqlData()
            {
                var sql = new StringBuilder();
                string tempSqlTableName = $"#temp{new Random().Next(10000000, 99999999)}{SqlTable}";

                string periodFilter = Period == null ? "" : $" where period='{Period?.Date}' ";

                sql.AppendLine($@"IF OBJECT_ID('tempdb..{tempSqlTableName}') IS NOT NULL DROP TABLE {tempSqlTableName}");
                sql.AppendLine($@"select top 0 * into {tempSqlTableName} from {SqlTable}");
                Config.Instance.Runtime.db.Execute(sql.ToString());

                Config.Instance.Runtime.db.BulkInsert(tempSqlTableName, SqlData);

                sql = new StringBuilder();
                sql.AppendLine("BEGIN TRY \nBEGIN TRAN");
                sql.AppendLine($@"ALTER TABLE {tempSqlTableName} DROP COLUMN {SqlTable}Id");
                sql.AppendLine($@"delete from {SqlTable} {periodFilter}");
                sql.AppendLine($@"exec('insert into  {SqlTable} select * from {tempSqlTableName}')");
                sql.AppendLine($@"DROP TABLE {tempSqlTableName}");
                sql.AppendLine($@"select count(1) as result from {SqlTable} {periodFilter}");
                sql.AppendLine("COMMIT TRAN \nEND TRY \nBEGIN CATCH \nROLLBACK TRAN");
                sql.AppendLine($@"select cast(0 as int) as result");
                sql.AppendLine("END CATCH");

                var dt = Config.Instance.Runtime.db.Select(sql.ToString());

                return (int)dt.Rows[0]["result"];
            }
            /// <summary>
            /// Выполняет импорт данных из dbf файла в sql таблицу.
            /// </summary>
            /// <returns>Количество импортированных строу.</returns>
            public int Import()
            {
                LoadDbfData();
                SqlData = Config.Instance.Runtime.db.Select($"select top 0 * from {SqlTable}");
                SetCommonFields();
                SetSqlData();
                DbfData = null;
                CommonFields = null;
                var result = SaveSqlData();
                SqlData = null;

                return result;
            }
        }

    }
}
