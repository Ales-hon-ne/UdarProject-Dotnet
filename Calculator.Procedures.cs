using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing;
using CE = UdarProject.CalculatorException;

namespace UdarProject
{
    partial class Calculator
    {

        /// <summary>
        /// Запускает процесс вычисления
        /// </summary>
        public void Calculate()
        {
            stopFlag = false;
            n = -1;
            //WARNING!!! Порядок вызова может быть важен!
            Initialize();
            MatAssoc();
            NodeAssoc();
            BeginCoord();
            CalculateSquares();
            CalcMasses();
            NullLoop();
            //SaveResult("initial.vtk");
            SaveImage("initial.png");
            MainLoop();
            SaveResult("result.vtk");
            SaveImage("final.png");
            SaveImage("final_T.png", DrawStyle.Temperature);
            SaveImage("final_stress.png", DrawStyle.Stress);
        }
        /// <summary>
        /// Сохраняет текущее состояние в VTK-файл. 
        /// Предусловие: завершение <see cref="avValues"/> и <see cref="BeginCoord"/> (или любой другой процедуры, 
        /// вычисляющей значения координат узлов). 
        /// Не имеет побочных эффектов, кроме создания файла.
        /// </summary>
        /// <param name="fileName">Имя файла</param>
        void SaveResult(string fileName)
        {
            var writer = saver.GetInstance(fileName);

            writer.WriteLine("# vtk DataFile Version 3.0");
            writer.WriteLine("Axial2D result");
            writer.WriteLine("ASCII");
            writer.WriteLine("DATASET UNSTRUCTURED_GRID");
            writer.WriteLine(string.Format("POINTS {0} double", countOfNodes));
            for (int i = 0; i < countOfNodes; ++i)
                writer.Write(string.Format(System.Globalization.CultureInfo.InvariantCulture,
                    "{0} {1} 0.0 ", nodes[0][i].R, nodes[0][i].Z));
            writer.WriteLine();
            writer.WriteLine(string.Format("CELLS {0} {1}", countOfElem, countOfElem * 4));
            for (int i = 0; i < countOfElem; ++i)
                writer.Write(string.Format("3 {0} {1} {2} ", elements[0][i].Nodes.Item1, elements[0][i].Nodes.Item2, elements[0][i].Nodes.Item3));
            writer.WriteLine();
            writer.WriteLine(string.Format("CELL_TYPES {0}", countOfElem));
            for (int i = 0; i < countOfElem; ++i)
                writer.Write("5 ");
            writer.WriteLine();
            writer.WriteLine(string.Format("CELL_DATA {0}", countOfElem));
            writer.WriteLine("SCALARS Erase int 1");
            writer.WriteLine("LOOKUP_TABLE default");
            for (int i = 0; i < countOfElem; ++i)
            {
                if (elements[0][i].Crashed)
                    writer.Write("2 ");
                else if (elements[0][i].I2p > 0)
                    writer.Write("1 ");
                else
                    writer.Write("0 ");
            }
            writer.WriteLine();
            writer.WriteLine("SCALARS sqI2p double 1");
            writer.WriteLine("LOOKUP_TABLE default");
            for (int i = 0; i < countOfElem; ++i)
            {
                writer.Write(string.Format(System.Globalization.CultureInfo.InvariantCulture,
                    "{0} ", elements[0][i].SqI2p));
            }
            writer.WriteLine();
            writer.WriteLine("SCALARS sqY2 double 1");
            writer.WriteLine("LOOKUP_TABLE default");
            for (int i = 0; i < countOfElem; ++i)
            {
                writer.Write(string.Format(System.Globalization.CultureInfo.InvariantCulture,
                    "{0} ", elements[0][i].sqY2));
            }
            writer.WriteLine();
            writer.WriteLine(string.Format("POINT_DATA {0}", countOfNodes));
            writer.WriteLine("SCALARS TemperatureNV double 1");
            writer.WriteLine("LOOKUP_TABLE default");
            foreach (var node in nodes[0])
            {
                double t = 0.0;
                var nl = node.Elements.Where(x => x != null).ToArray();
                foreach (var el in nl)
                    t += elements[0][el.Item1].T;
                t /= nl.Length;
                writer.Write(string.Format(System.Globalization.CultureInfo.InvariantCulture,
                    "{0} ", t));
            }
            writer.WriteLine();
            writer.WriteLine("SCALARS sqI2pNV double 1");
            writer.WriteLine("LOOKUP_TABLE default");
            foreach (var node in nodes[0])
            {
                double val = 0.0;
                var nl = node.Elements.Where(x => x != null).ToArray();
                foreach (var el in nl)
                    val += elements[0][el.Item1].SqI2p;
                val /= nl.Length;
                writer.Write(string.Format(System.Globalization.CultureInfo.InvariantCulture,
                    "{0} ", val));
            }
            writer.WriteLine();
            writer.WriteLine("SCALARS sqY2NV double 1");
            writer.WriteLine("LOOKUP_TABLE default");
            foreach (var node in nodes[0])
            {
                double val = 0.0;
                var nl = node.Elements.Where(x => x != null).ToArray();
                foreach (var el in nl)
                    val += elements[0][el.Item1].sqY2;
                val /= nl.Length;
                writer.Write(string.Format(System.Globalization.CultureInfo.InvariantCulture,
                    "{0} ", val));
            }
            writer.WriteLine();
            writer.WriteLine("SCALARS PressureNV double 1");
            writer.WriteLine("LOOKUP_TABLE default");
            foreach (var node in nodes[0])
            {
                double val = 0.0;
                var nl = node.Elements.Where(x => x != null).ToArray();
                foreach (var el in nl)
                    val += elements[0][el.Item1].P;
                val /= nl.Length;
                writer.Write(string.Format(System.Globalization.CultureInfo.InvariantCulture,
                    "{0} ", val));
            }
            writer.WriteLine();
            writer.WriteLine("SCALARS PsiNV double 1");
            writer.WriteLine("LOOKUP_TABLE default");
            foreach (var node in nodes[0])
            {
                double val = 0.0;
                var nl = node.Elements.Where(x => x != null).ToArray();
                foreach (var el in nl)
                    val += elements[0][el.Item1].Psi;
                val /= nl.Length;
                writer.Write(string.Format(System.Globalization.CultureInfo.InvariantCulture,
                    "{0} ", val));
            }
            writer.WriteLine();
            writer.WriteLine("SCALARS AbsSpeed double 1");
            writer.WriteLine("LOOKUP_TABLE default");
            foreach (var node in nodes[0])
            {
                writer.Write(string.Format(System.Globalization.CultureInfo.InvariantCulture,
                    "{0} ", Math.Sqrt(node.Speed.R * node.Speed.R + node.Speed.Z * node.Speed.Z)));
            }
            writer.WriteLine();
            writer.WriteLine("VECTORS Speed double");
            foreach (var node in nodes[0])
            {
                writer.Write(string.Format(System.Globalization.CultureInfo.InvariantCulture,
                    "{0} {1} 0 ", node.Speed.R, node.Speed.Z));
            }
            writer.WriteLine();
            saver.CloseInstance(fileName);
        }
        /// <summary>
        /// Сохраняет изображение текущего состояния в файл. 
        /// Предусловие: завершение <see cref="avValues"/> и <see cref="BeginCoord"/> (или любой другой процедуры, 
        /// вычисляющей значения координат узлов). 
        /// Не имеет побочных эффектов, кроме создания файла.
        /// </summary>
        /// <param name="fileName">Имя файла</param>
        void SaveImage(string fileName, DrawStyle style = DrawStyle.Crashed)
        {
            const int imgWidth = 700;
            int imgHeight = (int)(imgWidth * (baseHeight + overheadHeight) / baseRadius);
            Func<double, int> R2X = r => (int)(imgWidth * 0.05 + r * (imgWidth / baseRadius));
            Func<double, int> Z2Y = z => (int)(imgHeight * 1.05 - z * (imgWidth / baseRadius)); //коэффициент при z должен быть таким же, как и у r
            var img = new Bitmap((int)(imgWidth * 1.3), (int)(imgHeight * 1.3));
            var graph = Graphics.FromImage(img);
            graph.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            var pen = new Pen(Color.Black);
            graph.Clear(Color.White);
            foreach (var el in elements[0])
            {
                var poly = new System.Drawing.Point[3] 
                { 
                    new System.Drawing.Point(R2X(nodes[0][el.Nodes.Item1].R), Z2Y(nodes[0][el.Nodes.Item1].Z)),
                    new System.Drawing.Point(R2X(nodes[0][el.Nodes.Item2].R), Z2Y(nodes[0][el.Nodes.Item2].Z)),
                    new System.Drawing.Point(R2X(nodes[0][el.Nodes.Item3].R), Z2Y(nodes[0][el.Nodes.Item3].Z))
                };
                Imaging.ValueColor colorer;
                Brush brush;
                switch (style)
                {
                    case DrawStyle.Crashed:
                        colorer = Imaging.ValueColor.Crashed(materials[el.MaterialIndex].Color);
                        brush = new SolidBrush(colorer.GetColor(el.Crashed));
                        break;
                    case DrawStyle.Temperature:
                        colorer = Imaging.ValueColor.Temperature;
                        brush = new SolidBrush(colorer.GetColor(el.T));
                        break;
                    case DrawStyle.Stress:
                        colorer = Imaging.ValueColor.Stress;
                        brush = new SolidBrush(colorer.GetColor(el.sqY2));
                        break;
                    case DrawStyle.Speed:
                        colorer = Imaging.ValueColor.Speed;
                        double speed1 = Math.Sqrt(Math.Pow(nodes[0][el.Nodes.Item1].Speed.R, 2.0) + Math.Pow(nodes[0][el.Nodes.Item1].Speed.Z, 2.0));
                        double speed2 = Math.Sqrt(Math.Pow(nodes[0][el.Nodes.Item2].Speed.R, 2.0) + Math.Pow(nodes[0][el.Nodes.Item2].Speed.Z, 2.0));
                        double speed3 = Math.Sqrt(Math.Pow(nodes[0][el.Nodes.Item3].Speed.R, 2.0) + Math.Pow(nodes[0][el.Nodes.Item3].Speed.Z, 2.0));
                        brush = new SolidBrush(colorer.GetColor((speed1 + speed2 + speed3) / 3.0));
                        break;
                    default:
                        throw new Exception("Неверный способ отображения");
                }
                graph.FillPolygon(brush, poly);
                graph.DrawPolygon(pen, poly);
            }
            //pen.Width = 3;
            //graph.DrawLine(pen, new System.Drawing.Point(R2X(0), Z2Y(nodes[0][0].Z)), new System.Drawing.Point(R2X(0), Z2Y(nodes[0][countOfNodes - computeItem.OverheadRadius - 1].Z)));
            //graph.DrawLines(pen, Enumerable.Range(0, computeItem.BaseRadius + 1).Select(i => nodes[0][i]).Select(n => new System.Drawing.Point(R2X(n.R), Z2Y(n.Z))).ToArray());
            //graph.DrawLines(pen, Enumerable.Range(1, countBaseRows + 1).Select(i => nodes[0][i * (computeItem.BaseRadius + 1) - 1]).Select(n => new System.Drawing.Point(R2X(n.R), Z2Y(n.Z))).ToArray());
            //graph.DrawLines(pen, Enumerable.Range(0, computeItem.BaseRadius + 1).Select(i => nodes[0][i + firstOverheadNode - computeItem.BaseRadius - 1]).Select(n => new System.Drawing.Point(R2X(n.R), Z2Y(n.Z))).ToArray());
            //graph.DrawLines(pen, (new List<Node>() { nodes[0][firstOverheadNode - computeItem.BaseRadius + computeItem.OverheadRadius - 1] }).Concat(Enumerable.Range(2, countOHRows).Select(i => nodes[0][i * (computeItem.OverheadRadius + 1) - 2 + firstOverheadNode - computeItem.OverheadRadius])).Select(n => new System.Drawing.Point(R2X(n.R), Z2Y(n.Z))).ToArray());
            //graph.DrawLines(pen, Enumerable.Range(countOfNodes - computeItem.OverheadRadius - 1, computeItem.OverheadRadius + 1).Select(i => nodes[0][i]).Select(n => new System.Drawing.Point(R2X(n.R), Z2Y(n.Z))).ToArray());

            pen.Width = 3;
            if (style == DrawStyle.Speed)
            {
                pen.Color = Color.FromArgb(180, Color.DarkGreen);
                foreach (var node in nodes[0])
                {
                    graph.DrawEllipse(pen, R2X(node.R) - 2, Z2Y(node.Z) - 2, 4, 4);
                    graph.FillEllipse(Brushes.DarkGreen, R2X(node.R) - 2, Z2Y(node.Z) - 2, 4, 4);
                    int vecx = R2X(node.R) + (int)node.Speed.R / 3,
                        vecy = Z2Y(node.Z) - (int)node.Speed.Z / 3;
                    graph.DrawLine(pen,
                        R2X(node.R), Z2Y(node.Z),
                        vecx, vecy);
                }
                pen.Color = Color.Black;
            }

            pen.Width = 1;
            ((Action<string, Image>)saver.WriteImageToFile).BeginInvoke(fileName, img, null, null);
        }
        /// <summary>
        /// Заполняет значения перменных <see cref="countOfElem"/>, <see cref="countOfNodes"/>, 
        /// <see cref="dr0"/>, <see cref="dz0"/>, <see cref="overheadHeight"/>, <see cref="baseHeight"/>, 
        /// <see cref="overheadRadius"/>, <see cref="baseRadius"/>, <see cref="firstOverheadElem"/>,
        /// <see cref="firstOverheadNode"/>; 
        /// инициализирует массивы <see cref="elements"/> и <see cref="nodes"/>. 
        /// Имеет побочные эффекты.
        /// </summary>
        void Initialize()
        {
            if (computeItem.TimePeriod <= 0)
                throw new CE(MessageReader.GetMessage("Err:C:SmallTP"),
                    Tuple.Create<string, object>(CE.Value, computeItem.TimePeriod));
            if (computeItem.TimeStep <= 0)
                throw new CE(MessageReader.GetMessage("Err:C:SmallTS"),
                    Tuple.Create<string, object>(CE.Value, computeItem.TimeStep));
            countOfStep = (int)Math.Round(computeItem.TimePeriod / computeItem.TimeStep);
            if (countOfStep < 2)
                throw new CE(MessageReader.GetMessage("Err:C:CoTL2"),
                    Tuple.Create<string, object>(CE.Value, countOfStep),
                    Tuple.Create<string, object>(CE.Cause, MessageReader.GetMessage("Err:C:CoTL2/BC")));
            int bHeight = 0;
            if (computeItem.BaseHeight == null || computeItem.BaseHeight.Length == 0)
                throw new CE(MessageReader.GetMessage("Err:C:NoBHData"),
                    (computeItem.BaseHeight == null) ?
                        Tuple.Create<string, object>(CE.Cause, MessageReader.GetMessage("Err:C:NoBHData/NullRef")) :
                        Tuple.Create<string, object>(CE.Cause, MessageReader.GetMessage("Err:C:NoBHData/EmptyArr")));
            foreach (var stratHeight in computeItem.BaseHeight)
            {
                if (stratHeight <= 0)
                    throw new CE(MessageReader.GetMessage("Err:C:SmallSH"),
                        Tuple.Create<string, object>(CE.Value, stratHeight),
                        Tuple.Create<string, object>(CE.Position, Array.IndexOf(computeItem.BaseHeight, stratHeight)));
                bHeight += stratHeight;
            }
            int overHeight = 0;
            if (computeItem.BaseHeight == null || computeItem.BaseHeight.Length == 0)
                throw new CE(MessageReader.GetMessage("Err:C:NoOhHData"),
                    (computeItem.BaseHeight == null) ?
                        Tuple.Create<string, object>(CE.Cause, MessageReader.GetMessage("Err:C:NoBHData/NullRef")) :
                        Tuple.Create<string, object>(CE.Cause, MessageReader.GetMessage("Err:C:NoBHData/EmptyArr")));
            foreach (var stratHeight in computeItem.OverheadHeight)
            {
                if (stratHeight <= 0)
                    throw new CE(MessageReader.GetMessage("Err:C:SmallSH"),
                        Tuple.Create<string, object>(CE.Value, stratHeight),
                        Tuple.Create<string, object>(CE.Position, Array.IndexOf(computeItem.OverheadHeight, stratHeight)));
                overHeight += stratHeight;
            }
            countOfElem = 2 * (bHeight * computeItem.BaseRadius + overHeight * computeItem.OverheadRadius);
            if ((bool)computeItem.AlterTopology)
                countOfElem += bHeight + overHeight;
            //overHeight без единицы, т.к. иначе стык будет учтён дважды
            countOfNodes = (bHeight + 1) * (computeItem.BaseRadius + 1) + overHeight * (computeItem.OverheadRadius + 1);
            if ((bool)computeItem.AlterTopology)
                countOfNodes += (overHeight + bHeight + 1) / 2;
            dr0 = computeItem.FER;
            dz0 = computeItem.FEZ;
            if (dz0 <= 0 || dr0 <= 0)
            {
                var val = ((dz0 <= 0) ? "dZ = " + dz0.ToString() : "") + ", " + ((dr0 <= 0) ? "dR = " + dr0.ToString() : "");
                throw new CE(MessageReader.GetMessage("Err:C:SmallFES"),
                        Tuple.Create<string, object>(CE.Value, val));
            }
            overheadHeight = overHeight * dz0;
            countOHRows = overHeight;
            baseHeight = bHeight * dz0;
            countBaseRows = bHeight;
            overheadRadius = computeItem.OverheadRadius * dr0;
            baseRadius = computeItem.BaseRadius * dr0;
            firstOverheadElem = 2 * (bHeight * computeItem.BaseRadius);
            if ((bool)computeItem.AlterTopology)
                firstOverheadElem += bHeight;
            firstOverheadNode = (bHeight + 1) * (computeItem.BaseRadius + 1);
            if ((bool)computeItem.AlterTopology)
                firstOverheadNode += (bHeight + 1) / 2;
            if (computeItem.InstCount < 1)
                computeItem.InstCount = 1;
            elements = new Element[computeItem.InstCount + 1][];
            nodes = new Node[computeItem.InstCount + 1][];
            for (int j = 0; j <= computeItem.InstCount; ++j)
            {
                elements[j] = new Element[countOfElem];
                nodes[j] = new Node[countOfNodes];
                Parallel.For(0, countOfNodes, i => nodes[j][i].Elements = new Tuple<int, Node.NodeNmb>[6] { null, null, null, null, null, null });
            }
            centralNodes = new int[countBaseRows + countOHRows + 1];
            centralNodes[0] = 0;
            for (int i = 1; i < centralNodes.Length; ++i)
            {
                centralNodes[i] = centralNodes[i - 1] + ((i <= countBaseRows) ? computeItem.BaseRadius : computeItem.OverheadRadius) + 1;
                if ((bool)computeItem.AlterTopology && Math.Abs(bHeight - i) % 2 == 1)
                {
                    centralNodes[i] += 1;
                }
            }
            //Parallel.For(0, countBaseRows + 2, i => centralNodes[i] = i * (computeItem.BaseRadius + 1));
            //Parallel.For(0, countOHRows, i => centralNodes[i + countBaseRows + 1] = i * (computeItem.OverheadRadius + 1) + firstOverheadNode);
            var fixrtmp = new HashSet<int>(centralNodes);
            var fixztmp = new HashSet<int>();
            if (computeItem.Fixation == ComputeItem.FixationType.Side || computeItem.Fixation == ComputeItem.FixationType.Both)
                for (int i = 1; i < countBaseRows + 2; ++i)
                {
                    fixrtmp.Add(centralNodes[i] - 1);
                    fixztmp.Add(centralNodes[i] - 1);
                }
            if (computeItem.Fixation == ComputeItem.FixationType.Bottom || computeItem.Fixation == ComputeItem.FixationType.Both)
            {
                for (int i = 0; i < computeItem.BaseRadius + 1; ++i)
                {
                    fixztmp.Add(i);
                    fixrtmp.Add(i);
                }
                if ((bool)computeItem.AlterTopology)
                {
                    fixztmp.Add(centralNodes[1] - 1);
                    fixrtmp.Add(centralNodes[1] - 1);
                }
            }
            fixationR = fixrtmp.ToArray();
            fixationZ = fixztmp.ToArray();
        }
        /// <summary>
        /// Ассоциация материалов с элементами. 
        /// Предусловие: завершение <see cref="Initialize"/>. 
        /// Имеет побочные эффекты.
        /// </summary>
        void MatAssoc()
        {
            int strat = 0, predStrat, sid = 0;
            int rad = computeItem.BaseRadius * 2;
            if ((bool)computeItem.AlterTopology)
                rad += 1;
            //нижняя часть
            foreach (int sh in computeItem.BaseHeight)
            {
                predStrat = strat;
                strat += sh;
                for (int j = 0; j <= computeItem.InstCount; ++j)
                {
                    Parallel.For(predStrat * rad, strat * rad, i =>
                        {
                            elements[j][i].MaterialIndex = computeItem.BaseMaterials[sid];
                        });
                }
                ++sid;
            }
            sid = 0;
            strat = 0;
            rad = computeItem.OverheadRadius * 2;
            if ((bool)computeItem.AlterTopology)
                rad += 1;
            //верхняя часть
            foreach (int sh in computeItem.OverheadHeight)
            {
                predStrat = strat;
                strat += sh;
                Parallel.For(predStrat * rad + firstOverheadElem,
                    strat * rad + firstOverheadElem, i =>
                        {
                            for (int j = 0; j <= computeItem.InstCount; ++j)
                            {
                                elements[j][i].MaterialIndex = computeItem.OverheadMaterials[sid];
                            }
                        });
                ++sid;
            }
        }
        /// <summary>
        /// Определяет узлы, лежащие на вершинах квадрата. Не имеет побочных эффектов.
        /// </summary>
        /// <param name="sqid">Номер квадрата</param>
        /// <returns>Номера узлов начиная с левого нижнего против часовой стрелки (для правой половины)</returns>
        Tuple<int, int, int, int> GetSqNodes(int sqid)
        {
            int add;
            int radius;
            int pos;
            int firstOHSq = firstOverheadElem / 2;
            if (sqid < firstOHSq)
            {
                pos = sqid;
                radius = computeItem.BaseRadius;
                add = 0;
            }
            else if (sqid >= firstOHSq && sqid < firstOHSq + computeItem.OverheadRadius)
            {
                pos = sqid - firstOHSq;
                radius = computeItem.BaseRadius;
                add = firstOverheadNode - radius - 1;
            }
            else
            {
                radius = computeItem.OverheadRadius;
                pos = sqid - firstOHSq - radius;
                add = firstOverheadNode;
            }
            int rp;
            int row = Math.DivRem(pos, radius, out rp);
            int ns = row * (radius + 1) + rp + add;
            return Tuple.Create(ns, ns + 1, ns + radius + 2, ns + radius + 1);
        }
        /// <summary>
        /// Ассоциация узлов с элементами. 
        /// Предусловие: завершение <see cref="Initialize"/>. 
        /// Имеет побочные эффекты.
        /// </summary>
        void NodeAssoc()
        {
            const bool usel = false;
            if (!(bool)computeItem.AlterTopology)
                Parallel.For(0, countOfElem / 2, i =>
                {
                    var sqn = GetSqNodes(i);
                    for (int j = 0; j <= computeItem.InstCount; ++j)
                    {
                        if ((i < firstOverheadElem / 2 && !usel) ||
                            ((i < firstOverheadElem / 2 && (i % computeItem.BaseRadius) % 2 == j % 2) ||
                            (i >= firstOverheadElem / 2 && (i % computeItem.OverheadRadius) % 2 == j % 2))
                            && usel)
                        {
                            //заполним номера элементов из этого квадрата в списки узлов
                            nodes[j][sqn.Item1].Elements[3] = Tuple.Create(2 * i, Node.NodeNmb.Node1);
                            nodes[j][sqn.Item2].Elements[4] = Tuple.Create(2 * i + 1, Node.NodeNmb.Node1);
                            nodes[j][sqn.Item2].Elements[5] = Tuple.Create(2 * i, Node.NodeNmb.Node2);
                            nodes[j][sqn.Item3].Elements[0] = Tuple.Create(2 * i + 1, Node.NodeNmb.Node2);
                            nodes[j][sqn.Item4].Elements[1] = Tuple.Create(2 * i, Node.NodeNmb.Node3);
                            nodes[j][sqn.Item4].Elements[2] = Tuple.Create(2 * i + 1, Node.NodeNmb.Node3);
                            //заполним номера узлов в списки элементов
                            elements[j][2 * i].Nodes = Tuple.Create(sqn.Item1, sqn.Item2, sqn.Item4);
                            elements[j][2 * i + 1].Nodes = Tuple.Create(sqn.Item2, sqn.Item3, sqn.Item4);
                        }
                        else
                        {
                            nodes[j][sqn.Item1].Elements[2] = Tuple.Create(2 * i, Node.NodeNmb.Node1);
                            nodes[j][sqn.Item1].Elements[3] = Tuple.Create(2 * i + 1, Node.NodeNmb.Node1);
                            nodes[j][sqn.Item2].Elements[4] = Tuple.Create(2 * i, Node.NodeNmb.Node2);
                            nodes[j][sqn.Item3].Elements[5] = Tuple.Create(2 * i + 1, Node.NodeNmb.Node2);
                            nodes[j][sqn.Item3].Elements[0] = Tuple.Create(2 * i, Node.NodeNmb.Node3);
                            nodes[j][sqn.Item4].Elements[1] = Tuple.Create(2 * i + 1, Node.NodeNmb.Node3);
                            elements[j][2 * i].Nodes = Tuple.Create(sqn.Item1, sqn.Item2, sqn.Item3);
                            elements[j][2 * i + 1].Nodes = Tuple.Create(sqn.Item1, sqn.Item3, sqn.Item4);
                        }
                    }
                });
            else
                for (int j = 0; j <= computeItem.InstCount; ++j)
                {
                    int nf = 0;
                    int n0, n1, n2, n3;
                    int rpos = 0;
                    bool brow = countBaseRows % 2 == 0;
                    for (int i = 0; i < firstOverheadElem; ++i)
                    {
                        //int rpos = i % (2 * computeItem.BaseRadius + 1);
                        n1 = nf;
                        n0 = n1 + computeItem.BaseRadius + 2;
                        if ((rpos % 2 == 0 && brow) || (rpos % 2 == 1 && !brow))
                        {
                            n2 = n0;
                            n3 = n0 - 1;
                            nodes[j][n1].Elements[3] = Tuple.Create(i, Node.NodeNmb.Node1);
                            nodes[j][n2].Elements[5] = Tuple.Create(i, Node.NodeNmb.Node2);
                            nodes[j][n3].Elements[1] = Tuple.Create(i, Node.NodeNmb.Node3);
                        }
                        else
                        {
                            n2 = n1 + 1;
                            n3 = n0;
                            nf += 1;
                            nodes[j][n1].Elements[2] = Tuple.Create(i, Node.NodeNmb.Node1);
                            nodes[j][n2].Elements[4] = Tuple.Create(i, Node.NodeNmb.Node2);
                            nodes[j][n3].Elements[0] = Tuple.Create(i, Node.NodeNmb.Node3);
                        }
                        elements[j][i].Nodes = Tuple.Create(n1, n2, n3);
                        /*(!brow && rpos == 2 * computeItem.BaseRadius + 1)
                            nf += 1;*/
                        if (++rpos > 2 * computeItem.BaseRadius)
                        {
                            rpos = 0;
                            nf += 1;
                            brow = !brow;
                        }
                    }
                    //nf += 1;
                    brow = true;
                    rpos = 0;
                    for (int i = 0; i < countOfElem - firstOverheadElem; ++i)
                    {
                        //int n0, n1, n2, n3;
                        n1 = nf;
                        n0 = n1 + computeItem.OverheadRadius + 2;
                        if (i < 2 * computeItem.OverheadRadius + 1)
                            n0 = n1 + computeItem.BaseRadius + 2;
                        if ((rpos % 2 == 0 && brow) || (rpos % 2 == 1 && !brow))
                        {
                            n2 = n0;
                            n3 = n0 - 1;
                            nodes[j][n1].Elements[3] = Tuple.Create(i + firstOverheadElem, Node.NodeNmb.Node1);
                            nodes[j][n2].Elements[5] = Tuple.Create(i + firstOverheadElem, Node.NodeNmb.Node2);
                            nodes[j][n3].Elements[1] = Tuple.Create(i + firstOverheadElem, Node.NodeNmb.Node3);
                        }
                        else
                        {
                            n2 = n1 + 1;
                            n3 = n0;
                            nf += 1;
                            nodes[j][n1].Elements[2] = Tuple.Create(i + firstOverheadElem, Node.NodeNmb.Node1);
                            nodes[j][n2].Elements[4] = Tuple.Create(i + firstOverheadElem, Node.NodeNmb.Node2);
                            nodes[j][n3].Elements[0] = Tuple.Create(i + firstOverheadElem, Node.NodeNmb.Node3);
                        }
                        elements[j][i + firstOverheadElem].Nodes = Tuple.Create(n1, n2, n3);
                        if (++rpos > 2 * computeItem.OverheadRadius)
                        {
                            rpos = 0;
                            nf += 1;
                            if (i <= 2 * computeItem.OverheadRadius + 1)
                                nf += computeItem.BaseRadius - computeItem.OverheadRadius;
                            brow = !brow;
                        }
                    }
                }
        }
        /// <summary>
        /// Вычисление начальных координат узлов. 
        /// Предусловие: завершение <see cref="Initialize"/>. 
        /// Имеет побочные эффекты.
        /// </summary>
        void BeginCoord()
        {
            var rnd = new Random();
            Func<double> zShift = () => (rnd.Next(200) - 100) / 3000.0 * dz0;
            Func<double> rShift = () => (rnd.Next(200) - 100) / 3000.0 * dr0;
            Func<Tuple<double, double>> randCirc = () =>
                {
                    const double maxRadius = 0.05;
                    double rpi = rnd.NextDouble() * 2 * Math.PI;
                    double rl = rnd.NextDouble() * maxRadius;
                    return Tuple.Create(rl * dr0 * Math.Sin(rpi), rl * dz0 * Math.Cos(rpi));
                };
            if (!(bool)computeItem.AlterTopology)
                Parallel.For(0, countOfNodes, i =>
                {
                    for (int j = 0; j <= computeItem.InstCount; ++j)
                    {
                        if (i < firstOverheadNode)
                        {
                            int rp = i % (computeItem.BaseRadius + 1);
                            int zp = i / (computeItem.BaseRadius + 1);
                            nodes[j][i].Z = dz0 * zp;
                            nodes[j][i].R = dr0 * rp;
                            //Немного случаности не повредит. Позволит избежать части проблем, связанных с равномерной сеткой.
                            if (j > 1 && i > computeItem.BaseRadius && i < (firstOverheadNode - computeItem.BaseRadius) && MathEx.InRange(rp, 0, computeItem.BaseRadius))
                            {
                                var rs = randCirc();
                                nodes[j][i].Z += rs.Item1;
                                nodes[j][i].R += rs.Item2;
                            }
                        }
                        else
                        {
                            int rp = (i - firstOverheadNode) % (computeItem.OverheadRadius + 1);
                            int zp = (i - firstOverheadNode) / (computeItem.OverheadRadius + 1);
                            nodes[j][i].Z = dz0 * (zp + 1) + baseHeight;
                            nodes[j][i].R = dr0 * rp;
                            if (j > 1 && i > firstOverheadNode && i < (countOfNodes - computeItem.OverheadRadius) && MathEx.InRange(rp, 0, computeItem.OverheadRadius))
                            {
                                var rs = randCirc();
                                nodes[j][i].Z += rs.Item1;
                                nodes[j][i].R += rs.Item2;
                            }
                        }
                    }
                });
            else
                for (int j = 0; j <= computeItem.InstCount; ++j)
                {
                    bool brow = countBaseRows % 2 == 0;
                    int rp = 0, r = 0;
                    int rad = computeItem.BaseRadius;
                    for (int i = 0; i < countOfNodes; ++i)
                    {
                        nodes[j][i].R = MathEx.EnsureRange(rp * dr0 - ((brow) ? 0.0 : 0.5 * dr0 /*+ rShift()*/), 0.0, rad * dr0);
                        nodes[j][i].Z = r * dz0;
                        rp += 1;
                        if ((rp > rad && brow) || (rp > rad + 1 && !brow))
                        {
                            rp = 0;
                            r += 1;
                            brow = !brow;
                            if (r > countBaseRows)
                                rad = computeItem.OverheadRadius;
                        }
                    }
                }
        }
        /// <summary>
        /// Вычисляет площади элементов. 
        /// Предусловие: завершение <see cref="BeginCoord"/> или любой другой процедуры, 
        /// вычисляющей значения координат узлов. 
        /// Имеет побочные эффекты.
        /// </summary>
        void CalculateSquares()
        {
            Parallel.For(0, countOfElem, i =>
                {
                    for (int j = 1; j <= computeItem.InstCount; ++j)
                    {
                        var P1 = new Point()
                        {
                            R = nodes[j][elements[j][i].Nodes.Item1].R,
                            Z = nodes[j][elements[j][i].Nodes.Item1].Z
                        };
                        var P2 = new Point()
                        {
                            R = nodes[j][elements[j][i].Nodes.Item2].R,
                            Z = nodes[j][elements[j][i].Nodes.Item2].Z
                        };
                        var P3 = new Point()
                        {
                            R = nodes[j][elements[j][i].Nodes.Item3].R,
                            Z = nodes[j][elements[j][i].Nodes.Item3].Z
                        };
                        elements[j][i].Barycenter.R = (P1.R + P2.R + P3.R) / 3.0;
                        elements[j][i].Barycenter.Z = (P1.Z + P2.Z + P3.Z) / 3.0;
                        elements[j][i].Square = 0.5 * Math.Abs((P1.R - P3.R) * (P2.Z - P3.Z) -
                            (P2.R - P3.R) * (P1.Z - P3.Z));
                        /*if (elements[j][i].Square < 1e-15)
                            elements[j][i].Square = 1e-15;*/
                    }
                });
        }
        /// <summary>
        /// Вычисляет массы узлов. 
        /// Предусловие: завершение <see cref="MatAssoc"/>, <see cref="CalculateSquares"/>, <see cref="NodeAssoc"/>. 
        /// Имеет побочные эффекты.
        /// </summary>
        void CalcMasses()
        {
            Parallel.For(0, countOfElem, i =>
                {
                    for (int j = 0; j <= computeItem.InstCount; ++j)
                    {
                        elements[j][i].Mass = 2 * Math.PI * materials[elements[j][i].MaterialIndex].Ro0 * elements[j][i].Barycenter.R * elements[j][i].Square;
                    }
                });
            Parallel.For(0, countOfNodes, i =>
                {
                    for (int j = 0; j <= computeItem.InstCount; ++j)
                    {
                        double mass = 0;
                        foreach (var ei in nodes[j][i].Elements)
                            if (ei != null)
                                mass += elements[j][ei.Item1].Mass / 3.0;
                        nodes[j][i].Mass = mass;
                    }
                });
        }
        /// <summary>
        /// Определяет начальные условия. 
        /// Предусловие: завершение <see cref="MatAssoc"/>, <see cref="CalculateSquares"/>. 
        /// Имеет побочные эффекты.
        /// </summary>
        void NullLoop()
        {
            Parallel.For(0, computeItem.InstCount + 1, j =>
                {
                    Parallel.For(0, countOfElem, i =>
                        {
                            elements[j][i].F = materials[elements[j][i].MaterialIndex].Sigma0;
                            elements[j][i].V0 = 2 * Math.PI * elements[j][i].Square * elements[j][i].Barycenter.R;
                            elements[j][i].EpsP.RR = elements[j][i].EpsP.RZ = elements[j][i].EpsP.ZZ = elements[j][i].EpsP.TT = 0.0;
                            elements[j][i].Eps.RR = elements[j][i].Eps.RZ = elements[j][i].Eps.ZZ = elements[j][i].Eps.TT = 0.0;
                            elements[j][i].Energy = 0.0;
                            elements[j][i].OldSquare = 0.0;
                            elements[j][i].ThetaP = 0.0;
                            elements[j][i].Crashed = false;
                            elements[j][i].SqI2p = 0.0;
                            elements[j][i].Theta = 0.0;
                            elements[j][i].T = computeItem.T0;
                            elements[j][i].Psi = 0;
                        });
                    Parallel.For(0, countOfNodes, i =>
                        {
                            //По-умолчанию всё по нулям, далее может быть перезаписано
                            nodes[j][i].Speed.Z = 0.0;
                            nodes[j][i].Speed.R = 0.0;
                            nodes[j][i].NewSpeed.Z = 0.0;
                            nodes[j][i].NewSpeed.R = 0.0;
                            //Движение ударника
                            if (computeItem.OverheadSpeed.HasValue)
                            {
                                if (i >= firstOverheadNode)
                                {
                                    nodes[j][i].Speed.Z = -computeItem.OverheadSpeed.Value;
                                }
                                if (i >= (firstOverheadNode - computeItem.BaseRadius - 1) && i < (firstOverheadNode - computeItem.BaseRadius + computeItem.OverheadRadius))
                                {
                                    nodes[j][i].Speed.Z = -0.5 * computeItem.OverheadSpeed.Value;
                                }
                            }
                            //Верхний удар
                            if (computeItem.TopImpactSpeed.HasValue)
                            {
                                if (i >= countOfNodes - computeItem.OverheadRadius - 1)
                                {
                                    nodes[j][i].Speed.Z = -computeItem.TopImpactSpeed.Value;
                                }
                            }
                            //Нижний удар
                            if (computeItem.BottomImpactSpeed.HasValue)
                            {
                                if (i <= computeItem.BaseRadius + 1)
                                {
                                    nodes[j][i].Speed.Z = computeItem.BottomImpactSpeed.Value;
                                }
                            }
                        });
                });
            avValues();
        }
        /// <summary>
        /// Основной цикл вычислений. 
        /// Предусловие: завершение <see cref="NullLoop"/>, <see cref="CalcMasses"/>. 
        /// Имеет побочные эффекты.
        /// </summary>
        void MainLoop()
        {
            double tim = 0.0;
            //используемый шаг по времени (может не совпадать с заданным!)
            double dtime = computeItem.TimeStep;
            double dt;
            //текущее время
            double timepr;
            //индикатор расхождения по времени
            int ctec;
            var pcbRes = percentCallback.BeginInvoke(0, null, null);
            for (n = 0; n < countOfStep; ++n)
            {
                timepr = n * computeItem.TimeStep;
                ctec = 0;
                //if (tim + 2.0 * dtime > timepr && tim + dtime < timepr)
                if (MathEx.InRange(timepr - tim, dtime, 2 * dtime))
                    dtime = 0.51 * (timepr - tim);
                while (tim < timepr)
                {
                    ++ctec; // количество попыток определить текущий шаг
                    if (ctec > 1000) // индикатор можно уменьшить, нужны эксперименты
                    {
                        throw new CE(MessageReader.GetMessage("Err:C:DivSolution"),
                            Tuple.Create<string, object>(CE.Step, n));
                    }
                    if (stopFlag)
                    {
                        throw new CE(MessageReader.GetMessage("Err:UserStop"),
                            Tuple.Create<string, object>(CE.Step, n));
                    }
                    var TopNRange = Tuple.Create(countOfNodes - computeItem.OverheadRadius - 1, countOfNodes - 1);
                    var BottomNRange = Tuple.Create(0, computeItem.BaseRadius);
                    for (int j = 1; j <= computeItem.InstCount; ++j)
                        Parallel.For(0, countOfNodes, i =>
                            {
                                nodes[j][i].NewSpeed = CalcSpeed(dtime, i, j);
                                // Граничные условия:
                                // Верхний удар
                                if (computeItem.TopImpactSpeed.HasValue)
                                {
                                    if (MathEx.InRangeInclusive(i, TopNRange))
                                    {
                                        nodes[j][i].NewSpeed.Z = -computeItem.TopImpactSpeed.Value;
                                    }
                                }
                                // Нижний удар
                                if (computeItem.BottomImpactSpeed.HasValue)
                                {
                                    if (MathEx.InRangeInclusive(i, BottomNRange))
                                    {
                                        nodes[j][i].NewSpeed.Z = computeItem.BottomImpactSpeed.Value;
                                    }
                                }
                            });
                    // Сохранение осевой симметрии и закреплений
                    for (int j = 1; j <= computeItem.InstCount; ++j)
                        Parallel.ForEach(fixationR, i =>
                            {
                                nodes[j][i].NewSpeed.R = 0.0;
                            });
                    for (int j = 1; j <= computeItem.InstCount; ++j)
                        Parallel.ForEach(fixationZ, i =>
                            {
                                nodes[j][i].NewSpeed.Z = 0.0;
                            });
                    // Вычисление смещений узлов
                    for (int j = 1; j <= computeItem.InstCount; ++j)
                        Parallel.For(0, countOfNodes, i =>
                            {
                                nodes[j][i].OldCoord.R = nodes[j][i].R;
                                nodes[j][i].R += dtime * nodes[j][i].NewSpeed.R;
                                nodes[j][i].OldCoord.Z = nodes[j][i].Z;
                                nodes[j][i].Z += dtime * nodes[j][i].NewSpeed.Z;
                            });
                    // Копирование площадей во временную переменную и вычисление новых
                    for (int j = 1; j <= computeItem.InstCount; ++j)
                        Parallel.For(0, countOfElem, i =>
                            {
                                elements[j][i].OldSquare = elements[j][i].Square;
                            });
                    CalculateSquares();
                    // Вычисление подходящего шага по времени
                    dt = double.PositiveInfinity;
                    for (int j = 1; j <= computeItem.InstCount; ++j)
                    {
                        var t = elements[j].AsParallel().Select(element =>
                        {
                            var node1 = nodes[0][element.Nodes.Item1];
                            var node2 = nodes[0][element.Nodes.Item2];
                            var node3 = nodes[0][element.Nodes.Item3];
                            double len1 = Math.Pow(node3.R - node1.R, 2.0) + Math.Pow(node3.Z - node1.Z, 2.0);
                            double len2 = Math.Pow(node3.R - node2.R, 2.0) + Math.Pow(node3.Z - node2.Z, 2.0);
                            double len3 = Math.Pow(node2.R - node1.R, 2.0) + Math.Pow(node2.Z - node1.Z, 2.0);
                            double maxlen = Math.Max(len1, Math.Max(len2, len3));
                            double minh = 2.0 * element.Square / Math.Sqrt(maxlen);
                            return minh / (3.0 * Math.Sqrt((materials[element.MaterialIndex].K +
                                4.0 / 3.0 * materials[element.MaterialIndex].G) /
                                materials[element.MaterialIndex].Ro0 + 1.0));
                        }).Min();
                        if (t < dt)
                            dt = t;
                    }
                    // если подходящий шаг по времени больше заданного
                    if (dt < dtime)
                    {
                        for (int j = 1; j <= computeItem.InstCount; ++j)
                            Parallel.For(0, countOfNodes, i =>
                                {
                                    nodes[j][i].R = nodes[j][i].OldCoord.R;
                                    nodes[j][i].Z = nodes[j][i].OldCoord.Z;
                                });
                        CalculateSquares();
                        dtime = 0.89 * dt;
                        continue; //перезапуск вычислений с другим шагом по времени
                    }
                    // шаг настолько мал, что возникнут ошибки округления
                    if (computeItem.TimeStep > dtime * 1e10)
                        throw new CE(MessageReader.GetMessage("Err:C:SmallDT"),
                            Tuple.Create<string, object>(CE.Step, n),
                            Tuple.Create<string, object>(CE.Value, dtime));
                    tim += dtime;
                    for (int j = 1; j <= computeItem.InstCount; ++j)
                        Parallel.For(0, countOfNodes, i =>
                            {
                                nodes[j][i].Speed.R = nodes[j][i].NewSpeed.R;
                                nodes[j][i].Speed.Z = nodes[j][i].NewSpeed.Z;
                            });
                    for (int j = 1; j <= computeItem.InstCount; ++j)
                        Parallel.For(0, countOfElem, i => BaseLoopBody(i, dtime, j));
                }
                if ((n % (countOfStep / 100)) == 0)
                {
                    byte percent = (byte)((n * 100) / countOfStep);
                    if ((bool)computeItem.Animation)
                    {
                        avValues();
                        SaveImage("resim" + percent.ToString() + ".png", DrawStyle.Stress);
                        SaveImage("resims" + percent.ToString() + ".png", DrawStyle.Speed);
                        SaveImage("resimt" + percent.ToString() + ".png", DrawStyle.Temperature);
                        SaveResult("val" + percent.ToString() + ".vtk");
                    }
                    if (!pcbRes.IsCompleted)
                        pcbRes.AsyncWaitHandle.WaitOne();
                    pcbRes = percentCallback.BeginInvoke(percent, null, null);
                }
            }
            avValues();
            percentCallback(100);
        }
        /// <summary>
        /// Вычисляет новое значение скорости узла. 
        /// Не имеет побочных эффектов.
        /// </summary>
        /// <param name="dtime">Изменение времени</param>
        /// <param name="nodeIndex">Номер узла</param>
        /// <param name="instIndex">Номер экземпляра решения</param>
        /// <returns>Координаты конца вектора скорости</returns>
        Point CalcSpeed(double dtime, int nodeIndex, int instIndex)
        {
            const double dvtr = 2.0 / 3.0; // = 0.(6)
            double sumr = 0.0, sumz = 0.0, rc, zc;
            foreach (var ei in nodes[instIndex][nodeIndex].Elements)
                if (ei != null)
                {
                    var elem = elements[instIndex][ei.Item1];
                    var node1 = nodes[instIndex][elem.Nodes.Item1];
                    var node2 = nodes[instIndex][elem.Nodes.Item2];
                    var node3 = nodes[instIndex][elem.Nodes.Item3];
                    //центры сторон КЭ, противоположенных узлу в элементе elem
                    switch (ei.Item2)
                    {
                        case Node.NodeNmb.Node1:
                            rc = node3.R - node2.R;
                            zc = node2.Z - node3.Z;
                            break;
                        case Node.NodeNmb.Node2:
                            rc = node1.R - node3.R;
                            zc = node3.Z - node1.Z;
                            break;
                        case Node.NodeNmb.Node3:
                            rc = node2.R - node1.R;
                            zc = node1.Z - node2.Z;
                            break;
                        default: //никогда не сработает, но компилятор в это не верит
                            throw new Exception();
                    }
                    //+силы, действующие со стороны этого элемента на узел
                    sumr += elem.Barycenter.R * (elem.Sigma.RR * zc + elem.Sigma.RZ * rc) + dvtr * elem.Square * elem.Sigma.TT;
                    sumz += elem.Barycenter.R * (elem.Sigma.ZZ * rc + elem.Sigma.RZ * zc);
                }
            //второй закон Ньютона
            return new Point()
            {
                R = nodes[instIndex][nodeIndex].Speed.R - dtime * sumr / nodes[instIndex][nodeIndex].Mass,
                Z = nodes[instIndex][nodeIndex].Speed.Z - dtime * sumz / nodes[instIndex][nodeIndex].Mass
            };
        }
        /// <summary>
        /// Собственно, вся физика. 
        /// Имеет побочные эффекты.
        /// </summary>
        /// <param name="i">Номер КЭ</param>
        /// <param name="dtime">Период времени</param>
        /// <param name="instIndex">Номер экземпляра решения</param>
        void BaseLoopBody(int i, double dtime, int instIndex)
        {
            unchecked
            {
                double G = materials[elements[instIndex][i].MaterialIndex].G;
                double K = materials[elements[instIndex][i].MaterialIndex].K;
                double alpha = materials[elements[instIndex][i].MaterialIndex].Alpha;
                if (materials[elements[instIndex][i].MaterialIndex].DynamicAlpha.HasValue)
                    alpha = materials[elements[instIndex][i].MaterialIndex].DynamicAlpha.Value.Alpha(elements[instIndex][i].T);
                double sigma0 = materials[elements[instIndex][i].MaterialIndex].Sigma0;
                double sigma1 = materials[elements[instIndex][i].MaterialIndex].Sigma1;
                double ro0 = materials[elements[instIndex][i].MaterialIndex].Ro0;
                double k1 = materials[elements[instIndex][i].MaterialIndex].K1;
                double a = elements[instIndex][i].Square;
                double ak = elements[instIndex][i].OldSquare;
                double lmb = K - 2.0 / 3.0 * G;
                double mu = G;
                double c = materials[elements[instIndex][i].MaterialIndex].C;
                double gamma = materials[elements[instIndex][i].MaterialIndex].Gamma;
                double v = 2.0 * Math.PI * a * elements[instIndex][i].Barycenter.R;
                double vk = 2.0 * Math.PI * ak * (nodes[instIndex][elements[instIndex][i].Nodes.Item1].OldCoord.R + nodes[instIndex][elements[instIndex][i].Nodes.Item2].OldCoord.R + nodes[instIndex][elements[instIndex][i].Nodes.Item3].OldCoord.R) / 3.0;
                double vdif = 2.0 * (v - vk) / ((v + vk) * dtime);
                double z1 = 0.5 * (nodes[instIndex][elements[instIndex][i].Nodes.Item1].Z + nodes[instIndex][elements[instIndex][i].Nodes.Item1].OldCoord.Z);
                double z2 = 0.5 * (nodes[instIndex][elements[instIndex][i].Nodes.Item2].Z + nodes[instIndex][elements[instIndex][i].Nodes.Item2].OldCoord.Z);
                double z3 = 0.5 * (nodes[instIndex][elements[instIndex][i].Nodes.Item3].Z + nodes[instIndex][elements[instIndex][i].Nodes.Item3].OldCoord.Z);
                double r1 = 0.5 * (nodes[instIndex][elements[instIndex][i].Nodes.Item1].R + nodes[instIndex][elements[instIndex][i].Nodes.Item1].OldCoord.R);
                double r2 = 0.5 * (nodes[instIndex][elements[instIndex][i].Nodes.Item2].R + nodes[instIndex][elements[instIndex][i].Nodes.Item2].OldCoord.R);
                double r3 = 0.5 * (nodes[instIndex][elements[instIndex][i].Nodes.Item3].R + nodes[instIndex][elements[instIndex][i].Nodes.Item3].OldCoord.R);
                double u1 = nodes[instIndex][elements[instIndex][i].Nodes.Item1].Speed.R;
                double u2 = nodes[instIndex][elements[instIndex][i].Nodes.Item2].Speed.R;
                double u3 = nodes[instIndex][elements[instIndex][i].Nodes.Item3].Speed.R;
                double v1 = nodes[instIndex][elements[instIndex][i].Nodes.Item1].Speed.Z;
                double v2 = nodes[instIndex][elements[instIndex][i].Nodes.Item2].Speed.Z;
                double v3 = nodes[instIndex][elements[instIndex][i].Nodes.Item3].Speed.Z;

                // КОМПОНЕНТЫ ТЕНЗОРА СКОРОСТЕЙ ДЕФОРМАЦИЙ
                double epsdotrr = ((z2 - z3) * u1 + (z3 - z1) * u2 + (z1 - z2) * u3) / (a + ak);
                double epsdotrz = ((r3 - r2) * u1 + (r1 - r3) * u2 + (r2 - r1) * u3 + (z2 - z3) * v1 +
                    (z3 - z1) * v2 + (z1 - z2) * v3) / (2.0 * (a + ak));
                double epsdotzz = ((r3 - r2) * v1 + (r1 - r3) * v2 + (r2 - r1) * v3) / (a + ak);
                double epsdottt = -epsdotrr - epsdotzz + vdif;

                // КОМПОНЕНТЫ ТЕНЗОРА ПОЛНЫХ ДЕФОРМАЦИЙ
                double depsrr;
                double depsrz;
                double depszz;
                double depstt;
                elements[instIndex][i].Eps.RR += depsrr = epsdotrr * dtime;
                elements[instIndex][i].Eps.RZ += depsrz = epsdotrz * dtime;
                elements[instIndex][i].Eps.ZZ += depszz = epsdotzz * dtime;
                elements[instIndex][i].Eps.TT += depstt = epsdottt * dtime;

                // КОМПОНЕНТЫ ДЕВИАТОРА ТЕНЗОРА НАПРЯЖЕНИЙ
                elements[instIndex][i].S.RR += 2.0 * G * dtime * (epsdotrr - vdif / 3.0);
                elements[instIndex][i].S.ZZ += 2.0 * G * dtime * (epsdotzz - vdif / 3.0);
                elements[instIndex][i].S.RZ += G * dtime * epsdotrz;
                elements[instIndex][i].S.TT = -elements[instIndex][i].S.RR - elements[instIndex][i].S.ZZ;

                // КОМПОНЕНТЫ ДЕВИАТОРА ТЕНЗОРА НАПРЯЖЕНИЙ С УЧЁТОМ ПОВОРОТА
                double sin2omega = dtime / (a + ak) * ((z2 - z3) * v1 + (z3 - z1) * v2 + (z1 - z2) * v3 -
                    (r3 - r2) * u1 - (r1 - r3) * u2 - (r2 - r1) * u3);
                double cos2omega = Math.Sqrt(Math.Pow(1 - sin2omega, 2));
                double srrt = 0.5 * (elements[instIndex][i].S.RR + elements[instIndex][i].S.ZZ) + 0.5 * (elements[instIndex][i].S.RR - elements[instIndex][i].S.ZZ) * cos2omega - elements[instIndex][i].S.RZ * sin2omega;
                double szzt = 0.5 * (elements[instIndex][i].S.RR + elements[instIndex][i].S.ZZ) - 0.5 * (elements[instIndex][i].S.RR - elements[instIndex][i].S.ZZ) * cos2omega + elements[instIndex][i].S.RZ * sin2omega;
                double srzt = elements[instIndex][i].S.RZ * cos2omega + 0.5 * (elements[instIndex][i].S.RR - elements[instIndex][i].S.ZZ) * sin2omega;
                elements[instIndex][i].S.RR = srrt;
                elements[instIndex][i].S.ZZ = szzt;
                elements[instIndex][i].S.RZ = srzt;

                // ФУНКЦИЯ УПРОЧНЕНИЯ С УЧЁТОМ ПАДАЮЩЕЙ ДИАГРАММЫ
                if (elements[instIndex][i].F == 0.0)
                    elements[instIndex][i].F = 0.000001;
                double Heps = 2.0 * G;
                if (sigma1 < alf * elements[instIndex][i].P + sigma0 - alpha * elements[instIndex][i].I2p)
                {
                    Heps = 2.0 * G + K * Math.Pow(k1, 2) - Math.Pow(k1, 2.0) / K - alpha / elements[instIndex][i].F *
                        (elements[instIndex][i].S.RR * (elements[instIndex][i].EpsP.RR - elements[instIndex][i].EpsP.TT) + elements[instIndex][i].S.ZZ *
                        (elements[instIndex][i].EpsP.ZZ - elements[instIndex][i].EpsP.TT) + 2.0 * elements[instIndex][i].S.RZ * elements[instIndex][i].EpsP.RZ);
                }

                // ФУНКЦИЯ ДЕФОРМИРОВАНИЯ
                elements[instIndex][i].Psi = Math.Sqrt(2.0 * (Math.Pow(elements[instIndex][i].S.RR, 2.0) + Math.Pow(elements[instIndex][i].S.ZZ, 2.0) + Math.Pow(elements[instIndex][i].S.RZ, 2.0) + elements[instIndex][i].S.RR * elements[instIndex][i].S.ZZ)) - elements[instIndex][i].F;

                // ПРОИЗВОДНАЯ ФУНКЦИИ ДЕФОРМИРОВАНИЯ ПО ВРЕМЕНИ
                double psidot = 2.0 * G / elements[instIndex][i].F * (elements[instIndex][i].S.RR * (epsdotrr - epsdottt) + elements[instIndex][i].S.ZZ * (epsdotzz - epsdottt) +
                    2.0 * elements[instIndex][i].S.RZ * epsdotrz) - K * alf * k1 * (epsdotrr + epsdotzz + epsdottt);
                if (materials[elements[instIndex][i].MaterialIndex].DynamicAlpha.HasValue)
                    psidot += materials[elements[instIndex][i].MaterialIndex].DynamicAlpha.Value.DAlpha(elements[instIndex][i].T);
                
                // ПЕРЕКЛЮЧАТЕЛЬ УПРУГОСТЬ/ПЛАСТИЧНОСТЬ
                int H = ((elements[instIndex][i].Psi >= 0.0) && (psidot > 0.0)) ? 1 : 0; //шаманство с приведением типов ненадёжное

                // КОМПОНЕНТЫ ТЕНЗОРА СКОРОСТЕЙ ПЛАСТИЧЕСКИХ ДЕФОРМАЦИЙ
                double epsdotrrp = H * psidot / Heps * (elements[instIndex][i].S.RR / elements[instIndex][i].F - alf * (3.0 * K - k1 * G) / (3.0 * G));
                double epsdotzzp = H * psidot / Heps * (elements[instIndex][i].S.ZZ / elements[instIndex][i].F - alf * (3.0 * K - k1 * G) / (3.0 * G));
                double epsdotttp = H * psidot / Heps * (elements[instIndex][i].S.TT / elements[instIndex][i].F - alf * (3.0 * K - k1 * G) / (3.0 * G));
                double epsdotrzp = H * psidot / Heps * (elements[instIndex][i].S.RZ / elements[instIndex][i].F);

                elements[instIndex][i].EpsP.RR += epsdotrrp * dtime;
                elements[instIndex][i].EpsP.ZZ += epsdotzzp * dtime;
                elements[instIndex][i].EpsP.RZ += epsdotrzp * dtime;
                elements[instIndex][i].EpsP.TT += epsdotttp * dtime;

                // ПОЛНАЯ ОБЪЁМНАЯ ДЕФОРМАЦИЯ
                elements[instIndex][i].Theta = elements[instIndex][i].Eps.RR + elements[instIndex][i].Eps.ZZ + elements[instIndex][i].Eps.TT;

                // ПЛАСТИЧЕСКАЯ ОБЪЁМНАЯ ДЕФОРМАЦИЯ
                elements[instIndex][i].ThetaP += -H * psidot * k1 * dtime / Heps;
                if (alf == 0.0)
                    elements[instIndex][i].ThetaP = 0.0;

                // УПРУГАЯ ОБЪЁМНАЯ ДЕФОРМАЦИЯ
                double tete = elements[instIndex][i].Theta - elements[instIndex][i].ThetaP;

                // УЧЁТ ПСЕВДОВЯЗКОСТИ
                double speedsound = Math.Sqrt((K + 4.0 / 3.0 * G) / ro0);
                double q = 0;
                if (vdif < 0.0)
                {
                    q = Math.Pow(c0, 2) * ro0 * (a + ak) / (v + vk) * Math.Pow(vdif, 2) + cl * speedsound *
                        ro0 * Math.Sqrt(0.5 * (a + ak)) * Math.Abs(vdif) / (0.5 * (v + vk));
                }
                double q1 = ca * speedsound * ro0 * (a + ak) / (v + vk);

                // ГИДРОСТАТИЧЕСКОЕ ДАВЛЕНИЕ
                elements[instIndex][i].P = -K * tete;

                // КОМПОНЕНТЫ ТЕНЗОРА НАПРЯЖЕНИЙ С УЧЁТОМ ПСЕВДОВЯЗКОСТИ
                elements[instIndex][i].Sigma.RR = -(elements[instIndex][i].P + q) + (elements[instIndex][i].S.RR + 2.0 * q1 * (epsdotrr - vdif / 3.0));
                elements[instIndex][i].Sigma.ZZ = -(elements[instIndex][i].P + q) + (elements[instIndex][i].S.ZZ + 2.0 * q1 * (epsdotzz - vdif / 3.0));
                elements[instIndex][i].Sigma.TT = -elements[instIndex][i].Sigma.RR - elements[instIndex][i].Sigma.ZZ - 3.0 * (elements[instIndex][i].P + q);
                elements[instIndex][i].Sigma.RZ = elements[instIndex][i].S.RZ + q1 * epsdotrz;

                // ИНТЕНСИВНОСТЬ ПЛАСТИЧЕСКИХ ДЕФОРМАЦИЙ
                elements[instIndex][i].I2p = 0.5 * (Math.Pow(elements[instIndex][i].EpsP.RR - elements[instIndex][i].ThetaP / 3.0, 2) + Math.Pow(elements[instIndex][i].EpsP.ZZ - elements[instIndex][i].ThetaP / 3.0, 2) + 2 * Math.Pow(elements[instIndex][i].EpsP.RZ, 2));
                if ((elements[instIndex][i].SqI2p = Math.Sqrt(elements[instIndex][i].I2p)) > 1.0)
                    throw new CE(MessageReader.GetMessage("Err:C:BigI2p"),
                            Tuple.Create<string, object>(CE.Params, Tuple.Create(i, dtime, instIndex)),
                            Tuple.Create<string, object>(CE.Value, elements[instIndex][i].SqI2p),
                            Tuple.Create<string, object>(CE.Step, n));

                // ЭНЕРГИЯ
                elements[instIndex][i].Energy += elements[instIndex][i].Barycenter.R * dtime * (elements[instIndex][i].Sigma.RR * ((z1 - z2) * u3 + (z2 - z3) * u1 + (z3 - z1) * u2) +
                    elements[instIndex][i].Sigma.ZZ * ((r2 - r1) * v3 + (r1 - r3) * v2 + (r3 - r2) * v1) + elements[instIndex][i].Sigma.RZ * ((r2 - r1) * u3 + (r1 - r3) * u2 +
                    (r3 - r2) * u1 + (z1 - z2) * v3 + (z2 - z3) * v1 + (z3 - z1) * v2) +
                    elements[instIndex][i].Sigma.TT * elements[instIndex][i].Square * (u1 + u2 + u3) / (3.0 * elements[instIndex][i].Barycenter.R)) / elements[instIndex][i].Mass;

                // ПЕРЕМЕННЫЙ ПРЕДЕЛ ТЕКУЧЕСТИ (ФУНКЦИЯ F) С УЧЁТОМ ПАДАЮЩЕЙ ДИАГРАММЫ
                double F = k1 * alf * elements[instIndex][i].P + sigma0 - alpha * elements[instIndex][i].I2p;
                if (alpha > 0.0 && F < sigma1)
                {
                    elements[instIndex][i].Crashed = true;
                    F = sigma1;
                }
                if (alpha < 0.0 && sigma1 > sigma0 + 1 && F > sigma1)
                {
                    elements[instIndex][i].Crashed = true;
                    F = sigma1;
                }
                elements[instIndex][i].F = F;

                double Y2 = Math.Pow(elements[instIndex][i].S.RR, 2.0) + Math.Pow(elements[instIndex][i].S.ZZ, 2.0) + Math.Pow(elements[instIndex][i].S.RZ, 2.0) + elements[instIndex][i].S.RR * elements[instIndex][i].S.ZZ;

                // КОМПОНЕНТЫ ДЕВИАТОРА ТЕНЗОРА НАПРЯЖЕНИЙ С УЧЁТОМ ПЛАСТИЧНОСТИ
                if (elements[instIndex][i].Psi >= 0.0)
                {
                    elements[instIndex][i].S.RR = elements[instIndex][i].S.RR * F / Math.Sqrt(2.0 * Y2);
                    elements[instIndex][i].S.ZZ = elements[instIndex][i].S.ZZ * F / Math.Sqrt(2.0 * Y2);
                    elements[instIndex][i].S.RZ = elements[instIndex][i].S.RZ * F / Math.Sqrt(2.0 * Y2);
                }

                // Корень из второго инварианта тензора напряжений с учётом пластичности.
                // Используется для вывода на картинки.
                elements[instIndex][i].sqY2 = Math.Sqrt(Math.Pow(elements[instIndex][i].S.RR, 2.0) + Math.Pow(elements[instIndex][i].S.ZZ, 2.0) + Math.Pow(elements[instIndex][i].S.RZ, 2.0) + elements[instIndex][i].S.RR * elements[instIndex][i].S.ZZ);

                // ТЕМПЕРАТУРА
                // Работает правильно только для металлов!
                // Если произошло разрушение, температура фиксируется. Возможно, так делать не надо.
                if (!elements[instIndex][i].Crashed)
                {
                    double Told = elements[instIndex][i].T;
                    double eee = depsrr + depszz + depstt;
                    double Tplast = (2.0 * computeItem.T0 * G * H * (elements[instIndex][i].S.RR * depsrr + elements[instIndex][i].S.ZZ * depszz + elements[instIndex][i].S.RZ * depsrz + elements[instIndex][i].S.TT * depstt)) /
                        (ro0 * c * Told * Heps);
                    double Telast = -(computeItem.T0 * (3.0 * lmb + 2.0 * mu) * gamma * (eee)) / (c * ro0);
                    elements[instIndex][i].T = (Tplast + Telast) + Told;
                    elements[instIndex][i].dTdt = (Tplast + Telast)/dtime;
                }
            }
        }

        double avTInNode(int node, int instIndex)
        {
            int diver = 0;
            double nt = 0;
            foreach (var ei in nodes[instIndex][node].Elements)
                if (ei != null)
                {
                    ++diver;
                    nt += elements[instIndex][ei.Item1].T;
                }
            return nt / diver;
        }
        /// <summary>
        /// Вычисляет средние значения (для вывода). 
        /// Предусловие: завершение <see cref="Initialize"/>. 
        /// Побочные эффекты: изменение elements[0] и nodes[0].
        /// </summary>
        void avValues()
        {
            //Неоптимально, зато как красиво!
            var range = Enumerable.Range(1, computeItem.InstCount).ToList();
            Parallel.For(0, countOfElem, i =>
                {
                    //elements[0][i].Barycenter.R = range.Select(j => elements[j][i].Barycenter.R).Sum() / computeItem.InstCount;
                    //elements[0][i].Barycenter.Z = range.Select(j => elements[j][i].Barycenter.Z).Sum() / computeItem.InstCount;
                    elements[0][i].Crashed = range.Select(j => (elements[j][i].Crashed) ? 1.0 : 0.0).Sum() / computeItem.InstCount > 0.5;
                    elements[0][i].Energy = range.Select(j => elements[j][i].Energy).Sum() / computeItem.InstCount;
                    elements[0][i].F = range.Select(j => elements[j][i].F).Sum() / computeItem.InstCount;
                    elements[0][i].I2p = range.Select(j => elements[j][i].I2p).Sum() / computeItem.InstCount;
                    elements[0][i].P = range.Select(j => elements[j][i].P).Sum() / computeItem.InstCount;
                    elements[0][i].Psi = range.Select(j => elements[j][i].Psi).Sum() / computeItem.InstCount;
                    elements[0][i].SqI2p = Math.Sqrt(elements[0][i].I2p);
                    elements[0][i].sqY2 = range.Select(j => elements[j][i].sqY2).Sum() / computeItem.InstCount;
                    elements[0][i].T = range.Select(j => elements[j][i].T).Sum() / computeItem.InstCount;
                    elements[0][i].Theta = range.Select(j => elements[j][i].Theta).Sum() / computeItem.InstCount;
                    elements[0][i].ThetaP = range.Select(j => elements[j][i].ThetaP).Sum() / computeItem.InstCount;
                    //elements[0][i].Eps = range.Select(j => elements[j][i].Eps).Aggregate((l, r) => l + r) * (1.0 / computeItem.InstCount);
                    //elements[0][i].EpsP = range.Select(j => elements[j][i].Eps).Aggregate((l, r) => l + r) * (1.0 / computeItem.InstCount);
                    //elements[0][i].Sigma = range.Select(j => elements[j][i].Eps).Aggregate((l, r) => l + r) * (1.0 / computeItem.InstCount);
                    //elements[0][i].S = range.Select(j => elements[j][i].Eps).Aggregate((l, r) => l + r) * (1.0 / computeItem.InstCount);
                });
            Parallel.For(0, countOfNodes, i =>
                {
                    nodes[0][i].R = range.Select(j => nodes[j][i].R).Sum() / computeItem.InstCount;
                    nodes[0][i].Z = range.Select(j => nodes[j][i].Z).Sum() / computeItem.InstCount;
                    nodes[0][i].Speed = range.Select(j => nodes[j][i].Speed).Aggregate((l, r) => l + r) * (1.0 / computeItem.InstCount);
                    //nodes[0][i].NewSpeed.R = range.Select(j => nodes[j][i].NewSpeed.R).Sum() / computeItem.InstCount;
                    //nodes[0][i].NewSpeed.Z = range.Select(j => nodes[j][i].NewSpeed.Z).Sum() / computeItem.InstCount;
                });
        }
    }
}
