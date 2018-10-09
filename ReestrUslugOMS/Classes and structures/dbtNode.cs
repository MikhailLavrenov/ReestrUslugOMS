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
            FullName = Prev == null ? Name : $"{Prev.FullName}  >  {Name}";
            FullOrder = Prev == null ? Order : $"{Prev.FullOrder}.{Order}";

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
            newItem.Hidden = Hidden;
            newItem.DataSource = DataSource;

            return newItem;
        }

    }
}
