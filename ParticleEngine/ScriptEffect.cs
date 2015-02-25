using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Masa.ScriptEngine;
using Microsoft.Xna.Framework;
using Masa.Lib;

namespace Masa.ParticleEngine
{
	internal class ScriptEffect : ScriptEffectBase
	{
		ScriptRunner Script;

		#region Script

		/// <summary>
		/// 位置や速度加速度は複数オプション指定の場合すべての効果が加算される
		/// Theta=xy2次元での角度、Phi=3次元での角度(省略した場合0)
		/// </summary>
		[ScriptMember("make",
			OptionName = new[] { "pos", "apos", "posa", "vel", "vela", "ac", "aca", "acv", "alp", "r", "angle", "color" },
			OptionArgNum = new[] { 3, 3, 3, 3, 3, 3, 3, 1, 3, 2, 2, 1 })]
		protected void Make(float name,
			float relativePosX, float relativePosY, float relativePosZ,
			float absolutePosX, float absolutePosY, float absolutePosZ,
			float relativePosRadius, float relativePosTheta, float relativePosPhi,
			float velX, float velY, float velZ,
			float velRadius, float velTheta, float velPhi,
			float accelX, float accelY, float accelZ,
			float accelRadius, float accelTheta, float accelPhi,
			float accelVelRate,
			float alphaInitial, float alphaVel, float alphaAccel,
			float radiusX, float radiusY,
			float angle, float angleVel,
			Vector3 color)
		{
			Vector3 p = Position;
			Vector3 vel = Vector3.Zero;
			Vector3 ac = Vector3.Zero;
			Vector2 radius;
			if (alphaInitial.IsNan() || alphaVel.IsNan() || alphaAccel.IsNan() || radiusX.IsNan()) throw new ArgumentException();
			if (!absolutePosX.IsNan())
			{
				p = new Vector3(absolutePosX, absolutePosY, absolutePosZ.ValueOr0());
			}
			if (!relativePosX.IsNan())
			{
				if (relativePosZ.IsNan()) relativePosZ = 0;
				p += new Vector3(relativePosX, relativePosY, relativePosZ);
			}
			if (!relativePosRadius.IsNan())
			{
				p += MakeVector(relativePosRadius, relativePosTheta, relativePosPhi);
			}
			if (!velRadius.IsNan())
			{
				vel += MakeVector(velRadius, velTheta, velPhi);
			}
			if (!velX.IsNan())
			{
				vel += new Vector3(velX, velY, velZ.ValueOr0());
			}
			if (!accelRadius.IsNan())
			{
				ac += MakeVector(accelRadius, accelTheta, accelPhi);
			}
			if (!accelVelRate.IsNan())
			{
				ac += vel / accelVelRate;
			}
			if (!accelX.IsNan())
			{
				ac += new Vector3(accelX, accelY, accelZ.ValueOr0());
			}
			if (radiusY.IsNan())
			{
				radius = new Vector2(radiusX);
			}
			else
			{
				radius = new Vector2(radiusX, radiusY);
			}

			Manager.MakeParticle((int)name, new ParticleParameter(p, vel, ac, alphaInitial, alphaVel, alphaAccel, radius, new Vector2(angle.ValueOr0(), angleVel.ValueOr0()), color == Vector3.Zero ? Vector3.One : color));
		}

		[ScriptMember("sound")]
		protected void PlaySound(string name)
		{
			Manager.SoundPlayer.PlaySound(name);
		}

		[ScriptMember("x0")]
		protected float _X { get { return Position.X; } }
		[ScriptMember("y0")]
		protected float _Y { get { return Position.Y; } }
		[ScriptMember("z0")]
		protected float _Z { get { return Position.Z; } }



		#endregion


		ScriptEffectManager Manager;

		public void Set(ScriptEffectManager man, Random rnd, string name, Vector3 pos, float[] args)
		{
			base.Set(rnd, pos, args);
			Manager = man;
			Script = Manager.Script.GetScript(this, name);
		}

		public void Set(ScriptEffectManager man, Random rnd, string name, Vector2 pos, float[] args)
		{
			this.Set(man, rnd, name, new Vector3(pos, 0), args);
		}

		public void Update()
		{
			Script.Update();
		}
	}
}
