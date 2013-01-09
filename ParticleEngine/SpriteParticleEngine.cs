using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Masa.ParticleEngine
{
	

	public class SpriteParticleEngine : ParticleEngineBase
	{
		int count;
		readonly SpriteBatch spriteBatch;

		public SpriteParticleEngine(SpriteBatch sprite, Texture2D tex, ushort number)
			: base(tex, number)
		{
			spriteBatch = sprite;
			Enable = true;
		}


		public override void Draw()
		{
			if (!Enable) return;
			for (int i = 0; i < Vertex.Length; i++)
			{
				int t = Time - (int)Vertex[i].Time;
				Vector3 a = Vertex[i].Alpha;
				float alpha = a.X + a.Y * t + a.Z * t * t * .5f;
				if (alpha > 0)
				{
					Vector3 pos = (Vertex[i].Position + Vertex[i].Velocity * t + Vertex[i].Accel * t * t * 0.5f);
					spriteBatch.Draw(Texture, new Rectangle((int)(pos.X - Vertex[i].Radius.X), (int)(pos.Y - Vertex[i].Radius.Y), (int)(Vertex[i].Radius.X * 2), (int)(Vertex[i].Radius.Y * 2)), new Color(BlendColor * alpha));
				}
			}
		}

		public override void Make(ParticleParameter param)
		{
			if (!CheckMake())
			{
				return;
			}
			Set(count % Vertex.Length, param);
			count++;
		}





	}
}
