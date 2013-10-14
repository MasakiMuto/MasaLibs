using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Masa.Lib.XNA.Input
{
	public enum ButtonTag
	{
		A, B, X, Y,
		Start, Esc, Debug,
		Up, Down, Left, Right,
		MouseLeft, MouseRight, MouseMiddle,
	}

	/// <summary>
	/// コントローラー・キーボードからの入力情報集合体
	/// </summary>
	public class ControlState
	{
		public Button A, B, X, Y, Start, Esc, Debug;
		public Button Up, Down, Left, Right;

		/// <summary>
		/// ボタンを列挙(ABXY START ESC DEBUG)
		/// </summary>
		public IEnumerable<Button> Buttons
		{
			get
			{
				yield return A;///Z
				yield return B;///X
				yield return X;///C
				yield return Y;///Shift
				yield return Start;///Enter
				yield return Esc;
				yield return Debug;///F1
			}
		}

		/// <summary>
		/// 方向を列挙(↑↓←→)
		/// </summary>
		public IEnumerable<Button> Directions
		{
			get
			{
				yield return Up;
				yield return Down;
				yield return Left;
				yield return Right;
			}
		}

		/// <summary>
		/// 全入力を列挙
		/// </summary>
		public IEnumerable<Button> Inputs
		{
			get
			{
				yield return A;///Z
				yield return B;///X
				yield return X;///C
				yield return Y;///Shift
				yield return Start;///Enter
				yield return Esc;
				yield return Debug;///F1
				yield return Up;
				yield return Down;
				yield return Left;
				yield return Right;
			}
		}

		static readonly float OverRoot2 = (float)Math.Sqrt(2) / 2f;
		/// <summary>
		/// 正規化された入力ベクトル(入力がなければゼロベクトル)
		/// </summary>
		/// <returns></returns>
		public Vector2 DirectionVector()
		{
			int ud = 0;
			int lr = 0;
			if (Up.Push) ud--;
			if (Down.Push) ud++;
			if (Left.Push) lr--;
			if (Right.Push) lr++;
			if (ud == 0 && lr == 0)
			{
				return Vector2.Zero;
			}
			if (ud * lr == 0)
			{
				return new Vector2(lr, ud);
			}
			return new Vector2(lr * OverRoot2, ud * OverRoot2);
		}

		public ControlState()
		{
			A = new Button(ButtonTag.A);
			B = new Button(ButtonTag.B);
			X = new Button(ButtonTag.X);
			Y = new Button(ButtonTag.Y);
			Start = new Button(ButtonTag.Start);
			Esc = new Button(ButtonTag.Esc);
			Debug = new Button(ButtonTag.Debug);
			Up = new Button(ButtonTag.Up);
			Down = new Button(ButtonTag.Down);
			Left = new Button(ButtonTag.Left);
			Right = new Button(ButtonTag.Right);
		}

		internal void UpdateFromValue(short value)
		{
			int i = 0;
			foreach (var item in Inputs)
			{
				item.Input((value & (short)(1 << i)) != 0);
				i++;
			}
		}
	}
}
