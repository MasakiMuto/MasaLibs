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
		/// <returns>cull counter-clock</returns>
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

		/// <summary>
		/// 1*1*1 size
		/// </summary>
		/// <returns>cull counter-clock</returns>
		public static Tuple<Vector3[], short[]> CreateCube()
		{
			var v = new[]{
				new Vector3(1, 1, 1),
				new Vector3(1, -1, 1),
				new Vector3(-1, -1, 1),
				new Vector3(-1, 1, 1),
				
				new Vector3(1, 1, -1),
				new Vector3(1, -1, -1),
				new Vector3(-1, -1, -1),
				new Vector3(-1, 1, -1),
			};
			var i =new short[]{
				0, 1, 2, 0, 2, 3,
				6, 5, 4, 7, 6, 4,
				7, 4, 0, 7, 0, 3,
				6, 1, 5, 6, 2, 1,
				0, 5, 1, 0, 4, 5,
				7, 2, 6, 7, 3, 2
			};
			return Tuple.Create(v, i);
		}

	}
}
