using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Text;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

namespace ReestrUslugOMS.Classes_and_structures
{
    /// <summary>
    /// Текущие настройки программы
    /// </summary>
    public class Config
    {
        public RunTime Runtime { get; private set; }

        //SQL сервер
        public string sqlServer { get; private set; }
        public string sqlDataBase { get; private set; }
        public string dbUser { get; private set; }
        public string dbPass { get; private set; }
        public bool SqlServerOnLocalMachine { get; private set; }

        //Учетные записи                
        public string ADUsersOU { get; private set; }
        public string ADGroupsOU { get; private set; }

        //Релакс
        public string lpuCode { get; private set; }

        //Отчет
        public string PathReportXlsx { get; private set; }

        //Singleton
        public static Config Instance { get; private set; }
        public static Config Create()
        {
            if (Instance == null)
                Instance = new Config();

            Instance.Runtime = new RunTime();

            return Instance;
        }

        private Config()
        {
            //работа
            sqlServer = @"DBAPP-SRV1\EXPRESS2017";            
            dbUser = "";
            dbPass = "";

            ////дом
            //sqlServer = @"WS2016\SQLEXPRESS";
            //dbUser = "sa";
            //dbPass = "1";

            sqlDataBase = @"REESTR_FOMS";
            ADUsersOU = @"Все пользователи";
            ADGroupsOU = @"Все пользователи";

            lpuCode = "2101008";

            PathReportXlsx = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\report.xlsx";

            SqlServerOnLocalMachine = false;
            if (sqlServer.IndexOf(Environment.MachineName, StringComparison.CurrentCultureIgnoreCase) > 0)
                SqlServerOnLocalMachine = true;
        }

        /// <summary>
        /// Настройки и параметры текущего сеанса (процесса)
        /// </summary>
        public class RunTime
        {
            public string EFConnectionString { get; private set; }
            public string MsSqlConnectionString { get; private set; }

            public string ADDomainName { get; private set; }
            public string CurrentUserName { get; private set; }
            public dbtUser CurrentUser { get; private set; }

            public MSSQLDB db;
            public EFModel dbContext;


            public RunTime()
            {
                string security;

                if (Instance.dbUser != "" && Instance.dbPass != "")
                    security = String.Format(" User ID={0};Password={1}", Instance.dbUser, Instance.dbPass);
                else
                    security = "Integrated Security=SSPI";

                //MS SQL Server
                MsSqlConnectionString = String.Format("Data Source={0}; Initial Catalog={1}; {2}", Instance.sqlServer, Instance.sqlDataBase, security);
                db = new MSSQLDB(MsSqlConnectionString);
                //EntityFramework
                EFConnectionString = "metadata=res://*/{0}.csdl|res://*/{0}.ssdl|res://*/{0}.msl;provider=System.Data.SqlClient;provider connection string=\"data source = {1}; initial catalog = {2}; {3}; MultipleActiveResultSets = True; App = EntityFramework\" ";
                EFConnectionString = string.Format(EFConnectionString, "EFModel", Instance.sqlServer, Instance.sqlDataBase, security);
                dbContext = new EFModel(EFConnectionString);
                dbContext.Configuration.ProxyCreationEnabled = false;
                

                //подставляем текущего пользователя и домен
                using (var curentUser = UserPrincipal.Current)
                {
                    string sid = curentUser.Sid.ToString();
                    Task.WaitAll();
                    CurrentUser = dbContext.dbtUser.Where(x => x.Sid == sid).FirstOrDefault();

                    if (CurrentUser == null)
                    {
                        CurrentUser = new dbtUser();
                        CurrentUser.Login = curentUser.UserPrincipalName;
                        CurrentUser.Sid = sid;
                    }

                    CurrentUserName = curentUser.Name;
                    string server = ADDomainName = curentUser.Context.ConnectedServer;
                    if (curentUser.ContextType != ContextType.Domain)
                        ADDomainName = "";
                    else
                        ADDomainName = server.Substring(server.IndexOf('.') + 1).ToLower();
                }


            }
        }
    }
}
