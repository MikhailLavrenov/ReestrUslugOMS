using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using ReestrUslugOMS;

namespace ReestrUslugOMS
{
    public partial class dbtNode
    {
        public dbtNode Root
        {
            get
            {
                var result = this;

                while (result.Prev != null)
                    result = result.Prev;

                return result;
            }
        }
        public string FullName { get; private set; }
        public string FullOrder { get; private set; }
        public int Level { get; private set; }
        public Color? cColor { get; private set; }
        public List<dbtFormula> Formulas { get; private set; }
        public List<dbtNode> NextSortedList { get; private set; }
        private bool Initialized = false;

        //свойства отображения в отчете
        public int Col { get; set; }
        public int Row { get; set; }
        public bool CanCollapse { get; private set; }
        public string AltName
        {
            get
            {
                string result;

                if (CanCollapse == false)
                    result = Name;
                else
                {
                    if (Collapsed == true)
                        result = string.Format("{0}  {1}", "+", Name);
                    else
                        result = string.Format("{0}  {1}", "-", Name);
                }

                return result;
            }
        }

        private bool _Collapsed = false;
        public bool Collapsed
        {
            get
            {
                return this._Collapsed;
            }
            set
            {
                this._Collapsed = value;
                SetVisibleNextNodes(!value, true);
            }
        }
        public bool Visible { get; set; }
        public bool PlanSet { get; set; }

        public void InitializeAllNodesProperties()
        {
            Root.InitializeProperties();            
        }

        //рекурсия вперед
        private void InitializeProperties()
        {          
            SetNextSortedList();
            SetFormulaList();
            SetCanCollaspe();
            cColor = ColorTranslator.FromHtml(Color);
            Visible = true;
            
            SetFullName();
            SetFullOrder();
            SetLevel();

            Initialized = true;

            foreach (var node in NextSortedList)
                node.InitializeProperties();            
        }

        //индексатор
        //public dbtNode this[int index]
        //{
        //    get
        //    {
        //        dbtNode node = this;
        //        while (node.Index != index)
        //        {
        //            node = node.NextNode();
        //            if (node == null)
        //                break;
        //        }
        //        return node;
        //    }
        //}

        //следующая нода

        //public dbtNode NextNode(dbtNode prevNode = null)
        //{

        //    dbtNode result = null;

        //    foreach (var item in Next)
        //    {
        //        result = item;
        //    }







        //    return result;
        //    if ((prevNode == null) && (Next.Count != 0))
        //        return Next[0];

        //    for (int i = 0; i < Next.Count - 1; i++)
        //        if (prevNode.NodeId == Next[i].NodeId)
        //            return Next[++i];

        //    if (Prev == null)
        //        return null;

        //    return Prev.NextNode(this);
        //}

        private void SetNextSortedList()
        {
            if (Next != null)
            {
                NextSortedList = Next.OrderBy(x => x.Order).ToList();
                Next = null;
            }
            else if (NextSortedList == null)
                NextSortedList = new List<dbtNode>();
        }

        private void SetFormulaList()
        {
            if (Formula != null)
            {
                Formulas = Formula.ToList();
                Formula = null;
            }
            else if (Formulas == null)
                Formulas = new List<dbtFormula>();
        }

        private void SetFullName()
        {
            if (Prev == null)
                FullName = Name;
            else
                FullName = string.Format("{0}  >  {1}", this.Prev.FullName, this.Name);

        }

        private void SetFullOrder()
        {
            if (Prev == null)
                FullOrder = Order;
            else
                FullOrder = string.Format("{0}.{1}", Prev.FullOrder, this.Order);
        }

        public void SetAllFullNamesAndOrders()
        {
            Root.SetFullNamesAndOrders();
        }

        //рекурсия вперед
        private void SetFullNamesAndOrders()
        {
            SetFullName();
            SetFullOrder();

            if (Initialized==false)
                foreach (var node in Next)
                    node.SetFullNamesAndOrders();
            else
                foreach (var node in NextSortedList)
                    node.SetFullNamesAndOrders();
        }

        private int SetLevel()
        {
            if (this.Prev == null)
                Level = 0;
            else
                Level = Prev.Level + 1;

            return Level;
        }

        private void SetCanCollaspe()
        {
            if (DataSource == 0 && NextSortedList.Count(x => x.DataSource == 0) != 0)
                CanCollapse = true;
            else
                CanCollapse = false;
        }

        public bool IsEmptyBranch()
        {
            if (Initialized == false)
                Root.InitializeAllNodesProperties();

            bool result = true;

            if (DataSource != 0)
                result = false;
            else
                foreach (var item in NextSortedList)
                    if (!item.IsEmptyBranch())
                    {
                        result = false;
                        break;
                    }

            return result;
        }

        public dbtNode PartialCopy()
        {
            var newItem = new dbtNode();

            newItem.ParentId = this.ParentId;
            newItem.Name = this.Name;
            newItem.Order = this.Order;
            newItem.Color = this.Color;
            newItem.ReadOnly = this.ReadOnly;

            return newItem;
        }

        //устанавливает видимость для всех следующих нод
        public void SetVisibleNextNodes(bool visible, bool skipWithdataSource = false)
        {
            if (Initialized == false)
                Root.InitializeAllNodesProperties();

            bool skip = false;
            bool visibleCopy;

            foreach (dbtNode node in this.NextSortedList)
            {
                skip = false;
                visibleCopy = visible;

                if (visible == false)
                {
                    if (!(skipWithdataSource == true && node.DataSource!=0))
                        node.Visible = visible;
                    else
                        node.Visible = !visible;
                }
                else
                {
                    node.Visible = visible;

                    if ((node.CanCollapse == true) && (node.Collapsed == true))
                    {
                        visibleCopy = !visibleCopy;
                        skip = true;
                    }
                }
                node.SetVisibleNextNodes(visibleCopy, skip);
            }
        }

        //проверяет наличие ноды с заданными координатами в текущей и последующих нодах
        public bool Exist(Point point, out dbtNode node)
        {
            node = Exist(point);

            if (node == null)
               return false;
            else
                return true;
        }

        //рекурсия вперед
        private dbtNode Exist(Point point)
        {
            if (Initialized == false)
                Root.InitializeAllNodesProperties();

            dbtNode result=null;
            dbtNode node;

            if ((this.Col == point.X) && (this.Row == point.Y))
                result = this;
            else
                foreach (var item in NextSortedList)
                {
                    node = item.Exist(point);

                    if (node != null)
                    {
                        result = node;
                        break;
                    }
                }

            return result;
        }

        //возвращает список от текущей ноды и все последующие
        public List<dbtNode> ToList(int pos=0)
        {
            var result = new List<dbtNode>();

            ToList(result);
            result = result.GetRange(pos, result.Count - pos);

            return result;
        }

        //рекурсия вперед
        private void ToList(List<dbtNode> list)
        {
            if (Initialized == false)
                Root.InitializeAllNodesProperties();

            list.Add(this);
           
            foreach (var node in NextSortedList)
                node.ToList(list);
        }

        //возвращает массив от текущей ноды и все последующие
        public dbtNode[] ToArray(int pos = 0)
        {
            var result = new List<dbtNode>();

            ToList(result);
            result = result.GetRange(pos, result.Count - pos);

            return result.ToArray();
        }
    }
}
