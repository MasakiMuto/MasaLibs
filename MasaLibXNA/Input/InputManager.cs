using System;
using System.Collections.Generic;
using System.Linq;
using SharpDX;
using SharpDX.DirectInput;
using SharpDX.Toolkit;

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
		Keyboard = 0x01,///キーボードからのコントローラー入力
		Mouse = 0x02,
		Pad = 0x04,
		RawKeyBoard = 0x08,///キーボード全体の生入力
	}

	public class InputManager : IDisposable
	{
		public MouseState MouseState
		{
			get
			{
				return Mouse.State;
			}
			
		}

		///// <summary>
		///// キーボードの生データ
		///// </summary>
		//public MSInput::KeyboardState KeyState
		//{
		//    get;
		//    private set;
		//}

		///// <summary>
		///// パッドの生データ
		///// </summary>
		//public GamePadState PadState
		//{
		//    get;
		//    private set;
		//}



		//現状：キーボードからの入力をコントローラーに変換している

		/// <summary>
		/// 標準の入力
		/// </summary>
		public ControlState ControlState
		{
			get;
			private set;
		}

		/// <summary>
		/// ゲームプレイ用にシステムキーなどを無視できるようにした入力。リプレイ再生時はこちらを書き換える
		/// </summary>
		public ControlState GamePlayControlState
		{
			get;
			private set;
		}

		/// <summary>
		/// リプレイ用の入力値
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
		protected Mouse Mouse { get; private set; }

		protected GamePadBase[] GamePads { get; private set; }
		readonly int LeverDead;
		PadConfig padConfig;
		KeyboardConfig keyConfig;

		int[] lastPushedButton = {};
		Key[] lastPushedKey = {};

		internal DirectInput DirectInput { get; private set; }

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

		public InputManager(ActiveDevice device = ActiveDevice.Keyboard | ActiveDevice.Pad, int lever = 5000)
		{
			DirectInput = new DirectInput();
			LeverDead = lever;
			ControlState = new ControlState();
			GamePlayControlState = new ControlState();
			keyConfig = KeyboardConfig.GetDefault();
			padConfig = PadConfig.GetDefault();
			InitDevices(device);
			GetPushedKey();//初期化
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
			if (KeyBoard != null)
			{
				KeyBoard.Dispose();
				KeyBoard = null;
			}
			if (Mouse != null)
			{
				Mouse.Dispose();
				Mouse = null;
			}
			
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
				KeyBoard = new KeyBoard(DirectInput);
			}
			if ((Device & ActiveDevice.Mouse) != 0)
			{
				Mouse = new Mouse(DirectInput);
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
		/// 現在接続されているゲームパッド数を取得する(InputManagerに認識されていないもの含めて)
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
		/// 入力を取得してInputValueとControlStateを設定する
		/// GamePlayControlStateはUpdateGamePlay系を呼ばないと更新されないので注意
		/// </summary>
		public virtual void Update()
		{
			if ((Device & ActiveDevice.Mouse) != 0)
			{
				Mouse.Update();
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


		
		

		/// <summary>
		/// SHORT値からゲームキー入力を再現する(リプレイ再生)
		/// </summary>
		/// <param name="value"></param>
		public void UpdateGamePlayFromValue(short value)
		{
			//HACK キーボード入力全体の保存には未対応ゆえ、KeyStateを使う場合はバグる。
			inputValue = value;
			GamePlayControlState.UpdateFromValue(InputValue);
		}

		/// <summary>
		/// 入力装置からゲームキー入力を設定する(通常プレイ、リプレイ記録)
		/// </summary>
		public void UpdateGamePlayFromControl()
		{
			GamePlayControlState.UpdateFromValue(InputValue);
		}

		/// <summary>
		/// システム用入力の設定。通常はUpdateと同時に呼ばれるので明示的に呼ぶ必要なし
		/// </summary>
		public void UpdateControlState()
		{
			ControlState.UpdateFromValue(InputValue);
		}

		/// <summary>
		/// 全てのボタンから押されたボタンを取得する
		/// </summary>
		/// <returns>-1なら何も押されていない、それ以外ならPadConfig.IntToButtonに入れられる値</returns>
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

		static readonly Key[] DirectionKeys = new[] { Key.Up, Key.Down, Key.Left, Key.Right };
		/// <summary>
		/// 全てのキーから押されたキーを取得する
		/// </summary>
		/// <returns>-1なら何も押されてない、それ以外ならMicrosoft.Xna.Framework.Input.Keysにキャスト可能</returns>
		public int GetPushedKey()
		{
			if (KeyBoard == null)
			{
				return -1;
			}
			var pushed = KeyBoard.GetPushedKeys().ToArray();

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