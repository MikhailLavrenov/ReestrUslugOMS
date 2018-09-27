using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Windows.Forms;

namespace ReestrUslugOMS
{
    public class MSSQLDB : IDisposable
    {
        private SqlCommand command;
        public SqlConnection connection;
        public static string EFConnectionString;
        private SqlDataAdapter da;
        private SqlCommandBuilder cb;
        private DataTable dt;

        public MSSQLDB(string connectionString)
        {
            command = new SqlCommand();
            connection = new SqlConnection(connectionString);
            connection.Open();
        }
        public DataTable Select (string sql)
        {
            da = new SqlDataAdapter(sql, connection);
            cb = new SqlCommandBuilder(da);
            dt = new DataTable();
            da.Fill(dt);
            return dt;
        }
        public void Update(DataTable dt)
        {
            try
            {
                da.Update(dt);
            }
            catch(Exception)
            {
                MessageBox.Show("Ошибка сохранения", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public void Execute (string sql)
        {
            command = new SqlCommand(sql,connection);
            command.ExecuteNonQuery();
        }


        public void Dispose()
        {           
            connection.Close();
            connection.Dispose();
            command.Dispose();
            if (cb != null)
                cb.Dispose();
        }
    }


}
