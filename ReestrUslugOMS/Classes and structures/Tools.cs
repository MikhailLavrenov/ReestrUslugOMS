using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace ReestrUslugOMS
{
    public static class Tools
    {
        public static object EnumToDataSource<T>()
        {
            var list = Enum.GetValues(typeof(T))
                .Cast<Enum>()
                .Select(x => new { Value = x, Description = GetEnumDescription(x) })
                .ToList();

            return list;
        }

        public static string GetEnumDescription(Enum item)
        {
            Type type = item.GetType();
            FieldInfo fInfo = type.GetField(item.ToString());
            Attribute attr = Attribute.GetCustomAttribute(fInfo, typeof(DescriptionAttribute), false);
            DescriptionAttribute dAttr = attr as DescriptionAttribute;

            return dAttr.Description;
        }

        public static DateTime FirstDayDate(this DateTime value)
        {
            return value.AddDays(1-value.Day);
        }

        public static DateTime LastDayDate(this DateTime value)
        {
            return value.AddMonths(1).AddDays(-value.Day);
        }

        public static int Months(this DateTime value)
        {
            return value.Year * 12 + value.Month;
        }

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
