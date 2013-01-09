using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Masa.Lib.Input;

namespace Masa.Lib
{
	public enum SelectDirection
	{
		Vertical,
		Horizonal,
	}

	public enum SelectLoopType
	{
		Enable,
		Disable,
	}

	public abstract class SelecterBase
	{
		internal Input.ControlState Input;
		int index;
		internal int count;
		
		/// <summary>
		/// 選択肢の数
		/// </summary>
		public int OptionCount { get; private set; }

		/// <summary>
		/// 押しっぱなし認識周期 負だと押しっぱなしをスルー
		/// </summary>
		protected readonly int ReloadFreq;

		/// <summary>
		/// カウンタがこれを超えたら動いてループ開始、その後はReloadFreqのたびに動く
		/// </summary>
		protected readonly int BeginReloadCount;

		/// <summary>
		/// 両端をループさせない
		/// </summary>
		public bool SuppressLoop { get; set; }

		public int Index
		{
			get
			{
				return index;
			}
			set
			{
				if (!SuppressLoop)
				{
					index = MathUtil.PositiveMod(value, OptionCount);
				}
				else
				{
					index = value;
					if (index < 0)
					{
						index = 0;
					}
					else if (index >= OptionCount)
					{
						index = OptionCount - 1;
					}
				}
			}
		}

		protected SelecterBase(ControlState input, int max, int freq)
			: this(input, max, freq, freq)
		{

		}

		protected SelecterBase(ControlState input, int max, int freq, int loopBegin)
		{
			Input = input;
			OptionCount = max;
			ReloadFreq = freq;
			BeginReloadCount = loopBegin;
		}

		/// <summary>
		/// 項目が動いたらtrue,そうでなければfalseを返す
		/// </summary>
		/// <returns></returns>
		public abstract bool Update();

		protected bool IsMoved(bool input)
		{
			if (!input)
			{
				count = 0;
				return false;
			}
			count++;
			if (count == 1)//初回押し
			{
				return true;
			}
			if (ReloadFreq > 0 && count >= BeginReloadCount && (count - BeginReloadCount) % ReloadFreq == 0)
			{
				return true;
			}
			return false;
		}
	}

	/// <summary>
	/// 上下リスト・左右リストの選択
	/// </summary>
	public class LinearSelecter : SelecterBase
	{
		SelectDirection Direction;
		/// <summary>
		/// 0なら不動、1なら正、-1なら負
		/// </summary>
		public int LastMoved { get; protected set; }



		/// <summary>
		/// 
		/// </summary>
		/// <param name="input">使用する入力State 普通はキー入力そのままの</param>
		/// <param name="max">要素数</param>
		/// <param name="freq">押しっぱなし自動移動の速度。負なら押しっぱなしで動かない</param>
		/// <param name="dir">縦か横か</param>
		public LinearSelecter(ControlState input, int max, int freq, SelectDirection dir)
			: this(input, max, freq, freq, dir)
		{

		}

		public LinearSelecter(ControlState input, int max, int freq, int loopBegin, SelectDirection dir)
			: base(input, max, freq, loopBegin)
		{
			Direction = dir;
		}



		/// <summary>
		/// 項目が動いたらtrue,そうでなければfalseを返す
		/// </summary>
		/// <returns></returns>
		public override bool Update()
		{
			int d = 0;
			switch (Direction)
			{
				case SelectDirection.Vertical:
					if (Input.Up.Push) d--;
					if (Input.Down.Push) d++;
					break;
				case SelectDirection.Horizonal:
					if (Input.Left.Push) d--;
					if (Input.Right.Push) d++;
					break;
				default:
					break;
			}

			LastMoved = 0;
			if (IsMoved(d != 0))
			{
				if (d > 0 && !(SuppressLoop && Index == OptionCount - 1))
				{
					LastMoved = 1;
				}
				else if(d < 0 && !(SuppressLoop && Index == 0))
				{
					LastMoved = -1;
				}
				Index += d;
				return true;
			}
			return false;
		}

	}

	/// <summary>
	/// 平面リスト(ネームエントリなど)の選択
	/// </summary>
	public class PlaneSelecter : SelecterBase
	{
		SelectDirection Direction;
		int LineLength;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="input"></param>
		/// <param name="max">全要素数</param>
		/// <param name="freq"></param>
		/// <param name="length">一行(列)の長さ</param>
		/// <param name="dir">行方向だと+1か、列方向だとか</param>
		public PlaneSelecter(ControlState input, int max, int freq, int length, SelectDirection dir)
			: this(input, max, freq, freq, length, dir)
		{

		}

		public PlaneSelecter(ControlState input, int max, int freq, int loopBegin, int length, SelectDirection dir)
			: base(input, max, freq, loopBegin)
		{
			Direction = dir;
			LineLength = length;
		}

		public override bool Update()
		{
			int one = 0;
			int line = 0;
			switch (Direction)
			{
				case SelectDirection.Vertical:
					if (Input.Up.Push) one--;
					if (Input.Down.Push) one++;
					if (Input.Left.Push) line--;
					if (Input.Right.Push) line++;
					break;
				case SelectDirection.Horizonal:
					if (Input.Left.Push) one--;
					if (Input.Right.Push) one++;
					if (Input.Up.Push) line--;
					if (Input.Down.Push) line++;
					break;
				default:
					break;
			}
			
			if (IsMoved(one != 0 || line != 0))
			{
				Index += one + line * LineLength;
				return true;
			}
			else return false;
		}
	}
}
