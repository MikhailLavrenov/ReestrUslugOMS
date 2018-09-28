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

        public static bool Between (this DateTime value, DateTime? begin, DateTime? end, bool inclusive)
        {
            bool result;
            DateTime d1, d2;

            if (begin == null)
                d1 = DateTime.MinValue;
            else
                d1 = (DateTime)begin;

            if (end == null)
                d2 = DateTime.MaxValue;
            else
                d2 = (DateTime)end;

            if (inclusive==false && value > d1 && value < d2)
                result = true;
            if (inclusive == true && value >= d1 && value <= d2)
                result = true;
            else
                result = false;

            return result;
        }

    }
}
