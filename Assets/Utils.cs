using System;
using UnityEngine;

public static class Utils
{
    public static Color ToColor(int r, int g, int b, int a = 255)
    {
        return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
    }

    public static Color ToColor(string color)
    {
        if ((color.StartsWith("#")) && (color.Length == 7))
        {
            var r = Int32.Parse(color.Substring(1, 2), System.Globalization.NumberStyles.HexNumber);
            var g = Int32.Parse(color.Substring(3, 2), System.Globalization.NumberStyles.HexNumber);
            var b = Int32.Parse(color.Substring(5, 2), System.Globalization.NumberStyles.HexNumber);
            return ToColor(r, g, b);
        }

        return Color.black;
    }
}
