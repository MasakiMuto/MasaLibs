using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Masa.Lib.XNA
{
	public static class PostEffectCreater
	{
		public static VertexBuffer CreateVertexBuffer(GraphicsDevice graphics)
		{
			var vb = new VertexBuffer(graphics, typeof(VertexPositionTexture), 4, BufferUsage.WriteOnly);
			vb.SetData(new[]
			{
				new VertexPositionTexture(new Vector3(-1, 1, 0), new Vector2(0, 0)),
				new VertexPositionTexture(new Vector3(-1,-1, 0), new Vector2(0, 1)),
				new VertexPositionTexture(new Vector3( 1,-1, 0), new Vector2(1, 1)),
				new VertexPositionTexture(new Vector3( 1, 1, 0), new Vector2(1, 0))
			});
			return vb;
		}

		public static IndexBuffer CreateIndexBuffer(GraphicsDevice graphics)
		{
			var id = new IndexBuffer(graphics, IndexElementSize.SixteenBits, 6, BufferUsage.WriteOnly);
			id.SetData(new short[] { 2, 1, 0, 3, 2, 0 });
			return id;
		}

		public static void Draw(GraphicsDevice graphics)
		{
			graphics.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 4, 0, 2);
		}
	}
}
