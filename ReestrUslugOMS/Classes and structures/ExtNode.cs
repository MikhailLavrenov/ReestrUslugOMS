using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using ReestrUslugOMS;

namespace ReestrUslugOMS.Classes_and_structures
{
    public class ExtNode:dbtNode
    {
        public new ExtNode Root
        {
            get
            {
                var result = this;

                while (result.Prev != null)
                    result = result.Prev;

                return result;
            }
        }
        public int Index { get; set; }
        public int Level { get; private set; }
        public new Color? Color { get; private set; }
        public new List<dbtFormula> Formula { get; private set; }
        public new ExtNode Prev { get; private set; }
        public new List<ExtNode> Next { get; private set; }

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

        private ExtNode()
        {
        }

        public ExtNode(dbtNode node, ExtNode prev, int prevIndex)
        {
            NodeId = node.NodeId;
            ParentId = node.ParentId;
            Name = node.Name;
            Order = node.Order;
            Color= ColorTranslator.FromHtml(node.Color);
            ReadOnly = node.ReadOnly;
            DataSource = node.DataSource;
            Index = prevIndex+1;
            Visible = true;
            PlanSet = false;
            Formula = node.Formula == null ? new List<dbtFormula>() : node.Formula.ToList();
            Prev = prev;

            SetFullName();
            SetFullOrder();
            SetLevel();

            Next = new List<ExtNode>();
            foreach (var item in node.Next.OrderBy(x=>x.Order).ToList())
                Next.Add(new ExtNode(item,this,Index));

            SetCanCollapse();
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

        private int SetLevel()
        {
            if (Prev == null)
                Level = 0;
            else
                Level = Prev.Level + 1;

            return Level;
        }

        private void SetCanCollapse()
        {
            if (DataSource == 0 && Next.Count(x => x.DataSource == 0) != 0)
                CanCollapse = true;
            else
                CanCollapse = false;
        }

        public bool IsEmptyBranch()
        {
            bool result = true;

            if (DataSource != 0)
                result = false;
            else
                foreach (var item in Next)
                    if (item.IsEmptyBranch()==false)
                    {
                        result = false;
                        break;
                    }

            return result;
        }

        public void InitializeProperties()
        {
            Root._InitializeProperties();
        }
        //рекурсия вперед
        private void _InitializeProperties()
        {
            SetFullName();
            SetFullOrder();
            SetLevel();
            SetCanCollapse();

            foreach (var item in Next)
                item._InitializeProperties();            
        }
        
        public ExtNode AddRoot (string name)
        {
            var root = new ExtNode();

            root.Name = name;
            root.Order = "0";
            Prev = root;
            root.Next = new List<ExtNode>();
            root.Next.Add(this);
            
            return root;
        }

        //устанавливает видимость для всех следующих нод
        public void SetVisibleNextNodes(bool visible, bool skipWithdataSource = false)
        {
            bool skip = false;
            bool visibleCopy;

            foreach (ExtNode node in this.Next)
            {
                skip = false;
                visibleCopy = visible;

                if (visible == false)
                {
                    if (!(skipWithdataSource == true && node.DataSource != 0))
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
        public bool Exist(Point point, out ExtNode node)
        {
            node = _Exist(point);

            if (node == null)
                return false;
            else
                return true;
        }

        //рекурсия вперед
        private ExtNode _Exist(Point point)
        {
            ExtNode result = null;
            ExtNode node;

            if ((this.Col == point.X) && (this.Row == point.Y))
                result = this;
            else
                foreach (var item in Next)
                {
                    node = item._Exist(point);

                    if (node != null)
                    {
                        result = node;
                        break;
                    }
                }

            return result;
        }

        //возвращает список от текущей ноды и все последующие
        public List<ExtNode> ToList(int pos = 0)
        {
            var result = new List<ExtNode>();

            _ToList(result);
            result = result.GetRange(pos, result.Count - pos);

            return result;
        }

        //рекурсия вперед
        private void _ToList(List<ExtNode> list)
        {
            list.Add(this);

            foreach (var node in Next)
                node._ToList(list);
        }

        //возвращает массив от текущей ноды и все последующие
        public ExtNode[] ToArray(int pos = 0)
        {
            var result = new List<ExtNode>();

            _ToList(result);
            result = result.GetRange(pos, result.Count - pos);

            return result.ToArray();
        }
    }
}
