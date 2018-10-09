using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace ReestrUslugOMS.Classes_and_structures
{
    /// <summary>
    /// Расширенный класс ноды (элемента строки или столбца отчета по объемам медицинской помощи). 
    /// </summary>
    public class ExtNode : dbtNode
    {
        private bool? _Grouped;
        private bool _Visible;

        /// <summary>
        /// Возвращает ссылку на корневую ноду.
        /// </summary>
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
        /// <summary>
        /// Порядковый номер.
        /// </summary>
        public int Index { get; set; }
        /// <summary>
        /// Уровень вложенности, начиная с 0.
        /// </summary>
        public int Level { get; private set; }
        /// <summary>
        /// Цвет.
        /// </summary>
        public new Color? Color { get; private set; }
        /// <summary>
        /// Список формул.
        /// </summary>
        public new List<dbtFormula> Formula { get; private set; }
        /// <summary>
        /// Предыдущая нода. Для корневой ноды = Null.
        /// </summary>
        public new ExtNode Prev { get; private set; }
        /// <summary>
        /// Список волженных нод.
        /// </summary>
        public new List<ExtNode> Next { get; private set; }
        /// <summary>
        /// Номер строки (координата) в отчете.
        /// </summary>
        public int Col { get; set; }
        /// <summary>
        /// Номер столбца (координата) в отчете.
        /// </summary>
        public int Row { get; set; }
        /// <summary>
        /// Может сворачиваться и разворачиваться.
        /// </summary>
        public bool CanGroupUngroup { get; private set; }
        /// <summary>
        /// Видимость.
        /// </summary>
        public bool Visible
        {
            get
            {
                return _Visible;
            }
            set
            {
                _Visible = Hidden ? false : value;
            }
        }
        /// <summary>
        /// Разрешение вводить план пользователем.
        /// </summary>
        public bool PlanSet { get; set; }
        /// <summary>
        /// Имя ноды в соответствии с состоянием: развернута -  свернута +
        /// </summary>
        public string GetAltName()
        {
            string result;

            if (CanGroupUngroup == false)
                result = Name;
            else
            {
                if (Grouped == true)
                    result = $"+  {Name}";
                else
                    result = $"-  {Name}";
            }

            return result;
        }        
        /// <summary>
        /// Состояние свернуто (true),развернуто (false), не применимо (null)
        /// </summary>
        public bool? Grouped
        {
            get
            {
                return _Grouped;
            }
            set
            {
                if (Hidden)
                    _Grouped = true;
                else
                    _Grouped = CanGroupUngroup ? value : null;
            }
        }

        /// <summary>
        /// Конструктор по-умолчанию.
        /// </summary>
        private ExtNode()
        {
        }
        /// <summary>
        /// Конструктор для создания связанных нод по нодам из базового класса
        /// </summary>
        /// <param name="node">Нода базового класса</param>
        /// <param name="prev">Предыдущая нода</param>
        /// <param name="index">Порядковый номер по ссылке</param>
        public ExtNode(dbtNode node, ExtNode prev, ref int index)
        {
            Prev = prev;
            NodeId = node.NodeId;
            ParentId = node.ParentId;
            Name = node.Name;
            Order = node.Order;
            Color = ColorTranslator.FromHtml(node.Color);
            ReadOnly = node.ReadOnly;
            Hidden = node.Hidden || Prev?.Hidden == true ? true : false;
            DataSource = node.DataSource;
            Index = index++;
            PlanSet = false;
            Formula = node.Formula == null ? new List<dbtFormula>() : node.Formula.ToList();


            SetFullName();
            SetFullOrder();
            SetLevel();
            Visible = true;

            Next = new List<ExtNode>();
            foreach (var item in node.Next.OrderBy(x => x.Order).ToList())
                Next.Add(new ExtNode(item, this, ref index));

            SetCanGroupUnGroup();
            Grouped = false;
        }
        /// <summary>
        /// Задает полное имя. Зависит от этого же свойства предыдущей ноды.
        /// </summary>
        private void SetFullName()
        {
            FullName = Prev == null ? Name : $"{Prev.FullName}  >  {Name}";
        }
        /// <summary>
        /// Задает полный порядок сортировки. Зависит от этого же свойства предыдущей ноды.
        /// </summary>
        private void SetFullOrder()
        {
            FullOrder = Prev == null ? Order : $"{Prev.FullOrder}.{Order}";
        }
        /// <summary>
        /// Задает уровень вложенности. Зависит от этого же свойства предыдущей ноды.
        /// </summary>
        /// <returns>Уровень вложенности</returns>
        private void SetLevel()
        {
            Level = Prev == null ? 0 : Prev.Level + 1;
        }
        /// <summary>
        /// Задает возможность сворачивания/разворачивания. Зависит от своих вложенных нод на +1 уровне.
        /// </summary>
        private void SetCanGroupUnGroup()
        {
            if (Next.Count(x => x.Next.Count > 0 && x.Hidden == false) > 0)
                CanGroupUngroup = true;
            else
                CanGroupUngroup = false;
        }
        /// <summary>
        /// Проверяет среди всех вложенных нод наличие хотя бы одной ноды (результат true), значение которой может быть рассчитано.
        /// </summary>
        /// <returns>Результат</returns>
        public bool IsEmptyBranch()
        {
            var count = ToList().Count(x => x.DataSource != 0);

            return count == 0 ? true : false;
        }
        /// <summary>
        /// Устанавливает свойства полей, значения которых зависят от других нод. Начинает с корневой ноды не зависимо от текущего экземпляра, перебирает все связанные ноды. 
        /// </summary>
        public override void InitializeProperties()
        {
            int ind = -1;
            Root._InitializeProperties(ref ind);
        }
        /// <summary>
        /// Рекурсивно вперед вглубину устанавливает свойства полей, значения которых зависят от других нод.
        /// </summary>
        /// <param name="index"></param>
        private void _InitializeProperties(ref int index)
        {
            SetFullName();
            SetFullOrder();
            SetLevel();
            SetCanGroupUnGroup();
            Index = index++;

            foreach (var item in Next)
                item._InitializeProperties(ref index);
        }
        /// <summary>
        /// Создает новую корневую ноду. Новая корневая нода будет ссылать только на текущую ноду, текущая нода - на новую корневую ноду. Не пересчитывает зависимые свойства нод.
        /// </summary>
        /// <param name="name">Имя корневой ноды</param>
        /// <returns>Экземпляр корневой ноды</returns>
        public ExtNode AddRoot(string name)
        {
            var root = new ExtNode
            {
                Name = name,
                Order = "0",
                Next = new List<ExtNode>()
            };
            root.Next.Add(this);
            Prev = root;

            return root;
        }
        /// <summary>
        /// Поиск ноды с заданными координатами ячейки отчета в текущей и всех вложенных нодах. 
        /// </summary>
        /// <param name="point">Координата ячейки в отчете.</param>
        /// <param name="node">Найденная нода, если не найдена Null.</param>
        /// <returns>True если найдена,иначе False.</returns>
        public bool Exist(Point point, out ExtNode node)
        {
            node = ToList().Where(x => x.Row == point.Y && x.Col == point.X).FirstOrDefault();
            return node == null ? false : true;
        }
        /// <summary>
        /// Возвращает список  из текущей и вложенных нод, с заданным смещением от текущей ноды.
        /// </summary>
        /// <param name="pos">Смещение от текущей ноды</param>
        /// <returns>Список нод.</returns>
        public List<ExtNode> ToList(int pos = 0)
        {
            var list = new List<ExtNode>();

            _ToList(list);
            list = list.GetRange(pos, list.Count - pos);

            return list;
        }
        /// <summary>
        /// Рекурсивно вперед вглубину формирует список нод.
        /// </summary>
        /// <param name="list">Список нод.</param>
        private void _ToList(List<ExtNode> list)
        {
            list.Add(this);

            foreach (var node in Next)
                node._ToList(list);
        }
        /// <summary>
        /// Инверсия свернутого/развернутого состояния.
        /// </summary>
        public void GroupUnGroupReverse()
        {
            if (CanGroupUngroup == true)
            {
                if (Grouped == true)
                    UnGroup();
                else
                    Group();
            }
        }
        /// <summary>
        /// Сворачивает вложенные ноды.
        /// </summary>
        public void Group()
        {
            if (CanGroupUngroup == true)
            {
                Grouped = true;

                foreach (var item in Next.Where(x => x.DataSource == 0))
                    item.ToList().ForEach(x => { x.Visible = false; x.Grouped = true; });
            }

        }
        /// <summary>
        /// Разворачивает вложенные ноды.
        /// </summary>
        public void UnGroup()
        {
            if (CanGroupUngroup == true)
            {
                Grouped = false;

                foreach (var item in Next)
                {
                    item.Visible = true;
                    item.Grouped = true;

                    item.Next.Where(x => x.DataSource != 0)
                        .ToList()
                        .ForEach(x => { x.Visible = true; x.Grouped = true; });
                }
            }
        }
    }
}
