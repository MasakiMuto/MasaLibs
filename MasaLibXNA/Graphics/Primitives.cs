using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Masa.Lib.XNA;

namespace Masa.Lib.XNA.Graphics
{
	public static class Primitives
	{
		/// <summary>
		/// xy方向半径z方向奥行きのリングを作る。半径1、奥行き1
		/// </summary>
		/// <param name="split">円周の分割数</param>
		/// <param name="coordMaxX">テクスチャX座標最大値。ループしないなら1</param>
		/// <returns></returns>
		public static Tuple<IEnumerable<VertexPositionTexture>, IEnumerable<short>> CreateBeltRing(int split, float coordMaxX)
		{
			var vertex = Enumerable.Range(0, split + 1)
			.Select(a =>
				Enumerable.Range(0, 2)
				.Select(z => new VertexPositionTexture(new Vector3(MathUtilXNA.GetVector(1, a * MathHelper.TwoPi / split), z), new Vector2(((float)a) / split * coordMaxX, z)))
			)
			.SelectMany(a => a);
			var index = Enumerable.Range(0, split)
				.Select(x => x * 2)
				.SelectMany(x => new[] { x, x + 1, x + 2, x + 2, x + 1, x + 3 })
				.Select(x => (short)x);
			return Tuple.Create(vertex, index);
		}
	}
}
