using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct Powder : IComponentData
{
    public int index;
    public Vector2Int coord;

    public int type;
    public int life;
}