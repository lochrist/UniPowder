using Unity.Entities;
using UnityEngine;

class PowderType
{
    public Color color;
    public int id;
    public string name;
}

static class PowderTypes
{
    public const int Void = 0;
    public const int Sand = 1;
    public const int Fire = 2;
    public const int Water = 3;
    public const int Stone = 4;
    public const int Smoke = 5;
    public const int Steam = 6;
    public const int Acid = 7;
    public const int Glass = 8;
    public const int Wood = 9;

    public static PowderType[] values;

    public static void Init(EntityManager mgr)
    {
        values = new []
        {
            new PowderType {
                color = Utils.ToColor("#eeee10"), id = Sand, name = "Sand"
            },
            new PowderType
            {
                color = Utils.ToColor("#bf9c1d"), id = Wood, name = "Wood"
            },
            new PowderType
            {
                color = Utils.ToColor("#ff0000"), id = Fire, name = "Fire"
            },
            new PowderType
            {
                color = Utils.ToColor("#0000ff"), id = Water, name = "Water"
            },
            new PowderType
            {
                color = Utils.ToColor("#7f7f7f"), id = Stone, name = "Stone" 
            },
            new PowderType
            {
                color = Utils.ToColor("#878787"), id = Smoke, name = "Smoke"
            },
            new PowderType
            {
                color = Utils.ToColor("#e3e3e3"), id = Steam, name = "Steam"
            },
            new PowderType
            {
                color = Utils.ToColor("#ff33ee"), id = Acid, name = "Acid"
            },
            new PowderType
            {
                color = Utils.ToColor("#404040"), id = Glass, name = "Glass"
            }
        };
    }

    public static void Spawn(EntityCommandBuffer buffer, Vector2Int pos, int type)
    {
        buffer.CreateEntity();
        buffer.AddComponent(new Powder() { coord = pos, type = type, life = 50 });
    }
}

