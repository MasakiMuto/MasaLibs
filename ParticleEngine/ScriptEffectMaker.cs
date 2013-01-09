using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Masa.Lib;
using Masa.Lib.XNA;
using Masa.ScriptEngine;
using Microsoft.Xna.Framework;

namespace Masa.ParticleEngine
{
	/// <summary>
	/// パーティクル生成スクリプトを生成するスクリプトの実行機
	/// </summary>
	public class ScriptEffectMaker
	{
		#region Script
		public static string[] Table;
		string LookUp(float name)
		{
			return Table[(int)name];
		}

		[ScriptMember("set", OptionName = new[] { "pos", "posa" }, OptionArgNum = new[] { 3, 3 })]
		protected void Set(float name, float x, float y, float z, float r, float theta, float phy)
		{
			Vector3 p = Position;

			if (!x.IsNan())
			{
				if (z.IsNan()) z = 0;
				p.X += x;
				p.Y += y;
				p.Z += z;
			}
			else if (!r.IsNan())
			{
				if (phy.IsNan())
				{
					p += new Vector3(MathUtilXNA.GetVector(r, theta), 0);
				}
				else
				{
					p += MathUtilXNA.GetVector(r, theta, phy);
				}
			}

			manager.Set(LookUp(name), p);
		}

		[ScriptMember("rand")]
		protected float Rand()
		{
			return manager.Random.Next();
		}

		[ScriptMember("vanish")]
		protected void Vanish()
		{
			vanish = true;
		}

		#endregion

		ScriptRunner Script;
		ScriptEffectManager manager;
		bool vanish;
		public Vector3 Position { get; set; }
		public ScriptEffectMaker(ScriptEffectManager sem, string name, Vector3 pos)
		{
			vanish = false;
			manager = sem;
			Script = manager.Script.GetScript(this, name);
			Position = pos;
		}

		public virtual bool Update()
		{
			Script.Update();
			return !vanish;
		}

	}
}
