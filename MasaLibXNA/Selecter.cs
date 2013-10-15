using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Masa.Lib.XNA.Input;

namespace Masa.Lib.XNA
{
	public enum SelectDirection
	{
		Vertical,
		Horizonal,
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
					index = (value < 0) ? 0 : (value >= OptionCount ? OptionCount - 1 : value);
				}
			}
		}

		/// <summary>
		/// indexが端まで行ってもループさせない
		/// </summary>
		public bool SuppressLoop { get; set; }

		/// <summary>
		/// 0なら不動、+1なら正方向、-1なら負方向、+2なら大きく動くほうの正方向、-2なら大きく動くほうの負方向
		/// </summary>
		public int LastMoveDirection { get; protected set; }

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

		/// <summary>
		/// カーソルが動くべきタイミング
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
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
		/// 
		/// </summary>
		/// <param name="input">使用する入力State 普通はキー入力そのままの</param>
		/// <param name="max">要素数</param>
		/// <param name="freq">押しっぱなし自動移動の速度、負ならループしない</param>
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
			LastMoveDirection = 0;
			if (IsMoved(d != 0))
			{
				var last = Index;
				Index += d;
				if (last != Index)
				{
					if (d > 0)
					{
						LastMoveDirection = 1;
					}
					else if(d < 0)
					{
						LastMoveDirection = -1;
					}
					return true;
				}
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
		/// <param name="freq">負ならループしない</param>
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
			LastMoveDirection = 0;
			if (IsMoved(one != 0 || line != 0))
			{
				var last = Index;
				Index += one + line * LineLength;
				if(last != Index)
				{
					if (line > 0)
					{
						LastMoveDirection = 2;
					}
					else if (line < 0)
					{
						LastMoveDirection = -2;
					}
					else if (one > 0)
					{
						LastMoveDirection = 1;
					}
					else if (one < 0)
					{
						LastMoveDirection = -1;
					}
					return true;
				}
			}
			return false;
		}
	}
}
