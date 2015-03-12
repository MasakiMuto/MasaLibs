using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptRuntime.Util
{
	public static class Tuple
	{
		public static Tuple<TItem1, TItem2, TItem3> Create<TItem1, TItem2, TItem3>(TItem1 item1, TItem2 item2, TItem3 item3)
		{
			return new Tuple<TItem1, TItem2, TItem3>(item1, item2, item3);
		}
	}

	public class Tuple<TItem1, TItem2>
	{
		public readonly TItem1 Item1;
		public readonly TItem2 Item2;

		public Tuple(TItem1 item1, TItem2 item2)
		{
			Item1 = item1;
			Item2 = item2;
		}
	}


	public class Tuple<TItem1, TItem2, TItem3>
	{
		public readonly TItem1 Item1;
		public readonly TItem2 Item2;
		public readonly TItem3 Item3;

		public Tuple(TItem1 item1, TItem2 item2, TItem3 item3)
		{
			Item1 = item1;
			Item2 = item2;
			Item3 = item3;
		}
	}
	
}
