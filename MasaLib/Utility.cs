using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;

namespace Masa.Lib
{
	public static class Utility
	{
		[DllImport("kernel32.dll")]
		extern static ExecutionState SetThreadExecutionState(ExecutionState esFlags);

		[FlagsAttribute]
		enum ExecutionState : uint
		{
			// 関数が失敗した時の戻り値
			Null = 0,
			// スタンバイを抑止
			SystemRequired = 1,
			// 画面OFFを抑止
			DisplayRequired = 2,
			// 効果を永続させる。ほかオプションと併用する。
			Continuous = 0x80000000,
		}

		public static void SupressSleep()
		{
			SetThreadExecutionState(ExecutionState.Continuous | ExecutionState.DisplayRequired);
		}

		/// <summary>
		/// fromからlength個の配列を返す (fromを含む)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="origin"></param>
		/// <param name="from"></param>
		/// <param name="length">返り値の配列の要素数。もとの配列を突破する場合はもとの配列の末尾まで。</param>
		/// <returns></returns>
		public static T[] Slice<T>(this T[] origin, int from, int length)
		{
			if (length <= 0)
			{
				return new T[0];
			}
			if (from + length > origin.Length) length = origin.Length - from;
			var ret = new T[length];
			for (int i = 0; i < ret.Length; i++)
			{
				ret[i] = origin[i + from];
			}
			return ret;
		}

		/// <summary>
		/// 最初と最後を指定してスライス (最初の項と最後の項を含む)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="origin"></param>
		/// <param name="from">ここから含む</param>
		/// <param name="to">ここまで含む</param>
		/// <returns></returns>
		public static T[] SliceFromTo<T>(this T[] origin, int from, int to)
		{
			return origin.Slice(from, to - from + 1);
		}

		public static void ForAll<T>(this IEnumerable<T> collection, Action<T> action)
		{
			foreach (var item in collection)
			{
				action(item);
			}
		}

		public static void AddRangeOffset(this List<int> l, int[] target, int offset)
		{
			foreach (var item in target)
			{
				l.Add(item + offset);
			}
		}

		public static void AddRangeOffset(this List<short> l, short[] target, short offset)
		{
			foreach (short item in target)
			{
				l.Add((short)(item + offset));
			}
		}

		/// <summary>
		/// array[index % length]のアイテムを取得する。負対応
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="array"></param>
		/// <param name="index"></param>
		/// <returns>配列の長さが0ならばdefault(T)</returns>
		public static T ElementAtNormal<T>(this T[] array, int index)
		{
			if (array.Length == 0)
			{
				return default(T);
			}
			return array[MathUtil.PositiveMod(index, array.Length)];
		}

		/// <summary>
		/// リスト全体に対し処理を行いつつ、条件を満たす要素をリストから削除する。falseで削除
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list"></param>
		/// <param name="method">要素に対して行う処理。falseを返すとリストから削除される</param>
		public static void ForAllWithRemove<T>(this LinkedList<T> list, Func<T, bool> method)
		{
			LinkedListNode<T> node = list.First;
			LinkedListNode<T> delete = null;
			while (node != null)
			{
				if (method(node.Value) == false)
				{
					delete = node;
				}
				node = node.Next;
				if (delete != null)
				{
					delete.List.Remove(delete);
					delete = null;
				}
			}
		}

		/// <summary>
		/// 現在の型のベースクラスを追う(近い順に並びObjectで終わる、自分自身の型は含まない)
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static List<Type> GetBaseTypeTree(this Type type)
		{
			var list = new List<Type>();
			GetBaseTypeTreeInternal(type, list);
			return list;
		}

		static void GetBaseTypeTreeInternal(Type type, List<Type> list)
		{
			var bt = type.BaseType;
			if (bt == null)
			{
			}
			else
			{
				list.Add(bt);
				GetBaseTypeTreeInternal(bt, list);
			}
		}

		public static IEnumerable<T> GetCustomAttributes<T>(this System.Reflection.MemberInfo info, bool inherit)
		{
			return info.GetCustomAttributes(typeof(T), inherit).OfType<T>();
		}

		public static Version GetAssemblyVersion()
		{
			var asm = System.Reflection.Assembly.GetEntryAssembly();
			//asm = System.Reflection.Assembly.GetExecutingAssembly();
			return	new System.Reflection.AssemblyName(asm.FullName).Version;
		}

		#region MathUtils


		/// <summary>
		/// 1以下なら常に0を返す
		/// </summary>
		/// <param name="x"></param>
		/// <param name="divider"></param>
		/// <returns></returns>
		public static int PositiveMod(this int x, int divider)
		{
			if (divider <= 1)
			{
				return 0;
			}
			var value = x % divider;
			if (value < 0)
			{
				value += divider;
			}
			return value;
			//if (x >= 0)
			//{
			//	return x % divider;
			//}
			//else
			//{
			//	return x % divider + divider;
			//}
		}

		/// <summary>
		/// 1以下なら常に0を返す
		/// </summary>
		/// <param name="x"></param>
		/// <param name="divider"></param>
		/// <returns></returns>
		public static float PositiveMod(this float x, float divider)
		{
			if (divider <= 1)
			{
				return 0;
			}
			if (x >= 0)
			{
				return x % divider;
			}
			else
			{
				return x % divider + divider;
			}
		}

		/// <summary>
		/// 単振動。count=0で0
		/// </summary>
		/// <param name="count">位相</param>
		/// <param name="time">周期</param>
		/// <param name="a">振幅</param>
		/// <returns></returns>
		public static float Vibrate(float count, float time, float a)
		{
			return (float)Math.Sin(count / time * Math.PI * 2) * a;
		}

		/// <summary>
		/// 最大の変化量を指定して値を変化させる
		/// </summary>
		/// <param name="value">元の値</param>
		/// <param name="target">目的の値</param>
		/// <param name="limit">変化量上限(正値)</param>
		/// <returns></returns>
		public static float LimitChange(float value, float target, float limit)
		{
			System.Diagnostics.Debug.Assert(limit > 0);
			if (Math.Abs(value - target) < limit)
			{
				return target;
			}
			else if (value > target)
			{
				return value - limit;
			}
			else
			{
				return value + limit;
			}
		}



		#endregion


	}
}
