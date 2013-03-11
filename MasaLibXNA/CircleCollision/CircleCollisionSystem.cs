using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Masa.Lib;

namespace Masa.Lib.XNA.CircleCollision
{
	public struct CollisionTarget
	{
		readonly Vector2 Position;
		readonly float Radius;
		public CollisionTarget(Vector2 pos, float rad)
		{
			Position = pos;
			Radius = rad;
		}

		public Vector2 GetDistanceVector(CollisionTarget target)
		{
			return Position - target.Position;
		}

		public float GetSquareRadius(CollisionTarget target)
		{
			return MathUtil.Pow2(Radius + target.Radius);
		}

		//public bool IsCollision(CollisionTarget target)
		//{
		//	return GetCollisionCircle(Position, Radius, target.Position, target.Radius);
		//}
	}

	public class CollisionPart
	{
		public float BaseAngle { get; set; }
		public float Radius { get; set; }
		public float Distance { get; set; }
		readonly CircleCollisionSystem Parent;

		public CollisionPart(float angle, float radius, float distance, CircleCollisionSystem parent)
		{
			Parent = parent;
			BaseAngle = angle;
			Radius = radius;
			Distance = distance;
		}

		public Vector2 GetCollisionPosition()
		{
			return Parent.Position + MathUtilXNA.GetVector(Distance, BaseAngle + Parent.Angle);
		}

		public CollisionTarget GetCollisionTarget()
		{
			return new CollisionTarget(GetCollisionPosition(), Radius);
		}
	}

	public class CircleCollisionSystem
	{
		public float PredictateRadius { get; set; }
		public Vector2 Position { get; set; }

		public float Radius { get; set; }
		/// <summary>
		/// 向いている角。CollisionPartの位置を計算するのに使う
		/// </summary>
		public float Angle { get; set; }
		
		public List<CollisionPart> Collisions { get; private set; }
		
		CollisionTarget GetCollisionTarget()
		{
			return new CollisionTarget(Position, Radius);
		}

		CollisionTarget GetPredictateCollisionTarget()
		{
			return new CollisionTarget(Position, PredictateRadius);
		}

		/// <summary>
		/// 初期化する
		/// </summary>
		public void Set()
		{
			if (Collisions != null)
			{
				Collisions.Clear();
			}
			PredictateRadius = float.NaN;
			
		}

		/// <summary>
		/// 円の判定を追加する
		/// </summary>
		/// <param name="dist">Positionからの距離</param>
		/// <param name="angle">Positionからの偏角</param>
		/// <param name="radius">円の半径</param>
		/// <returns>CollisionsでのIndex</returns>
		public int AddCollision(float dist, float angle, float radius)
		{
			if (Collisions == null)
			{
				Collisions = new List<CollisionPart>();
			}
			Collisions.Add(new CollisionPart(angle, radius, dist, this));
			return Collisions.Count - 1;
		}

		/// <summary>
		/// CollisionPartの状態を更新する
		/// </summary>
		/// <param name="index">設定するもののIndex(AddCollisionでの返り値)</param>
		/// <param name="dist">NaNなら更新しない</param>
		/// <param name="angle">NaNなら更新しない</param>
		/// <param name="radius">NaNなら更新しない</param>
		public void UpdateCollision(int index, float dist, float angle, float radius)
		{
			var item = Collisions[index];
			if (!dist.IsNan())
			{
				item.Distance = dist;
			}
			if (!angle.IsNan())
			{
				item.BaseAngle = angle;
			}
			if (!radius.IsNan())
			{
				item.Radius = radius;
			}
		}




		public bool GetCollision(CircleCollisionSystem target)
		{
			if (IsCollision(GetCollisionTarget(), target.GetCollisionTarget()))//コア判定
			{
				return true;
			}
			bool mul1 = IsMultiCollision();
			bool mul2 = target.IsMultiCollision();
			if (!mul1 && !mul2)//single-singleならend
			{
				return false;
			}
			if (mul1 && mul2)
			{
				return GetMultiMultiCollision(this, target);
			}
			if (!mul1 && mul2)
			{
				return GetSingleMultiCollision(this, target);
			}
			if (mul1 && !mul2)
			{
				return GetSingleMultiCollision(target, this);
			}
			throw new Exception();
		}

		static bool GetSingleSingleCollision(CircleCollisionSystem c1, CircleCollisionSystem c2)
		{
			return c1.IsCollision(c1.GetCollisionTarget(), c2.GetCollisionTarget());
		}

		static bool GetMultiMultiCollision(CircleCollisionSystem c1, CircleCollisionSystem c2)
		{
			if (!GetMultiMultiCollisionPredict(c1, c2))
			{
				return false;
			}
			return c1.Collisions
				.Select(i => i.GetCollisionTarget())
				.Any(i =>
					c2.Collisions
					.Select(j => j.GetCollisionTarget())
					.Any(j => c1.IsCollision(i, j))
				);
		}

		static bool GetSingleMultiCollision(CircleCollisionSystem single, CircleCollisionSystem multi)
		{
			if (!GetSingleMultiCollisionPredict(single, multi))
			{
				return false;
			}
			return GetSingleMultiCollision(single.GetCollisionTarget(), multi);
		}

		static bool GetSingleMultiCollision(CollisionTarget single, CircleCollisionSystem multi)
		{
			return multi.Collisions.Any(i => multi.IsCollision(i.GetCollisionTarget(), single));
		}

		static bool GetMultiMultiCollisionPredict(CircleCollisionSystem c1, CircleCollisionSystem c2)
		{
			bool p1 = c1.IsNeedPredict();
			bool p2 = c2.IsNeedPredict();
			if (!p1 && !p2)
			{
				return true;
			}
			if (p1 && p2)
			{
				return c1.IsCollision(c1.GetPredictateCollisionTarget(), c2.GetPredictateCollisionTarget());
			}
			if (p1 && !p2)
			{
				return GetSingleMultiCollision(c1.GetPredictateCollisionTarget(), c2);
			}
			if (!p1 && p2)
			{
				return GetSingleMultiCollision(c2.GetPredictateCollisionTarget(), c1);
			}
			return true;

		}

		static bool GetSingleMultiCollisionPredict(CircleCollisionSystem single, CircleCollisionSystem multi)
		{
			if (multi.IsNeedPredict())
			{
				return single.IsCollision(multi.GetPredictateCollisionTarget(), single.GetCollisionTarget());
			}
			else
			{
				return true;
			}
		}


		bool IsNeedPredict()
		{
			return !PredictateRadius.IsNan();
		}

		bool IsMultiCollision()
		{
			return Collisions != null && Collisions.Count > 0;
		}

		/// <summary>
		/// 円と円との判定部分
		/// </summary>
		/// <param name="t1"></param>
		/// <param name="t2"></param>
		/// <returns></returns>
		protected virtual bool IsCollision(CollisionTarget t1, CollisionTarget t2)
		{
			return t1.GetDistanceVector(t2).LengthSquared() < t1.GetSquareRadius(t2);
		}


	}
}
