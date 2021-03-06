﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using DefinitionDictionary = System.Collections.Generic.Dictionary<string, object[]>;

namespace Masa.ScriptEngine
{
	/// <summary>
	/// ()の中身を表す構造体。最初と最後に( ) はつかない
	/// </summary>
	class PareBlock
	{
		public object[] tokens;
		public PareBlock(IEnumerable<object> t)
		{
			tokens = Parser.ParseStatement(t);//再帰
		}

		public override string ToString()
		{
			return tokens.Aggregate("(", (s, x) => s + x.ToString() + " ") + ")";
		}
	}

	/// <summary>
	/// ドット演算子前後
	/// </summary>
	class DotBlock
	{
		public object Left;
		public object Right;

		public DotBlock(object left, object right)
		{
			Left = left;
			Right = right;
		}

		public override string ToString()
		{
			var s = new StringBuilder();
			s.Append(Left.ToString());
			s.Append('.');
			s.Append(Right.ToString());
			return s.ToString();
		}
	}

	/// <summary>
	/// オプション項を表す構造体。最初に : はつかない
	/// </summary>
	class OptionBlock
	{
		public object[] Tokens;
		public string Name;
		public OptionBlock(IEnumerable<object> t)
		{
			var obj = t.FirstOrDefault();
			if (obj == null)
			{
				throw new ParseException("オプションの記法が不正\n : の後にトークンがない");
			}
			Name = obj as string;
			if (Name == null)
			{
				throw new ParseException("オプションの記法が不正\n : 後のトークンが文字列でない");
			}
			//tokens = t;
			Tokens = Parser.ParseStatement(t.Skip(1).ToArray());//再帰
		}

		public override string ToString()
		{
			var s = new StringBuilder(Name);
			foreach (var item in Tokens)
			{
				s.Append(item.ToString());
			}
			return s.ToString();
		}
	}

	/// <summary>
	/// Scannerが作ったトークン列をLine, PareBlock, OptionBlockなどにまとめる
	/// </summary>
	static class Parser
	{
		public static Line[] Parse(object[] tokens)
		{
			var lines = SplitTokensToLine(tokens).Where(l=>!l.IsEmpty);
			foreach (Line item in lines)
			{
				//if (!(item.Tokens[0] is string))
				//{
				//	throw new ParseException("行頭が無効な字です。", item);
				//}
				item.Tokens = ParseStatement(item.Tokens);
			}
			tokens = null;//不要
			
			return lines.ToArray();
		}



		static Line[] SplitTokensToLine(object[] tokens)
		{
			var tokenLines = DivideLine(tokens);
			var lines = new List<Line>();
			var definition = new DefinitionDictionary();
			for (int i = 0; i < tokenLines.Count; i++)
			{
				var l = new Line(i + 1, CheckBlockLevel(tokenLines[i]), ProcessDefinition( CleanLine(tokenLines[i]), definition));
				if (!l.IsEmpty) lines.Add(l);
				l.Index = lines.Count - 1;
			}
			return lines.ToArray();
		}

		static object[] ProcessDefinition(object[] line, DefinitionDictionary dict)
		{
			if (line.Length > 0 && line[0].Equals(Marks.Define))
			{
				dict[line[1] as string] = line.Skip(2).ToArray();
				return null;
			}
			else
			{
				List<object> tokens = new List<object>();
				object[] def = null;
				foreach (var item in line)
				{

					if (item is string && dict.TryGetValue(item as string, out def))
					{
						tokens.AddRange(def);
					}
					else
					{
						tokens.Add(item);
					}
				}
				return tokens.ToArray();
				//var n = new Line(line.Number, line.Level, new obj
			}
		}

		

		/// <summary>
		/// 1文を生トークン、PareBlock, OptionBlockにばらす
		/// </summary>
		/// <param name="line"></param>
		/// <returns></returns>
		internal static object[] ParseStatement(IEnumerable<object> line)
		{
			return GroupToDotBlock(GroupToOptionBlock(GroupToPareBlock(line)));
		}

		/// <summary>
		/// 5 20 ( 3 + 4 * (2 + 1) ) : from x y : way 360 (5 + 4)
		/// 5 20 PareBlock{ 3 + 4 * PareBlock{2 + 1} } : from x y : way 360 PareBlock{5 + 4}
		/// </summary>
		/// <param name="line"></param>
		/// <returns></returns>
		static object[] GroupToPareBlock(IEnumerable<object> line)
		{
			var ret = new List<object>();
			var inner = new List<object>();
			int pareLevel = 0;
			bool addFlag;
			foreach (object token in line)
			{
				addFlag = true;
				var mark = Marks.No;
				if (token is Marks)
				{
					mark = (Marks)token;
				}
				if (mark == Marks.PareOp)
				{
					if (pareLevel == 0)
					{
						addFlag = false;
					}
					pareLevel++;
				}
				else if (mark == Marks.PareCl)
				{
					pareLevel--;
					if (pareLevel == 0)
					{
						addFlag = false;
						var p = new PareBlock(inner);
						ret.Add(p);
						inner.Clear();
					}
				}
				if (addFlag)
				{
					if (pareLevel == 0)
					{
						ret.Add(token);
					}
					else
					{
						inner.Add(token);
					}
				}
				Debug.Assert(pareLevel >= 0, "カッコの対応が不正");
			}
			Debug.Assert(pareLevel == 0, "カッコの対応が不正");
			return ret.ToArray();
		}

		/// <summary>
		/// 5 20 Pareblock : from x y : way 360 Pareblock
		/// 5 20 Pareblock OptionBlock{from, x, y}, OptionBlock{way 360, Pareblock}
		/// </summary>
		/// <param name="line"></param>
		/// <returns></returns>
		static IEnumerable<object> GroupToOptionBlock(IEnumerable<object> line)
		{
			var ret = new List<object>();
			var inner = new List<object>();
			bool inOption = false;
			foreach (object token in line)
			{
				if (Marks.Colon.Equals(token))
				{
					if (inOption)
					{
						ret.Add(new OptionBlock(inner));
						inner.Clear();
					}
					else
					{
						inOption = true;
					}
				}
				else
				{
					if (inOption)
					{
						inner.Add(token);
					}
					else
					{
						ret.Add(token);
					}
				}
			}
			
			if (inOption)
			{
				ret.Add(new OptionBlock(inner));
				inner.Clear();
			}
			return ret.ToArray();
		}

		/// <summary>
		/// 行ないし括弧の中身にドット構文があればDotBlockに置換する
		/// </summary>
		/// <param name="block"></param>
		/// <returns></returns>
		static object[] GroupToDotBlock(IEnumerable<object> block)
		{
			foreach (object t in block)
			{
				if (t is PareBlock)//引数などに存在しうるドット構文を先に処理
				{
					var p = t as PareBlock;
					p.tokens = GroupToDotBlock(p.tokens);
				}
				if (t is OptionBlock)
				{
					var o = t as OptionBlock;
					o.Tokens = GroupToDotBlock(o.Tokens);
				}
			}
			
			var src = new LinkedList<object>(block);
			LinkedListNode<object> current = src.First;
			while (current != null)
			{
				if (current.Value is Marks && ((Marks)current.Value) == Marks.Dot)
				{
					var dot = new DotBlock(current.Previous.Value, current.Next.Value);
					src.Remove(current.Previous);
					src.Remove(current.Next);
					src.AddAfter(current, dot);
					current = current.Next;
					src.Remove(current.Previous);//ドット前後3トークンを削除してDotBlockに置き換え
				}
				else
				{
					current = current.Next;
				}
			}
			return src.ToArray();
		}

		
		/// <summary>
		/// 行がどれだけインデントされているか取得する
		/// </summary>
		/// <param name="line">調べる行</param>
		/// <returns>インデント数</returns>
		static int CheckBlockLevel(object[] line)
		{
			for (int i = 0; i < line.Length; i++)
			{
				if (!Marks.Tab.Equals(line[i]))
				{
					return i;
				}
			}
			return 0;
		}

		/// <summary>
		/// タブ、改行、コメントなどを取り除く
		/// </summary>
		/// <param name="line"></param>
		/// <returns></returns>
		static object[] CleanLine(object[] line)
		{
			var ret = new List<object>();
			foreach (var item in line)
			{
				if (!(item is Marks))
				{
					ret.Add(item);
				}
				else
				{
					var i = (Marks)item;
					if (i == Marks.Tab || i == Marks.Return)
					{
					}
					else if (i == Marks.Semicolon)
					{
						return ret.ToArray();
					}
					else
					{
						ret.Add(item);
					}
				}
			}
			return ret.ToArray();
		}

		static List<object[]> DivideLine(object[] tokens)
		{
			var ret = new List<object[]>();
			var line = new List<object>();
			bool comment = false;
			int index = 0;
			while (index < tokens.Length)
			{
				object token = tokens[index];
				var mark = Marks.No;
				if (token is Marks)
				{
					mark = (Marks)token;
				}
				if (mark == Marks.Return)
				{
					ret.Add(line.ToArray());
					line.Clear();
					comment = false;
				}
				else
				{
					if (mark == Marks.Semicolon)
					{
						comment = true;
					}
					if (!comment)
					{
						line.Add(token);
					}
				}
				index++;
			}
			//最終行が改行で終わっていなかった場合用
			ret.Add(line.ToArray());
			line.Clear();
			return ret;
		}
	}
}
