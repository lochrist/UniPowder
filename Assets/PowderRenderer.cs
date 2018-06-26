using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphicDNA;
using UnityEngine.Networking;

public class PowderRenderer : MonoBehaviour
{
    public Texture2D FontImage;
    public TextAsset FontConfig;
    public BitmapFont BitmapFont;
    public Rect ParentRect;

    public static List<RenderCmd> Cmds = new List<RenderCmd>();
    public static int NbCmds;

    private void Awake()
    {
        BitmapFont = null;
        if (FontImage && FontConfig)
            BitmapFont = BitmapFont.FromXml(FontConfig, FontImage);
    }

    private void OnGUI()
    {
        // Set the 0,0 at the top-left corner of this panel
        ParentRect = Drawing2D.GetWorldRect(this.transform as RectTransform);
        Drawing2D.SetParentBounds(ParentRect);

        if (Event.current.type == EventType.Repaint)
        {
            for (var i = 0; i < NbCmds; ++i)
            {
                var cmd = Cmds[i];
                Drawing2D.DrawPoint(cmd.coord, PowderTypes.values[cmd.type].color);
            }
        }

        Drawing2D.ClearParentBounds();
    }
}
