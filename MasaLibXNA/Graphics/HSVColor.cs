using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

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
			int hi = (int)(H / 60f);
			float f = (H / 60f) - hi;
			float p = V * (1 - S);
			float q = V * (1 - f * S);
			float t = V * (1 - (1 - f) * S);
			switch (hi)
			{
				case 0:
					return new Color(V, t, p) * A;
				case 1:
					return new Color(q, V, p) * A;
				case 2:
					return new Color(p, V, t) * A;
				case 3:
					return new Color(p, q, V) * A;
				case 4:
					return new Color(t, p, V) * A;
				case 5:
					return new Color(V, p, q) * A;
				default:
					throw new Exception();
				
			}
		}

	}
}
