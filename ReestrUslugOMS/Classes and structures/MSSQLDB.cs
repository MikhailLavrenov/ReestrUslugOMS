using System;
using System.Data.SqlClient;
using System.Data;

namespace ReestrUslugOMS
{
    /// <summary>
    /// Класс для работы с БД microsoft sql server через ADO
    /// </summary>
    public class MSSQLDB : IDisposable
    {
        private SqlCommand command;
        private SqlConnection connection;
        private SqlDataAdapter da;

        /// <summary>
        /// Конструктор, принимает строку подлючения
        /// </summary>
        /// <param name="connectionString">Строка подключения</param>
        public MSSQLDB(string connectionString)
        {
            command = new SqlCommand();
            connection = new SqlConnection(connectionString);
            connection.Open();
        }
        /// <summary>
        /// Выполняет sql запрос, возвращающий значения (select)
        /// </summary>
        /// <param name="sql">SQL запрос</param>
        /// <returns>Результат выполнения SQL запроса</returns>
        public DataTable Select (string sql)
        {
            DataTable dt;

            da = new SqlDataAdapter(sql, connection);
            dt = new DataTable();
            da.Fill(dt);

            return dt;
        }
        /// <summary>
        /// Выполняет sql запрос, не возвращающий значения (insert, update, delete ...)
        /// </summary>
        /// <param name="sql">SQL запрос</param>
        public void Execute (string sql)
        {
            command = new SqlCommand(sql,connection);
            command.ExecuteNonQuery();
        }
        /// <summary>
        /// Закрывает подключение к базе данных и освобождает ресурсы
        /// </summary>
        public void Dispose()
        {           
            connection.Close();
            connection.Dispose();
            command.Dispose();
        }
    }
}
