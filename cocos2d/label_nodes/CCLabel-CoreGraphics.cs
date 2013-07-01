using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;
using System.Runtime.InteropServices;

#if MONOMAC
using MonoMac.CoreGraphics;
using MonoMac.AppKit;
using MonoMac.Foundation;
using MonoMac.CoreText;
using MonoMac.ImageIO;

#else
using MonoTouch.CoreGraphics;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using MonoTouch.CoreText;
using MonoTouch.ImageIO;
#endif

namespace Cocos2D
{
	public partial class CCLabel
	{

		private static CTFont _font;
		private static CGBitmapContext _bitmap;
		private static IntPtr _bitmapData;
		private static CCColor4B _brush;
		private static Dictionary<char, KerningInfo> _abcValues = new Dictionary<char, KerningInfo>();

		private void CreateFont(string fontName, float fontSize, CCRawList<char> charset)
		{

			_font = CCLabelUtilities.CreateFont (fontName, fontSize);

			var value = new CCLabelUtilities.ABCFloat[1];

			_abcValues.Clear();;

			for (int i = 0; i < charset.Count; i++)
			{
				var ch = charset[i];
				CCLabelUtilities.GetCharABCWidthsFloat(ch, _font, out value);
				_abcValues.Add(ch, new KerningInfo() { A = value[0].abcfA, B = value[0].abcfB, C = value[0].abcfC });
			}

		}

		private float GetFontHeight()
		{
			return _font.GetHeight();
		}

		private CCSize GetMeasureString(string text)
		{
			return CCLabelUtilities.MeasureString(text, _font);
		}

		private KerningInfo GetKerningInfo(char ch)
		{
			return _abcValues[ch];
		}

		private void CreateBitmap(int width, int height)
		{
			if (_bitmap == null || (_bitmap.Width < width || _bitmap.Height < height))
			{

				_bitmap = CCLabelUtilities.CreateBitmap (width, height);
			}

			//if (_brush == null)
			//{
				_brush = new CCColor4B(Microsoft.Xna.Framework.Color.White);
			//}
		}

		private unsafe byte* GetBitmapData(string s, out int stride)
		{

			var size = GetMeasureString(s);

			var w = (int)(Math.Ceiling(size.Width += 2));
			var h = (int)(Math.Ceiling(size.Height += 2));

			CreateBitmap(w, h);

			CCLabelUtilities.NativeDrawString(_bitmap, s, _font, _brush, new RectangleF(0,0,w,h));

			_bitmapData = _bitmap.Data;

			stride = _bitmap.BytesPerRow;

			return (byte*)_bitmapData;
		}

//		public static CCBMFontConfiguration InitializeFont(string fontName, float fontSize, string charset)
//		{
//			if (m_pData == null)
//			{
//				InitializeTTFAtlas(1024, 1024);
//			}
//
//			if (String.IsNullOrEmpty(charset))
//			{
//				charset = " ";
//			}
//
//			var chars = new CCRawList<char>();
//
//			var fontKey = GetFontKey(fontName, fontSize);
//
//			CCBMFontConfiguration fontConfig;
//
//			if (!s_pConfigurations.TryGetValue(fontKey, out fontConfig))
//			{
//				fontConfig = new CCBMFontConfiguration();
//				s_pConfigurations.Add(fontKey, fontConfig);
//			}
//
//			for (int i = 0; i < charset.Length; i++)
//			{
//				var ch = charset[i];
//				if (!fontConfig.m_pFontDefDictionary.ContainsKey(ch) && chars.IndexOf(ch) == -1)
//				{
//					chars.Add(ch);
//				}
//			}
//
//			if (chars.Count == 0)
//			{
//				return fontConfig;
//			}
//
//			var font = CCLabelUtilities.CreateFont(fontName, fontSize);
//
//			fontConfig.m_nCommonHeight = (int)Math.Ceiling(font.GetHeight());
//
//
//			var bitmap = CCLabelUtilities.CreateBitmap(fontConfig.m_nCommonHeight * 4, fontConfig.m_nCommonHeight * 2);
//
//			var data = new int[bitmap.Width * bitmap.Height];
//
//
//
//			var brush = new CCColor4B (Microsoft.Xna.Framework.Color.White);
//
//			CCLabelUtilities.ABCFloat[] value = new CCLabelUtilities.ABCFloat[1];
//			CCLabelUtilities.ABCFloat[] values = new CCLabelUtilities.ABCFloat[chars.Count];
//
//			for (int i = 0; i < chars.Count; i++)
//			{
//				var ch = chars[i];
//				CCLabelUtilities.GetCharABCWidthsFloat(ch, font, out value);
//				values[i] = value[0];
//			}
//
//			var layoutRectangle = new RectangleF (0, 0, bitmap.Width, bitmap.Height);
//
//			for (int i = 0; i < chars.Count; i++)
//			{
//				var s = chars[i].ToString();
//
//				var charSize = CCLabelUtilities.MeasureString(s, font);
//
//				int w = (int)Math.Ceiling(charSize.Width + 2);
//				int h = (int)Math.Ceiling(charSize.Height + 2);
//
//				bitmap.ClearRect (layoutRectangle);
//
//				CCLabelUtilities.NativeDrawString (bitmap, s, font, brush, layoutRectangle);
//
//				var bitmapData = bitmap.Data;
//				unsafe
//				{
//					var pBase = (byte*)bitmapData;
//					var stride = bitmap.BytesPerRow;
//
//					int minX = w;
//					int maxX = 0;
//					int minY = h;
//					int maxY = 0;
//
//					for (int y = 0; y < h; y++)
//					{
//						var row = (int*)(pBase + y * stride);
//
//						for (int x = 0; x < w; x++)
//						{
//							if (row[x] != 0)
//							{
//								minX = Math.Min(minX, x);
//								maxX = Math.Max(maxX, x);
//								minY = Math.Min(minY, y);
//								maxY = Math.Max(maxY, y);
//							}
//						}
//					}
//
//					w = Math.Max(maxX - minX + 1, 1);
//					h = Math.Max(maxY - minY + 1, 1);
//
//					//maxX = minX + w;
//					//maxY = minY + h;
//
//					int index = 0;
//					for (int y = minY; y <= maxY; y++)
//					{
//						var row = (int*)(pBase + y * stride);
//						for (int x = minX; x <= maxX; x++)
//						{
//							data[index] = row[x];
//							index++;
//						}
//					}
//
//					var region = AllocateRegion(w, h);
//
//					if (region.x >= 0)
//					{
//						SetRegionData(region, data, w);
//
//						var fontDef = new CCBMFontConfiguration.CCBMFontDef()
//						{
//							charID = chars[i],
//							rect = new CCRect(region.x, region.y, region.width, region.height),
//							xOffset = minX, // + (int)Math.Ceiling(values[i].abcfA),
//							yOffset = minY,
//							xAdvance = (int)Math.Ceiling(values[i].abcfA + values[i].abcfB + values[i].abcfC)
//						};
//
//						fontConfig.CharacterSet.Add(chars[i]);
//						fontConfig.m_pFontDefDictionary.Add(chars[i], fontDef);
//					}
//					else
//					{
//						CCLog.Log("Texture atlas is full");
//					}
//				}
//			}
//
//			m_bTextureDirty = true;
//
//			return fontConfig;
//		}

	}
}
