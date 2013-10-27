using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using SharpDX.Toolkit.Graphics;
/*
namespace Masa.Lib.XNA
{
	public class GlowEffectDrawer : IDisposable
	{
		readonly GraphicsDevice GraphicsDevice;

		readonly Vector2[] texCoord;
		Vector2[] position;
		//VertexPositionTexture[] vertex;
		VertexBuffer vertexBuffer;
		readonly short[] index;
		IndexBuffer indexBuffer;
		readonly Texture2D texture;
		readonly Effect effect;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="graphicsDevice"></param>
		/// <param name="texture"></param>
		/// <param name="margin">UV座標の中心からの距離</param>
		/// <param name="spriteEffect">描画用SpriteBatchエフェクト</param>
		public GlowEffectDrawer(GraphicsDevice graphicsDevice, Texture2D texture, Vector2 margin, Effect spriteEffect)
		{
			this.GraphicsDevice = graphicsDevice;
			this.texture = texture;
			texCoord = new[]
				{
					0, .5f - margin.X, .5f + margin.X, 1,
					1, 1,
					1, .5f + margin.X, .5f - margin.Y, 0,
					0, 0,
					.5f - margin.X, .5f + margin.X, .5f + margin.X, .5f - margin.X
				}
				.Zip
				(
					new[]
					{
						0, 0, 0, 0,
						.5f - margin.Y, .5f + margin.Y,
						1, 1, 1, 1,
						.5f + margin.Y, .5f - margin.Y,
						.5f - margin.Y, .5f - margin.Y, .5f + margin.Y, .5f + margin.Y
					},
					(x, y) => new Vector2(x, y)
				)
				.ToArray();
			position = new Vector2[texCoord.Length];
			vertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionTexture), texCoord.Length, BufferUsage.WriteOnly);


			index = new short[]
			{
				0, 11, 12, 0, 12, 1, 
				1, 12, 13, 1, 13, 2,
				2, 13, 4, 2, 4, 3,
				13, 14, 5, 13, 5, 4,
				14, 7, 6, 14, 6, 5,
				15, 8, 7, 15, 7, 14,
				10, 9, 8, 10, 8, 15,
				11, 10, 15, 11, 15, 12,
				12, 15, 14, 12, 14, 13
			};
			indexBuffer = new IndexBuffer(GraphicsDevice, IndexElementSize.SixteenBits, index.Length, BufferUsage.WriteOnly);
			indexBuffer.SetData(index);

			effect = spriteEffect;
		}

		public GlowEffectDrawer(GraphicsDevice graphicsDevice, Texture2D texture, Effect spriteEffect)
			: this(graphicsDevice, texture, Vector2.Zero, spriteEffect)
		{

		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="blend">乗算される色</param>
		/// <param name="leftTop">左上座標</param>
		/// <param name="size">全体の大きさ</param>
		/// <param name="margin">周辺の幅</param>
		public void Draw(Color blend, Vector2 leftTop, Vector2 size, Vector2 margin)
		{
			SetPosition(leftTop, size, margin);
			vertexBuffer.SetData(position.Zip(texCoord, (p, t) => new VertexPositionTexture(new Vector3(p, 0), t)).ToArray());
			GraphicsDevice.SetVertexBuffer(vertexBuffer);
			GraphicsDevice.Indices = indexBuffer;

			GraphicsDevice.RasterizerState = RasterizerState.CullClockwise;
			GraphicsDevice.Textures[0] = texture;
			//GraphicsDevice.BlendState = BlendState.Additive;
			GraphicsDevice.DepthStencilState = DepthStencilState.None;
			effect.Parameters["TargetSize"].SetValue(new Vector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height));
			effect.Parameters["Color"].SetValue(blend.ToVector4());
			effect.CurrentTechnique.Passes[0].Apply();
			//GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, indexBuffer.IndexCount / 3);
			GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertexBuffer.VertexCount, 0, indexBuffer.IndexCount / 3);
		}

		void SetPosition(Vector2 leftTop, Vector2 size, Vector2 margin)
		{
			var rightBottom = leftTop + size;
			position[12] = leftTop;
			position[13] = new Vector2(rightBottom.X, leftTop.Y);
			position[14] = rightBottom;
			position[15] = new Vector2(leftTop.X, rightBottom.Y);
			position[0] = leftTop - margin;
			position[1] = new Vector2(leftTop.X, leftTop.Y - margin.Y);
			position[2] = new Vector2(rightBottom.X, leftTop.Y - margin.Y);
			position[3] = new Vector2(rightBottom.X + margin.X, leftTop.Y - margin.Y);
			position[4] = new Vector2(rightBottom.X + margin.X, leftTop.Y);
			position[5] = new Vector2(rightBottom.X + margin.X, rightBottom.Y);
			position[6] = rightBottom + margin;
			position[7] = new Vector2(rightBottom.X, rightBottom.Y + margin.Y);
			position[8] = new Vector2(leftTop.X, rightBottom.Y + margin.Y);
			position[9] = new Vector2(leftTop.X - margin.X, rightBottom.Y + margin.Y);
			position[10] = new Vector2(leftTop.X - margin.X, rightBottom.Y);
			position[11] = new Vector2(leftTop.X - margin.X, leftTop.Y);
			//vertex[2].Position = new Vector3(rightBottom.X, leftTop.
		}

		public void Dispose()
		{
			if (vertexBuffer != null)
			{
				vertexBuffer.Dispose();
				vertexBuffer = null;
			}
			if (indexBuffer != null)
			{
				indexBuffer.Dispose();
				indexBuffer = null;
			}
			GC.SuppressFinalize(this);
		}
	}
}
*/