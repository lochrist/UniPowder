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
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GraphicDNA
{
    public class Drawing2D
    {
        public static float? mScreenWidth = null;
        /// <summary>
        /// Screen Width, in pixels. If no custom value is set, unity's Screen.Width will be used as default
        /// </summary>
        public static float ScreenWidth
        {
            get
            {
                if (mScreenWidth.HasValue)
                    return mScreenWidth.Value;
                else return Screen.width;
            }
            set { mScreenWidth = value; }
        }
        public static float? mScreenHeight = null;
        /// <summary>
        /// Screen Height, in pixels. If no custom value is set, unity's Screen.Height will be used as default
        /// </summary>
        public static float ScreenHeight
        {
            get
            {
                if (mScreenHeight.HasValue)
                    return mScreenHeight.Value;
                else return Screen.height;
            }
            set { mScreenHeight = value; }
        }


        /// <summary>
        /// This is a cache to store circle points already generated, to speed up drawing circles. 
        /// </summary>
        private static readonly Dictionary<int, IList<Vector2>> m_circleCache = new Dictionary<int, IList<Vector2>>();
        private static Rect? mParentBounds;
        private static Material mGLMaterialColorOnly = null;
        private static Material mGLMaterialColorAndTexture = null;
        private static bool mUseCustomProjectionMatrix = false;

        private static Matrix4x4? mProjectionMatrix;
        /// <summary>
        /// Builds a projection matrix for OpenGL, that sets the 0,0 in the TopLeft corner of the screen, just like in the GUI
        /// </summary>
        private static Matrix4x4 ProjectionMatrix
        {
            get
            {
                if (!mProjectionMatrix.HasValue)
                    mProjectionMatrix = Matrix4x4.Ortho(0, 1, 1, 0, -1, 100);
                return mProjectionMatrix.Value;
            }
        }

        /// <summary>
        /// Returns the default material to fill shapes
        /// </summary>
        public static Material MaterialColorOnly
        {
            get
            {
                if (mGLMaterialColorOnly == null || mGLMaterialColorOnly.Equals(null))
                {
                    Shader shader = Shader.Find("Hidden/Internal-Colored");       // This shader takes only into account the color, not the texture
                    mGLMaterialColorOnly = new Material(shader);
                }

                return mGLMaterialColorOnly;
            }
        }
        /// <summary>
        /// Returns the default material to fill shapes
        /// </summary>
        public static Material MaterialColorAndTexture
        {
            get
            {
                if (mGLMaterialColorAndTexture == null || mGLMaterialColorAndTexture.Equals(null))
                {
                    Shader shader = Shader.Find("UI/Default");                      // This shader takes combines the color with the texture
                    mGLMaterialColorAndTexture = new Material(shader);
                }

                return mGLMaterialColorAndTexture;
            }
        }  

        private static Texture2D mDefaultDashedLineTexture;
        /// <summary>
        /// Default pixel texture used for filling dashed shapes
        /// </summary>
        public static Texture2D DefaultDashedLineTexture
        {
            get
            {
                // Generate a single pixel texture if it doesn't exist
                if (!mDefaultDashedLineTexture)
                {
                    mDefaultDashedLineTexture = new Texture2D(16, 2);
                    mDefaultDashedLineTexture.wrapMode = TextureWrapMode.Repeat;
                    Color col = Color.white;
                    for (int i = 0; i < 8; i++)
                    {
                        mDefaultDashedLineTexture.SetPixel(i, 0, col);
                        mDefaultDashedLineTexture.SetPixel(i, 1, col);
                    }
                    col = new Color();
                    for (int i = 8; i < mDefaultDashedLineTexture.width; i++)
                    {
                        mDefaultDashedLineTexture.SetPixel(i, 0, col);
                        mDefaultDashedLineTexture.SetPixel(i, 1, col);
                    }

                    mDefaultDashedLineTexture.Apply();
                }

                return mDefaultDashedLineTexture;
            }
        }

        private static Texture2D mDefaultCircleTexture;
        /// <summary>
        /// Default circle texture used for filling round shapes
        /// </summary>
        public static Texture2D DefaultCircleTexture
        {
            get
            {
                // Generate a single pixel texture if it doesn't exist
                if (!mDefaultCircleTexture)
                    mDefaultCircleTexture = CreateCircleTexture(128);

                return mDefaultCircleTexture;
            }
        }

        private static Texture2D mDefaultTriangleTexture;
        /// <summary>
        /// Default Triangle texture used for filling triangle shapes
        /// </summary>
        public static Texture2D DefaultTriangleTexture
        {
            get
            {
                if (!mDefaultTriangleTexture)
                    mDefaultTriangleTexture = CreateTriangleTexture(64);
                return mDefaultTriangleTexture;
            }
        }

        private static GUIStyle mDefaultTextStyle;
        /// <summary>
        /// Default text style to render text
        /// </summary>
        public static GUIStyle DefaultTextStyle
        {
            get
            {
                if (mDefaultTextStyle == null || mDefaultTextStyle.Equals(null))
                    mDefaultTextStyle = new GUIStyle();
                mDefaultTextStyle.richText = false;
                mDefaultTextStyle.stretchWidth = true;
                mDefaultTextStyle.stretchHeight = true;
                mDefaultTextStyle.wordWrap = false;
                return mDefaultTextStyle;
            }
        }


        #region Utils
        /// <summary>
        /// Loads the appropiate projection matrix
        /// </summary>
        private static void LoadProjectionMatrix()
        {
            if(mUseCustomProjectionMatrix)
                GL.LoadProjectionMatrix(ProjectionMatrix);
            else GL.LoadOrtho();
        }
        /// <summary>
        /// Clears the frame buffer window to the specified color
        /// </summary>
        /// <param name="pClearColor"></param>
        public static void ClearFrameBuffer(Color pClearColor)
        {
            GL.Clear(true, true, pClearColor);
        }
        /// <summary>
        /// Transforms a list of 2D points in screen GUI coordinates, to Vector3 points valid for GL rendering with an Orthogonal projection matrix
        /// </summary>
        /// <param name="pPoints"></param>
        /// <returns></returns>
        private static Vector3[] BuildGLVertexBuffer(IList<Vector2> pPoints)
        {
            Vector3[] retVal = new Vector3[pPoints.Count];
            for (int i = 0; i < pPoints.Count; i++)
                retVal[i] = GUIPointToGLVertex(pPoints[i]);
            return retVal;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pGUIPoint"></param>
        /// <returns></returns>
        private static Vector3 GUIPointToGLVertex(Vector2 pGUIPoint)
        {
            Vector2 pt = GetPosInParent(pGUIPoint);
            return new Vector3(pt.x / ScreenWidth, (ScreenHeight - pt.y) / ScreenHeight, 0);
        }
        /// <summary>
        /// Returns the vertices of a quad that represents a line with thickness
        /// The way GL expects quad vertices is (0,0), (0,1), (1,1), (1,0)
        /// </summary>
        /// <param name="pointA">Vector in screen coords (absolute coords, parents or groups are ignored in this operation)</param>
        /// <param name="pointB">Vector in screen coords (absolute coords, parents or groups are ignored in this operation)</param>
        /// <param name="pLineWidth">Line thickness, in pixels</param>
        /// <returns></returns>
        private static Vector2[] GetLineQuad(Vector2 pointA, Vector2 pointB, float pLineWidth)
        {
            Vector2 axisZ = (pointB - pointA);
            Vector2 axisX = new Vector2(axisZ.y, -axisZ.x) * -1;
            axisX.Normalize();
            float halfWidth = pLineWidth * 0.5f;

            return new Vector2[]
            {
                pointA + (axisX * halfWidth),
                pointA - (axisX * halfWidth),
                pointB - (axisX * halfWidth),
                pointB + (axisX * halfWidth),
            };
        }
        /// <summary>
        /// Sets the material to be used
        /// </summary>
        /// <param name="pOverrideTexture"></param>
        private static void SetMaterial(Texture2D pOverrideTexture = null)
        {
            if (pOverrideTexture == null)
                MaterialColorOnly.SetPass(0);
            else
                MaterialColorAndTexture.SetPass(0);
        }
        #endregion

        #region Parent Bounds
        /// <summary>
        /// Sets the coordinate system for drawing operations so the (0,0) is the top-left corner of the group. 
        /// If pClip is enabled, all controls are clipped to the group. Groups CANNOT be nested         
        /// 
        /// Please note: Do not use GUI.BeginGroup to achieve this, as that might result in a malfunction of the drawing features
        /// </summary>
        /// <param name="pRect">Parent rectangle to use as a reference</param>
        /// <param name="pClip">True to enable clipping to this rectangle</param>
        public static void SetParentBounds(Rect pRect)
        {
            mParentBounds = pRect;
        }
        /// <summary>
        /// Clears any parent bounds or coordinate system. After a call to this method, clipping will be deactivated and all 
        /// coordinates will be relative to screen's origin.
        /// </summary>
        public static void ClearParentBounds()
        {
            mParentBounds = null;
        }
        /// <summary>
        /// Gets the rect relative to parent coords
        /// </summary>
        /// <param name="pRect"></param>
        /// <returns></returns>
        private static Rect GetRectInParent(Rect pRect)
        {
            if (mParentBounds.HasValue)
                return new Rect(pRect.x + mParentBounds.Value.x, pRect.y + mParentBounds.Value.y, pRect.width, pRect.height);
            else return pRect;
        }
        /// <summary>
        /// Gets the Pos relative to parent coords
        /// </summary>
        /// <param name="pPt"></param>
        /// <returns></returns>
        private static Vector2 GetPosInParent(Vector2 pPt)
        {
            if (mParentBounds.HasValue)
                return pPt + mParentBounds.Value.position;
            else return pPt;
        }
        /// <summary>
        /// Returns the world position and size of a UI element (provided its RectTransform)
        /// The returned position corresponds to the Top-Left corner of the element. 
        /// </summary>
        /// <param name="pTransform">RectTransform of the UI Element</param>
        public static Rect GetWorldRect(RectTransform pTransform)
        {
            Vector3[] worldCorners = new Vector3[4];
            pTransform.GetWorldCorners(worldCorners);


            // World corners del RectTransform, en este orden: BL, TL, TR, BR
            // Importante, las Y están expresadas desde abajo (el 0,0 está abajo a la izda), por lo que hay que hacer (Screen.Height - y) si se quiere el origen 
            // de coords arriba a la izda
            Vector2 mTLCorner = new Vector2(worldCorners[1].x, ScreenHeight - worldCorners[1].y);
            float mWidth = worldCorners[2].x - worldCorners[0].x;
            float mHeight = worldCorners[2].y - worldCorners[0].y;

            return new Rect(mTLCorner.x, mTLCorner.y, mWidth, mHeight);
        }
        ///// <summary>
        ///// Returns a rectangle to mimic a line with thickness
        ///// </summary>
        ///// <param name="p1"></param>
        ///// <param name="p2"></param>
        ///// <param name="lineWidth"></param>
        ///// <param name="distance"></param>
        ///// <param name="angle"></param>
        //private static Rect GetLineRect(Vector2 p1, Vector2 p2, float lineWidth, out float distance, out float angle)
        //{
        //    Vector2 pointA = GetPosInParent(p1);
        //    Vector2 pointB = GetPosInParent(p2);

        //    // Determine the angle of the line.
        //    // Vector3.Angle always returns a positive number.
        //    // If pointB is above pointA, then angle needs to be negative.
        //    angle = Vector2.Angle(pointB - pointA, Vector2.right);
        //    if (pointA.y > pointB.y)
        //        angle = -angle;

        //    // Note that the pivot point is at +.5 from pointA.y, this is so that the width of the line
        //    //  is centered on the origin at pointA.
        //    distance = (pointB - pointA).magnitude;
        //    Vector2 size = new Vector2(distance, lineWidth);

        //    // Calculate offsetY, to center in the middle of the rect
        //    float offsetY = 0f;
        //    if (lineWidth > 1)
        //        offsetY = lineWidth / 2f;

        //    // Build rect to be drawn
        //    Rect rect = new Rect(0, -offsetY, size.x, size.y);
        //    return rect;
        //}
        #endregion

        #region Point & Texture
        /// <summary>
        /// Draws a point
        /// </summary>
        /// <param name="pPoint">Coordinates of the point (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="color">Color of the point</param>
        public static void DrawPoint(Vector2 pPoint, Color color)
        {
            DrawPoint(pPoint, color, 1);
        }
        /// <summary>
        /// Draws a point
        /// </summary>
        /// <param name="pPoint">Coordinates of the point (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="color">Color of the point</param>
        /// <param name="pSize">Size of the point, in pixels</param>
        public static void DrawPoint(Vector2 pPoint, Color color, float pSize)
        {
            if (pSize > 1)
            {
                float halfSize = pSize / 2;
                FillRect(new Rect(pPoint.x - halfSize, pPoint.y - halfSize, pSize, pSize), color, null);
            }
            else FillRect(new Rect(pPoint.x, pPoint.y, 1, 1), color, null);
        }
        /// <summary>
        /// Draws a texture
        /// </summary>
        /// <param name="pRect">Rectangle in the screen to draw the texture (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="color">Color that will be multiplied to the texture (white for no effect)</param>
        /// <param name="pTexture">Texture to draw</param>
        public static void DrawTexture(Rect pRect, Color color, Texture2D pTexture)
        {
            FillRect(pRect, color, pTexture);
        }
        #endregion

        #region Line
        /// <summary>
        /// Draws a line between two points. GUI.ContentColor is assumed to draw the line
        /// </summary>
        /// <param name="pointA">First point of the line (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="pointB">Second point of the line (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        public static void DrawLine(Vector2 pointA, Vector2 pointB)
        {
            DrawLine(pointA, pointB, GUI.contentColor, 1.0f, null, null);
        }
        /// <summary>
        /// Draws a line between two points. 
        /// </summary>
        /// <param name="pointA">First point of the line (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="pointB">Second point of the line (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="color">Color of the line</param>
        public static void DrawLine(Vector2 pointA, Vector2 pointB, Color color)
        {
            DrawLine(pointA, pointB, color, 1.0f, null, null);
        }
        /// <summary>
        /// Draws a line between two points. GUI.ContentColor is assumed to draw the line
        /// </summary>
        /// <param name="pointA">First point of the line (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="pointB">Second point of the line (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="width">Line thickness, in pixels</param>
        public static void DrawLine(Vector2 pointA, Vector2 pointB, float width)
        {
            DrawLine(pointA, pointB, GUI.contentColor, width, null, null);
        }
        /// <summary>
        /// Draws a line between two points. 
        /// </summary>
        /// <param name="pointA">First point of the line (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="pointB">Second point of the line (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="color">Color of the line</param>
        /// <param name="width">Line thickness, in pixels</param>
        public static void DrawLine(Vector2 pointA, Vector2 pointB, Color color, float width)
        {
            DrawLine(pointA, pointB, color, width, null, null);
        }
        /// <summary>
        /// Draws a line between two points, using a customized base texture to fill the segment
        /// </summary>
        /// <param name="p1">First point of the line (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="p2">Second point of the line (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="color">Color of the line</param>
        /// <param name="lineWidth">Line thickness, in pixels</param>
        /// <param name="pOverrideTexture">Custom texture to use as base fill (null to use default fill)</param>
        /// <param name="pTilingMultiplier">Number of repetitions of the texture in U,V (null to use default, with 1 repetition in each direction) </param>
        public static void DrawLine(Vector2 p1, Vector2 p2, Color color, float lineWidth, Texture2D pOverrideTexture = null, Vector2? pTilingMultiplier = null)
        {
            // Special case: if line thickness is 1, and there's no override texture, it's faster to use GL.Lines
            if (lineWidth <= 1 && pOverrideTexture == null)
            {
                Vector3[] vb = new Vector3[] { GUIPointToGLVertex(p1), GUIPointToGLVertex(p2) };
                DrawLines(vb, color);
            }
            else
            {
                // Regular case: line thickness > 1, draw using quad
                Vector2[] pts = GetLineQuad(p1, p2, lineWidth);
                FillQuads(pts, color, pOverrideTexture, pTilingMultiplier);
            }
        }
        /// <summary>
        /// Draws lines with thickness == 1, using GUI 2D points 
        /// </summary>
        /// <param name="pPointPairs">Pairs of points, with the start-end points for each line (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="pColor">Color of all lines</param>
        public static void DrawLines(Vector2[] pPointPairs, Color pColor)
        {
            Vector3[] vertexBuffer = BuildGLVertexBuffer(pPointPairs);
            DrawLines(vertexBuffer, pColor);
        }
        /// <summary>
        /// Draws lines with thickness == 1, using OpenGL Vector3 points in homogeneous coords. Please use the BuildGLVertexBuffer method to convert from GUI 
        /// coordinates to OpenGL homogeneous coordinates
        /// </summary>
        /// <param name="vertexBuffer">Pairs of points in homogeneous coordinates, valid for OpenGL rendering with an Ortho projection matrix</param>
        /// <param name="pColor">Color of all lines</param>
        public static void DrawLines(Vector3[] vertexBuffer, Color pColor)
        {
            // If we are currently in the Repaint event, begin to draw a clip of the size of 
            // previously reserved rectangle, and push the current matrix for drawing.
            GL.PushMatrix();
            LoadProjectionMatrix();

            // Clear the current render buffer, setting a new background colour, and set our
            // material for rendering.
            MaterialColorOnly.SetPass(0);

            // Start drawing in OpenGL Lines, to draw the lines of the grid.
            GL.Begin(GL.LINES);
            GL.Color(pColor);

            for (int i = 0; i < vertexBuffer.Length - 1; i += 2)
            {
                GL.Vertex(vertexBuffer[i]);
                GL.Vertex(vertexBuffer[i + 1]);
            }

            // End lines drawing.
            GL.End();

            // Pop the current matrix for rendering, and end the drawing clip.
            GL.PopMatrix();
        }
        ///// <summary>
        ///// Welds segment vertices together to fix joints
        ///// </summary>
        ///// <param name="vertices"></param>
        //private static void FixLineCorners(List<Vector2> vertices)
        //{
        //    for (int i = 0; i <= vertices.Count - 8; i += 4)
        //    {
        //        //      B              C
        //        //      -----------------
        //        //      |      quad i   |/\
        //        //      ----------------/  \
        //        //      A              D \  \
        //        //                        \  \  quad i +1
        //        //                         \  \
        //        //                          \  \
        //        //                           \  /
        //        //                            \/
        //        //                                                   
        //        //
        //        Vector2 quad_i_A = vertices[i];
        //        Vector2 quad_i_B = vertices[i + 1];
        //        Vector2 quad_i_C = vertices[i + 2];
        //        Vector2 quad_i_D = vertices[i + 3];

        //        Vector2 quad_i1_A = vertices[i + 4];
        //        Vector2 quad_i1_B = vertices[i + 5];
        //        Vector2 quad_i1_C = vertices[i + 6];
        //        Vector2 quad_i1_D = vertices[i + 7];

        //        // Modify end vertices of the quad (if it's not the last quad)
        //        if (i < vertices.Count - 4)
        //        {
        //            // Average quad_i->C with quadi+1->B
        //            Vector2 avg = (quad_i_C + quad_i1_B) * 0.5f;
        //            vertices[i + 2] = avg;
        //            vertices[i + 5] = avg;

        //            // Average quad_i->D with quadi+1->!
        //            avg = (quad_i_D + quad_i1_A) * 0.5f;
        //            vertices[i + 3] = avg;
        //            vertices[i + 4] = avg;
        //        }
        //    }

        //}
        /// <summary>
        /// Draws lines, using OpenGL Vector3 points in homogeneous coords. Please use the BuildGLVertexBuffer method to convert from GUI 
        /// coordinates to OpenGL homogeneous coordinates
        /// </summary>
        /// <param name="pPointPairs">Pairs of points, with the start-end points for each line (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="pColor">Color of all lines</param>
        /// <param name="lineWidth">Line thickness, in pixels</param>
        /// <param name="pOverrideTexture">Texture to overlay (null to use only color)</param>
        /// <param name="pTilingMultiplier">Tiling multiplier or null for default tiling (1, 1)</param>
        /// <param name="pFixLineCorners">If true, fixes line corners considering the line to be continuous. Looks better but it's significantly slower (default = true)</param>
        public static void DrawLines(Vector2[] pPointPairs, Color pColor, float lineWidth, Texture2D pOverrideTexture = null, Vector2? pTilingMultiplier = null, bool pFixLineCorners = true)
        {
            Vector2 axisZ, axisX;
            Vector2 pointA, pointB, pointC = Vector2.zero;
            float halfWidth = lineWidth * 0.5f;

            // The length we need to give corners is the Hipotenuse of a triangle where both sides are halfWidth
            //              |\
            //      --------|--
            //              |
            float cornerLen = 0f;
            if (pFixLineCorners)
                cornerLen = new Vector2(halfWidth, halfWidth).magnitude;

            List<Vector2> vertices = new List<Vector2>();
            for (int i = 0; i < pPointPairs.Length - 1; i += 2)
            {
                pointA = pPointPairs[i];
                pointB = pPointPairs[i + 1];

                axisZ = (pointB - pointA).normalized;
                axisX = new Vector2(axisZ.y, -axisZ.x) * -1;

                //   B              C
                //   ----------------
                //   |              |
                //   ----------------
                //   A              D
                Vector2 quadA = Vector2.zero, quadB = Vector2.zero, quadC, quadD;
                if (i == 0)
                {
                    quadA = pointA + (axisX * halfWidth);
                    quadB = pointA - (axisX * halfWidth);
                }
                quadC = pointB - (axisX * halfWidth);
                quadD = pointB + (axisX * halfWidth);

                bool closedShape = false;
                bool isLastSegment = i >= pPointPairs.Length - 3;
                if (pFixLineCorners)
                {
                    //      B              C
                    //      -----------------
                    //      |      quad i   |/\
                    //      ----------------/  \
                    //      A              D \  \
                    //                        \  \  quad i +1
                    //                         \  \
                    //                          \/
                    // If it's not the last segment, fix points C and D according to next segment
                    {
                        //                  
                        //      AxisNextSegment
                        //                   ^   ^ axisAVG
                        // pointA             \ /
                        //   x-----------------x -> axisZ
                        //               pointB \
                        //                       \
                        //                        \
                        //                         x pointC
                        // 
                        // 
                        closedShape = false;
                        if (!isLastSegment)
                            pointC = pPointPairs[i + 3];        // Este es +3 porque los verts vienen por pares. Eso quiere decir que el +2 vuelve a ser el último del segmento anterior
                        else
                        {
                            // If it's the las segment, check if the shape is closed
                            float dist = (pPointPairs[0] - pPointPairs[pPointPairs.Length - 1]).magnitude;
                            if (dist < 0.1f)        // closed shape, can consider 1st point as next segment
                            {
                                pointC = pPointPairs[1];
                                closedShape = true;
                            }
                        }

                        // Fix final points of quad
                        if (!isLastSegment || closedShape)
                        {
                            Vector2 axisZNextSegment = (pointB - pointC).normalized;
                            Vector2 avgAxis = (axisZ + axisZNextSegment);

                            // If axisZ and AxisZNextSegment were exactly the opposite, avgAxis will be zero. In such case, axisX should be used (the perpendicular to axisZ)
                            if (avgAxis.magnitude < 0.01)
                                avgAxis = axisX;
                            else avgAxis.Normalize();


                            if (Vector2.Dot(avgAxis, axisX) < 0)
                                avgAxis *= -1;

                            quadC = pointB - (avgAxis * cornerLen);
                            quadD = pointB + (avgAxis * cornerLen);
                        }
                    }
                }

                // If it's the first segment, add the four vertices. If not, just copy the two first from the previous quad
                if (i == 0)
                {
                    vertices.Add(quadA);
                    vertices.Add(quadB);
                    vertices.Add(quadC);
                    vertices.Add(quadD);
                }
                else
                {
                    int idx = vertices.Count;
                    vertices.Add(vertices[idx - 1]);
                    vertices.Add(vertices[idx - 2]);
                    vertices.Add(quadC);
                    vertices.Add(quadD);
                }

                // If it's the last segment and the shape is closed, fix initial points of quad
                if (isLastSegment && closedShape)
                {
                    vertices[0] = vertices[vertices.Count - 1];
                    vertices[1] = vertices[vertices.Count - 2];
                }
            }

            //for (int i = 0; i < vertices.Count - 4; i += 4)
            //{
            //    if (i % 2 == 0)
            //    {
            //        Drawing2D.DrawText("A", vertices[i + 0].x + 2, vertices[i + 0].y + 2, 12, Color.yellow);
            //        Drawing2D.DrawText("B", vertices[i + 1].x + 2, vertices[i + 1].y + 2, 12, Color.yellow);
            //        Drawing2D.DrawText("C", vertices[i + 2].x + 2, vertices[i + 2].y + 2, 12, Color.yellow);
            //        Drawing2D.DrawText("D", vertices[i + 3].x + 2, vertices[i + 3].y + 2, 12, Color.yellow);
            //    }
            //}

            FillQuads(vertices.ToArray(), pColor, pOverrideTexture, pTilingMultiplier);
        }

        /// <summary>
        /// Draws a dashed line between two points. GUI.ContentColor is assumed to draw the line
        /// </summary>
        /// <param name="pointA">First point of the line (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="pointB">Second point of the line (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        public static void DrawDashedLine(Vector2 pointA, Vector2 pointB)
        {
            DrawDashedLine(pointA, pointB, GUI.contentColor, 1.0f);
        }
        /// <summary>
        /// Draws a dashed line between two points. 
        /// </summary>
        /// <param name="pointA">First point of the line (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="pointB">Second point of the line (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="color">Color of the line</param>
        public static void DrawDashedLine(Vector2 pointA, Vector2 pointB, Color color)
        {
            DrawDashedLine(pointA, pointB, color, 1.0f);
        }
        /// <summary>
        /// Draws a dashed line between two points. GUI.ContentColor is assumed to draw the line
        /// </summary>
        /// <param name="pointA">First point of the line (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="pointB">Second point of the line (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="width">Line thickness, in pixels</param>
        public static void DrawDashedLine(Vector2 pointA, Vector2 pointB, float width)
        {
            DrawDashedLine(pointA, pointB, GUI.contentColor, width);
        }
        /// <summary>
        /// Draws a dashed line between two points.
        /// </summary>
        /// <param name="p1">First point of the line (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="p2">Second point of the line (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="color">Color of the line</param>
        /// <param name="lineWidth">Line thickness, in pixels</param>
        /// <param name="pDashMultiplier">Dash frequency multiplier (min = 0.1, max = 10, default = 3)</param>
        public static void DrawDashedLine(Vector2 p1, Vector2 p2, Color color, float lineWidth, float pDashMultiplier = 3f)
        {
            // Calculate texcoords fo the tiling
            float distance = (p1 - p2).magnitude;
            float numRepetitions = Mathf.Clamp(pDashMultiplier, 0.1f, 10f) * (distance / 60f);
            Vector2 maxUV = new Vector2(numRepetitions, 1);

            Vector2[] pts = GetLineQuad(p1, p2, lineWidth);
            FillQuads(pts, color, DefaultDashedLineTexture, maxUV);
        }
        ///// <summary>
        ///// Draws lines, using OpenGL Vector3 points in homogeneous coords. Please use the BuildGLVertexBuffer method to convert from GUI 
        ///// coordinates to OpenGL homogeneous coordinates
        ///// </summary>
        ///// <param name="pPointPairs">Pairs of points, with the start-end points for each line (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        ///// <param name="pColor">Color of all lines</param>
        ///// <param name="lineWidth">Line thickness, in pixels</param>
        ///// <param name="pOverrideTexture">Texture to overlay (null to use only color)</param>
        ///// <param name="pTilingMultiplier">Tiling multiplier or null for default tiling (1, 1)</param>
        ///// <param name="pFixLineCorners">If true, fixes line corners considering the line to be continuous. Looks better but it's significantly slower (default = true)</param>
        //public static void DrawDashedLines(Vector2[] pPointPairs, Color pColor, float lineWidth, bool pFixLineCorners = true)
        //{
        //    Vector2 pointA, pointB;
        //    float halfWidth = lineWidth * 0.5f;

        //    List<Vector2> vertices = new List<Vector2>();
        //    for (int i = 0; i < pPointPairs.Length - 1; i += 2)
        //    {
        //        pointA = pPointPairs[i];
        //        pointB = pPointPairs[i + 1];
        //        vertices.AddRange(GetLineQuad(pointA, pointB, lineWidth));
        //    }

        //    if (pFixLineCorners)
        //        FixLineCorners(vertices);

        //    FillQuads(vertices.ToArray(), pColor, DefaultDashedLineTexture, pTilingMultiplier);
        //}

        #endregion

        #region Arrow  
        /// <summary>
        /// 
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="color"></param>
        /// <param name="lineWidth"></param>
        /// <param name="pTipWidth"></param>
        /// <param name="pTipLength"></param>
        private static void DrawArrowTip(Vector2 p1, Vector2 p2, Color color, float pTipWidth, float pTipLength)
        {
            Vector2 axisZ = (p2 - p1);
            axisZ.Normalize();
            Vector2 axisX = new Vector2(axisZ.y, -axisZ.x) * -1;
            axisX.Normalize();

            float halfWidth = pTipWidth * 0.5f;

            Vector2 a, b, c;
            a = p2 + (axisX * halfWidth);
            b = p2 - (axisX * halfWidth);
            c = p2 + (axisZ * pTipLength);
            FillTriangle(a, b, c, color, null, null);
        }
        /// <summary>
        /// Draws an arrow
        /// </summary>
        /// <param name="p1">First point of the arrow (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="p2">Second point of the arrow (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="color">Color of the arrow</param>
        /// <param name="lineWidth">Line thickness, in pixels</param>
        /// <param name="pTipWidth">Width of the tip, in pixels</param>
        /// <param name="pTipLength">Length of the tip, in pixels</param>
        public static void DrawArrow(Vector2 p1, Vector2 p2, Color color, float lineWidth, float pTipWidth, float pTipLength)
        {
            DrawLine(p1, p2, color, lineWidth, null);
            DrawArrowTip(p1, p2, color, pTipWidth, pTipLength);
        }
        /// <summary>
        /// Draws an arrow
        /// </summary>
        /// <param name="p1">First point of the arrow (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="p2">Second point of the arrow (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="color">Color of the arrow</param>
        /// <param name="lineWidth">Line thickness, in pixels</param>
        /// <param name="pTipWidth">Width of the tip, in pixels</param>
        /// <param name="pTipLength">Length of the tip, in pixels</param>
        /// <param name="pDashMultiplier">Dash frequency multiplier (min = 0.1, max = 10, default = 3)</param>
        public static void DrawDashedArrow(Vector2 p1, Vector2 p2, Color color, float lineWidth, float pTipWidth, float pTipLength, float pDashMultiplier = 3)
        {
            DrawDashedLine(p1, p2, color, lineWidth, pDashMultiplier);
            DrawArrowTip(p1, p2, color, pTipWidth, pTipLength);
        }
        #endregion

        #region Arc
        /// <summary>
        /// Creates a list of points that builds an arc (plain points, not point pairs)
        /// </summary>
        /// <param name="radius"></param>
        /// <param name="sides"></param>
        /// <param name="startingAngleDeg"></param>
        /// <param name="degrees"></param>
        /// <returns></returns>
        private static List<Vector2> CreateArc(float radius, int sides, float startingAngleDeg, float degrees)
        {
            List<Vector2> list = new List<Vector2>();
            list.AddRange(CreateCircle(sides, true, false));
            list.RemoveAt(list.Count - 1);
            double num = 0.0;
            double num2 = 360.0 / ((double)sides);
            while ((num + (num2 / 2.0)) < startingAngleDeg)
            {
                num += num2;
                list.Add(list[0]);
                list.RemoveAt(0);
            }
            list.Add(list[0]);
            int num3 = (int)((((double)degrees) / num2) + 0.5);
            list.RemoveRange(num3 + 1, (list.Count - num3) - 1);

            return list;
        }
        /// <summary>
        /// Draws a Arc. GUI.ContentColor is assumed to draw the circle
        /// </summary>
        /// <param name="center">Center of the circle (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="radius">Radius of the circle, in Pixels</param>
        /// <param name="sides">Number of sides (more sides, more detail and less performance)</param>
        /// <param name="pStartAngleDeg">Degrees where arc starts (being 0º == 15h in a clock and 270º == 12h) </param>
        /// <param name="pDegrees">Number of degrees to rotate, starting from pStartAngleDeg</param>
        public static void DrawArc(Vector2 center, float radius, int sides, float pStartAngleDeg, float pDegrees)
        {
            DrawArc(center, radius, sides, pStartAngleDeg, pDegrees, GUI.contentColor, 1f, null);
        }
        /// <summary>
        /// Draws a Arc.
        /// </summary>
        /// <param name="center">Center of the circle (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="radius">Radius of the circle, in Pixels</param>
        /// <param name="sides">Number of sides (more sides, more detail and less performance)</param>
        /// <param name="pStartAngleDeg">Degrees where arc starts (being 0º == 15h in a clock and 270º == 12h) </param>
        /// <param name="pDegrees">Number of degrees to rotate, starting from pStartAngleDeg</param>
        /// <param name="color">Color of the circle</param>
        public static void DrawArc(Vector2 center, float radius, int sides, float pStartAngleDeg, float pDegrees, Color color)
        {
            DrawArc(center, radius, sides, pStartAngleDeg, pDegrees, color, 1f, null);
        }
        /// <summary>
        /// Draws a Arc.
        /// </summary>
        /// <param name="center">Center of the circle (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="radius">Radius of the circle, in Pixels</param>
        /// <param name="sides">Number of sides (more sides, more detail and less performance)</param>
        /// <param name="pStartAngleDeg">Degrees where to start the arc, being 0º = 15h on a clock, 90º = 18h, 180º = 21h, etc</param>
        /// <param name="pDegrees">Number of degrees to rotate, starting from pStartAngleDeg</param>
        /// <param name="color">Color of the circle</param>
        /// <param name="lineWidth">Line thickness, in pixels</param>
        public static void DrawArc(Vector2 center, float radius, int sides, float pStartAngleDeg, float pDegrees, Color color, float lineWidth)
        {
            DrawArc(center, radius, sides, pStartAngleDeg, pDegrees, color, lineWidth, null);
        }
        /// <summary>
        /// Draws a Arc.
        /// </summary>
        /// <param name="center">Center of the circle (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="radius">Radius of the circle, in Pixels</param>
        /// <param name="sides">Number of sides (more sides, more detail and less performance)</param>
        /// <param name="pStartAngleDeg">Degrees where to start the arc, being 0º = 15h on a clock, 90º = 18h, 180º = 21h, etc</param>
        /// <param name="pDegrees">Number of degrees to rotate, starting from pStartAngleDeg</param>
        /// <param name="color">Color of the circle</param>
        /// <param name="lineWidth">Line thickness, in pixels</param>
        /// <param name="pLineTexture">Custom base texture to fill the segments (null to use default fill)</param>        
        public static void DrawArc(Vector2 center, float radius, int sides, float pStartAngleDeg, float pDegrees, Color color, float lineWidth, Texture2D pLineTexture)
        {
            IList<Vector2> pts = CreateArc(radius, sides, pStartAngleDeg, pDegrees);

            Vector2[] array = new Vector2[pts.Count];
            for (int i = 0; i < pts.Count; i++)
                array[i] = (pts[i] * radius) + center;

            DrawPolygon(array, color, lineWidth, pLineTexture, true, true);
        }
        /// <summary>
        /// Draws a dashed Arc.
        /// </summary>
        /// <param name="center">Center of the circle (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="radius">Radius of the circle, in Pixels</param>
        /// <param name="sides">Number of sides (more sides, more detail and less performance)</param>
        /// <param name="pStartAngleDeg">Degrees where to start the arc, being 0º = 15h on a clock, 90º = 18h, 180º = 21h, etc</param>
        /// <param name="pDegrees">Number of degrees to rotate, starting from pStartAngleDeg</param>
        /// <param name="color">Color of the circle</param>
        /// <param name="lineWidth">Line thickness, in pixels</param>
        /// <param name="pDashMultiplier">Dash frequency multiplier (min = 0.1, max = 10, default = 3)</param>
        public static void DrawDashedArc(Vector2 center, float radius, int sides, float pStartAngleDeg, float pDegrees, Color color, float lineWidth, float pDashMultiplier = 3)
        {
            IList<Vector2> pts = CreateArc(radius, sides, pStartAngleDeg, pDegrees);

            Vector2[] array = new Vector2[pts.Count];
            for (int i = 0; i < pts.Count; i++)
                array[i] = (pts[i] * radius) + center;

            DrawDashedPolygon(array, color, lineWidth, true, pDashMultiplier);
        }
        #endregion

        #region Circle
        /// <summary>
        /// Returns a plain list of points that build a circle (not point pairs, just points)
        /// </summary>
        /// <param name="sides"></param>
        /// <param name="pSaveCache"></param> 
        /// <returns></returns>
        private static IList<Vector2> CreateCircle(int sides, bool pSaveCache = true, bool pCloseCircle = true)
        {
            if (m_circleCache.ContainsKey(sides))
                return m_circleCache[sides];

            List<Vector2> list = new List<Vector2>();
            float radius = 1;
            double step = 6.2831853071795862 / ((double)sides);
            double num = 0;
            //for (double i = 0.0; i < 6.2831853071795862; i += num)
            for (int i = 0; i < sides; i++)
            {
                list.Add(new Vector2((float)(radius * Math.Cos(num)), (float)(radius * Math.Sin(num))));
                num += step;
            }

            if (pCloseCircle)
                list.Add(new Vector2((float)(radius * Math.Cos(0.0)), (float)(radius * Math.Sin(0.0))));

            if (pSaveCache)
            {
                if (m_circleCache.ContainsKey(sides))
                    m_circleCache.Remove(sides);
                m_circleCache.Add(sides, list);
            }
            return list;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static Texture2D CreateCircleTexture(int pTextureWidthHeight)
        {
            int r = pTextureWidthHeight / 2;
            int cx = r, cy = r;
            Color col = Color.white;

            Texture2D tex = new Texture2D(pTextureWidthHeight, pTextureWidthHeight);

            int x, y, px, nx, py, ny, d;
            Color32[] tempArray = tex.GetPixels32();
            Array.Clear(tempArray, 0, tempArray.Length);        // asegurarme de que inicialmente están a (0, 0, 0, 0)

            // Dibujar círculos concéntricos
            for (x = 0; x < r; x++)
            {
                d = (int)Mathf.Ceil(Mathf.Sqrt(r * r - x * x));
                for (y = 0; y < d; y++)
                {
                    px = cx + x;
                    nx = cx - x;
                    py = cy + y;
                    ny = cy - y;

                    tempArray[(py * pTextureWidthHeight) + px] = col;
                    tempArray[(py * pTextureWidthHeight) + nx] = col;
                    tempArray[(ny * pTextureWidthHeight) + px] = col;
                    tempArray[(ny * pTextureWidthHeight) + nx] = col;
                }
            }
            tex.SetPixels32(tempArray);
            tex.Apply();
            return tex;
        }
        /// <summary>
        /// Draws a circle. GUI.ContentColor is assumed to draw the circle
        /// </summary>
        /// <param name="center">Center of the circle (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="radius">Radius of the circle, in Pixels</param>
        /// <param name="sides">Number of sides (more sides, more detail and less performance)</param>
        public static void DrawCircle(Vector2 center, float radius, int sides)
        {
            DrawCircle(center, radius, sides, GUI.contentColor, 1f, null);
        }
        /// <summary>
        /// Draws a circle.
        /// </summary>
        /// <param name="center">Center of the circle (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="radius">Radius of the circle, in Pixels</param>
        /// <param name="sides">Number of sides (more sides, more detail and less performance)</param>
        /// <param name="color">Color of the circle</param>
        public static void DrawCircle(Vector2 center, float radius, int sides, Color color)
        {
            DrawCircle(center, radius, sides, color, 1f, null);
        }
        /// <summary>
        /// Draws a circle.
        /// </summary>
        /// <param name="center">Center of the circle (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="radius">Radius of the circle, in Pixels</param>
        /// <param name="sides">Number of sides (more sides, more detail and less performance)</param>
        /// <param name="color">Color of the circle</param>
        /// <param name="lineWidth">Line thickness, in pixels</param>
        public static void DrawCircle(Vector2 center, float radius, int sides, Color color, float lineWidth)
        {
            DrawCircle(center, radius, sides, color, lineWidth, null);
        }
        /// <summary>
        /// Draws a circle.
        /// </summary>
        /// <param name="center">Center of the circle (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="radius">Radius of the circle, in Pixels</param>
        /// <param name="sides">Number of sides (more sides, more detail and less performance)</param>
        /// <param name="color">Color of the circle</param>
        /// <param name="lineWidth">Line thickness, in pixels</param>
        /// <param name="pLineTexture">Custom base texture to fill the segments (null to use default fill)</param>        
        public static void DrawCircle(Vector2 center, float radius, int sides, Color color, float lineWidth, Texture2D pLineTexture)
        {
            // Don't close the circle, as DrawPolygon will close it 
            IList<Vector2> pts = CreateCircle(sides, true, false);

            Vector2[] array = new Vector2[pts.Count];
            for (int i = 0; i < pts.Count; i++)
                array[i] = (pts[i] * radius) + center;

            DrawPolygon(array, color, lineWidth, pLineTexture, false, true);
        }
        /// <summary>
        /// Draws a circle.
        /// </summary>
        /// <param name="center">Center of the circle (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="radius">Radius of the circle, in Pixels</param>
        /// <param name="sides">Number of sides (more sides, more detail and less performance)</param>
        /// <param name="color">Color of the circle</param>
        /// <param name="lineWidth">Line thickness, in pixels</param>        
        /// <param name="pDashMultiplier">Dash frequency multiplier (min = 0.1, max = 10, default = 3)</param>
        public static void DrawDashedCircle(Vector2 center, float radius, int sides, Color color, float lineWidth, float pDashMultiplier = 3f)
        {
            // Don't close the circle, as DrawPolygon will close it 
            IList<Vector2> pts = CreateCircle(sides, true, false);

            Vector2[] array = new Vector2[pts.Count];
            for (int i = 0; i < pts.Count; i++)
                array[i] = (pts[i] * radius) + center;

            DrawDashedPolygon(array, color, lineWidth, false, pDashMultiplier);
        }

        /// <summary>
        /// Fills a circle, using GUI.Content color
        /// </summary>
        /// <param name="center">Center of the circle (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="radius">Radius of the circle, in Pixels</param>
        public static void FillCircle(Vector2 center, float radius)
        {
            FillCircle(center, radius, GUI.contentColor, DefaultCircleTexture);
        }
        /// <summary>
        /// Fills a circle
        /// </summary>
        /// <param name="center">Center of the circle (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="radius">Radius of the circle, in Pixels</param>
        /// <param name="color">Color of the circle</param>
        public static void FillCircle(Vector2 center, float radius, Color color)
        {
            FillCircle(center, radius, color, DefaultCircleTexture);
        }
        /// <summary>
        /// Fills a circle
        /// </summary>
        /// <param name="center">Center of the circle (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="radius">Radius of the circle, in Pixels</param>
        /// <param name="color">Color of the circle</param>
        /// <param name="pCircleTexture">Custom base texture to perform the fill (must be round texture to keep the circle shape)</param>
        public static void FillCircle(Vector2 center, float radius, Color color, Texture2D pCircleTexture)
        {
            Vector2 left = Vector2.left * radius;
            Vector2 down = Vector2.down * radius;
            Vector2[] vertices = new Vector2[]
            {
               center + left + down,
               center + left - down,
               center - left - down,
               center - left + down,
            };
            FillQuads(vertices, color, pCircleTexture);
        }
        #endregion

        #region Quad
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCenter"></param>
        /// <param name="pWidth"></param>
        /// <param name="pHeight"></param>
        /// <param name="pOrientationDeg"></param>
        /// <returns></returns>
        private static Vector2[] GetQuadCorners(Vector2 pCenter, float pWidth, float pHeight, float? pOrientationDeg = null)
        {
            float halfWidth = pWidth * 0.5f;
            float halfHeight = pHeight * 0.5f;
            Vector2[] vertices;

            if (pOrientationDeg.HasValue)
            {
                vertices = new Vector2[4]
                {
                    new Vector2(-halfWidth, halfHeight),
                    new Vector2(-halfWidth, -halfHeight),
                    new Vector2(halfWidth, -halfHeight),
                    new Vector2(halfWidth, halfHeight),
                };

                Matrix4x4 mat = Matrix4x4.Rotate(Quaternion.Euler(0, 0, pOrientationDeg.Value));
                for (int i = 0; i < vertices.Length; i++)
                    vertices[i] = (Vector2)mat.MultiplyPoint(vertices[i]) + pCenter;
            }
            else
            {
                vertices = new Vector2[4]
                {
                new Vector2(-halfWidth, halfHeight) + pCenter,
                new Vector2(-halfWidth, -halfHeight) + pCenter,
                new Vector2(halfWidth, -halfHeight) + pCenter,
                new Vector2(halfWidth, halfHeight) + pCenter,
                };
            }
            return vertices;
        }
        /// <summary>
        /// Fills a quad with an arbitrary orientation
        /// </summary>
        /// <param name="pCenter">Center of the quad (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="pWidth">Width of the quad, in pixels</param>
        /// <param name="pHeight">Height of the quad, in pixels</param>
        /// <param name="pColor">Color of the quad</param>
        /// <param name="pLineWidth">Line thickness, in pixels</param>
        /// <param name="pOrientation">Orientation of the quad, in degrees, or null to use default</param>
        /// <param name="pOverrideTexture">Fill texture to overlay, or null to use color only</param>
        public static void DrawQuad(Vector2 pCenter, float pWidth, float pHeight, Color pColor, float pLineWidth, float? pOrientationDeg = null, Texture2D pOverrideTexture = null)
        {
            Vector2[] vertices = GetQuadCorners(pCenter, pWidth, pHeight, pOrientationDeg);
            DrawPolygon(vertices, pColor, pLineWidth, pOverrideTexture, false, true);
        }     
        /// <summary>
        /// Fills a quad with an arbitrary orientation
        /// </summary>
        /// <param name="pCenter">Center of the quad (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="pWidth">Width of the quad, in pixels</param>
        /// <param name="pHeight">Height of the quad, in pixels</param>
        /// <param name="pColor">Color of the quad</param>
        /// <param name="pLineWidth">Line thickness, in pixels</param>
        /// <param name="pOrientation">Orientation of the quad, in degrees, or null to use default</param>
        /// <param name="pDashMultiplier">Dash frequency multiplier (min = 0.1, max = 10, default = 3)</param>
        public static void DrawDashedQuad(Vector2 pCenter, float pWidth, float pHeight, Color pColor, float pLineWidth, float? pOrientationDeg = null, float pDashMultiplier = 3)
        {
            Vector2[] vertices = GetQuadCorners(pCenter, pWidth, pHeight, pOrientationDeg);
            DrawDashedPolygon(vertices, pColor, pLineWidth, false, pDashMultiplier);
        }

        /// <summary>
        /// Fills a quad with an arbitrary orientation
        /// </summary>
        /// <param name="pCenter">Center of the quad (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="pWidth">Width of the quad, in pixels</param>
        /// <param name="pHeight">Height of the quad, in pixels</param>
        /// <param name="pColor">Color of the quad</param>
        /// <param name="pOrientation">Orientation of the quad, in degrees, or null to use default</param>
        /// <param name="pOverrideTexture">Fill texture to overlay, or null to use color only</param>
        /// <param name="pTilingMultiplier">Tiling multiplier or null for default tiling (1, 1)</param>
        public static void FillQuad(Vector2 pCenter, float pWidth, float pHeight, Color pColor, float? pOrientationDeg = null, Texture2D pOverrideTexture = null, Vector2? pTilingMultiplier = null)
        {
            Vector2[] vertices = GetQuadCorners(pCenter, pWidth, pHeight, pOrientationDeg);
            FillQuads(vertices, pColor, pOverrideTexture, pTilingMultiplier);
        }
        /// <summary>
        /// Fills quads defined by their list of vertices. 
        /// Each quad is defined by its 4 vertices in the following order: BL, TL, TR, BR
        /// </summary>
        /// <param name="pVertices">Vertices of the Quads in the following order: BL, TL, TR, BR (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="color">Color of all quads</param>
        public static void FillQuads(Vector2[] pVertices, Color color)
        {
            // If we are currently in the Repaint event, begin to draw a clip of the size of 
            // previously reserved rectangle, and push the current matrix for drawing.
            GL.PushMatrix();
            LoadProjectionMatrix();

            MaterialColorOnly.SetPass(0);

            // Start drawing in OpenGL Quads, to draw the background canvas. Set the
            // colour black as the current OpenGL drawing colour, and draw a quad covering
            // the dimensions of the layoutRectangle.   
            GL.Begin(GL.QUADS);
            GL.Color(color);

            // BL corner 
            for (int i = 0; i <= pVertices.Length - 4; i += 4)
            {
                GL.Vertex(GUIPointToGLVertex(pVertices[i]));
                GL.Vertex(GUIPointToGLVertex(pVertices[i + 1]));
                GL.Vertex(GUIPointToGLVertex(pVertices[i + 2]));
                GL.Vertex(GUIPointToGLVertex(pVertices[i + 3]));
            }

            // End drawing.
            GL.End();

            // Pop the current matrix for rendering, and end the drawing clip.
            GL.PopMatrix();
        }
        /// <summary>
        /// Fills quads defined by their list of vertices. 
        /// Each quad is defined by its 4 vertices in the following order: BL, TL, TR, BR
        /// </summary>
        /// <param name="pVertices">Vertices of the Quad in the following order: BL, TL, TR, BR (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="color">Color of the quad</param>
        /// <param name="pOverrideTexture">Texture to overlay, if any</param>
        /// <param name="pTilingMultiplier">Tiling multiplier or null for default tiling (1, 1)</param>
        public static void FillQuads(Vector2[] pVertices, Color color, Texture2D pOverrideTexture, Vector2? pTilingMultiplier = null)
        {
            Vector2 maxUV = Vector2.one;
            if (pTilingMultiplier.HasValue)
                maxUV = pTilingMultiplier.Value;

            Vector2[] texCoords = new Vector2[pVertices.Length];
            for (int i = 0; i <= pVertices.Length - 4; i += 4)
            {
                texCoords[i] = new Vector2(0, 0);
                texCoords[i + 1] = new Vector2(0f, maxUV.y);
                texCoords[i + 2] = new Vector2(maxUV.x, maxUV.y);
                texCoords[i + 3] = new Vector2(maxUV.x, 0);
            }

            FillQuads(pVertices, texCoords, color, pOverrideTexture);
        }
        /// <summary>
        /// Fills quads defined by their list of vertices
        /// Each quad is defined by its 4 vertices in the following order: BL, TL, TR, BR
        /// </summary>
        /// <param name="pVertices">Vertices of the Quad in the following order: BL, TL, TR, BR (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="pTexCoords">TexCoords of each vertex</param>
        /// <param name="color">Color of the quad</param>
        /// <param name="pOverrideTexture">Texture to overlay, if any</param>
        /// <param name="pTilingMultiplier">Tiling multiplier or null for default tiling (1, 1)</param>
        public static void FillQuads(Vector2[] pVertices, Vector2[] pTexCoords, Color color, Texture2D pOverrideTexture)
        {
            // If we are currently in the Repaint event, begin to draw a clip of the size of 
            // previously reserved rectangle, and push the current matrix for drawing.
            GL.PushMatrix();
            LoadProjectionMatrix();

            MaterialColorAndTexture.mainTexture = pOverrideTexture;
            SetMaterial(pOverrideTexture);

            // Start drawing in OpenGL Quads, to draw the background canvas. Set the
            // colour black as the current OpenGL drawing colour, and draw a quad covering
            // the dimensions of the layoutRectangle.   
            GL.Begin(GL.QUADS);
            GL.Color(color);

            // BL corner 
            for (int i = 0; i <= pVertices.Length - 4; i += 4)
            {
                GL.TexCoord2(pTexCoords[i].x, pTexCoords[i].y);
                GL.Vertex(GUIPointToGLVertex(pVertices[i]));
                // TL corner 
                GL.TexCoord2(pTexCoords[i+1].x, pTexCoords[i+1].y);
                GL.Vertex(GUIPointToGLVertex(pVertices[i + 1]));
                // TR corner 
                GL.TexCoord2(pTexCoords[i+2].x, pTexCoords[i+2].y);
                GL.Vertex(GUIPointToGLVertex(pVertices[i + 2]));
                // BT corner 
                GL.TexCoord2(pTexCoords[i+3].x, pTexCoords[i+3].y);
                GL.Vertex(GUIPointToGLVertex(pVertices[i + 3]));
            }

            // End drawing.
            GL.End();

            // Pop the current matrix for rendering, and end the drawing clip.
            GL.PopMatrix();
        }

        /// <summary>
        /// WIP
        /// </summary>
        /// <param name="previewRect"></param>
        public static void FillQuadFullScreen(Color color)
        {
            GL.PushMatrix();
            LoadProjectionMatrix();

            MaterialColorOnly.SetPass(0);

            GL.Begin(GL.QUADS);
            GL.Color(color);

            GL.Vertex3(0f, 0f, 0f);
            GL.Vertex3(0f, 1f, 0f);
            GL.Vertex3(1f, 1f, 0f);
            GL.Vertex3(1f, 0f, 0f);

            GL.End();
            GL.PopMatrix();
        }
        /// <summary>
        /// WIP
        /// </summary>
        /// <param name="previewRect"></param>
        public static void FillQuadFullScreen(Color color, Texture2D pOverrideTexture = null, Vector2? pTilingMultiplier = null)
        {
            //GL.sRGBWrite = (QualitySettings.activeColorSpace == ColorSpace.Linear);
            GL.PushMatrix();
            LoadProjectionMatrix();

            // Important: set texture before calling to material.SetPass
            MaterialColorAndTexture.mainTexture = pOverrideTexture;
            SetMaterial(pOverrideTexture);

            Vector2 maxUV = Vector2.one;
            if (pTilingMultiplier.HasValue)
                maxUV = pTilingMultiplier.Value;


            GL.Begin(GL.QUADS);
            GL.Color(color);

            GL.TexCoord2(0f, 0f);
            GL.Vertex3(0f, 0f, 0f);

            GL.TexCoord2(0f, maxUV.y);
            GL.Vertex3(0f, 1f, 0f);

            GL.TexCoord2(maxUV.x, maxUV.y);
            GL.Vertex3(1f, 1f, 0f);

            GL.TexCoord2(maxUV.x, 0f);
            GL.Vertex3(1f, 0f, 0f);

            GL.End();
            GL.PopMatrix();
            //GL.sRGBWrite = false;
        }
        #endregion

        #region Rect
        /// <summary>
        /// Draws a rectangle (draws the four sides of it). GUI.ContentColor is assumed
        /// </summary>
        /// <param name="pRect">Rectangle (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        public static void DrawRect(Rect pRect)
        {
            DrawRect(pRect, GUI.contentColor, 1f, null);
        }
        /// <summary>
        /// Draws a rectangle (draws the four sides of it)
        /// </summary>
        /// <param name="pRect">Rectangle (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="color">Color of the rectangle.</param>
        public static void DrawRect(Rect pRect, Color color)
        {
            DrawRect(pRect, color, 1f, null);
        }
        /// <summary>
        /// Draws a rectangle (draws the four sides of it)
        /// </summary>
        /// <param name="pRect">Rectangle (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="color">Color of the rectangle.</param>
        /// <param name="lineWidth">Line thickness, in pixels</param>
        public static void DrawRect(Rect pRect, Color color, float lineWidth)
        {
            DrawRect(pRect, color, lineWidth, null);
        }
        /// <summary>
        /// Draws a rectangle (draws the four sides of it)
        /// </summary>
        /// <param name="pRect">Rectangle (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="color">Color of the rectangle.</param>
        /// <param name="lineWidth">Line thickness, in pixels</param>
        /// <param name="pTexture">Custom texture to use as base fill (null to use default fill)</param>
        public static void DrawRect(Rect pRect, Color color, float lineWidth, Texture2D pTexture)
        {
            // Para que en las esquinas quede bien alineado, a las lineas verticales les sumo la mitad de la anchura de la linea
            Vector2 halfLine = Vector2.zero;
            if (lineWidth > 1)
                halfLine = new Vector2(0f, lineWidth / 2);

            Vector2 BL = new Vector2(pRect.xMin, pRect.yMax);
            Vector2 BR = new Vector2(pRect.xMax, pRect.yMax);

            Vector2 TR = new Vector2(pRect.xMax, pRect.yMin);
            Vector2 TL = new Vector2(pRect.xMin, pRect.yMin);

            DrawLine(TL, TR, color, lineWidth, pTexture);
            DrawLine(BL, BR, color, lineWidth, pTexture);
            DrawLine(TL - halfLine, BL + halfLine, color, lineWidth, pTexture);
            DrawLine(TR - halfLine, BR + halfLine, color, lineWidth, pTexture);
        }
        /// <summary>
        /// Draws a rectangle (draws the four sides of it)
        /// </summary>
        /// <param name="pRect">Rectangle (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="color">Color of the rectangle.</param>
        /// <param name="lineWidth">Line thickness, in pixels</param>
        /// <param name="pDashMultiplier">Dash frequency multiplier (min = 0.1, max = 10, default = 3)</param>
        public static void DrawDashedRect(Rect pRect, Color color, float lineWidth, float pDashMultiplier = 3)
        {
            // Para que en las esquinas quede bien alineado, a las lineas verticales les sumo la mitad de la anchura de la linea
            Vector2 halfLine = Vector2.zero;
            if (lineWidth > 1)
                halfLine = new Vector2(0f, lineWidth / 2);
            Vector2 BL = new Vector2(pRect.xMin, pRect.yMax);
            Vector2 BR = new Vector2(pRect.xMax, pRect.yMax);
            Vector2 TR = new Vector2(pRect.xMax, pRect.yMin);
            Vector2 TL = new Vector2(pRect.xMin, pRect.yMin);

            DrawDashedLine(TL, TR, color, lineWidth, pDashMultiplier);
            DrawDashedLine(TR - halfLine, BR + halfLine, color, lineWidth, pDashMultiplier);
            DrawDashedLine(BL, BR, color, lineWidth, pDashMultiplier);
            DrawDashedLine(TL - halfLine, BL + halfLine, color, lineWidth, pDashMultiplier);
        }

        /// <summary>
        /// Draws Rects using OpenGL, ideal for performance critical situations. 
        /// Please note: Rects drawn using OpenGL are not affected by clipping
        /// </summary>
        /// <param name="pRects">List of rectangles to draw (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="pColor">Color of all rects</param>
        /// <param name="pClearFrameBufferColor">True to clear the whole frame buffer, false otherwise</param>
        public static void DrawRects(IList<Rect> pRects, Color pColor, bool pClearFrameBufferColor = false)
        {
            // If we are currently in the Repaint event, begin to draw a clip of the size of 
            // previously reserved rectangle, and push the current matrix for drawing.
            GL.PushMatrix();

            // Find the "Hidden/Internal-Colored" shader, and cache it for use.
            LoadProjectionMatrix();

            // Clear the current render buffer, setting a new background colour, and set our
            // material for rendering.
            GL.Clear(true, pClearFrameBufferColor, Color.black);

            MaterialColorOnly.SetPass(0);

            // Start drawing in OpenGL Quads, to draw the background canvas. Set the
            // colour black as the current OpenGL drawing colour, and draw a quad covering
            // the dimensions of the layoutRectangle.
            GL.Begin(GL.LINES);
            GL.Color(pColor);

            List<Vector2> pts = new List<Vector2>();
            foreach (Rect r in pRects)
            {
                pts.Add(new Vector2(r.xMin, r.yMin));
                pts.Add(new Vector2(r.xMax, r.yMin));

                pts.Add(new Vector2(r.xMax, r.yMin));
                pts.Add(new Vector2(r.xMax, r.yMax));

                pts.Add(new Vector2(r.xMax, r.yMax));
                pts.Add(new Vector2(r.xMin, r.yMax));

                pts.Add(new Vector2(r.xMin, r.yMax));
                pts.Add(new Vector2(r.xMin, r.yMin));
            }

            Vector3[] vb = BuildGLVertexBuffer(pts);
            for (int i = 0; i < vb.Length - 1; i += 2)
            {
                GL.Vertex(vb[i]);
                GL.Vertex(vb[i + 1]);
            }

            // End lines drawing.
            GL.End();

            // Pop the current matrix for rendering, and end the drawing clip.
            GL.PopMatrix();
        }

        /// <summary>
        /// Fills a rectange. GUI.ContentColor is assumed
        /// </summary>
        /// <param name="pRect">Rectangle (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        public static void FillRect(Rect pRect)
        {
            FillRect(pRect, GUI.contentColor, null);
        }
        /// <summary>
        /// Fills a rectange. 
        /// </summary>
        /// <param name="pRect">Rectangle (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="color">Color of the rectangle</param>
        public static void FillRect(Rect pRect, Color color)
        {
            FillRect(pRect, color, null);
        }
        /// <summary>
        /// Fills a rectange. 
        /// </summary>
        /// <param name="pRectangle">Rectangle (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="pColor">Color of the rectangle</param>
        /// <param name="pTexture">Custom texture to use as base fill (null to use default fill)</param>
        /// <param name="pTilingMultiplier">Number of repetitions of the texture in U,V (null to use default (1, 1)) </param>
        public static void FillRect(Rect pRectangle, Color pColor, Texture2D pTexture = null, Vector2? pTilingMultiplier = null)
        {
            Vector2[] vertices = new Vector2[]
            {
               new Vector2(pRectangle.xMin, pRectangle.yMax),
               new Vector2(pRectangle.xMin, pRectangle.yMin),
               new Vector2(pRectangle.xMax, pRectangle.yMin),
               new Vector2(pRectangle.xMax, pRectangle.yMax),
            };

            FillQuads(vertices, pColor, pTexture, pTilingMultiplier);
        }
        #endregion

        #region Triangle
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static Texture2D CreateTriangleTexture(int pTextureWidthHeight)
        {
            Color col = Color.white;

            Texture2D tex = new Texture2D(pTextureWidthHeight, pTextureWidthHeight);

            int x, y;
            Color32[] tempArray = tex.GetPixels32();
            Array.Clear(tempArray, 0, tempArray.Length);        // asegurarme de que inicialmente están a (0, 0, 0, 0)

            int height = 0;
            for (x = 0; x < pTextureWidthHeight; x++)
            {
                for (y = height; y < pTextureWidthHeight - height; y++)
                {
                    tempArray[(y * pTextureWidthHeight) + x] = col;
                }

                height++;
            }


            tex.SetPixels32(tempArray);
            tex.Apply();
            return tex;
        }
        /// <summary>
        /// Draws a Triangle, defined by 3 points
        /// </summary>
        /// <param name="pA">First point of the triangle (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="pB">Second point of the triangle (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="pC">Third point of the triangle (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="color">Color of the triangle</param>
        /// <param name="lineWidth">Line thickness, in pixels</param>
        public static void DrawTriangle(Vector2 pA, Vector2 pB, Vector2 pC, Color color, float lineWidth)
        {
            DrawTriangle(pA, pB, pC, color, lineWidth, null);
        }
        /// <summary>
        /// Draws a Triangle, defined by 3 points
        /// </summary>
        /// <param name="pA">First point of the triangle (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="pB">Second point of the triangle (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="pC">Third point of the triangle (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="color">Color of the triangle</param>
        /// <param name="lineWidth">Line thickness, in pixels</param>
        /// <param name="pLineTexture">Custom texture to use as base fill (null to use default fill)</param>
        public static void DrawTriangle(Vector2 pA, Vector2 pB, Vector2 pC, Color color, float lineWidth, Texture2D pLineTexture)
        {
            Vector2[] pointPairs = new Vector2[]
            {
                pA, pB,
                pB, pC,
                pC, pA,
            };
            DrawLines(pointPairs, color, lineWidth, pLineTexture, null, true);
        }
        /// <summary>
        /// Draws a Triangle, defined by 3 points
        /// </summary>
        /// <param name="pA">First point of the triangle (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="pB">Second point of the triangle (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="pC">Third point of the triangle (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="color">Color of the triangle</param>
        /// <param name="lineWidth">Line thickness, in pixels</param>
        /// <param name="pDashMultiplier">Dash frequency multiplier (min = 0.1, max = 10, default = 3)</param>
        public static void DrawDashedTriangle(Vector2 pA, Vector2 pB, Vector2 pC, Color color, float lineWidth, float pDashMultiplier = 3)
        {
            DrawDashedLine(pA, pB, color, lineWidth, pDashMultiplier);
            DrawDashedLine(pB, pC, color, lineWidth, pDashMultiplier);
            DrawDashedLine(pC, pA, color, lineWidth, pDashMultiplier);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pA">First point of the triangle (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="pB">Second point of the triangle (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="pC">Third point of the triangle (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="color">Color of the Triangle</param>
        /// <param name="pOverrideTexture">Texture to overlay (null to use color only)</param>
        /// <param name="pTilingMultiplier">Number of repetitions of the texture in U,V (null to use default (1, 1)) </param>
        public static void FillTriangle(Vector2 pA, Vector2 pB, Vector2 pC, Color color, Texture2D pOverrideTexture = null, Vector2? pTilingMultiplier = null)
        {
            // If we are currently in the Repaint event, begin to draw a clip of the size of 
            // previously reserved rectangle, and push the current matrix for drawing.
            GL.PushMatrix();
            LoadProjectionMatrix();

            // Important: set texture before calling to material.SetPass
            MaterialColorAndTexture.mainTexture = pOverrideTexture;
            SetMaterial(pOverrideTexture);

            // Start drawing in OpenGL Quads, to draw the background canvas. Set the
            // colour black as the current OpenGL drawing colour, and draw a quad covering
            // the dimensions of the layoutRectangle.   
            GL.Begin(GL.TRIANGLES);
            GL.Color(color);

            Vector2 maxUV = Vector2.one;
            if (pTilingMultiplier.HasValue)
                maxUV = pTilingMultiplier.Value;

            // BL corner 
            GL.TexCoord2(0f, 0f);
            GL.Vertex(GUIPointToGLVertex(pA));
            // TL corner 
            GL.TexCoord2(maxUV.x, 0);
            GL.Vertex(GUIPointToGLVertex(pB));
            // TR corner 
            GL.TexCoord2(maxUV.x * 0.5f, maxUV.y);
            GL.Vertex(GUIPointToGLVertex(pC));

            // End drawing.
            GL.End();

            // Pop the current matrix for rendering, and end the drawing clip.
            GL.PopMatrix();
        }
        #endregion

        #region Polygons
        /// <summary>
        /// Draws a closed polygon (draws a line between each pair of vertices, in order, and one closing line between the last and the first)
        /// Assumes GUI.ContentColor
        /// </summary>
        /// <param name="pVertices">Array of vertices of the polygon (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        public static void DrawPolygon(Vector2[] pVertices)
        {
            DrawPolygon(pVertices, GUI.contentColor, 1f, null);
        }
        /// <summary>
        /// Draws a closed polygon (draws a line between each pair of vertices, in order, and one closing line between the last and the first)
        /// </summary>
        /// <param name="pVertices">Array of vertices of the polygon (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="color">Color of the polygon</param>
        public static void DrawPolygon(Vector2[] pVertices, Color color)
        {
            DrawPolygon(pVertices, color, 1f, null);
        }
        /// <summary>
        /// Draws a closed polygon (draws a line between each pair of vertices, in order, and one closing line between the last and the first)
        /// </summary>
        /// <param name="pVertices">Array of vertices of the polygon (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="color">Color of the polygon</param>
        /// <param name="lineWidth">Line thickness, in pixels</param>
        public static void DrawPolygon(Vector2[] pVertices, Color color, float lineWidth)
        {
            DrawPolygon(pVertices, color, lineWidth, null);
        }
        /// <summary>
        /// Draws a closed polygon (draws a line between each pair of vertices, in order, and one closing line between the last and the first)
        /// </summary>
        /// <param name="pVertices">Array of vertices of the polygon (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="color">Color of the polygon</param>
        /// <param name="lineWidth">Line thickness, in pixels</param>
        /// <param name="pLineTexture">Custom texture to use as base fill (null to use default fill)</param>
        public static void DrawPolygon(Vector2[] pVertices, Color color, float lineWidth, Texture2D pLineTexture)
        {
            DrawPolygon(pVertices, color, lineWidth, pLineTexture, false);
        }
        /// <summary>
        /// Draws a closed or open polygon (draws a line between each pair of vertices, in order, and if pLeaveOpen is false, draws a 
        /// closing line between the last and the first)
        /// </summary>
        /// <param name="pVertices">Array of vertices of the polygon, plain list, not point pairs (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="color">Color of the polygon</param>
        /// <param name="lineWidth">Line thickness, in pixels</param>
        /// <param name="pLineTexture">Custom texture to use as base fill (null to use default fill)</param>
        /// <param name="pLeaveOpen">True to leave the polygon open. False to close it with a final line between the last and first vertices</param>
        /// <param name="pFixLineCorners">If true, fixes line corners considering the line to be continuous. Looks better but it's significantly slower (default = true)</param>
        public static void DrawPolygon(Vector2[] pVertices, Color color, float lineWidth, Texture2D pLineTexture, bool pLeaveOpen, bool pFixLineCorners = true)
        {
            // DrawLines expects point pairs, not plain vertices. Build pairs now
            List<Vector2> pointPairs = new List<Vector2>();
            for (int i = 0; i < pVertices.Length - 1; i++)
            {
                pointPairs.Add(pVertices[i]);
                pointPairs.Add(pVertices[i + 1]);
            }
            if (!pLeaveOpen)
            {
                pointPairs.Add(pVertices[pVertices.Length - 1]);
                pointPairs.Add(pVertices[0]);
            }

            DrawLines(pointPairs.ToArray(), color, lineWidth, pLineTexture, null, pFixLineCorners);
        }

        /// <summary>
        /// Draws a closed or open, dashed polygon (draws a dashed line between each pair of vertices, in order, and if pLeaveOpen is false, draws a 
        /// closing line between the last and the first)
        /// </summary>
        /// <param name="pVertices">Array of vertices of the polygon (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="color">Color of the polygon</param>
        /// <param name="lineWidth">Line thickness, in pixels</param>
        /// <param name="pLineTexture">Custom texture to use as base fill (null to use default fill)</param>
        /// <param name="pLeaveOpen">True to leave the polygon open. False to close it with a final line between the last and first vertices</param>
        /// <param name="pDashMultiplier">Dash frequency multiplier (min = 0.1, max = 10, default = 3)</param>
        public static void DrawDashedPolygon(Vector2[] pVertices, Color color, float lineWidth, bool pLeaveOpen, float pDashMultiplier = 3f)
        {
            for (int i = 0; i < pVertices.Length - 1; i++)
                DrawDashedLine(pVertices[i], pVertices[i + 1], color, lineWidth, pDashMultiplier);

            if (!pLeaveOpen)
                DrawDashedLine(pVertices[pVertices.Length - 1], pVertices[0], color, lineWidth, pDashMultiplier);
        }

        /// <summary>
        /// Draws a pentagon
        /// </summary>
        /// <param name="center">Center of the pentagon (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="radius">Radius, in pixels</param>
        /// <param name="color">Color of the pentagon</param>
        /// <param name="lineWidth">Line thickness, in pixels</param>
        public static void DrawPentagon(Vector2 center, float radius, Color color, float lineWidth)
        {
            DrawPentagon(center, radius, color, lineWidth, null);
        }
        /// <summary>
        /// Draws a pentagon
        /// </summary>
        /// <param name="center">Center of the pentagon (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="radius">Radius, in pixels</param>
        /// <param name="color">Color of the pentagon</param>
        /// <param name="lineWidth">Line thickness, in pixels</param>
        /// <param name="pLineTexture">Custom texture to use as base fill (null to use default fill)</param>
        public static void DrawPentagon(Vector2 center, float radius, Color color, float lineWidth, Texture2D pLineTexture)
        {
            DrawCircle(center, radius, 5, color, lineWidth, pLineTexture);
        }
        /// <summary>
        /// Draws a dashed pentagon
        /// </summary>
        /// <param name="center">Center of the pentagon (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="radius">Radius, in pixels</param>
        /// <param name="color">Color of the pentagon</param>
        /// <param name="lineWidth">Line thickness, in pixels</param>
        /// <param name="pDashMultiplier">Dash frequency multiplier (min = 0.1, max = 10, default = 3)</param>
        public static void DrawDashedPentagon(Vector2 center, float radius, Color color, float lineWidth, float pDashMultiplier = 3)
        {
            DrawDashedCircle(center, radius, 5, color, lineWidth, pDashMultiplier);
        }

        /// <summary>
        /// Draws a Hexagon
        /// </summary>
        /// <param name="center">Center of the Hexagon (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="radius">Radius, in pixels</param>
        /// <param name="color">Color of the pentagon</param>
        /// <param name="lineWidth">Line thickness, in pixels</param>
        public static void DrawHexagon(Vector2 center, float radius, Color color, float lineWidth)
        {
            DrawHexagon(center, radius, color, lineWidth, null);
        }
        /// <summary>
        /// Draws a Hexagon
        /// </summary>
        /// <param name="center">Center of the Hexagon (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="radius">Radius, in pixels</param>
        /// <param name="color">Color of the pentagon</param>
        /// <param name="lineWidth">Line thickness, in pixels</param>
        /// <param name="pLineTexture">Custom texture to use as base fill (null to use default fill)</param>
        public static void DrawHexagon(Vector2 center, float radius, Color color, float lineWidth, Texture2D pLineTexture)
        {
            DrawCircle(center, radius, 6, color, lineWidth, pLineTexture);
        }
        /// <summary>
        /// Draws a dashed Hexagon
        /// </summary>
        /// <param name="center">Center of the Hexagon (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="radius">Radius, in pixels</param>
        /// <param name="color">Color of the pentagon</param>
        /// <param name="lineWidth">Line thickness, in pixels</param>
        /// <param name="pDashMultiplier">Dash frequency multiplier (min = 0.1, max = 10, default = 3)</param>
        public static void DrawDashedHexagon(Vector2 center, float radius, Color color, float lineWidth, float pDashMultiplier = 3)
        {
            DrawDashedCircle(center, radius, 6, color, lineWidth, pDashMultiplier);
        }
        /// <summary>
        /// Draws a Octogon
        /// </summary>
        /// <param name="center">Center of the Octogon (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="radius">Radius, in pixels</param>
        /// <param name="color">Color of the pentagon</param>
        /// <param name="lineWidth">Line thickness, in pixels</param>
        public static void DrawOctogon(Vector2 center, float radius, Color color, float lineWidth)
        {
            DrawOctogon(center, radius, color, lineWidth, null);
        }
        /// <summary>
        /// Draws a Octogon
        /// </summary>
        /// <param name="center">Center of the Octogon (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="radius">Radius, in pixels</param>
        /// <param name="color">Color of the pentagon</param>
        /// <param name="lineWidth">Line thickness, in pixels</param>
        /// <param name="pLineTexture">Custom texture to use as base fill (null to use default fill)</param>
        public static void DrawOctogon(Vector2 center, float radius, Color color, float lineWidth, Texture2D pLineTexture)
        {
            DrawCircle(center, radius, 8, color, lineWidth, pLineTexture);
        }
        /// <summary>
        /// Draws a dashed Octogon
        /// </summary>
        /// <param name="center">Center of the Octogon (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="radius">Radius, in pixels</param>
        /// <param name="color">Color of the pentagon</param>
        /// <param name="lineWidth">Line thickness, in pixels</param>
        /// <param name="pDashMultiplier">Dash frequency multiplier (min = 0.1, max = 10, default = 3)</param>
        public static void DrawDashedOctogon(Vector2 center, float radius, Color color, float lineWidth, float pDashMultiplier = 3)
        {
            DrawDashedCircle(center, radius, 8, color, lineWidth, pDashMultiplier);
        }
        #endregion

        #region Text
        /// <summary>
        /// Draws text using BitmapFonts (compatible with OnGUI and OnPostRender methods)
        /// </summary>
        /// <param name="pText">Text to draw</param>
        /// <param name="pPos">Coords of the TopLeft corner of the text (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="pFontSize">Font size</param>
        /// <param name="pColor">Color of the text</param>
        /// <param name="pFont">BitmapFont to be used for rendering</param>
        public static void DrawText(string pText, Vector2 pPos, int pFontSize, Color pColor, BitmapFont pFont)
        {
            DrawText(pText, pPos.x, pPos.y, 0, (float)(pFontSize + pFont.FontSizeToPixelsOffset), pColor, pFont);
        }
        /// <summary>
        /// Draws text using BitmapFonts (compatible with OnGUI and OnPostRender methods)
        /// </summary>
        /// <param name="pText">Text to draw</param>
        /// <param name="pX">X coord of the TopLeft corner of the text (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="pY">Y coord of the TopLeft corner of the text (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="pFontSize">Font size</param>
        /// <param name="pColor">Color of the text</param>
        /// <param name="pFont">BitmapFont to be used for rendering</param>
        public static void DrawText(string pText, float pX, float pY, int pFontSize, Color pColor, BitmapFont pFont)
        {
            DrawText(pText, pX, pY, 0, (float)(pFontSize + pFont.FontSizeToPixelsOffset), pColor, pFont);
        }
        /// <summary>
        /// Draws text using BitmapFonts (compatible with OnGUI and OnPostRender methods)
        /// </summary>
        /// <param name="pText">Text to draw</param>
        /// <param name="pX">X coord of the TopLeft corner of the text (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="pY">Y coord of the TopLeft corner of the text (use relative coordinates to parent only if SetParentBounds has been used, use absolute screen coords otherwise)</param>
        /// <param name="pRotationDegs">Rotation of text, in degrees</param>
        /// <param name="pTextWorldSizePixels">Text world height, in pixels</param>
        /// <param name="pColor">Color of the text</param>
        /// <param name="pFont">BitmapFont to be used for rendering</param>
        public static void DrawText(string pText, float pX, float pY, float pRotationDegs, float pTextWorldSizePixels, Color pColor, BitmapFont pFont)
        {                        
            Vector2 pos = GetPosInParent(new Vector2(pX, pY));
            Vector2 axisY = Vector2.up;
            Vector2 axisX = Vector2.right;
            if (pRotationDegs != 0)
            {
                Quaternion orientation = Quaternion.Euler(0, 0, pRotationDegs);
                axisY = orientation * axisY;
                axisX = orientation * axisX;
            }

            Vector2[] verts;
            Vector2[] texCoords;
            pFont.GetTextVertices(pText, pos, axisX, axisY, pTextWorldSizePixels, out verts, out texCoords);           

            FillQuads(verts, texCoords, pColor, pFont.mFontImage);
        }

        /// <summary>
        /// Draws text using GUI Label capabilities and TextStyles (compatible with OnGui methods only)
        /// </summary>
        /// <param name="pText">Text to draw</param>
        /// <param name="pPos">Coords of the TopLeft corner of the text (use relative coordinates to parent only if BeginGroup has been used, use absolute screen coords otherwise)</param>
        /// <param name="pFontSize">Font size</param>
        /// <param name="pColor">Color of the text</param>
        public static void DrawGUIText(string pText, Vector2 pPos, int pFontSize, Color pColor)
        {
            DrawGUIText(pText, pPos.x, pPos.y, pFontSize, pColor);
        }
        /// <summary>
        /// Draws text using GUI Label capabilities and TextStyles (compatible with OnGui methods only)
        /// </summary>
        /// <param name="pText">Text to draw</param>
        /// <param name="pX">X coord of the TopLeft corner of the text (use relative coordinates to parent only if BeginGroup has been used, use absolute screen coords otherwise)</param>
        /// <param name="pY">Y coord of the TopLeft corner of the text (use relative coordinates to parent only if BeginGroup has been used, use absolute screen coords otherwise)</param>
        /// <param name="pFontSize">Font size</param>
        /// <param name="pColor">Color of the text</param>
        public static void DrawGUIText(string pText, float pX, float pY, int pFontSize, Color pColor)
        {
            DefaultTextStyle.fontSize = pFontSize;
            DefaultTextStyle.normal.textColor = pColor;
            Vector2 pt = GetPosInParent(new Vector2(pX, pY));
            GUI.Label(new Rect(pt.x, pt.y, 1, 1), pText, DefaultTextStyle);
        }
        /// <summary>
        /// Draws text using GUI Label capabilities and TextStyles (compatible with OnGui methods only)
        /// </summary>
        /// <param name="pText">Text to draw</param>
        /// <param name="pX">X coord of the TopLeft corner of the text (use relative coordinates to parent only if BeginGroup has been used, use absolute screen coords otherwise)</param>
        /// <param name="pY">Y coord of the TopLeft corner of the text (use relative coordinates to parent only if BeginGroup has been used, use absolute screen coords otherwise)</param>
        /// <param name="pTextStyle">Custom Text Style</param>
        public static void DrawGUIText(string pText, float pX, float pY, GUIStyle pTextStyle)
        {
            Vector2 pt = GetPosInParent(new Vector2(pX, pY));
            GUI.Label(new Rect(pt.x, pt.y, 1, 1), pText, pTextStyle);
        }
        #endregion
    }
}