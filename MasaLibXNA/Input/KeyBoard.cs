using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX.DirectInput;
using SharpDX;

namespace Masa.Lib.XNA.Input
{
	public class KeyBoard : IInputDevice, IDisposable
	{
		public KeyboardState KeyState
		{
			get;
			private set;
		}

		public short CurrentValue { get; private set; }

		public static string KeyToString(Key key)
		{
			return Enum.GetName(typeof(Key), key);
		}

		public KeyboardConfig Config;
		Keyboard keyboard;

		public KeyBoard(DirectInput input)
		{
			Config = KeyboardConfig.GetDefault();
			keyboard = new Keyboard(input);
			Update();
		}

		public void Update()
		{
			keyboard.Acquire();
			keyboard.Poll();
			KeyState = keyboard.GetCurrentState();
			CurrentValue = 0;
			Key[] bt = Config.ButtonArray;
			int k = 0;
			for (int i = 0; i < bt.Length; i++)
			{
				if(KeyState.IsPressed(bt[i]))
				{
					CurrentValue += (short)(1 << k);
				}
				k++;
			}
			var arrow = KeyboardConfig.ArrowArray;
			for (int i = 0; i < arrow.Length; i++)
			{
				if(KeyState.IsPressed(arrow[i]))
				{
					CurrentValue += (short)(1 << k);
				}
				k++;
			}
		}

		public IEnumerable<Key> GetPushedKeys()
		{
			return KeyState.PressedKeys;
		}

		public string GetDeviceName()
		{
			return keyboard.Information.InstanceName;
		}

		public void Dispose()
		{
			if (keyboard != null)
			{
				keyboard.Unacquire();
				keyboard.Dispose();
				keyboard = null;
			}
			GC.SuppressFinalize(this);
		}

		~KeyBoard()
		{
			Dispose();
		}
	}
}
