﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Masa.Lib.XNA
{
	public class TextureColorConverter
	{
		Texture2D baseTexture;
		Color[] buffer;
		HSVColor[] hsv;

		public TextureColorConverter(Texture2D baseTexture)
		{
			this.baseTexture = baseTexture;
			SetBuffer();
		}

		void SetBuffer()
		{
			buffer = new Color[baseTexture.Width * baseTexture.Height];
			baseTexture.GetData(buffer);
			hsv = buffer.Select(c => new HSVColor(c)).ToArray();
		}

		/// <summary>
		/// 色を加算する
		/// </summary>
		/// <param name="h">色相の加算値</param>
		/// <returns></returns>
		public Texture2D Convert(float h)
		{
			return Convert(i => { i.H += h; return i.ToRGB(); });
		}

		public Texture2D ConvertAbsolute(float h)
		{
			return Convert(i => { i.H = h; return i.ToRGB(); });
		}

		public Texture2D Convert(Func<HSVColor, Color> converter)
		{
			var tex = new Texture2D(baseTexture.GraphicsDevice, baseTexture.Width, baseTexture.Height);
			buffer = hsv.Select(converter).ToArray();
			tex.SetData(buffer);
			return tex;
		}


	}
}
