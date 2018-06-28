using UnityEngine;
using UnityEngine.UI;

public class PowderUI : MonoBehaviour
{
    public Rect parentRect;

    float m_DeltaTime = 0.0f;
    float m_UIOffset = 30;

    public Text fpsText;
    public Text particlesCountText;
    public Text brushSizeText;
    public Text brushTypeText;

    public Toggle sandBtn;
    public Toggle waterBtn;
    public Toggle acidBtn;
    public Toggle lavaBtn;

    public Toggle fireBtn;
    public Toggle steamBtn;
    public Toggle smokeBtn;

    public Toggle stoneBtn;
    public Toggle glassBtn;
    public Toggle woodBtn;

    public Text pauseBtnText;

    private void Awake()
    {
    }

    private void Start()
    {
        UpdateBrushSize();
        UpdateBrushTypeText();
        UpdateParticlesCount();

        UpdateBtnTint(PowderTypes.Sand, sandBtn);
        UpdateBtnTint(PowderTypes.Water, waterBtn);
        UpdateBtnTint(PowderTypes.Lava, lavaBtn);
        UpdateBtnTint(PowderTypes.Acid, acidBtn);

        UpdateBtnTint(PowderTypes.Fire, fireBtn);
        UpdateBtnTint(PowderTypes.Steam, steamBtn);
        UpdateBtnTint(PowderTypes.Smoke, smokeBtn);

        UpdateBtnTint(PowderTypes.Glass, glassBtn);
        UpdateBtnTint(PowderTypes.Stone, stoneBtn);
        UpdateBtnTint(PowderTypes.Wood, woodBtn);
    }

    private void UpdateBtnTint(int type, Toggle btn)
    {
        btn.GetComponent<Image>().color = PowderTypes.values[type].color;
    }

    private void Update()
    {
        m_DeltaTime += (Time.unscaledDeltaTime - m_DeltaTime) * 0.1f;
        UpdateFps();
        UpdateParticlesCount();
    }

    private void OnGUI()
    {
        /*
        // Set the 0,0 at the top-left corner of this panel
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
        */
    }

    public void UpdateFps()
    {
        var msec = m_DeltaTime * 1000.0f;
        var fps = 1.0f / m_DeltaTime;
        fpsText.text = $"FPS: {fps:0.}";
    }

    public void UpdateParticlesCount()
    {
        particlesCountText.text = string.Format("Particles: {0}", PowderGame.powderCount);
    }

    public void IncBrushSize()
    {
        if (PowderGame.brushSize < 7)
        {
            PowderGame.brushSize++;
            UpdateBrushSize();
        }
    }

    public void DecBrushSize()
    {
        if (PowderGame.brushSize > 0)
        {
            PowderGame.brushSize--;
            UpdateBrushSize();
        }
    }

    public void UpdateBrushSize()
    {
        brushSizeText.text = string.Format("Brush Size: {0}", PowderGame.brushSize);
    }

    // TODO: isOn is always false???
    public void ToggleGenerator(bool isOn)
    {
        if (PowderGame.generatorMode)
        {
            // Back to normal Mode 
            if (PowderTypes.values[PowderGame.currentPowder].IsGenerator())
                PowderGame.currentPowder = PowderTypes.values[PowderGame.currentPowder].generatedElementType;
            PowderGame.generatorMode = !PowderGame.generatorMode;
        }
        else
        {
            // Go to generator mode if we support it:
            var generatorType = PowderTypes.FindGeneratorType(PowderGame.currentPowder);
            if (generatorType != -1)
            {
                PowderGame.currentPowder = generatorType;
                PowderGame.generatorMode = !PowderGame.generatorMode;
            }
        }

        UpdateBrushTypeText();
    }

    public void ToggleSand(bool isOn)
    {
        ToggleBrushType(PowderTypes.Sand, sandBtn, isOn);
    }

    public void ToggleWater(bool isOn)
    {
        ToggleBrushType(PowderTypes.Water, waterBtn, isOn);
    }

    public void ToggleAcid(bool isOn)
    {
        ToggleBrushType(PowderTypes.Acid, acidBtn, isOn);
    }

    public void ToggleLava(bool isOn)
    {
        ToggleBrushType(PowderTypes.Lava, lavaBtn, isOn);
    }

    public void ToggleSteam(bool isOn)
    {
        ToggleBrushType(PowderTypes.Steam, steamBtn, isOn);
    }

    public void ToggleSmoke(bool isOn)
    {
        ToggleBrushType(PowderTypes.Smoke, smokeBtn, isOn);
    }

    public void ToggleFire(bool isOn)
    {
        ToggleBrushType(PowderTypes.Fire, fireBtn, isOn);
    }

    public void ToggleStone(bool isOn)
    {
        ToggleBrushType(PowderTypes.Stone, stoneBtn, isOn);
    }

    public void ToggleWood(bool isOn)
    {
        ToggleBrushType(PowderTypes.Wood, woodBtn, isOn);
    }

    public void ToggleGlass(bool isOn)
    {
        ToggleBrushType(PowderTypes.Glass, glassBtn, isOn);
    }

    public void ToggleBrushType(int type, Toggle btn, bool isOn)
    {
        // TODO: this shouldn't be implemented as a toggle...
        if (PowderGame.currentPowder == type)
            return;

        if (PowderGame.generatorMode)
        {
            var generatorType = PowderTypes.FindGeneratorType(type);
            if (generatorType != -1)
            {
                PowderGame.currentPowder = generatorType;
            }
        }
        else
        {
            PowderGame.currentPowder = type;
        }

        UpdateBrushTypeText();
    }

    void UpdateBrushTypeText()
    {
        brushTypeText.text = string.Format("Brush: {0}", PowderTypes.values[PowderGame.currentPowder].name);
    }

    public void Reset()
    {
        PowderGame.Reset();
    }

    public void TogglePause()
    {
        PowderGame.simulate = !PowderGame.simulate;
        pauseBtnText.text = PowderGame.simulate ? "Pause" : "Play";
    }
}
