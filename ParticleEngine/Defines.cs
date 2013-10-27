using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using SharpDX.Toolkit.Graphics;

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
		internal Vector3 Color;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="po">初期座標</param>
		/// <param name="ve">初期速度</param>
		/// <param name="ac">加速度</param>
		/// <param name="al">アルファの初期値、初期変化量、変化量の変化量(アルファ値についての位置、速度、加速度ということ)</param>
		/// <param name="r"></param>
		public ParticleParameter(Vector3 po, Vector3 ve, Vector3 ac, Vector3 al, Vector2 r, Vector2 angle, Vector3 color)
		{
			Pos = po;
			Vel = ve;
			Ac = ac;
			Alpha = al;
			Radius = r;
			Angle = angle;
			Color = color;
		}

		public ParticleParameter(Vector2 po, Vector2 ve, Vector2 ac, Vector3 al, Vector2 r, Vector2 angle, Vector3 color)
			: this(new Vector3(po, 0), new Vector3(ve, 0), new Vector3(ac, 0), al, r, angle, color)
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
		public ParticleParameter(Vector3 po, Vector3 ve, Vector3 ac, float a0, float va, float aa, Vector2 r, Vector2 angle, Vector3 color)
			: this(po, ve, ac, new Vector3(a0, va, aa), r, angle, color)
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

	
}
