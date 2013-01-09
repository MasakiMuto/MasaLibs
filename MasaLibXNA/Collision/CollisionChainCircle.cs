using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Masa.Lib.XNA.Collision
{
	/// <summary>
	/// 同一半径の円が複数連なった物。うねうねする触手などに。
	/// </summary>
	public class CollisionChainCircle : Collision
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

		public Vector2[] Positions;

		public CollisionChainCircle(float r, Vector2[] pos)
		{
			Radius = r;
			Positions = pos;
		}

		public void SetPositions(Vector2[] p)
		{
			Positions = p;
		}

		internal override bool GetCollisionCircle(CollisionCircle obj)
		{
			for (int i = 0; i < Positions.Length; i++)
			{
				if (Pow2(obj.Radius + Radius) >= (obj.Position - Positions[i]).LengthSquared())
				{
					return true;
				}
			}
			return false;
		}

		internal override bool GetCollisionChainCircle(CollisionChainCircle obj)
		{
			for (int i = 0; i < Positions.Length; i++)
			{
				for (int k = 0; k < obj.Positions.Length; k++)
				{
					if (Pow2(obj.Radius + Radius) >= (obj.Positions[k] - Positions[i]).LengthSquared())
					{
						return true;
					}
				}
			}
			return false;
		}

		internal override bool GetCollisionRect(CollisionRect obj)
		{
			return obj.GetCollisionChainCircle(this);
		}

		internal override bool GetCollisionLine(CollisionLine obj)
		{
			throw new System.NotImplementedException();
		}

		internal override bool GetCollisionPoint(CollisionPoint obj)
		{
			throw new System.NotImplementedException();
		}

		internal override bool GetCollisionPolygon(CollisionPolygon obj)
		{
			throw new System.NotImplementedException();
		}

	}

	
}
