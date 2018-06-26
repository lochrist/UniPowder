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

public class GUIExamplesStatic : MonoBehaviour
{
    public Texture2D FontImage;
    public TextAsset FontConfig;
    private BitmapFont mBitmapFont;
    private Rect mParentRect;
    private float mRectangleHeight = 80;
    private float mRectangleWidth;
    private float mRectangleY;
    private float mRectangleX = 10;
    private int mNumSamples = 8;
    private float mSampleWidth;
    private float mSamplesMargin = 20;

    public Texture2D CustomTexturePicture;
    public Texture2D CustomTexture3D;
    public Texture2D CustomTexture3DVertical;
    public Texture2D CustomTextureLines;
    public Texture2D CustomTextureProgressBar;

    #region DrawMethods
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
    /// Lines example
    /// </summary>
    private void DrawLines()
    {        
        Drawing2D.DrawRect(new Rect(mRectangleX, mRectangleY, mRectangleWidth, mRectangleHeight));
        Drawing2D.DrawText("Lines & Arrows", mRectangleX + 5, mRectangleY + 5, 18, Color.white, mBitmapFont);

        float x = 15 - mSampleWidth;
        float y = 60;
        Drawing2D.DrawLine(new Vector2(x += mSampleWidth, y), new Vector2(x + mSampleWidth - mSamplesMargin, y), ToColor(36, 86, 188), 4f);
        Drawing2D.DrawLine(new Vector2(x += mSampleWidth, y), new Vector2(x + mSampleWidth - mSamplesMargin, y), ToColor(93, 166, 221), 8f);
        Drawing2D.DrawLine(new Vector2(x += mSampleWidth, y), new Vector2(x + mSampleWidth - mSamplesMargin, y), ToColor(21, 179, 89), 16f, CustomTexture3D);
        Drawing2D.DrawDashedLine(new Vector2(x += mSampleWidth, y), new Vector2(x + mSampleWidth - mSamplesMargin, y), ToColor(175, 217, 141), 4f, 3);
        Drawing2D.DrawDashedLine(new Vector2(x += mSampleWidth, y), new Vector2(x + mSampleWidth - mSamplesMargin, y), ToColor(248, 222, 104), 8f, 3);
        Drawing2D.DrawDashedLine(new Vector2(x += mSampleWidth, y), new Vector2(x + mSampleWidth - mSamplesMargin, y), ToColor(255, 196, 126), 16f, 3);
        Drawing2D.DrawArrow(new Vector2(x += mSampleWidth, y + 10), new Vector2(x + mSampleWidth - mSamplesMargin - 25, y - 10), ToColor(255, 112, 92), 4, 20, 25);
        Drawing2D.DrawDashedArrow(new Vector2(x += mSampleWidth, y - 10), new Vector2(x + mSampleWidth - mSamplesMargin - 25, y + 10), ToColor(232, 62, 83), 4, 20, 25, 3);
    }
    /// <summary>
    /// Rectangles Example
    /// </summary>
    private void DrawRectangles()
    {
        mRectangleY += mRectangleHeight + 20;
        Drawing2D.DrawRect(new Rect(mRectangleX, mRectangleY, mRectangleWidth, mRectangleHeight));
        Drawing2D.DrawText("Rects & Quads", mRectangleX + 5, mRectangleY + 5, 18, Color.white, mBitmapFont);
        float x = 15 - mSampleWidth;
        float y = mRectangleY + 35;
        float height = mRectangleHeight - 55;
        float quadWidth = mSampleWidth - mSamplesMargin;
        Drawing2D.DrawRect(new Rect(x += mSampleWidth, y, quadWidth, height), ToColor(36, 86, 188), 4f);
        Drawing2D.DrawRect(new Rect(x += mSampleWidth, y, quadWidth, height), ToColor(93, 166, 221), 8f);
        Drawing2D.DrawQuad(new Vector2((x += mSampleWidth) + (quadWidth / 2), y), quadWidth, height, ToColor(21, 179, 89), 10f, -5f);
        Drawing2D.DrawDashedRect(new Rect(x += mSampleWidth, y, quadWidth, height), ToColor(175, 217, 141), 2f, 3);
        Drawing2D.DrawDashedQuad(new Vector2((x += mSampleWidth) + (quadWidth / 2), y + 1), quadWidth, height, ToColor(248, 222, 104), 3f, 7f, 3);
        Drawing2D.DrawDashedRect(new Rect(x += mSampleWidth, y, quadWidth, height), ToColor(255, 196, 126), 6f, 3);
        Drawing2D.FillQuad(new Vector2((x += mSampleWidth) + (quadWidth / 2), y + 10), quadWidth, height, ToColor(255, 112, 92), 8);
        Drawing2D.FillRect(new Rect(x += mSampleWidth, y - 10, quadWidth, height + 20), ToColor(232, 62, 83), CustomTexture3D);
    }
    /// <summary>
    /// Circles Example
    /// </summary>
    private void DrawCircles()
    {
        mRectangleY += mRectangleHeight + 20;
        Drawing2D.DrawRect(new Rect(mRectangleX, mRectangleY, mRectangleWidth, mRectangleHeight));
        Drawing2D.DrawText("Circles", mRectangleX + 5, mRectangleY + 5, 18, Color.white, mBitmapFont);
        float x = 15 - mSampleWidth;
        float y = mRectangleY + 50;
        float radius = mRectangleHeight - 60;
        Drawing2D.DrawCircle(new Vector2((x += mSampleWidth) + ((mSampleWidth / 2) - radius), y), radius, 8, ToColor(36, 86, 188), 2f);
        Drawing2D.DrawCircle(new Vector2((x += mSampleWidth) + ((mSampleWidth / 2) - radius), y), radius, 20, ToColor(93, 166, 221), 2f);
        Drawing2D.DrawCircle(new Vector2((x += mSampleWidth) + ((mSampleWidth / 2) - radius), y), radius, 20, ToColor(21, 179, 89), 4f);
        radius = mRectangleHeight - 50;
        y = mRectangleY + 40;
        Drawing2D.DrawDashedCircle(new Vector2((x += mSampleWidth) + ((mSampleWidth / 2) - radius), y), radius, 12, ToColor(175, 217, 141), 3f, 3f);
        Drawing2D.DrawDashedCircle(new Vector2((x += mSampleWidth) + ((mSampleWidth / 2) - radius), y), radius, 12, ToColor(248, 222, 104), 3f, 3f);
        Drawing2D.DrawDashedCircle(new Vector2((x += mSampleWidth) + ((mSampleWidth / 2) - radius), y), radius, 12, ToColor(255, 196, 126), 3f, 3f);
        Drawing2D.FillCircle(new Vector2((x += mSampleWidth) + ((mSampleWidth / 2) - radius), y), radius, ToColor(255, 112, 92));
        Drawing2D.FillCircle(new Vector2((x += mSampleWidth) + ((mSampleWidth / 2) - radius), y), radius * 1.15f, ToColor(232, 62, 83));

    }
    /// <summary>
    /// Arcs Example
    /// </summary>
    private void DrawArcs()
    {
        mRectangleY += mRectangleHeight + 20;
        Drawing2D.DrawRect(new Rect(mRectangleX, mRectangleY, mRectangleWidth, mRectangleHeight));
        Drawing2D.DrawText("Arcs", mRectangleX + 5, mRectangleY + 5, 18, Color.white, mBitmapFont);
        float x = 15 - mSampleWidth;
        float y = mRectangleY + 50;
        float radius = mRectangleHeight - 60;
        Drawing2D.DrawArc(new Vector2((x += mSampleWidth) + ((mSampleWidth / 2) - radius), y), radius, 8, 0, 270, ToColor(36, 86, 188), 2f, CustomTextureLines);
        Drawing2D.DrawArc(new Vector2((x += mSampleWidth) + ((mSampleWidth / 2) - radius), y), radius, 12, 90, 270, ToColor(93, 166, 221), 2f, CustomTextureLines);
        Drawing2D.DrawArc(new Vector2((x += mSampleWidth) + ((mSampleWidth / 2) - radius), y), radius, 16, 0, 180, ToColor(21, 179, 89), 4f, CustomTextureLines);
        radius = mRectangleHeight - 50;
        y = mRectangleY + 40;
        Drawing2D.DrawDashedArc(new Vector2((x += mSampleWidth) + ((mSampleWidth / 2) - radius), y), radius, 12, 0, 270, ToColor(175, 217, 141), 3f, 3f);
        Drawing2D.DrawDashedArc(new Vector2((x += mSampleWidth) + ((mSampleWidth / 2) - radius), y), radius, 12, 45, 225, ToColor(248, 222, 104), 4f, 3f);
        Drawing2D.DrawDashedArc(new Vector2((x += mSampleWidth) + ((mSampleWidth / 2) - radius), y), radius, 12, 90, 270, ToColor(255, 196, 126), 6f, 3f);

        Drawing2D.FillCircle(new Vector2((x += mSampleWidth) + ((mSampleWidth / 2) - radius), y), radius, ToColor(255, 112, 92));
        Drawing2D.DrawDashedArc(new Vector2((x) + ((mSampleWidth / 2) - radius), y), radius + 2, 12, 90, 270, ToColor(255, 112, 92), 6f, 3f);
        Drawing2D.FillCircle(new Vector2((x += mSampleWidth) + ((mSampleWidth / 2) - radius), y), radius * 1.15f, ToColor(232, 62, 83));
        Drawing2D.DrawDashedArc(new Vector2((x) + ((mSampleWidth / 2) - radius), y), (radius * 1.15f) + 2, 12, 0, 180, ToColor(232, 62, 83), 6f, 3f);
    }
    /// <summary>
    /// Triangles Example
    /// </summary>
    private void DrawTriangles()
    {
        mRectangleY += mRectangleHeight + 20;
        Drawing2D.DrawRect(new Rect(mRectangleX, mRectangleY, mRectangleWidth, mRectangleHeight));
        Drawing2D.DrawText("Triangles", mRectangleX + 5, mRectangleY + 5, 18, Color.white, mBitmapFont);
        float x = 15 - mSampleWidth;
        float y = mRectangleY + 50;
        x += mSampleWidth + 50;
        Drawing2D.DrawTriangle(new Vector2(x, y), new Vector2(x + 30, y - 15), new Vector2(x + 15, y + 15), ToColor(36, 86, 188), 2f);
        x += mSampleWidth;
        Drawing2D.DrawTriangle(new Vector2(x - 10, y + 5), new Vector2(x + 60, y - 20), new Vector2(x + 15, y + 15), ToColor(93, 166, 221), 4f);
        x += mSampleWidth;
        Drawing2D.DrawTriangle(new Vector2(x + 5, y - 10), new Vector2(x + 40, y - 15), new Vector2(x + 15, y + 20), ToColor(21, 179, 89), 2f);
        x += mSampleWidth + 20;
        Drawing2D.DrawDashedTriangle(new Vector2(x - 20, y), new Vector2(x + 30, y - 5), new Vector2(x + 15, y + 15), ToColor(175, 217, 141), 2f, 3f);
        x += mSampleWidth;
        Drawing2D.DrawDashedTriangle(new Vector2(x - 10, y - 15), new Vector2(x + 60, y - 20), new Vector2(x + 15, y + 15), ToColor(248, 222, 104), 4f, 3);
        x += mSampleWidth;
        Drawing2D.DrawDashedTriangle(new Vector2(x - 35, y - 30), new Vector2(x + 60, y - 25), new Vector2(x + 15, y + 20), ToColor(255, 196, 126), 6f, 3);
        Drawing2D.FillTriangle(new Vector2(x += mSampleWidth, y - 20), new Vector2(x + 50, y), new Vector2(x + 30, y + 20), ToColor(255, 112, 92));
        Drawing2D.FillTriangle(new Vector2(x += mSampleWidth, y), new Vector2(x + 90, y - 20), new Vector2(x + 20, y + 20), ToColor(232, 62, 83));
    }
    /// <summary>
    /// Polygons Example
    /// </summary>
    private void DrawPolygons()
    {
        mRectangleY += mRectangleHeight + 20;
        Drawing2D.DrawRect(new Rect(mRectangleX, mRectangleY, mRectangleWidth, mRectangleHeight));
        Drawing2D.DrawText("Polygons", mRectangleX + 5, mRectangleY + 5, 18, Color.white, mBitmapFont);
        float x = 15 - mSampleWidth;
        float y = mRectangleY + 50;
        float radius = mRectangleHeight - 60;
        Drawing2D.DrawPentagon(new Vector2((x += mSampleWidth) + ((mSampleWidth / 2) - radius), y), radius, ToColor(36, 86, 188), 2f, CustomTextureLines);
        Drawing2D.DrawHexagon(new Vector2((x += mSampleWidth) + ((mSampleWidth / 2) - radius), y), radius, ToColor(93, 166, 221), 4f, CustomTextureLines);
        Drawing2D.DrawOctogon(new Vector2((x += mSampleWidth) + ((mSampleWidth / 2) - radius), y - 5), radius + 10, ToColor(21, 179, 89), 2f, CustomTextureLines);
        Drawing2D.DrawDashedPentagon(new Vector2((x += mSampleWidth) + ((mSampleWidth / 2) - radius), y), radius, ToColor(175, 217, 141), 2f, 5);
        Drawing2D.DrawDashedHexagon(new Vector2((x += mSampleWidth) + ((mSampleWidth / 2) - radius), y), radius, ToColor(248, 222, 104), 4f, 4);
        Drawing2D.DrawDashedOctogon(new Vector2((x += mSampleWidth) + ((mSampleWidth / 2) - radius), y - 5), radius + 10, ToColor(255, 196, 126), 4f, 4);
        x += mSampleWidth + 20;
        Vector2[] vertices = new Vector2[6];
        vertices[0] = new Vector2(x - 10, y - 30);
        vertices[1] = new Vector2(x + 10, y - 15);
        vertices[2] = new Vector2(x + 100, y - 25);
        vertices[3] = new Vector2(x + 65, y + 25);
        vertices[4] = new Vector2(x + 0, y + 15);
        vertices[5] = new Vector2(x - 10, y - 10);
        Drawing2D.DrawPolygon(vertices, ToColor(255, 112, 92), 4f);
        x += mSampleWidth;
        vertices = new Vector2[6];
        vertices[0] = new Vector2(x - 10, y - 30);
        vertices[1] = new Vector2(x + 10, y - 15);
        vertices[3] = new Vector2(x + 100, y + 25);
        vertices[2] = new Vector2(x + 65, y - 25);
        vertices[4] = new Vector2(x + 0, y + 15);
        vertices[5] = new Vector2(x - 10, y - 10);
        Drawing2D.DrawDashedPolygon(vertices, ToColor(232, 62, 83), 2f, false, 3f);
    }
    /// <summary>
    /// 
    /// </summary>
    private void DrawCompound()
    {
        mNumSamples = 7;
        mSampleWidth = (mParentRect.width - 30) / (mNumSamples);


        mRectangleY += mRectangleHeight + 20;
        Drawing2D.DrawRect(new Rect(mRectangleX, mRectangleY, mRectangleWidth, mRectangleHeight));
        Drawing2D.DrawText("Other", mRectangleX + 5, mRectangleY + 5, 18, Color.white, mBitmapFont);
        float x = 15 - mSampleWidth;
        float y = mRectangleY + 35;
        float height = mRectangleHeight - 55;

        // Bars Chart
        x += 50;
        Drawing2D.FillRect(new Rect((x += mSampleWidth + 20), y - 10, 15, 50), ToColor(36, 86, 188), CustomTexture3DVertical);
        Drawing2D.FillRect(new Rect( x + 15, y, 15, 40), ToColor(175, 217, 141), CustomTexture3DVertical);
        Drawing2D.FillRect(new Rect(x + 30, y + 20, 15, 20), ToColor(255, 112, 92), CustomTexture3DVertical);
        Drawing2D.FillRect(new Rect(x + 45, y - 5, 15, 45), ToColor(93, 166, 221), CustomTexture3DVertical);

        // Progress Bar (circle)
        float radius = mRectangleHeight - 60;
        y += 5;
        x -= 100;
        Drawing2D.DrawArc(new Vector2((x += mSampleWidth) + ((mSampleWidth / 2) - radius), y), radius * 1.5f, 32, 270, 270, ToColor(21, 179, 89), 8f, CustomTextureLines);
        Drawing2D.FillCircle(new Vector2((x) + ((mSampleWidth / 2) - radius), y), radius * 1.45f, ToColor(21, 179, 89, 128));
        Drawing2D.DrawText("75%", (x) + ((mSampleWidth / 2) - radius - 15), y - 9, 18, Color.white, mBitmapFont);
        y -= 5;

        // Progress Bar
        x += 30;
        CustomTextureProgressBar.wrapMode = TextureWrapMode.Repeat;
        Drawing2D.DrawRect(new Rect(x += mSampleWidth, y, mSampleWidth - mSamplesMargin, height - 5), Color.white, 1f);
        Drawing2D.FillRect(new Rect(x, y + 1, mSampleWidth - mSamplesMargin - 20, height - 5 - 1), ToColor(255, 255, 255, 200), CustomTextureProgressBar, new Vector2(4, 1));
        Drawing2D.DrawText("85%", (x) + ((mSampleWidth / 2) - 30), y + 1, 17, Color.white, mBitmapFont);
        //Drawing2D.DrawGUIText("85%", (x) + ((mSampleWidth / 2) - 30), y + 1, 17, Color.white);


        // X-Y Axis
        x += 60;
        Drawing2D.DrawArrow(new Vector2(x += mSampleWidth + 20, y + 35), new Vector2(x + 40, y + 35), ToColor(255, 112, 92), 4, 20, 25);
        Drawing2D.DrawText("X", x + 45, y + 15, 12, ToColor(255, 112, 92), mBitmapFont);
        Drawing2D.DrawArrow(new Vector2(x, y + 35), new Vector2(x, y - 5), ToColor(248, 222, 104), 4, 20, 25);
        Drawing2D.DrawText("Y", x + 10, y - 15, 12, ToColor(248, 222, 104), mBitmapFont);

        // Chart
        x -= 110;
        Drawing2D.DrawLine(new Vector2(x += mSampleWidth + 20, y + 35), new Vector2(x + 250, y + 35), ToColor(240, 240, 240), 2);       // X Axis
        Drawing2D.DrawLine(new Vector2(x, y + 35), new Vector2(x, y - 25), ToColor(240, 240, 240), 2);                                  // Y Axis
        Drawing2D.DrawLine(new Vector2(x, y + 20), new Vector2(x + 250, y + 20), ToColor(180, 180, 180, 128), 1);       // 10 Axis
        Drawing2D.DrawLine(new Vector2(x, y + 5), new Vector2(x + 250, y + 5), ToColor(180, 180, 180, 128), 1);       // 20 Axis
        Drawing2D.DrawLine(new Vector2(x, y - 10), new Vector2(x + 250, y - 10), ToColor(180, 180, 180, 128), 1);       // 30 Axis
        Drawing2D.DrawLine(new Vector2(x, y - 25), new Vector2(x + 250, y - 25), ToColor(180, 180, 180, 128), 1);       // 30 Axis
        Drawing2D.DrawText("Time", x + 220, y + 20, 11, ToColor(240, 240, 240), mBitmapFont);
        Drawing2D.DrawText("Value", x + 5, y - 25, 11, ToColor(240, 240, 240), mBitmapFont);
        Vector2[] vertices = new Vector2[12];
        vertices[0] = new Vector2(x + 10, y + 30);
        vertices[1] = new Vector2(x + 30, y + 15);
        vertices[2] = new Vector2(x + 50, y + 20);
        vertices[3] = new Vector2(x + 70, y + 25);
        vertices[4] = new Vector2(x + 90, y + 15);
        vertices[5] = new Vector2(x + 110, y - 5);
        vertices[6] = new Vector2(x + 130, y - 15);
        vertices[7] = new Vector2(x + 150, y - 25);
        vertices[8] = new Vector2(x + 170, y - 5);
        vertices[9] = new Vector2(x + 190, y - 10);
        vertices[10] = new Vector2(x + 210, y - 15);
        vertices[11] = new Vector2(x + 230, y - 20);
        Drawing2D.DrawPolygon(vertices, ToColor(232, 62, 83), 4f, null, true);

        // Picture
        x += 110;
        Drawing2D.FillQuad(new Vector2((x += mSampleWidth) + 40, y + 5), 140, 64, Color.white, 8, CustomTexturePicture, null);
        Drawing2D.DrawQuad(new Vector2((x) + 40, y + 5), 140, 64, Color.white, 3, 8);

        x -= 20;
        Drawing2D.FillQuad(new Vector2((x += mSampleWidth) + 40, y + 5), 140, 64, Color.green, -6, CustomTexturePicture, null);
        Drawing2D.DrawQuad(new Vector2((x) + 40, y + 5), 140, 64, Color.white, 1, -6);

    }
    #endregion

    /// <summary>
    /// 
    /// </summary>
    private void Awake()
    {
        mBitmapFont = null;
        if (FontImage && FontConfig)
            mBitmapFont = BitmapFont.FromXml(FontConfig, FontImage);
    }
    /// <summary>
    /// 
    /// </summary>
    private void OnGUI()
    {
        // Set the 0,0 at the top-left corner of this panel
        mParentRect = Drawing2D.GetWorldRect(this.transform as RectTransform);
        Drawing2D.SetParentBounds(mParentRect);

        if (Event.current.type == EventType.Repaint)
        {
            mNumSamples = 8;
            mRectangleY = 10;
            mRectangleWidth = mParentRect.width - 20;
            mSampleWidth = (mParentRect.width - 30) / (mNumSamples);

            DrawLines();
            DrawRectangles();
            DrawCircles();
            DrawArcs();
            DrawTriangles();
            DrawPolygons();
            DrawCompound();


        }

        Drawing2D.ClearParentBounds();
    }
}
