using System;
using System.IO;
using System.Reflection;
using Aleshonne.StM;

namespace UdarProject
{
    /// <summary>
    /// Предоставляет функции для чтения данных из файла сообщений.
    /// </summary>
    /// <remarks>
    /// Класс имеет статический конструктор, который пытается загрузить 
    /// файл сообщений из папки с исполняемым файлом. Имя файла сообщений
    /// может быть: Messages.stmsg, Messages.aa.stmsg или Messages.aa-AA.stmsg,
    /// где aa и aa-AA представляют код текущей локали. Если файл не был найден,
    /// используется его копия из ресурсов (только русский язык!).
    /// </remarks>
    internal static class MessageReader
    {
        private const string fileName = "Messages";
        private const string fileExt = ".stmsg";
        private static StMDocument data;
        private static void Load()
        {
            var fld = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var cid = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            var ccid = System.Globalization.CultureInfo.CurrentUICulture.Name;
            var fn = Path.Combine(fld, fileName + "." + ccid + fileExt);
            if (!File.Exists(fn))
            {
                fn = Path.Combine(fld, fileName + "." + cid + fileExt);
                if (!File.Exists(fn))
                {
                    fn = Path.Combine(fld, fileName + fileExt);
                    if (!File.Exists(fn))
                    {
                        data = new StMDocument();
                        throw new FileNotFoundException("Не удалось загрузить файл сообщений.", fn);
                    }
                }
            }
            data = new StMDocument(fn);
        }
        static MessageReader()
        {
            try
            {
                Load();
            }
            catch (Exception e)
            {
                ErrorLogger.Log(e);
            }
        }
        /// <summary>
        /// Получает сообщение по ключу.
        /// </summary>
        /// <param name="key">Ключ для поиска сообщения</param>
        /// <returns>
        /// Возвращает сообщение, если оно было найдено, и пустую строку в противном случае.
        /// </returns>
        public static string GetMessage(string key)
        {
            if(data == null)
                return string.Empty;
            return data.GetValue(key) ?? string.Empty;
        }
        /// <summary>
        /// Получает сообщение по ключу и заменяет элементы формата на соответствующие значения.
        /// </summary>
        /// <param name="key">Ключ для поиска сообщения</param>
        /// <param name="values">Значения объектов для подстановки</param>
        /// <returns>
        /// Возвращает отформатированное сообщение, если оно было найдено, и массив 
        /// значений для форматирования в противном случае.
        /// </returns>
        public static string GetMessage(string key, params object[] values)
        {
            string res;
            res = GetMessage(key);
            if (!String.IsNullOrEmpty(res))
                return String.Format(res, values);
            else
                return string.Join(", ", values);
        }
    }
}
