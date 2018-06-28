using System;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

public enum PowderState
{
    Solid,
    Liquid,
    Gas,
    Powder
}

public class PowderType
{
    public Color color;
    public int id;
    public string name;
    public string description;
    public PowderState state;
    public Func<Vector2Int, Powder> creator;
    public MeshInstanceRenderer renderer;
    public int generatedElementType;

    public bool IsGenerator()
    {
        return generatedElementType != 0;
    }
}

public static class PowderTypes
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
    public const int Lava = 10;

    /*
     liquid:
     lava
     water
     acid

    solid:
    stone
    glass
    wood

    powder:
    sand

    Gas
    fire
    steam
    smoke

    Generator

     */

    public const int SandGenerator = 11;
    public const int FireGenerator = 12;
    public const int WaterGenerator = 13;
    public const int SmokeGenerator = 14;
    public const int SteamGenerator = 15;
    public const int AcidGenerator = 16;
    public const int LavaGenerator = 17;

    public const int MaxType = 18;

    public static PowderType[] values;

    public static void Init(EntityManager mgr)
    {
        var rand = Rand.Create();
        values = new PowderType[MaxType];
        values[Void] = new PowderType()
        {
            id = Void,
        };
        values[Sand] = new PowderType
        {
            color = Utils.ToColor("#eeee10"),
            id = Sand,
            state = PowderState.Powder,
            name = "Sand",
            creator = (coord) => new Powder { coord = coord, life = -1, type = Sand },
            renderer = GetRendererPrototype("Sand")
        };
        values[Wood] = new PowderType
        {
            color = Utils.ToColor("#bf9c1d"),
            id = Wood,
            state = PowderState.Solid,
            name = "Wood",
            creator = (coord) => new Powder { coord = coord, life = -1, type = Wood },
            renderer = GetRendererPrototype("Wood")
        };
        values[Fire] = new PowderType
        {
            color = Utils.ToColor("#ff0000"),
            id = Fire,
            state = PowderState.Gas,
            name = "Fire",
            creator = (coord) => new Powder { coord = coord, life = 200 + rand.Range(0, 75), type = Fire },
            renderer = GetRendererPrototype("Fire")
        };
        values[Water] = new PowderType
        {
            color = Utils.ToColor("#0000ff"),
            id = Water,
            state = PowderState.Liquid,
            name = "Water",
            creator = (coord) => new Powder { coord = coord, life = -1, type = Water },
            renderer = GetRendererPrototype("Water")
        };
        values[Stone] = new PowderType
        {
            color = Utils.ToColor("#7f7f7f"),
            id = Water,
            state = PowderState.Solid,
            name = "Stone",
            creator = (coord) => new Powder { coord = coord, life = -1, type = Stone },
            renderer = GetRendererPrototype("Stone"),
        };
        values[Smoke] = new PowderType
        {
            color = Utils.ToColor("#878787"),
            id = Smoke,
            state = PowderState.Gas,
            name = "Smoke",
            creator = (coord) => new Powder { coord = coord, life = 150 + rand.Range(0, 150), type = Smoke },
            renderer = GetRendererPrototype("Smoke")
        };
        values[Steam] = new PowderType
        {
            color = Utils.ToColor("#e3e3e3"),
            id = Steam,
            state = PowderState.Gas,
            name = "Steam",
            creator = (coord) => new Powder { coord = coord, life = 200 + rand.Range(0, 200), type = Steam },
            renderer = GetRendererPrototype("Steam")
        };
        values[Acid] = new PowderType
        {
            color = Utils.ToColor("#ff33ee"),
            id = Acid,
            state = PowderState.Liquid,
            name = "Acid",
            creator = (coord) => new Powder { coord = coord, life = -1, type = Acid },
            renderer = GetRendererPrototype("Acid")
        };
        values[Glass] = new PowderType
        {
            color = Utils.ToColor("#404040"),
            id = Acid,
            state = PowderState.Solid,
            name = "Glass",
            creator = (coord) => new Powder { coord = coord, life = -1, type = Glass },
            renderer = GetRendererPrototype("Glass")
        };
        values[Lava] = new PowderType
        {
            color = Utils.ToColor("#FF0000"),
            id = Lava,
            state = PowderState.Liquid,
            name = "Lava",
            creator = (coord) => new Powder { coord = coord, life = -1, type = Lava },
            renderer = GetRendererPrototype("Lava")
        };
        values[Lava] = new PowderType
        {
            color = Utils.ToColor("#FF0000"),
            id = Lava,
            state = PowderState.Liquid,
            name = "Lava",
            creator = (coord) => new Powder { coord = coord, life = -1, type = Lava },
            renderer = GetRendererPrototype("Lava")
        };
        values[SandGenerator] = new PowderType
        {
            color = Utils.ToColor("#777777"),
            id = SandGenerator,
            state = PowderState.Solid,
            name = "Sand Gen",
            creator = (coord) => new Powder { coord = coord, life = -1, type = SandGenerator },
            renderer = GetRendererPrototype("Generator"),
            generatedElementType = Sand
        };
        values[FireGenerator] = new PowderType
        {
            color = Utils.ToColor("#777777"),
            id = FireGenerator,
            state = PowderState.Solid,
            name = "Fire Gen",
            creator = (coord) => new Powder { coord = coord, life = -1, type = FireGenerator },
            renderer = GetRendererPrototype("Generator"),
            generatedElementType = Fire
        };
        values[WaterGenerator] = new PowderType
        {
            color = Utils.ToColor("#777777"),
            id = WaterGenerator,
            state = PowderState.Solid,
            name = "Water Gen",
            creator = (coord) => new Powder { coord = coord, life = -1, type = WaterGenerator },
            renderer = GetRendererPrototype("Generator"),
            generatedElementType = Water
        };
        values[SmokeGenerator] = new PowderType
        {
            color = Utils.ToColor("#777777"),
            id = SmokeGenerator,
            state = PowderState.Solid,
            name = "Smoke Gen",
            creator = (coord) => new Powder { coord = coord, life = -1, type = SmokeGenerator },
            renderer = GetRendererPrototype("Generator"),
            generatedElementType = Smoke
        };
        values[SteamGenerator] = new PowderType
        {
            color = Utils.ToColor("#777777"),
            id = SteamGenerator,
            state = PowderState.Solid,
            name = "Steam Gen",
            creator = (coord) => new Powder { coord = coord, life = -1, type = SteamGenerator },
            renderer = GetRendererPrototype("Generator"),
            generatedElementType = Steam
        };
        values[AcidGenerator] = new PowderType
        {
            color = Utils.ToColor("#777777"),
            id = AcidGenerator,
            state = PowderState.Solid,
            name = "Acid Gen",
            creator = (coord) => new Powder { coord = coord, life = -1, type = AcidGenerator },
            renderer = GetRendererPrototype("Generator"),
            generatedElementType = Acid
        };
        values[LavaGenerator] = new PowderType
        {
            color = Utils.ToColor("#777777"),
            id = LavaGenerator,
            state = PowderState.Solid,
            name = "Lava Gen",
            creator = (coord) => new Powder { coord = coord, life = -1, type = LavaGenerator },
            renderer = GetRendererPrototype("Generator"),
            generatedElementType = Lava
        };
    }

    public static int FindGeneratorType(int type)
    {
        for (var i = 0; i < values.Length; ++i)
        {
            if (values[i].generatedElementType == type)
                return i;
        }
        return -1;
    }

    private static MeshInstanceRenderer GetRendererPrototype(string protoName)
    {
        var proto = GameObject.Find(protoName);
        if (proto == null)
        {
            throw new Exception("Cannot find " + protoName);
        }
        var result = proto.GetComponent<MeshInstanceRendererComponent>().Value;
        UnityEngine.Object.Destroy(proto);
        return result;
    }
}

