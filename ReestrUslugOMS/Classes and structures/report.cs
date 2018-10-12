using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using unvell.ReoGrid;
using ReestrUslugOMS.Classes_and_structures;
using unvell.ReoGrid.DataFormat;
using System.Threading.Tasks;

namespace ReestrUslugOMS
{
    /// <summary>
    /// Класс для ввода планов и построения отчета по объемам медицинской помощи
    /// </summary>
    public class Report
    {
        /// <summary>
        /// Строка отчета по которой проверяется отклонение от  контрольной суммы. Отклонение сигнализирует, что пропущены или дублируются коды врачей.
        /// </summary>
        public ExtNode CheckSumRow { get; private set; }
        /// <summary>
        /// Результат проверки отклонения от контрольной суммы. True- отклонение обнаружено. False - не обнаружено. По умолчанию False.
        /// </summary>
        public bool CheckSumFailed { get; private set; }
        /// <summary>
        /// Массив отображаемых строк отчета
        /// </summary>
        public ExtNode[] Rows { get; private set; }
        /// <summary>
        /// Массив отображаемых столбцов отчета
        /// </summary>
        public ExtNode[] Cols { get; private set; }
        /// <summary>
        /// Массив значений ячеек отчета
        /// </summary>
        public double[,] ResultValues { get; private set; }
        /// <summary>
        /// Словарь содержащий наборы значений различных типов
        /// </summary>
        public Dictionary<enDataSource, object> DataSourcesDict { get; set; }
        /// <summary>
        /// Максимальный уровень строк
        /// </summary>
        public int MaxRowLevel { get; private set; }
        /// <summary>
        /// Максимальный уровень столбцов
        /// </summary>
        public int MaxColLevel { get; private set; }
        /// <summary>
        /// Колчество знаков после запятой для округления значений ячеек
        /// </summary>
        public int Round { get; set; }
        /// <summary>
        /// Колчество знаков после запятой для округления процентных значений ячеек
        /// </summary>
        public int PercentRound { get; set; }
        /// <summary>
        /// Вид отчета
        /// </summary>
        public enReportMode ReportType { get; private set; }
        /// <summary>
        /// Начало периода за который рассчитыватются значения. Учитываются только месяц и год.
        /// </summary>
        public DateTime BeginPeriod { get; private set; }
        /// <summary>
        /// Конец периода за который рассчитыватются значения. Учитываются только месяц и год.
        /// </summary>
        public DateTime EndPeriod { get; private set; }
        /// <summary>
        /// Режим расчета значений отчета относительно отклоненных от оплаты услуг
        /// </summary>
        public enErrorMode Errors { get; private set; }
        /// <summary>
        /// Режим построения отчета относительно региона выдачи полиса ОМС (иногородние пациенты)
        /// </summary>
        public enInsuranceMode InsuranceTerritory { get; private set; }


        /// <summary>
        /// Конструктор, принимает значение перечисления вида отчета enReportMode
        /// </summary>
        /// <param name="reportType">Вид отчета</param>
        public Report(enReportMode reportType)
        {
            MaxColLevel = 0;
            MaxRowLevel = 0;
            Round = 0;
            PercentRound = 1;
            DataSourcesDict = new Dictionary<enDataSource, object>();
            ReportType = reportType;           

            SetRowsCols();
            SetPlanSet();

            MaxRowLevel = Rows.Max(x => x.Level);
            MaxColLevel = Cols.Max(x => x.Level);

            CheckSumRow=Rows.Where(x => x.Name == Config.Instance.ReportCheckSumNodeName).FirstOrDefault();
            CheckSumFailed = false;
        }
        /// <summary>
        /// Формирует массив строк и столбцов отчета
        /// </summary>
        private void SetRowsCols()
        {
            dbtNode dbtRoot;
            ExtNode extRoot;

            Config.Instance.Runtime.dbContext.dbtNode.Include(x => x.Formula).Load();
            var nodes = Config.Instance.Runtime.dbContext.dbtNode.Local.ToList();

            //формируем и заполняем массив столбцов
            dbtRoot = nodes.Where(x => x.Name == "Столбцы" && x.Prev == null).First();

            int ind = -1;
            extRoot = new ExtNode(dbtRoot, null, ref ind);
            Cols = extRoot.ToList(1).ToArray();

            //формируем и заполняем массив строк 
            dbtRoot = nodes.Where(x => x.Name == "Строки" && x.Prev == null).First();
            ind = 1;
            extRoot = new ExtNode(dbtRoot, null, ref ind);

            if (ReportType == enReportMode.ПланВрача || ReportType == enReportMode.ПланОтделения)
            {
                //выбираем строки в соответствии с правами пользователя
                extRoot = extRoot.ToList().Where(x => x.NodeId == Config.Instance.Runtime.CurrentUser.NodeId).First();

                //добавляем корневую ноду для структурной целостности и навигации
                if (extRoot.Prev != null)
                    extRoot = extRoot.AddRoot("Строки");
            }

            //отбрасываем ноды, не связанные с конкретным планом
            if (ReportType == enReportMode.ПланВрача || ReportType == enReportMode.ПланОтделения)
                foreach (var row in extRoot.ToList())
                {
                    if (ReportType == enReportMode.ПланВрача)
                        row.Next.RemoveAll(x => x.DataSource != 0 && x.DataSource != enDataSource.ПланВрача && x.Name != "План вр");
                    else if (ReportType == enReportMode.ПланОтделения)
                        row.Next.RemoveAll(x => x.DataSource != 0 && x.DataSource != enDataSource.ПланОтделения && x.Name != "План отд");
                }

            foreach (var row in extRoot.ToList())
                row.Next.RemoveAll(x => x.IsEmptyBranch());

            extRoot.InitializeProperties();
            Rows = extRoot.ToList(1).ToArray();
        }
        /// <summary>
        /// помечает ноды на пересечении которых пользователь может вводить планы
        /// </summary>
        private void SetPlanSet()
        {
            if (ReportType == enReportMode.ПланВрача || ReportType == enReportMode.ПланОтделения)
            {
                foreach (var item in Cols)
                    if (item.ReadOnly == false && item.DataSource != 0)
                        item.PlanSet = true;

                foreach (var item in Rows)
                    if (item.ReadOnly == false && item.DataSource != 0)
                        if ((item.DataSource == enDataSource.ПланВрача && ReportType == enReportMode.ПланВрача) || (item.DataSource == enDataSource.ПланОтделения && ReportType == enReportMode.ПланОтделения))
                            item.PlanSet = true;
            }
        }
        /// <summary>
        /// Устанавливает настройки отчета для рассчета значений ячеек
        /// </summary>
        /// <param name="dateStart">Дата начала периода, включительно (учитываются  месяц и год, день не важен)</param>
        /// <param name="dateEnd">Дата окончания периода, включительно (учитываются  месяц и год, день не важен)</param>
        /// <param name="errors">Режим расчета значений отчета относительно отклоненных от оплаты услуг</param>
        /// <param name="insuranceTerritory">Режим построения отчета относительно региона выдачи полиса ОМС (иногородние пациенты)</param>
        public void SetParams(DateTime dateStart, DateTime dateEnd, enErrorMode errors = (enErrorMode)1, enInsuranceMode insuranceTerritory = (enInsuranceMode)1)
        {
            BeginPeriod = dateStart.FirstDayDate();
            EndPeriod = dateEnd.LastDayDate();
            Errors = errors;
            InsuranceTerritory = insuranceTerritory;
        }
        /// <summary>
        /// Создает словарь с источниками данных для расчетов
        /// </summary>
        private void SetDataSources()
        {
            DataSourcesDict.Clear();

            if (ReportType == enReportMode.Отчет)
            {
                var val1 = Config.Instance.Runtime.dbContext.sp_ReportFact(BeginPeriod.Month, BeginPeriod.Year, EndPeriod.Month, EndPeriod.Year, Config.Instance.LpuCode, (int)Errors, (int)InsuranceTerritory).ToList();
                DataSourcesDict.Add(enDataSource.РеестрыСчетов, val1);

                var val2 = Config.Instance.Runtime.dbContext.dbtPlan.Where(x => x.Type == enReportMode.ПланВрача && x.Period >= BeginPeriod && x.Period <= EndPeriod).ToList();
                DataSourcesDict.Add(enDataSource.ПланВрача, val2);


                var val3 = Config.Instance.Runtime.dbContext.dbtPlan.Where(x => x.Type == enReportMode.ПланОтделения && x.Period >= BeginPeriod && x.Period <= EndPeriod).ToList();
                DataSourcesDict.Add(enDataSource.ПланОтделения, val3);

            }

            else if (ReportType == enReportMode.ПланВрача)
            {
                var val = Config.Instance.Runtime.dbContext.dbtPlan.Where(x => x.Type == enReportMode.ПланВрача && x.Period.Month == BeginPeriod.Month && x.Period.Year == BeginPeriod.Year).ToList();
                DataSourcesDict.Add(enDataSource.ПланВрача, val);
            }

            else if (ReportType == enReportMode.ПланОтделения)
            {
                var val = Config.Instance.Runtime.dbContext.dbtPlan.Where(x => x.Type == enReportMode.ПланОтделения && x.Period.Month == BeginPeriod.Month && x.Period.Year == BeginPeriod.Year).ToList();
                DataSourcesDict.Add(enDataSource.ПланОтделения, val);
            }
        }
        /// <summary>
        /// Рассчитывает значения ячеек отчета за весь период, в т.ч. вычисляет строки с процентами
        /// </summary>
        public void SetResultValues()
        {
            SetDataSources();

            var date = BeginPeriod.AddDays(14);            
            var list = new List<double[,]>();

            while (date.BetweenInMonths(BeginPeriod, EndPeriod))
            {
                list.Add(SetResultValues(date));
                date = date.AddMonths(1);
            };

            ResultValues = new double[Rows.Length, Cols.Length];
            double value;

            for (int i = 0; i < Rows.Length; i++)
                for (int j = 0; j < Cols.Length; j++)
                {
                    value = 0;

                    foreach (var item in list)
                        value += item[i, j];

                    ResultValues[i, j] = Math.Round(value, Round, MidpointRounding.AwayFromZero);
                }

            //считаем проценты в строках
            foreach (var row in Rows)
                foreach (var col in Cols)
                    if (row.DataSource == enDataSource.Отчет)
                            if (row.Formula[0].ResultType == enResultType.ПроцентыДелимое || row.Formula[0].ResultType == enResultType.ПроцентыДелитель)
                                ResultValues[row.Index, col.Index] = PercentRows(row, col);

            //Проверяем отклонение от контрольной суммы
            CheckSumFailed = false;
            if (CheckSumRow!=null)
            for (int j = 0; j < Cols.Length; j++)
                if (Math.Abs( ResultValues[CheckSumRow.Index, j]) > 1)
                {
                    CheckSumFailed = true;
                    break;
                }
        }
        /// <summary>
        /// Рассчитывает значения ячеек отчета за заданный период, не вычисляет строки с процентами
        /// </summary>
        /// <param name="date">Задает период (месяц и год, день не важен) за который рассчитываются значения</param>
        /// <returns>Массив рассчитанных значений</returns>
        private double[,] SetResultValues(DateTime date)
        {
            var result = new double[Rows.Length, Cols.Length];

            //подставляем известные данные
            foreach (var row in Rows)
                foreach (var col in Cols)
                    {
                        if (row.DataSource == enDataSource.РеестрыСчетов && col.DataSource == enDataSource.РеестрыСчетов)
                            result[row.Index, col.Index] = GetFact(row, col, date);
                        else if (row.DataSource == enDataSource.ПланВрача || row.DataSource == enDataSource.ПланОтделения)
                            if (col.DataSource != 0)
                                result[row.Index, col.Index] = GetPlan(row, col, date);
                    }

            //суммируем строки
            for (int lev = MaxRowLevel - 1; lev > 0; lev--)
                foreach (var row in Rows)
                    foreach (var col in Cols)
                        if (lev == row.Level && result[row.Index, col.Index] == 0 && row.DataSource == enDataSource.Отчет && col.DataSource != 0)
                                if (row.Formula[0].ResultType == enResultType.ВложенныеЭлемены || row.Formula[0].ResultType == enResultType.ЭлементыТекущейГруппы)
                                    result[row.Index, col.Index] = SubSum(row, Rows, col, enDirection.Строки, date, result);

            //суммируем столбцы
            for (int lev = MaxColLevel - 1; lev > 0; lev--)
                foreach (var col in Cols)
                    foreach (var row in Rows)                    
                        if (lev == col.Level && result[row.Index, col.Index] == 0 && col.DataSource == enDataSource.Отчет && row.DataSource != 0)
                                if (col.Formula[0].ResultType == enResultType.ВложенныеЭлемены || col.Formula[0].ResultType == enResultType.ЭлементыТекущейГруппы)
                                    result[row.Index, col.Index] = SubSum(col, Cols, row, enDirection.Столбцы, date, result);

            return result;
        }
        /// <summary>
        /// Рассчитывает значение на пересечении строки и столбца отчета по выполненым услугам
        /// </summary>
        /// <param name="rowNumber">Порядковый номер строки в массиве строк</param>
        /// <param name="colNumber">Порядковый номер столбца в массиве столбцов</param>
        /// <param name="date">Задает период (месяц и год, день не важен) за который рассчитывается значение</param>
        /// <returns>Рассчитанное значение</returns>
        private double GetPlan(ExtNode row, ExtNode col, DateTime date)
        {
            double result;

            var data = (List<dbtPlan>)DataSourcesDict[row.DataSource];

            result = data
                .Where(x => x.Period.Month == date.Month && x.Period.Year == date.Year)
                .Where(x => x.RowNodeId == row.NodeId && x.ColNodeId == col.NodeId)
                .FirstOrDefault()?.Value ?? 0;

            return result;
        }
        /// <summary>
        /// Рассчитывает значение на пересечении строки и столбца отчета по введенным планам
        /// </summary>
        /// <param name="rowNumber">Порядковый номер строки в массиве строк</param>
        /// <param name="colNumber">Порядковый номер столбца в массиве столбцов</param>
        /// <param name="date">Задает период (месяц и год, день не важен) за который рассчитывается значение</param>
        /// <returns>Рассчитанное значение</returns>
        private double GetFact(ExtNode row, ExtNode col, DateTime date)
        {
            double result = 0;
            double num;
            var data = ((List<sp_ReportFactResult>)DataSourcesDict[enDataSource.РеестрыСчетов]).Where(x => x.Period?.Month == date.Month && x.Period?.Year == date.Year).ToList();

            foreach (var fRow in row.Formula.Where(x => date.BetweenInMonths(x.DateBegin, x.DateEnd)).ToList())
                foreach (var fCol in col.Formula.Where(x => date.BetweenInMonths(x.DateBegin, x.DateEnd)).ToList())
                    foreach (var dataItem in data)
                    {
                        if (dataItem.GetValue(fRow.DataType) == fRow.DataValue || (fRow.DataType == enDataType.КодВрача && fRow.DataValue == "*"))
                            if (dataItem.GetValue(fCol.DataType) == fCol.DataValue || (fCol.DataType == enDataType.КодВрача && fCol.DataValue == "*"))
                            {
                                num = dataItem.GetValue(fCol.ResultType);
                                result += fCol.Calculate(num, fRow.Operation);
                            }
                    }

            return result;
        }
        /// <summary>
        /// Рассчитывает значение на пересечении строки и столбца отчета, путем суммирования элементов вложенных нод
        /// </summary>
        /// <param name="nodePos">Нода, во вложенных элементах которой рассчитывается значение</param>
        /// <param name="nodes">Массив нод по которым производится суммирование</param>
        /// <param name="resPos">Нода, на пересечении с которой рассчитывается значение</param>
        /// <param name="direction">Значение перечисления, определяющее направление расчета (Строки или Столбцы)</param>
        /// <param name="date">Задает период (месяц и год, день не важен) за который рассчитывается значение</param>
        /// <param name="resultValues">Массив рассчитанных значений за период</param>
        /// <returns>Рассчитанное значение</returns>
        private double SubSum(ExtNode node, ExtNode[] nodes, ExtNode resNode, enDirection direction, DateTime date, double[,] resultValues)
        {
            if (direction == 0)
                throw new Exception("Не определено направление суммирования");

            enOperation? secondMultiplier= resNode.Formula.Count == 0 ? null : resNode.Formula[0]?.Operation;
            
            double result = 0;
            double num = 0;
            var subNodes = new List<ExtNode>();
            foreach (var item in node.Prev.Next)
                subNodes.AddRange(item.Next.Where(x => x.DataSource != 0).ToList());

            var neighbourNodes = node.Prev.Next.Where(x => x.DataSource != 0).ToList();

            foreach (var formula in node.Formula.Where(x => date.BetweenInMonths(x.DateBegin, x.DateEnd)))
            {
                var nodeList = formula.ResultType == enResultType.ВложенныеЭлемены ? subNodes : neighbourNodes;
                foreach (var item in nodeList)
                    if ((formula.DataType == enDataType.НазваниеЭлемента && item.Name == formula.DataValue) || (formula.DataType == enDataType.НомерЭлемента && item.NodeId.ToString() == formula.DataValue))
                    {
                        if (direction == enDirection.Строки)
                            num = resultValues[item.Index, resNode.Index];
                        else if (direction == enDirection.Столбцы)
                            num = resultValues[resNode.Index, item.Index];

                        result += formula.Calculate(num, secondMultiplier);
                    }
            }

            return result;
        }
        /// <summary>
        /// Рассчитывает значение в процентах на пересечении строки и столбца отчета
        /// </summary>
        /// <param name="rowNumber">Порядковый номер строки в массиве строк</param>
        /// <param name="colNumber">Порядковый номер столбца в массиве столбцов</param>
        /// <returns>Рассчитанное значение</returns>
        private double PercentRows(ExtNode row, ExtNode col)
        {
            double result = 0;

            var formulaDelimoe = row.Formula.Where(x => x.ResultType == enResultType.ПроцентыДелимое).First();
            var formulaDelitel = row.Formula.Where(x => x.ResultType == enResultType.ПроцентыДелитель).First();

            var nodeDelimoe = row.Prev.Next.Where(x => x.Name == formulaDelimoe.DataValue).First();
            var nodeDelitel = row.Prev.Next.Where(x => x.Name == formulaDelitel.DataValue).First();

            var delimoe = ResultValues[nodeDelimoe.Index, col.Index];
            var delitel = ResultValues[nodeDelitel.Index, col.Index];

            if (delimoe != 0 && delitel != 0)
                result = 100 * delimoe / delitel;

            result = Math.Round(result, PercentRound, MidpointRounding.AwayFromZero);

            return result;
        }
        /// <summary>
        /// Вставляет значения ячеек отчета на лист ReoGridControl
        /// </summary>
        /// <param name="sheet">Ссылка на лист ReoGridControl</param>
        public void ValuesToReoGrid(Worksheet sheet)
        {
            //заполняем значения
            for (int i = 0; i < Rows.Length; i++)
                for (int j = 0; j < Cols.Length; j++)
                    if (ResultValues[i, j] != 0)
                    {
                        if (Rows[i].DataSource == enDataSource.Отчет && (Rows[i].Formula[0].ResultType == enResultType.ПроцентыДелимое || Rows[i].Formula[0].ResultType == enResultType.ПроцентыДелимое))
                            sheet[Rows[i].Row, Cols[j].Col] = ResultValues[i, j].ToString() + "%";
                        else
                            sheet[Rows[i].Row, Cols[j].Col] = ResultValues[i, j].ToString();
                    }
                    else
                        sheet[Rows[i].Row, Cols[j].Col] = null;

        }
        /// <summary>
        /// Вставляет заголовки отчета на лист ReoGridControl и настраивает внешний вид отчета
        /// </summary>
        /// <param name="sheet">Ссылка на лист ReoGridControl</param>
        public void HeadersToReoGrid(Worksheet sheet)
        {
            sheet.Reset();

            //задаем количество строк и столбцов
            sheet.Rows = MaxColLevel + Rows.Length;
            sheet.Columns = MaxRowLevel + Cols.Length;

            //задаем формат клеток = текст, чтобы не искажался вывод значений
            sheet.SetRangeDataFormat(0, 0, sheet.Rows, sheet.Columns, CellDataFormatFlag.Text);

            //заполняем заголовки строк
            for (int i = 0; i < Rows.Length; i++)
            {
                Rows[i].Col = Rows[i].Level - 1;
                Rows[i].Row = MaxColLevel + i;
                sheet[Rows[i].Row, Rows[i].Col] = Rows[i].GetAltName();
            }

            //заполняем заголовки столбцов
            for (int i = 0; i < Cols.Length; i++)
            {
                Cols[i].Col = MaxRowLevel + i;
                Cols[i].Row = Cols[i].Level - 1;
                sheet[Cols[i].Row, Cols[i].Col] = Cols[i].GetAltName();
            }

            //ValuesToReoGrid(sheet);

            #region форматирование

            //задаем линии клеток
            RangeBorderStyle borderStyle = new RangeBorderStyle(Color.Gray, BorderLineStyle.Dotted);
            sheet.SetRangeBorders(0, 0, sheet.Rows, sheet.Columns, BorderPositions.All, borderStyle);

            //задаем внешнюю границу
            borderStyle = new RangeBorderStyle(Color.Gray, BorderLineStyle.None);
            sheet.SetRangeBorders(0, 0, sheet.Rows, sheet.Columns, BorderPositions.Left | BorderPositions.Top, borderStyle);

            //задаем размер шрифта
            sheet.Ranges[0, 0, sheet.RowCount, sheet.ColumnCount].Style.FontSize = 8;

            //выделяем жирным заголовок
            sheet.Ranges[0, 0, sheet.Rows, MaxRowLevel].Style.Bold = true;
            sheet.Ranges[0, 0, MaxColLevel, sheet.Columns].Style.Bold = true;

            //объединяем верхний левый угол
            sheet.Ranges[0, 0, MaxColLevel, MaxRowLevel].Merge();

            //объединяем строки
            foreach (var row in Rows)
            {
                try
                {
                    if (row.DataSource == 0)
                    {
                        //объединим клетки вперед
                        sheet.Ranges[row.Row, row.Col, 1, sheet.Columns].Merge();

                        //объединим клетки вниз
                        var node = Rows.Where(x => x.Level <= row.Level && x.Row > row.Row).OrderBy(x => x.Row).FirstOrDefault();
                        int n = node == null ? sheet.Rows : node.Row;
                        n = n - row.Row;
                        sheet.Ranges[row.Row + 1, row.Col, n - 1, 1].Merge();
                    }
                    //объединим клетки вперед
                    else
                        sheet.Ranges[row.Row, row.Col, 1, MaxRowLevel - row.Col].Merge();
                }
                catch (Exception) { }
            }

            //объединяем столбцы (1 реализация)
            foreach (var col in Cols)
            {
                try
                {
                    if (col.DataSource == 0)
                    {
                        //объединим клетки вниз
                        if (col.Row + 1 < MaxColLevel)
                            sheet.Ranges[col.Row + 1, col.Col, MaxColLevel - col.Row - 1, 1].Merge();
                        //объединим клетки вперед
                        var node = Cols.Where(x => x.Level <= col.Level && x.Col > col.Col).OrderBy(x => x.Col).FirstOrDefault();
                        int n = node == null ? sheet.Columns : node.Col;
                        n = n - col.Col;

                        sheet.Ranges[col.Row, col.Col, 1, n].Merge();
                    }
                    //объединим клетки вниз
                    else if (col.Row + 1 < MaxColLevel)
                        sheet.Ranges[col.Row, col.Col, MaxColLevel - col.Row, 1].Merge();
                }
                catch (Exception) { }
            }

            //разрешаем перенос текста
            //sheet.Ranges[0, 0, sheet.Columns, sheet.ColumnCount].Style.TextWrap = TextWrapMode.WordBreak;

            //закрепляем заговки
            sheet.FreezeToCell(MaxColLevel, MaxRowLevel, FreezeArea.LeftTop);

            //выравнивание области значений
            sheet.Ranges[MaxColLevel, MaxRowLevel, sheet.Rows, sheet.ColumnCount].Style.HorizontalAlign = ReoGridHorAlign.Center;
            sheet.Ranges[MaxColLevel, MaxRowLevel, sheet.Rows, sheet.ColumnCount].Style.VerticalAlign = ReoGridVerAlign.Middle;

            //выравнивание заголовков столбцов
            sheet.Ranges[0, 0, MaxColLevel, sheet.Columns].Style.HorizontalAlign = ReoGridHorAlign.Left;
            sheet.Ranges[0, 0, MaxColLevel, sheet.Columns].Style.VerticalAlign = ReoGridVerAlign.Top;

            //выравнивание заголовков строк
            sheet.Ranges[0, 0, sheet.Rows, MaxRowLevel].Style.HorizontalAlign = ReoGridHorAlign.Left;
            sheet.Ranges[0, 0, sheet.Rows, MaxRowLevel].Style.VerticalAlign = ReoGridVerAlign.Middle;

            //запрещаем редактирование
            sheet.Ranges[0, 0, sheet.Rows, sheet.ColumnCount].IsReadonly = true;

            //меняем цвет столбцов 
            foreach (var item in Cols)
                if (item.Color != null)
                    sheet.Ranges[item.Row, item.Col, 1, 1].Style.BackColor = (Color)item.Color;

            //меняем цвет строк
            foreach (var item in Rows)
                if (item.Color != null)
                    sheet.Ranges[item.Row, item.Level - 1, 1, sheet.Columns].Style.BackColor = (Color)item.Color;

            //добавляем группировку
            //sheet.AddOutline(RowOrColumn.Column, 5, 6);

            //меняем цвет клеток для ввода плана
            if (ReportType == enReportMode.ПланВрача || ReportType == enReportMode.ПланОтделения)
                foreach (var row in Rows)
                    foreach (var col in Cols)
                        if (row.PlanSet == true && col.PlanSet == true)
                        {
                            sheet.Ranges[row.Row, col.Col, 1, 1].IsReadonly = false;
                            sheet.Ranges[row.Row, col.Col, 1, 1].Style.BackColor = Color.FromArgb(215, 255, 215);
                        }

            #endregion задаем форматы
        }
        /// <summary>
        /// Создает, изменяет, удаляет и сохраняет введенные планы с листа ReoGridControl
        /// </summary>
        /// <param name="sheet">Ссылка на лист ReoGridControl</param>
        public void SavePlan(Worksheet sheet)
        {
            double newVal, oldVal;
            List<dbtPlan> listValues = null;
            dbtPlan planItem;
            var period = BeginPeriod.FirstDayDate().AddDays(14);

            foreach (var entry in Config.Instance.Runtime.dbContext.ChangeTracker.Entries())
                if (Config.Instance.Runtime.dbContext.Entry(entry.Entity).State != EntityState.Unchanged)
                    Config.Instance.Runtime.dbContext.Entry(entry.Entity).State = EntityState.Unchanged;

            if (ReportType == enReportMode.ПланВрача)
                listValues = (List<dbtPlan>)DataSourcesDict[enDataSource.ПланВрача];
            else if (ReportType == enReportMode.ПланОтделения)
                listValues = (List<dbtPlan>)DataSourcesDict[enDataSource.ПланОтделения];

            for (int i = 0; i < Rows.Length; i++)
                for (int j = 0; j < Cols.Length; j++)
                {
                    if (Rows[i].PlanSet != true || Cols[j].PlanSet != true)
                        continue;

                    var cell = (string)sheet[Rows[i].Row, Cols[j].Col];
                    newVal = string.IsNullOrEmpty(cell) ? 0 : double.Parse(cell);
                    oldVal = ResultValues[i, j];

                    if (oldVal == newVal)
                        continue;

                    planItem = listValues.Where(x => x.RowNodeId == Rows[i].NodeId && x.ColNodeId == Cols[j].NodeId).FirstOrDefault();

                    if (newVal == 0)    //нулевые значения не хранить в базе т.к. по умолчанию и так 0
                        Config.Instance.Runtime.dbContext.dbtPlan.Remove(planItem);
                    else if (planItem == null)
                    {
                        planItem = new dbtPlan();
                        planItem.RowNodeId = Rows[i].NodeId;
                        planItem.ColNodeId = Cols[j].NodeId;
                        planItem.Type = ReportType;
                        planItem.Period = period;
                        Config.Instance.Runtime.dbContext.dbtPlan.Add(planItem);
                    }
                    planItem.Value = newVal;
                }

            Config.Instance.Runtime.dbContext.SaveChanges();
        }
        /// <summary>
        /// Сворачивает и разворачивает строки и столбцы отчета по двойному клику на ячейке
        /// </summary>
        /// <param name="control">Ссылка на ReoGridControl</param>
        /// <param name="scrollBarsPosition">Координаты полос прокруток (костыль из-за бага ReoGridControl)</param>
        public void GroupUngroup(ReoGridControl control, ref PointF scrollBarsPosition)
        {
            ExtNode node, row, col;
            node = row = col = null;
            var sheet = control.CurrentWorksheet;
            var clickPoint = new Point { X = sheet.FocusPos.Col, Y = sheet.FocusPos.Row };

            if (Rows[0].Root.Exist(clickPoint, out row))
                node = row;
            else if (Cols[0].Root.Exist(clickPoint, out col))
                node = col;

            if (node?.CanGroupUngroup == true)
            {
                node.GroupUnGroupReverse();

                if (row != null)
                    foreach (var item in Rows)
                    {
                        sheet[item.Row, item.Col] = item.GetAltName();
                        if (item.Visible == false)
                            sheet.HideRows(item.Row, 1);
                        else
                            sheet.ShowRows(item.Row, 1);
                    }
                else if (col != null)
                    foreach (var item in Cols)
                    {
                        sheet[item.Row, item.Col] = item.GetAltName();
                        if (item.Visible == false)
                            sheet.HideColumns(item.Col, 1);
                        else
                            sheet.ShowColumns(item.Col, 1);
                    }

                var pos = scrollBarsPosition;
                scrollBarsPosition = new PointF();
                control.ScrollCurrentWorksheet(pos.X, pos.Y);
            }
        }
        /// <summary>
        /// Cворачивает и разворачивает строки и столбцы отчета по заданному уровню детализации.
        /// </summary>
        /// <param name="control">Ссылка на ReoGridControl</param>
        /// <param name="scrollBarsPosition">Координаты полос прокруток (костыль из-за бага ReoGridControl)</param>
        /// <param name="rowLevel">Заданный польозвателем уровень отображения строк (-1 если без изменений)</param>
        /// <param name="colLevel">Заданный польозвателем уровень отображения столбцов (-1 если без изменений)</param>
        public void GroupUngroup(ReoGridControl control, ref PointF scrollBarsPosition, int rowLevel = -1, int colLevel = -1)
        {
            var sheet = control.CurrentWorksheet;

            if (rowLevel != -1)
            {
                Rows.Where(x => x.Level < rowLevel && x.CanGroupUngroup).ToList().ForEach(x => x.UnGroup());    //предыдущие уровни разворачиваем
                Rows.Where(x => x.Level == rowLevel && x.CanGroupUngroup).ToList().ForEach(x => x.Group());     //текущий уровень сворачиваем

                foreach (var node in Rows)
                {
                    sheet[node.Row, node.Col] = node.GetAltName();

                    if (node.Visible == false)
                        sheet.HideRows(node.Row, 1);
                    else
                        sheet.ShowRows(node.Row, 1);
                }
            }

            if (colLevel != -1)
            {
                Cols.Where(x => x.Level < colLevel && x.CanGroupUngroup).ToList().ForEach(x => x.UnGroup());    //предыдущие уровни разворачиваем
                Cols.Where(x => x.Level == colLevel && x.CanGroupUngroup).ToList().ForEach(x => x.Group());     //текущий уровень сворачиваем

                foreach (var node in Cols)
                {
                    sheet[node.Row, node.Col] = node.GetAltName();

                    if (node.Visible == false)
                        sheet.HideColumns(node.Col, 1);
                    else
                        sheet.ShowColumns(node.Col, 1);
                }
            }

            var pos = scrollBarsPosition;
            scrollBarsPosition.X = 0;
            scrollBarsPosition.Y = 0;

            control.ScrollCurrentWorksheet(pos.X, pos.Y);
        }

        //тестовый метод для подбора цвета клеток для ввода плана
        public void SetColor(ReoGridControl control)
        {
            var sheet = control.CurrentWorksheet;

            var enterColor = Color.White;
            var cd = new ColorDialog();
            if (cd.ShowDialog() == DialogResult.OK)
                enterColor = cd.Color;

            if (ReportType == enReportMode.ПланВрача || ReportType == enReportMode.ПланОтделения)
                for (int i = 0; i < Rows.Length; i++)
                    for (int j = 0; j < Cols.Length; j++)
                        if (Rows[i].ReadOnly == false && Cols[j].ReadOnly == false)
                            if (Cols[j].DataSource != 0)
                                if ((Rows[i].DataSource == enDataSource.ПланВрача && ReportType == enReportMode.ПланВрача) || (Rows[i].DataSource == enDataSource.ПланОтделения && ReportType == enReportMode.ПланОтделения))
                                {
                                    sheet.Ranges[Rows[i].Row, Cols[j].Col, 1, 1].IsReadonly = false;
                                    sheet.Ranges[Rows[i].Row, Cols[j].Col, 1, 1].Style.BackColor = enterColor;
                                }

        }

    }





}
