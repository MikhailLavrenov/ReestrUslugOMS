using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

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
                        ImportList.Add(new Item(enImportTableNames.Patient, $"{periodPath}{smoFolder}\\P{Config.Instance.LpuCode}.DBF", Period, false, smoFolder));
                        ImportList.Add(new Item(enImportTableNames.Service, $"{periodPath}{smoFolder}\\S{Config.Instance.LpuCode}.DBF", Period, false, smoFolder));
                        ImportList.Add(new Item(enImportTableNames.Error, $"{periodPath}{smoFolder}\\E{Config.Instance.LpuCode}.DBF", Period, false, smoFolder));
                    }
                    ImportList.Add(new Item(enImportTableNames.Patient, $"{periodPath}{Config.Instance.InoFolder}\\I{Config.Instance.LpuCode}.DBF", Period,false,"IN"));
                    ImportList.Add(new Item(enImportTableNames.Service, $"{periodPath}{Config.Instance.InoFolder}\\C{Config.Instance.LpuCode}.DBF", Period, false, "IN"));
                    ImportList.Add(new Item(enImportTableNames.Error, $"{periodPath}{Config.Instance.InoFolder}\\M{Config.Instance.LpuCode}.DBF", Period, false, "IN"));
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
                    Organisation = item.Organisation,
                    DateTime = DateTime.Now,
                    Period = item.Period,
                    Status = item.DbfExist ? enImportStatus.Begin : enImportStatus.FileNotFound,
                    Count = 0,
                };
                Config.Instance.Runtime.dbContext.dbtImportHistory.Add(history);
                Config.Instance.Runtime.dbContext.SaveChanges();

                if (item.DbfExist == false)
                    continue;

                insertedCount = -1;
                //try
                {
                    insertedCount = item.Import();
                }
               // catch (Exception) { }
               // finally
                {
                    history.DateTime = DateTime.Now;
                    if (insertedCount == -1)
                        history.Status = enImportStatus.Failed;
                    else if (insertedCount == 0)
                        history.Status = enImportStatus.Warning;
                    else
                        history.Status = enImportStatus.End;

                    history.Count = insertedCount >=0 ? insertedCount : 0;
                    Config.Instance.Runtime.dbContext.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Класс описывает импортируемый объект и выполняет импорт данных.
        /// </summary>
        public class Item
        {
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
            /// Название страховой организации
            /// </summary>
            public string Organisation { get; set; }

            /// <summary>
            /// Конструктор
            /// </summary>
            /// <param name="sqlTable">Название sql таблицы</param>
            /// <param name="dbfPath">Полный путь к dbf файлу</param>
            /// <param name="period">Период импорта. Если нет null.</param>
            /// <param name="encoded">Необходимость расшифровать строки. True - да, False - нет. По умолчанию - нет.</param>
            public Item(enImportTableNames sqlTable, string dbfPath, DateTime? period = null, bool encoded = false, string organisation = "")
            {
                DbfPath = dbfPath;
                DbfExist = File.Exists(DbfPath);
                SqlTable = sqlTable;
                Period = period;
                Encoded = encoded;
                Organisation = organisation;
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
            /// Загружает из dbf и подготавливает данные для загрузки в sql таблицу.
            /// </summary>
            /// <returns>Данные для загрузки в sql таблицу.</returns>
            public DataTable GetSqlData()
            {
                var SqlData = Config.Instance.Runtime.db.Select($"select top 0 * from {SqlTable}");

                using (Stream strm = new FileStream(DbfPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var tab = NDbfReader.Table.Open(strm))
                {
                    var reader = tab.OpenReader(Encoding.GetEncoding(866));

                    var dbfFields = tab.Columns.Select(x => new { Column = x, Name = Tools.DropNullCharacter(x.Name), x.Type }).ToList();
                    var sqlFields = SqlData.Columns.Cast<DataColumn>().Select(x => new { Name = x.ColumnName, Type = x.DataType }).ToList();

                    var commonFields = (from dbfField in dbfFields
                                        join sqlField in sqlFields
                                        on new { dbfField.Name, dbfField.Type } equals new { sqlField.Name, sqlField.Type }
                                        select new { dbfField.Name, dbfField.Column, dbfField.Type })
                                        .ToList();

                    while (reader.Read())
                    {
                        var newRow = SqlData.NewRow();

                        foreach (var commonField in commonFields)
                        {
                            newRow[commonField.Name] = reader.GetValue(commonField.Column);
                            if (Period != null)
                                newRow["Period"] = Period;
                        }

                        SqlData.Rows.Add(newRow);
                    }

                    if (Encoded)
                        foreach (var column in commonFields.Select(x => x.Name))
                            if (new string[] { "FAM", "IM", "OT" }.Contains(column.ToUpper()))
                                foreach (var row in SqlData.AsEnumerable())
                                    row[column] = Decode(row[column].ToString());

                    return SqlData;
                }
            }
            /// <summary>
            /// Записывает данные в sql таблицу.
            /// </summary>
            /// <param name="sqlData">Данные для загрузки в sql таблицу</param>
            /// <returns>Количество загруженных строк.</returns>
            private int SaveSqlData(DataTable sqlData)
            {
                var sql = new StringBuilder();
                string tempSqlTableName = $"#temp{new Random().Next(10000000, 99999999)}{SqlTable}";

                string periodFilter = Period == null ? "" : $" where period='{Period?.Date}' ";

                sql.AppendLine($@"IF OBJECT_ID('tempdb..{tempSqlTableName}') IS NOT NULL DROP TABLE {tempSqlTableName}");
                sql.AppendLine($@"select top 0 * into {tempSqlTableName} from {SqlTable}");
                Config.Instance.Runtime.db.Execute(sql.ToString());

                Config.Instance.Runtime.db.BulkInsert(tempSqlTableName, sqlData);

                sql = new StringBuilder();
                sql.AppendLine("BEGIN TRY \nBEGIN TRAN");
                sql.AppendLine($@"ALTER TABLE {tempSqlTableName} DROP COLUMN {SqlTable}Id");
                sql.AppendLine($@"delete from {SqlTable} {periodFilter}");
                sql.AppendLine($@"exec('insert into  {SqlTable} select * from {tempSqlTableName}')");
                sql.AppendLine($@"DROP TABLE {tempSqlTableName}");
                sql.AppendLine($@"select count(1) as result from {SqlTable} {periodFilter}");
                sql.AppendLine("COMMIT TRAN \nEND TRY \nBEGIN CATCH \nROLLBACK TRAN");
                sql.AppendLine($@"select cast(-1 as int) as result");
                sql.AppendLine("END CATCH");

                var dt = Config.Instance.Runtime.db.Select(sql.ToString());

                return (int)dt.Rows[0]["result"];
            }
            /// <summary>
            /// Выполняет импорт данных из dbf файла в sql таблицу.
            /// </summary>
            /// <returns>Количество импортированных строк.</returns>
            public int Import()
            {
                var data = GetSqlData();
                var result = SaveSqlData(data);

                return result;
            }
        }

    }
}
