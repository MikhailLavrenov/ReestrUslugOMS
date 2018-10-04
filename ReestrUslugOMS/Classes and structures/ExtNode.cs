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
        /// Номер строки (координата) в отчете
        /// </summary>
        public int Col { get; set; }
        /// <summary>
        /// Номер столбца (координата) в отчете
        /// </summary>
        public int Row { get; set; }
        /// <summary>
        /// Может сворачиваться и разворачиваться
        /// </summary>
        public bool CanCollapse { get; private set; }
        /// <summary>
        /// Имя ноды в соответствии с состоянием: развернута -  свернута +
        /// </summary>
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
        /// <summary>
        /// Управляет состоянием сворачивания/разворачивания
        /// </summary>
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
        /// <summary>
        /// Видимость.
        /// </summary>
        public bool Visible { get; set; }
        /// <summary>
        /// Разрешение вводить план пользователем.
        /// </summary>
        public bool PlanSet { get; set; }

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
            NodeId = node.NodeId;
            ParentId = node.ParentId;
            Name = node.Name;
            Order = node.Order;
            Color = ColorTranslator.FromHtml(node.Color);
            ReadOnly = node.ReadOnly;
            DataSource = node.DataSource;
            Index = index++;
            Visible = true;
            PlanSet = false;
            Formula = node.Formula == null ? new List<dbtFormula>() : node.Formula.ToList();
            Prev = prev;

            SetFullName();
            SetFullOrder();
            SetLevel();

            Next = new List<ExtNode>();
            foreach (var item in node.Next.OrderBy(x => x.Order).ToList())
                Next.Add(new ExtNode(item, this, ref index));

            SetCanCollapse();
        }

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
        /// Задает уровень вложенности. Зависит от этого же свойства предыдущей ноды.
        /// </summary>
        /// <returns>Уровень вложенности</returns>
        private void SetLevel()
        {
            if (Prev == null)
                Level = 0;
            else
                Level = Prev.Level + 1;
        }

        /// <summary>
        /// Задает возможность сворачивания/разворачивания. Зависит от своих вложенных нод на +1 уровне.
        /// </summary>
        private void SetCanCollapse()
        {
            if (DataSource == 0 && Next.Count(x => x.DataSource == 0) != 0)
                CanCollapse = true;
            else
                CanCollapse = false;
        }

        /// <summary>
        /// Проверяет среди всех вложенных нод наличие хотя бы одной ноды (результат true), значение которой может быть рассчитано.
        /// </summary>
        /// <returns>Результат</returns>
        public bool IsEmptyBranch()
        {
            bool result = true;

            if (DataSource != 0)
                result = false;
            else
                foreach (var item in Next)
                    if (item.IsEmptyBranch() == false)
                    {
                        result = false;
                        break;
                    }

            return result;
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
            SetCanCollapse();
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
            var root = new ExtNode();

            root.Name = name;
            root.Order = "0";
            Prev = root;
            root.Next = new List<ExtNode>();
            root.Next.Add(this);

            return root;
        }

        /// <summary>
        /// Устанавливает видимость для всех волженных нод.
        /// </summary>
        /// <param name="visible">Видимость.</param>
        /// <param name="skipWithdataSource">Не скрывать ноды, значение которой может быть рассчитано.</param>
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

        /// <summary>
        /// Поиск ноды с заданными координатами ячейки отчета в текущей и всех вложенных нодах. 
        /// </summary>
        /// <param name="point">Координата ячейки в отчете.</param>
        /// <param name="node">Найденная нода, если не найдена Null.</param>
        /// <returns>True если найдена,иначе False.</returns>
        public bool Exist(Point point, out ExtNode node)
        {
            node = _Exist(point);

            if (node == null)
                return false;
            else
                return true;
        }

        /// <summary>
        /// Рекурсивный поиск ноды с заданными координатами ячейки отчета в текущей и всех вложенных нодах.
        /// </summary>
        /// <param name="point">Координата ячейки в отчете.</param>
        /// <returns>Найденная нода, если не найдена Null.</returns>
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

        /// <summary>
        /// Возвращает список  из текущей и вложенных нод, с заданным смещением от текущей ноды.
        /// </summary>
        /// <param name="pos">Смещение от текущей ноды</param>
        /// <returns>Список нод.</returns>
        public List<ExtNode> ToList(int pos = 0)
        {
            var result = new List<ExtNode>();

            _ToList(result);
            result = result.GetRange(pos, result.Count - pos);

            return result;
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
        /// Возвращает массив из текущей и вложенных нод, с заданным смещением от текущей ноды.
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public ExtNode[] ToArray(int pos = 0)
        {
            var result = new List<ExtNode>();

            _ToList(result);
            result = result.GetRange(pos, result.Count - pos);

            return result.ToArray();
        }
    }
}
