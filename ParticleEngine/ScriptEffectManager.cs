using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Masa.Lib;

namespace Masa.ParticleEngine
{


	/// <summary>
	/// スクリプト管理されたパーティクルエフェクトの総合管理クラス
	/// </summary>
	public class ScriptEffectManager
	{
		enum EffectParam
		{
			Projection,
			ViewProjection,
			Offset,
			Time,
			TargetSize
		}

		//ScriptEffect[] effects;
		LinkedPoolObjectBaseManager<ScriptEffect> effects;
		ParticleEngineBase[] particles;
		readonly string[] textureTable;//ScriptEffectのスクリプト生成に使うTable
		internal readonly ScriptEngine.ScriptManager Script;
		//static readonly Matrix Default2DView = Matrix.CreateLookAt(new Vector3(0, 0, 10), Vector3.Zero, Vector3.Up);
		protected readonly GraphicsDevice Device;
		internal Random Random;
		Viewport viewport;
		readonly Effect particleEffect;
		int count;
		public Matrix Projection { get; set; }
		readonly Vector2 FieldSize;
		ParticleMode mode;
		public Vector2 Offset { get; set; }
		readonly EffectParameter[] effectParams;

		public Effect ParticleEffect
		{
			get { return particleEffect; }
		}

		public string[] TextureTable
		{
			get { return textureTable; }
		}

		public static Matrix GetDefault2DProjection(float width, float height)
		{
			return Matrix.CreateOrthographic(width, height, -10, 100);
		}

		public ScriptEffectManager(ScriptEngine.ScriptManager script, GraphicsDevice device, Effect effect,
			ParticleMode mode, Matrix projection, Vector2 filedSize,
			IEnumerable<ParticleManagerInitializer> particle, Random rnd)
		{
			viewport = new Viewport(0, 0, (int)filedSize.X, (int)filedSize.Y);
			Script = script;
			effects = new LinkedPoolObjectBaseManager<ScriptEffect>();
			if (rnd == null)
			{
				rnd = new System.Random();//乱数のインスタンスを指定しないときは勝手に作る
			}
			Random = rnd;
			particles = particle.Select(p =>
				{
					if (p.Disabled)
					{
						return null;
					}
					else
					{
						return new ParticleEngine(effect, device, p.Texture, p.Number, mode, projection, filedSize)
							{
								BlendColor = p.color,
								BlendMode = p.Blend,
								Layer = p.Layer
							};
					}
				}).ToArray();
			textureTable = particle.Select(p => p.Name).ToArray();
			Device = device;
			particleEffect = effect;
			Projection = projection;
			FieldSize = filedSize;
			this.mode = mode;
			effectParams = new EffectParameter[Enum.GetValues(typeof(EffectParam)).Length];
			Action<EffectParam, string> setParam = (p, name) => effectParams[(int)p] = particleEffect.Parameters[name];
			setParam(EffectParam.Projection, "Projection");
			setParam(EffectParam.ViewProjection, "ViewProjection");
			setParam(EffectParam.Offset, "Offset");
			setParam(EffectParam.Time, "Time");
			setParam(EffectParam.TargetSize, "TargetSize");
		}



		


		/// <summary>
		/// 
		/// </summary>
		/// <param name="script"></param>
		/// <param name="device"></param>
		/// <param name="effect">描画に使用するシェーダ</param>
		/// <param name="mode"></param>
		/// <param name="width">画面幅(レンダリングターゲットの幅)</param>
		/// <param name="height">画面高さ(レンダリングターゲットの高さ)</param>
		/// <param name="particle">使用する各パーティクルエンジンの設定</param>
		/// <param name="rnd">エフェクトから乱数を使うときの乱数発生器。特に指定しないならnull</param>
		public ScriptEffectManager(ScriptEngine.ScriptManager script, GraphicsDevice device, Effect effect,
			ParticleMode mode, float width, float height, IEnumerable<ParticleManagerInitializer> particle, Random rnd)
			: this(script, device, effect, mode, GetDefault2DProjection(width, height), new Vector2(width, height), particle, rnd)
		{
		}

		/// <summary>
		/// ディレクトリ内のものをすべて読む
		/// </summary>
		/// <param name="dir">ScriptManagerのRootからの相対パス</param>
		public void LoadDirectoryScript(string dir)
		{
			Script.LoadFromDirectory(dir, typeof(ScriptEffect), textureTable);
		}

		public void LoadScript(string fileName)
		{
			Script.Load(fileName, typeof(ScriptEffect), textureTable);
		}

		/// <summary>
		/// パーティクル発生スクリプトを作成する
		/// </summary>
		/// <param name="name"></param>
		/// <param name="pos"></param>
		public void Set(string name, Vector2 pos)
		{
			//for (int i = 0; i < effects.Length; i++)
			//{
			//    if (!effects[i].Flag)
			//    {
			//        effects[i].Set(name, pos);
			//        break;
			//    }
			//}
			Set(name, new Vector3(pos, 0));
		}

		public void Set(string name, Vector3 pos)
		{
			effects.GetFirstUnusedItem().Set(this, Random, name, pos, null);
		}

		public void Set(string name, Vector3 pos, float arg0, float arg1, float arg2)
		{
			effects.GetFirstUnusedItem().Set(this, Random, name, pos, new[] { arg0, arg1, arg2 });
		}

		public void Set(string name, Vector2 pos, float arg0, float arg1, float arg2)
		{
			Set(name, new Vector3(pos, 0), arg0, arg1, arg2);
		}

		/// <summary>
		/// 指定した名前のParticleEngineの更新、描画処理を行うか設定する
		/// </summary>
		/// <param name="name"></param>
		/// <param name="flag"></param>
		public void SetParticleEnable(string name, bool flag)
		{
			var pt = particles[Array.IndexOf(textureTable, name)];
			if (pt != null)
			{
				pt.Enable = flag;
			}
		}

		/// <summary>
		/// すべてのParticleEngineの更新、描画処理を行うかを一括で設定する
		/// </summary>
		/// <param name="flag"></param>
		public void SetAllParticleEnable(bool flag)
		{
			for (int i = 0; i < particles.Length; i++)
			{
				particles[i].Enable = flag;
			}
		}

		public void Update()
		{
			count++;
			//for (int i = 0; i < effects.Length; i++)
			//{
			//    if (effects[i].Flag)
			//    {
			//        effects[i].Update();
			//    }
			//}
			//effects.ForAllWithRemove(s =>
			//	{
			//		s.Update();
			//		return s.Flag;
			//	});
			foreach (var item in effects.ActiveItems())
			{
				item.Update();
			}

			for (int i = 0; i < particles.Length; i++)
			{
				if (particles[i] != null)
				{
					particles[i].Update();
				}
			}
		}

		protected virtual void SetBlendState(ParticleBlendMode mode)
		{
			Device.BlendState = ParticleBlendState.State(mode);
		}

		public void Draw(Matrix view)
		{
			SetDrawStates(view);
			foreach (var item in particles.Where(p => p != null && p.Enable).GroupBy(p => p.BlendMode).OrderBy(x=>x.Key))
			{
				SetBlendState(item.Key);
				foreach (var pt in item)
				{
					pt.Draw();
				}
			}
		}

		void SetDrawStates(Matrix view)
		{
			Device.SamplerStates[0] = SamplerState.LinearWrap;
			Device.RasterizerState = RasterizerState.CullNone;
			Device.Viewport = viewport;
			if (mode == ParticleMode.TwoD)
			{
				Device.DepthStencilState = DepthStencilState.None;
			}
			else
			{
				Device.DepthStencilState = DepthStencilState.DepthRead;
				effectParams[(int)EffectParam.Projection].SetValue(Projection);
				effectParams[(int)EffectParam.ViewProjection].SetValue(view * Projection);
			}
			effectParams[(int)EffectParam.Offset].SetValue(Offset);
			effectParams[(int)EffectParam.Time].SetValue(count);
			effectParams[(int)EffectParam.TargetSize].SetValue(FieldSize);
			
		}


		/// <summary>
		/// パーティクルを描画する。GraphicDeviceのBlendStateとDepthStencilStateを変更する副作用。
		/// </summary>
		public void Draw()
		{
			Draw(Matrix.Identity);//2DならViewは不要
		}

		/// <summary>
		/// レイヤー番号が一致するものだけ描画。2D専用
		/// </summary>
		/// <param name="layer"></param>
		public void DrawLayer(int layer)
		{
			SetDrawStates(Matrix.Identity);
			foreach (var item in particles.Where(p=> p!= null && p.Enable && p.Layer == layer).GroupBy(p=>p.BlendMode).OrderByDescending(x=>(int)x.Key))
			{
				SetBlendState(item.Key);
				foreach (var pt in item)
				{
					pt.Draw();
				}
			}
		}

		/// <summary>
		/// 現在処理中のエフェクトを全て破棄し、パーティクルも全て非表示にする
		/// </summary>
		public void ClearParticle()
		{
			//for (int i = 0; i < effects.Length; i++)
			//{
			//    effects[i].Flag = false;
			//}
			effects.DeleteAll();
			//effects.Clear();
			for (int i = 0; i < particles.Length; i++)
			{
				particles[i].Clear();
			}
		}

		/// <summary>
		/// 粒を生成
		/// </summary>
		/// <param name="name"></param>
		/// <param name="param"></param>
		internal void MakeParticle(int name, ParticleParameter param)
		{
			if (particles[name] != null)
			{
				particles[name].Make(param);
			}
		}


	}
}
