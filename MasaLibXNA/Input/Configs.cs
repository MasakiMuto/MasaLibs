using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using SharpDX.XInput;
using SharpDX.DirectInput;
using System.Xml.Linq;
using MSButton = SharpDX.XInput.GamepadButtonFlags;

namespace Masa.Lib.XNA.Input
{
	public abstract class ConfigBase<T>
	{
		public T[] ButtonArray { get; protected set; }
		public T this[Buttons bt]
		{
			get
			{
				return ButtonArray[(int)bt];
			}
			set 
			{
				ButtonArray[(int)bt] = value; 
			}
		}

		public abstract string XElementName { get; } 

		protected ConfigBase(T a, T b, T x, T y, T l, T r, T st, T es, T db)
		{
			ButtonArray = new[]
			{
				a, b, x, y, l, r, st, es, db
			};
		}

		protected ConfigBase(T[] buttons)
		{
			ButtonArray = buttons;
		}

		protected static Dictionary<Buttons, T> XElementToKeyNumbers(XElement inputElement)
		{
			return Enum.GetValues(typeof(Buttons))
				.Cast<Buttons>()
				.ToDictionary(i => i,
					i => (T)Enum.Parse(typeof(T), inputElement.Element(Enum.GetName(typeof(Buttons), i)).Value)
				);	
		}

		public XElement ToXElement()
		{
			return ToXElement(XElementName);
		}

		public XElement ToXElement(string name)
		{
			var item = new XElement(name);
			item.Add
			(
					Enum.GetValues(typeof(Buttons))
					.Cast<Buttons>()
					.Select(i => new XElement(Enum.GetName(typeof(Buttons), i), ButtonArray[(int)i]))
					.ToArray()
			);
			return item;
		}

		public abstract void ResetDefault();
	}

	public class KeyboardConfig : ConfigBase<Key>
	{
		public static readonly Key[] ArrowArray;

		public override string XElementName
		{
			get { return  "keyboard_config";}
		}

		static KeyboardConfig()
		{
			ArrowArray = new[] { Key.Up, Key.Down, Key.Left, Key.Right };
		}

		public KeyboardConfig(Key a, Key b, Key x, Key y, Key l, Key r, Key st, Key es, Key db)
			: base(a, b, x, y, l, r, st, es, db)
		{

		}

		public KeyboardConfig(int a, int b, int x, int y, int l, int r, int st, int es, int db)
			: this((Key)a, (Key)b, (Key)x, (Key)y, (Key)l, (Key)r, (Key)st, (Key)es, (Key)db)
		{

		}

		public KeyboardConfig(Key[] keys)
			: base(keys)
		{

		}

		public KeyboardConfig Clone()
		{
			return new KeyboardConfig(ButtonArray.Clone() as Key[]);
		}

		static Key[] GetDefaultButtons()
		{
			return new []{Key.Z, Key.X, Key.C, Key.LeftShift, Key.Q, Key.W, Key.Return, Key.Escape, Key.F1};
		}

		public static KeyboardConfig GetDefault()
		{
			return new KeyboardConfig(GetDefaultButtons());
		}

		public override void ResetDefault()
		{
			ButtonArray = GetDefaultButtons();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="inputElement">keyboard_configタグの要素</param>
		/// <returns></returns>
		public static KeyboardConfig FromXElement(XElement inputElement)
		{
			var b = XElementToKeyNumbers(inputElement);			
			//int[] b = EnumerateButtonNames().Select(i=> int.Parse(inputElement.Element(i).Value)).ToArray();

			return new KeyboardConfig(b[Buttons.A], b[Buttons.B], b[Buttons.X], b[Buttons.Y], b[Buttons.L], b[Buttons.R], b[Buttons.Start], b[Buttons.Esc], b[Buttons.Debug]);
		}

	}

	public class PadConfig : ConfigBase<MSButton>
	{
		static readonly Dictionary<int, MSButton> ButtonIntTable;
		static readonly Dictionary<MSButton, int> IntButtonTable;
		public static readonly MSButton[] DPadArray;
		static PadConfig()
		{
			ButtonIntTable = new Dictionary<int, MSButton>()
	        {
	            {0, MSButton.A},
	            {1, MSButton.B},
	            {7, MSButton.Back},
				//{12, MSButton},
	            {4, MSButton.LeftShoulder},
	            {8, MSButton.LeftThumb},
	            {5, MSButton.RightShoulder},
	            {9, MSButton.RightThumb},
	            {6, MSButton.Start},
	            {2, MSButton.X},
	            {3, MSButton.Y},
	        };
			IntButtonTable = ButtonIntTable.ToDictionary(i => i.Value, i => i.Key);
			DPadArray = new[] { MSButton.DPadUp, MSButton.DPadDown, MSButton.DPadLeft, MSButton.DPadRight };
			
		}

		/// <summary>
		/// get only
		/// </summary>
		public int[] IntButtonArray
		{
			get
			{
				return ButtonArray.Select(i => ButtonToInt(i)).ToArray();
			}
		}

		public override string XElementName
		{
			get { return "pad_config"; }
		}


		public PadConfig(MSButton a, MSButton b, MSButton x, MSButton y, MSButton l, MSButton r, MSButton st, MSButton es, MSButton db)
			: base(a, b, x, y, l, r, st, es, db)
		{

		}

		public PadConfig(int a, int b, int x, int y, int l, int r, int st, int es, int db)
			: base(IntToButton(a), IntToButton(b), IntToButton(x), IntToButton(y), IntToButton(l), IntToButton(r),
			IntToButton(st), IntToButton(es), IntToButton(db))
		{
		}

		public PadConfig(MSButton[] buttons)
			: base(buttons)
		{

		}

		public static PadConfig GetDefault()
		{
			return new PadConfig(GetDefaultButtons());
		}

		static MSButton[] GetDefaultButtons()
		{
			return new[] {MSButton.A, MSButton.B, MSButton.X, MSButton.Y, MSButton.LeftShoulder, MSButton.RightShoulder,
				MSButton.Start, MSButton.Back, MSButton.RightThumb };
		}

		public override void ResetDefault()
		{
			ButtonArray = GetDefaultButtons();
		}

		public PadConfig Clone()
		{
			return new PadConfig(ButtonArray.Clone() as MSButton[]);
		}

		/// <summary>
		/// 対応するボタンがなければしいたけボタンを返す
		/// </summary>
		/// <param name="button"></param>
		/// <returns></returns>
		public static MSButton IntToButton(int button)
		{
			if (ButtonIntTable.ContainsKey(button))
			{
				return ButtonIntTable[button];
			}
			else
			{
				return MSButton.None;
			}
		}

		public static int ButtonToInt(MSButton button)
		{
			if (IntButtonTable.ContainsKey(button))
			{
				return IntButtonTable[button];
			}
			else
			{
				return -1;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="inputElement">pad_configタグの要素</param>
		/// <returns></returns>
		public static PadConfig FromXElement(XElement inputElement)
		{
			var b = XElementToKeyNumbers(inputElement);
			//int[] b = EnumerateButtonNames().Select(i=> int.Parse(inputElement.Element(i).Value)).ToArray();

			return new PadConfig(b[Buttons.A], b[Buttons.B], b[Buttons.X], b[Buttons.Y],  b[Buttons.L], b[Buttons.R], b[Buttons.Start], b[Buttons.Esc], b[Buttons.Debug]);
		}
	}
}
