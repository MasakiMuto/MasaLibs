using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Masa.Lib
{
	public class Maybe<T, S>
		where T : class
		where S : class
	{
		public readonly T Value1;
		public readonly S Value2;

		public Maybe(T value)
		{
			if (value == null)
			{
				throw new ArgumentNullException();
			}
			Value1 = value;
		}

		public Maybe(S value)
		{
			if (value == null)
			{
				throw new ArgumentNullException();
			}
			Value2 = value;
		}

		public bool IsT()
		{
			return Value1 != null;
		}

		public bool IsS()
		{
			return Value2 != null;
		}

		public override string ToString()
		{
			if (IsT())
			{
				return String.Format("{0} is {1}, {2}", base.ToString(), typeof(T).Name, Value1.ToString());
			}
			else if (IsS())
			{
				return String.Format("{0} is {1}, {2}", base.ToString(), typeof(S).Name, Value2.ToString());
			}
			else
			{
				throw new Exception();
			}
		}
	}
}
