using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;

namespace Masa.Lib.XNA.Collision
{
	/// <summary>
	/// 始点終点からなる太さを持たない線分
	/// </summary>
	public class CollisionLine : Collision
	{

		public Vector2 Line;
		public Vector2 Begin//始点
		{
			get { return Position; }
		}
		public Vector2 End///終点
		{
			get { return Position + Line; }
		}

		public CollisionLine(Vector2 pos, Vector2 line)
		{
			Position = pos;
			Line = line;

		}


		internal override bool GetCollisionCircle(CollisionCircle obj)
		{
			//float rd2 = Pow2(obj.Radius + Width);
			//Vector2 ao = obj.Position - Position;
			//Vector2 bo = obj.Position - End;

			//if (ao.LengthSquared() <= rd2 || bo.LengthSquared() <= rd2) return true;
			//if (!(Vector2.Dot(-Line, ao) * Vector2.Dot(Line, bo) >= 0)) return false;
			//if ((Line.Y * obj.Position.X + Line.X * obj.Position.Y + Position.X * Line.Y - Position.Y * Line.X)
			//    / Line.LengthSquared() <= rd2) return true;
			if (System.Math.Abs(MathUtilXNA.Cross(obj.Position - Begin, Line) / (Line.Length())) > obj.Radius) return false;
			if (Vector2.Dot(Line, obj.Position - Begin) * Vector2.Dot(-Line, obj.Position - End) <= 0) return true;
			if (obj.GetCollision(new CollisionPoint(Begin)) || obj.GetCollision(new CollisionPoint(End))) return true;
			return false;
		}

		internal override bool GetCollisionLine(CollisionLine obj)
		{
			if (MathUtilXNA.Cross(Line, obj.Position - Position) * MathUtilXNA.Cross(Line, obj.End - Position) <= 0
				&& MathUtilXNA.Cross(obj.Line, Position - obj.Position) * MathUtilXNA.Cross(obj.Line, End - obj.Position) <= 0) return true;
			return false;
		}

		internal override bool GetCollisionPolygon(CollisionPolygon obj)
		{
			return obj.GetCollision(this);
		}

		internal override bool GetCollisionPoint(CollisionPoint obj)
		{
			return false;
		}

		internal override bool GetCollisionRect(CollisionRect obj)
		{
			return obj.GetCollision(this);
		}

		internal override bool GetCollisionChainCircle(CollisionChainCircle obj)
		{
			throw new NotImplementedException();
		}
	}
}
