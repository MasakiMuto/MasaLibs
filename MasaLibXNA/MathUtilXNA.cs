using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Masa.Lib;
using System.Diagnostics;

namespace Masa.Lib.XNA
{
	public static class MathUtilXNA
	{

		#region Util

		/// <summary>
		/// z=0でxy=0にならないProjection行列を作成する
		/// </summary>
		/// <param name="nearWidth">NearPlaneの幅</param>
		/// <param name="farWidth">FarPlaneの幅</param>
		/// <param name="aspectRatio">x/yの値</param>
		/// <param name="nearPlane">nearPlaneのz</param>
		/// <param name="farPlane">farPlaneのz</param>
		/// <returns></returns>
		public static Matrix CreatePerspective(float nearWidth, float farWidth, float aspectRatio, float nearPlane, float farPlane)
		{
			nearWidth *= .5f;
			farWidth *= .5f;
			float nearHeight = nearWidth / aspectRatio;
			float farHeight = farWidth / aspectRatio;
			float fn = farPlane - nearPlane;
			float alpha = (nearPlane * farWidth - farPlane * nearWidth) / (farWidth - nearWidth);
			return new Matrix(fn / (farWidth - nearWidth), 0, 0, 0,
								0, fn / (farHeight - nearHeight), 0, 0,
								0, 0, -(farPlane - alpha) / fn, -1,
								0, 0, -(farPlane - alpha) / fn * nearPlane, alpha);
		}






		//角度差を -Pi ~ +Piで返す
		public static float GetAngleDistance(float a1, float a2)
		{
			return MathHelper.WrapAngle(a1 - a2);
		}





		/// <summary>
		/// 戦車風固定角度自機狙い
		/// </summary>
		/// <param name="target"></param>
		/// <param name="divitation"></param>
		/// <returns></returns>
		public static float LimitedAim(float target, int divitation)
		{
			//float single = (MathHelper.TwoPi / divitation);
			//float an1 = target % single;
			//float an2 = single - an1;
			//target += an2;
			//if (an1 < an2)
			//{
			//	target -= an1 + an2;
			//}
			//return target;
			float single = MathHelper.TwoPi / divitation;
			target = MathHelper.WrapAngle(target) + MathHelper.Pi;//0-2pi
			float a1 = target % single;
			float a2 = target - a1 - MathHelper.Pi;
			if (a1 > single * .5f)
			{
				return a2 + single;
			}
			else
			{
				return a2;
			}

		}


		#endregion

		#region Vector

		public static float Angle(this Vector2 vec)
		{
			return MathUtil.Atan2(vec.Y, vec.X);
		}

		///origin原点の極座標を(Distance, Angle)で返す
		public static Vector2 ToPolar(this Vector2 p, Vector2 origin)
		{
			return new Vector2((p - origin).Length(), (p - origin).Angle());
		}

		public static Vector2 ToPolar(this Vector2 p)
		{
			return new Vector2(p.Length(), p.Angle());
		}

		//二次元外積
		public static float Cross(Vector2 v1, Vector2 v2)
		{
			return v1.X * v2.Y - v1.Y * v2.X;
		}

		///origin原点の極座標(Distance, Angle)を直交座標に変換
		public static Vector2 ToRect(this Vector2 p, Vector2 origin)
		{
			return new Vector2(MathUtil.Cos(p.Y) * p.X, MathUtil.Sin(p.Y) * p.X) + origin;
		}

		public static Vector2 Rotate(this Vector2 v, float angle)
		{
			return new Vector2(v.X * MathUtil.Cos(angle) - v.Y * MathUtil.Sin(angle), v.X * MathUtil.Sin(angle) + v.Y * MathUtil.Cos(angle));
		}

		public static Vector3 GetVector(float length, float angle1, float angle2)
		{
			Debug.Assert(!length.IsNan() && !angle1.IsNan() && !angle2.IsNan());
			return new Vector3(MathUtil.Cos(angle1) * MathUtil.Cos(angle2), MathUtil.Sin(angle1),
				MathUtil.Cos(angle1) * MathUtil.Sin(angle2)) * length;
		}

		public static Vector2 GetVector(float length, float angle)
		{
			Debug.Assert(!length.IsNan() && !angle.IsNan());
			return new Vector2(MathUtil.Cos(angle) * length, MathUtil.Sin(angle) * length);
		}

		public static Vector3 CopyVector(Vector3 origin)
		{
			return new Vector3(origin.X, origin.Y, origin.Z);
		}

		public static Vector2 CopyVector(Vector2 origin)
		{
			return new Vector2(origin.X, origin.Y);
		}

		/// <summary>
		/// 要素ごとの積
		/// </summary>
		/// <param name="v1"></param>
		/// <param name="v2"></param>
		/// <returns></returns>
		public static Vector2 ElementMul(this Vector2 v1, Vector2 v2)
		{
			return new Vector2(v1.X * v2.X, v1.Y * v2.Y);
		}

		/// <summary>
		/// 要素ごとのv1/v2
		/// </summary>
		/// <param name="v1"></param>
		/// <param name="v2"></param>
		/// <returns></returns>
		public static Vector2 ElementDiv(this Vector2 v1, Vector2 v2)
		{
			return new Vector2(v1.X / v2.X, v1.Y / v2.Y);
		}

		public static Vector3 ElementMul(this Vector3 v1, Vector3 v2)
		{
			return new Vector3(v1.X * v2.X, v1.Y * v2.Y, v1.Z * v2.Z);
		}

		public static Vector3 ElementDiv(this Vector3 v1, Vector3 v2)
		{
			return new Vector3(v1.X / v2.X, v1.Y / v2.Y, v1.Z / v2.Z);
		}

		/// <summary>
		/// 各要素の逆数
		/// </summary>
		/// <param name="v"></param>
		/// <returns></returns>
		public static Vector2 Inverse(this Vector2 v)
		{
			return new Vector2(1f / v.X, 1f / v.Y);
		}

		/// 特定の長さのランダムな向きの三次元ベクトル
		public static Vector3 GetRandomVector(float length, Random rnd)
		{
			var v = new Vector3(rnd.Next(-1000, 1000), rnd.Next(-1000, 1000), rnd.Next(-1000, 1000));
			v.Normalize();
			return v * length;
		}

		public static Vector2 Clamp(Vector2 value, Vector2 min, Vector2 max)
		{
			value.X = MathHelper.Clamp(value.X, min.X, max.X);
			value.Y = MathHelper.Clamp(value.Y, min.Y, max.Y);
			return value;
		}

		/// <summary>
		/// 基準ベクトルの左側に対象ベクトルがあれば+1、右側なら-1
		/// </summary>
		/// <param name="baseVector"></param>
		/// <param name="targetVector"></param>
		/// <returns></returns>
		public static int GetLeftRight(Vector2 baseVector, Vector2 targetVector)
		{
			return Cross(baseVector, targetVector) > 0
				? 1
				: -1;
		}

		public static Rectangle GetRect(float x, float y, float width, float height)
		{
			return new Rectangle((int)x, (int)y, (int)width, (int)height);
		}

		public static Rectangle GetRect(Vector2 pos, Vector2 size)
		{
			return GetRect(pos.X, pos.Y, size.X, size.Y);
		}

		#endregion

		#region Collisions


		public static bool CollisionRect(Vector2 center1, Vector2 size1, Vector2 center2, Vector2 size2)
		{
			return Math.Abs(center1.X - center2.X) < (size1.X + size2.X) * .5f && Math.Abs(center1.Y - center2.Y) < (size1.Y + size2.Y) * .5f;
		}

		/// <summary>
		/// 範囲外から完全に出たかを判定する
		/// </summary>
		/// <param name="position">主体の中心</param>
		/// <param name="frame">判定の範囲(最小は0)</param>
		/// <param name="margin">「完全に」というための余裕</param>
		/// <returns></returns>
		public static bool IsOverRect(Vector2 position, Vector2 frame, Vector2 margin)
		{
			return IsOverRect(position, Vector2.Zero, frame, margin);
		}

		public static bool IsOverRect(Vector2 position, Vector2 leftTop, Vector2 rightBottom, Vector2 margin)
		{
			return position.X < leftTop.X - margin.X || position.X > rightBottom.X + margin.X
				|| position.Y < leftTop.Y - margin.Y || position.Y > rightBottom.Y + margin.Y;
		}

		/// <summary>
		/// 少しでも中に入っているか
		/// </summary>
		/// <param name="position"></param>
		/// <param name="leftTop"></param>
		/// <param name="rightBottom"></param>
		/// <param name="margin">キャラクターの大きさ</param>
		/// <returns></returns>
		public static bool IsInRect(Vector2 position, Vector2 leftTop, Vector2 rightBottom, Vector2 margin)
		{
			return (leftTop.X < position.X + margin.X && position.X - margin.X < rightBottom.X)
				&& (leftTop.Y < position.Y + margin.Y && position.Y - margin.Y < rightBottom.Y);
		}

		/// <summary>
		/// 平面上の点の群れから、それら全てを内包する最小の多角形を求める
		/// </summary>
		/// <param name="p"></param>
		/// <returns></returns>
		public static Vector2[] GetMinimamPolygon(Vector2[] p)
		{
			Vector2[] points = new Vector2[p.Length + 1];
			points[0] = new Vector2();
			p.CopyTo(points, 1);
			var index = new List<int>(points.Length);
			var output = new List<Vector2>(points.Length);
			int now = 0;
			float max = 0;
			float l;

			//原点から一番遠い点を探す
			for (int i = 0; i < points.Length; i++)
			{
				l = points[i].LengthSquared();
				if (max < l)
				{
					max = l;
					now = i;
				}
			}
			index.Add(now);
			output.Add(points[now]);

			Func<int, bool> checkIn = (int i) =>
				{
					Vector2 o = points[now] - points[i];
					if (o.LengthSquared() < 0.1) return false;
					for (int j = 0; j < points.Length; j++)
					{
						if (Cross(o, points[j] - points[i]) < 0) return false;
					}
					return true;
				};

			while (true)
			{
			SEARCH_POINT:
				for (int i = 0; i < points.Length; i++)
				{
					//if (i != now && (!index.Contains(i) || i == index[0]))
					if (i != now && !index.Contains(i))
					{
						/*origin = points[now] - points[i];
						for (int j = 0; j < points.Length; j++)
						{
							if (i != j && j != now)
							{
								if (Cross(origin, points[j] - points[i]) < 0) break;
							}
							if (j == points.Length - 1)
							{
								if (i == index[0])
								{
									goto END;
								}
								now = i;
								index.Add(i);
								output.Add(points[i]);
								goto SEARCH_POINT;
							}
						}*/
						if (checkIn(i))
						{
							now = i;
							index.Add(i);
							output.Add(points[i]);
							goto SEARCH_POINT;
						}
					}
				}
				break;
			}

			return output.ToArray();
		}
		#endregion

	}
}
