using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MSInput = Microsoft.Xna.Framework.Input;
using MSButton = Microsoft.Xna.Framework.Input.Buttons;

namespace Masa.Lib.XNA.Input
{
	public enum Buttons
	{
		A,
		B,
		X,
		Y,
		L,
		R,
		Start,
		Esc,
		Debug,
	}

	public enum Axises
	{
		Horizontal1,
		Verticla1,
		Horizontal2,
		Vertical2
	}

	[Serializable]
	public class NoDeviceException : Exception
	{
		public NoDeviceException(string msg)
			: base(msg)
		{

		}
	}



	public class MouseState
	{
		public int X, Y;
		public int DX, DY;
		public Button Left, Right, Middle;

		public IEnumerable<Button> Buttons
		{
			get
			{
				yield return Left;
				yield return Right;
				yield return Middle;
			}
		}

		public MouseState()
		{
			Left = new Button(ButtonTag.MouseLeft);
			Right = new Button(ButtonTag.MouseRight);
			Middle = new Button(ButtonTag.MouseMiddle);
		}

		public Vector2 Delta
		{
			get { return new Vector2(DX, DY); }
		}
	}

	


	

	[Flags]
	public enum ActiveDevice
	{
		No = 0x00,
		Keyboard = 0x01,///�L�[�{�[�h����̃R���g���[���[����
		Mouse = 0x02,
		Pad = 0x04,
		RawKeyBoard = 0x08,///�L�[�{�[�h�S�̂̐�����
	}

	public class InputManager : IDisposable
	{
		public MouseState MouseState
		{
			get;
			private set;
		}

		///// <summary>
		///// �L�[�{�[�h�̐��f�[�^
		///// </summary>
		//public MSInput::KeyboardState KeyState
		//{
		//    get;
		//    private set;
		//}

		///// <summary>
		///// �p�b�h�̐��f�[�^
		///// </summary>
		//public GamePadState PadState
		//{
		//    get;
		//    private set;
		//}



		//����F�L�[�{�[�h����̓��͂��R���g���[���[�ɕϊ����Ă���

		/// <summary>
		/// �W���̓���
		/// </summary>
		public ControlState ControlState
		{
			get;
			private set;
		}

		/// <summary>
		/// �Q�[���v���C�p�ɃV�X�e���L�[�Ȃǂ𖳎��ł���悤�ɂ������́B���v���C�Đ����͂����������������
		/// </summary>
		public ControlState GamePlayControlState
		{
			get;
			private set;
		}

		/// <summary>
		/// ���v���C�p�̓��͒l
		/// </summary>
		public short InputValue
		{
			get
			{
				return inputValue;
			}
		}

		short inputValue;

		public KeyboardConfig KeyConfig
		{
			get
			{
				if (KeyBoard != null)
				{
					return KeyBoard.Config;
				}
				return null;
			}
			set
			{
				keyConfig = value;
				ApplyConfig();
			}
		}

		public PadConfig PadConfig
		{
			get
			{
				if (GamePads != null)
				{
					return GamePads[0].Config;
				}
				else return null;
			}
			set
			{
				padConfig = value;
				ApplyConfig();
			}
		}

		public ActiveDevice Device;

		protected KeyBoard KeyBoard { get; private set; }

		protected GamePadBase[] GamePads { get; private set; }
		readonly Game Game;
		readonly int LeverDead;
		PadConfig padConfig;
		KeyboardConfig keyConfig;

		int[] lastPushedButton = {};
		Keys[] lastPushedKey = {};

		internal SharpDX.DirectInput.DirectInput DirectInput { get; private set; }

		public IEnumerable<GamePadBase> GetGamePads()
		{
			if (GamePads != null)
			{
				//for (int i = 0; i < gamePads.Length; i++)
				//{
				//	yield return gamePads[i];
				//}
				return GamePads;
			}
			return Enumerable.Empty<GamePadBase>();
		}

		public InputManager(Game game, ActiveDevice device = ActiveDevice.Keyboard | ActiveDevice.Pad, int lever = 500)
		{
			Game = game;
			DirectInput = new SharpDX.DirectInput.DirectInput();
			LeverDead = lever;
			MouseState = new MouseState();
			var ms = MSInput::Mouse.GetState();
			MouseState.X = ms.X;
			MouseState.Y = ms.Y;
			ControlState = new ControlState();
			GamePlayControlState = new ControlState();
			keyConfig = KeyboardConfig.GetDefault();
			padConfig = PadConfig.GetDefault();
			InitDevices(device);
			GetPushedKey();//������
		}

		public void Dispose()
		{
			DisposeDevices();
			GC.SuppressFinalize(this);
		}

		void DisposeDevices()
		{
			if (DirectInput != null)
			{
				DirectInput.Dispose();
				DirectInput = null;

			}
			if (GamePads != null)
			{
				for (int i = 0; i < GamePads.Length; i++)
				{
					GamePads[i].Dispose();
					GamePads[i] = null;
				}
			}
			GamePads = null;
			
		}

		~InputManager()
		{
			DisposeDevices();
		}

		void ApplyConfig()
		{
			if (KeyBoard != null)
			{
				KeyBoard.Config = keyConfig;
			}
			if (GamePads != null)
			{
				foreach (var item in GamePads)
				{
					item.Config = padConfig;
				}
			}
		}

		public virtual void InitDevices(ActiveDevice device)
		{
			Device = device;
			if ((Device & ActiveDevice.Keyboard) != 0)
			{
				KeyBoard = new KeyBoard();
			}
			if ((Device & ActiveDevice.Pad) != 0)
			{
				int padNum = GetPadNum();
				if (padNum == 0)
				{
					Device -= ActiveDevice.Pad;
				}
				else
				{
					GamePads = Enumerable.Range(0, padNum)
						.Select<int, GamePadBase>(i => new GamePad(LeverDead, i, DirectInput))
						.Concat(XInputPad.GetAvailableControllers()
							.Select(x=>new XInputPad(x)))
						.ToArray();
				}
				//try
				//{
				//    gamePad = new GamePad(game, lever);
				//}
				//catch (Exception)
				//{
				//    Device -= ActiveDevice.Pad;
				//}
			}
			ApplyConfig();
		}

		/// <summary>
		/// ���ݐڑ�����Ă���Q�[���p�b�h�����擾����(InputManager�ɔF������Ă��Ȃ����̊܂߂�)
		/// </summary>
		/// <returns></returns>
		public int GetPadNum()
		{
			return GamePadDevice.GetPadNumber(DirectInput);
		}

		//public void InitAllPad(Game game, int lever)
		//{
		//    int padNum = GamePadDevice.GetPadNumber();
		//    if (padNum > 0)
		//    {
		//        gamePads = new GamePad[padNum];
		//        for (int i = 0; i < padNum; i++)
		//        {
		//            gamePads[i] = new GamePad(game, lever, i);
		//        }
		//    }
		//}

		/// <summary>
		/// ���͂��擾����InputValue��ControlState��ݒ肷��
		/// GamePlayControlState��UpdateGamePlay�n���Ă΂Ȃ��ƍX�V����Ȃ��̂Œ���
		/// </summary>
		public virtual void Update()
		{
			if ((Device & ActiveDevice.Mouse) != 0)
			{
				UpdateMouse();
			}
			inputValue = 0;
			if ((Device & ActiveDevice.Keyboard) != 0)
			{
				KeyBoard.Update();
				inputValue |= KeyBoard.CurrentValue;
			}
			if ((Device & ActiveDevice.Pad) != 0)
			{
				foreach (var item in GamePads)
				{
					item.Update();
					inputValue |= item.CurrentValue;
				}
				//inputValue |= gamePads.Aggregate((short)0, (short val, GamePad pad) => (short)(val | pad.Update()));
				//inputValue |= gamePad.Update();
			}
			UpdateControlState();
		}

		public void ClampMousePosition(int minX, int maxX, int minY, int maxY)
		{
			var st = MSInput.Mouse.GetState();
			int x = st.X;
			int y = st.Y;
			x = (int)MathHelper.Clamp(x, minX, maxX);
			y = (int)MathHelper.Clamp(y, minY, maxY);
			SetMousePosition(x, y);
		}

		/// <summary>
		/// �}�E�X�ʒu���w�肳�ꂽ���W���Ɏ��߂�
		/// </summary>
		/// <param name="right"></param>
		/// <param name="bottom"></param>
		public void ClampMousePosition(int right, int bottom)
		{
			ClampMousePosition(0, right, 0, bottom);
		}

		/// <summary>
		/// �}�E�X�ʒu�������I�ɐݒ肷��
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public void SetMousePosition(int x, int y)
		{
			MSInput.Mouse.SetPosition(x, y);
			//MouseState.X = x;
			//MouseState.Y = y;
			lastX = x;
			lastY = y;
		}

		int lastX, lastY;

		void UpdateMouse()
		{
			var mouse = MSInput.Mouse.GetState();

			ButtonState[] ms = { mouse.LeftButton, mouse.RightButton, mouse.MiddleButton };
			int i = 0;
			foreach (var item in MouseState.Buttons)
			{
				item.Input(ms[i] == ButtonState.Pressed);
				i++;
			}

			MouseState.X = mouse.X;
			MouseState.Y = mouse.Y;
			MouseState.DX = mouse.X - lastX;
			MouseState.DY = mouse.Y - lastY;
			lastX = MouseState.X;
			lastY = MouseState.Y;
		}


		/// <summary>
		/// SHORT�l����Q�[���L�[���͂��Č�����(���v���C�Đ�)
		/// </summary>
		/// <param name="value"></param>
		public void UpdateGamePlayFromValue(short value)
		{
			//HACK �L�[�{�[�h���͑S�̂̕ۑ��ɂ͖��Ή��䂦�AKeyState���g���ꍇ�̓o�O��B
			inputValue = value;
			GamePlayControlState.UpdateFromValue(InputValue);
		}

		/// <summary>
		/// ���͑��u����Q�[���L�[���͂�ݒ肷��(�ʏ�v���C�A���v���C�L�^)
		/// </summary>
		public void UpdateGamePlayFromControl()
		{
			GamePlayControlState.UpdateFromValue(InputValue);
		}

		/// <summary>
		/// �V�X�e���p���͂̐ݒ�B�ʏ��Update�Ɠ����ɌĂ΂��̂Ŗ����I�ɌĂԕK�v�Ȃ�
		/// </summary>
		public void UpdateControlState()
		{
			ControlState.UpdateFromValue(InputValue);
		}

		/// <summary>
		/// �S�Ẵ{�^�����牟���ꂽ�{�^�����擾����
		/// </summary>
		/// <returns>-1�Ȃ牽��������Ă��Ȃ��A����ȊO�Ȃ�PadConfig.IntToButton�ɓ������l</returns>
		public int GetPushedButton()
		{
			var pushed = GetGamePads()
				.SelectMany(x => x.GetPushedButton()).ToArray();
			var exc = pushed.Except(lastPushedButton).ToArray();
			if (exc.Any())
			{
				lastPushedButton = pushed;
				return exc.First();
			}
			else
			{
				lastPushedButton = pushed;
				return -1;
			}
		}

		static readonly Keys[] DirectionKeys = new[] { Keys.Up, Keys.Down, Keys.Left, Keys.Right };
		/// <summary>
		/// �S�ẴL�[���牟���ꂽ�L�[���擾����
		/// </summary>
		/// <returns>-1�Ȃ牽��������ĂȂ��A����ȊO�Ȃ�Microsoft.Xna.Framework.Input.Keys�ɃL���X�g�\</returns>
		public int GetPushedKey()
		{
			var pushed = Keyboard.GetState().GetPressedKeys()
				.Where(k => !DirectionKeys.Contains(k))
				.ToArray();

			var inter = pushed.Except(lastPushedKey).ToArray();
			lastPushedKey = pushed;
			if (inter.Any())
			{
				return (int)inter.First();
			}
			else return -1;
		}

	}
}