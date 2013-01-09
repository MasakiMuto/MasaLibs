using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Masa.Lib
{

	public class LinkedPoolObjectBaseManager<T> where T : PoolObjectBase, new()
	{
		LinkedList<T> items;
		LinkedListNode<T> headItem;		

		public LinkedPoolObjectBaseManager()
		{
			items = new LinkedList<T>();
			Init();
		}

		void Init()
		{
			items.AddFirst(new T() { ListIndex = items.Count });
			headItem = items.First;
		
		}

		public T GetFirstUnusedItem()
		{
			var last = headItem;
			do
			{
				if (!headItem.Value.IsUsed)
				{
					var val = headItem.Value;
					Next(last);
					return val;
				}
			}
			while (!Next(last));
			items.AddAfter(headItem, new T() { ListIndex = items.Count });
			headItem = headItem.Next;
			return headItem.Value;
		}

		/// <summary>
		/// 一周したらtrue
		/// </summary>
		/// <param name="last"></param>
		/// <returns></returns>
		bool Next(LinkedListNode<T> last)
		{
			headItem = headItem.Next;
			if (headItem == null)
			{
				headItem = items.First;
			}
			return last == headItem;
		}

		public IEnumerable<T> ActiveItems()
		{
			return items.Where(i => i.Flag);
		}

		public void DeleteAll()
		{
			items.Clear();
			Init();
		}
	}
}
