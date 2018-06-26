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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Xml.Serialization;

namespace GraphicDNA
{
    /// <summary>
    /// Bitmap font class
    /// Uses an input Image file with the atlas of characters, and an XML file with the configuration in AngelCode .fnt format (compatible
    /// with AngelCode BMFont, ShoeBox, Hiero, etc).
    /// Credits for some parts of the code: http://www.angelcode.com/ 
    /// </summary>
    [Serializable]
    [XmlRoot("font")]
    public class BitmapFont 
    {
        [XmlElement("info")]
        public FontInfo Info
        {
            get;
            set;
        }

        [XmlElement("common")]
        public FontCommon Common
        {
            get;
            set;
        }

        [XmlArray("pages")]
        [XmlArrayItem("page")]
        public List<FontPage> Pages
        {
            get;
            set;
        }

        [XmlArray("chars")]
        [XmlArrayItem("char")]
        public List<FontChar> Chars
        {
            get;
            set;
        }
        private Dictionary<char, FontChar> mCharsDictionary = new Dictionary<char, FontChar>();

        [XmlArray("kernings")]
        [XmlArrayItem("kerning")]
        public List<FontKerning> Kernings
        {
            get;
            set;
        }
        [XmlIgnore]
        public Texture2D mFontImage;
        /// <summary>
        /// FontSizes differ slightly from pixel sizes, this parameter allows to adjust this offset. A value of 2 means that a fontSize of 18
        /// will be treated internally as 20.
        /// </summary>
        public int FontSizeToPixelsOffset = 2;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pFontConfigXmlFile"></param>
        /// <param name="pFontImage"></param>
        /// <returns></returns>
        public static BitmapFont FromXml(TextAsset pFontConfigXmlFile, Texture2D pFontImage)
        {
            return BitmapFont.FromXml(pFontConfigXmlFile.bytes, pFontImage);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pXmlStr"></param>
        private static BitmapFont FromXml(byte[] pData, Texture2D pFontImage)
        {            
            System.Xml.Serialization.XmlSerializer deserializer = new System.Xml.Serialization.XmlSerializer(typeof(BitmapFont));
            System.IO.MemoryStream strm = new System.IO.MemoryStream(pData);
            System.IO.TextReader textReader = new System.IO.StreamReader(strm);
            BitmapFont file = (BitmapFont)deserializer.Deserialize(textReader);
            file.mFontImage = pFontImage;

            file.mCharsDictionary.Clear();
            foreach(FontChar chr in file.Chars)
            {
                char c = Convert.ToChar(chr.ID);
                chr.Init(file.Info.Size, pFontImage.width, pFontImage.height, c);
                file.mCharsDictionary.Add(c, chr);
            }

            textReader.Close();
            return file;
        }

        ///// <summary>
        ///// Builds the 3D vertices and texture coords needed to draw text using a BitmapFont
        ///// </summary>
        ///// <param name="pText">Text to be drawn</param>
        ///// <param name="pos">Position of the lower left corner of the text, in 3D</param>
        ///// <param name="axisX">Right Axis of the text</param>
        ///// <param name="axisY">Up Axis of the text</param>
        ///// <param name="pTextHeightWorldCoords">Text height in world coords</param>
        ///// <param name="verts"></param>
        ///// <param name="texCoords"></param>
        //public void GetTextVertices(string pText, Vector3 pTextPosition, Vector3 axisX, Vector3 axisY, float pTextHeightWorldCoords, out Vector3[] verts, out Vector2[] texCoords)
        //{
        //    verts = new Vector3[pText.Length * 4];
        //    texCoords = new Vector2[pText.Length * 4];

        //    int vIdx = 0;
        //    int cIdx = 0;
        //    Vector3 pos = pTextPosition;
        //    foreach (char chr in pText)
        //    {
        //        if (!mCharsDictionary.ContainsKey(chr))
        //            continue;

        //        FontChar info = mCharsDictionary[chr];

        //        // Vertex order is: BL, TL, TR, BR
        //        verts[vIdx++] = pos;
        //        verts[vIdx++] = pos + (axisY * info.RelativeSize.y * pTextHeightWorldCoords);
        //        verts[vIdx++] = pos + (axisY * info.RelativeSize.y * pTextHeightWorldCoords) + (axisX * info.RelativeSize.x * pTextHeightWorldCoords);
        //        verts[vIdx++] = pos + (axisX * info.RelativeSize.x * pTextHeightWorldCoords);
        //        pos += axisX * info.RelativeAdvanceInX * pTextHeightWorldCoords;

        //        texCoords[cIdx++] = new Vector2(info.PositionUV.x, 1 - (info.PositionUV.y + info.SizeUV.y));
        //        texCoords[cIdx++] = new Vector2(info.PositionUV.x, 1 - info.PositionUV.y);
        //        texCoords[cIdx++] = new Vector2(info.PositionUV.x + info.SizeUV.x, 1 - (info.PositionUV.y));
        //        texCoords[cIdx++] = new Vector2(info.PositionUV.x + info.SizeUV.x, 1 - (info.PositionUV.y + info.SizeUV.y));
        //    }
        //}
        /// <summary>
        /// Builds the 3D vertices and texture coords needed to draw text in 2D, using a BitmapFont
        /// </summary>
        /// <param name="pText">Text to be drawn</param>
        /// <param name="pos">Position of the lower left corner of the text, in 2D</param>
        /// <param name="axisX">Right Axis of the text</param>
        /// <param name="axisY">Up Axis of the text</param>
        /// <param name="pTextHeightWorldCoords">Text height in world coords</param>
        /// <param name="verts"></param>
        /// <param name="texCoords"></param>
        public void GetTextVertices(string pText, Vector3 pTextPosition, Vector3 axisX, Vector3 axisY, float pTextHeightWorldCoords, out Vector3[] verts, out Vector2[] texCoords)
        {
            verts = new Vector3[pText.Length * 4];
            texCoords = new Vector2[pText.Length * 4];

            int vIdx = 0;
            int cIdx = 0;
            Vector3 pos = pTextPosition;
            foreach (char chr in pText)
            {
                if (!mCharsDictionary.ContainsKey(chr))
                    continue;

                FontChar info = mCharsDictionary[chr];

                Vector3 offset = axisX * (info.RelativeOffset.x * pTextHeightWorldCoords) + axisY * (info.RelativeOffset.y * pTextHeightWorldCoords);

                // Here, vertex order is inverted on purpose: instead of BL, TL, TR, BR, and because 2D coords have the origin in the bottom 
                // left corner, the order is set to TL, BL, BR, TR, to invert vertices vertically
                pos += offset;
                verts[vIdx++] = (pos) + (axisY * info.RelativeSize.y * pTextHeightWorldCoords);
                verts[vIdx++] = (pos);
                verts[vIdx++] = (pos) + (axisX * info.RelativeSize.x * pTextHeightWorldCoords);
                verts[vIdx++] = (pos) + (axisY * info.RelativeSize.y * pTextHeightWorldCoords) + (axisX * info.RelativeSize.x * pTextHeightWorldCoords);
                pos -= offset;

                // Apply advance to next char
                pos += axisX * info.RelativeAdvanceInX * pTextHeightWorldCoords;

                texCoords[cIdx++] = new Vector2(info.PositionUV.x, 1 - (info.PositionUV.y + info.SizeUV.y));
                texCoords[cIdx++] = new Vector2(info.PositionUV.x, 1 - info.PositionUV.y);
                texCoords[cIdx++] = new Vector2(info.PositionUV.x + info.SizeUV.x, 1 - (info.PositionUV.y));
                texCoords[cIdx++] = new Vector2(info.PositionUV.x + info.SizeUV.x, 1 - (info.PositionUV.y + info.SizeUV.y));
            }
        }


        /// <summary>
        /// Builds the 2D vertices and texture coords needed to draw text in 2D, using a BitmapFont
        /// </summary>
        /// <param name="pText">Text to be drawn</param>
        /// <param name="pos">Position of the lower left corner of the text, in 2D</param>
        /// <param name="axisX">Right Axis of the text</param>
        /// <param name="axisY">Up Axis of the text</param>
        /// <param name="pTextHeightWorldCoords">Text height in world coords</param>
        /// <param name="verts"></param>
        /// <param name="texCoords"></param>
        public void GetTextVertices(string pText, Vector2 pTextPosition, Vector2 axisX, Vector2 axisY, float pTextHeightWorldCoords, out Vector2[] verts, out Vector2[] texCoords)
        {
            verts = new Vector2[pText.Length * 4];
            texCoords = new Vector2[pText.Length * 4];

            int vIdx = 0;
            int cIdx = 0;
            Vector2 pos = pTextPosition;
            foreach (char chr in pText)
            {
                if (!mCharsDictionary.ContainsKey(chr))
                    continue;

                FontChar info = mCharsDictionary[chr];

                // Vertex order is: BL, TL, TR, BR
                Vector2 offset = axisX * (info.RelativeOffset.x * pTextHeightWorldCoords) + axisY * (info.RelativeOffset.y * pTextHeightWorldCoords);

                // Here, vertex order is inverted on purpose: instead of BL, TL, TR, BR, and because 2D coords have the origin in the bottom 
                // left corner, the order is set to TL, BL, BR, TR, to invert vertices vertically
                pos += offset;
                verts[vIdx++] = (pos) + (axisY * info.RelativeSize.y * pTextHeightWorldCoords);
                verts[vIdx++] = (pos);
                verts[vIdx++] = (pos) + (axisX * info.RelativeSize.x * pTextHeightWorldCoords);
                verts[vIdx++] = (pos) + (axisY * info.RelativeSize.y * pTextHeightWorldCoords) + (axisX * info.RelativeSize.x * pTextHeightWorldCoords);
                pos -= offset;

                // Apply advance to next char
                pos += axisX * info.RelativeAdvanceInX * pTextHeightWorldCoords;
                
                texCoords[cIdx++] = new Vector2(info.PositionUV.x, 1 - (info.PositionUV.y + info.SizeUV.y));
                texCoords[cIdx++] = new Vector2(info.PositionUV.x, 1 - info.PositionUV.y);
                texCoords[cIdx++] = new Vector2(info.PositionUV.x + info.SizeUV.x, 1 - (info.PositionUV.y));
                texCoords[cIdx++] = new Vector2(info.PositionUV.x + info.SizeUV.x, 1 - (info.PositionUV.y + info.SizeUV.y));
            }
        }

    }


    [Serializable]
    public class FontInfo
    {
        private RectInt _Padding;
        private Vector2Int _Spacing;

        /// <summary>
        /// This is the name of the true type font.
        /// </summary>
        [XmlAttribute("face")]
        public String Face
        {
            get;
            set;
        }
        /// <summary>
        /// The size of the true type font.
        /// </summary>
        [XmlAttribute("size")]
        public Int32 Size
        {
            get;
            set;
        }

        [XmlAttribute("bold")]
        public Int32 Bold
        {
            get;
            set;
        }

        [XmlAttribute("italic")]
        public Int32 Italic
        {
            get;
            set;
        }
        /// <summary>
        /// The name of the OEM charset used (when not unicode).
        /// </summary>
        [XmlAttribute("charset")]
        public String CharSet
        {
            get;
            set;
        }
        /// <summary>
        /// Set to 1 if it is the unicode charset.
        /// </summary>
        [XmlAttribute("unicode")]
        public Int32 Unicode
        {
            get;
            set;
        }
        /// <summary>
        /// The font height stretch in percentage. 100% means no stretch.
        /// </summary>
        [XmlAttribute("stretchH")]
        public Int32 StretchHeight
        {
            get;
            set;
        }
        /// <summary>
        /// Set to 1 if smoothing was turned on.
        /// </summary>
        [XmlAttribute("smooth")]
        public Int32 Smooth
        {
            get;
            set;
        }
        /// <summary>
        /// The supersampling level used. 1 means no supersampling was used.
        /// </summary>
        [XmlAttribute("aa")]
        public Int32 SuperSampling
        {
            get;
            set;
        }
        /// <summary>
        /// The padding for each character (up, right, down, left).
        /// </summary>
        [XmlAttribute("padding")]
        public String Padding
        {
            get
            {
                return ((int)_Padding.x) + "," + ((int)_Padding.y) + "," + ((int)_Padding.width) + "," + ((int)_Padding.height);
            }
            set
            {
                String[] padding = value.Split(',');
                _Padding = new RectInt(Convert.ToInt32(padding[0]), Convert.ToInt32(padding[1]), Convert.ToInt32(padding[2]), Convert.ToInt32(padding[3]));
            }
        }
        /// <summary>
        /// The spacing for each character (horizontal, vertical).
        /// </summary>
        [XmlAttribute("spacing")]
        public String Spacing
        {
            get
            {
                return _Spacing.x + "," + _Spacing.y;
            }
            set
            {
                String[] spacing = value.Split(',');
                _Spacing = new Vector2Int(Convert.ToInt32(spacing[0]), Convert.ToInt32(spacing[1]));
            }
        }
        /// <summary>
        /// The outline thickness for the characters.
        /// </summary>
        [XmlAttribute("outline")]
        public Int32 OutLine
        {
            get;
            set;
        }
    }

    [Serializable]
    public class FontCommon
    {
        /// <summary>
        /// This is the distance in pixels between each line of text.
        /// </summary>
        [XmlAttribute("lineHeight")]
        public Int32 LineHeight
        {
            get;
            set;
        }
        /// <summary>
        /// The number of pixels from the absolute top of the line to the base of the characters.
        /// </summary>
        [XmlAttribute("base")]
        public Int32 Base
        {
            get;
            set;
        }
        /// <summary>
        /// The width of the texture, normally used to scale the x pos of the character image.
        /// </summary>
        [XmlAttribute("scaleW")]
        public Int32 ScaleW
        {
            get;
            set;
        }
        /// <summary>
        /// The height of the texture, normally used to scale the y pos of the character image.
        /// </summary>
        [XmlAttribute("scaleH")]
        public Int32 ScaleH
        {
            get;
            set;
        }
        /// <summary>
        /// The number of texture pages included in the font.
        /// </summary>
        [XmlAttribute("pages")]
        public Int32 Pages
        {
            get;
            set;
        }
        /// <summary>
        /// Set to 1 if the monochrome characters have been packed into each of the texture channels. In this case alphaChnl describes what is stored in each channel.
        /// </summary>
        [XmlAttribute("packed")]
        public Int32 Packed
        {
            get;
            set;
        }
        /// <summary>
        /// Set to 0 if the channel holds the glyph data, 1 if it holds the outline, 2 if it holds the glyph and the outline, 3 if its set to zero, and 4 if its set to one.
        /// </summary>
        [XmlAttribute("alphaChnl")]
        public Int32 AlphaChannel
        {
            get;
            set;
        }
        /// <summary>
        /// Set to 0 if the channel holds the glyph data, 1 if it holds the outline, 2 if it holds the glyph and the outline, 3 if its set to zero, and 4 if its set to one.
        /// </summary>
        [XmlAttribute("redChnl")]
        public Int32 RedChannel
        {
            get;
            set;
        }
        /// <summary>
        /// Set to 0 if the channel holds the glyph data, 1 if it holds the outline, 2 if it holds the glyph and the outline, 3 if its set to zero, and 4 if its set to one.
        /// </summary>
        [XmlAttribute("greenChnl")]
        public Int32 GreenChannel
        {
            get;
            set;
        }
        /// <summary>
        /// Set to 0 if the channel holds the glyph data, 1 if it holds the outline, 2 if it holds the glyph and the outline, 3 if its set to zero, and 4 if its set to one.
        /// </summary>
        [XmlAttribute("blueChnl")]
        public Int32 BlueChannel
        {
            get;
            set;
        }
    }

    [Serializable]
    public class FontPage
    {
        /// <summary>
        /// Page Id
        /// </summary>
        [XmlAttribute("id")]
        public Int32 ID
        {
            get;
            set;
        }
        /// <summary>
        /// The texture file name.
        /// </summary>
        [XmlAttribute("file")]
        public String File
        {
            get;
            set;
        }
    }

    [Serializable]
    public class FontChar
    {
        /// <summary>
        /// Character ID according to ASCII convention
        /// </summary>
        [XmlAttribute("id")]
        public Int32 ID
        {
            get;
            set;
        }
        /// <summary>
        /// The left position of the character image in the texture.
        /// </summary>
        [XmlAttribute("x")]
        public Int32 X
        {
            get;
            set;
        }
        /// <summary>
        /// The top position of the character image in the texture.
        /// </summary>
        [XmlAttribute("y")]
        public Int32 Y
        {
            get;
            set;
        }
        /// <summary>
        /// The width of the character image in the texture.
        /// </summary>
        [XmlAttribute("width")]
        public Int32 Width
        {
            get;
            set;
        }
        /// <summary>
        /// The height of the character image in the texture.
        /// </summary>
        [XmlAttribute("height")]
        public Int32 Height
        {
            get;
            set;
        }
        /// <summary>
        /// How much the current position should be offset when copying the image from the texture to the screen
        /// </summary>
        [XmlAttribute("xoffset")]
        public Int32 XOffset
        {
            get;
            set;
        }
        /// <summary>
        /// How much the current position should be offset when copying the image from the texture to the screen
        /// </summary>
        [XmlAttribute("yoffset")]
        public Int32 YOffset
        {
            get;
            set;
        }
        /// <summary>
        /// How much the current position should be advanced after drawing the character.
        /// </summary>
        [XmlAttribute("xadvance")]
        public Int32 XAdvance
        {
            get;
            set;
        }
        /// <summary>
        /// The texture page where the character image is found.
        /// </summary>
        [XmlAttribute("page")]
        public Int32 Page
        {
            get;
            set;
        }
        /// <summary>
        /// The texture channel where the character image is found (1 = blue, 2 = green, 4 = red, 8 = alpha, 15 = all channels).
        /// </summary>
        [XmlAttribute("chnl")]
        public Int32 Channel
        {
            get;
            set;
        }

        /// <summary>
        /// Original Font Size
        /// </summary>
        [XmlIgnore]
        public int OriginalFontSize;
        /// <summary>
        /// Size of the texture atlas that holds all characters, in Pixels
        /// </summary>
        [XmlIgnore]
        public Vector2 TextureSize;
        /// <summary>
        /// Character to be printed
        /// </summary>
        [XmlIgnore]
        public char Char;
        /// <summary>
        /// UV Texture coords of the char within the texture atlas
        /// </summary>
        [XmlIgnore]
        public Vector2 PositionUV;
        /// <summary>
        /// Size of the char, relative to the FontSize (in percent, 0..1)
        /// </summary>
        [XmlIgnore]
        public Vector2 RelativeSize;
        /// <summary>
        /// How much the current position should be offset when copying the image from the texture to the screen, in relative coords to fontsize (0..1)
        /// </summary>
        [XmlIgnore]
        public Vector2 RelativeOffset;
        /// <summary>
        /// Size of the char in TextureCoords
        /// </summary>
        [XmlIgnore]
        public Vector2 SizeUV;
        /// <summary>
        /// Advance in the X direction, in Coords relative to the font size
        /// </summary>
        [XmlIgnore]
        public float RelativeAdvanceInX;

        /// <summary>
        /// Preprocess some character data for rendering
        /// </summary>
        /// <param name="pFontSize">Size of the font</param>
        /// <param name="pTextureWidth">Width of the texture atlas</param>
        /// <param name="pTextureHeight">Height of the texture atlas</param>
        /// <param name="pChr">Character of this char</param>
        public void Init(int pFontSize, int pTextureWidth, int pTextureHeight, char pChr)
        {
            OriginalFontSize = pFontSize;
            TextureSize = new Vector2(pTextureWidth, pTextureHeight);
            this.Char = pChr;
            this.PositionUV = new Vector2((float)this.X / TextureSize.x, (float)this.Y / TextureSize.y);
            this.SizeUV = new Vector2((float)this.Width / TextureSize.x, (float)this.Height / TextureSize.y);
            this.RelativeSize = new Vector2((float)this.Width / (float)pFontSize, (float)this.Height / (float)pFontSize);
            this.RelativeOffset = new Vector2((float)this.XOffset / (float)pFontSize, (float)this.YOffset / (float)pFontSize);
            this.RelativeAdvanceInX = (float)this.XAdvance / (float)pFontSize;
        }


    }

    [Serializable]
    public class FontKerning
    {
        /// <summary>
        /// First char in the pair
        /// </summary>
        [XmlAttribute("first")]
        public Int32 First
        {
            get;
            set;
        }
        /// <summary>
        /// Second char in the pair
        /// </summary>
        [XmlAttribute("second")]
        public Int32 Second
        {
            get;
            set;
        }
        /// <summary>
        /// How much the x position should be adjusted when drawing the second character immediately following the first.
        /// </summary>
        [XmlAttribute("amount")]
        public Int32 Amount
        {
            get;
            set;
        }
    }
}