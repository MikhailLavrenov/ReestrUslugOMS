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
                    ImportList.Add(new Item($"{periodPath}PAT.DBF", enImportTableNames.Patient, period: Period));
                    ImportList.Add(new Item($"{periodPath}PATU.DBF", enImportTableNames.Service, period: Period));
                }
                else if (item == enImportItems.УслугиПациентыОшибкиПослеРазложения)
                {
                    foreach (var smoFolder in Config.Instance.SmoFolders)
                    {
                        ImportList.Add(new Item($"{periodPath}{smoFolder}\\P{Config.Instance.LpuCode}.DBF", enImportTableNames.Patient,  Period, smoFolder));
                        ImportList.Add(new Item($"{periodPath}{smoFolder}\\S{Config.Instance.LpuCode}.DBF", enImportTableNames.Service,  Period, smoFolder));
                        ImportList.Add(new Item($"{periodPath}{smoFolder}\\E{Config.Instance.LpuCode}.DBF", enImportTableNames.Error,  Period, smoFolder));
                    }
                    ImportList.Add(new Item($"{periodPath}{Config.Instance.InoFolder}\\I{Config.Instance.LpuCode}.DBF", enImportTableNames.Patient,  Period, "IN"));
                    ImportList.Add(new Item($"{periodPath}{Config.Instance.InoFolder}\\C{Config.Instance.LpuCode}.DBF", enImportTableNames.Service,  Period, "IN"));
                    ImportList.Add(new Item($"{periodPath}{Config.Instance.InoFolder}\\M{Config.Instance.LpuCode}.DBF", enImportTableNames.Error,  Period, "IN"));
                }
                else if (item == enImportItems.МедПерсонал)
                    ImportList.Add(new Item($"{Config.Instance.RelaxPath}BASE\\DESCR\\MEDPERS.DBF", enImportTableNames.Doctor));

                else if (item == enImportItems.КлассификаторУслуг)
                    ImportList.Add(new Item($"{Config.Instance.RelaxPath}BASE\\COMMON\\KMU.DBF", enImportTableNames.ServiceList));

                else if (item == enImportItems.СРЗ)
                    ImportList.Add(new Item($"{Config.Instance.RelaxPath}SRZ\\SRZ.DBF", enImportTableNames.PreventiveExam, encoded: true));
            }
        }

        public void Import()
        {
            foreach (var group in ImportList.GroupBy(x => x.SqlTable))
            {
                var tempTableName = CreateTempTable(group.Key);

                foreach (var item in group)
                    item.ImportToTempTable(tempTableName);
                
                var insertCount = ImportToMainTable(group.Key, tempTableName, group.First().Period);
                var history = new dbtImportHistory()
                {
                    Table = group.Key,
                    Period = group.First().Period,
                    DateTime = DateTime.Now,
                    Count = insertCount >= 0 ? insertCount : 0,
                };

                if (insertCount > 0)
                    history.Status = enImportStatus.OK;
                else if (insertCount == -1)
                    history.Status = enImportStatus.Error;
                else if (insertCount == 0)
                    history.Status = enImportStatus.Warning;

                if (group.Count() > 1)
                {
                    var details = new StringBuilder();

                    foreach (var item in group)
                    {
                        var detail = item.DbfExist ? item.ImportCount.ToString() : "Нет файла";
                        details.Append($"{item.Organisation}: {detail}; ");
                    }
                    history.Details = details.ToString();
                }

                Config.Instance.Runtime.dbContext.dbtImportHistory.Add(history);
                Config.Instance.Runtime.dbContext.SaveChanges();
            }
        }
        /// <summary>
        /// Создает новую временную таблицу для импорта
        /// </summary>
        private string CreateTempTable(enImportTableNames tableName)
        {
            var tempTableName = $"#{tableName}{Guid.NewGuid().ToString("N")}";

            var sql = new StringBuilder();
            sql.AppendLine($@"IF OBJECT_ID('tempdb..{tempTableName}') IS NOT NULL DROP TABLE {tempTableName}");
            sql.AppendLine($@"select top 0 * into {tempTableName} from {tableName}");
            Config.Instance.Runtime.db.Execute(sql.ToString());

            return tempTableName;
        }
        /// <summary>
        ///  Перемещает данные на sql сервере из временной таблицы в основную.
        /// </summary>
        public int ImportToMainTable(enImportTableNames mainTable, string tempTable, DateTime? period)
        {
            var sql = new StringBuilder();
            var periodFilter = period==null ? "" : $" where period='{period?.Date}' ";

            sql.AppendLine("BEGIN TRY \nBEGIN TRAN");
            sql.AppendLine($@"ALTER TABLE {tempTable} DROP COLUMN {mainTable}Id");
            sql.AppendLine($@"delete from {mainTable} {periodFilter}");
            sql.AppendLine($@"exec('insert into  {mainTable} select * from {tempTable}')");            
            sql.AppendLine($@"select count(1) as result from {tempTable}");
            sql.AppendLine($@"DROP TABLE {tempTable}");
            sql.AppendLine("COMMIT TRAN \nEND TRY \nBEGIN CATCH \nROLLBACK TRAN");
            sql.AppendLine($@"select cast(-1 as int) as result");
            sql.AppendLine("END CATCH");

            var dt = Config.Instance.Runtime.db.Select(sql.ToString());
            return (int)dt.Rows[0]["result"];
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
            public DateTime? Period { get; set; }
            /// <summary>
            /// Содержит зашифрованные строки - true, иначе - false.
            /// </summary>
            public bool Encoded { get; set; }
            /// <summary>
            /// Название страховой организации
            /// </summary>
            public string Organisation { get; set; }
            /// <summary>
            /// Количество импортированных строк
            /// </summary>
            public dbtImportHistory Result { get; private set; }
            public int ImportCount { get; private set; }

            /// <summary>
            /// Конструктор
            /// </summary>
            /// <param name="sqlTable">Название sql таблицы</param>
            /// <param name="dbfPath">Полный путь к dbf файлу</param>
            /// <param name="period">Период импорта. Если нет null.</param>
            /// <param name="encoded">Необходимость расшифровать строки. True - да, False - нет. По умолчанию - нет.</param>
            public Item(string dbfPath, enImportTableNames sqlTable, DateTime? period = null, string organisation = "", bool encoded = false)
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
            private DataTable GetSqlData()
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
            /// Импортирует данные на sql сервере во временную таблицу.
            /// </summary>
            /// <param name="sqlData">Данные для загрузки в sql таблицу.</param>
            public void ImportToTempTable(string tempSqlTable)
            {
                if (DbfExist)
                {
                    var periodFilter = Period == null ? "" : $" where period='{Period?.Date}' ";
                    var data = GetSqlData();

                    Config.Instance.Runtime.db.BulkInsert(tempSqlTable, data);

                    ImportCount = data.Rows.Count;
                }
            }
        }
    }
}

