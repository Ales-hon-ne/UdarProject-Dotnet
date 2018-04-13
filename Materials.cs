using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Drawing;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.IO;
//using System.Data;

namespace UdarProject
{
    /// <summary>
    /// Описывает важные свойства материала.
    /// </summary>
    [DataContract]
    struct Material
    {
        [DataContract]
        public struct VarAlpha
        {
            [DataMember]
            double T_min;
            [DataMember]
            double T_max;
            [DataMember]
            double A_T_min;
            [DataMember]
            double A_T_max;
            /// <summary>
            /// Возвращает значение коэффициента разупрочнения при указанной температуре.
            /// </summary>
            /// <param name="T">Температура</param>
            /// <returns>Значение в диапазоне [A_T_min, A_T_max].</returns>
            public double Alpha(double T)
            {
                return (A_T_max - A_T_min) * Math.Tanh(10/(T_max - T_min)*(T - 0.5*(T_max + T_min))) + 0.5 * (A_T_max + A_T_min);
            }
            /// <summary>
            /// Возвращает значение производной функции коэффициента разупрочнения 
            /// по температуре при указанной температуре.
            /// </summary>
            /// <param name="T">Температура</param>
            /// <returns>Значение производной.</returns>
            /// <remarks>Производная вычисляется приближённо.</remarks>
            public double DAlpha(double T)
            {
                const double step = 1e-5;
                const double invdstep = 5e4; // = 1/2step
                return invdstep * (Alpha(T - step) + Alpha(T + step));
            }
        }
        /// <summary>
        /// Материал по-умолчанию.
        /// </summary>
        public static Material Default
        {
            get
            {
                return new Material() 
                { 
                    Name = MessageReader.GetMessage("Inf:DefMat"), 
                    Color = Color.Black 
                };
            }
        }
        /// <summary>
        /// Стандартный воздух.
        /// </summary>
        public static Material Air
        {
            get
            {
                return new Material() 
                { 
                    Name = MessageReader.GetMessage("Inf:Air"), 
                    Color = Color.White, 
                    Alpha = 0, 
                    C = 0.022, 
                    G = 10000, 
                    Gamma = 0, 
                    K = 100, 
                    K1 = 0, 
                    Ro0 = 1.2, 
                    Sigma0 = 1000, 
                    Sigma1 = 0 
                };
            }
        }
        internal static Material[] Read(Stream s)
        {
            var res = (new DataContractJsonSerializer(typeof(Material[]))).ReadObject(s);
            return (Material[])res;
        }

        /// <summary>
        /// Видимое имя материала.
        /// </summary>
        [DataMember(Order = 1)]
        public string Name;

        /// <summary>
        /// Модуль сдвига.
        /// </summary>
        [DataMember(Order = 10)]
        public double G;
        /// <summary>
        /// Начальная плотность.
        /// </summary>
        [DataMember(Order = 10)]
        public double Ro0;
        /// <summary>
        /// Начальный предел текучести.
        /// </summary>
        [DataMember(Order = 10)]
        public double Sigma0;
        /// <summary>
        /// Предел текучести при разрушении.
        /// </summary>
        [DataMember(Order = 10)]
        public double Sigma1;
        /// <summary>
        /// Коэффициент объёмного сжатия.
        /// </summary>
        [DataMember(Order = 10)]
        public double K;
        /// <summary>
        /// Коэффициент угла внутреннего трения.
        /// </summary>
        [DataMember(IsRequired = false, Order = 10, EmitDefaultValue = false)]
        public double K1;
        /// <summary>
        /// Теплопроводность.
        /// </summary>
        [DataMember(Order = 10)]
        public double C;
        /// <summary>
        /// Коэффициент линейного теплового расширения.
        /// </summary>
        [DataMember(Order = 10)]
        public double Gamma;
        /// <summary>
        /// Коэффициент разупрочнения.
        /// </summary>
        [DataMember(IsRequired = false, Order = 10, EmitDefaultValue = false)]
        public double Alpha;
        /// <summary>
        /// Переменный коэффициент разупрочнения.
        /// </summary>
        [DataMember(IsRequired = false, Order = 10, EmitDefaultValue = false)]
        public VarAlpha? DynamicAlpha;

        /// <summary>
        /// Отображаемый цвет.
        /// </summary>
        public Color Color;
        /// <summary>
        /// Конвертер цвета для сохранения в файл.
        /// </summary>
        [DataMember(Name = "Color", Order = 10)]
        private string _strColor
        {
            get { return Convert.ToString(Color.ToArgb(), 16); }
            set { Color = Color.FromArgb(Convert.ToInt32(value, 16)); }
        }
        public override string ToString()
        {
            return Name;
        }
    }
}
