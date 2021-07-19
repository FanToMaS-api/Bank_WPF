using System.Collections.ObjectModel;
using System;

namespace ExtensionLibrary
{
    public static class Class1
    {
        /// <summary>
        /// Возвращает элемент коллекции
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        static public T GetElem<T>(this ObservableCollection<T> list, T obj)
        {
            return list[list.IndexOf(obj)];
        }
        static public int ToInt(this string str)
        {
            return Convert.ToInt32(str);
        }
        /// <summary>
        /// Возвращает double тип
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        static public double ToDouble(this string str)
        {
            return Convert.ToDouble(str);
        }
        /// <summary>
        /// Возвращает uint тип
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        static public UInt32 ToUInt(this string str)
        {
            return Convert.ToUInt32(str);
        }
    }
}
