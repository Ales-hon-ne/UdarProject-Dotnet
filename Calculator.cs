using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UdarProject
{
    /// <summary>
    /// Представляет ошибку, произошедшую во время вычислений.
    /// </summary>
    internal class CalculatorException : ApplicationException
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="CalculatorException"/>.
        /// </summary>
        public CalculatorException() : base() { }
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="CalculatorException"/>, 
        /// используюя указанное сообщение об ошибке.
        /// </summary>
        /// <param name="message">Сообщение об ошибке</param>
        public CalculatorException(string message) : base(message) { }
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="CalculatorException"/>, 
        /// используюя указанное сообщение об ошибке и набором дополнитеьлной информации.
        /// </summary>
        /// <param name="message">Сообщение об ошибке</param>
        /// <param name="data">Дополнительная информация</param>
        public CalculatorException(string message, params Tuple<string, object>[] data)
            : this(message)
        {
            foreach (var d in data)
                Data.Add(d.Item1, d.Item2);
        }
        /// <summary>
        /// Причина ошибки.
        /// </summary>
        public static readonly string Cause = MessageReader.GetMessage("Err:Cause");
        /// <summary>
        /// Значение, вызвавшее ошибку.
        /// </summary>
        public static readonly string Value = MessageReader.GetMessage("Err:Value");
        /// <summary>
        /// Позиция ошибочной величины.
        /// </summary>
        public static readonly string Position = MessageReader.GetMessage("Err:Position");
        /// <summary>
        /// Аргументы функции, в которой произошла ошибка.
        /// </summary>
        public static readonly string Params = MessageReader.GetMessage("Err:Params");
        /// <summary>
        /// Шаг, на котором произошла ошибка.
        /// </summary>
        public static readonly string Step = MessageReader.GetMessage("Err:Step");
    }

    partial class Calculator
    {
        /// <summary>
        /// Представляет координаты точки в срезе (описывают линию в реальном пространстве)
        /// </summary>
        private struct Point
        {
            /// <summary>
            /// Координата по радиусу (от центра к краю).
            /// </summary>
            public double R;
            /// <summary>
            /// Координата по высоте (снизу вверх).
            /// </summary>
            public double Z;
            public static Point operator +(Point left, Point right)
            {
                return new Point() { R = left.R + right.R, Z = left.Z + right.Z };
            }
            public static Point operator *(Point left, double right)
            {
                return new Point() { R = left.R * right, Z = left.Z * right };
            }
            public static Point operator *(double left, Point right)
            {
                return right * left;
            }
        }
        /// <summary>
        /// Тензор в цидиндрическом пространстве
        /// </summary>
        private struct Tensor
        {
            public double RR;
            public double RZ;
            public double ZZ;
            public double TT;
            public static Tensor operator +(Tensor left, Tensor right)
            {
                return new Tensor()
                {
                    RR = left.RR + right.RR,
                    RZ = left.RZ + right.RZ,
                    ZZ = left.ZZ + right.ZZ,
                    TT = left.TT + right.TT
                };
            }
            public static Tensor operator *(Tensor left, double right)
            {
                return new Tensor()
                {
                    RR = left.RR * right,
                    RZ = left.RZ * right,
                    ZZ = left.ZZ * right,
                    TT = left.TT * right
                };
            }
            public static Tensor operator *(double left, Tensor right)
            {
                return right * left;
            }
        }

        /// <summary>
        /// Треугольный конечный элемент
        /// </summary>
        private struct Element
        {
            /// <summary>
            /// Индекс материала элемента.
            /// </summary>
            public int MaterialIndex;
            /// <summary>
            /// Индексы узлов, образующих элемент.
            /// </summary>
            /// <remarks>
            /// Item1 - для правой части нижний левый, далее против часовой.
            /// </remarks>
            public Tuple<int, int, int> Nodes;
            /// <summary>
            /// Площадь элемента.
            /// </summary>
            public double Square;
            /// <summary>
            /// Площадь элемента на предыдущем шаге.
            /// </summary>
            public double OldSquare;
            /// <summary>
            /// Центр масс элемента.
            /// </summary>
            public Point Barycenter;
            /// <summary>
            /// Масса элемента.
            /// </summary>
            public double Mass;
            /// <summary>
            /// Индикатор разрушения элемента.
            /// </summary>
            public bool Crashed;
            /// <summary>
            /// Предел текучести.
            /// </summary>
            public double F;
            /// <summary>
            /// Начальный объём КЭ (кольца треугольного сечения).
            /// </summary>
            public double V0;
            /// <summary>
            /// Давление.
            /// </summary>
            public double P;
            /// <summary>
            /// Тензор деформаций.
            /// </summary>
            public Tensor Eps;
            /// <summary>
            /// Тензор пластических деформаций.
            /// </summary>
            public Tensor EpsP;
            /// <summary>
            /// Квадрат второго инварианта тензора пластических деформаций.
            /// </summary>
            public double I2p;
            /// <summary>
            /// Второй инвариант тензора пластических деформаций (интенсивность 
            /// пластических деформаций).
            /// </summary>
            public double SqI2p;
            /// <summary>
            /// Тензор напряжений.
            /// </summary>
            public Tensor Sigma;
            /// <summary>
            /// Девиатор тензора напряжений.
            /// </summary>
            public Tensor S;
            /// <summary>
            /// Полные объёмные деформации.
            /// </summary>
            public double Theta;
            /// <summary>
            /// Пластические объёмные деформации.
            /// </summary>
            public double ThetaP;
            /// <summary>
            /// Температура.
            /// </summary>
            public double T;
            /// <summary>
            /// Значение функции деформирования.
            /// </summary>
            public double Psi;
            /// <summary>
            /// Внутренняя энергия.
            /// </summary>
            public double Energy;
            /// <summary>
            /// Выводимое напряжение (корень из второго инварианта девиатора тензора напряжений).
            /// </summary>
            public double sqY2;
            /// <summary>
            /// Производная от температуры по времени.
            /// </summary>
            public double dTdt;
            public override string ToString()
            {
                return Nodes.Item1.ToString() + " " + Nodes.Item2.ToString() + " " + Nodes.Item3.ToString();
            }
        }
        /// <summary>
        /// Узел сетки
        /// </summary>
        private struct Node
        {
            public override string ToString()
            {
                return "(" + R.ToString() + ";" + Z.ToString() + ")";
            }
            public enum NodeNmb { Node1 = 1, Node2, Node3 }
            /// <summary>
            /// Координата по радиусу (от центра к краю).
            /// </summary>
            public double R;
            /// <summary>
            /// Координата по высоте (снизу вверх).
            /// </summary>
            public double Z;
            /// <summary>
            /// Кооржинаты на предыдущем шаге.
            /// </summary>
            public Point OldCoord;
            /// <summary>
            /// Индексы элементов, примыкающих к узлу, и номера этого узла в тех элементах. 
            /// Null - отсутствие элемента.
            /// </summary>
            public Tuple<int, NodeNmb>[] Elements;
            /// <summary>
            /// "Масса" узла.
            /// </summary>
            public double Mass;
            /// <summary>
            /// Скорость узла (вершина вектора).
            /// </summary>
            public Point Speed;
            /// <summary>
            /// Скорость узла на предыдущем шаге по времени (вершина вектора).
            /// </summary>
            public Point NewSpeed;
        }
        enum DrawStyle { Crashed = 0, Temperature, Stress, Speed }
        
        /// <summary>
        /// Постановка задачи.
        /// </summary>
        private ComputeItem computeItem;
        /// <summary>
        /// Информирует менеджер очереди вычислений о текущем прогрессе расчёта.
        /// </summary>
        private Action<byte> percentCallback;
        /// <summary>
        /// Используемые материалы. Нулевой элемент - воздух.
        /// </summary>
        private Material[] materials;
        /// <summary>
        /// Предоставляет возможности для сохранения данных.
        /// </summary>
        private Saver saver;

        /// <summary>
        /// Флаг безусловной остановки.
        /// </summary>
        private bool stopFlag;
        /// <summary>
        /// !!!Выясняется!!!
        /// </summary>
        private double alf = 0.01;
        /// <summary>
        /// Коэффициент квадратичной псевдовязкости.
        /// </summary>
        private double c0 = 5e-6;
        /// <summary>
        /// Коэффициент линейной псевдовязкости.
        /// </summary>
        private double cl = 5e-11;
        /// <summary>
        /// Коэффициент сдвиговой вязкости.
        /// </summary>
        private double ca = 5e-7;

        /// <summary>
        /// Создаёт новый экземпляр класса <see cref="Calculator"/>.
        /// </summary>
        /// <param name="ci">Постановка задачи</param>
        /// <param name="mat">Список материалов</param>
        /// <param name="sav">Параметры сохранения результатов</param>
        /// <param name="pc">Информатор о текущем состоянии расчёта</param>
        public Calculator(ComputeItem ci, List<Material> mat, Saver sav, Action<byte> pc = null)
        {
            percentCallback = pc ?? (_ => { ;});
            computeItem = ci;
            // Нулевой элемент - воздух
            materials = Enumerable.Repeat(Material.Air, 1).Concat(mat).ToArray();
            saver = sav;
            stopFlag = false;
        }
        /// <summary>
        /// Устанавливает значение флага остановки в true.
        /// </summary>
        public void Stop()
        {
            stopFlag = true;
        }
    }
}
