using System;
using System.Collections.Generic;
using SharpDX;


namespace Masa.ParticleEngine
{
	using SharpDX.Toolkit.Graphics;

	public class ParticleEngine : ParticleEngineBase, IDisposable 
	{
		readonly GraphicsDevice Device;
		Effect Drawer;
		int Count;///���݋󂢂Ă���擪index
		int LastCount;
		
		Buffer<ParticleIndexVertex> IndexVertexBuffer;
		Buffer<ParticleVertex> VertexDataBuffer;
		//VertexBuffer VertexDataBuffer;

		Buffer<short> Index;
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

		static ParticleIndexVertex[] indexVertex;

		static ParticleEngine()
		{
			indexVertex = new ParticleIndexVertex[6];
			int[] x = { 0, 0, 1, 0, 1, 1 };
			int[] y = { 0, 1, 1, 0, 1, 0 };

			for (int i = 0; i < indexVertex.Length; i++)
			{
				indexVertex[i] = new ParticleIndexVertex(new Vector2(x[i], y[i]));
				//indexVertex[i].Index = i;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="drawer">�p�[�e�B�N����`�悷�邽�߂̃V�F�[�_�[</param>
		/// <param name="device"></param>
		/// <param name="tex">�p�[�e�B�N���̃e�N�X�`��</param>
		/// <param name="number">�p�[�e�B�N���ő吔</param>
		public ParticleEngine(Effect drawer, GraphicsDevice device, Texture2D tex, ushort number,
			ParticleMode mode, Matrix projection, Vector2 fieldSize)
			:base(tex, number)
		{
			Device = device;
			
			//int[] x = { -1, -1, 1, 1 };
			//int[] y = { 1, -1, -1, 1 };
			
			VertexDataBuffer = Buffer.Vertex.New<ParticleVertex>(device, ParticleNum, SharpDX.Direct3D11.ResourceUsage.Dynamic);

			IndexVertexBuffer = Buffer.Vertex.New<ParticleIndexVertex>(device, indexVertex);

			short[] index = new short[] { 0, 1, 2, 0, 2, 3 };
			Index = Buffer.Index.New<short>(device, index);
			
			
			Drawer = drawer;
			InitEffectParam();
			Mode = mode;
			Projection = projection;
			FieldSize = fieldSize;
			BlendColor = Vector4.One;
			Enable = true;

			SetVertex();//������
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
			VertexDataBuffer.SetData(Vertex);
		}

		/// <summary>
		/// �p�[�e�B�N���Q�𔭐�������
		/// </summary>
		/// <param name="num">�p�[�e�B�N���̐�����</param>
		/// <param name="func">�p�[�e�B�N���̏����ʑ���^����f���Q�[�g�B0����num�����Ɉ����Ƃ��ČĂ΂��</param>
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
		/// �p�[�e�B�N���Q�𔭐�������
		/// </summary>
		/// <param name="param">�p�[�e�B�N���̏����ʑ��̔z��B���̔z��̗v�f�������p�[�e�B�N���������</param>
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
		/// �p�[�e�B�N�����ЂƂ���������
		/// </summary>
		/// <param name="param">�p�[�e�B�N���̏����ʑ�</param>
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
		/// �V�F�[�_��Projectio, View, Time, FieldSize�p�����^��ScriptEffectManager�Őݒ�ς݂̏ꍇ��Draw
		/// </summary>
		public override void Draw()
		{
			if (!Enable)
			{
				return;
			}
			ParamColor.SetValue(BlendColor);
			//Device.SetIndexBuffer(Index, false);
		
			//Device.Textures[0] = Texture;
			if (LastCount != Count)
			{
				SetVertex();
			}

			SetBuffer();
			//Drawer.Parameters["Texture"].SetResource(Texture);
			
			Drawer.CurrentTechnique.Passes[(int)Mode].Apply();
			Device.DrawInstanced(PrimitiveType.TriangleList, 6, VertexDataBuffer.ElementCount);
			//Device.DrawIndexedInstanced(PrimitiveType.TriangleList, 3, VertexDataBuffer.ElementCount);
			
			LastCount = Count;
		}

		void SetBuffer()
		{
			//Device.SetVertexBuffers(bind, IndexVertexBuffer);
			Device.SetVertexBuffer(0, IndexVertexBuffer);
			Device.SetVertexBuffer(1, VertexDataBuffer);
			var layout = VertexInputLayout.New(VertexBufferLayout.New(0, typeof(ParticleIndexVertex)),
				VertexBufferLayout.New(1, typeof(ParticleVertex)));
			Device.SetVertexInputLayout(layout);
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
	/// �������ȒP�ɂ���2D��p�̃p�[�e�B�N���G���W��
	/// </summary>
	public class ParticleEngine2D : ParticleEngine
	{
		Matrix View;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="drawer">�p�[�e�B�N����`�悷�邽�߂̃V�F�[�_�[</param>
		/// <param name="device"></param>
		/// <param name="tex">�p�[�e�B�N���̃e�N�X�`��</param>
		/// <param name="number">�p�[�e�B�N���ő吔(�ō���65535)</param>
		/// <param name="width">�p�[�e�B�N����`�悷��Ώۂ̃����_�����O�^�[�Q�b�g�̕�(�����_�����O�^�[�Q�b�g���g���Ă��Ȃ���΃E�B���h�E�̕�)</param>
		/// <param name="height">�p�[�e�B�N����`�悷��Ώۂ̃����_�����O�^�[�Q�b�g�̍���(�����_�����O�^�[�Q�b�g���g���Ă��Ȃ���΃E�B���h�E�̍���)</param>
		public ParticleEngine2D(Effect drawer, GraphicsDevice device, Texture2D tex, ushort number, int width, int height)
			: base(drawer, device, tex, number, ParticleMode.TwoD, Matrix.OrthoLH(width, height, -10, 100), new Vector2(width, height))
		{
			View = Matrix.LookAtLH(new Vector3(0, 0, 10), Vector3.Zero, Vector3.Up);
			//View = Matrix.CreateLookAt(new Vector3(0, 0, 10), Vector3.Zero, Vector3.Up);
		}

		public override void Draw()
		{
			base.Draw(View);
		}

	}

}
