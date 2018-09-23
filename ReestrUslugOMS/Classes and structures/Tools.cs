using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace ReestrUslugOMS.Classes_and_structures
{
    public class Tools
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

    }
}
