using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Masa.Lib
{
	public static class MathUtil
	{
		#region Random

		/// <summary>
		/// +1 か -1をランダムに返す
		/// </summary>
		/// <param name="rand"></param>
		/// <returns></returns>
		public static int NextPN(this Random rand)
		{
			return rand.Next(2) * 2 - 1;
		}

		/// <summary>
		/// 歪んだ分布のfloat乱数
		/// </summary>
		/// <param name="rand"></param>
		/// <param name="max">最大値(負の場合は負が帰る)</param>
		/// <param name="a">正の乗数。0に近いほど0付近に集まり、大きいほどmax付近に集まる。</param>
		/// <returns>0~maxまでのflaot</returns>
		public static float NextDistorted(this Random rand, float max, float a)
		{
			return (float)((1d - Math.Pow(rand.NextDouble(), a)) * max);
			//return (float)((Math.Pow(rand.NextDouble() + 1, a)) / (Math.Pow(2, a)) * max);
		}

		/// <summary>
		/// 擬似的・簡易的な正規乱数 N(μ, σ^2) (μ前後)
		/// </summary>
		/// <param name="rand"></param>
		/// <param name="average">平均値μ</param>
		/// <param name="devitation">標準偏差σ</param>
		/// <returns></returns>
		public static double NextNormal(this Random rand, double average, double devitation)
		{
			//double sum = 0;
			//for (int i = 0; i < 12; i++)
			//{
			//    sum += rand.NextDouble();
			//}
			//sum -= 6;
			//return sum * devitation + average;
			return NextSemiNormal(rand, average, devitation, 6);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="rand"></param>
		/// <param name="average">平均値μ</param>
		/// <param name="devitation">標準偏差σ</param>
		/// <param name="num">0～1の積み重ね量</param>
		/// <returns>average + (-num ~ +num) * devitation</returns>
		public static double NextSemiNormal(this Random rand, double average, double devitation, int num)
		{
			double sum = 0;
			for (int i = 0; i < num * 2; i++)
			{
				sum += rand.NextDouble();
			}
			sum -= num;
			return sum * devitation + average;
		}

		public static float GetRandomPM(float range, Random rnd)
		{
			return (float)(rnd.NextDouble() * range * 2 - range);
		}

		public static float GetRandom(float min, float max, Random rnd)
		{
			return (float)(rnd.NextDouble() * (max - min) - min);
		}

		#endregion

		#region NaN

		public static bool IsNan(this float val)
		{
			return float.IsNaN(val);
		}

		public static bool IsNan(this double val)
		{
			return double.IsNaN(val);
		}

		/// <summary>
		/// NaNなら0、そうでなければ値を返す
		/// </summary>
		/// <param name="val"></param>
		/// <returns></returns>
		public static float ValueOr0(this float val)
		{
			return (val.IsNan() ? 0 : val);
		}

		#endregion

		#region Float Wrapper

		public static float Cos(float a)
		{
			return (float)Math.Cos(a);
		}

		public static float Sin(float a)
		{
			return (float)Math.Sin(a);
		}

		public static float Tan(float a)
		{
			return (float)Math.Tan(a);
		}

		//-Pi ~ +Pi
		public static float Atan2(float y, float x)
		{
			return (float)Math.Atan2(y, x);
		}

		/// <summary>
		/// xのy乗
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public static float Pow(float x, int y)
		{
			float r = 1;
			if (y >= 0)
			{
				for (int i = 0; i < y; i++)
				{
					r *= x;
				}
			}
			else
			{
				for (int i = 0; i < y; i--)
				{
					r /= x;
				}
			}
			return r;
		}

		/// <summary>
		/// xのy乗
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public static float Pow(float x, float y)
		{
			return (float)(Math.Pow(x, y));
		}

		/// <summary>
		/// 自乗
		/// </summary>
		/// <param name="x"></param>
		/// <returns></returns>
		public static float Pow2(float x)
		{
			return x * x;
		}

		#endregion

		#region Utils


		/// <summary>
		/// 1以下なら常に0を返す
		/// </summary>
		/// <param name="x"></param>
		/// <param name="divider"></param>
		/// <returns></returns>
		public static int PositiveMod(this int x, int divider)
		{
			if (divider <= 1)
			{
				return 0;
			}
			var value = x % divider;
			if (value < 0)
			{
				value += divider;
			}
			return value;
			//if (x >= 0)
			//{
			//	return x % divider;
			//}
			//else
			//{
			//	return x % divider + divider;
			//}
		}

		/// <summary>
		/// 1以下なら常に0を返す
		/// </summary>
		/// <param name="x"></param>
		/// <param name="divider"></param>
		/// <returns></returns>
		public static float PositiveMod(this float x, float divider)
		{
			if (divider <= 1)
			{
				return 0;
			}
			if (x >= 0)
			{
				return x % divider;
			}
			else
			{
				return x % divider + divider;
			}
		}

		#endregion

	}
}
