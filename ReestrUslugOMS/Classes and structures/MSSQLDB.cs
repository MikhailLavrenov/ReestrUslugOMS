using System;
using System.Data.SqlClient;
using System.Data;
using ReestrUslugOMS.Classes_and_structures;

namespace ReestrUslugOMS
{
    /// <summary>
    /// Класс для работы с БД microsoft sql server через ADO
    /// </summary>
    public class MSSQLDB : IDisposable
    {
        private SqlConnection Connection;
        public bool NeedDispose { get; private set; }


        /// <summary>
        /// Конструктор, принимает строку подлючения
        /// </summary>
        /// <param name="connectionString">Строка подключения</param>
        public MSSQLDB()
        {
            Connection = new SqlConnection(Config.Instance.Runtime.MsSqlConnectionString);
            Connection.Open();
            NeedDispose = true;
        }
        /// <summary>
        /// Выполняет sql запрос, возвращающий значения (select)
        /// </summary>
        /// <param name="sql">SQL запрос</param>
        /// <returns>Результат выполнения SQL запроса</returns>
        public DataTable Select (string sql)
        {
            var da = new SqlDataAdapter(sql, Connection);
            var dt = new DataTable();
            da.Fill(dt);
            
            return dt;
        }
        /// <summary>
        /// Выполняет sql запрос, не возвращающий значения (insert, update, delete ...)
        /// </summary>
        /// <param name="sql">SQL запрос</param>
        public void Execute (string sql)
        {            
            var command = new SqlCommand(sql,Connection);
            command.ExecuteNonQuery();
        }
        /// <summary>
        /// Вставляет (insert) строки в таблицу sql-сервера.
        /// </summary>
        /// <param name="tableName">Название таблицы для вставки</param>
        /// <param name="data">Вставляемые строки</param>
        public void BulkInsert(string tableName, DataTable data)
        {
            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(Connection))
            {                
                bulkCopy.DestinationTableName = tableName;
                bulkCopy.WriteToServer(data);
            }
        }
        /// <summary>
        /// Закрывает подключение к базе данных и освобождает ресурсы
        /// </summary>
        public void Dispose()
        {           
            Connection.Dispose();
            NeedDispose = false;
        }
    }
}
