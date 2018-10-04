namespace ReestrUslugOMS
{
    public partial class dbtNode
    {
        /// <summary>
        /// Возвращает ссылку на корневую ноду.
        /// </summary>
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
        /// <summary>
        /// Полное имя. Формируется путем сцепления имен от корневой ноды.
        /// </summary>
        public string FullName { get; protected set; }

        /// <summary>
        /// Полный порядок сортировки. Формируется путем сцепления порядков сортировки от корневой ноды.
        /// </summary>
        public string FullOrder { get; protected set; }

        /// <summary>
        /// Задает полное имя. Зависит от этого же свойства предыдущей ноды.
        /// </summary>
        private void SetFullName()
        {
            if (Prev == null)
                FullName = Name;
            else
                FullName = string.Format("{0}  >  {1}", this.Prev.FullName, this.Name);
        }

        /// <summary>
        /// Задает полный порядок сортировки. Зависит от этого же свойства предыдущей ноды.
        /// </summary>
        private void SetFullOrder()
        {
            if (Prev == null)
                FullOrder = Order;
            else
                FullOrder = string.Format("{0}.{1}", Prev.FullOrder, this.Order);
        }

        /// <summary>
        /// Устанавливает свойства полей, значения которых зависят от других нод. Начинает с корневой ноды не зависимо от текущего экземпляра, перебирает все связанные ноды. 
        /// </summary>
        public virtual void InitializeProperties()
        {
            Root._InitializeProperties();
        }

        /// <summary>
        /// Рекурсивно вперед вглубину устанавливает свойства полей, значения которых зависят от других нод.
        /// </summary>
        private void _InitializeProperties()
        {
            SetFullName();
            SetFullOrder();

                foreach (var node in Next)
                    node._InitializeProperties();

        }

        /// <summary>
        /// Копирует текущий экземпляр класса без создания ссылок на связанные классы. Не копирует зависимые свойства.
        /// </summary>
        /// <returns>Новый экземпляр класса</returns>
        public dbtNode PartialCopy()
        {
            var newItem = new dbtNode();

            newItem.NodeId = NodeId;
            newItem.ParentId = ParentId;
            newItem.Name = Name;
            newItem.Order = Order;
            newItem.Color = Color;
            newItem.ReadOnly = ReadOnly;
            newItem.DataSource = DataSource;

            return newItem;
        }

    }
}
