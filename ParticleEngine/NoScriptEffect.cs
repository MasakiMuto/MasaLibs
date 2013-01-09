using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Masa.ScriptEngine;

namespace Masa.ParticleEngine
{
	public class NoScriptEffectBase : ScriptEffectBase
	{
		protected readonly ScriptRunnerEmulator ScriptRunner;
		NoScriptEffectManager manager;

		protected Vector2 Position2 { get { return new Vector2(Position.X, Position.Y); } }

		protected void Make(string name, Vector2 pos, Vector2 vel, Vector2 acc, float a0, float av, float ac, Vector2 r, float angle)
		{
			manager.MakeParticle(name, new ParticleParameter(pos, vel, acc, new Vector3(a0, av, ac), r, angle));
		}

		public NoScriptEffectBase()
			: base()
		{
			ScriptRunner = new ScriptRunnerEmulator();
		}

		public void Set(NoScriptEffectManager man, Random rnd, Vector3 pos, float[] args)
		{
			base.Set(rnd, pos, args);
			manager = man;
			ScriptRunner.Reset();
		}

		public void Set(NoScriptEffectManager man, Random rnd, Vector2 pos, float[] args)
		{
			this.Set(man, rnd, new Vector3(pos, 0), args);
		}
	

		public virtual void Update()
		{
			ScriptRunner.Update();
		}

	}
}
