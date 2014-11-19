using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX.XInput;
using SharpDX;
using XButtons = SharpDX.XInput.GamepadButtonFlags;
using Masa.Lib;

namespace Masa.Lib.XNA.Input
{
	public class XInputPad : GamePadBase
	{
		UserIndex padNumber;
		Controller controller;
		public static IEnumerable<UserIndex> GetAvailableControllers()
		{
			return Enum.GetValues(typeof(UserIndex)).Cast<UserIndex>()
				.Where(x => new Controller(x).IsConnected);
			
		}

		public XInputPad(UserIndex number)
		{
			padNumber = number;
			controller = new SharpDX.XInput.Controller(padNumber);
			if (!controller.IsConnected)
			{
				throw new NoDeviceException("XInputパッド" + number.ToString() + "は接続されていない");
			}
			Config = PadConfig.GetDefault();
		}

		public override string GetDeviceName()
		{
			return "XInput Pad " + controller.UserIndex;
		}

		public override void Update()
		{
			if (!controller.IsConnected)
			{
				CurrentValue = 0;
				return;
			}
			var state = controller.GetState();
			
			CurrentValue = 0;
			var bt = Config.ButtonArray;
			int i;
			for (i = 0; i < bt.Length; i++)
			{
				if(state.Gamepad.Buttons.HasFlag(bt[i]))
				//if (state.IsButtonDown(bt[i]))
				{
					CurrentValue += (short)(1 << i);
				}
			}
			bool[] dir = ProcessDirection(state.Gamepad);
			for (int k = 0; k < dir.Length; k++)
			{
				if (dir[k])
				{
					CurrentValue += (short)(1 << i);
				}
				i++;
			}
		}

		bool[] ProcessDirection(SharpDX.XInput.Gamepad state)
		{
			var dir = new bool[4];//0up 1down 2left 3right 
			for (int i = 0; i < dir.Length; i++)
			{
				dir[i] = state.Buttons.HasFlag(PadConfig.DPadArray[i]);// || state.Buttons.HasFlag(PadConfig.LeftStickArray[i]);
				//dir[i] = state.IsButtonDown(PadConfig.DPadArray[i]) || state.IsButtonDown(PadConfig.LeftStickArray[i]);
			}
			return dir;
		}

		public override IEnumerable<int> GetPushedButton()
		{
			var state = controller.GetState().Gamepad;
			return Utility.GetEnumValues<XButtons>()
				.Where(x => state.Buttons.HasFlag(x))
				.Select(x => PadConfig.ButtonToInt(x));
			//XPad.GetState(padNumber).Buttons.Y
		}

		public override void Dispose()
		{

		}
	}
}
