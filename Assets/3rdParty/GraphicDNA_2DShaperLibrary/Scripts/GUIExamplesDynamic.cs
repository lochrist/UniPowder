/* Copyright (C) GraphicDNA - All Rights Reserved
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 * Written by Iñaki Ayucar <iayucar@simax.es>, September 2016
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
 * THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS 
 * IN THE SOFTWARE.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphicDNA;

public class GUIExamplesDynamic : MonoBehaviour
{
    public enum eRenderMode
    {
        OnGUI,
        OnPostRender,
    }
    public eRenderMode RenderMode = eRenderMode.OnGUI;
    public Texture2D FontImage;
    public TextAsset FontConfig;
    private BitmapFont mBitmapFont;    
    public Texture2D CustomTexturePicture;
    public Texture2D CustomTexture3D;
    public Texture2D CustomTexture3DVertical;
    public Texture2D CustomTextureLines;
    public Texture2D CustomTextureProgressBar;
    private float mTime;
    private float mTime2;
    private List<float> mChartValuesRed = new List<float>();
    private List<float> mChartValuesYellow = new List<float>();
    private float mChartX = 110;
    private float mChartY = 50;
    private float mChartWidth = 500;
    private float mChartHeight = 80;
    private Vector2 mRangeChartRed = new Vector2(0, 1);
    private Vector2 mRangeChartYellow = new Vector2(0, 1);
    private List<float> mBarsHeight = new List<float>()
    {
        20,
        65,
        30,
        25,
    };
    private float mPictureAngle1, mPictureAngle2;
    private Color mPicturecolor = Color.white;

    /// <summary>
    /// 
    /// </summary>
    private void Awake()
    {
        mBitmapFont = null;
        if (FontImage && FontConfig)
            mBitmapFont = BitmapFont.FromXml(FontConfig, FontImage);
    }

    #region Update
    /// <summary>
    /// 
    /// </summary>
    private void UpdateChartValues(List<float> chartValues, float pMinRange, float pMaxRange)
    {
        chartValues.Add(Random.Range(pMinRange, pMaxRange));
        int maxSamples = (int)(mChartWidth / 10);
        for (int i = maxSamples; i < chartValues.Count; i++)
            chartValues.RemoveAt(0);
    }
    /// <summary>
    /// 
    /// </summary>
    private void Update()
    {
        mTime += Time.deltaTime;
        if (mTime > 0.05f)
        {
            mTime = 0;
            UpdateChartValues(mChartValuesRed, mRangeChartRed.x, mRangeChartRed.y);
            UpdateChartValues(mChartValuesYellow, mRangeChartYellow.x, mRangeChartYellow.y);

            for (int i = 0; i < mBarsHeight.Count; i++)
                mBarsHeight[i] = Random.Range(10f, 70f);
        }

        mTime2 += Time.deltaTime;
        if(mTime2 >1f)
        {
            mTime2 = 0;
            mRangeChartRed = new Vector2(Random.Range(0f, 0.5f), Random.Range(0.51f, 1f));
            mRangeChartYellow = new Vector2(Random.Range(0f, 0.5f), Random.Range(0.51f, 1f));

            mPicturecolor = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
        }
    }
    #endregion

    #region Draw
    /// <summary>
    /// 
    /// </summary>
    /// <param name="r"></param>
    /// <param name="g"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    private Color ToColor(int r, int g, int b, int a = 255)
    {
        return new UnityEngine.Color((float)r / 255f, (float)g / 255f, (float)b / 255f, (float)a / 255f);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="pChartValues"></param>
    /// <param name="pColor"></param>
    private void DrawSeries(List<float> pChartValues, Color pColor)
    {
        List<Vector2> vertices = new List<Vector2>();
        float xVal = mChartX + 10;
        float yRange = mChartHeight - 20;
        foreach (float val in pChartValues)
        {
            vertices.Add(new Vector2(xVal, 50 + (val * yRange)));
            xVal += 10;
        }
        Drawing2D.DrawPolygon(vertices.ToArray(), pColor, 4f, null, true);

    }
    /// <summary>
    /// 
    /// </summary>
    private void DrawChart()
    {


        // Draw main axis
        Drawing2D.DrawLine(new Vector2(mChartX, mChartY + mChartHeight), new Vector2(mChartX + mChartWidth, mChartY + mChartHeight), ToColor(240, 240, 240), 2);       // X Axis
        Drawing2D.DrawLine(new Vector2(mChartX, mChartY + mChartHeight), new Vector2(mChartX, mChartY - 25), ToColor(240, 240, 240), 2);                    // Y Axis

        // Draw guide horizontal lines
        Drawing2D.DrawLine(new Vector2(mChartX, mChartY + 65), new Vector2(mChartX + mChartWidth, mChartY + 65), ToColor(180, 180, 180, 128), 1);
        Drawing2D.DrawLine(new Vector2(mChartX, mChartY + 50), new Vector2(mChartX + mChartWidth, mChartY + 50), ToColor(180, 180, 180, 128), 1);
        Drawing2D.DrawLine(new Vector2(mChartX, mChartY + 35), new Vector2(mChartX + mChartWidth, mChartY + 35), ToColor(180, 180, 180, 128), 1);
        Drawing2D.DrawLine(new Vector2(mChartX, mChartY + 20), new Vector2(mChartX + mChartWidth, mChartY + 20), ToColor(180, 180, 180, 128), 1);
        Drawing2D.DrawLine(new Vector2(mChartX, mChartY + 5), new Vector2(mChartX + mChartWidth, mChartY + 5), ToColor(180, 180, 180, 128), 1);
        Drawing2D.DrawLine(new Vector2(mChartX, mChartY - 10), new Vector2(mChartX + mChartWidth, mChartY - 10), ToColor(180, 180, 180, 128), 1);
        Drawing2D.DrawLine(new Vector2(mChartX, mChartY - 25), new Vector2(mChartX + mChartWidth, mChartY - 25), ToColor(180, 180, 180, 128), 1);
        Drawing2D.DrawText("Time", mChartX + mChartWidth, mChartY + mChartHeight - 10, 11, ToColor(240, 240, 240), mBitmapFont);
        Drawing2D.DrawText("Value", mChartX + 5, mChartY - 25, 11, ToColor(240, 240, 240), mBitmapFont);

        // Draw Series
        DrawSeries(mChartValuesRed, ToColor(232, 62, 83));
        DrawSeries(mChartValuesYellow, ToColor(248, 222, 104));
    }
    /// <summary>
    /// 
    /// </summary>
    private void DrawChartBars()
    {
        // Bars Chart
        float x = 700;
        float y = 120;
        float barWidth = 20;

        
        Drawing2D.FillRect(new Rect(x + 0 , y - mBarsHeight[0], barWidth, mBarsHeight[0]), ToColor(36, 86, 188), CustomTexture3DVertical);
        Drawing2D.FillRect(new Rect(x + 20, y - mBarsHeight[1], barWidth, mBarsHeight[1]), ToColor(175, 217, 141), CustomTexture3DVertical);
        Drawing2D.FillRect(new Rect(x + 40, y - mBarsHeight[2], barWidth, mBarsHeight[2]), ToColor(255, 112, 92), CustomTexture3DVertical);
        Drawing2D.FillRect(new Rect(x + 60, y - mBarsHeight[3], barWidth, mBarsHeight[3]), ToColor(93, 166, 221), CustomTexture3DVertical);
    }
    /// <summary>
    /// 
    /// </summary>
    private void DrawPicture()
    {
        mPictureAngle1 += Time.deltaTime * 20f;
        mPictureAngle2 -= Time.deltaTime * 4f;

        // Picture
        float x = 200;
        float y = 260;
        Drawing2D.FillQuad(new Vector2(x + 40, y + 5), 200, 108, Color.white, mPictureAngle1, CustomTexturePicture, null);
        Drawing2D.DrawQuad(new Vector2(x + 40, y + 5), 200, 108, Color.white, 2, mPictureAngle1);

        Drawing2D.FillQuad(new Vector2(x + 370, y + 5), 200, 108, mPicturecolor, mPictureAngle2, CustomTexturePicture, null);
        Drawing2D.DrawQuad(new Vector2(x + 370, y + 5), 200, 108, mPicturecolor, 2, mPictureAngle2);
    }
    /// <summary>
    /// Draws all elements in the demo
    /// </summary>
    private void DrawAll()
    {       
        DrawChart();
        DrawChartBars();
        DrawPicture();
    }
    #endregion

    /// <summary>
    /// 
    /// </summary>
    private void OnGUI()
    {
        if (this.RenderMode == eRenderMode.OnGUI)
        {
            Drawing2D.ScreenWidth = Screen.width;
            Drawing2D.ScreenHeight = Screen.height;

            Rect mParentRect = Drawing2D.GetWorldRect(this.transform as RectTransform);
            Drawing2D.SetParentBounds(mParentRect);

            DrawAll();

            Drawing2D.ClearParentBounds();
        }
    }
    /// <summary>
    /// 
    /// </summary>
    private void OnPostRender()
    {
        if (this.RenderMode == eRenderMode.OnPostRender)
        {
            Drawing2D.ScreenWidth = 1024;
            Drawing2D.ScreenHeight = 512;
            Drawing2D.ClearFrameBuffer(new Color(0, 0, 0, 0.1f));

            DrawAll();
        }
    }    
}
