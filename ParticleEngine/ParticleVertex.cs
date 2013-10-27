using System;
using System.Collections.Generic;
using System.Text;
using SharpDX;
using SharpDX.Toolkit.Graphics;

namespace Masa.ParticleEngine
{
	internal struct ParticleIndexVertex 
	{
		public Vector2 Tex;
		/*
		public readonly static VertexDeclaration Decla = new VertexDeclaration(
			new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)
			);

		public VertexDeclaration VertexDeclaration
		{
			get { return Decla; }
		}
	*/
		public ParticleIndexVertex(Vector2 tex)
		{
			Tex = tex;
		}
	}

	public struct ParticleVertex 
	{
		public Vector3 Position;
		public Vector3 Velocity;
		public Vector3 Accel;
		public Vector3 Alpha;///Alphaのvalue, speed, accel
		public Vector2 Radius;
		//public float Index;
		public float Time;
		public Vector2 Angle;
		public Vector3 Color;
		//public Vector2 TexCoord;
		 

		public static int SizeInBytes
		{
			get
			{
				return 68;
			}
		}
		/*
		public readonly static VertexDeclaration Declaration = new VertexDeclaration
			(
				new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
				new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Position, 1),
				new VertexElement(24, VertexElementFormat.Vector3, VertexElementUsage.Position, 2),
				new VertexElement(36, VertexElementFormat.Vector3, VertexElementUsage.Color, 0),
				new VertexElement(48, VertexElementFormat.Vector2, VertexElementUsage.PointSize, 0),
				new VertexElement(56, VertexElementFormat.Single, VertexElementUsage.BlendWeight, 0),
				new VertexElement(60, VertexElementFormat.Vector2, VertexElementUsage.Position, 3),
				new VertexElement(68, VertexElementFormat.Vector3, VertexElementUsage.Color, 1)
			);
					//new VertexElement(48, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0));
					//new VertexElement(52, VertexElementFormat.Single, VertexElementUsage.BlendIndices, 0));
		//					new VertexElement(48, VertexElementFormat.Single, VertexElementUsage.BlendIndices, 1));

		public VertexDeclaration VertexDeclaration
		{
			get
			{
				return Declaration;
			}
		}
		 */
	}
}
