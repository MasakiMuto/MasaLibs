using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Masa.ScriptEngine
{
	public class ScriptRunnerEmulator
	{
		public readonly SimpleEnvironment Environment;

		public float State { get { return Environment.State; } }
		public float StateFrame { get { return Environment.StateFrame; } }
		public float LastStateFrame { get { return Environment.LastStateFrame; } }
		public float Frame { get { return Environment.Frame; } }
		public float LastFrame { get { return Environment.LastFrame; } }

		public ScriptRunnerEmulator()
		{
			Environment = new SimpleEnvironment();
		}

		public void Update()
		{
			Environment.FrameUpdate();
		}

		public bool Loop(int times, float freq, float from)
		{
			return StateFrame >= from && ((times == 0 || StateFrame < from + freq * times) && (StateFrame - from) % from == 0);
		}

		public void Reset()
		{
			Environment.State = 0;
			Environment.StateFrame = 0;
			Environment.LastStateFrame = 0;
			Environment.LastFrame = 0;
			Environment.Frame = 0;
		}
	}
}
