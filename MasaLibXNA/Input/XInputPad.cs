using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using XPad = Microsoft.Xna.Framework.Input.GamePad;
using XButtons = Microsoft.Xna.Framework.Input.Buttons;

namespace Masa.Lib.XNA.Input
{
	public class XInputPad : GamePadBase
	{
		PlayerIndex padNumber;

		public static IEnumerable<PlayerIndex> GetAvailableControllers()
		{
			return Enum.GetValues(typeof(PlayerIndex)).Cast<PlayerIndex>()
				.Where(x => XPad.GetState(x).IsConnected);
		}

		public XInputPad(PlayerIndex number)
		{
			padNumber = number;
			if (!XPad.GetState(number).IsConnected)
			{
				throw new NoDeviceException("XInputパッド" + number.ToString() + "は接続されていない");
			}
			Config = PadConfig.GetDefault();
		}

		public override void Update()
		{
			GamePadState state = XPad.GetState(padNumber, GamePadDeadZone.IndependentAxes);
			if (!state.IsConnected)
			{
				CurrentValue = 0;
				return;
			}
			CurrentValue = 0;
			var bt = Config.ButtonArray;
			int i;
			for (i = 0; i < bt.Length; i++)
			{
				if (state.IsButtonDown(bt[i]))
				{
					CurrentValue += (short)(1 << i);
				}
			}
			bool[] dir = ProcessDirection(state);
			for (int k = 0; k < dir.Length; k++)
			{
				if (dir[k])
				{
					CurrentValue += (short)(1 << i);
				}
				i++;
			}
		}

		bool[] ProcessDirection(GamePadState state)
		{
			var dir = new bool[4];//0up 1down 2left 3right 
			for (int i = 0; i < dir.Length; i++)
			{
				dir[i] = state.IsButtonDown(PadConfig.DPadArray[i]) || state.IsButtonDown(PadConfig.LeftStickArray[i]);
			}
			return dir;
		}

		public override IEnumerable<int> GetPushedButton()
		{
			var state = XPad.GetState(padNumber);
			return Enum.GetValues(typeof(XButtons)).Cast<XButtons>()
				.Where(x => state.IsButtonDown(x))
				.Select(x => PadConfig.ButtonToInt(x));
			//XPad.GetState(padNumber).Buttons.Y
		}

		public override void Dispose()
		{

		}
	}
}
