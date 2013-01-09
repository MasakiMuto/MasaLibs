using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Masa.Lib.XNA
{
	/// <summary>
	/// 動作が怪しい
	/// </summary>
	/// <typeparam name="T"></typeparam>
	class WritableVertexBuffer<T> where T : struct, IVertexType
	{
		DynamicVertexBuffer buffer;
		int MaxElement;
		int Position;

		public DynamicVertexBuffer VertexBuffer
		{
			get
			{
				return buffer;
			}
		}

		public WritableVertexBuffer(GraphicsDevice device, int max)
		{
			buffer = new DynamicVertexBuffer(device, typeof(T), max, BufferUsage.WriteOnly);
			MaxElement = max;
		}

		public int SetData(T[] vertices)
		{
			if (vertices.Length == 0)
			{
				return 0;
			}
			//buffer.SetData(
			var op = SetDataOptions.NoOverwrite;
			int stride = buffer.VertexDeclaration.VertexStride;

			if (vertices.Length > MaxElement)
			{
				throw new Exception("頂点数が頂点バッファの最大格納数を超えています。");
			}

			if (Position + vertices.Length > MaxElement)
			{
				var before = vertices.SliceFromTo(0, MaxElement - Position - 1);
				var after = vertices.SliceFromTo(MaxElement - Position, vertices.Length - 1);
				SetData(before);

				op = SetDataOptions.NoOverwrite;
				Position = 0;
			}

			buffer.SetData(stride * Position, vertices, 0, vertices.Length, stride, op);
			Position += vertices.Length;

			return Position - vertices.Length;
		}
	}
}
