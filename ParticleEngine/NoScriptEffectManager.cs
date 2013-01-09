using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Masa.Lib;

namespace Masa.ParticleEngine
{
	public class NoScriptEffectManager 
	{
		Dictionary<string, SpriteParticleEngine> Particles;
		readonly SpriteBatch sprite;
		IGrouping<ParticleBlendMode, SpriteParticleEngine>[] group;
		LinkedPoolObjectBaseManager<NoScriptEffectBase> effects;

		public NoScriptEffectManager(SpriteBatch spriteBatch, IEnumerable<ParticleManagerInitializer> particle)
		{
			Particles = particle.Select(p => new
			{
				Key = p.Name,
				Value = new SpriteParticleEngine(spriteBatch, p.Texture, p.Number)
				{
					BlendMode = p.Blend,
					BlendColor = p.color
				}
			}).ToDictionary(a => a.Key, a => a.Value);
			group = Particles.Select(i => i.Value).GroupBy(p => p.BlendMode).ToArray();
			sprite = spriteBatch;
			effects = new LinkedPoolObjectBaseManager<NoScriptEffectBase>();
		}

		public void AddEffects(IEnumerable<NoScriptEffectBase> items)
		{

			//effects.AddRange(items);
		}

		public void SetEffect<T>(Vector2 pos) where T : NoScriptEffectBase
		{

			//var e = effects.OfType<T>().FirstOrDefault(i=>!i.Flag);
			//if (e != null)
			//{
			//	e.Set(pos);
			//}
			//System.Diagnostics.Debug.Assert(e != null, "エフェクト作成器のキャラオーバー:" + typeof(T));//作れなかったらエラーを投げる
		}

		public void MakeParticle(string name, ParticleParameter param)
		{
			Particles[name].Make(param);
		}

		public void Update()
		{
			foreach (var item in effects.ActiveItems())
			{
				item.Update();
			}
			foreach (var item in Particles.Select(p => p.Value))
			{
				item.Update();
			}
		}

		public void Clear()
		{
			foreach (var item in Particles.Select(p => p.Value))
			{
				item.Clear();
			}
		}

		public void Draw()
		{
			foreach (var item in group)
			{
				sprite.Begin(SpriteSortMode.Deferred, ParticleBlendState.State(item.Key));
				foreach (var p in item)
				{
					p.Draw();
				}
				sprite.End();
			}
		}
	}
}
