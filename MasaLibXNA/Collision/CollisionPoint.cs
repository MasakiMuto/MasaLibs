using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;

namespace Masa.Lib.XNA.Collision
{
	/// <summary>
	/// 大きさを持たない点
	/// </summary>
	public class CollisionPoint : Collision
	{
		public CollisionPoint(Vector2 pos)
		{
			Position = pos;
		}

		internal override bool GetCollisionCircle(CollisionCircle obj)
		{
			return obj.GetCollision(this);
		}

		internal override bool GetCollisionPolygon(CollisionPolygon obj)
		{
			return obj.GetCollision(this);
		}

		internal override bool GetCollisionPoint(CollisionPoint obj)
		{
			return (obj.Position == Position);
		}

		internal override bool GetCollisionLine(CollisionLine obj)
		{
			return obj.GetCollision(this);
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
