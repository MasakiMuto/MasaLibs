using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using MathHelper = SharpDX.MathUtil;

namespace Masa.Lib.XNA
{
	public struct HSVColor
	{
		float h, s, v, a;
		/// <summary>
		/// 色相 0..360
		/// </summary>
		public float H
		{
			get
			{
				return h;
			}
			set
			{
				h = MathUtil.PositiveMod(value, 360);
			}
		}
		/// <summary>
		/// 彩度 0..1
		/// </summary>
		public float S
		{
			get
			{
				return s;
			}
			set
			{
				s = MathHelper.Clamp(value, 0, 1);
			}
		}
		/// <summary>
		/// 輝度 0..1
		/// </summary>
		public float V
		{
			get
			{
				return v;
			}
			set
			{
				v = MathHelper.Clamp(value, 0, 1);
			}
		}

		/// <summary>
		/// アルファ 0..1
		/// </summary>
		public float A
		{
			get
			{
				return a;
			}
			set
			{
				a = MathHelper.Clamp(value, 0, 1);
			}
		}

		public HSVColor(Color color)
			: this()
		{
			Vector3 rgb = color.ToVector3();
			A = color.A / 255f;
			if (A == 0)
			{
				rgb = Vector3.Zero;
			}
			else
			{
				rgb /= A;
			}

			float max = Math.Max(rgb.X, Math.Max(rgb.Y, rgb.Z));
			float min = Math.Min(rgb.X, Math.Min(rgb.Y, rgb.Z));
			V = max;
			if (max == 0)
			{
				H = 0;
				S = 0;
				return;
			}
			S = (max - min) / max;
			if (S == 0)
			{
				H = 0;
				return;
			}
			if (max == rgb.X)
			{
				H = 60f * (rgb.Y - rgb.Z) / (max - min);
			}
			else if (max == rgb.Y)
			{
				H = 60f * (rgb.Z - rgb.X) / (max - min) + 120f;
			}
			else
			{
				H = 60f * (rgb.X - rgb.Y) / (max - min) + 240f;
			}
		}

		public Color ToRGB()
		{
			return new Color(HSVToRGB(H, S, V)) * A;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="h">0..360</param>
		/// <param name="s">0..1</param>
		/// <param name="v">0..1</param>
		/// <returns></returns>
		public static Vector3 HSVToRGB(float h, float s, float v)
		{
			h = MathUtil.PositiveMod(h, 360f);
			s = MathHelper.Clamp(s, 0, 1);
			v = MathHelper.Clamp(v, 0, 1);
			int hi = (int)(h / 60f);
			if (hi >= 6)
			{
				hi = 0;
			}
			float f = (h / 60f) - hi;
			float p = v * (1 - s);
			float q = v * (1 - f * s);
			float t = v * (1 - (1 - f) * s);
			switch (hi)
			{
				case 0:
					return new Vector3(v, t, p);
				case 1:
					return new Vector3(q, v, p);
				case 2:
					return new Vector3(p, v, t);
				case 3:
					return new Vector3(p, q, v);
				case 4:
					return new Vector3(t, p, v);
				case 5:
					return new Vector3(v, p, q);
				default:
					throw new Exception();
			}
		}

	}
}
