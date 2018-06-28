using System.Collections.Generic;
using UnityEngine;
using GraphicDNA;

public class PowderRenderer : MonoBehaviour
{
    public Texture2D fontImage;
    public TextAsset fontConfig;
    public BitmapFont bitmapFont;
    public Rect parentRect;

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

        GUILayout.Space(PowderGame.pixelWorldRect.y - m_UIOffset);
        GUILayout.BeginHorizontal();
        GUILayout.Space(PowderGame.pixelWorldRect.x);
        GUILayout.Label("Particles: " + PowderGame.powderCount);
        GUILayout.Space(30);
        var msec = m_DeltaTime * 1000.0f;
        var fps = 1.0f / m_DeltaTime;
        GUILayout.Label($"{msec:0.0} ms ({fps:0.} fps)");
        GUILayout.Label(PowderTypes.values[PowderGame.currentPowder].name);
        GUILayout.Space(30);

        if (GUILayout.Button("-") && PowderGame.brushSize > 0)
        {
            PowderGame.brushSize--;
        }
        GUILayout.Label("Brush: " + (PowderGame.brushSize + 1));
        if (GUILayout.Button("+") && PowderGame.brushSize < 6)
        {
            PowderGame.brushSize++;
        }

        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Reset"))
        {
            PowderGame.Reset();
        }

        if (GUILayout.Button("Print Info"))
        {
            PowderGame.PrintInfo();
        }
        GUILayout.EndHorizontal();

        if (Event.current.type == EventType.Repaint)
        {
            Drawing2D.DrawRect(PowderGame.pixelWorldRect, PowderGame.worldBoundariesColor, 2);
        }

        GUILayout.Space(PowderGame.pixelWorldRect.height + m_UIOffset);
        GUILayout.BeginHorizontal();
        GUILayout.Space(PowderGame.pixelWorldRect.x);
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
