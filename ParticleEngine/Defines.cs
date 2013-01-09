using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Masa.ParticleEngine
{
	public enum ParticleMode
	{
		TwoD,
		ThreeD,
	}

	/// <summary>
	/// パーティクルの初期位相を表す構造体
	/// </summary>
	public struct ParticleParameter
	{
		internal Vector3 Pos, Vel, Ac;
		internal Vector3 Alpha;
		internal Vector2 Radius;
		internal Vector2 Angle;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="po">初期座標</param>
		/// <param name="ve">初期速度</param>
		/// <param name="ac">加速度</param>
		/// <param name="al">アルファの初期値、初期変化量、変化量の変化量(アルファ値についての位置、速度、加速度ということ)</param>
		/// <param name="r"></param>
		public ParticleParameter(Vector3 po, Vector3 ve, Vector3 ac, Vector3 al, Vector2 r, Vector2 angle)
		{
			Pos = po;
			Vel = ve;
			Ac = ac;
			Alpha = al;
			Radius = r;
			Angle = angle;
		}

		public ParticleParameter(Vector2 po, Vector2 ve, Vector2 ac, Vector3 al, Vector2 r, Vector2 angle) : this(new Vector3(po, 0), new Vector3(ve, 0), new Vector3(ac, 0), al, r, angle)
		{
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="po">初期座標</param>
		/// <param name="ve">初期速度</param>
		/// <param name="ac">加速度</param>
		/// <param name="a0">アルファの初期値</param>
		/// <param name="va">アルファの初期変化量</param>
		/// <param name="aa">アルファの変化量の変化量</param>
		/// <param name="r"></param>
		public ParticleParameter(Vector3 po, Vector3 ve, Vector3 ac, float a0, float va, float aa, Vector2 r, Vector2 angle)
			: this(po, ve, ac, new Vector3(a0, va, aa), r, angle)
		{

		}
	}

	public enum ParticleBlendMode
	{
		Add,
		Subtract,
		Mul,
		Alpha
	}

	public class ParticleManagerInitializer
	{
		internal Texture2D Texture;
		internal string Name;
		internal ushort Number;
		internal Vector4 color;
		internal bool Disabled;
		public readonly ParticleBlendMode Blend;
		public readonly int Layer;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="tex">パーティクルの画像</param>
		/// <param name="name">ScriptEffectManagerから生成するときの識別名</param>
		/// <param name="num">パーティクル最大数。多いほど描画負荷が大きくなる</param>
		public ParticleManagerInitializer(Texture2D tex, string name, ushort num, Color col)
			: this(tex, name, num, col.ToVector4())
		{

		}

		public ParticleManagerInitializer(Texture2D tex, string name, ushort num, Vector4 vec)
			: this(tex, name, num, vec, ParticleBlendMode.Add, 0)
		{

		}

		public ParticleManagerInitializer(Texture2D tex, string name, ushort num, Vector4 vec, ParticleBlendMode mode, int layer)
		{
			Texture = tex;
			Name = name;
			Number = num;
			color = vec;
			Blend = mode;
			Layer = layer;
		}

		public ParticleManagerInitializer(Texture2D tex, string name, ushort num, Color col, ParticleBlendMode mode)
			: this(tex, name, num, col.ToVector4(), mode, 0)
		{
		}

		public ParticleManagerInitializer(Texture2D tex, string name, ushort num)
			: this(tex, name, num, Vector4.One)
		{

		}

		/// <summary>
		/// ダミーロード
		/// </summary>
		/// <param name="name"></param>
		public ParticleManagerInitializer(string name)
		{
			Name = name;
			Disabled = true;
		}
	}

	public static class ParticleBlendState
	{
		static BlendState[] blendStates;
		const int BlendModeNum = 4;

		static ParticleBlendState()
		{
			blendStates = new BlendState[BlendModeNum];
			blendStates[(int)ParticleBlendMode.Add] = new BlendState()
			{
				AlphaBlendFunction = BlendFunction.Add,
				ColorBlendFunction = BlendFunction.Add,
				ColorDestinationBlend = Blend.One,
				AlphaDestinationBlend = Blend.One,
				ColorSourceBlend = Blend.One,
				AlphaSourceBlend = Blend.One,
			};
			blendStates[(int)ParticleBlendMode.Alpha] = BlendState.AlphaBlend;
			blendStates[(int)ParticleBlendMode.Subtract] = new BlendState()
			{
				AlphaBlendFunction = BlendFunction.ReverseSubtract,
				ColorBlendFunction = BlendFunction.ReverseSubtract,
				AlphaDestinationBlend = Blend.One,
				ColorDestinationBlend = Blend.One,
				ColorSourceBlend = Blend.SourceAlpha,
				AlphaSourceBlend = Blend.SourceAlpha,
				//AlphaDestinationBlend = Blend.InverseDestinationAlpha,
				//ColorDestinationBlend = Blend.InverseDestinationColor,
				ColorWriteChannels = ColorWriteChannels.Red,
				ColorWriteChannels1 = ColorWriteChannels.Green,
				ColorWriteChannels2 = ColorWriteChannels.Blue,
			};
			blendStates[(int)ParticleBlendMode.Mul] = new BlendState()
			{
				ColorWriteChannels = ColorWriteChannels.Red | ColorWriteChannels.Green | ColorWriteChannels.Blue,
				ColorBlendFunction = BlendFunction.Add,
				AlphaBlendFunction = BlendFunction.Add,
				ColorDestinationBlend = Blend.SourceColor,
				AlphaDestinationBlend = Blend.SourceColor,
				ColorSourceBlend = Blend.Zero,
				AlphaSourceBlend = Blend.Zero

			};
		}

		public static BlendState State(ParticleBlendMode blend)
		{
			return blendStates[(int)blend];
		}
	}

}
