using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Masa.Lib.XNA.Input
{
	public class MultiInputManager : InputManager
	{
		/// <summary>
		/// 0番がキーボードのもの、それ以外は(index - 1)番のパッドのもの
		/// </summary>
		public ControlState[] ControlStates
		{
			get;
			private set;
		}

		public MultiInputManager(Game game)
			: base(game)
		{

		}

		public void SetPadConfigs(PadConfig[] configs)
		{
			if (GamePads == null)
			{
				return;
			}
			for (int i = 0; i < configs.Length && i < GamePads.Length; i++)
			{
				if (configs[i] != null && GamePads[i] != null)
				{
					GamePads[i].Config = configs[i];
				}
			}
		}

		/// <summary>
		/// 呼び出すとControlStateと各パッドのコンフィグが初期化される
		/// </summary>
		/// <param name="device"></param>
		public override void InitDevices(ActiveDevice device)
		{
			base.InitDevices(device);
			ControlStates = new[] { new ControlState() }.Concat(GamePads.Select(x => x != null ? new ControlState() : null)).ToArray();
		}

		public override void Update()
		{
			base.Update();
			ControlStates[0].UpdateFromValue(KeyBoard.CurrentValue);
			for (int i = 0; i < ControlStates.Length - 1 && i < GamePads.Length; i++)
			{
				ControlStates[i + 1].UpdateFromValue(GamePads[i].CurrentValue);
			}
		}


	}
}
