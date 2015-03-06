using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Masa.Lib;

namespace Masa.Lib.XNA
{
	/// <summary>
	/// ホーミングレーザーなどの描画器
	/// </summary>
	public class BeltDrawer
	{
		struct BeltVertex : IVertexType
		{
			Vector2 Position;
			Vector2 Texture;
			Color Color;

			public BeltVertex(Vector2 pos, Vector2 tex, Color color)
			{
				Position = pos;
				Texture = tex;
				Color = color;
			}

			static VertexDeclaration declaration = new VertexDeclaration(
				new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),
				new VertexElement(8, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
				new VertexElement(16, VertexElementFormat.Color, VertexElementUsage.Color, 0)
				);

			public VertexDeclaration VertexDeclaration
			{
				get { return declaration; }
			}
		}

		BeltVertex[] buffer;
		short[] index;
		GraphicsDevice graphics;
		Effect beltEffect;
		ushort lastPosition;
		int lastIndex;
		Texture2D texture;
		EffectParameter halfTargetSizeInv;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="bufferSize">最大頂点数。2～32767</param>
		/// <param name="graphics"></param>
		/// <param name="beltEffect"></param>
		public BeltDrawer(short bufferSize, GraphicsDevice graphics, Effect beltEffect)
		{
			buffer = new BeltVertex[bufferSize * 2];
			if (buffer.Length > short.MaxValue)
			{
				throw new Exception("バッファサイズが大きすぎる。最大" + short.MaxValue / 2);
			}
			index = new short[(bufferSize - 1) * 2 * 3];
			this.graphics = graphics;
			this.beltEffect = beltEffect;
			halfTargetSizeInv = this.beltEffect.Parameters["halfTargetSizeInv"];
		}

		/// <summary>
		/// 帯の描画をセットする
		/// </summary>
		/// <param name="vertex">帯の中心の各頂点</param>
		/// <param name="width">帯の太さ(端から端まで)</param>
		public void SetVertex(Vector2[] vertex, float width, Color color)
		{
			SetVertex(vertex, vertex.Length, width, color);
		}

		/// <summary>
		/// 帯の描画をセットする
		/// </summary>
		/// <param name="vertex">帯の中心の各頂点</param>
		/// <param name="width">帯の太さ(端から端まで)</param>
		/// <param name="count">先頭からの使う頂点数</param>
		public void SetVertex(Vector2[] vertex, int count, float width, Color color)
		{
			if (!CheckBufferSize(count))
			{
				return;
			}
			Vector2 dir = Vector2.Zero;
			for (int i = 0; i < count; i++)
			{
				if (i != count - 1)
				{
					dir = GetWidthVector(vertex[i], vertex[i + 1], width);
					PutRectIndex();
				}
				PutVertex(vertex[i], dir, color, 1f / count * i);
				
			}
		}

		public void SetVertexLoop(Vector2[] vertex, float width, Color color)
		{
			SetVertexLoop(vertex, vertex.Length, width, color);
		}

		public void SetVertexLoop(Vector2[] vertex, int count, float width, Color color)
		{
			if (!CheckBufferSize(count))
			{
				return;
			}
			int first = lastPosition;
			for (int i = 0; i < count; i++)
			{
				Vector2 dir;
				if (i == count - 1)
				{
					dir = GetWidthVector(vertex[i], vertex[0], width);
					PutIndex(lastPosition);
					PutIndex(lastPosition + 1);
					PutIndex(first);
					PutIndex(first);
					PutIndex(lastPosition + 1);
					PutIndex(first + 1);
				}
				else
				{
					dir = GetWidthVector(vertex[i], vertex[i + 1], width);
					PutRectIndex();
				}
				PutVertex(vertex[i], dir, color, 1f / count * i);

			}

		}

		public void SetTexture(Texture2D tex)
		{
			texture = tex;
		}

		#region internal vertex

		bool CheckBufferSize(int vertexLength)
		{
			if (lastPosition + vertexLength * 2 > buffer.Length)
			{
				return false;
				//throw new Exception("バッファ溢れ");
			}
			else return true;
		}


		
		void PutIndex(int value)
		{
			index[lastIndex] = (short)value;
			lastIndex++;
		}

		void PutRectIndex()
		{
			PutIndex(lastPosition);
			PutIndex(lastPosition + 1);
			PutIndex(lastPosition + 2);
			PutIndex(lastPosition + 2);
			PutIndex(lastPosition + 1);
			PutIndex(lastPosition + 3);
		}

		void PutVertex(Vector2 vertex, Vector2 dir, Color color, float tex)
		{
			buffer[lastPosition] = new BeltVertex(vertex + dir, new Vector2(tex, 0), color);
			lastPosition++;
			buffer[lastPosition] = new BeltVertex(vertex - dir, new Vector2(tex, 1), color);
			lastPosition++;
		}

		Vector2 GetWidthVector(Vector2 v0, Vector2 v1, float width)
		{
			var v = Vector2.Normalize(v1 - v0) * width * .5f;
			if (v.X.IsNan() || v.Y.IsNan())
			{
				v = new Vector2(width * .5f, 0);
			}
			return new Vector2(v.Y, -v.X);
		}

		#endregion

		public void Draw(Vector2 targetSize)
		{
			Draw(targetSize, Vector2.Zero);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="targetSize">レンダリングターゲットの大きさ</param>
		public void Draw(Vector2 targetSize, Vector2 offset)
		{
			if (lastPosition == 0)
			{
				return;
			}
			EffectPass pass;
			if (texture != null)
			{
				graphics.Textures[0] = texture;
				pass = beltEffect.CurrentTechnique.Passes[1];
			}
			else
			{
				pass = beltEffect.CurrentTechnique.Passes[0];
			}
			beltEffect.Parameters["offset"].SetValue(offset);
			halfTargetSizeInv.SetValue(targetSize.Inverse() * 2f);
			graphics.DepthStencilState = DepthStencilState.None;
			graphics.BlendState = BlendState.AlphaBlend;
			graphics.RasterizerState = RasterizerState.CullNone;
			pass.Apply();
			graphics.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, buffer, 0, lastPosition, index, 0, lastIndex / 3);
			
			lastPosition = 0;
			lastIndex = 0;
		}
	}
}
