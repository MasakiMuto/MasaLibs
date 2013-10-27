using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;

namespace Masa.Lib.XNA.Collision
{
	/// <summary>
	/// 回転なしのxy軸に沿った長方形
	/// </summary>
	public class CollisionRect : Collision
	{
		float Width;
		float Height;

		float Top { get { return Position.Y + Height / 2f; } }
		float Bottom { get { return Position.Y - Height / 2f; } }
		float Left { get { return Position.X - Width / 2f; } }
		float Right { get { return Position.X + Width / 2f; } }

		Vector2 TopLeft { get { return new Vector2(Position.X - Width / 2f, Position.Y + Height / 2f); } }
		Vector2 TopRight { get { return new Vector2(Position.X + Width / 2f, Position.Y + Height / 2f); } }
		Vector2 BottomLeft { get { return new Vector2(Position.X - Width / 2f, Position.Y - Height / 2f); } }
		Vector2 BottomRight { get { return new Vector2(Position.X + Width / 2f, Position.Y - Height / 2f); } }

		public CollisionRect(float x, float y, float w, float h)
		{
			Position = new Vector2(x, y);
			Width = w;
			Height = h;
		}

		internal override bool GetCollisionPoint(CollisionPoint obj)
		{
			return obj.Position.X > Left && obj.Position.X < Right && obj.Position.Y < Top && obj.Position.Y > Bottom;
		}

		internal override bool GetCollisionCircle(CollisionCircle obj)//適当
		{
			return obj.Position.X + obj.Radius > Left && obj.Position.X - obj.Radius < Right && obj.Position.Y - obj.Radius < Top && obj.Position.Y + obj.Radius > Bottom;
		}

		internal override bool GetCollisionLine(CollisionLine obj)
		{
			if (obj.Position.X > Left && obj.Position.X < Right && obj.Position.Y < Top && obj.Position.Y > Bottom) return true;
			if (obj.End.X > Left && obj.End.X < Right && obj.End.Y < Top && obj.End.Y > Bottom) return true;
			if (MathUtilXNA.Cross(obj.Line, TopLeft - obj.Begin) * MathUtilXNA.Cross(obj.Line, BottomRight - obj.Begin) <= 0) return true;
			if (MathUtilXNA.Cross(obj.Line, TopRight - obj.Begin) * MathUtilXNA.Cross(obj.Line, BottomLeft - obj.Begin) <= 0) return true;
			return false;
		}

		internal override bool GetCollisionPolygon(CollisionPolygon obj)//適当
		{
			return obj.OutCircle.GetCollision(this);
		}

		internal override bool GetCollisionRect(CollisionRect obj)
		{
			if (!((Left < obj.Left && obj.Left < Right) || (obj.Left < Left && Left < obj.Right))) return false;
			if (!((Top < obj.Top && obj.Top < Bottom) || (obj.Top < Top && Top < obj.Bottom))) return false;
			return true;
		}

		internal override bool GetCollisionChainCircle(CollisionChainCircle obj)
		{
			throw new NotImplementedException();
		}
	}
}
