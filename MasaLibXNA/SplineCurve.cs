using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Masa.Lib;
using System.Diagnostics;
using Microsoft.Xna.Framework;


namespace Masa.Lib.XNA
{
	public class SplineCurve
	{
		Vector2[] points;
		float[] times;
		readonly float Speed;
		readonly Vector2 LastVelocity;
		readonly Vector2[] Velocitys;
		readonly float[] Lengths;


		public SplineCurve(IEnumerable<Vector2> points, float speed, Vector2 lastVelDir)
		{
			Debug.Assert(points.Count() >= 2);
			this.points = points.ToArray();

			Speed = speed;
			LastVelocity = Vector2.Normalize(lastVelDir);
			Velocitys = Enumerable.Range(0, this.points.Length).Select(x => CalcVelocity(x)).ToArray();
			Lengths = Enumerable.Range(0, this.points.Length - 1).Select(x => CalcLength(x)).ToArray();

			times = new float[this.points.Length - 1];
			for (int i = 0; i < times.Length; i++)
			{
				times[i] = Lengths[i] / Speed;
				if (i > 0)
				{
					times[i] += times[i - 1];
				}
			}
		}

		/// <summary>
		/// スプライン曲線上の時刻timeにおける点を取得。points[0]のときtime == 0,曲線を走り終わっていたら終点からそのままの速度で直線運動した点を返す
		/// </summary>
		/// <param name="time">0以上</param>
		/// <returns></returns>
		public Vector2 GetPosition(float time)
		{
			Debug.Assert(time >= 0);
			for (int i = 0; i < times.Length; i++)
			{
				if (time < times[i])
				{
					float from = time;
					if (i > 0)
					{
						from = time - times[i - 1];
					}
					var p1 = points[i];
					var p2 = points[i + 1];
					//var t = (MathUtil.Pow(from * Speed / Lengths[i] - .5f, 3) + 0.125f) * 4;
					var t = from * Speed / Lengths[i];
					return CalcPoint(t, p1, p2, Vector2.Normalize( Velocitys[i]) * Lengths[i], Vector2.Normalize( Velocitys[i + 1]) * Lengths[i]);
				}
			}
			return points[points.Length - 1] + LastVelocity * Speed * (time - times[times.Length - 1]);
		}

		Vector2 CalcVelocity(int index)
		{
			if (index == points.Length - 1)
			{
				return LastVelocity * (points[index] - points[index - 1]).Length();
			}
			else
			{
				Vector2 dir;
				if (index == 0)
				{
					return points[1] - points[0];
					dir = points[1] - points[0];
					dir.Normalize();
					return (points[0] - points[1]).Length() * dir;
				}
				else
				{
					
					
					dir = points[index + 1] - points[index - 1];
					//return dir;
					var len = ((points[index + 1] - points[index]).Length() + (points[index] - points[index - 1]).Length()) * .5f;
					return Vector2.Normalize(dir) * len;
				}
				//dir.Normalize();
				//return dir * (points[index] - points[index + 1]).Length();
			}



		}

		Vector2 CalcPoint(float t, Vector2 p1, Vector2 p2, Vector2 v1, Vector2 v2)
		{
			float t2 = t * t;
			float t3 = t * t * t;
			return p1 * (2 * t3 + -3 * t2 + 1) + p2 * (-2 * t3 + 3 * t2) + v1 * (t3 + -2 * t2 + t) + v2 * (t3 - t2);
		}

		float CalcLength(int index)
		{
			var p1 = points[index];
			var p2 = points[index + 1];
			var v1 = Velocitys[index];
			var v2 = Velocitys[index + 1];
			var p = p1;
			var length = 0f;
			const int Divide = 8;
			for (int i = 0; i < Divide; i++)
			{
				var tmp = CalcPoint((i + 1f) / (Divide + 1), p1, p2, v1, v2);
				length += (tmp - p).Length();
				p = tmp;
			}
			length += (p2 - p).Length();
			return length;
		}

	}


	//public class SplineCurve
	//{
	//	SplineElement[] elements;

	//	public SplineCurve(IEnumerable<Vector2> points, float time)
	//	{
	//		elements = new SplineElement[points.Count()];
	//		CalcTimes(points, time);

	//	}

	//	void CalcTimes(IEnumerable<Vector2> points, float time)
	//	{
	//		var times = ConfigureTimeByLength(points);
	//		elements[0].Time0 = 0;
	//		for (int i = 0; i < elements.Length - 1; i++)
	//		{
	//			elements[i].Span = time * times[i];
	//			if (i < elements.Length - 2)
	//			{
	//				elements[i + 1].Time0 = elements[i].Time0 + elements[i].Span;
	//			}
	//		}
	//	}

	//	void CalcParams(IEnumerable<Vector2> points)
	//	{
	//		for (int i = 0; i < elements.Length; i++)
	//		{
	//			elements[i].A = points.ElementAt(i);
	//			if (i == 0 || i == elements.Length - 1)
	//			{
	//				elements[i].C = Vector2.Zero;
	//			}
	//			else
	//			{
	//				elements[i].C = 3f * ((elements[i + 1].A - elements[i].A) / elements[i].Span - (elements[i].A - elements[i - 1].A) / elements[i - 1].Span);
	//			}

	//		}
	//	}

	//	/// <summary>
	//	/// 時間を各線分の長さで割り振る。合計1
	//	/// </summary>
	//	/// <param name="points"></param>
	//	/// <returns></returns>
	//	float[] ConfigureTimeByLength(IEnumerable<Vector2> points)
	//	{
	//		var len = new float[points.Count() - 1];
	//		float total = 0f;
	//		for (int i = 0; i < len.Length; i++)
	//		{
	//			len[i] = (points.ElementAt(i + 1) - points.ElementAt(i)).Length();
	//			total += len[i];
	//		}
	//		return len.Select(x => x / total).ToArray();
	//	}


	//}

	//struct SplineElement
	//{
	//	public float Time0, Span;
	//	public Vector2 A, B, C, D;

	//	public Vector2 Calc(float time)
	//	{
	//		float t = time - Time0;
	//		Debug.Assert(t >= 0 && t <= Span);
	//		return A + B * t + C * t * t + D * t * t * t;
	//	}
	//}
}
