using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace Masa.Lib.XNA.Input
{
	public class KeyBoard : IInputDevice
	{
		public KeyboardState KeyState
		{
			get;
			private set;
		}

		public short CurrentValue { get; private set; }

		public static string KeyToString(Keys key)
		{
			return Enum.GetName(typeof(Keys), key);
		}

		public KeyboardConfig Config;

		public KeyBoard()
		{
			Config = KeyboardConfig.GetDefault();
		}

		public void Update()
		{
			CurrentValue = 0;
			KeyState = Keyboard.GetState();
			Keys[] bt = Config.ButtonArray;
			int k = 0;
			for (int i = 0; i < bt.Length; i++)
			{
				if (KeyState.IsKeyDown(bt[i]))
				{
					CurrentValue += (short)(1 << k);
				}
				k++;
			}
			var arrow = KeyboardConfig.ArrowArray;
			for (int i = 0; i < arrow.Length; i++)
			{
				if (KeyState.IsKeyDown(arrow[i]))
				{
					CurrentValue += (short)(1 << k);
				}
				k++;
			}
		}

		public IEnumerable<Keys> GetPushedKeys()
		{
			return KeyState.GetPressedKeys();
		}

		public string GetDeviceName()
		{
			return "Keyboard";
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);
		}

		~KeyBoard()
		{
			Dispose();
		}
	}
}
