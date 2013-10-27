using SharpDX;

namespace Masa.Lib.XNA.Collision
{
	/// <summary>
	/// ただの円
	/// </summary>
	public class CollisionCircle : Collision
	{
		float radius;
		public float Radius
		{
			get { return radius; }
			set
			{
				radius = value;
				if (radius < 0) radius = 0;
			}
		}

		public CollisionCircle(float r, Vector2 pos)
		{
			Radius = r;
			Position = pos;
		}

		internal override bool GetCollisionCircle(CollisionCircle obj)
		{
			return Pow2(obj.Radius + Radius) >= (obj.Position - Position).LengthSquared();
		}

		internal override bool GetCollisionLine(CollisionLine obj)
		{
			return obj.GetCollisionCircle(this);
		}

		internal override bool GetCollisionPolygon(CollisionPolygon obj)
		{
			return obj.GetCollisionCircle(this);
		}

		internal override bool GetCollisionPoint(CollisionPoint obj)
		{
			return (Position - obj.Position).LengthSquared() <= Pow2(radius);
		}

		internal override bool GetCollisionRect(CollisionRect obj)
		{
			return obj.GetCollisionCircle(this);

		}

		internal override bool GetCollisionChainCircle(CollisionChainCircle obj)
		{
			return obj.GetCollisionCircle(this);
		}
	}
}
