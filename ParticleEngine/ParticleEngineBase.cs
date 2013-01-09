using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Masa.ParticleEngine
{

	public abstract class ParticleEngineBase
	{
		internal enum ParticleState
		{
			Active,
			Suspend,
			Inactive,
		}
		ParticleState state;

		public bool Enable
		{
			get
			{
				return state == ParticleState.Active;
			}
			set
			{
				if (value)
				{
					state = ParticleState.Suspend;
				}
				else
				{
					state = ParticleState.Inactive;
				}
			}
		}
		public Vector4 BlendColor { get; set; }
		public ParticleBlendMode BlendMode { get; set; }
		protected int Time { get; private set; }
		protected readonly Texture2D Texture;
		protected ParticleVertex[] Vertex;
		protected ushort ParticleNum { get { return (ushort)Vertex.Length; } }
		public int Layer { get; set; }

		public ParticleEngineBase(Texture2D tex, ushort number)
		{
			Texture = tex;
			Vertex = new ParticleVertex[number];
			for (int i = 0; i < Vertex.Length; i++)
			{
				Vertex[i].Alpha.X = 0;
			}
			Time = 0;
			state = ParticleState.Suspend;
		}

		public virtual void Update()
		{
			//if (!Enable) return; //無効な状態でもTimeを回しておかないとScriptEffectManagerのcountとの同期がずれる
			Time++;
		}

		public abstract void Draw();

		public virtual void Clear()
		{
			for (int i = 0; i < Vertex.Length; i++)
			{
				Vertex[i].Alpha = Vector3.Zero;
			}
		}

		public abstract void Make(ParticleParameter param);

		protected bool CheckMake()
		{
			if (state == ParticleState.Suspend)
			{
				state = ParticleState.Active;
			}
			return state != ParticleState.Inactive;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="i">パーティクルを入れる場所</param>
		/// <param name="param"></param>

		protected void Set(int i, ParticleParameter param)
		{
			Vertex[i] = new ParticleVertex()
			{
				Position = param.Pos,
				Velocity = param.Vel,
				Accel = param.Ac,
				Radius = param.Radius,
				Alpha = param.Alpha,
				Time = this.Time,
				Angle = param.Angle,

			};
		}

	}

}
