using Microsoft.Xna.Framework;

namespace Masa.Lib.XNA.Collision
{
	public abstract class Collision
	{
		public virtual Vector2 Position { get; set; }

		public bool GetCollision(Collision obj)
		{
			//if (obj == null) return false;
			if (obj is CollisionCircle)
			{
				return GetCollisionCircle((CollisionCircle)obj);
			}
			if (obj is CollisionRect)
			{
				return GetCollisionRect((CollisionRect)obj);
			}
			if (obj is CollisionPolygon)
			{
				return GetCollisionPolygon((CollisionPolygon)obj);
			}
			if (obj is CollisionLine)
			{
				return GetCollisionLine((CollisionLine)obj);
			}
			if (obj is CollisionPoint)
			{
				return GetCollisionPoint((CollisionPoint)obj);
			}
			return false;
		}

		abstract internal bool GetCollisionCircle(CollisionCircle obj);
		abstract internal bool GetCollisionLine(CollisionLine obj);
		abstract internal bool GetCollisionPolygon(CollisionPolygon obj);
		abstract internal bool GetCollisionPoint(CollisionPoint obj);
		abstract internal bool GetCollisionRect(CollisionRect obj);
		abstract internal bool GetCollisionChainCircle(CollisionChainCircle obj);
		protected float Pow2(float value)
		{
			return value * value;
		}
	}

	

	

	

	

	

	//public class CollisionLine : Collision
	//{
	//    float width;
	//    public float Width
	//    {
	//        get
	//        {
	//            return width;
	//        }
	//        set
	//        {
	//            width = value;
	//            if (width < 0) width = 0;
	//        }
	//    }
	//    public Vector2 Line;
	//    public Vector2 End///終点
	//    {
	//        get { return Position + Line; }
	//    }

	//    public CollisionLine(Vector2 pos, Vector2 line, float width)
	//    {
	//        Position = pos;
	//        Line = line;
	//        Width = width;
	//    }

	//    protected override bool GetCollisionCircle(CollisionCircle obj)
	//    {
	//        float rd2 = Pow2(obj.Radius + Width);
	//        Vector2 ao = obj.Position - Position;
	//        Vector2 bo = obj.Position - End;

	//        if (ao.LengthSquared() <= rd2 || bo.LengthSquared() <= rd2) return true;
	//        if (!(Vector2.Dot(-Line, ao) * Vector2.Dot(Line, bo) >= 0)) return false;
	//        if ((Line.Y * obj.Position.X + Line.X * obj.Position.Y + Position.X * Line.Y - Position.Y * Line.X)
	//            / Line.LengthSquared() <= rd2) return true;
	//        return false;
	//    }

	//    /**
	//     * 太さのある線分同士は未実装
	//    **/
	//    protected override bool GetCollisionLine(CollisionLine obj)
	//    {
	//        if (Vector2.Dot(Line, obj.Position - Position) * Vector2.Dot(Line, obj.End - Position) <= 0
	//            && Vector2.Dot(obj.Line, Position - obj.Position) * Vector2.Dot(obj.Line, End - obj.Position) <= 0) return true;
	//        return false;
	//    }

	//    protected override bool GetCollisionPolygon(CollisionPolygon obj)
	//    {
	//        return obj.GetCollision(this);
	//    }

	//    protected override bool GetCollisionPoint(CollisionPoint obj)
	//    {
	//        throw new System.NotImplementedException();
	//    }
	//}
}
