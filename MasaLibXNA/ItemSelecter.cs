using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Masa.Lib.XNA
{
	public struct ItemSelection<T>
	{
		public readonly bool Selected;
		public readonly T Item;
		public ItemSelection(T item, bool select)
		{
			Selected = select;
			Item = item;
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public class ItemSelecter<TKey, TValue> : LinearSelecter
	{
		Tuple<TKey, TValue>[] Items;

		public Tuple<TKey, TValue> SelectedItem
		{
			get
			{
				return Items[Index];
			}
		}

		public IEnumerable<ItemSelection<TKey>> EnumrateKeys()
		{
			return Items.Select((i, j) => new ItemSelection<TKey>(i.Item1, j == Index));
		}

		public IEnumerable<ItemSelection<TValue>> EnumrateValues()
		{
			return Items.Select((i, j) => new ItemSelection<TValue>(i.Item2, j == Index));
		}

		public ItemSelecter(Input.ControlState input, IEnumerable<Tuple<TKey, TValue>> items, int freq, int loopBegin, SelectDirection direction)
			: base(input, items.Count(), freq, loopBegin, direction)
		{
			Items = items.ToArray();
		}


	}
}
