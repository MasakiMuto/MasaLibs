using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX.DirectInput;

namespace Masa.Lib.XNA.Input
{
	public class Mouse : IDisposable
	{
		public MouseState State { get; private set; }
		SharpDX.DirectInput.Mouse mouse;

		public Mouse(DirectInput input)
		{
			mouse = new SharpDX.DirectInput.Mouse(input);
			State = new MouseState();
			Update();
		}

		public void Update()
		{
			var state = mouse.GetCurrentState();
			State.DX = state.X - State.X;
			State.DY = state.Y - State.Y;
			State.X = state.X;
			State.Y = state.Y;

			State.Left.Input(state.Buttons[0]);
			State.Middle.Input(state.Buttons[1]);
			State.Right.Input(state.Buttons[2]);
		}

		public void Dispose()
		{
			if (mouse != null)
			{
				mouse.Dispose();
				mouse = null;
			}
		}
	}
}
