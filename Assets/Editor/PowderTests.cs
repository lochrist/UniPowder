using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;

public class NewTestScript {

    [Test]
    public void PowderComparison()
    {
        var p = new Powder() { coord = new Vector2Int(45, 78), life = 34, type = PowderTypes.Sand };
        var p2 = new Powder() { coord = new Vector2Int(45, 78), life = 34, type = PowderTypes.Sand }; ;
        Assert.IsTrue(p.Same(p2));

        p2.type = PowderTypes.Acid;
        Assert.IsFalse(p.Same(p2));

        p2 = p;
        Assert.IsTrue(p.Same(p2));
    }
}
