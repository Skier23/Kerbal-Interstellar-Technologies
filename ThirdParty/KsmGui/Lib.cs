using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ThirdParty.KsmGui
{
    class Lib
    {
        ///<summary>standardized kerbalism string colors</summary>
        public enum Kolor
        {
            None,
            Green,
            Yellow,
            Orange,
            Red,
            PosRate,
            NegRate,
            Science,
            Cyan,
            LightGrey,
            DarkGrey
        }


        ///<summary>return the unity Colot  for kerbalism Kolors</summary>
        public static Color KolorToColor(Kolor color)
        {
            switch (color)
            {
                case Kolor.None: return new Color(1.000f, 1.000f, 1.000f);
                case Kolor.Green: return new Color(0.533f, 1.000f, 0.000f);
                case Kolor.Yellow: return new Color(1.000f, 0.824f, 0.000f);
                case Kolor.Orange: return new Color(1.000f, 0.502f, 0.000f);
                case Kolor.Red: return new Color(1.000f, 0.200f, 0.200f);
                case Kolor.PosRate: return new Color(0.533f, 1.000f, 0.000f);
                case Kolor.NegRate: return new Color(1.000f, 0.502f, 0.000f);
                case Kolor.Science: return new Color(0.427f, 0.812f, 0.965f);
                case Kolor.Cyan: return new Color(0.000f, 1.000f, 1.000f);
                case Kolor.LightGrey: return new Color(0.800f, 0.800f, 0.800f);
                case Kolor.DarkGrey: return new Color(0.600f, 0.600f, 0.600f);
                default: return new Color(1.000f, 1.000f, 1.000f);
            }
        }

    }
}
