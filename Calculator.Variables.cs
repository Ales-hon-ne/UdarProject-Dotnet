using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UdarProject
{
    //Здесь - переменные для вычисления
    partial class Calculator
    {
        /// <summary>
        /// Число шагов по времени.
        /// </summary>
        int countOfStep;
        /// <summary>
        /// Число конечных элементов.
        /// </summary>
        int countOfElem;
        /// <summary>
        /// Номер первого КЭ верхней части.
        /// </summary>
        int firstOverheadElem;
        /// <summary>
        /// Номер первого узла верхней части.
        /// </summary>
        int firstOverheadNode;
        /// <summary>
        /// Число узлов.
        /// </summary>
        int countOfNodes;
        /// <summary>
        /// Начальный размер КЭ по радиусу.
        /// </summary>
        double dr0;
        /// <summary>
        /// Начальный размер КЭ по высоте.
        /// </summary>
        double dz0;
        /// <summary>
        /// Радиус верхней части в метрах.
        /// </summary>
        double overheadRadius;
        /// <summary>
        /// Высота верхней части в метрах.
        /// </summary>
        double overheadHeight;
        /// <summary>
        /// Число рядов верхней части.
        /// </summary>
        int countOHRows;
        /// <summary>
        /// Радиус нижней части в метрах.
        /// </summary>
        double baseRadius;
        /// <summary>
        /// Высота нижней части в метрах.
        /// </summary>
        double baseHeight;
        /// <summary>
        /// Число рядов нижней части.
        /// </summary>
        int countBaseRows;
        /// <summary>
        /// Массив элементов.
        /// </summary>
        Element[][] elements;
        /// <summary>
        /// Массив узлов.
        /// </summary>
        Node[][] nodes;
        /// <summary>
        /// Номера узлов на оси симметрии.
        /// </summary>
        int[] centralNodes;
        /// <summary>
        /// Номера узлов, зафиксированных по радиусу.
        /// </summary>
        int[] fixationR;
        /// <summary>
        /// Номера узлов, зафиксированных по высоте.
        /// </summary>
        int[] fixationZ;
        /// <summary>
        /// Текущий номер шага.
        /// </summary>
        int n;
    }
}
