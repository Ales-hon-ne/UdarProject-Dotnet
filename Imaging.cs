using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace UdarProject
{
    static class Imaging
    {
        public struct ValueColor
        {
            private static Dictionary<Color, ValueColor> colInst;
            static ValueColor()
            {
                colInst = new Dictionary<Color, ValueColor>();
            }
            
            List<Tuple<double, Color>> colors;
            public Color GetColor(double value)
            {
                if (colors == null || colors.Count == 0)
                    return Color.Black;
                if (value < colors[0].Item1)
                    return colors[0].Item2;
                if (value > colors[colors.Count - 1].Item1)
                    return colors[colors.Count - 1].Item2;
                Tuple<double, Color> l = null, r = null;
                for (int i = 0; i < colors.Count - 1; )
                {
                    l = colors[i];
                    r = colors[++i];
                    if (MathEx.InRangeInclusive(value, l.Item1, r.Item1))
                        break;
                }
                double p = (value - l.Item1) / (r.Item1 - l.Item1);
                int R = l.Item2.R + (int)(p * (r.Item2.R - l.Item2.R));
                int G = l.Item2.G + (int)(p * (r.Item2.G - l.Item2.G));
                int B = l.Item2.B + (int)(p * (r.Item2.B - l.Item2.B));
                return Color.FromArgb(R, G, B);
            }
            public Color GetColor(bool value)
            {
                return GetColor((value) ? 1.0 : -1.0);
            }

            public static ValueColor Crashed(Color normal)
            {
                if (!colInst.ContainsKey(normal))
                    colInst.Add(normal, new ValueColor()
                        {
                            colors = new List<Tuple<double, Color>>() 
                            { 
                                Tuple.Create(0.0, normal),
                                Tuple.Create(0.0, Color.Tomato) 
                            }
                        });
                return colInst[normal];
            }
            public static readonly ValueColor Temperature = new ValueColor()
            {
                colors = new List<Tuple<double, Color>>() 
                    { 
                        Tuple.Create(0.0, Color.Black),
                        Tuple.Create(273.15, Color.DarkGray),
                        Tuple.Create(373.15, Color.DarkRed),
                        Tuple.Create(473.15, Color.Red),
                        Tuple.Create(573.15, Color.Yellow),
                        Tuple.Create(673.15, Color.LightYellow),
                        Tuple.Create(773.15, Color.White)
                    }
            };
            public static readonly ValueColor Speed = new ValueColor()
            {
                colors = new List<Tuple<double, Color>>() 
                    { 
                        Tuple.Create(0.0, Color.LightGray),
                        //Tuple.Create(50.0, Color.Blue),
                        Tuple.Create(100.0, Color.Yellow),
                        Tuple.Create(250.0, Color.Red),
                        Tuple.Create(500.0, Color.Violet)
                    }
            };
            public static readonly ValueColor Stress = new ValueColor()
            {
                colors = new List<Tuple<double, Color>>() 
                    { 
                        Tuple.Create(0.0, Color.Blue),
                        Tuple.Create(1e5, Color.LightBlue),
                        Tuple.Create(1e6, Color.Aqua),
                        Tuple.Create(1e7, Color.Green),
                        Tuple.Create(5e7, Color.YellowGreen),
                        Tuple.Create(1e8, Color.Yellow),
                        Tuple.Create(5e8, Color.Orange),
                        Tuple.Create(1e9, Color.Red),
                        Tuple.Create(5e9, Color.Violet),
                    }
            };
        }
    }
}
