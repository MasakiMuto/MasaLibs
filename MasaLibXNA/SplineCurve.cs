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
		/// 最終速度方向を(最後の点 - その一つ前の点)方向にする
		/// </summary>
		/// <param name="points"></param>
		/// <param name="speed"></param>
		public SplineCurve(IEnumerable<Vector2> points, float speed)
			: this(points, speed, points.Last() - points.ElementAt(points.Count() - 2))
		{
		}

		struct SectionTime
		{
			public readonly int Section;
			public readonly float Time;
			public SectionTime(int section, float time)
			{
				Section = section;
				Time = time;
			}
		}

		/// <summary>
		/// ある全区間時刻における点の存在区間と区間開始からの時間を返す
		/// </summary>
		/// <param name="time"></param>
		/// <returns></returns>
		SectionTime GetSectionAndTime(float time)
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
					return new SectionTime(i, from);
				}
			}
			return new SectionTime(times.Length, time - times[times.Length - 1]);
		}

		/// <summary>
		/// スプライン曲線上の時刻timeにおける点を取得。points[0]のときtime == 0,曲線を走り終わっていたら終点からそのままの速度で直線運動した点を返す
		/// </summary>
		/// <param name="time">0以上</param>
		/// <returns></returns>
		public Vector2 GetPosition(float time)
		{
			Debug.Assert(time >= 0);
			var sect = GetSectionAndTime(time);
			if (sect.Section < times.Length)
			{
				var p1 = points[sect.Section];
				var p2 = points[sect.Section + 1];
				var v1 = (Velocitys[sect.Section]) * Lengths[sect.Section];
				var v2 = (Velocitys[sect.Section + 1]) * Lengths[sect.Section];
				var t = sect.Time * Speed / Lengths[sect.Section];
				return CalcPoint(t, p1, p2, v1, v2);
			}
			else
			{
				return points[points.Length - 1] + Velocitys[Velocitys.Length - 1] * Speed * (sect.Time);
			}

		}

		public Vector2 GetVelocity(float time)
		{
			var sect = GetSectionAndTime(time);
			if (sect.Section < times.Length)
			{
				var p1 = points[sect.Section];
				var p2 = points[sect.Section + 1];
				var v1 = Velocitys[sect.Section] * Lengths[sect.Section];
				var v2 = Velocitys[sect.Section + 1] * Lengths[sect.Section];
				var t = sect.Time * Speed / Lengths[sect.Section];
				return Speed / Lengths[sect.Section] * CalcPointDifferent(t, p1, p2, v1, v2);
			}
			else
			{
				return Velocitys[Velocitys.Length - 1] * Speed;
			}
		}

		Vector2 CalcVelocity(int index)
		{
			Vector2 dir;
			if (index == points.Length - 1)
			{
				dir = LastVelocity;
				//dir = points[index] - points[index - 1];
			}
			else if (index == 0)
			{
				dir = points[1] - points[0];
			}
			else
			{
				dir = points[index + 1] - points[index - 1];
			}
			return Vector2.Normalize(dir);
		}

		Vector2 CalcPoint(float t, Vector2 p1, Vector2 p2, Vector2 v1, Vector2 v2)
		{
			float t2 = t * t;
			float t3 = t * t * t;
			return p1 * (2 * t3 + -3 * t2 + 1) + p2 * (-2 * t3 + 3 * t2) + v1 * (t3 + -2 * t2 + t) + v2 * (t3 - t2);
		}

		/// <summary>
		/// CalcPointの微分
		/// </summary>
		/// <param name="t"></param>
		/// <param name="p1"></param>
		/// <param name="p2"></param>
		/// <param name="v1"></param>
		/// <param name="v2"></param>
		/// <returns></returns>
		Vector2 CalcPointDifferent(float t, Vector2 p1, Vector2 p2, Vector2 v1, Vector2 v2)
		{
			var t2 = t * t;
			return p1 * (6 * t2 - 6 * t) + p2 * (-6 * t2 + 6 * t) + v1 * (3 * t2 - 4 * t + 1) + v2 * (3 * t2 - 2 * t);
		}

		float CalcTemporarySpeed(int index)
		{
			if (index == points.Length - 1)
			{
				return (points[index] - points[index - 1]).Length();
			}
			else if (index == 0)
			{
				return (points[1] - points[0]).Length();
			}
			else
			{
				return ((points[index + 1] - points[index]).Length() + (points[index] - points[index - 1]).Length()) * .5f;
			}
		}

		/// <summary>
		/// 暫定速さを使って大雑把に区間の長さを計算する
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		float CalcLength(int index)
		{
			var p1 = points[index];
			var p2 = points[index + 1];
			var v1 = Velocitys[index] * CalcTemporarySpeed(index);
			var v2 = Velocitys[index + 1] * CalcTemporarySpeed(index + 1);
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

}
