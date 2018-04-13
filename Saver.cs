using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace UdarProject
{
    class Saver: IDisposable
    {
        public interface ISaverInstance
        {
            /// <summary>
            /// Записывает строку в файл.
            /// </summary>
            /// <param name="s">Строка для записи</param>
            void Write(string s);
            /// <summary>
            /// Записывает строку, дополненную признаком конца строки, в файл.
            /// </summary>
            /// <param name="s">Строка для записи</param>
            void WriteLine(string s);
            /// <summary>
            /// Записывает признак конца строки в файл.
            /// </summary>
            void WriteLine();
            /// <summary>
            /// Очищает буферы записи.
            /// </summary>
            void Flush();
        }
        private class SaverInstance: ISaverInstance
        {
            private StreamWriter writer;
            public SaverInstance(string fileName)
            {
                writer = new StreamWriter(fileName, false, Encoding.ASCII);
            }
            public void Write(string s)
            {
                writer.Write(s);
            }
            public void WriteLine(string s)
            {
                writer.WriteLine(s);
            }
            public void WriteLine()
            {
                writer.WriteLine();
            }
            public void Close()
            {
                writer.Close();
            }
            public void Flush()
            {
                writer.Flush();
            }
            public void WriteImage(System.Drawing.Image img)
            {
                img.Save(writer.BaseStream, System.Drawing.Imaging.ImageFormat.Png);
            }
        }

        private Dictionary<string, SaverInstance> savers;

        public Saver(string dir)
        {
            dir = Path.GetDirectoryName(dir + "/1.txt");
            if (!System.IO.Directory.Exists(dir))
                System.IO.Directory.CreateDirectory(dir);
            this.Directory = dir;
            savers = new Dictionary<string, SaverInstance>();
        }
        /// <summary>
        /// Текущая директория.
        /// </summary>
        public string Directory { get; private set; }

        public void WriteImageToFile(string fileName, System.Drawing.Image img)
        {
            fileName = fileName.ToLowerInvariant().Trim();
            if (savers.ContainsKey(fileName))
                throw new Exception(MessageReader.GetMessage("Err:S:CantWriteSI"));
            var inst = new SaverInstance(Path.Combine(Directory, fileName));
            inst.WriteImage(img);
            inst.Close();
        }

        public ISaverInstance GetInstance(string fileName)
        {
            fileName = fileName.ToLowerInvariant().Trim();
            if(!savers.ContainsKey(fileName))
                savers[fileName] = new SaverInstance(Path.Combine(Directory, fileName));
            return savers[fileName];
        }

        public void CloseInstance(string fileName)
        {
            fileName = fileName.ToLowerInvariant().Trim();
            if (savers.ContainsKey(fileName))
            {
                var sv = savers[fileName];
                savers.Remove(fileName);
                sv.Flush();
                sv.Close();
            }
        }

        void IDisposable.Dispose()
        {
            foreach (var c in savers)
                c.Value.Close();
        }
    }
}
