using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Masa.Lib.XNA.Input
{
	/// <summary>
	/// コントローラー・キーボードからの入力キーひとつの状態
	/// </summary>
	public class Button
	{
		int state;
		public readonly ButtonTag Tag;

		public bool JustPush { get { return (state == 1); } }
		public bool Push { get { return state > 0; } }
		public bool JustRelease { get { return state == -1; } }
		public bool Release { get { return state <= 0; } }
		public int PushTime { get { return state; } }

		public Button(ButtonTag tag)
		{
			Tag = tag;
		}

		/// <summary>
		/// 押されているか否かのデータを入力して、情報を更新する
		/// </summary>
		/// <param name="p"></param>
		public void Input(bool p)
		{
			if (p)
			{
				state++;
			}
			else
			{
				if (state > 0) state = -1;
				else state = 0;
			}
		}
	}
}
