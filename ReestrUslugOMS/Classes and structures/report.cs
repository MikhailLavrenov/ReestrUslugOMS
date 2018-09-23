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
using OfficeOpenXml;
using unvell.ReoGrid;
using ReestrUslugOMS.Classes_and_structures;


namespace ReestrUslugOMS
{
    public class Report
    {
        public dbtNode[] Rows { get; private set; }
        public dbtNode[] Cols { get; private set; }
        public ResultItem[,] ResultValues { get; private set; }
        public Dictionary<enDataSource, Object> DataSourcesDict { get; set; }

        public int MaxRowLevel { get; private set; }
        public int MaxColLevel { get; private set; }

        //настройки отчета
        public int Round { get; set; }
        public enReportMode ReportType { get; private set; }
        public DateTime BeginPeriod { get; private set; }
        public DateTime EndPeriod { get; private set; }
        public enErrorMode Errors { get; private set; }
        public enInsuranceMode InsuranceTerritory { get; private set; }


        public Report(enReportMode reportType) //конструктор
        {
            MaxColLevel = 0;
            MaxRowLevel = 0;
            Round = 1;
            DataSourcesDict = new Dictionary<enDataSource, object>();
            ReportType = reportType;

            

            SetRowsCols();
            SetPlanSet();

            MaxRowLevel = this.Rows.Max(x => x.Level);
            MaxColLevel = this.Cols.Max(x => x.Level);

            this.ResultValues = new ResultItem[this.Rows.Length, this.Cols.Length];
        }

        //заполняет массив строк и стобцов
        private void SetRowsCols()
        {
            dbtNode root;

            Config.Instance.Runtime.dbContext.dbtNode.Include(x => x.Formula).Load();
            var nodes = Config.Instance.Runtime.dbContext.dbtNode.Local.ToList();
            var pr = nodes[0].Root;
            nodes[0].Prev = pr;
            //var nodes = Config.Instance.Runtime.dbContext.dbtNode.Include(x => x.Formula).AsNoTracking().ToList();

            pr = nodes[0].Root;

            //формируем и заполняем массив столбцов
            root = nodes.Where(x => x.Name == "Столбцы" && x.Prev == null).First();
            root.InitializeAllNodesProperties();
            Cols = root.ToArray(1);

            //формируем и заполняем массив строк 
            if (ReportType == enReportMode.Отчет)
                root = nodes.Where(x => x.Name == "Строки" && x.Prev == null).First();
            
            else if (ReportType == enReportMode.ПланВрача || ReportType == enReportMode.ПланОтделения)
            {
                //выбираем строки в соответствии с правами пользователя
                root = nodes.Where(x => x.NodeId == Config.Instance.Runtime.CurrentUser.NodeId).First();
                if (root.Prev != null)
                {
                    //добавляем корневую ноду для структурной целостности и навигации
                    root.Prev = new dbtNode() {  Name="Строки"   };
                    root.Prev.Next.Add(root);
                    root = root.Prev;
                }
            }
            root.Prev = null;
            root.InitializeAllNodesProperties();
            var rows = root.ToList();

            if (ReportType == enReportMode.ПланВрача || ReportType == enReportMode.ПланОтделения)
                //отбрасываем ноды, не связанные с конкретным планом
                foreach (var row in rows)
                {
                    if (ReportType == enReportMode.ПланВрача)
                        row.NextSortedList.RemoveAll(x => x.DataSource != 0 && x.DataSource != enDataSource.ПланВрача && x.Name != "План вр");
                    else if (ReportType == enReportMode.ПланОтделения)
                        row.NextSortedList.RemoveAll(x => x.DataSource != 0 && x.DataSource != enDataSource.ПланОтделения && x.Name != "План отд");
                }

            foreach (var row in rows)
                row.NextSortedList.RemoveAll(x => x.IsEmptyBranch());

            root.InitializeAllNodesProperties();
            Rows = root.ToArray(1);
        }

        //помечает ноды на пересечении которых пользователь может вводить планы
        private void SetPlanSet()
        {            
            if (ReportType == enReportMode.ПланВрача || ReportType == enReportMode.ПланОтделения)
            {
                //Cols.Union(Rows).Where(x => x.ReadOnly == false && x.DataSource != 0).ToList().ForEach(x=>x.PlanSet=true);
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

            SetDataSources();
            SetResultValues();
        }

        private void SetDataSources()
        {
            DataSourcesDict.Clear();

            if (this.ReportType == enReportMode.Отчет)
            {
                var val1 = Config.Instance.Runtime.dbContext.sp_ReportFact(BeginPeriod.Month, BeginPeriod.Year, EndPeriod.Month, EndPeriod.Year, Config.Instance.lpuCode, (int)ReportType, (int)InsuranceTerritory).ToList();
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

        public void SetResultValues()
        {
            this.ResultValues = new ResultItem[this.Rows.Length, this.Cols.Length];            

            //подставляем известные данные
            for (int i = 0; i < Rows.Length; i++)
                for (int j = 0; j < Cols.Length; j++)
                    if (Rows[i].DataSource != 0 && Cols[j].DataSource != 0)
                    {
                        if (Rows[i].DataSource == enDataSource.РеестрыСчетов && Cols[j].DataSource == enDataSource.РеестрыСчетов)
                            ResultValues[i, j].Value = GetFact(i, j);
                        else if (Rows[i].DataSource == enDataSource.ПланВрача || Rows[i].DataSource == enDataSource.ПланОтделения)
                            ResultValues[i, j] = GetPlan(i, j);
                    }

            //суммируем строки
            for (int lev = MaxRowLevel - 1; lev > 0; lev--)
                for (int i = 0; i < Rows.Length; i++)
                    for (int j = 0; j < Cols.Length; j++)
                        if (ResultValues[i, j].Value == 0 && Cols[j].DataSource != 0)
                            if ((lev == Rows[i].Level) && (Rows[i].DataSource == enDataSource.Отчет) && (Rows[i].Formulas[0].ResultType == enResultType.ВложенныеЭлемены))
                                ResultValues[i, j].Value = this.SubSum(i, j, enSumType.Строки);

            //суммируем столбцы
            for (int lev = MaxColLevel - 1; lev > 0; lev--)
                for (int i = 0; i < Cols.Length; i++)
                    for (int j = 0; j < Rows.Length; j++)
                        if (ResultValues[j, i].Value == 0 && Rows[j].DataSource != 0)
                            if ((lev == Cols[i].Level) && (Cols[i].DataSource == enDataSource.Отчет) && (Cols[i].Formulas[0].ResultType == enResultType.ВложенныеЭлемены))
                                ResultValues[j, i].Value = this.SubSum(i, j, enSumType.Столбцы);
        }

        private ResultItem GetPlan(int rowNumber, int colNumber)
        {
            var result = new ResultItem();

            if (ReportType == enReportMode.Отчет)
            {
                var data = (List<sp_ReportPlanResult>)DataSourcesDict[Rows[rowNumber].DataSource];

                foreach (var dataItem in data)
                    if (Rows[rowNumber].NodeId == dataItem.RowNodeId)
                        if (Cols[colNumber].NodeId == dataItem.ColNodeId)
                        {
                            result.Value = (double)dataItem.Value;
                            break;
                        }
            }
            else if (ReportType == enReportMode.ПланВрача || ReportType == enReportMode.ПланОтделения)
            {
                var data = (List<dbtPlan>)DataSourcesDict[Rows[rowNumber].DataSource];

                foreach (var dataItem in data)
                    if (Rows[rowNumber].NodeId == dataItem.RowNodeId)
                        if (Cols[colNumber].NodeId == dataItem.ColNodeId)
                        {
                            result.Value = dataItem.Value;
                            result.Id = dataItem.PlanId;
                            break;
                        }
            }

            result.Value = Math.Round(result.Value, Round, MidpointRounding.AwayFromZero);

            return result;
        }

        private double GetFact(int rowNumber, int colNumber)
        {
            double res = 0;
            double num;
            var data = (List<sp_ReportFactResult>)DataSourcesDict[enDataSource.РеестрыСчетов];

            foreach (var fRow in Rows[rowNumber].Formulas)
                foreach (var fCol in Cols[colNumber].Formulas)
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

        private static void SubCoord(dbtNode[] nodes, int pos, out int begin, out int end)
        {
            begin = pos;
            end = pos;
            int level = nodes[pos].Level;

            for (int i = pos; i >= 0; i--)
            {
                if (nodes[i].Level >= level)
                    begin = i;
                else break;
            }
            for (int i = pos; i < nodes.Length; i++)
            {
                if (nodes[i].Level >= level)
                    end = i;
                else break;
            }
        }

        private double SubSum(int nodePos, int resPos, enSumType sumType)
        {
            dbtNode[] nodes;
            enOperation? secondMultiplier;
            if (sumType == enSumType.Строки)
            {
                nodes = Rows;
                secondMultiplier = Cols[resPos].Formulas.Count == 0 ? null : Cols[resPos].Formulas[0]?.Operation;
            }
            else
            {
                nodes = Cols;
                secondMultiplier = Rows[resPos].Formulas.Count == 0 ? null : Rows[resPos].Formulas[0]?.Operation;
            }

            double result = 0;
            double num;
            int level = nodes[nodePos].Level;

            SubCoord(nodes, nodePos, out int beg, out int end);

            for (int j = 0; j < nodes[nodePos].Formulas.Count; j++)
                for (int i = beg; i <= end; i++)
                    if (nodes[i].Level == level + 1)
                    {
                        if (((nodes[nodePos].Formulas[j].DataType == enDataType.НазваниеЭлемента) && (nodes[i].Name == nodes[nodePos].Formulas[j].DataValue)) || ((nodes[nodePos].Formulas[j].DataType == enDataType.НазваниеЭлемента) && (nodes[i].NodeId.ToString() == nodes[nodePos].Formulas[j].DataValue)))
                        {
                            if (sumType == enSumType.Строки)
                                num = ResultValues[i, resPos].Value;
                            else if (sumType == enSumType.Столбцы)
                                num = ResultValues[resPos, i].Value;
                            else
                                num = 0;

                            result += nodes[nodePos].Formulas[j].Calculate(num, secondMultiplier);
                        }
                    }

            result = Math.Round(result, Round, MidpointRounding.AwayFromZero);

            return result;
        }

        public void ToReoGrid(Worksheet sheet)
        {
            sheet.Reset();

            //задаем количество строк и столбцов
            sheet.Rows = MaxColLevel + Rows.Length;
            sheet.Columns = MaxRowLevel + Cols.Length;

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

            //заполняем значения
            for (int i = 0; i < Rows.Length; i++)
                for (int j = 0; j < Cols.Length; j++)
                    if (ResultValues[i, j].Value != 0)
                        sheet[Rows[i].Row, Cols[j].Col] = ResultValues[i, j].Value.ToString();


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
                if (string.IsNullOrEmpty(item.Color) == false)
                    sheet.Ranges[item.Row, item.Col, 1, 1].Style.BackColor = (Color)item.cColor;

            //меняем цвет строк
            foreach (var item in Rows)
                if (string.IsNullOrEmpty(item.Color) == false)
                    sheet.Ranges[item.Row, item.Level - 1, 1, sheet.Columns].Style.BackColor = (Color)item.cColor;

            //добавляем группировку
            //sheet.AddOutline(RowOrColumn.Column, 5, 6);

            //меняем цвет клеток для ввода плана
            if (ReportType == enReportMode.ПланВрача || ReportType == enReportMode.ПланОтделения)
                foreach (var row in Rows)
                    foreach (var col in Cols)
                        if (row.PlanSet== true && col.PlanSet == true)
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
                Config.Instance.Runtime.dbContext.Entry(entry.Entity).State = System.Data.Entity.EntityState.Unchanged;

            if (ReportType == enReportMode.ПланВрача)
                listValues = (List<dbtPlan>)DataSourcesDict[enDataSource.ПланВрача];
            else if (ReportType == enReportMode.ПланОтделения)
                listValues = (List<dbtPlan>)DataSourcesDict[enDataSource.ПланОтделения];          

            for (int i = 0; i < Rows.Length; i++)
                for (int j = 0; j < Cols.Length; j++)
                {
                    if (Rows[i].PlanSet != true || Cols[j].PlanSet != true)
                        continue;

                    newVal = sheet[Rows[i].Row, Cols[j].Col] == null ? 0 : (double)sheet[Rows[i].Row, Cols[j].Col];
                    oldVal = ResultValues[i, j].Value;

                    if (oldVal == newVal)
                        continue;

                    planItem=listValues.Where(x => x.RowNodeId == Rows[i].NodeId && x.ColNodeId == Cols[j].NodeId).FirstOrDefault();

                    if (newVal == 0)    //нулевые значения не хранить в базе т.к. по умолчанию и так 0
                        Config.Instance.Runtime.dbContext.dbtPlan.Remove(planItem);
                    else if (planItem==null)
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

        public void ExpandCollapse(ReoGridControl control)
        {
            var sheet = control.CurrentWorksheet;
            var point = new Point { X = sheet.FocusPos.Col, Y = sheet.FocusPos.Row };
            int index;
            dbtNode node;

            if (Rows[0].Root.Exist(point, out node))
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

            else if (Cols[0].Root.Exist(point, out node))
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
        }

        public void ExpandCollapse(ReoGridControl control, int rowLevel = -1, int colLevel = -1)
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
        }

        public struct ResultItem
        {
            public int Id;
            public double Value;
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

        //public void ToMetroGrid(MetroGrid dgv)
        //{
        //    добавляем строки и столбцы в DataGridView
        //    dgv.Columns.Clear();
        //    for (int i = 0; i < MaxRowLevel + Cols.Length; i++)
        //        dgv.Columns.Add("", "");
        //    dgv.Rows.Add(MaxColLevel + Rows.Length);

        //    заполняем заголовки строк
        //    for (int i = 0; i < Rows.Length; i++)
        //    {
        //        Rows[i].X = Rows[i].Level - 1;
        //        Rows[i].Y = MaxColLevel + i;
        //        dgv[Rows[i].X, Rows[i].Y].Value = Rows[i].AltName;
        //    }

        //    заполняем заголовки столбцов
        //    for (int i = 0; i < Cols.Length; i++)
        //    {
        //        Cols[i].X = MaxRowLevel + i;
        //        Cols[i].Y = Cols[i].Level - 1;
        //        dgv[Cols[i].X, Cols[i].Y].Value = Cols[i].AltName;
        //    }

        //    заполняем значения
        //    for (int i = 0; i < Rows.Length; i++)
        //        for (int j = 0; j < Cols.Length; j++)
        //            if (ResultValues[i, j].Value != 0)
        //                dgv[Cols[j].X, Rows[i].Y].Value = ResultValues[i, j].Value.ToString();

        //    закрепляем заголовки столбцов и устанавливаем жирный шрифт
        //    for (int i = 0; i < MaxRowLevel; i++)
        //    {
        //        dgv.Columns[i].Frozen = true;
        //        dgv.Columns[i].DefaultCellStyle.Font = new Font(dgv.Font, FontStyle.Bold);
        //    }

        //    закрепляем заголовки строк и устанавливаем жирный шрифт
        //    for (int i = 0; i < MaxColLevel; i++)
        //    {
        //        dgv.Rows[i].Frozen = true;
        //        dgv.Rows[i].DefaultCellStyle.Font = new Font(dgv.Font, FontStyle.Bold);
        //    }

        //    запрещаем редактирование
        //    for (int i = 0; i < dgv.Rows.Count; i++)
        //        for (int j = 0; j < dgv.Columns.Count; j++)
        //            dgv[j, i].ReadOnly = true;

        //    меняем цвет столбцов
        //    for (int i = 0; i < Cols.Length; i++)
        //    {
        //        if (Cols[i].cColor == Color.White)
        //            continue;
        //        for (int j = i; j < Cols.Length; j++)
        //        {
        //            if ((Cols[j].Level > Cols[i].Level) && (Cols[j].DataSource == 0))
        //                break;

        //            for (int k = Cols[i].Level - 1; k < MaxColLevel; k++)
        //                dgv[Cols[j].X, k].Style.BackColor = (Color)Cols[i].cColor;
        //        }
        //    }

        //    меняем цвет строк
        //    for (int i = 0; i < Rows.Length; i++)
        //    {
        //        if (Rows[i].cColor == Color.White)
        //            continue;
        //        for (int j = i; j < Rows.Length; j++)
        //        {
        //            if ((Rows[j].Level < Rows[i].Level) || (Rows[j].DataSource != 0))
        //                break;

        //            for (int k = Rows[i].Level - 1; k < dgv.Columns.Count; k++)
        //                dgv[k, Rows[j].Y].Style.BackColor = (Color)Rows[i].cColor;
        //        }
        //    }

        //    меняем цвет клеток для ввода
        //    if ((ReportType == enReportMode.ПланВрача) || (ReportType == enReportMode.ПланОтделения))
        //        for (int i = 0; i < Rows.Length; i++)
        //            for (int j = 0; j < Cols.Length; j++)
        //                if (Rows[i].ReadOnly == Cols[j].ReadOnly == false)
        //                    if (Cols[j].DataSource != 0)
        //                        if (((Rows[i].DataSource == enDataSource.ПланВрача) && (ReportType == enReportMode.ПланВрача)) || ((Rows[i].DataSource == enDataSource.ПланОтделения) && (ReportType == enReportMode.ПланОтделения)))
        //                        {
        //                            dgv[Cols[j].X, Rows[i].Y].Style.BackColor = Color.GreenYellow;
        //                            dgv[Cols[j].X, Rows[i].Y].ReadOnly = false;
        //                        }
        //}

        //public void ExpandCollapse(MetroGrid dgv, Point cell)
        //{
        //    dgv.CurrentCell = null;

        //    int ind;

        //    if (Rows[0].Prev.Exist(cell, out ind))
        //    {
        //        if (Rows[ind].CanCollapse == true)
        //        {
        //            Rows[ind].Collapsed = !Rows[ind].Collapsed;
        //            dgv[Rows[ind].X, Rows[ind].Y].Value = Rows[ind].AltName;

        //            for (int i = ind; i < Rows.Length; i++)
        //                dgv.Rows[Rows[i].Y].Visible = Rows[i].Visible;
        //        }
        //    }

        //    else if (Cols[0].Prev.Exist(cell, out ind))
        //        if (Cols[ind].CanCollapse == true)
        //        {
        //            Cols[ind].Collapsed = !Cols[ind].Collapsed;
        //            dgv[Cols[ind].X, Cols[ind].Y].Value = Cols[ind].AltName;

        //            for (int i = ind; i < Cols.Length; i++)
        //                dgv.Columns[Cols[i].X].Visible = Cols[i].Visible;
        //        }
        //}

        //public void ExpandCollapse(MetroGrid dgv, int rowLevel, int colLevel)
        //{
        //    dgv.CurrentCell = null;

        //    //строки
        //    for (int i = 0; i < Rows.Length; i++)
        //        if (Rows[i].CanCollapse == true)
        //        {
        //            if (Rows[i].Level == rowLevel)
        //                Rows[i].Collapsed = true;
        //            else if (Rows[i].Level == rowLevel - 1)
        //                Rows[i].Collapsed = false;

        //            dgv[Rows[i].X, Rows[i].Y].Value = Rows[i].AltName;
        //        }

        //    for (int i = 0; i < Rows.Length; i++)
        //        dgv.Rows[Rows[i].Y].Visible = Rows[i].Visible;

        //    //столбцы
        //    for (int i = 0; i < Cols.Length; i++)
        //        if (Cols[i].CanCollapse == true)
        //        {
        //            if (Cols[i].Level == colLevel)
        //                Cols[i].Collapsed = true;
        //            else if (Cols[i].Level == colLevel - 1)
        //                Cols[i].Collapsed = false;

        //            dgv[Cols[i].X, Cols[i].Y].Value = Cols[i].AltName;
        //        }
        //    for (int i = 0; i < Cols.Length; i++)
        //        dgv.Columns[Cols[i].X].Visible = Cols[i].Visible;
        //}

        //public void SavePlan(ResultItem[,] NewResultValues)
        //{
        //    string sql;
        //    for (int i = 0; i < ResultValues.GetLength(0); i++)
        //        for (int j = 0; j < ResultValues.GetLength(1); j++)
        //            if (ResultValues[i, j].Value != NewResultValues[i, j].Value)
        //            {
        //                if (NewResultValues[i, j].Value == 0)
        //                    sql = string.Format(@"delete from [plan] where plan_id={0}", ResultValues[i, j].Id);
        //                else
        //                {
        //                    if (ResultValues[i, j].Id != 0)
        //                        sql = string.Format(@"update  [plan] set value={0}  where plan_id={1}", NewResultValues[i, j].Value, ResultValues[i, j].Id);
        //                    else
        //                        sql = string.Format(@"insert into [plan] (type,row_id,col_id,value,month,year) values ({0},{1},{2},{3},{4},{5})", (int)ReportType, Rows[i].NodeId, Cols[j].NodeId, NewResultValues[i, j].Value, BeginPeriod.Month, BeginPeriod.Year);
        //                }

        //                //db.Execute(sql);
        //            }
        //}

        //public void SaveExcel(string file)
        //{
        //    //UpdateReportCells();

        //    using (var package = new ExcelPackage())
        //    {
        //        File.Delete(file);
        //        ExcelWorksheet ws = package.Workbook.Worksheets.Add("Лист1");

        //        for (int i = 0; i < ReportCells.GetLength(0); i++)
        //            for (int j = 0; j < ReportCells.GetLength(1); j++)
        //                ws.Cells[i + 1, j + 1].Value = ReportCells[i, j];

        //        ws.Cells.AutoFitColumns();

        //        package.SaveAs(new FileInfo(file));
        //    }
        //}
    }





}
