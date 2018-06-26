using System.Collections.Generic;
using UnityEngine;
using GraphicDNA;

public class PowderRenderer : MonoBehaviour
{
    public Texture2D fontImage;
    public TextAsset fontConfig;
    public BitmapFont bitmapFont;
    public Rect parentRect;

    public static List<RenderCmd> cmds = new List<RenderCmd>();
    public static int nbCmds;

    private void Awake()
    {
        bitmapFont = null;
        if (fontImage && fontConfig)
            bitmapFont = BitmapFont.FromXml(fontConfig, fontImage);
    }

    private void OnGUI()
    {
        // Set the 0,0 at the top-left corner of this panel
        parentRect = Drawing2D.GetWorldRect(this.transform as RectTransform);
        Drawing2D.SetParentBounds(parentRect);

        if (Event.current.type == EventType.Repaint)
        {
            for (var i = 0; i < nbCmds; ++i)
            {
                var cmd = cmds[i];
                Drawing2D.DrawPoint(new Vector2(cmd.coord.x, Screen.height - cmd.coord.y), PowderTypes.values[cmd.type].color);
            }
        }

        Drawing2D.ClearParentBounds();
    }
}
