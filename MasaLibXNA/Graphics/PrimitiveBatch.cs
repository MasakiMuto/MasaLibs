using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Masa.Lib.XNA
{
	public class PrimitiveBatch
	{
		readonly GraphicsDevice graphics;
		readonly Effect effect;
		static readonly RasterizerState wire;
		EffectParameter effectColor, effectSize;
		static VertexDeclaration declaration;

		static PrimitiveBatch()
		{
			wire = new RasterizerState()
			{
				CullMode = CullMode.None,
				FillMode = FillMode.WireFrame,
				
			};
			declaration = new VertexDeclaration(new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0));
		}

		public PrimitiveBatch(GraphicsDevice graphics, Effect primitiveEffect)
		{
			this.graphics = graphics;
			effect = primitiveEffect;
			effectColor = effect.Parameters["Color"];
			effectSize = effect.Parameters["Size"];
		}



		//public void BeginWire()
		//{
		//    graphics.RasterizerState = wire;
			
		//}

		void SetStates(Color color, Vector2 targetSize)
		{
			graphics.RasterizerState = wire;
			effectColor.SetValue(color.ToVector4());
			effectSize.SetValue(new Vector2(1f / targetSize.X, 1f / targetSize.Y));
			effect.CurrentTechnique.Passes[0].Apply();
		}

		public void Draw(Vector2[] vertex, Color color, Vector2 targetSize)
		{
			SetStates(color, targetSize);
			graphics.DrawUserPrimitives(PrimitiveType.LineStrip, vertex, 0, vertex.Length - 1, declaration);

			///graphics.DrawUserPrimitives(PrimitiveType.LineStrip, 
		}

		public void DrawLines(Vector2[] vertex, Color color, Vector2 targetSize)
		{
			SetStates(color, targetSize);
			graphics.DrawUserPrimitives(PrimitiveType.LineList, vertex, 0, vertex.Length / 2, declaration);
		}


	}
}
