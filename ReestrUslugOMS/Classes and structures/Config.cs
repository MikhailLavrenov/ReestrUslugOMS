using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Threading.Tasks;

namespace ReestrUslugOMS.Classes_and_structures
{
    /// <summary>
    /// Настройки программы.
    /// </summary>
    public class Config
    {
        /// <summary>
        /// "ЭКземпляр настроек запущенного приложения.
        /// </summary>
        public RunTime Runtime { get; private set; }
        /// <summary>
        /// Имя microsoft sql сервера (сервер\экземпляр).
        /// </summary>
        public string SqlServer { get; private set; }
        /// <summary>
        /// Имя базы данных.
        /// </summary>
        public string SqlDataBase { get; private set; }
        /// <summary>
        /// Имя для входа в базу данных.
        /// </summary>
        public string DbUser { get; private set; }
        /// <summary>
        /// Пароль для входа в базу данных.
        /// </summary>
        public string DbPass { get; private set; }
        /// <summary>
        /// SQL сервер установлен локально.
        /// </summary>
        public bool SqlServerOnLocalMachine { get; private set; }
        /// <summary>
        /// Organizational Unit доменных пользователей.
        /// </summary>
        public string DomainUsersOU { get; private set; }
        /// <summary>
        /// Organizational Unit доменных групп безопасности.
        /// </summary>
        public string DomainGroupsOU { get; private set; }
        /// <summary>
        /// Код медицинской организации по ФОМСу.
        /// </summary>
        public string LpuCode { get; private set; }
        private string _RelaxPath;
        /// <summary>
        /// Путь к директории Релакса.
        /// </summary>
        public string RelaxPath
        {
            get { return _RelaxPath; }
            set { _RelaxPath = value.Last() == '\\' ? value : $"{value}\\"; }
        }
        /// <summary>
        /// Названия папок страховых компаний в Релаксе. Содержат разложенные по страховым компаниям реестры-счетов.
        /// </summary>
        public string[] SmoFolders { get; set; }
        /// <summary>
        /// Названия папки иногородних в Релаксе. Содержит разложенные по страховым компаниям реестры-счетов.
        /// </summary>
        public string InoFolder { get; set; }


        /// <summary>
        /// Путь к отчету по объемам медицинской помощи.
        /// </summary>
        public string PathReportXlsx { get; private set; }
        /// <summary>
        /// Название строки отчета, по которой проверяется отклонение от контрольной суммы. Отклонение сигнализирует, что пропущены или дублируются коды врачей.
        /// </summary>
        public string ReportCheckSumNodeName { get; private set; }
        /// <summary>
        /// Ссылка на экземпляр настроек. 
        /// </summary>
        public static Config Instance { get; private set; }

        /// <summary>
        /// Статический конструктор
        /// </summary>
        static Config()
        {
            if (Instance == null)
            {
                Instance = new Config();
                Instance.Runtime = new RunTime();
            }
        }
        /// <summary>
        /// Закрытый конструктор.
        /// </summary>
        private Config()
        {
            //работа
            SqlServer = @"DBAPP-SRV1\EXPRESS2017";
            DbUser = "";
            DbPass = "";

            ////дом
            //sqlServer = @"WS2016\SQLEXPRESS";
            //dbUser = "sa";
            //dbPass = "1";

            SqlDataBase = @"REESTR_FOMS";
            DomainUsersOU = @"Все пользователи";
            DomainGroupsOU = @"Все пользователи";

            LpuCode = "2101008";
            ReportCheckSumNodeName = "Расхождение (Контрольная сумма)";

            RelaxPath = @"P:\";
            PathReportXlsx = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\report.xlsx";

            SmoFolders = new string[] {"F7","F8","F9","FB", "FE" };

            SqlServerOnLocalMachine = false;
            if (SqlServer.IndexOf(Environment.MachineName, StringComparison.CurrentCultureIgnoreCase) > 0)
                SqlServerOnLocalMachine = true;
        }
        /// <summary>
        /// Настройки запущенного приложения.
        /// </summary>
        public class RunTime
        {
            /// <summary>
            /// Строка подключения Entity Framework 
            /// </summary>
            public string EFConnectionString { get; private set; }
            /// <summary>
            /// Строка подключения ADO Microsft Sql Server
            /// </summary>
            public string MsSqlConnectionString { get; private set; }
            /// <summary>
            /// Полное квалифицированное имя домена (FQDN)
            /// </summary>
            public string DomainName { get; private set; }
            /// <summary>
            /// ФОИ текущего пользователя
            /// </summary>
            public string CurrentUserName { get; private set; }
            /// <summary>
            /// Права текущего пользователя на доступ к разделам программы
            /// </summary>
            public dbtUser CurrentUser { get; private set; }
            /// <summary>
            /// Работа с базой данных через ADO
            /// </summary>
            public MSSQLDB db { get; set; }
            /// <summary>
            /// Работа с базой данных через Entity Framework
            /// </summary>
            public EFModel dbContext { get; set; }

            /// <summary>
            /// Конструктор
            /// </summary>
            public RunTime()
            {
                string security;

                if (Instance.DbUser != "" && Instance.DbPass != "")
                    security = string.Format(" User ID={0};Password={1}", Instance.DbUser, Instance.DbPass);
                else
                    security = "Integrated Security=SSPI";

                //MS SQL Server
                MsSqlConnectionString = string.Format("Data Source={0}; Initial Catalog={1}; {2}", Instance.SqlServer, Instance.SqlDataBase, security);

                //EntityFramework
                EFConnectionString = "metadata=res://*/{0}.csdl|res://*/{0}.ssdl|res://*/{0}.msl;provider=System.Data.SqlClient;provider connection string=\"data source = {1}; initial catalog = {2}; {3}; MultipleActiveResultSets = True; App = EntityFramework\" ";
                EFConnectionString = string.Format(EFConnectionString, "EFModel", Instance.SqlServer, Instance.SqlDataBase, security);
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
                    string server = DomainName = curentUser.Context.ConnectedServer;
                    if (curentUser.ContextType != ContextType.Domain)
                        DomainName = "";
                    else
                        DomainName = server.Substring(server.IndexOf('.') + 1).ToLower();
                }


            }
        }
    }
}
