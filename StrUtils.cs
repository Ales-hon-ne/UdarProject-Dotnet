using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Aleshonne.StM
{
    internal static class StrUtils
    {
        public const char ArrayDelimiter = ';';
        private static string ConvertToString(Type T, object value)
        {
            if (T == typeof(string))
                return (string)value;
            if (T.IsArray)
            {
                return string.Join(new string(ArrayDelimiter, 1), ((Array)(object)value).Cast<object>().Select(x => ConvertToString(T.GetElementType(), x)));
            }
            if (T.IsPrimitive)
            {
                return value.ToString();
            }
            var conv = TypeDescriptor.GetConverter(T);
            return conv.ConvertToInvariantString(value);
        }
        public static string ConvertToString<T>(T value)
        {
            return ConvertToString(typeof(T), (object)value);
        }
        private static object ConvertFromString(Type T, string value)
        {
            if (T == null)
                throw new ArgumentNullException();
            if (T == typeof(object) || T == typeof(string))
                return (object)value;
            bool nlt = Nullable.GetUnderlyingType(T) != null;
            if (!nlt && (T.IsClass || T.IsInterface))
                nlt = true;
            object res;
            if (value == null)
                res = null;
            else
            {
                try
                {
                    if (T == typeof(System.Guid))
                    {
                        return Guid.Parse(value);
                    }
                    if (T == typeof(System.Text.StringBuilder))
                    {
                        return new StringBuilder(value);
                    }
                    if (T.IsArray)
                    {
                        var at = T.GetElementType();
                        var tmp = value.Split(ArrayDelimiter).Select(v => ConvertFromString(at, v)).ToArray();
                        var tr = Array.CreateInstance(at, tmp.Length);
                        for (int i = 0; i < tmp.Length; ++i)
                            tr.SetValue(tmp[i], i);
                        return tr;
                    }
                    var conv = TypeDescriptor.GetConverter(T);
                    res = conv.ConvertFromInvariantString(value);
                }
                catch (Exception)
                {
                    res = null;
                }
            }
            if (nlt && res == null)
                return null;
            if (!nlt && res == null)
                throw new InvalidCastException("string -> " + T.Name);
            return res;
        }
        public static T ConvertFromString<T>(string value)
        {
            var res = ConvertFromString(typeof(T), value);
            if (res == null)
                return default(T);
            return (T)res;
        }
    }
}
