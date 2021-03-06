using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Masa.ParticleEngine
{
	public class ParticleEngine : ParticleEngineBase, IDisposable 
	{
		readonly GraphicsDevice Device;
		Effect Drawer;
		int Count;///現在空いている先頭index
		int LastCount;
		
		VertexBuffer IndexVertexBuffer;
		DynamicVertexBuffer VertexDataBuffer;
		//VertexBuffer VertexDataBuffer;

		IndexBuffer Index;
		EffectParameter ParamView, ParamProj, ParamTime, ParamField, ParamColor;

		public Matrix Projection
		{
			get;
			set;
		}
		public ParticleMode Mode
		{
			get;
			set;
		}
		public Vector2 FieldSize
		{
			get;
			set;
		}
		VertexBufferBinding bind;

		static ParticleIndexVertex[] indexVertex;

		static ParticleEngine()
		{
			indexVertex = new ParticleIndexVertex[4];
			int[] x = { 0, 0, 1, 1 };
			int[] y = { 0, 1, 1, 0 };

			for (int i = 0; i < indexVertex.Length; i++)
			{
				indexVertex[i] = new ParticleIndexVertex(new Vector2(x[i], y[i]));
				//indexVertex[i].Index = i;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="drawer">パーティクルを描画するためのシェーダー</param>
		/// <param name="device"></param>
		/// <param name="tex">パーティクルのテクスチャ</param>
		/// <param name="number">パーティクル最大数</param>
		public ParticleEngine(Effect drawer, GraphicsDevice device, Texture2D tex, ushort number,
			ParticleMode mode, Matrix projection, Vector2 fieldSize)
			:base(tex, number)
		{
			Device = device;
			
			//int[] x = { -1, -1, 1, 1 };
			//int[] y = { 1, -1, -1, 1 };
			
			VertexDataBuffer = new DynamicVertexBuffer(device, typeof(ParticleVertex), ParticleNum * 1, BufferUsage.WriteOnly);

		
			IndexVertexBuffer = new VertexBuffer(device, typeof(ParticleIndexVertex), indexVertex.Length, BufferUsage.WriteOnly);
			IndexVertexBuffer.SetData(indexVertex);

			short[] index = new short[] { 0, 1, 2, 0, 2, 3 };
			Index = new IndexBuffer(device, IndexElementSize.SixteenBits, index.Length, BufferUsage.WriteOnly);
			Index.SetData(index);
			
			Drawer = drawer;
			InitEffectParam();
			Mode = mode;
			Projection = projection;
			FieldSize = fieldSize;
			BlendColor = Vector4.One;
			Enable = true;

			SetVertex();//初期化
			bind = new VertexBufferBinding(VertexDataBuffer, 0, 1);
		}

		void InitEffectParam()
		{
			ParamView = GetParam("ViewProjection");
			ParamProj = GetParam("Projection");
			ParamTime = GetParam("Time");
			ParamField = GetParam("TargetSize");
			ParamColor = GetParam("Color");
		}

		EffectParameter GetParam(string name)
		{
			return Drawer.Parameters[name];
		}

		public override void Clear()
		{
			base.Clear();
			SetVertex();
		}

		void SetVertex()
		{
			VertexDataBuffer.SetData(Vertex, 0, Vertex.Length, SetDataOptions.Discard);
		}

		/// <summary>
		/// パーティクル群を発生させる
		/// </summary>
		/// <param name="num">パーティクルの生成数</param>
		/// <param name="func">パーティクルの初期位相を与えるデリゲート。0からnumを順に引数として呼ばれる</param>
		public void Make(int num, Func<int, ParticleParameter> func)
		{
			if (!CheckMake())
			{
				return;
			}
			for (int i = 0; i < num; i++)
			{
				Set(Count % ParticleNum, func(i));
				Count++;
			}
		}

		/// <summary>
		/// パーティクル群を発生させる
		/// </summary>
		/// <param name="param">パーティクルの初期位相の配列。この配列の要素数だけパーティクルが作られる</param>
		public void Make(ParticleParameter[] param)
		{
			if (!CheckMake())
			{
				return;
			}
			for (int i = 0; i < param.Length; i++)
			{
				Set(Count % ParticleNum, param[i]);
				Count++;
			}
		}

		/// <summary>
		/// パーティクルをひとつ発生させる
		/// </summary>
		/// <param name="param">パーティクルの初期位相</param>
		public override void Make(ParticleParameter param)
		{
			if (!CheckMake())
			{
				return;
			}
			Set(Count % ParticleNum, param);
			Count++;
		}


		public void Draw(Matrix view)
		{
			if (!Enable)
			{
				return;
			}
			ParamProj.SetValue(Projection);
			ParamView.SetValue(view * Projection);
			ParamTime.SetValue(Time);
			ParamField.SetValue(FieldSize);

			Draw();
		}

		/// <summary>
		/// シェーダのProjectio, View, Time, FieldSizeパラメタをScriptEffectManagerで設定済みの場合のDraw
		/// </summary>
		public override void Draw()
		{
			if (!Enable)
			{
				return;
			}
			ParamColor.SetValue(BlendColor);
			Device.Indices = Index;
			Device.Textures[0] = Texture;
			int buf = VertexDataBuffer.VertexCount;
			if (LastCount != Count)
			{
				#region 2倍バッファー
				//2倍バッファー
				/*if (Count % buf == 0 || Count / buf == LastCount / buf)
				{
					VertexDataBuffer.SetData(LastCount % buf, Vertex, LastCount % PARTICLE_NUM, Count - LastCount, ParticleVertex.SizeInBytes, SetDataOptions.NoOverwrite);
				}
				else
				{
					int use = buf - (LastCount % buf);
					VertexDataBuffer.SetData(LastCount % buf, Vertex, LastCount % PARTICLE_NUM, use, ParticleVertex.SizeInBytes, SetDataOptions.NoOverwrite);
					VertexDataBuffer.SetData(0, Vertex, 0, Count - LastCount - use, ParticleVertex.SizeInBytes, SetDataOptions.NoOverwrite);
				}*/


				/*if (Count % PARTICLE_NUM == 0 || Count / PARTICLE_NUM == LastCount / PARTICLE_NUM)//一周しない場合
				{
					VertexDataBuffer.SetData(ParticleVertex.SizeInBytes * (LastCount % PARTICLE_NUM), Vertex, LastCount % PARTICLE_NUM, Count - LastCount, ParticleVertex.SizeInBytes, SetDataOptions.None);
				}
				else
				{
					int use = PARTICLE_NUM - (LastCount % PARTICLE_NUM);
					VertexDataBuffer.SetData(ParticleVertex.SizeInBytes * (LastCount % PARTICLE_NUM), Vertex, LastCount % PARTICLE_NUM, use, ParticleVertex.SizeInBytes, SetDataOptions.None);
					VertexDataBuffer.SetData(0, Vertex, 0, Count - LastCount - use, ParticleVertex.SizeInBytes, SetDataOptions.None);
				}*/
				#endregion
				SetVertex();
			}

			SetBuffer();

			Drawer.CurrentTechnique.Passes[(int)Mode].Apply();
			Device.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, IndexVertexBuffer.VertexCount, 0, Index.IndexCount / 3, bind.VertexBuffer.VertexCount);
			#region OLD

			/*if (Count % buf == 0)
			{
				var bind = new VertexBufferBinding(VertexDataBuffer, 0 + PARTICLE_NUM, 1);

				Device.SetVertexBuffers(bind, IndexVertexBuffer);
				Drawer.CurrentTechnique.Passes[(int)Mode].Apply();
				Device.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, IndexVertexBuffer.VertexCount, 0, Index.IndexCount / 3, bind.VertexBuffer.VertexCount / 2);
			}
			else if (Count % buf >= PARTICLE_NUM)//またがない
			{

				
				var bind = new VertexBufferBinding(VertexDataBuffer, Count % buf - PARTICLE_NUM, 1);

				Device.SetVertexBuffers(bind, IndexVertexBuffer);
				Drawer.CurrentTechnique.Passes[(int)Mode].Apply();
				Device.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, IndexVertexBuffer.VertexCount, 0, Index.IndexCount / 3, bind.VertexBuffer.VertexCount / 2);
			}
			else
			{
				int n = Count % buf + PARTICLE_NUM;
				var bind = new VertexBufferBinding(VertexDataBuffer, n, 1);
				Device.SetVertexBuffers(bind, IndexVertexBuffer);
				Drawer.CurrentTechnique.Passes[(int)Mode].Apply();
				Device.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, IndexVertexBuffer.VertexCount, 0, Index.IndexCount / 3, bind.VertexBuffer.VertexCount - n);

				bind = new VertexBufferBinding(VertexDataBuffer, 0, 1);
				Device.SetVertexBuffers(bind, IndexVertexBuffer);
				Drawer.CurrentTechnique.Passes[(int)Mode].Apply();
				Device.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, IndexVertexBuffer.VertexCount, 0, Index.IndexCount / 3, PARTICLE_NUM - (bind.VertexBuffer.VertexCount - n));
			}*/
			#endregion
			LastCount = Count;
		}

		void SetBuffer()
		{
			//Device.SetVertexBuffers(bind, IndexVertexBuffer);
			Device.SetVertexBuffers(IndexVertexBuffer, bind);
		}

		

		void SetParameter(string name, Matrix value)
		{
			Drawer.Parameters[name].SetValue(value);
		}

		void SetParameter(string name, float value)
		{
			Drawer.Parameters[name].SetValue(value);
		}

		void SetParameter(string name, Vector2 value)
		{
			Drawer.Parameters[name].SetValue(value);
		}

		void SetParameter(string name, Vector4 value)
		{
			Drawer.Parameters[name].SetValue(value);
		}

		public void Dispose()
		{
			if (VertexDataBuffer != null)
			{
				VertexDataBuffer.Dispose();
				VertexDataBuffer = null;
			}
			if (IndexVertexBuffer != null)
			{
				IndexVertexBuffer.Dispose();
				IndexVertexBuffer = null;
			}
			if (Index != null)
			{
				Index.Dispose();
				Index = null;
			}
			GC.SuppressFinalize(this);
		}

		~ParticleEngine()
		{
			Dispose();
			
		}
	}

	/// <summary>
	/// 扱いを簡単にした2D専用のパーティクルエンジン
	/// </summary>
	public class ParticleEngine2D : ParticleEngine
	{
		Matrix View;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="drawer">パーティクルを描画するためのシェーダー</param>
		/// <param name="device"></param>
		/// <param name="tex">パーティクルのテクスチャ</param>
		/// <param name="number">パーティクル最大数(最高で65535)</param>
		/// <param name="width">パーティクルを描画する対象のレンダリングターゲットの幅(レンダリングターゲットを使っていなければウィンドウの幅)</param>
		/// <param name="height">パーティクルを描画する対象のレンダリングターゲットの高さ(レンダリングターゲットを使っていなければウィンドウの高さ)</param>
		public ParticleEngine2D(Effect drawer, GraphicsDevice device, Texture2D tex, ushort number, int width, int height)
			: base(drawer, device, tex, number, ParticleMode.TwoD, Matrix.CreateOrthographic(width, height, -10, 100), new Vector2(width, height))
		{
			View = Matrix.CreateLookAt(new Vector3(0, 0, 10), Vector3.Zero, Vector3.Up);
		}

		public override void Draw()
		{
			base.Draw(View);
		}

	}

}
