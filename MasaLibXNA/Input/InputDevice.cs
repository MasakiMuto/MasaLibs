using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace Masa.Lib.XNA.Input
{
	interface IInputDevice
	{
		void Update();
		short CurrentValue { get; }
		string GetDeviceName();
	}

}
