using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Masa.Lib;
using SharpDX;
using System.Diagnostics;

namespace Masa.Lib.XNA
{
	public static class Curves
	{
		/// <summary>
		/// ベジェ曲線上の時刻tにおける座標を返す
		/// </summary>
		/// <param name="t">0~1までのfloat</param>
		/// <param name="points">制御点。数は自由</param>
		/// <returns></returns>
		public static Vector2 Bezier(float t, IEnumerable<Vector2> points)
		{
			Debug.Assert(t >= 0 && t <= 1);
			int length = points.Count();
			Debug.Assert(length >= 2);
			var p = Vector2.Zero;
			for (int i = 0; i < length; i++)
			{
				p += points.ElementAt(i) * Combination(length - 1, i) * MathUtil.Pow(t, i) * MathUtil.Pow(1 - t, length - 1 - i);
			}
			return p;
		}

		static int Combination(int n, int m)
		{
			var x = 1;
			for (int i = 0; i < m; i++)
			{
				x *= (n - i);
			}
			for (int i = 0; i < m; i++)
			{
				x /= (i + 1);
			}
			return x;
		}
	}
}
