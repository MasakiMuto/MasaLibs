using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;
using SlimDX.DirectInput;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using XInput = Microsoft.Xna.Framework.Input;

namespace Masa.Lib.XNA.Input
{
	/// <summary>
	/// デバイスに直接対応するもの。DirectInputを使う。
	/// </summary>
	public class GamePadDevice : IDisposable
	{
		Joystick Stick { get; set; }

		static DirectInput directInput;

		static DirectInput DirectInput
		{
			get
			{
				if (directInput == null)
				{
					directInput = new DirectInput();
				}
				return directInput;
			}
		}

		static IList<DeviceInstance> GetDevices()
		{
			return DirectInput.GetDevices(DeviceClass.GameController, DeviceEnumerationFlags.AttachedOnly);
		}

		/// <summary>
		/// 接続されているパッドの数を取得する
		/// </summary>
		/// <returns></returns>
		public static int GetPadNumber()
		{
			return GetDevices().Count;
		}

		/// <summary>
		/// 複数のゲームパッドのボタン入力の和を取る
		/// </summary>
		/// <param name="pads"></param>
		/// <returns>各ボタンが押されている/いないの配列。状態をとれるパッドが存在しない場合はnull</returns>
		public static bool[] ZipPadsStates(GamePadDevice[] pads)
		{
			var states = pads.Select(p => p.GetState()).Where(s => s != null);
			if (states.Any())
			{
				bool[] bt = states.Select(s => s.GetButtons())
					.Aggregate((a, b) => a.Zip(b, (x, y) => x || y).ToArray()).ToArray();//複数パッドを合成
				return bt;
			}
			else return null;
		}

		/// <summary>
		/// 番号指定でデバイス生成。番号のデバイスが存在しなければ例外を投げる。
		/// </summary>
		/// <param name="window"></param>
		/// <param name="padNumber">0始まり</param>
		public GamePadDevice(IntPtr window, int padNumber)
		{

			var devices = GetDevices();
			if (devices.Count <= padNumber)
			{
				throw new Exception("指定された数のパッドがつながれていない");
			}
			try
			{
				Stick = new Joystick(DirectInput, devices[padNumber].InstanceGuid);
				Stick.SetCooperativeLevel(window, CooperativeLevel.Exclusive | CooperativeLevel.Foreground);
			}
			catch (DirectInputException e)
			{
				throw new Exception("パッドの初期化に失敗", e);
			}

			///スティックの範囲設定
			foreach (var item in Stick.GetObjects())
			{
				if ((item.ObjectType & ObjectDeviceType.Axis) != 0)
				{
					Stick.GetObjectPropertiesById((int)item.ObjectType).SetRange(-1000, 1000);
				}

			}
			Stick.Acquire();
		}

		/// <summary>
		/// ゲームパッドのデバイスを生成。1番目のパッドを使用。
		/// </summary>
		/// <param name="window"></param>
		public GamePadDevice(IntPtr window)
			: this(window, 0)
		{

		}

		public JoystickState GetState()
		{
			if (Stick == null)
			{
				return null;
			}
			if (Stick.Acquire().IsFailure)
			{
				return null;
			}
			if (Stick.Poll().IsFailure)
			{
				throw new Exception();
			}

			return Stick.GetCurrentState();
		}

		public void Dispose()
		{
			if (Stick != null)
			{
				Stick.Unacquire();
				Stick.Dispose();
			
			}
			GC.SuppressFinalize(this);
		}
	}

	/// <summary>
	/// DirectInputデバイスをラップしたもの
	/// </summary>
	public class GamePad : IInputDevice
	{
		GamePadDevice Device;

		public PadConfig Config { get; set; }
		int border;
		public int StickBorder
		{
			get
			{
				return border;
			}
			set
			{
				if (value < 0) value = 0;
				border = value;
			}
		}

		public JoystickState State { get; private set; }

		public GamePad(Game game, int lever, int padNumber)
		{
			Config = PadConfig.GetDefault();
			StickBorder = lever;
			try
			{
				Device = new GamePadDevice(game.Window.Handle, padNumber);
			}
			catch
			{
				throw new NoDeviceException(padNumber + "番パッドの生成に失敗");
			}
		}

		public GamePad(Game game, int lever)
			: this(game, lever, 0)
		{

		}

		~GamePad()
		{
			if (Device != null)
			{
				Device.Dispose();
			}
		}

		public short Update()
		{
			if (Device == null)
			{
				return 0;
			}
			State = null;
			try
			{
				State = Device.GetState();
			}
			catch (Exception)
			{
				Device = null;
				return 0;
			}
			if (State == null)
			{
				return 0;
			}

			return ProcessState(State);
		}

		short ProcessState(JoystickState state)
		{
			var bt = Config.IntButtonArray;
			short ret = 0;
			int k = 0;
			for (k = 0; k < bt.Length; k++)
			{
				if (state.IsPressed(bt[k]))
				{
					ret += (short)(1 << k);
				}
			}
			bool[] dir = ProcessDirection(state);
			for (int i = 0; i < dir.Length; i++)
			{
				if (dir[i])
				{
					ret += (short)(1 << k);
				}
				k++;
			}
			return ret;
		}

		bool[] ProcessDirection(JoystickState state)
		{
			var dir = new bool[4];//0down 1up 2left 3right 
			int x = state.X;
			int y = state.Y;
			int pov = state.GetPointOfViewControllers()[0];
			if (Math.Abs(y) >= StickBorder)
			{
				if (y < 0) dir[0] = true;
				if (y > 0) dir[1] = true;
			}
			if (Math.Abs(x) >= StickBorder)
			{
				if (x < 0) dir[2] = true;
				if (x > 0) dir[3] = true;
			}
			if (pov != -1)
			{
				//0 up, 9000 right, 18000 down, 27000 left
				double angle = (-pov + 9000) * Math.PI * 2d / 36000d;
				double px = Math.Cos(angle);
				double py = Math.Sin(angle);
				const double Border = .3d;
				if (px > Border) dir[3] = true;
				if (px < -Border) dir[2] = true;
				if (py > Border) dir[0] = true;
				if (py < -Border) dir[1] = true;

			}

			return dir;
		}
	}
}
