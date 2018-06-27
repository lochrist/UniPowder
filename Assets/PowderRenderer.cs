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

    float m_DeltaTime = 0.0f;
    float m_UIOffset = 30;

    private void Awake()
    {
        bitmapFont = null;
        if (fontImage && fontConfig)
            bitmapFont = BitmapFont.FromXml(fontConfig, fontImage);
    }

    private void Update()
    {
        m_DeltaTime += (Time.unscaledDeltaTime - m_DeltaTime) * 0.1f;
    }

    private void OnGUI()
    {
        // Set the 0,0 at the top-left corner of this panel
        parentRect = Drawing2D.GetWorldRect(this.transform as RectTransform);
        Drawing2D.SetParentBounds(parentRect);

        GUILayout.Space(PowderGame.worldRect.y - m_UIOffset);
        GUILayout.BeginHorizontal();
        GUILayout.Space(PowderGame.worldRect.x);
        GUILayout.Label("Particles: " + PowderGame.powderCount);
        GUILayout.Space(30);
        var msec = m_DeltaTime * 1000.0f;
        var fps = 1.0f / m_DeltaTime;
        GUILayout.Label($"{msec:0.0} ms ({fps:0.} fps)");
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Reset"))
        {
            PowderGame.Reset();
        }
        GUILayout.EndHorizontal();

        if (Event.current.type == EventType.Repaint)
        {
            Drawing2D.DrawRect(PowderGame.worldRect, PowderGame.worldBoundariesColor, 2);

            for (var i = 0; i < nbCmds; ++i)
            {
                var cmd = cmds[i];
                Drawing2D.DrawPoint(new Vector2(cmd.coord.x + PowderGame.worldRect.x, Screen.height - PowderGame.worldRect.y - cmd.coord.y), 
                    PowderTypes.values[cmd.type].color,
                    2f);
            }
        }

        GUILayout.Space(PowderGame.worldRect.height + m_UIOffset);
        GUILayout.BeginHorizontal();
        GUILayout.Space(PowderGame.worldRect.x);
        // Powder buttons to support:
        // sand
        // water
        // fire
        // stone
        // wood
        // smoke
        // steam
        // Acid
        // Glass
        if (GUILayout.Button("Sand"))
        {
            PowderGame.currentPowder = PowderTypes.Sand;
        }
        if (GUILayout.Button("Water"))
        {
            PowderGame.currentPowder = PowderTypes.Water;
        }
        if (GUILayout.Button("Fire"))
        {
            PowderGame.currentPowder = PowderTypes.Fire;
        }
        if (GUILayout.Button("Stone"))
        {
            PowderGame.currentPowder = PowderTypes.Stone;
        }
        if (GUILayout.Button("Wood"))
        {
            PowderGame.currentPowder = PowderTypes.Wood;
        }
        if (GUILayout.Button("Smoke"))
        {
            PowderGame.currentPowder = PowderTypes.Smoke;
        }
        if (GUILayout.Button("Steam"))
        {
            PowderGame.currentPowder = PowderTypes.Steam;
        }
        if (GUILayout.Button("Acid"))
        {
            PowderGame.currentPowder = PowderTypes.Acid;
        }
        if (GUILayout.Button("Glass"))
        {
            PowderGame.currentPowder = PowderTypes.Glass;
        }
        GUILayout.EndHorizontal();
        Drawing2D.ClearParentBounds();
    }
}
