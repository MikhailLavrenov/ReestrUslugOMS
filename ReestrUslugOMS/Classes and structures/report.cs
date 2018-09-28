using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MetroFramework.Controls;
using unvell.ReoGrid;
using ReestrUslugOMS.Classes_and_structures;
using unvell.ReoGrid.DataFormat;

namespace ReestrUslugOMS
{
    public class Report
    {
        public ExtNode[] Rows { get; private set; }
        public ExtNode[] Cols { get; private set; }
        public double[,] ResultValues { get; private set; }
        public Dictionary<enDataSource, Object> DataSourcesDict { get; set; }

        public int MaxRowLevel { get; private set; }
        public int MaxColLevel { get; private set; }

        //настройки отчета
        public int Round { get; set; }
        public int PercentRound { get; set; }
        public enReportMode ReportType { get; private set; }
        public DateTime BeginPeriod { get; private set; }
        public DateTime EndPeriod { get; private set; }
        public enErrorMode Errors { get; private set; }
        public enInsuranceMode InsuranceTerritory { get; private set; }


        public Report(enReportMode reportType) //конструктор
        {
            MaxColLevel = 0;
            MaxRowLevel = 0;
            Round = 0;
            PercentRound = 1;
            DataSourcesDict = new Dictionary<enDataSource, object>();
            ReportType = reportType;

            SetRowsCols();
            SetPlanSet();

            MaxRowLevel = this.Rows.Max(x => x.Level);
            MaxColLevel = this.Cols.Max(x => x.Level);
        }

        //заполняет массив строк и столбцов
        private void SetRowsCols()
        {
            dbtNode dbtRoot;
            ExtNode extRoot;

            Config.Instance.Runtime.dbContext.dbtNode.Include(x => x.Formula).Load();
            var nodes = Config.Instance.Runtime.dbContext.dbtNode.Local.ToList();
            //var nodes = Config.Instance.Runtime.dbContext.dbtNode.Include(x => x.Formula).AsNoTracking().ToList();            


            //формируем и заполняем массив столбцов
            dbtRoot = nodes.Where(x => x.Name == "Столбцы" && x.Prev == null).First();

            int ind = -1;
            extRoot = new ExtNode(dbtRoot, null,ref ind);
            Cols = extRoot.ToArray(1);


            //формируем и заполняем массив строк 
            dbtRoot = nodes.Where(x => x.Name == "Строки" && x.Prev == null).First();
            ind = 1;
            extRoot = new ExtNode(dbtRoot, null,ref ind);

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
            Rows = extRoot.ToArray(1);
        }

        //помечает ноды на пересечении которых пользователь может вводить планы
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

        public void SetParams(DateTime dateStart, DateTime dateEnd, enErrorMode errors = (enErrorMode)1, enInsuranceMode insuranceTerritory = (enInsuranceMode)1)
        {
            BeginPeriod = dateStart;
            EndPeriod = dateEnd;
            Errors = errors;
            InsuranceTerritory = insuranceTerritory;

            
        }

        private void SetDataSources()
        {
            DataSourcesDict.Clear();

            if (this.ReportType == enReportMode.Отчет)
            {
                var val1 = Config.Instance.Runtime.dbContext.sp_ReportFact(BeginPeriod.Month, BeginPeriod.Year, EndPeriod.Month, EndPeriod.Year, Config.Instance.lpuCode, (int)Errors, (int)InsuranceTerritory).ToList();
                DataSourcesDict.Add(enDataSource.РеестрыСчетов, val1);

                var val2 = Config.Instance.Runtime.dbContext.sp_ReportPlan((byte)enReportMode.ПланВрача, BeginPeriod.Month, BeginPeriod.Year, EndPeriod.Month, EndPeriod.Year).ToList();
                DataSourcesDict.Add(enDataSource.ПланВрача, val2);

                var val3 = Config.Instance.Runtime.dbContext.sp_ReportPlan((byte)enReportMode.ПланОтделения, BeginPeriod.Month, BeginPeriod.Year, EndPeriod.Month, EndPeriod.Year).ToList();
                DataSourcesDict.Add(enDataSource.ПланОтделения, val3);
            }

            else if (this.ReportType == enReportMode.ПланВрача)
            {
                var val = Config.Instance.Runtime.dbContext.dbtPlan.Where(x => x.Type == enReportMode.ПланВрача && x.Year == BeginPeriod.Year && x.Month == BeginPeriod.Month).ToList();
                DataSourcesDict.Add(enDataSource.ПланВрача, val);
            }

            else if (this.ReportType == enReportMode.ПланОтделения)
            {
                var val = Config.Instance.Runtime.dbContext.dbtPlan.Where(x => x.Type == enReportMode.ПланОтделения && x.Year == BeginPeriod.Year && x.Month == BeginPeriod.Month).ToList();
                DataSourcesDict.Add(enDataSource.ПланОтделения, val);
            }
        }

        private void SetResultValues()
        {
            SetDataSources();

            DateTime date = BeginPeriod.AddDays(14);

            var list = new List<double[,]>();

            while (date.Between(BeginPeriod,EndPeriod,false))
            {
                list.Add(SetResultValues(date));
                date.AddMonths(1);
            };

            ResultValues = new double[Rows.Length, Cols.Length];

            //доделать!!!!!!!!!
        }

        private double[,] SetResultValues(DateTime date)
        {
            var result = new double[Rows.Length, Cols.Length];

            //подставляем известные данные
            for (int i = 0; i < Rows.Length; i++)
                for (int j = 0; j < Cols.Length; j++)
                    if (Rows[i].DataSource != 0 && Cols[j].DataSource != 0)
                    {
                        if (Rows[i].DataSource == enDataSource.РеестрыСчетов && Cols[j].DataSource == enDataSource.РеестрыСчетов)
                            result[i, j] = GetFact(i, j, date);
                        else if (Rows[i].DataSource == enDataSource.ПланВрача || Rows[i].DataSource == enDataSource.ПланОтделения)
                            result[i, j] = GetPlan(i, j, date);
                    }

            //суммируем строки
            for (int lev = MaxRowLevel - 1; lev > 0; lev--)
                for (int i = 0; i < Rows.Length; i++)
                    for (int j = 0; j < Cols.Length; j++)
                        if (lev == Rows[i].Level && result[i, j] == 0 && Cols[j].DataSource != 0)
                            if (Rows[i].DataSource == enDataSource.Отчет && Rows[i].Formula[0].ResultType == enResultType.ВложенныеЭлемены)
                                result[i, j] = SubSum(i, j, enDirection.Строки, date);

            //суммируем столбцы
            for (int lev = MaxColLevel - 1; lev > 0; lev--)
                for (int i = 0; i < Cols.Length; i++)
                    for (int j = 0; j < Rows.Length; j++)
                        if (lev == Cols[i].Level && result[j, i] == 0 && Rows[j].DataSource != 0)
                            if (Cols[i].DataSource == enDataSource.Отчет && Cols[i].Formula[0].ResultType == enResultType.ВложенныеЭлемены)
                                result[j, i] = SubSum(i, j, enDirection.Столбцы, date);

            return result;
        }

        private double GetPlan(int rowNumber, int colNumber, DateTime date)
        {
            double result = 0;

            if (ReportType == enReportMode.Отчет)
            {
                var data = ((List<sp_ReportPlanResult>)DataSourcesDict[Rows[rowNumber].DataSource]).Where(x=>x.Period?.Month==date.Month && x.Period?.Year==date.Year).ToList();

                foreach (var dataItem in data)
                    if (Rows[rowNumber].NodeId == dataItem.RowNodeId)
                        if (Cols[colNumber].NodeId == dataItem.ColNodeId)
                        {
                            result = (double)dataItem.Value;
                            break;
                        }
            }
            else if (ReportType == enReportMode.ПланВрача || ReportType == enReportMode.ПланОтделения)
            {
                var data = ((List<dbtPlan>)DataSourcesDict[Rows[rowNumber].DataSource]).Where(x => x.Month == date.Month && x.Year == date.Year).ToList(); ;

                foreach (var dataItem in data)
                    if (Rows[rowNumber].NodeId == dataItem.RowNodeId)
                        if (Cols[colNumber].NodeId == dataItem.ColNodeId)
                        {
                            result = dataItem.Value;
                            break;
                        }
            }

            result = Math.Round(result, Round, MidpointRounding.AwayFromZero);

            return result;
        }

        private double GetFact(int rowNumber, int colNumber, DateTime date)
        {
            double res = 0;
            double num;
            var data = ((List<sp_ReportFactResult>)DataSourcesDict[enDataSource.РеестрыСчетов]).Where(x => x.Period?.Month == date.Month && x.Period?.Year == date.Year).ToList(); ;

            foreach (var fRow in Rows[rowNumber].Formula)
                foreach (var fCol in Cols[colNumber].Formula)
                    foreach (var dataItem in data)
                    {
                        string r1 = fRow.DataValue;
                        string dr = dataItem.GetValue(fRow.DataType);
                        string c1 = fCol.DataValue;
                        string dc = dataItem.GetValue(fCol.DataType);
                        if (dataItem.GetValue(fRow.DataType) == fRow.DataValue)
                            if (dataItem.GetValue(fCol.DataType) == fCol.DataValue)
                            {
                                num = dataItem.GetValue(fCol.ResultType);
                                res += fCol.Calculate(num, fRow.Operation);
                            }
                    }

            res = Math.Round(res, Round, MidpointRounding.AwayFromZero);

            return res;
        }

        private double SubSum(int nodePos, int resPos, enDirection direction, DateTime date)
        {
            ExtNode[] nodes;
            enOperation? secondMultiplier;
            if (direction == enDirection.Строки)
            {
                nodes = Rows;
                secondMultiplier = Cols[resPos].Formula.Count == 0 ? null : Cols[resPos].Formula[0]?.Operation;
            }
            else
            {
                nodes = Cols;
                secondMultiplier = Rows[resPos].Formula.Count == 0 ? null : Rows[resPos].Formula[0]?.Operation;
            }

            var node = nodes[nodePos];
            double result = 0;
            double num;
            var subNodes = new List<ExtNode>();

            foreach (var item in node.Prev.Next)
                subNodes.AddRange( item.Next.Where(x => x.DataSource != 0).ToList() );

            foreach (var formula in node.Formula.Where(x => date.Between(x.DateBegin, x.DateEnd, false)).ToList())
                    foreach (var subNode in subNodes)
                        if ((formula.DataType == enDataType.НазваниеЭлемента && subNode.Name == formula.DataValue) || (formula.DataType == enDataType.НазваниеЭлемента && subNode.NodeId.ToString() == formula.DataValue))
                        {
                            if (direction == enDirection.Строки)
                                num = ResultValues[subNode.Index, resPos];
                            else if (direction == enDirection.Столбцы)
                                num = ResultValues[resPos, subNode.Index];
                            else
                                num = 0;

                            result += formula.Calculate(num, secondMultiplier);
                        }

            result = Math.Round(result, Round, MidpointRounding.AwayFromZero);

            return result;
        }

        private double PercentRows (int rowNumber, int colNumber)
        {
            double result=0;

            ExtNode row= Rows[rowNumber];

            var formulaDelimoe = row.Formula.Where(x => x.ResultType == enResultType.ПроцентыДелимое).First();
            var formulaDelitel = row.Formula.Where(x => x.ResultType == enResultType.ПроцентыДелитель).First();

            var nodeDelimoe = row.Prev.Next.Where(x => x.Name == formulaDelimoe.DataValue).First();
            var nodeDelitel = row.Prev.Next.Where(x => x.Name == formulaDelitel.DataValue).First();

            var delimoe = ResultValues[nodeDelimoe.Index, colNumber];
            var delitel = ResultValues[nodeDelitel.Index, colNumber];

            if (delimoe!=0 && delitel!=0)
                result = 100*delimoe/delitel;

            result = Math.Round(result, PercentRound, MidpointRounding.AwayFromZero);

            return result;
        }

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
                sheet[Rows[i].Row, Rows[i].Col] = Rows[i].AltName;
            }

            //заполняем заголовки столбцов
            for (int i = 0; i < Cols.Length; i++)
            {
                Cols[i].Col = MaxRowLevel + i;
                Cols[i].Row = Cols[i].Level - 1;
                sheet[Cols[i].Row, Cols[i].Col] = Cols[i].AltName;
            }

            //ValuesToReoGrid(sheet);

            #region форматирование

            //задаем линии клеток
            RangeBorderStyle borderStyle = new RangeBorderStyle(Color.Gray, BorderLineStyle.Dotted);
            sheet.SetRangeBorders(0, 0, sheet.Rows, sheet.Columns, BorderPositions.All, borderStyle);

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

        public void SavePlan(Worksheet sheet)
        {
            double newVal, oldVal;
            List<dbtPlan> listValues = null;
            dbtPlan planItem;

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
                        planItem.Month = BeginPeriod.Month;
                        planItem.Year = BeginPeriod.Year;
                        Config.Instance.Runtime.dbContext.dbtPlan.Add(planItem);
                    }
                    planItem.Value = newVal;
                }

            Config.Instance.Runtime.dbContext.SaveChanges();
        }

        public void ExpandCollapse(ReoGridControl control, ref PointF scrollBarsPosition)
        {
            var sheet = control.CurrentWorksheet;
            var clickPoint = new Point { X = sheet.FocusPos.Col, Y = sheet.FocusPos.Row };
            int index;
            ExtNode node;

            if (Rows[0].Root.Exist(clickPoint, out node) && node.CanCollapse)
            {
                index = Array.IndexOf(Rows, node);

                if (Rows[index].CanCollapse == true)
                {
                    Rows[index].Collapsed = !Rows[index].Collapsed;
                    sheet[Rows[index].Row, Rows[index].Col] = Rows[index].AltName;

                    for (int i = index; i < Rows.Length; i++)
                    {
                        if (Rows[i].Visible == false)
                            sheet.HideRows(Rows[i].Row, 1);
                        else
                            sheet.ShowRows(Rows[i].Row, 1);
                    }
                }
            }

            else if (Cols[0].Root.Exist(clickPoint, out node) && node.CanCollapse)
            {
                index = Array.IndexOf(Cols, node);
                if (Cols[index].CanCollapse == true)
                {
                    Cols[index].Collapsed = !Cols[index].Collapsed;
                    sheet[Cols[index].Row, Cols[index].Col] = Cols[index].AltName;

                    for (int i = index + 1; i < Cols.Length; i++)
                    {
                        if (Cols[i].Visible == false)
                            sheet.HideColumns(Cols[i].Col, 1);
                        else
                            sheet.ShowColumns(Cols[i].Col, 1);
                    }
                }
            }
            else
                return;

            var pos = scrollBarsPosition;
            scrollBarsPosition.X = 0;
            scrollBarsPosition.Y = 0;
            
            control.ScrollCurrentWorksheet(pos.X, pos.Y);
        }

        public void ExpandCollapse(ReoGridControl control, ref PointF scrollBarsPosition, int rowLevel = -1, int colLevel = -1 )
        {
            var sheet = control.CurrentWorksheet;

            if (rowLevel != -1)
            {
                for (int i = 0; i < Rows.Length; i++)
                {
                    if (Rows[i].CanCollapse == true)
                    {
                        if (Rows[i].Level == rowLevel)  //текущий уровень сворачиваем
                            Rows[i].Collapsed = true;
                        else if (Rows[i].Level < rowLevel)   //предыдущие уровни разворачиваем
                            Rows[i].Collapsed = false;

                        sheet[Rows[i].Row, Rows[i].Col] = Rows[i].AltName;
                    }
                }

                for (int j = 0; j < Rows.Length; j++)
                {
                    if (Rows[j].Visible == false)
                        sheet.HideRows(Rows[j].Row, 1);
                    else
                        sheet.ShowRows(Rows[j].Row, 1);
                }
            }

            if (colLevel != -1)
            {
                for (int i = 0; i < Cols.Length; i++)
                {
                    if (Cols[i].CanCollapse == true)
                    {
                        if (Cols[i].Level == colLevel)  //текущий уровень сворачиваем
                            Cols[i].Collapsed = true;
                        else if (Cols[i].Level < colLevel)   //предыдущие уровни разворачиваем
                            Cols[i].Collapsed = false;

                        sheet[Cols[i].Row, Cols[i].Col] = Cols[i].AltName;
                    }
                }

                for (int j = 0; j < Cols.Length; j++)
                {
                    if (Cols[j].Visible == false)
                        sheet.HideColumns(Cols[j].Col, 1);
                    else
                        sheet.ShowColumns(Cols[j].Col, 1);
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



        //public void SetResultValues()
        //{
        //    SetDataSources();

        //    ResultValues = new double[Rows.Length, Cols.Length];

        //    //подставляем известные данные
        //    for (int i = 0; i < Rows.Length; i++)
        //        for (int j = 0; j < Cols.Length; j++)
        //            if (Rows[i].DataSource != 0 && Cols[j].DataSource != 0)
        //            {
        //                if (Rows[i].DataSource == enDataSource.РеестрыСчетов && Cols[j].DataSource == enDataSource.РеестрыСчетов)
        //                    ResultValues[i, j] = GetFact(i, j);
        //                else if (Rows[i].DataSource == enDataSource.ПланВрача || Rows[i].DataSource == enDataSource.ПланОтделения)
        //                    ResultValues[i, j] = GetPlan(i, j);
        //            }

        //    //суммируем строки
        //    for (int lev = MaxRowLevel - 1; lev > 0; lev--)
        //        for (int i = 0; i < Rows.Length; i++)
        //            for (int j = 0; j < Cols.Length; j++)
        //                if (lev == Rows[i].Level && ResultValues[i, j] == 0 && Cols[j].DataSource != 0)
        //                    if (Rows[i].DataSource == enDataSource.Отчет && Rows[i].Formula[0].ResultType == enResultType.ВложенныеЭлемены)
        //                        ResultValues[i, j] = SubSum(i, j, enDirection.Строки);

        //    //суммируем столбцы
        //    for (int lev = MaxColLevel - 1; lev > 0; lev--)
        //        for (int i = 0; i < Cols.Length; i++)
        //            for (int j = 0; j < Rows.Length; j++)
        //                if (lev == Cols[i].Level && ResultValues[j, i] == 0 && Rows[j].DataSource != 0)
        //                    if (Cols[i].DataSource == enDataSource.Отчет && Cols[i].Formula[0].ResultType == enResultType.ВложенныеЭлемены)
        //                        ResultValues[j, i] = SubSum(i, j, enDirection.Столбцы);

        //    //считаем проценты в строках
        //    for (int lev = MaxRowLevel; lev > 0; lev--)
        //        for (int i = 0; i < Rows.Length; i++)
        //            for (int j = 0; j < Cols.Length; j++)
        //                if (lev == Rows[i].Level && ResultValues[i, j] == 0 && Rows[i].DataSource == enDataSource.Отчет)
        //                    if (Rows[i].Formula[0].ResultType == enResultType.ПроцентыДелимое || Rows[i].Formula[0].ResultType == enResultType.ПроцентыДелитель)
        //                        ResultValues[i, j] = PercentRows(i, j);
        //}

    }





}
