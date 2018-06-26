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

    float deltaTime = 0.0f;

    private void Awake()
    {
        bitmapFont = null;
        if (fontImage && fontConfig)
            bitmapFont = BitmapFont.FromXml(fontConfig, fontImage);
    }

    private void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
    }

    private void OnGUI()
    {
        // Set the 0,0 at the top-left corner of this panel
        parentRect = Drawing2D.GetWorldRect(this.transform as RectTransform);
        Drawing2D.SetParentBounds(parentRect);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Particles: " + PowderGame.powderCount);

        var msec = deltaTime * 1000.0f;
        var fps = 1.0f / deltaTime;
        GUILayout.Label($"{msec:0.0} ms ({fps:0.} fps)");
        GUILayout.EndHorizontal();

        if (Event.current.type == EventType.Repaint)
        {
            Drawing2D.DrawRect(PowderGame.worldRect, PowderGame.worldBoundariesColor, 2);

            for (var i = 0; i < nbCmds; ++i)
            {
                var cmd = cmds[i];
                Drawing2D.DrawPoint(new Vector2(cmd.coord.x + PowderGame.worldRect.x, Screen.height - PowderGame.worldRect.y - cmd.coord.y), PowderTypes.values[cmd.type].color);
            }
        }

        Drawing2D.ClearParentBounds();
    }
}
