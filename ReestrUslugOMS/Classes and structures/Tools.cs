using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace ReestrUslugOMS
{
    /// <summary>
    /// Вспомогательный класс конвертеров
    /// </summary>
    public static class Tools
    {
        /// <summary>
        /// Преобразует любой enum в объект для DataSource графических компонентов
        /// </summary>
        /// <typeparam name="T">Тип перечесления enum</typeparam>
        /// <returns>Объект для DataSource</returns>
        public static object EnumToDataSource<T>()
        {
            var list = Enum.GetValues(typeof(T))
                .Cast<Enum>()
                .Select(x => new { Value = x, Description = GetEnumDescription(x) })
                .ToList();

            return list;
        }

        /// <summary>
        /// Возвращает описание (Description) перечисления класса Enum
        /// </summary>
        /// <param name="item">Перечисление класса Enum</param>
        /// <returns>Строка описания (Description)</returns>
        public static string GetEnumDescription(Enum item)
        {
            Type type = item.GetType();
            FieldInfo fInfo = type.GetField(item.ToString());
            Attribute attr = Attribute.GetCustomAttribute(fInfo, typeof(DescriptionAttribute), false);
            DescriptionAttribute dAttr = attr as DescriptionAttribute;

            return dAttr.Description;
        }

        /// <summary>
        /// Возвращает дату с первым числом месяца указанной даты
        /// </summary>
        /// <param name="value">Дата</param>
        /// <returns>Новая дата</returns>
        public static DateTime FirstDayDate(this DateTime value)
        {
            return value.AddDays(1-value.Day);
        }

        /// <summary>
        /// Возвращает дату с последним числом месяца указанной даты
        /// </summary>
        /// <param name="value">Дата</param>
        /// <returns>Новая дата</returns>
        public static DateTime LastDayDate(this DateTime value)
        {
            return value.AddMonths(1).AddDays(-value.Day);
        }

        /// <summary>
        /// Преобразует заданную в количество полных месяцов
        /// </summary>
        /// <param name="value">Дата</param>
        /// <returns>Количество месяцев</returns>
        public static int Months(this DateTime value)
        {
            return value.Year * 12 + value.Month;
        }

        /// <summary>
        /// Определяет находиться ли дата в заданном диапазоне дат. Сравнение даты с Null возвращает true. Сравнение происходит с условиями включения: >= и <=.
        /// </summary>
        /// <param name="value">Сравниваемая дата</param>
        /// <param name="begin">Начальная дата интервала</param>
        /// <param name="end">Конечная дата интервала</param>
        /// <returns>Логический результат сравнения</returns>
        public static bool BetweenInMonths (this DateTime value, DateTime? begin, DateTime? end)
        {
            bool result;
            DateTime date1, date2;

            if (begin == null)
                date1 = DateTime.MinValue;
            else
                date1 = (DateTime)begin;

            if (end == null)
                date2 = DateTime.MaxValue;
            else
                date2 = (DateTime)end;

            int mvalue;

            mvalue = value.Months();

            if (mvalue >= date1.Months() && mvalue <= date2.Months() )
                result = true;
            else
                result = false;

            return result;
        }

    }
}
