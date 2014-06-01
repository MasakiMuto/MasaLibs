using System;
using System.Collections.Generic;
using System.Linq;
using SharpDX.DirectInput;

namespace Masa.Lib.XNA.Input
{
	/// <summary>
	/// DirectInputデバイスをラップしたもの
	/// </summary>
	public class GamePad : GamePadBase
	{
		GamePadDevice Device;

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

		JoystickState State { get; set; }

		public GamePad(int lever, int padNumber, DirectInput directInput)
		{
			Config = PadConfig.GetDefault();
			StickBorder = lever;
			try
			{
				Device = new GamePadDevice(padNumber, directInput);
			}
			catch
			{
				throw new NoDeviceException(padNumber + "番パッドの生成に失敗");
			}
		}

		public GamePad(int lever, DirectInput directInput)
			: this(lever, 0, directInput)
		{

		}

		~GamePad()
		{
			this.Dispose();
		}

		public override void Update()
		{
			if (Device == null)
			{
				CurrentValue = 0;
				return;
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
				return;
			}
			if (State == null)
			{
				CurrentValue = 0;
				return;
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
				if (state.Buttons[bt[k]])
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

		const int Axis = 65536 / 2;

		bool[] ProcessDirection(JoystickState state)
		{
			var dir = new bool[4];//0down 1up 2left 3right 
			int x = state.X;
			int y = state.Y;
			int pov = state.PointOfViewControllers[0];
			if (Math.Abs(y - Axis) >= StickBorder)
			{
				if (y < Axis) dir[0] = true;
				if (y > Axis) dir[1] = true;
			}
			if (Math.Abs(x - Axis) >= StickBorder)
			{
				if (x < Axis) dir[2] = true;
				if (x > Axis) dir[3] = true;
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

		public override void Dispose()
		{
			if (Device != null)
			{
				Device.Dispose();
				Device = null;
			}
			GC.SuppressFinalize(this);
		}


		public override IEnumerable<int> GetPushedButton()
		{
			if (State == null)
			{
				return Enumerable.Empty<int>();
			}
			else
			{
				return State.Buttons
					.Select((b, i) => b ? i : -1)
					.Where(x => x != -1);
			}
		}
	}
}
