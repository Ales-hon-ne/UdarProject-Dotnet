using System;
using System.Collections.Generic;
//using System.Linq;
//using System.Text;

namespace UdarProject
{
    static class MathEx
    {
        /// <summary>
        /// Проверяет, является ли значение значением по-умолчанию.
        /// </summary>
        /// <typeparam name="T">Тип данных</typeparam>
        /// <param name="value">Значение</param>
        /// <returns>Возвращает true, если value == default(T), в остальных случаях false</returns>
        public static bool IsDefault<T>(T value)
        {
            return EqualityComparer<T>.Default.Equals(value, default(T));
        }
        /// <summary>
        /// Проверяет, включено ли значение в интервал.
        /// </summary>
        /// <param name="value">Значение, которое нужно проверить на включение в интервал</param>
        /// <param name="interval">Интервал (Item1; Item2)</param>
        /// <returns>
        /// Возвращает true, если value принадлежит interval, и false в противном случае.
        /// </returns>
        public static bool InRange<T>(T value, Tuple<T, T> interval) where T: IComparable<T>
        {
            return InRange(value, interval.Item1, interval.Item2);
        }
        /// <summary>
        /// Проверяет, включено ли значение в интервал.
        /// </summary>
        /// <param name="value">Значение, которое нужно проверить на включение в интервал</param>
        /// <param name="left">Левая граница интервала</param>
        /// <param name="right">Правая граница интервала</param>
        /// <returns>
        /// Возвращает true, если value принадлежит (left; right), и false в противном случае.
        /// </returns>
        public static bool InRange<T>(T value, T left, T right) where T : IComparable<T>
        {
            return value.CompareTo(left) == 1 && value.CompareTo(right) == -1;
        }
        /// <summary>
        /// Проверяет, включено ли значение в отрезок.
        /// </summary>
        /// <param name="value">Значение, которое нужно проверить на включение в отрезок</param>
        /// <param name="interval">Отрезок [Item1; Item2]</param>
        /// <returns>
        /// Возвращает true, если value принадлежит interval, и false в противном случае.
        /// </returns>
        public static bool InRangeInclusive<T>(T value, Tuple<T, T> interval) where T : IComparable<T>
        {
            return InRangeInclusive(value, interval.Item1, interval.Item2);
        }
        /// <summary>
        /// Проверяет, включено ли значение в отрезок.
        /// </summary>
        /// <param name="value">Значение, которое нужно проверить на включение в отрезок</param>
        /// <param name="left">Левая граница отрезка</param>
        /// <param name="right">Правая граница отрезка</param>
        /// <returns>
        /// Возвращает true, если value принадлежит [left; right], и false в противном случае.
        /// </returns>
        public static bool InRangeInclusive<T>(T value, T left, T right) where T : IComparable<T>
        {
            return value.CompareTo(left) >= 0 && value.CompareTo(right) <= 0;
        }
        /// <summary>
        /// Возвращает значение из интервала (left; right).
        /// </summary>
        /// <param name="value">Исходное значение</param>
        /// <param name="left">Левая граница интервала</param>
        /// <param name="right">Правая граница интервала</param>
        /// <returns>
        /// Если value принадлежит (left; right), возвращает value, иначе ближайшую к нему 
        /// границу интервала. Если обе границы расположены на равном расстоянии, то 
        /// возвращает левую.
        /// </returns>
        public static double EnsureRange(double value, double left, double right)
        {
            unchecked
            {
                if (value > left && value < right)
                    return value;
                else if (Math.Abs(right - value) >= Math.Abs(left - value))
                    return left;
                else
                    return right;
            }
        }
        /// <summary>
        /// Возвращает значение из интервала (left; right).
        /// </summary>
        /// <param name="value">Исходное значение</param>
        /// <param name="left">Левая граница интервала</param>
        /// <param name="right">Правая граница интервала</param>
        /// <returns>
        /// Если value принадлежит (left; right), возвращает value, иначе ближайшую к нему 
        /// границу интервала. Если обе границы расположены на равном расстоянии, 
        /// то возвращает левую.</returns>
        public static int EnsureRange(int value, int left, int right)
        {
            unchecked
            {
                if (value > left && value < right)
                    return value;
                else if (Math.Abs(right - value) >= Math.Abs(left - value))
                    return left;
                else
                    return right;
            }
        }
        /// <summary>
        /// Возвращает значение из интервала (left; right).
        /// </summary>
        /// <param name="value">Исходное значение</param>
        /// <param name="left">Левая граница интервала</param>
        /// <param name="right">Правая граница интервала</param>
        /// <returns>
        /// Если value принадлежит (left; right), возвращает value, иначе ближайшую к 
        /// нему границу интервала. Если интервал вырожденный, то возвращает левую.
        /// </returns>
        public static T EnsureRange<T>(T value, T left, T right) where T : IComparable<T>
        {
            if (left.CompareTo(right) >= 0)
                return left;
            if (value.CompareTo(left) == 1 && value.CompareTo(right) == -1)
                return value;
            else if (value.CompareTo(right) >=0)
                return right;
            else
                return left;
        }

        private static IDictionary<Tuple<double, double, double>, Func<double, double>> _HWSaver;
        private static IDictionary<Tuple<double, double, double>, Func<double, double>> HWSaver
        {
            get
            {
                if (_HWSaver == null)
                    _HWSaver = new Dictionary<Tuple<double, double, double>, Func<double, double>>();
                return _HWSaver;
            }
        }
        /// <summary>
        /// Возвращает функцию, описывающую гармонические синусоидальные колебания с заданными амплитудой, периодом и смещением.
        /// </summary>
        /// <param name="period">Период колебаний (>0)</param>
        /// <param name="amplitude">Амплитуда колебаний (>0)</param>
        /// <param name="shift">Начальное смещение</param>
        /// <returns>Функция f(t) = amplitude*sin(2pi/period*t+shift).</returns>
        public static Func<double, double> HarmonicWave(double period, double amplitude, double shift = 0.0)
        {
            if (period <= 0)
                throw new ArgumentOutOfRangeException("period");
            if (amplitude <= 0)
                throw new ArgumentOutOfRangeException("amplitude");
            while (shift > Math.PI)
                shift -= 2 * Math.PI;
            while (shift < -Math.PI)
                shift += 2 * Math.PI;
            var id = Tuple.Create(period, amplitude, shift);
            Func<double, double> f;
            if (!HWSaver.TryGetValue(id, out f))
            {
                f = t => amplitude * Math.Sin(2.0 * Math.PI / period * t + shift);
                HWSaver.Add(id, f);
            }
            return f;
        }
    }
}
