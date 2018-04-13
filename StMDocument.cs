using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;
using System.Text.RegularExpressions;
using System.ComponentModel;

namespace Aleshonne.StM
{
    /// <summary>
    /// Это исключение выбрасывается при возникновении ошибки парсера, не имеющей 
    /// специализированного исключения.
    /// </summary>
    public class StMException : Exception 
    {
        internal StMException() : base() { }
        internal StMException(string message) : base(message) { }
        internal StMException(string message, Exception innerException) : base(message, innerException) { }
        internal StMException(Exception innerException) : this("", innerException) { }
    }
    /// <summary>
    /// Это исключение выбрасывается, если данные закончились до завершения формирования
    /// структуры элемента.
    /// </summary>
    /// <remarks>
    /// Наиболее частой причиной этого исключения является незакрытая секция. Также возможно
    /// появление исключения, если в последней строке есть значение в кавычках с пропущеной 
    /// закрывающей кавычкой. Чтобы получать это исключение при потере конца файла, единственным
    /// элементом верхнего уровня должна быть секция, содрежащая остальные элементы.
    /// </remarks>
    public class UnexpectedEndException : StMException 
    {
        internal UnexpectedEndException() : base() { }
    }
    /// <summary>
    /// Это исключение выбрасывается, если нарушена структура документа.
    /// </summary>
    /// <remarks>
    /// Возможные причины: запись или секция без имени, подзапись без родительской записи, 
    /// закрытие неоткрытой секции, пропущенная закрывающая кавычка.
    /// </remarks>
    public class InvalidStructureException : StMException 
    {
        /// <summary>
        /// Номер строки, в которой обнаружено нарушение.
        /// </summary>
        public int LineNomber
        {
            get
            {
                return (int)Data["line"];
            }
            private set
            {
                Data["line"] = value;
            }
        }
        internal InvalidStructureException(int line) : base() 
        {
            LineNomber = line;
        }
    }
    /// <summary>
    /// Это исключение выбрасывается, если парсер встретил недопустимый символ 
    /// (или допустимый символ в недопустимой позиции).
    /// </summary>
    public class InvalidCharacterException : StMException
    {
        /// <summary>
        /// Символ, вызвавший исключение.
        /// </summary>
        public char Character
        {
            get
            {
                return (char)Data["char"];
            }
            private set
            {
                Data["char"] = value;
            }
        }
        /// <summary>
        /// Номер строки, в которой встретился символ.
        /// </summary>
        public int LineNomber
        {
            get
            {
                return (int)Data["line"];
            }
            private set
            {
                Data["line"] = value;
            }
        }
        /// <summary>
        /// Номер символа в строке.
        /// </summary>
        public int CharNomber
        {
            get
            {
                return (int)Data["cnmb"];
            }
            private set
            {
                Data["cnmb"] = value;
            }
        }
        internal InvalidCharacterException(char character, int line, int cnmb)
            : base()
        {
            Character = character;
            LineNomber = line;
            CharNomber = cnmb;
        }
    }
    /// <summary>
    /// Это исключение выбрасывается, если парсер встретил недопустимую escape-последовательность.
    /// </summary>
    public class InvalidEscapeException : StMException
    {
        /// <summary>
        /// Последоватеьлность, вызвавшая исключение.
        /// </summary>
        public string Sequence
        {
            get
            {
                return (string)Data["seq"];
            }
            private set
            {
                Data["seq"] = value;
            }
        }
        /// <summary>
        /// Номер строки, в которой встретиласть последовательность.
        /// </summary>
        public int LineNomber
        {
            get
            {
                return (int)Data["line"];
            }
            private set
            {
                Data["line"] = value;
            }
        }
        /// <summary>
        /// Позиция последовательности в строке.
        /// </summary>
        public int CharNomber
        {
            get
            {
                return (int)Data["cnmb"];
            }
            private set
            {
                Data["cnmb"] = value;
            }
        }
        internal InvalidEscapeException(string seq, int line, int cnmb)
            : base()
        {
            Sequence = seq;
            LineNomber = line;
            CharNomber = cnmb;
        }
    }
    /// <summary>
    /// Это исключение выбрасывается, если произошла попытка обратиться к несуществующему элементу.
    /// </summary>
    public class ElementNotFoundException : StMException
    {
        /// <summary>
        /// Предполагаемый путь к элементу.
        /// </summary>
        public string Path
        {
            get
            {
                return (string)Data["path"];
            }
            private set
            {
                Data["path"] = value;
            }
        }
        internal ElementNotFoundException(string path)
            : base()
        {
            Path = path;
        }
    }
    /// <summary>
    /// Это исключение выбрасывается, если тип элемента не соответствует ожидаемому.
    /// </summary>
    public class InvalidElementTypeException : StMException
    {
        /// <summary>
        /// Путь к элементу.
        /// </summary>
        public string Path
        {
            get
            {
                return (string)Data["path"];
            }
            private set
            {
                Data["path"] = value;
            }
        }
        /// <summary>
        /// Ожидаемый тип элемента.
        /// </summary>
        public StMDocument.ValueType ExpectedType
        {
            get
            {
                return (StMDocument.ValueType)Data["expectedType"];
            }
            private set
            {
                Data["expectedType"] = value;
            }
        }
        /// <summary>
        /// Реальный тип элемента.
        /// </summary>
        public StMDocument.ValueType Type
        {
            get
            {
                return (StMDocument.ValueType)Data["type"];
            }
            private set
            {
                Data["type"] = value;
            }
        }
        internal InvalidElementTypeException(string path, StMDocument.ValueType expectedType, StMDocument.ValueType type)
            : base()
        {
            Path = path;
            ExpectedType = expectedType;
            Type = type;
        }
    }
    /// <summary>
    /// Это исключение выбрасывается, если не удалось прочитать значение.
    /// </summary>
    public class CantReadValueException : StMException
    {
        internal CantReadValueException() : base() { }
    }
    /// <summary>
    /// Класс представляет редактируемый документ формата StM.
    /// </summary>
    public class StMDocument
    {
        /// <summary>
        /// Содержит определения для типов элементов.
        /// </summary>
        [Flags]
        public enum ValueType { 
            /// <summary>
            /// Элемент не существует.
            /// </summary>
            None = 0, 
            /// <summary>
            /// Элемент - секция, содержащая другие элементы.
            /// </summary>
            Section = 1, 
            /// <summary>
            /// Элемент - запись, содержащая значение и, возможно, подзаписи.
            /// </summary>
            Value = 2, 
            /// <summary>
            /// Элемент - подзапись.
            /// </summary>
            SubValue = 4 }
        /// <summary>
        /// Представляет базовый класс для узла документа.
        /// </summary>
        private abstract class BaseNode
        {
            /// <summary>
            /// Описывает общее поведение конструктора для узла документа.
            /// </summary>
            /// <param name="parent">Узел-родитель нового узла; может быть null</param>
            /// <param name="name">Имя нового узла</param>
            /// <remarks>
            /// Конструктор обладает побочными эффектами: в список потомков parent 
            /// автоматически заносится новый узел. Невозможно создать несколько узлов с
            /// одним именем и родителем (кроме родителя null).
            /// </remarks>
            public BaseNode(BaseNode parent, string name)
            {
                Parent = parent;
                Name = name;
            }
            private string _name;
            /// <summary>
            /// Имя узла.
            /// </summary>
            public string Name
            {
                get
                {
                    return _name;
                }
                set
                {
                    if (!(IsValidName(value) || (value == null && _parent == null)))
                        throw new Exception("Invalid name");
                    if (_parent != null)
                    {
                        if (_parent.Childs == null)
                            throw new Exception("Invalid parent");
                        if (_parent.Childs.Find(x => x.Name == value) != null)
                            throw new Exception("Duplicate name.");
                    }
                    _name = value;
                }
            }
            /// <summary>
            /// Значение узла.
            /// </summary>
            public abstract string Value { get; set; }
            /// <summary>
            /// Список потомков узла.
            /// </summary>
            public abstract List<BaseNode> Childs { get; }
            /// <summary>
            /// Тип узла.
            /// </summary>
            public abstract ValueType Type { get; }
            private BaseNode _parent;
            /// <summary>
            /// Родитель узла.
            /// </summary>
            /// <remarks>
            /// Смена родителя обладает побочными эффектами (кроме родителя null).
            /// </remarks>
            public BaseNode Parent { 
                get { return _parent; }
                set
                {
                    if (_parent == value)
                        return;
                    if (value != null && (value.Childs == null || Type == ValueType.Section && value.Type != ValueType.Section))
                        throw new Exception("Invalid parent.");
                    var np = value;
                    while (np != null)
                    {
                        if (np == this)
                            throw new StMException();
                        np = np.Parent;
                    }
                    if (value != null && value.Childs != null)
                    {
                        if (value.Childs.Find(x => x.Name == Name) != null)
                            throw new Exception("Duplicate key.");
                        value.Childs.Add(this);
                    }
                    if (_parent != null)
                        _parent.Childs.Remove(this);
                    _parent = value;
                }
            }
            public string Path
            {
                get
                {
                    var res = Name;
                    var p = Parent;
                    while (p.Parent != null)
                    {
                        res = (p.Name ?? "") + '/' + res;
                        p = p.Parent;
                    }
                    return res;
                }
            }
            public override string ToString()
            {
                return this.Path + ((this.Type != ValueType.Section) ? (": " + this.Value ?? "<null>") : ">");
            }
        }
        private class SectionNode : BaseNode
        {
            private List<BaseNode> childs;
            public SectionNode(BaseNode parent, string name) : base(parent, name)
            {
                childs = new List<BaseNode>();
            }
            public override string Value { get { return null; } set { ;} }
            public override List<BaseNode> Childs { get { return childs; } }
            public override ValueType Type { get { return ValueType.Section; } }
        }
        private class ValueNode : BaseNode
        {
            private List<BaseNode> childs;
            public ValueNode(BaseNode parent, string name, string value) : base(parent, name)
            {
                childs = new List<BaseNode>();
                Value = value;
            }
            public override string Value { get; set; }
            public override List<BaseNode> Childs { get { return childs; } }
            public override ValueType Type { get { return ValueType.Value; } }
        }
        private class SubValueNode : BaseNode
        {
            public SubValueNode(BaseNode parent, string name, string value, bool an = false): base(parent, name)
            {
                Value = value;
                AutoName = an;
            }
            public override string Value { get; set; }
            public override List<BaseNode> Childs { get { return null /*Enumerable.Empty<BaseNode>()*/; } }
            public override ValueType Type { get { return ValueType.SubValue; } }
            public bool AutoName { get; private set; }
        }

        private enum PState { Error, Other, ID, Value, Section, SectionOpen, SectionClose, WaitEndL, SubValue, Comment, WaitSymbol }
        private static bool NamePredicateFirst(char c) { return char.IsLetterOrDigit(c) || c == '_'; }
        private static bool NamePredicateTail(char c) { return char.IsLetterOrDigit(c) || c == '_' || c == '-'; }
        private static bool SVCPredicate(char c) { return (int)c > 31 && (char.IsLetter(c) || char.IsPunctuation(c) || char.IsNumber(c) || c == '\u0020'); }
        private static bool NELPredicate(char c) { return c == '\n' || c == '\r' || c == '\u0085' || c == '\u2028' || c == '\u2029'; }
        private static bool QSCPredicate(char c) { return (int)c >= 32 && !char.IsControl(c) && !char.IsSurrogate(c) && (!char.IsWhiteSpace(c) || c == '\u0020'); }
        private static string StrToQu(string s)
        {
            if (s.Length == 0)
                return string.Empty;
            s = s.Normalize(NormalizationForm.FormC);
            bool quoted = char.IsWhiteSpace(s[0]) || char.IsWhiteSpace(s[s.Length - 1]);
            var res = new StringBuilder(s.Length);
            for (int i = 0; i < s.Length; ++i )
            {
                quoted = quoted || !SVCPredicate(s[i]);
                if (s[i] == '"')
                {
                    res.Append("\\\""); // \"
                    continue;
                }
                // 6 вариантов конца строки - издевательство!
                if (NELPredicate(s[i]))
                {
                    if (s[i] == '\n' && i > 0 && s[i - 1] == '\r')
                        continue;
                    res.Append("\\n");
                    continue;
                }
                if (s[i] == '\t')
                {
                    res.Append("\\t");
                    continue;
                }
                if (!QSCPredicate(s[i]))
                {
                    res.Append("\\u"+((int)s[i]).ToString("X4"));
                    continue;
                }
                res.Append(s[i]);
            }
            if (quoted)
            {
                res.Insert(0, '"');
                res.Append('"');
            }
            return res.ToString();
        }
        private static string ReadID(TextReader reader)
        {
            var res = new StringBuilder();
            int nci;
            bool first = true;
            bool auto = false;
            while ((nci = reader.Peek()) != -1)
            {
                var c = (char)nci;
                if (first && char.IsWhiteSpace(c) && !NELPredicate(c))
                {
                    reader.Read();
                    ++charNomber;
                    continue;
                }
                if (first && c == '-')
                {
                    res.Append(c);
                    auto = true;
                }
                else if (!auto && ((first && NamePredicateFirst(c)) || (!first && NamePredicateTail(c))))
                    res.Append(c);
                else if (char.IsWhiteSpace(c) || c == ':' || (c == ']' && !auto))
                    break;
                else
                    throw new InvalidCharacterException(c, lineNomber, charNomber);
                reader.Read();
                ++charNomber;
                first = false;
            }
            return res.ToString();
        }
        private static string ReadSimpleValue(TextReader reader, out bool bc)
        {
            int nci;
            int ls = 0;
            var res = new StringBuilder();
            bc = false;
            while ((nci = reader.Peek()) != -1)
            {
                var c = (char)nci;
                if (c == '/')
                {
                    reader.Read();
                    ++charNomber;
                    if (reader.Peek() == (int)c)
                    {
                        bc = true;
                        break;
                    }
                    else
                    {
                        res.Append('/');
                        continue;
                    }
                }
                if (SVCPredicate(c))
                {
                    res.Append(c);
                    if (nci == 32)
                        ++ls;
                    else
                        ls = 0;
                }
                else if (char.IsWhiteSpace(c))
                    break;
                else
                    throw new InvalidCharacterException(c, lineNomber, charNomber);
                reader.Read();
                ++charNomber;
            }
            if (ls > 0)
                res.Remove(res.Length - ls, ls); // TrimEnd
            return res.ToString();
        }
        private static string ReadQuotedValue(TextReader reader)
        {
            int nci;
            var res = new StringBuilder();
            bool qs = false;
            bool esc = false;
            bool start = true;
            while ((nci = reader.Read()) != -1)
            {
                ++charNomber;
                var c = (char)nci;
                if (start && char.IsWhiteSpace(c)) //ведущие пробелы
                    continue;
                if (esc && qs) //в строке есть escape-последоватеьлность
                {
                    esc = false;
                    switch (c)
                    {
                        case '"':
                        case '\\':
                            res.Append(c);
                            break;
                        case 'u':
                        case 'U':
                            var hf = new char[4];
                            reader.Read(hf, 0, 4);
                            charNomber += 4;
                            int ic;
                            if (!int.TryParse(new string(hf), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out ic))
                                goto default;
                            res.Append((char)ic);
                            break;
                        case 'n':
                        case 'N':
                            res.Append(Environment.NewLine);
                            break;
                        case 't':
                        case 'T':
                            res.Append('\t');
                            break;
                        default:
                            throw new InvalidEscapeException("\\" + c, lineNomber, charNomber);
                    }
                    continue;
                }
                if (c == '"') //кавычка (не в escape-последовательности)
                {
                    qs = !qs;
                    if(qs && !start)
                        throw new InvalidCharacterException(c, lineNomber, charNomber);
                    start = false;
                    continue;
                }
                if (c == '\\' && qs) // начало escape-последоватеьлности
                {
                    esc = true;
                    continue;
                }
                if (qs) //текст в кавычках
                {
                    if (QSCPredicate(c))
                        res.Append(c);
                    else if (NELPredicate(c))
                        throw new InvalidStructureException(lineNomber);
                    else
                        throw new InvalidCharacterException(c, lineNomber, charNomber);
                    continue;
                }
                else
                    break;
            }
            if (qs)
                throw new UnexpectedEndException();
            return res.ToString();
        }
        private static string ReadValue(TextReader reader, out bool bc)
        {
            int nci;
            bc = false;
            while ((nci = reader.Peek()) != -1)
            {
                var c = (char)nci;
                if (NELPredicate(c))
                    return String.Empty;
                if (char.IsWhiteSpace(c))
                {
                    reader.Read();
                    ++charNomber;
                    continue;
                }
                if (c == '"')
                    return ReadQuotedValue(reader);
                else if (SVCPredicate(c))
                {
                    return ReadSimpleValue(reader, out bc);
                }
                else
                    throw new InvalidCharacterException(c, lineNomber, charNomber);
            }
            return String.Empty;
        }
        private static int lineNomber, charNomber;
        private static BaseNode Parse(Stream data)
        {
            var res = new SectionNode(null, null); // виртуальный безымянный корень
            BaseNode parent = res;
            BaseNode last = null;
            data.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(data, Encoding.UTF8, true);
            var state = PState.Other;
            lineNomber = 1;
            charNomber = 0;
            int lan = -1;
            int nci;
            string nrd = null;
            while ((nci = reader.Read()) != -1)
            {
                ++charNomber;
                var nc = (char)nci;
                if (nc == '\r' && reader.Peek() == 10) // \r\n == \n
                    continue;
                if (NELPredicate(nc))
                {
                    if (state == PState.WaitEndL || state == PState.Comment)
                        state = PState.Other;
                    if (state == PState.Value || 
                        state == PState.SubValue ||
                        state == PState.WaitSymbol)
                        throw new InvalidStructureException(lineNomber);
                    ++lineNomber;
                    charNomber = 0;
                }
                if (state == PState.Comment)
                    continue;
                if (char.IsWhiteSpace(nc) && (
                    state == PState.Other || 
                    state == PState.SectionOpen ||
                    state == PState.SectionClose ||
                    state == PState.WaitEndL ||
                    state == PState.Value ||
                    state == PState.SubValue ||
                    state == PState.WaitSymbol))
                    continue;
                if (nc == '[' && state == PState.Other)
                {
                    state = PState.Section;
                    continue;
                }
                if (nc == '>' && state == PState.Section)
                {
                    state = PState.SectionOpen;
                    nrd = ReadID(reader);
                    if (nrd == "-")
                        throw new InvalidCharacterException('-', lineNomber, charNomber - 1);
                    continue;
                }
                if (nc == '<' && state == PState.Section)
                {
                    state = PState.SectionClose;
                    continue;
                }
                if (nc == ']' && state == PState.SectionOpen)
                {
                    if (string.IsNullOrWhiteSpace(nrd))
                        throw new InvalidStructureException(lineNomber);
                    parent = new SectionNode(parent, nrd);
                    nrd = null;
                    last = null;
                    state = PState.WaitEndL;
                    continue;
                }
                if (nc == ']' && state == PState.SectionClose)
                {
                    parent = parent.Parent;
                    if (parent == null)
                        throw new InvalidStructureException(lineNomber);
                    state = PState.WaitEndL;
                    last = null;
                    continue;
                }
                if (nc == ']' && state == PState.WaitSymbol)
                {
                    state = PState.WaitEndL;
                    continue;
                }
                if (NamePredicateFirst(nc) && state == PState.SectionClose)
                {
                    nrd = nc + ReadID(reader);
                    do
                    {
                        parent = parent.Parent;
                        if (parent == null)
                            throw new InvalidStructureException(lineNomber);
                    } while (parent.Name == nrd);
                    state = PState.WaitSymbol;
                    last = null;
                    continue;
                }
                if (NamePredicateFirst(nc) && state == PState.Other)
                {
                    nrd = nc + ReadID(reader);
                    state = PState.Value;
                    continue;
                }
                if (nc == ':' && (state == PState.Value || state == PState.SubValue))
                {
                    if(string.IsNullOrWhiteSpace(nrd))
                        throw new InvalidStructureException(lineNomber);
                    bool bc;
                    if (state == PState.Value)
                    {
                        last = new ValueNode(parent, nrd, ReadValue(reader, out bc));
                        lan = 0;
                    }
                    else
                        new SubValueNode(last, nrd, ReadValue(reader, out bc), lan > 0); //побочные эффекты!!!
                    nrd = null;
                    if (bc)
                        state = PState.Comment;
                    else
                        state = PState.WaitEndL;
                    continue;
                }
                if (nc == '/' && reader.Peek() == (int)nc)
                {
                    state = PState.Comment;
                    continue;
                }
                if (nc == '/' && state == PState.Other)
                {
                    nrd = ReadID(reader);
                    if(lan > 0 && nrd != "-")
                        throw new InvalidStructureException(lineNomber);
                    if (nrd == "-")
                    {
                        if (lan >= 0)
                            nrd = (lan++).ToString();
                        else
                            throw new InvalidStructureException(lineNomber);
                    }
                    else
                    {
                        lan = -1;
                    }
                    state = PState.SubValue;
                    continue;
                }
                throw new InvalidCharacterException(nc, lineNomber, charNomber);
            }
            if (parent.Parent != null)
                throw new UnexpectedEndException();
            return res;
        }

        private readonly BaseNode Root;
        /// <summary>
        /// Создаёт новый экземпляр класса <see cref="StMDocument"/>.
        /// </summary>
        public StMDocument()
        {
            Root = new SectionNode(null, null);
        }
        /// <summary>
        /// Создаёт новый экземпляр класса <see cref="StMDocument"/> 
        /// и заполняет его данными из файла.
        /// </summary>
        /// <param name="fileName">Имя файла</param>
        public StMDocument(string fileName)
        {
            using (var s = new FileStream(fileName, FileMode.Open))
                Root = Parse(s);
        }
        /// <summary>
        /// Создаёт новый экземпляр класса <see cref="StMDocument"/> 
        /// и заполняет его данными из потока.
        /// </summary>
        /// <param name="stream">Поток с данными</param>
        public StMDocument(Stream stream)
        {
            Root = Parse(stream);
        }
        private void SaveToStream(StreamWriter wr, BaseNode node, int indent, int ni, char ic)
        {
            switch (node.Type)
            {
                case ValueType.Value:
                    wr.WriteLine(node.Name.PadLeft(ni + node.Name.Length, ic) + ": " + StrToQu(node.Value));
                    if(node.Childs != null)
                        foreach (var sv in node.Childs)
                            SaveToStream(wr, sv, indent, ni + indent, ic);
                    break;
                case ValueType.SubValue:
                    wr.WriteLine("/".PadLeft(ni + 1, ic) + ((((SubValueNode)node).AutoName) ? "-" : node.Name) + ": " + StrToQu(node.Value));
                    break;
                case ValueType.Section:
                    wr.WriteLine("[>".PadLeft(ni + 2, ic) + node.Name + "]");
                    if (node.Childs != null)
                        foreach (var sv in node.Childs)
                            SaveToStream(wr, sv, indent, ni + indent, ic);
                    wr.WriteLine("[<]".PadLeft(ni + 3, ic));
                    break;
            }
        }
        /// <summary>
        /// Сохраняет документ в поток с указанной величиной отступа.
        /// </summary>
        /// <param name="stream">Поток для сохранения</param>
        /// <param name="indent">Величина отступа блоков</param>
        public void SaveToStream(Stream stream, int indent = 2)
        {
            var wr = new StreamWriter(stream);
            foreach (var sv in Root.Childs)
                SaveToStream(wr, sv, Math.Abs(indent), 0, (indent < 0)?'\t':' ');
            wr.Flush();
        }
        /// <summary>
        /// Сохраняет документ в файл с указанной величиной отступа.
        /// </summary>
        /// <param name="fileName">Имя файла</param>
        /// <param name="indent">Величина отступа блоков</param>
        public void SaveToFile(string fileName, int indent = 2)
        {
            using (var s = new FileStream(fileName, FileMode.Create))
                SaveToStream(s, indent);
        }
        /// <summary>
        /// Возвращает элемент по указанному пути.
        /// </summary>
        /// <param name="path">Путь до элемента</param>
        /// <returns>
        /// Возвращает BaseNode x такое, что x == GetElement(x.Path), если оно существует, 
        /// и null в противном случае. Если path == null или path == "", то возвращает Root.
        /// </returns>
        private BaseNode GetNode(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return Root;
            var sspl = path.Split(':', '/', '\\').Select(x => x.Trim());
            var rl = Root.Childs;
            foreach (var s in sspl.Take(sspl.Count() - 1))
            {
                var st = s.Trim();
                var fnd = rl.Find(x => x.Name == st);
                if (fnd == null)
                    return null;
                rl = fnd.Childs;
                if (rl == null)
                    return null;
            }
            return rl.Find(x => x.Name == sspl.Last());
        }
        /// <summary>
        /// Возвращает значение элемента по указанному пути, приведённое к типу T.
        /// </summary>
        /// <typeparam name="T">Тип, к которому нужно привести значение</typeparam>
        /// <param name="path">Путь к элементу</param>
        /// <returns>
        /// Значение элемента, если оно существует. Если тип T - класс, в случае ошибки будет 
        /// возвращено null. Если тип T - System.Nullable[Tv], будет возвращено GetValue[Tv] 
        /// в случае успеха, и null в случае ошибки. Если тип T - System.Object, будет возвращена
        /// приведённая к object строка.
        /// </returns>
        /// <exception cref="CantReadValueException">
        /// Выбрасывается, когда T - тип, не допускающий null, и выполняется хотя бы одно 
        /// из условий: требуемый элемент не существует, требуемый элеменет не содержит значения, 
        /// значение элемента невозможно привести к типу T, тип T не предусматривает
        /// преобразования string->T.
        /// </exception>
        /// <remarks>
        /// Если требуется выполнить преобразование к своему типу, он должен иметь атрибут 
        /// <see cref="TypeConverterAttribute"/>. Или можно воспользоваться перегруженной
        /// функцией GetValue[T](path, converter). Если T - dynamic, то значениями будут
        /// строки (аналогично object).
        /// </remarks>
        public T GetValue<T>(string path)
        {
            return GetValue<T>(path, StrUtils.ConvertFromString<T>);
        }
        /// <summary>
        /// Возвращает значение элемента по указанному пути, приведённое к типу T 
        /// с помощью указанного конвертера.
        /// </summary>
        /// <typeparam name="T">Тип, к которому нужно привести значение</typeparam>
        /// <param name="path">Путь к элементу</param>
        /// <param name="converter">Конвертер типов</param>
        /// <returns>
        /// Возвращает результат работы конвертера на значение элемента 
        /// по указанному пути.
        /// </returns>
        /// <remarks>
        /// Конвертер должен предусматривать получение null и String.Empty.
        /// </remarks>
        public T GetValue<T>(string path, Converter<string, T> converter)
        {
            if (converter == null)
                converter = StrUtils.ConvertFromString<T>;
            return converter(GetValue(path));
        }
        /// <summary>
        /// Возвращает значение элемента по указанному пути.
        /// </summary>
        /// <param name="path">Путь к элементу</param>
        /// <returns>Значение элемента, если оно существует, и null в противном случае.</returns>
        /// <remarks>В качестве разделителя компонент пути могут быть 
        /// использованы символы ":", "/" и "\".</remarks>
        /// <example>d.GetValue("Form/Buttons:Cancel") == 
        /// d.GetValue("Form:Buttons/Cancel") == 
        /// d.GetValue("Form\\Buttons:Cancel")</example>
        public string GetValue(string path)
        {
            var n = GetNode(path);
            if (n == null)
                return null;
            else
                return n.Value;
        }
        /// <summary>
        /// Проверяет, не содержит ли строка символов, недопустимых для имени.
        /// </summary>
        /// <param name="name">Проверяемая строка</param>
        /// <returns>True, если строку можно использовать как имя элемента.</returns>
        /// <remarks>Имя элемента должно удовлетворять выражению: 
        /// [буква|цифра|"_"][буква|цифра|"_"|"-"]*. 
        /// Ведущие и концевые пробелы не допускаются. 
        /// Некоторые допустимые имена: "name", "long_name_15", "_name", "42-666".
        /// Некоторые недопустимые имена: "-1", "long name", "100%", "!name".</remarks>
        public static bool IsValidName(string name)
        {
            return name != null && 
                name.Length >= 1 && 
                name.Take(1).All(NamePredicateFirst) &&
                name.Skip(1).All(NamePredicateTail);
        }
        /// <summary>
        /// Создаёт в документе новую секцию.
        /// </summary>
        /// <param name="parentPath">Родитель секции. <see cref="String.Empty"/> или null, 
        /// если нужно разместить секцию в корне.</param>
        /// <param name="name">Имя секции</param>
        public void CreateElement(string parentPath, string name)
        {
            if (!IsValidName(name))
                throw new ArgumentException("Invalid name.");
            var parent = GetNode(parentPath);
            if (parent == null)
                throw new ArgumentException("Node does not exists.");
            if (parent.Type != ValueType.Section)
                throw new ArgumentException("Parent of section is not section.");
            BaseNode v = new SectionNode(parent, name);
        }
        /// <summary>
        /// Создаёт в документе новую запись.
        /// </summary>
        /// <param name="parentPath">Родитель записи; <see cref="String.Empty"/> или null, 
        /// если нужно разместить запись в корне</param>
        /// <param name="name">Имя записи</param>
        /// <param name="value">Содержимое записи</param>
        public void CreateElement(string parentPath, string name, string value)
        {
            if (!IsValidName(name))
                throw new ArgumentException("Invalid name.");
            var parent = GetNode(parentPath);
            if (parent == null)
                throw new ElementNotFoundException(parentPath);
            if (parent.Type == ValueType.Section)
                new ValueNode(parent, name, value);
            else if (parent.Type == ValueType.Value)
                new SubValueNode(parent, name, value);
            else
                throw new InvalidElementTypeException(parentPath, ValueType.Section | ValueType.Value, ValueType.SubValue);
        }
        /// <summary>
        /// Создаёт в документе новую запись.
        /// </summary>
        /// <param name="parentPath">Родитель записи; <see cref="String.Empty"/> или null, 
        /// если нужно разместить запись в корне</param>
        /// <param name="name">Имя записи</param>
        /// <param name="value">Содержимое записи</param>
        /// <param name="converter">Конвертер типов</param>
        public void CreateElement<T>(string parentPath, string name, T value, Converter<T, string> converter = null)
        {
            if (converter == null)
                converter = StrUtils.ConvertToString<T>;
            CreateElement(parentPath, name, converter(value));
        }
        /// <summary>
        /// Меняет родителя элемента.
        /// </summary>
        /// <param name="path">Текущий путь до элемента</param>
        /// <param name="newParent">Новый родитель элемента; должен существовать и его 
        /// тип должен совпадать с типом текущего родителя</param>
        public void MoveElement(string path, string newParent)
        {
            var value = GetNode(path);
            if(value == null || value == Root)
                throw new ArgumentException("Node does not exists.");
            var np = GetNode(newParent);
            if (np == null)
                throw new ArgumentException("New parent does not exists.");
            if(value == np)
                throw new ArgumentException("New parent equivalent to moving value.");
            var p = np;
            while((p = p.Parent) != null)
                if(value == p)
                    throw new ArgumentException("New parent has value in parent chain.");
            if (value.Parent.Type != np.Type)
                throw new ArgumentException("Invalid type of new parent.");
            value.Parent = np;
        }
        /// <summary>
        /// Удаляет элемент и всех его потомков.
        /// </summary>
        /// <param name="path">Путь до элемента</param>
        /// <exception cref="ArgumentException">Элемент по указанному пути не существует.</exception>
        public void RemoveElement(string path)
        {
            var value = GetNode(path);
            if (value == null)
                throw new ArgumentException("Node does not exists.");
            if (value.Parent == null) // пользователь пытается удалить Root
                value.Childs.Clear();
            else
                value.Parent.Childs.Remove(value);
        }
        /// <summary>
        /// Получает тип элемента.
        /// </summary>
        /// <param name="path">Путь до элемента</param>
        /// <returns>Возвращает тип элемента, если он существует, и <see cref="ValueType.None"/> 
        /// в противном случае.</returns>
        public ValueType GetElementType(string path)
        {
            var value = GetNode(path);
            if (value == null)
                return ValueType.None;
            return value.Type;
        }
        /// <summary>
        /// Изменяет имя элемента.
        /// </summary>
        /// <param name="path">Путь к элементу</param>
        /// <param name="newName">Новое имя</param>
        /// <exception cref="ArgumentException">Новое имя содержит недопустимые символы; 
        /// или элемент с таким именем уже существует; 
        /// или элемент, который нужно переименовать, не существует.</exception>
        public void RenameElement(string path, string newName)
        {
            var n = GetNode(path);
            if (n == null || n.Parent == null)
                throw new ElementNotFoundException(path);
            n.Name = newName;
        }
        /// <summary>
        /// Получает список имён всех потомков первого уровня элемента.
        /// </summary>
        /// <param name="path">Путь к элементу</param>
        /// <returns>
        /// Возвращает список имён всех потомков первого уровня элемента, если он существует, 
        /// и null в противном случае.
        /// </returns>
        public IEnumerable<string> GetChilds(string path)
        {
            var n = GetNode(path);
            if (n == null)
                return Enumerable.Empty<string>();
            return n.Childs.Select(x => x.Name).ToList();
        }
        /// <summary>
        /// Получает список значений всех потомков первого уровня элемента.
        /// </summary>
        /// <param name="path">Путь к элементу</param>
        /// <returns>
        /// Возвращает список значений всех потомков первого уровня элемента, если он существует, 
        /// и null в противном случае. Если потомок не имеет значения, то соответствующий
        /// элемент будет null.
        /// </returns>
        public IEnumerable<string> GetChildsValues(string path)
        {
            var n = GetNode(path);
            if (n == null)
                return null;
            return n.Childs.Select(x => x.Value).ToList();
        }
        /// <summary>
        /// Получает список значений всех потомков первого уровня элемента, приведённых
        /// к типу T.
        /// </summary>
        /// <typeparam name="T">Тип, к которому нужно привести значения</typeparam>
        /// <param name="path">Путь к элементу</param>
        /// <param name="converter">Конвертер типов; если null - используется конвертер по-умолчанию</param>
        /// <returns>
        /// Возвращает список значений всех потомков первого уровня элемента, если он существует, 
        /// и null в противном случае. 
        /// </returns>
        /// <remarks>
        /// Поведние конвертера по-умолчанию совпадает с таковым у <see cref="GetValue&lt;T&gt;"/>.
        /// </remarks>
        public IEnumerable<T> GetChildsValues<T>(string path, Converter<string, T> converter = null)
        {
            var n = GetNode(path);
            if (n == null)
                return null;
            if (converter == null)
                converter = StrUtils.ConvertFromString<T>;
            return n.Childs.Select(x => converter(x.Value)).ToList();
        }
        /// <summary>
        /// Меняет значение элемента на указанное.
        /// </summary>
        /// <typeparam name="T">Тип значения</typeparam>
        /// <param name="path">Путь до элемента</param>
        /// <param name="value">Значение</param>
        /// <param name="converter">Конвертер типов</param>
        /// <exception cref="ElementNotFoundException">
        /// Элемент не существует.
        /// </exception>
        /// <exception cref="InvalidElementTypeException">
        /// Элемент не содержит значения (является секцией).
        /// </exception>
        public void SetValue<T>(string path, T value, Converter<T, string> converter = null)
        {
            var n = GetNode(path);
            if (n == null)
                throw new ElementNotFoundException(path);
            if (n.Type == ValueType.Section)
                throw new InvalidElementTypeException(path, ValueType.Value | ValueType.SubValue, n.Type);
            if (converter == null)
                converter = StrUtils.ConvertToString<T>;
            n.Value = converter(value);
        }
    }
}
