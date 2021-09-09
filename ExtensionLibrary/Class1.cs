using System;
using System.Collections.ObjectModel;

namespace ExtensionLibrary
{
    public static class Class1
    {
        /// <summary>
        ///     Возвращает элемент коллекции
        /// </summary>
        public static T GetElem<T>(this ObservableCollection<T> list, T obj)
        {
            return list[list.IndexOf(obj)];
        }
        public static int ToInt(this string str)
        {
            return Convert.ToInt32(str);
        }

        /// <summary>
        ///     Возвращает double тип
        /// </summary>
        public static double ToDouble(this string str)
        {
            return Convert.ToDouble(str);
        }

        /// <summary>
        ///     Возвращает uint тип
        /// </summary>
        public static uint ToUInt(this string str)
        {
            return Convert.ToUInt32(str);
        }
    }
}
