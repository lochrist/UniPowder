using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class PowderType
{
    public Color color;
    public int id;
    public string name;
}

static class PowderTypes
{
    public static int Sand = 0;
    public static int Wood = 1;
    public static int Fire = 2;
    public static int Water = 3;
    public static int Stone = 4;
    public static int Smoke = 5;
    public static int Steam = 6;
    public static int Acid = 7;
    public static int Glass = 8;

    public static PowderType[] values;

    static PowderTypes()
    {
        values = new []
        {
            new PowderType { color = Utils.ToColor("#eeee10"), id = Sand, name = "Sand" },
            new PowderType { color = Utils.ToColor("#bf9c1d"), id = Wood, name = "Wood" },
            new PowderType { color = Utils.ToColor("#ff0000"), id = Fire, name = "Fire" },
            new PowderType { color = Utils.ToColor("#0000ff"), id = Water, name = "Water" },
            new PowderType { color = Utils.ToColor("#7f7f7f"), id = Stone, name = "Stone" },
            new PowderType { color = Utils.ToColor("#878787"), id = Smoke, name = "Smoke" },
            new PowderType { color = Utils.ToColor("#e3e3e3"), id = Steam, name = "Steam" },
            new PowderType { color = Utils.ToColor("#ff33ee"), id = Acid, name = "Acid" },
            new PowderType { color = Utils.ToColor("#404040"), id = Glass, name = "Glass" }
        };
    }
}

