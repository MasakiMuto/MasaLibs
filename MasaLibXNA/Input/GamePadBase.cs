using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Masa.Lib.XNA.Input
{
	public abstract class GamePadBase : IInputDevice, IDisposable
	{
		public PadConfig Config { get; set; }


		public short CurrentValue
		{
			get;
			protected set;
		}

		public abstract void Update();

		public abstract IEnumerable<int> GetPushedButton();

		public abstract void Dispose();
	}
}
