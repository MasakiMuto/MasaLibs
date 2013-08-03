using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Masa.Lib;
using Masa.ScriptEngine;
using Masa.Lib.XNA;

namespace Masa.ParticleEngine
{
	public abstract class ScriptEffectBase : PoolObjectBase
	{
		protected Random rand;
		#region Script


		protected Vector3 MakeVector(float r, float theta, float phy)//極座標
		{
			if (phy.IsNan())
			{
				return new Vector3(MathUtilXNA.GetVector(r, theta), 0);
			}
			else
			{
				return MathUtilXNA.GetVector(r, theta, phy);
			}
		}

		[ScriptMember("vanish")]
		protected void Vanish()
		{
			Delete();
		}

		[ScriptMember("rand", OptionName = new[] { "max", "min" }, OptionArgNum = new[] { 1, 1 })]
		protected float Rand(float max, float min)
		{
			if (max.IsNan() && min.IsNan())
			{
				return rand.Next();
			}
			else if (max.IsNan())
			{
				return rand.Next() + min;
			}
			else if (min.IsNan())
			{
				return rand.Next((int)max);
			}
			else
			{
				return rand.Next((int)min, (int)max);
			}
		}

		[ScriptMember("randpm")]
		protected float RandPM()
		{
			return rand.NextPN();
		}


		/// <summary>
		/// 歪みrand
		/// </summary>
		/// <param name="max"></param>
		/// <param name="a"></param>
		/// <returns></returns>
		[ScriptMember("randdis")]
		protected float RandDist(float max, float a)
		{
			return rand.NextDistorted(max, a);
		}

		[ScriptMember("randnml")]
		protected float RandNormal(float average, float dis)
		{
			return (float)rand.NextNormal(average, dis);
		}

		/// <summary>
		/// 0-1の実数乱数
		/// </summary>
		/// <returns></returns>
		[ScriptMember("rand1")]
		protected float Rand1()
		{
			return (float)rand.NextDouble();
		}

		protected float[] Args;
		[ScriptMember("arg0")]
		protected float Arg0
		{
			get
			{
				if (Args == null) return 0;
				return Args[0];
			}
		}
		[ScriptMember("arg1")]
		protected float Arg1
		{
			get
			{
				if (Args == null) return 0;
				return Args[1];
			}
		}
		[ScriptMember("arg2")]
		protected float Arg2
		{
			get
			{
				if (Args == null) return 0;
				return Args[2];
			}
		}

		#endregion


		protected Vector3 Position;

		

		protected void Set(Random rnd, Vector3 pos, float[] args)
		{
			base.Set();
			rand = rnd;
			Position = pos;
			Args = args;
		}

		protected void Set(Random rnd, Vector2 pos, float[] args)
		{
			this.Set(rnd, new Vector3(pos, 0), args);
		}

	}
}
