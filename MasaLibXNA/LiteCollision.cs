using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using Masa.Lib;

namespace Masa.Lib.XNA.Collision
{
	public enum CollisionType
	{
		Circle,
		Rect,
		ChainCircle,
	}

	public class LiteCollision
	{
		public CollisionType Type;
		public Vector2 Position;
		Vector2[] Positions;
		int chainLength;

		/// <summary>
		/// for Circle & ChainCircle
		/// </summary>
		public float Radius;

		/// <summary>
		/// for Rect
		/// </summary>
		public float Width, Height;

		float Bottom { get { return Position.Y + Height / 2; } }
		float Top { get { return Position.Y - Height / 2; } }
		float Left { get { return Position.X - Width / 2; } }
		float Right { get { return Position.X + Width / 2; } }


		public LiteCollision()
		{
		}


		/// <summary>
		/// 円判定として初期化
		/// </summary>
		/// <param name="r"></param>
		public void SetAsCicle(float r)
		{
			Radius = r;
			Type = CollisionType.Circle;
		}

		/// <summary>
		/// 無回転矩形として初期化
		/// </summary>
		/// <param name="w"></param>
		/// <param name="h"></param>
		public void SetAsRect(float w, float h)
		{
			Width = w;
			Height = h;
			Type = CollisionType.Rect;
		}

		public void SetAsChainCircle(float r, Vector2[] pos)
		{
			Type = CollisionType.ChainCircle;
			Radius = r;
			SetChainCirclePosition(pos);
		}

		public void SetAsChainCircle(float r, Vector2[] pos, int length)
		{
			SetAsChainCircle(r, pos);
			chainLength = length;
		}

		/// <summary>
		/// ChainCircle専用
		/// </summary>
		/// <param name="pos"></param>
		public void SetChainCirclePosition(Vector2[] pos)
		{
			if (Type != CollisionType.ChainCircle)
			{
				throw new Exception("ChainCirlceではないのにChainCircle扱いした");
			}
			Positions = pos;
			chainLength = pos.Length;
		}

		public void SetChainCirclePosition(Vector2[] pos, int length)
		{
			SetChainCirclePosition(pos);
			chainLength = length;
		}

		public bool GetHit(LiteCollision target)
		{
			switch (Type)
			{
				case CollisionType.Circle:
					switch (target.Type)
					{
						case CollisionType.Circle:
							return HitCircleCircle(this, target);
						case CollisionType.Rect:
							return HitCircleRect(this, target);
						case CollisionType.ChainCircle:
							return HitCircleChain(this, target);
						default:
							break;
					}
					break;
				case CollisionType.Rect:
					switch (target.Type)
					{
						case CollisionType.Circle:
							return HitCircleRect(target, this);
						case CollisionType.Rect:
							return HitRectRect(this, target);
						default:
							break;
					}
					break;
				case CollisionType.ChainCircle:
					switch (target.Type)
					{
						case CollisionType.Circle:
							return HitCircleChain(target, this);
						case CollisionType.Rect:
							break;
						case CollisionType.ChainCircle:
							break;
						default:
							break;
					}
					break;
				default:
					break;
			}
			throw new NotImplementedException("未定義の当たり判定組 " + Type.ToString() + " * " + target.Type.ToString());
		}

		public Vector2 GetHitPoint(LiteCollision target)
		{
			switch (target.Type)
			{
				case CollisionType.Circle:
					if (this.Type == CollisionType.Circle)
					{
						return GetHitPointCircleCircle(this, target);
					}
					else if (this.Type == CollisionType.Rect)
					{
						return GetHitPointRectCircle(target, this);
					}
					break;
				case CollisionType.Rect:
					if (this.Type == CollisionType.Circle)
					{
						return GetHitPointRectCircle(this, target);
					}
					break;
				case CollisionType.ChainCircle:
					break;
				default:
					break;
			}
			throw new NotImplementedException();
		}

		#region Collision
		static bool HitCircleRect(LiteCollision circle, LiteCollision rect)
		{
			///正確ではない
			return (rect.Left < circle.Position.X + circle.Radius)
				&& (circle.Position.X - circle.Radius < rect.Right)
				&& (rect.Top - circle.Radius < circle.Position.Y)
				&& (circle.Position.Y < rect.Bottom + circle.Radius);
		}

		static bool HitCircleCircle(LiteCollision circle1, LiteCollision circle2)
		{
			return HitCircleCircleHelper(circle1.Position, circle1.Radius, circle2.Position, circle2.Radius);
		}

		static bool HitRectRect(LiteCollision rect1, LiteCollision rect2)
		{
			//(rect1.Position.X - rect1.Width / 2f < rect2.Position.X - rect2.Width / 2f)
			throw new NotImplementedException("じっそうめんどくさい");
		}

		static bool HitCircleChain(LiteCollision circle, LiteCollision chain)
		{
			for (int i = 0; i < chain.chainLength; i++)
			{
				if (HitCircleCircleHelper(circle.Position, circle.Radius, chain.Positions[i], chain.Radius))
				{
					return true;
				}
			}
			return false;
		}

		static bool HitCircleCircleHelper(Vector2 pos1, float r1, Vector2 pos2, float r2)
		{
			return (pos1 - pos2).LengthSquared() < (r1 * r1 + r2 * r2);
		}
		#endregion

		#region HitPoint

		static Vector2 GetHitPointCircleCircle(LiteCollision c1, LiteCollision c2)
		{
			LiteCollision sml, big;
			if (c1.Radius > c2.Radius)
			{
				sml = c2;
				big = c1;
			}
			else
			{
				sml = c1;
				big = c2;
			}
			Vector2 d = sml.Position - big.Position;
			float dis = d.LengthSquared();
			if (dis > MathUtil.Pow2(c1.Radius + c2.Radius))///そもそも当たっていない
			{
				throw new Exception();
			}
			
			if (MathUtil.Pow2(big.Radius - sml.Radius) > dis)///小さいほうが完全に内包されてる
			{
				return sml.Position;
			}
			//d.Normalize();
			return big.Position + d * ((MathUtil.Pow2(big.Radius) - MathUtil.Pow2(sml.Radius) + dis) * .5f / dis);
			//return sml.Position + d * ((1 + (big.Radius * big.Radius - sml.Radius * sml.Radius) / dis) / 2f);
			//return sml.Position + d * ((1 + (sml.Radius * sml.Radius - big.Radius * big.Radius) / dis) / 2);
		}

		static Vector2 GetHitPointRectCircle(LiteCollision ccl, LiteCollision rct)
		{
			Vector2 c = ccl.Position;
			bool xin = rct.Left < c.X && c.X < rct.Right;
			bool yin = rct.Top < c.Y && c.Y < rct.Bottom;
			if (xin && yin)//円が中に入っている
			{
				return c;
			}
			if (xin)
			{
				if (c.Y > rct.Position.Y)
				{
					return new Vector2(c.X, rct.Bottom);
				}
				else
				{
					return new Vector2(c.X, rct.Top);
				}
			}
			else
			{
				if (c.X > rct.Position.X)
				{
					return new Vector2(rct.Right, c.Y);
				}
				else
				{
					return new Vector2(rct.Left, c.Y);
				}
			}
		}

		#endregion
	}
}
