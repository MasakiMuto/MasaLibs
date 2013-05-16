using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Masa.Lib.XNA
{
	public static class GraphicUtil
	{
		#region String
		
		public static void DrawStringCenter(this SpriteBatch batch, SpriteFont font, string text, Vector2 pos, Color col)
		{
			Vector2 size = font.MeasureString(text);
			batch.DrawString(font, text, pos - size / 2, col);
		}

		/// <summary>
		/// xy中央揃え
		/// </summary>
		public static void DrawStringCenter(this SpriteBatch batch, SpriteFont font, string text, Vector2 pos, Color col, float scale)
		{
			Vector2 size = font.MeasureString(text);
			batch.DrawString(font, text, pos, col, 0, size / 2, scale, SpriteEffects.None, 0);
		}

		/// <summary>
		/// xのみ中央揃え、yは標準と同じ
		/// </summary>
		/// <param name="batch"></param>
		/// <param name="font"></param>
		/// <param name="text"></param>
		/// <param name="pos"></param>
		/// <param name="col"></param>
		public static void DrawStringXCenter(this SpriteBatch batch, SpriteFont font, string text, Vector2 pos, Color col)
		{
			float y = 0;
			foreach (var line in text.Split('\n'))
			{
				Vector2 size = font.MeasureString(line);
				batch.DrawString(font, line, new Vector2(pos.X - size.X * .5f, pos.Y + y), col);
				y += size.Y;
			}
		}

		/// <summary>
		/// 右揃え
		/// </summary>
		/// <param name="batch"></param>
		/// <param name="font"></param>
		/// <param name="text"></param>
		/// <param name="pos"></param>
		/// <param name="col"></param>
		public static void DrawStringRight(this SpriteBatch batch, SpriteFont font, string text, Vector2 pos, Color col)
		{
			float y = 0;
			foreach (var line in text.Split('\n'))
			{
				Vector2 size = font.MeasureString(line);
				batch.DrawString(font, line, pos - new Vector2(size.X, -y), col);
				y += size.Y;
			}
			
		}

		public static void DrawStringRotate(this SpriteBatch batch, SpriteFont font, string text, Vector2 position, float angle, Color color)
		{
			var size = font.MeasureString(text);
			var rot = MathUtilXNA.Rotate(size, angle);
			batch.DrawString(font, text, position + rot * .5f, color, angle, size * .5f, 1, SpriteEffects.None, 0);
		}

		public static int ClampStringLengthByWidth(this SpriteFont font, string line, float width)
		{
			float w = font.MeasureString(line).X;
			if (w <= width)
			{
				return line.Length;
			}
			int length = line.Length;
			int i = 1;
			while (true)
			{
				if (i >= length)
				{
					break;
				}
				i *= 2;
			}
			length = i;
			int range = length / 2;

			while (range > 1)
			{
				w = font.MeasureString(line.Substring(0, Math.Min(line.Length, length))).X;
				if (w > width)
				{
					length -= range;
				}
				else
				{
					length += range;
				}
				range /= 2;
			}
			return length - 1;
		}

		public static string DivideLineByWidth(this SpriteFont font, string line, float width)
		{
			StringBuilder builder = new StringBuilder();
			int head = 0, tail;

			while (true)
			{
				tail = ClampStringLengthByWidth(font, line.Substring(head), width);
				builder.AppendLine(line.Substring(head, tail));
				if (head + tail == line.Length)
				{
					return builder.ToString();
				}
				head += tail;
			}
		}



		
		
		#endregion


		public static void DrawCenter(this SpriteBatch batch, Texture2D texture, Vector2 center, Color color, float angle, float scale)
		{
			Vector2 size = new Vector2(texture.Width * .5f, texture.Height * .5f);
			batch.Draw(texture, center, null, color, angle, size, scale, SpriteEffects.None, 0);
		}

		#region Color

		public static Color CreateRGB(int rgb)
		{
			return new Color((rgb & 0xff0000) >> 16, (rgb & 0x00ff00) >> 8, rgb & 0x0000ff);
		}

		public static Color CreateRGBA(int rgba)
		{
			return new Color((rgba & 0xff000000) >> 24, (rgba & 0x00ff0000) >> 16, (rgba & 0x0000ff00) >> 8, rgba & 0x000000ff);
		}

		/// <summary>
		/// 色の加算。Max超えたらMaxが入る。Alphaもそのまま足す
		/// </summary>
		/// <param name="c1"></param>
		/// <param name="c2"></param>
		/// <returns></returns>
		public static Color Add(this Color c1, Color c2)
		{
			return new Color(c1.R + c2.R, c1.G + c2.G, c1.B + c2.B, c1.A + c2.A);
		}

		/// <summary>
		/// rgb = value, a = 1のColorを作成
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static Color CreateColor(float value)
		{
			return new Color(value, value, value);
		}

		#endregion


		#region Stencil

		static DepthStencilState StencilWriter = new DepthStencilState()
		{
			StencilEnable = true,
			StencilPass = StencilOperation.Replace,
			StencilFunction = CompareFunction.Always,
			DepthBufferEnable = false,
			ReferenceStencil = 1
		};

		static DepthStencilState StencilUser = new DepthStencilState()
		{
			StencilEnable = true,
			StencilPass = StencilOperation.Keep,
			StencilFunction =  CompareFunction.Equal,
			ReferenceStencil = 1
		};

		public static void DrawStencil(this SpriteBatch sprite, Texture2D stencil, Texture2D texture)
		{
			sprite.Begin(SpriteSortMode.Deferred, null, null, StencilWriter, null);

		}
		#endregion
	}
}
