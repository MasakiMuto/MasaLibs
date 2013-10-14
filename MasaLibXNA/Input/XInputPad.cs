using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using XPad = Microsoft.Xna.Framework.Input.GamePad;

namespace Masa.Lib.XNA.Input
{
	public class XInputPad : IInputDevice
	{
		PlayerIndex padNumber;
		public PadConfig Config { get; set; }
		

		public XInputPad(PlayerIndex number)
		{
			padNumber = number;
			if (!XPad.GetState(number).IsConnected)
			{
				throw new NoDeviceException("XInputパッド" + number.ToString() + "は接続されていない");
			}
			Config = PadConfig.GetDefault();
		}

		public void Update()
		{
			GamePadState state = XPad.GetState(padNumber, GamePadDeadZone.IndependentAxes);
			if (!state.IsConnected)
			{
				CurrentValue = 0;
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
			var dir = new bool[4];//0down 1up 2left 3right 
			for (int i = 0; i < dir.Length; i++)
			{
				dir[i] = state.IsButtonDown(PadConfig.DPadArray[i]) || state.IsButtonDown(PadConfig.LeftStickArray[i]);
			}
			return dir;
		}
		public short CurrentValue { get; private set; }
	}
}
