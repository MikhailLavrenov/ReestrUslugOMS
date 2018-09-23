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


//        WITH tab  AS(
//SELECT 0 as ReportId primary key, 0 as ParentId, cast('root' as varchar(1000) ) as [name], cast(1 as varchar(50)) as [order], 0 as [level]
//        UNION ALL
//SELECT re.ReportId, re.parentId, cast(tab.name+' -> '+re.name as varchar(1000)) as name, cast(tab.[order]+'.'+re.[order] as varchar(50)) as [order], tab.level+1 from Report re
//join tab on re.parentId=tab.ReportId
//) 
//SELECT ReportId, name, [order] FROM tab  order by[order]



        static public string RecursiveReportReq = @"
                                                        WITH tab  AS(
                                                        SELECT ReportId, parentId, cast(name as varchar(1000)) as name, cast([order] as varchar(50)) as [order], 0 as [level]
                                                        from Report where parentId = 0
                                                        UNION ALL
                                                        SELECT hi.ReportId, hi.parentId, cast(tab.name+' -> '+hi.name as varchar(1000)) as name, cast(tab.[order]+'.'+hi.[order] as varchar(50)) as [order], tab.level+1 from Report hi
                                                        join tab on hi.parentId=tab.ReportId 
                                                        ) 
                                                        ";
        static public string RowsColumnsReq = @"
                                                {0}
                                                select hi.*, tab.level from Report hi
                                                join tab  on hi.ReportId=tab.ReportId and tab.name like '{1}%'
                                                {2}
                                                order by tab.[order]
                                                ";

        static public string RowsReq = string.Format(RowsColumnsReq, RecursiveReportReq, "Строки", null);
        static public string ColumnsReq = string.Format(RowsColumnsReq, RecursiveReportReq, "Столбцы", null);
        static public string DocPlanRowsReq = string.Format(RowsColumnsReq, RecursiveReportReq, "Строки", @"where hi.name like 'План' or hi.name like 'План.вр' or len(hi.calcdata)<2 or hi.calcdata is null ");
        static public string DepPlanRowsReq = string.Format(RowsColumnsReq, RecursiveReportReq, "Строки", @"where hi.name like 'План.отд' or len(hi.calcdata)<2 or hi.calcdata is null ");

        static public string BillsReq = @"

                                            DECLARE @god1 int;
                                            DECLARE @month1 int;											                                            
											DECLARE @god2 int;
                                            DECLARE @month2 int;
                                            DECLARE @mcod varchar(7);

											SET @month1 = {0};
											SET @god1 = {1};
                                            SET @month2 = {2};
                                            SET @god2 = {3};
                                            SET @mcod = '{4}';

                                            with tab as
                                            (
                                            select patu.cod, patu.otd, patu.tn1,1 as usl, patu.k_u, kmu.uet1*k_u as uet,
                                            CASE WHEN patu.YEAR-datepart(yyyy,dr)<=3 THEN 1 ELSE 0  END AS do3, 
                                            CASE WHEN patu.YEAR-datepart(yyyy,dr)<=3 THEN 0 ELSE 1 END AS posle3, 
                                            CASE WHEN ERR.RECID is not null THEN 1 ELSE 0 END AS err,
                                            CASE WHEN Q like 'IN' THEN 1 ELSE 0 END AS ino
                                            from patu 
                                            left outer join pat on patu.c_i=pat.c_i and patu.sn_pol=pat.sn_pol and pat.month=patu.month and pat.year=patu.year
                                            left outer join err on err.month=patu.month and err.year=patu.year and err.mcod=@mcod and patu.recid=err.recid and err.f = 'S' 
                                            left outer join kmu on kmu.code=patu.cod 
											where patu.year*100+patu.month between @god1*100+@month1 and @god2*100+@month2
                                            )

                                            select ltrim(cod) as [код_услуги],ltrim(otd) as [код_отделения],ltrim(tn1) as [код_врача],sum(usl) as [сумма_услуг],sum(k_u) as [произведение_услуг],sum(uet) as [сумма_ует],sum(do3) as [сумма_услуг_до_3_лет],sum(posle3) as [сумма_услуг_после_3_лет],1 as vsevr,0 as v,0 as u
                                            from tab
                                            {5}
                                            group by cod,otd,tn1
                                            order by tn1,otd,cod                                            
                                            ";

        static public string PlanPeriodReq= @"
                                        DECLARE @god1 int;
                                        DECLARE @month1 int;
                                        DECLARE @god2 int;
                                        DECLARE @month2 int;
                                        DECLARE @type int;

										SET @month1 = {0};
										SET @god1 = {1};
                                        SET @month2 = {2};
                                        SET @god2 = {3};
                                        SET @type = {4};

                                        select rowId, colId, sum(value) value from [Plan] 
                                        where type=@type and year*100+month between @god1*100+@month1 and @god2*100+@month2
                                        group by rowId, colId, value
                                        ";

        static public string PlanReq = @"
                                        DECLARE @god1 int;
                                        DECLARE @month1 int;
                                        DECLARE @type int;

										SET @month1 = {0};
										SET @god1 = {1};
                                        SET @type = {2};

                                        select PlanId, rowId, colId, value value from [Plan] 
                                        where type=@type and month=@month1 and year=@god1
                                        ";


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
