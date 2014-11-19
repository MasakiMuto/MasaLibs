using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Masa.Lib.XNA.Input
{
	interface IInputDevice
	{
		void Update();
		short CurrentValue { get; }
		string GetDeviceName();
	}

}
