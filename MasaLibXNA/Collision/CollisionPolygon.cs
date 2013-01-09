using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Masa.Lib.XNA.Collision
{
	/// <summary>
	/// 頂点列と回転・平行移動ができる多角形
	/// </summary>
	public class CollisionPolygon : Collision
	{
		Vector2[] vertices;
		public Vector2[] temp;
		float rotate;
		public float Rotate
		{
			get { return rotate; }
			set
			{
				if (value == rotate) return;
				rotate = value;
				Trans = true;
			}
		}
		public override Vector2 Position
		{
			get
			{
				return base.Position;
			}
			set
			{
				if (value == base.Position) return;
				base.Position = value;
				Trans = true;
			}
		}

		public int Count { get { return vertices.Length; } }

		bool Trans;
		CollisionCircle outCircle;//外接円
		public CollisionCircle OutCircle { get { return outCircle; } }
		CollisionCircle inCircle;//内接円

		public CollisionPolygon(Vector2[] v)
		{
			vertices = (Vector2[])v.Clone();
			Trans = true;
			temp = new Vector2[v.Length];

			float or = 0, ir = float.MaxValue;
			float t;
			foreach (var item in vertices)
			{
				t = item.LengthSquared();
				or = MathHelper.Max(or, t);
				ir = MathHelper.Min(ir, t);
			}
			or = (float)System.Math.Sqrt(or);
			ir = (float)System.Math.Sqrt(ir);
			outCircle = new CollisionCircle(or, Position);
			inCircle = new CollisionCircle(ir, Position);
		}

		public void Apply()
		{
			if (!Trans) return;
			for (int i = 0; i < vertices.Length; i++)
			{
				temp[i] = vertices[i].Rotate(rotate) + Position;
			}
			outCircle.Position = inCircle.Position = Position;
		}

		Vector2 Side(int num)
		{
			return temp[(num + 1) % temp.Length] - temp[num % temp.Length];
		}

		CollisionLine Line(int num)
		{
			return new CollisionLine(temp[num % temp.Length], temp[(num + 1) % temp.Length] - temp[num % temp.Length]);
		}

		internal override bool GetCollisionLine(CollisionLine obj)
		{
			Apply();
			Vector2 end = obj.End;
			if (!outCircle.GetCollision(obj)) return false;
			if (inCircle.GetCollision(obj)) return true;
			for (int i = 0; i < temp.Length; i++)
			{
				if (obj.GetCollision(Line(i))) return true;
			}
			return false;
		}

		internal override bool GetCollisionPolygon(CollisionPolygon obj)
		{
			obj.Apply();
			Apply();
			if (!outCircle.GetCollision(obj.outCircle)) return false;
			if (inCircle.GetCollision(obj.inCircle)) return true;

			if (GetCollisionPoint(new CollisionPoint(obj.Position))) return true;
			if (obj.GetCollisionPoint(new CollisionPoint(Position))) return true;
			for (int i = 0; i < temp.Length; i++)
			{
				for (int j = 0; j < obj.temp.Length; j++)
				{
					if (Line(i).GetCollision(obj.Line(j))) return true;
				}
			}
			return false;
		}

		internal override bool GetCollisionCircle(CollisionCircle obj)
		{
			Apply();
			if (!outCircle.GetCollision(obj)) return false;
			if (inCircle.GetCollision(obj)) return true;
			if (GetCollisionPoint(new CollisionPoint(obj.Position))) return true;//円が完全に入っている
			for (int i = 0; i < temp.Length; i++)//辺と円が交わる
			{
				if (Line(i).GetCollision(obj)) return true;
			}
			foreach (var item in temp)//頂点と円が交わる
			{
				if (new CollisionPoint(item).GetCollision(obj)) return true;
			}
			return false;
		}

		internal override bool GetCollisionPoint(CollisionPoint obj)
		{
			Apply();
			if (!outCircle.GetCollision(obj)) return false;
			if (inCircle.GetCollision(obj)) return true;
			bool neg;
			neg = MathUtilXNA.Cross(Side(0), obj.Position - temp[0]) < 0;
			for (int i = 1; i < temp.Length; i++)
			{
				if (neg != (MathUtilXNA.Cross(Side(i), obj.Position - temp[i]) < 0)) return false;
			}
			return true;
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
