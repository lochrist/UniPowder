using System;
using Unity.Entities;
using UnityEngine;

enum PowderState
{
    Solid,
    Liquid,
    Gas,
    Powder
}

class PowderType
{
    public Color color;
    public int id;
    public string name;
    public string description;
    public PowderState state;
    public Func<Vector2Int, Powder> creator;
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
    public const int MaxType = 10;

    public static PowderType[] values;

    public static void Init(EntityManager mgr)
    {
        values = new PowderType[MaxType];
        values[Sand] = new PowderType
        {
            color = Utils.ToColor("#eeee10"),
            id = Sand,
            state = PowderState.Powder,
            name = "Sand",
            creator = (coord) => new Powder { coord = coord, life = -1, type = Sand }
        };
        values[Wood] = new PowderType
        {
            color = Utils.ToColor("#bf9c1d"),
            id = Wood,
            state = PowderState.Solid,
            name = "Wood",
            creator = (coord) => new Powder { coord = coord, life = -1, type = Wood }
        };
        values[Fire] = new PowderType
        {
            color = Utils.ToColor("#ff0000"),
            id = Fire,
            state = PowderState.Gas,
            name = "Fire",
            creator = (coord) => new Powder { coord = coord, life = 50, type = Fire }
        };
        values[Water] = new PowderType
        {
            color = Utils.ToColor("#0000ff"),
            id = Water,
            state = PowderState.Liquid,
            name = "Water",
            creator = (coord) => new Powder { coord = coord, life = -1, type = Water }
        };
        values[Stone] = new PowderType
        {
            color = Utils.ToColor("#7f7f7f"),
            id = Water,
            state = PowderState.Solid,
            name = "Stone",
            creator = (coord) => new Powder { coord = coord, life = -1, type = Stone }
        };
        values[Smoke] = new PowderType
        {
            color = Utils.ToColor("#878787"),
            id = Smoke,
            state = PowderState.Gas,
            name = "Smoke",
            creator = (coord) => new Powder { coord = coord, life = -1, type = Smoke }
        };
        values[Steam] = new PowderType
        {
            color = Utils.ToColor("#e3e3e3"),
            id = Steam,
            state = PowderState.Gas,
            name = "Steam",
            creator = (coord) => new Powder { coord = coord, life = -1, type = Steam }
        };
        values[Acid] = new PowderType
        {
            color = Utils.ToColor("#ff33ee"),
            id = Acid,
            state = PowderState.Liquid,
            name = "Acid",
            creator = (coord) => new Powder { coord = coord, life = -1, type = Acid }
        };
        values[Glass] = new PowderType
        {
            color = Utils.ToColor("#404040"),
            id = Acid,
            state = PowderState.Solid,
            name = "Glass",
            creator = (coord) => new Powder { coord = coord, life = -1, type = Glass }
        };
    }
}

