using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Masa.Lib
{

	public abstract class PoolObjectBase
	{
		public bool Flag { get; private set; }

		public virtual bool IsUsed { get { return Flag; } }
		/// <summary>
		/// PoolObjectManagerBaseにより設定される
		/// </summary>
		public int ListIndex { get; internal set; }

		public PoolObjectBase()
		{
		}

		public virtual void Set()
		{
			Flag = true;
		}

		public virtual void Delete()
		{
			Flag = false;
		}
	}

	public abstract class PoolObjectManagerBase<T> where T : PoolObjectBase, new()
	{
		List<T> items;
		int headIndex;
		readonly int DefaultSize;

		/// <summary>
		/// リストの生成、および項目の初期化
		/// </summary>
		/// <param name="defaultSize"></param>
		public PoolObjectManagerBase(int defaultSize)
		{
			DefaultSize = defaultSize;
			items = Enumerable.Range(0, defaultSize).Select(i => new T() {ListIndex = i }).ToList();
		}

		/// <summary>
		/// 先頭の未使用アイテムを返す
		/// </summary>
		/// <returns>使えるものが無ければnull</returns>
		protected T GetFirstUnusedItem()
		{
			int ind = GetFirstUnusedIndex();
			if (ind == -1)
			{
				return null;
			}
			else
			{
				return items[ind];
			}
			//for (int i = 0; i < items.Count; i++)
			//{
			//	if (!items[(i + headIndex) % items.Count].IsUsed)
			//	{
			//		headIndex += i + 1;
			//		return items[(headIndex - 1) % items.Count];
			//	}
			//}
			//headIndex = items.Count;
			//return null;

		}

		/// <summary>
		/// 未使用アイテムのインデックスを返す。
		/// </summary>
		/// <returns>正規化された配列のインデックス、存在しなければ-1</returns>
		protected int GetFirstUnusedIndex()
		{
			for (int i = 0; i < items.Count; i++)
			{
				if (!items[(i + headIndex) % items.Count].IsUsed)
				{
					headIndex += i + 1;
					return (headIndex - 1) % items.Count;
				}
			}
			headIndex = items.Count;
			return -1;
		}

		protected T GetFirstUnusedItemWithExtend()
		{
			var i = GetFirstUnusedItem();
			if (i == null)
			{
				Extend(DefaultSize);
				i = GetFirstUnusedItem();
			}
			return i;
		}

		public abstract T GetFirst();

		/// <summary>
		/// 配列を拡張する
		/// </summary>
		/// <param name="extraSize"></param>
		protected void Extend(int extraSize)
		{
			int count = items.Count;
			items.AddRange(Enumerable.Repeat(0, extraSize).Select(i => new T() { ListIndex = i + count }));
		}

		public IEnumerable<T> ActiveItems()
		{
			return items.Where(i => i.Flag);
		}

		/// <summary>
		/// 生きているもの全てにDeleteを適用する
		/// </summary>
		public void DeleteAll()
		{
			foreach (var item in items)
			{
				item.Delete();
			}
		}

	}
}
