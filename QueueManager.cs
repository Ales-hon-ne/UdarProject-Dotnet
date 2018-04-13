using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

namespace UdarProject
{
    static class ErrorLogger
    {
        private const int maxSize = 1048576; // 1MB
        private static string fileName;
        private static TextWriter logFile;
        private static bool canWrite;
        private static object sync;
        static ErrorLogger()
        {
            sync = new object();
            Constructor();
        }
        private static void TestFileSize()
        {
            try
            {
                lock (sync)
                {
                    if (File.Exists(fileName) && new FileInfo(fileName).Length > maxSize)
                    {
                        if (logFile != null)
                            logFile.Close();
                        var bakfn = fileName + ".old";
                        if (File.Exists(bakfn))
                            File.Delete(bakfn);
                        File.Move(fileName, bakfn);
                        logFile = new StreamWriter(fileName, true, Encoding.UTF8);
                        canWrite = true;
                    }
                }
            }
            catch (Exception)
            {
                canWrite = false;
            }
        }
        private static void Constructor()
        {
            try
            {
                fileName = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    "Ales'hon'ne",
                    "Axial2D",
                    Assembly.GetExecutingAssembly().GetName().Version.ToString(2),
                    "errors.log");
                lock (sync)
                {
                    if (!Directory.Exists(Path.GetDirectoryName(fileName)))
                        Directory.CreateDirectory(Path.GetDirectoryName(fileName));
                    logFile = new StreamWriter(fileName, true, Encoding.UTF8);
                    canWrite = true;
                }
                TestFileSize();
            }
            catch (Exception)
            {
                canWrite = false;
            }
        }
        /// <summary>
        /// Записывает информацию об исключении в лог и возвращает описание первичного исключения.
        /// </summary>
        /// <param name="e">Исключение, которое нужно записать в лог</param>
        /// <returns>
        /// Строка, содержащая сборку, тип и описание исключения, лежащего в начале цепочки.
        /// </returns>
        public static string Log(Exception e)
        {
            if (!canWrite)
            {
                Constructor();
            }
            else
            {
                TestFileSize();
            }
            try
            {
                lock (sync)
                {
                    if (canWrite)
                    {
                        logFile.Write(DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss.fff|"));
                        logFile.WriteLine(Environment.UserName);
                    }
                    var msg = String.Empty;
                    while (true) // не читайте код дальше; он работает правильно *совершаю джедайский жест*
                    {
                        var mv = "[" + e.Source + "|" + e.GetType().Name + "]" + e.Message.Replace(Environment.NewLine, " ");
                        msg = mv;
                        if (canWrite)
                            logFile.WriteLine(mv);
                        var trace = new System.Diagnostics.StackTrace(e, false);
                        if (canWrite)
                            logFile.WriteLine(String.Join(Environment.NewLine,
                                trace.GetFrames().Select(x => x.GetMethod())
                                .Select(x => "  |>[" + x.Module.ToString() + "]" +
                                    x.DeclaringType + "." + x.Name + ((x.IsGenericMethod) ? ("[" +
                                    String.Join<Type>(", ", x.GetGenericArguments()) + "]") : "") +
                                    "(" + String.Join(", ", x.GetParameters()
                                    .Select((p, i) => (i <= 3 || x.GetParameters().Length < 6) ?
                                        (p.Name + ": " + p.ParameterType.ToString()) :
                                        ((i == 4) ? "...[" + (x.GetParameters().Length - 4).ToString() +
                                        "]" : "")).Where(p => !string.IsNullOrWhiteSpace(p))) +
                                    ")" + ((x is MethodInfo && ((MethodInfo)x).ReturnType != typeof(void)) ?
                                    (": " + ((MethodInfo)x).ReturnType.ToString()) : ""))));
                        if (e.InnerException == null)
                            break;
                        e = e.InnerException;
                    }
                    if (!canWrite)
                        msg = "[Logger error!]" + msg;
                    if (e.Data.Count > 0 && canWrite)
                    {
                        logFile.WriteLine("Data:");
                        foreach (var key in e.Data.Keys)
                            logFile.WriteLine("  [{0}]: {1}", key, e.Data[key]);
                    }
                    if (canWrite)
                        logFile.Flush();
                    return msg;
                }
            }
            catch (Exception)
            {
                canWrite = false;
                return String.Empty;
            }
        }
        public static string FileName { get { return fileName; } }
    }
    /// <summary>
    /// Обратные вызовы для менеджера очереди.
    /// 
    /// preExecute -> (preCICalc -> [CIPCRef, CISUpd] -> (postCIGood | postCIError) -> postCIAny) -> postExecute
    /// </summary>
    struct QMSetting
    {
        public enum WorkStatus { Run, Complete, Error, Lag }
        /// <summary>
        /// Действия, выполняемые до начала обработки очереди. Параметр - число задач.
        /// </summary>
        public Action<int> preExecute;
        /// <summary>
        /// Действия, выполняемые после окончания обработки очереди.
        /// </summary>
        public Action postExecute;
        /// <summary>
        /// Действия, выполняемые до начала вычисления задачи. Параметры - номер, имя И время запуска задачи.
        /// </summary>
        public Action<int, string, DateTime> preCICalc;
        /// <summary>
        /// Действия, выполняемые при обновлении счётчика процентов. Параметры - значение счётчика и время обновления.
        /// </summary>
        public Action<byte, DateTime> CIPCRef;
        /// <summary>
        /// Действия, выполняемые при обновлении статуса задачи. Параметр - новый статус.
        /// </summary>
        public Action<WorkStatus> CISUpd;
        /// <summary>
        /// Действия, выполняемые при успешном завершении задачи.
        /// </summary>
        public Action postCIGood;
        /// <summary>
        /// Действия, выполняемые при завершении задакчи с ошибкой. Параметр - сообщение об ошибке.
        /// </summary>
        public Action<string> postCIError;
        /// <summary>
        /// Действия, выполняемые при завершении задачи (после <see cref="postCIGood"/> или <see cref="postCIError"/>).
        /// </summary>
        public Action postCIAny;
    }

    [DataContract]
    struct ComputeItem
    {
        [DataContract]
        public enum SymmetryType { Free = 0, Soft, Hard }
        [DataContract]
        public enum FixationType { Free = 0, Bottom, Side, Both }

        /// <summary>
        /// Исходная строка имени.
        /// </summary>
        [DataMember(Name = "Name", IsRequired = false, Order = 1, EmitDefaultValue = false)]
        public string RawName;
        /// <summary>
        /// Отображаемое имя.
        /// </summary>
        public string Name
        {
            get
            {
                var dn = RawName;
                if (!string.IsNullOrWhiteSpace(dn))
                    foreach (var fl in typeof(ComputeItem).GetFields())
                    {
                        var repn = fl.Name;
                        var attr = fl.GetCustomAttributes(typeof(DataMemberAttribute), false);
                        if (attr.Length > 0)
                            repn = ((DataMemberAttribute)attr[0]).Name ?? repn;
                        dn = dn.Replace("{" + repn + "}", (fl.GetValue((object)this) ?? "{null}").ToString());
                    }
                return dn;
            }
            set
            {
                RawName = value;
            }
        }
        /// <summary>
        /// Период времени.
        /// </summary>
        [DataMember(IsRequired = false, Order = 10)]
        public double TimePeriod;
        /// <summary>
        /// Шаг по времени.
        /// </summary>
        [DataMember(IsRequired = false, Order = 20)]
        public double TimeStep;
        /// <summary>
        /// Тип фиксации.
        /// </summary>
        public FixationType Fixation;
        /// <summary>
        /// Конвертер типа фиксации в строку для записи в файл.
        /// </summary>
        [DataMember(Name = "Fixation", IsRequired = false, Order = 30, EmitDefaultValue = false)]
        private string _strFixation
        {
            get
            {
                return Enum.GetName(typeof(FixationType), Fixation);
            }
            set
            {
                FixationType val;
                if (!Enum.TryParse(value, true, out val))
                {
                    val = FixationType.Free;
                    int vali;
                    if (int.TryParse(value, out vali))
                        val = (FixationType)vali;
                }
                Fixation = val;
            }
        }
        /// <summary>
        /// Число КЭ в верхней части по радиусу.
        /// </summary>
        [DataMember(IsRequired = false, Order = 40)]
        public int OverheadRadius;
        /// <summary>
        /// Число КЭ в нижней части по радиусу.
        /// </summary>
        [DataMember(IsRequired = false, Order = 50)]
        public int BaseRadius;
        /// <summary>
        /// Толщины слоёв (в КЭ) в верхней части.
        /// </summary>
        [DataMember(IsRequired = false, Order = 60)]
        public int[] OverheadHeight;
        /// <summary>
        /// Толщины слоёв (в КЭ) в нижней части.
        /// </summary>
        [DataMember(IsRequired = false, Order = 70)]
        public int[] BaseHeight;
        /// <summary>
        /// Начальный размер КЭ по радиусу.
        /// </summary>
        [DataMember(IsRequired = false, Order = 80)]
        public double FER;
        /// <summary>
        /// Начальная высота КЭ.
        /// </summary>
        [DataMember(IsRequired = false, Order = 90)]
        public double FEZ;
        /// <summary>
        /// Материалы слоёв верхней части (нумеруются с 1).
        /// </summary>
        [DataMember(IsRequired = false, Order = 100)]
        public int[] OverheadMaterials;
        /// <summary>
        /// Материалы слоёв нижней части (нумеруются с 1).
        /// </summary>
        [DataMember(IsRequired = false, Order = 110)]
        public int[] BaseMaterials;
        /// <summary>
        /// Начальная температура.
        /// </summary>
        [DataMember(IsRequired = false, Order = 115)]
        public double T0;
        /// <summary>
        /// Скорость нижней волны.
        /// </summary>
        [DataMember(IsRequired = false, Order = 120, EmitDefaultValue = false)]
        public double? BottomWaveSpeed;
        /// <summary>
        /// Скорость боковой волны.
        /// </summary>
        [DataMember(IsRequired = false, Order = 130, EmitDefaultValue = false)]
        public double? LateralWaveSpeed;
        /// <summary>
        /// Скорость движения верхней части.
        /// </summary>
        [DataMember(IsRequired = false, Order = 140, EmitDefaultValue = false)]
        public double? OverheadSpeed;
        /// <summary>
        /// Скорость верхнего удара.
        /// </summary>
        [DataMember(IsRequired = false, Order = 150, EmitDefaultValue = false)]
        public double? TopImpactSpeed;
        /// <summary>
        /// Скорость нижнего удара.
        /// </summary>
        [DataMember(IsRequired = false, Order = 160, EmitDefaultValue = false)]
        public double? BottomImpactSpeed;
        /// <summary>
        /// Режим принудитеьлной симметрии.
        /// </summary>
        [DataMember(IsRequired = false, Order = 170, EmitDefaultValue = false)]
        public SymmetryType Symmetry;
        /// <summary>
        /// Определяет, нужно ли сохранять картинки для анимации.
        /// </summary>
        [DataMember(Name = "SaveAnimation", IsRequired = false, Order = 3, EmitDefaultValue = false)]
        public bool? Animation;
        /// <summary>
        /// Определяет число расчётов. Если больше 1, то считается несколько раз, на выходе среднее.
        /// </summary>
        [DataMember(IsRequired = false, Order = 2, EmitDefaultValue = false)]
        public int InstCount;
        /// <summary>
        /// Если истина, то используется альтернативная топология.
        /// </summary>
        [DataMember(IsRequired = false, Order = 4, EmitDefaultValue = false)]
        public bool? AlterTopology;

        public override string ToString()
        {
            return Name;
        }
    }

    [DataContract(Name = "ComputeQueue")]
    class QueueManager
    {
        const string taskFileName = "#task.json";
        /// <summary>
        /// Добавляет запись в файл отчёта.
        /// </summary>
        /// <param name="error">Если true, добавит метку ошибки</param>
        /// <param name="code">Код сообщения</param>
        /// <param name="text">Содержимое записи</param>
        /// <param name="flush">Если true, очистит буфер записи</param>
        private void AddRepString(bool error, string code, string text, bool flush = true)
        {
            if (error)
                reportWriter.Write("!");
            else
                reportWriter.Write(" ");
            reportWriter.Write(DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss.fff|"));
            reportWriter.Write(code.PadRight(4) + "|");
            reportWriter.WriteLine(text);
            if (flush)
                reportWriter.Flush();
        }
        private int taskNomber;
        private StreamWriter reportWriter;
        private QueueManager()
        {
            materials = new List<Material>();
            queue = new List<ComputeItem>();
        }
        [DataMember(Name = "ResultDirectory", Order = 1, IsRequired = false, EmitDefaultValue = false)]
        private string resDir;
        [DataMember(Name = "MaterialFile", Order = 2, IsRequired = false, EmitDefaultValue = false)]
        private string outMat;
        [DataMember(Name = "Items", Order = 30)]
        private List<ComputeItem> queue;
        [DataMember(Name = "Materials", Order = 20)]
        private List<Material> materials;
        private bool stopFlag;
        private List<Material> outML;

        private void Calc(ComputeItem ci, QMSetting setting)
        {
            AddRepString(false, "TS", MessageReader.GetMessage("Msg:T:Start", taskNomber, ci.Name ?? String.Empty), false);
            var startTime = DateTime.Now;
            int percent = 0;
            bool percentRefreshed = true;
            var lastConnect = startTime; // хранит время последнего выхода процесса на связь
            string dname = Path.Combine(resDir, startTime.ToString("yyyyMMddHHmmssfff"));
            if (!string.IsNullOrWhiteSpace(ci.Name))
                dname += "[" + new string(ci.Name.Trim().Select(c => (System.IO.Path.GetInvalidFileNameChars().Contains(c)) ? '_' : c).ToArray()) + "]";
            var saver = new Saver(dname);
            AddRepString(false, "TDir", dname);
            int mfi = 1;
            ci.BaseMaterials = ci.BaseMaterials.ToArray(); // копия массива
            ci.OverheadMaterials = ci.OverheadMaterials.ToArray();
            var usingmat = materials.Concat(outML).Where((_, i) =>
                {
                    var bi = ci.BaseMaterials.ToList().BinarySearch(i + 1);
                    var oi = ci.OverheadMaterials.ToList().BinarySearch(i + 1);
                    if (bi >= 0)
                        ci.BaseMaterials[bi] = mfi;
                    if (oi >= 0)
                        ci.OverheadMaterials[oi] = mfi;
                    if (oi >= 0 || bi >= 0)
                    {
                        ++mfi;
                        return true;
                    }
                    return false;
                }).ToList();
            {
                var sqm = new QueueManager();
                sqm.materials = usingmat;
                sqm.queue.Add(ci);
                QueueManager.SaveToFile(Path.Combine(dname, taskFileName), sqm);
            }
            setting.preCICalc(taskNomber, ci.Name, startTime);
            var calculator = new Calculator(ci, usingmat, saver, x =>
            {
                percent = x;
                percentRefreshed = true;
                lastConnect = DateTime.Now;
            });
            bool error = false;
            string msg = "";
            try
            {
                Task calc = new Task(calculator.Calculate);
                calc.Start();
                while (!calc.IsCompleted)
                {
                    calc.Wait(2000, new CancellationToken(stopFlag));
                    if (calc.IsFaulted)
                        throw calc.Exception;
                    if (percentRefreshed)
                    {
                        percentRefreshed = false;
                        setting.CIPCRef((byte)percent, lastConnect);
                        setting.CISUpd(QMSetting.WorkStatus.Run);
                    }
                    if (DateTime.Now.Subtract(lastConnect).TotalMinutes > 10)
                    {
                        setting.CISUpd(QMSetting.WorkStatus.Lag);
                    }
                }
            }
            catch (Exception e)
            {
                error = true;
                msg = ErrorLogger.Log(e);
            }
            ((IDisposable)saver).Dispose();
            if (error)
            {
                setting.CISUpd(QMSetting.WorkStatus.Error);
                setting.postCIError(msg);
                AddRepString(true, "TErr", msg, false);
                AddRepString(false, "TEF", MessageReader.GetMessage("Msg:T:FinError", taskNomber, percent), false);
            }
            else
            {
                setting.CISUpd(QMSetting.WorkStatus.Complete);
                setting.postCIGood();
                AddRepString(false, "TF", MessageReader.GetMessage("Msg:T:Fin", taskNomber), false);
            }
            setting.postCIAny();
        }

        public void Stop()
        {
            stopFlag = true;
        }
        public void Execute(QMSetting setting)
        {
            try
            {
                stopFlag = false;
                setting.preExecute(queue.Count);
                if (!Directory.Exists(resDir))
                    Directory.CreateDirectory(resDir);
                taskNomber = 0;
                using (reportWriter = new StreamWriter(Path.Combine(resDir, "#report" + DateTime.Now.ToString("yyyyMMddHHmmssfff")) + ".txt", false, Encoding.UTF8))
                {
                    if (outMat != null && File.Exists(outMat))
                        using (var matReader = new FileStream(outMat, FileMode.Open))
                        {
                            outML = (List<Material>)(new DataContractJsonSerializer(typeof(List<Material>))).ReadObject(matReader);
                            AddRepString(false, "QInf", MessageReader.GetMessage("Msg:Q:InfOMF", outML.Count));
                        }
                    AddRepString(false, "QS", MessageReader.GetMessage("Msg:Q:Start"), false);
                    AddRepString(false, "QInf", MessageReader.GetMessage("Msg:Q:InfTC", queue.Count));
                    try
                    {
                        foreach (var ci in queue)
                        {
                            ++taskNomber;
                            Calc(ci, setting);
                            if (stopFlag)
                                throw new Exception(MessageReader.GetMessage("Err:UserStop"));
                        }
                        AddRepString(false, "QF", MessageReader.GetMessage("Msg:Q:Fin"));
                    }
                    catch (Exception e)
                    {
                        AddRepString(true, "QErr", ErrorLogger.Log(e), false);
                        AddRepString(false, "QEF", MessageReader.GetMessage("Msg:Q:FinError"));
                    }
                }
                setting.postExecute();
            }
            catch (Exception e)
            {
                ErrorLogger.Log(e);
            }
        }
        public static void SaveToStream(Stream dest, QueueManager value)
        {
            try
            {
                var x = new DataContractJsonSerializer(typeof(QueueManager));
                x.WriteObject(dest, value);
            }
            catch (Exception e)
            {
                throw new Exception(MessageReader.GetMessage("Err:Q:CantSSave"), e);
            }
        }
        public static void SaveToFile(string fileName, QueueManager value)
        {
            try
            {
                using (var writer = new FileStream(fileName, FileMode.Create))
                    SaveToStream(writer, value);
            }
            catch (Exception e)
            {
                var ex = new Exception(MessageReader.GetMessage("Err:Q:CantFSave"), e);
                ex.Data.Add(MessageReader.GetMessage("Err:FileName"), fileName);
                throw ex;
            }
        }
        public static QueueManager LoadFromStream(Stream s)
        {
            // Внимание, магия!
            try
            {
                var res = (QueueManager)(new DataContractJsonSerializer(typeof(QueueManager))).ReadObject(s);
                if (res.queue.Count == 0)
                    return res;
                var pred = res.queue[0];
                res.queue = res.queue.Take(1).Concat(res.queue.Skip(1).Select(x =>
                    {
                        object t = x;
                        foreach (var v in typeof(ComputeItem).GetFields()
                            .Select(f => Tuple.Create<dynamic, FieldInfo>(f.GetValue(x), f))
                            .Where(v => v.Item1 == null || MathEx.IsDefault(v.Item1))
                            .Select(v => v.Item2))
                            v.SetValue(t, v.GetValue(pred));
                        pred = (ComputeItem)t;
                        return pred;
                    })).ToList();
                return res;
            }
            catch (Exception e)
            {
                throw new Exception(MessageReader.GetMessage("Err:Q:CantSRead"), e);
            }
        }
        /// <summary>
        /// Создаёт новый экземпляр <see cref="QueueManager"/>, содержащий данные из файла.
        /// </summary>
        /// <param name="fileName">Файл с данными</param>
        /// <returns></returns>
        public static QueueManager LoadFromFile(string fileName)
        {
            try
            {
                using (var reader = new FileStream(fileName, FileMode.Open))
                {
                    var res = LoadFromStream(reader);
                    if (string.IsNullOrWhiteSpace(res.resDir))
                        res.resDir = Path.GetDirectoryName(fileName);
                    if (res.materials == null)
                        res.materials = new List<Material>();
                    return res;
                }
            }
            catch (Exception e)
            {
                var ex = new Exception(MessageReader.GetMessage("Err:Q:CantFRead"), e);
                ex.Data.Add(MessageReader.GetMessage("Err:FileName"), fileName);
                throw ex;
            }
        }
    }
}
