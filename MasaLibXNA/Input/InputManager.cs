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
		Start,
		Esc,
		Debug,
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
			Left = new Button();
			Right = new Button();
			Middle = new Button();
		}

		public Vector2 Delta
		{
			get { return new Vector2(DX, DY); }
		}
	}

	/// <summary>
	/// コントローラー・キーボードからの入力情報集合体
	/// </summary>
	public class ControlState
	{
		public Button A, B, X, Y, Start, Esc, Debug;
		public Button Up, Down, Left, Right;

		/// <summary>
		/// ボタンを列挙(ABXY START ESC DEBUG)
		/// </summary>
		public IEnumerable<Button> Buttons
		{
			get
			{
				yield return A;///Z
				yield return B;///X
				yield return X;///C
				yield return Y;///Shift
				yield return Start;///Enter
				yield return Esc;
				yield return Debug;///F1
			}
		}

		/// <summary>
		/// 方向を列挙(↑↓←→)
		/// </summary>
		public IEnumerable<Button> Directions
		{
			get
			{
				yield return Up;
				yield return Down;
				yield return Left;
				yield return Right;
			}
		}

		/// <summary>
		/// 全入力を列挙
		/// </summary>
		public IEnumerable<Button> Inputs
		{
			get
			{
				yield return A;///Z
				yield return B;///X
				yield return X;///C
				yield return Y;///Shift
				yield return Start;///Enter
				yield return Esc;
				yield return Debug;///F1
				yield return Up;
				yield return Down;
				yield return Left;
				yield return Right;
			}
		}

		static readonly float OverRoot2 = (float)Math.Sqrt(2) / 2f;
		/// <summary>
		/// 正規化された入力ベクトル(入力がなければゼロベクトル)
		/// </summary>
		/// <returns></returns>
		public Vector2 DirectionVector()
		{
			int ud = 0;
			int lr = 0;
			if (Up.Push) ud--;
			if (Down.Push) ud++;
			if (Left.Push) lr--;
			if (Right.Push) lr++;
			if (ud == 0 && lr == 0)
			{
				return Vector2.Zero;
			}
			if (ud * lr == 0)
			{
				return new Vector2(lr, ud);
			}
			return new Vector2(lr * OverRoot2, ud * OverRoot2);
		}

		public ControlState()
		{
			A = new Button();
			B = new Button();
			X = new Button();
			Y = new Button();
			Start = new Button();
			Esc = new Button();
			Debug = new Button();
			Up = new Button();
			Down = new Button();
			Left = new Button();
			Right = new Button();
		}
	}


	/// <summary>
	/// コントローラー・キーボードからの入力キーひとつの状態
	/// </summary>
	public class Button
	{
		int state;

		public bool JustPush { get { return (state == 1); } }
		public bool Push { get { return state > 0; } }
		public bool JustRelease { get { return state == -1; } }
		public bool Release { get { return state <= 0; } }
		public int PushTime { get { return state; } }

		public Button()
		{
		}

		/// <summary>
		/// 押されているか否かのデータを入力して、情報を更新する
		/// </summary>
		/// <param name="p"></param>
		public void Input(bool p)
		{
			if (p)
			{
				state++;
			}
			else
			{
				if (state > 0) state = -1;
				else state = 0;
			}
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

	public class InputManager
	{
		public MouseState MouseState
		{
			get;
			private set;
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
				if (keyBoard != null)
				{
					return keyBoard.Config;
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
				if (gamePads != null)
				{
					return gamePads[0].Config;
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

		KeyBoard keyBoard;

		GamePad[] gamePads;
		readonly Game Game;
		readonly int LeverDead;
		PadConfig padConfig;
		KeyboardConfig keyConfig;

		int[] lastPushedButton = {};
		Keys[] lastPushedKey = {};

		public IEnumerable<GamePad> GamePads()
		{
			if (gamePads != null)
			{
				for (int i = 0; i < gamePads.Length; i++)
				{
					yield return gamePads[i];
				}
			}
		}

		public InputManager(Game game, ActiveDevice device = ActiveDevice.Keyboard | ActiveDevice.Pad, int lever = 500)
		{
			Game = game;
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
			GetPushedKey();//初期化
		}

		void ApplyConfig()
		{
			if (keyBoard != null)
			{
				keyBoard.Config = keyConfig;
			}
			if (gamePads != null)
			{
				foreach (var item in gamePads)
				{
					item.Config = padConfig;
				}
			}
		}

		public void InitDevices(ActiveDevice device)
		{
			Device = device;
			if ((Device & ActiveDevice.Keyboard) != 0)
			{
				keyBoard = new KeyBoard();
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
					gamePads = Enumerable.Range(0, padNum).Select(i => new GamePad(Game, LeverDead, i)).ToArray();
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

		public static int GetPadNum()
		{
			return GamePadDevice.GetPadNumber();
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
		public void Update()
		{
			if ((Device & ActiveDevice.Mouse) != 0)
			{
				UpdateMouse();
			}
			inputValue = 0;
			if ((Device & ActiveDevice.Keyboard) != 0)
			{
				inputValue |= keyBoard.Update();
			}
			if ((Device & ActiveDevice.Pad) != 0)
			{
				inputValue |= gamePads.Aggregate((short)0, (short val, GamePad pad) => (short)(val | pad.Update()));
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
		/// マウス位置を指定された座標内に収める
		/// </summary>
		/// <param name="right"></param>
		/// <param name="bottom"></param>
		public void ClampMousePosition(int right, int bottom)
		{
			ClampMousePosition(0, right, 0, bottom);
		}

		/// <summary>
		/// マウス位置を強制的に設定する
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
		/// SHORT値からゲームキー入力を再現する(リプレイ再生)
		/// </summary>
		/// <param name="value"></param>
		public void UpdateGamePlayFromValue(short value)
		{
			//HACK キーボード入力全体の保存には未対応ゆえ、KeyStateを使う場合はバグる。
			inputValue = value;
			int i = 0;
			foreach (var item in GamePlayControlState.Inputs)
			{
				item.Input((InputValue & (short)(1 << i)) != 0);
				i++;
			}
		}

		/// <summary>
		/// 入力装置からゲームキー入力を設定する(通常プレイ、リプレイ記録)
		/// </summary>
		public void UpdateGamePlayFromControl()
		{
			int i = 0;
			foreach (var item in GamePlayControlState.Inputs)
			{
				item.Input((InputValue & (short)(1 << i)) != 0);
				i++;
			}
		}

		/// <summary>
		/// システム用入力の設定。通常はUpdateと同時に呼ばれるので明示的に呼ぶ必要なし
		/// </summary>
		public void UpdateControlState()
		{
			int i = 0;
			foreach (var item in ControlState.Inputs)
			{
				item.Input((InputValue & (short)(1 << i)) != 0);
				i++;
			}
		}

		/// <summary>
		/// 全てのボタンから押されたボタンを取得する
		/// </summary>
		/// <returns>-1なら何も押されていない、それ以外ならPadConfig.IntToButtonに入れられる値</returns>
		public int GetPushedButton()
		{
			var pushed = GamePads()
				.Where(p=>p.State != null)
				.Select(p =>
					p.State.GetButtons()
					.Select((b, i) => b ? i : -1)
					.Where(i => i != -1)
					
				)
				.SelectMany(a => a).ToArray();
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
		/// 全てのキーから押されたキーを取得する
		/// </summary>
		/// <returns>-1なら何も押されてない、それ以外ならMicrosoft.Xna.Framework.Input.Keysにキャスト可能</returns>
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