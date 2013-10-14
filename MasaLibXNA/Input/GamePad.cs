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
	/// DirectInputデバイスをラップしたもの
	/// </summary>
	public class GamePad : IInputDevice, IDisposable
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

		public GamePad(Game game, int lever, int padNumber, DirectInput directInput)
		{
			Config = PadConfig.GetDefault();
			StickBorder = lever;
			try
			{
				Device = new GamePadDevice(game.Window.Handle, padNumber, directInput);
			}
			catch
			{
				throw new NoDeviceException(padNumber + "番パッドの生成に失敗");
			}
		}

		public GamePad(Game game, int lever, DirectInput directInput)
			: this(game, lever, 0, directInput)
		{

		}

		~GamePad()
		{
			this.Dispose();
		}

		public void Update()
		{
			if (Device == null)
			{
				CurrentValue = 0;
			}
			State = null;
			try
			{
				State = Device.GetState();
			}
			catch (Exception)
			{
				Device = null;
				CurrentValue = 0;
			}
			if (State == null)
			{
				CurrentValue = 0;
			}

			CurrentValue = ProcessState(State);
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

		public void Dispose()
		{
			if (Device != null)
			{
				Device.Dispose();
				Device = null;
			}
			GC.SuppressFinalize(this);
		}

		public short CurrentValue { get; private set; }
	}
}
