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
        public string FullName { get; protected set; }
        public string FullOrder { get; protected set; }

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

                foreach (var node in Next)
                    node.SetFullNamesAndOrders();

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

    }
}
