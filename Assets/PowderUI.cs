using System.Collections.Generic;
using UnityEngine;
using GraphicDNA;

public class PowderUI : MonoBehaviour
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
        var toggleMode = GUILayout.Toggle(PowderGame.generatorMode, "Generator");
        if (PowderGame.generatorMode != toggleMode)
        {
            if (PowderGame.generatorMode)
            {
                // Back to normal Mode 
                if (PowderTypes.values[PowderGame.currentPowder].IsGenerator())
                    PowderGame.currentPowder = PowderTypes.values[PowderGame.currentPowder].generatedElementType;
                PowderGame.generatorMode = toggleMode;
            }
            else
            {
                // Go to generator mode if we support it:
                var generatorType = PowderTypes.FindGeneratorType(PowderGame.currentPowder);
                if (generatorType != -1)
                {
                    PowderGame.currentPowder = generatorType;
                    PowderGame.generatorMode = toggleMode;
                }
            }
        }

        if (GUILayout.Button("Sand"))
        {
            PowderGame.currentPowder = PowderGame.generatorMode ? PowderTypes.SandGenerator : PowderTypes.Sand;
        }
        if (GUILayout.Button("Water"))
        {
            PowderGame.currentPowder = PowderGame.generatorMode ? PowderTypes.WaterGenerator : PowderTypes.Water;
        }
        if (GUILayout.Button("Fire"))
        {
            PowderGame.currentPowder = PowderGame.generatorMode ? PowderTypes.FireGenerator : PowderTypes.Fire;
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
            PowderGame.currentPowder = PowderGame.generatorMode ? PowderTypes.SmokeGenerator : PowderTypes.Smoke;
        }
        if (GUILayout.Button("Steam"))
        {
            PowderGame.currentPowder = PowderGame.generatorMode ? PowderTypes.SteamGenerator : PowderTypes.Steam;
        }
        if (GUILayout.Button("Acid"))
        {
            PowderGame.currentPowder = PowderGame.generatorMode ? PowderTypes.AcidGenerator : PowderTypes.Acid;
        }
        if (GUILayout.Button("Glass"))
        {
            PowderGame.currentPowder = PowderTypes.Glass;
        }
        if (GUILayout.Button("Lava"))
        {
            PowderGame.currentPowder = PowderGame.generatorMode ? PowderTypes.LavaGenerator : PowderTypes.Lava;
        }
        GUILayout.EndHorizontal();
        Drawing2D.ClearParentBounds();
    }
}
