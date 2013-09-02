using System;
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
	struct PareBlock
	{
		public object[] tokens;
		public PareBlock(object[] t)
		{
			tokens = Parser.ParseStatement(t);//再帰
		}

		public override string ToString()
		{
			return tokens.Aggregate("(", (s, x) => s + x.ToString() + " ") + ")";
		}
	}

	/// <summary>
	/// オプション項を表す構造体。最初に : はつかない
	/// </summary>
	struct OptionBlock
	{
		public object[] Tokens;
		public string Name;
		public OptionBlock(object[] t)
		{
			if (t.Length == 0)
			{
				throw new ParseException("オプションの記法が不正\n : の後にトークンがない");
			}
			if (!(t[0] is string))
			{
				throw new ParseException("オプションの記法が不正\n : 後のトークンが文字列でない");
			}
			//tokens = t;
			Name = (string)t[0];
			Tokens = Parser.ParseStatement(t.Skip(1).ToArray());//再帰
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
		internal static object[] ParseStatement(object[] line)
		{
			object[] tokens = GroupToPareBlock(line);
			tokens = GroupToOptionBlock(tokens);
			return tokens;
		}

		/// <summary>
		/// 5 20 ( 3 + 4 * (2 + 1) ) : from x y : way 360 (5 + 4)
		/// 5 20 PareBlock{ 3 + 4 * PareBlock{2 + 1} } : from x y : way 360 PareBlock{5 + 4}
		/// </summary>
		/// <param name="line"></param>
		/// <returns></returns>
		static object[] GroupToPareBlock(object[] line)
		{
			var ret = new List<object>();
			var inner = new List<object>();
			int pareLevel = 0;
			bool addFlag;
			for (int i = 0; i < line.Length; i++)
			{
				object token = line[i];
				addFlag = true;
				if (Marks.PareOp.Equals(token))
				{
					if (pareLevel == 0)
					{
						addFlag = false;
					}
					pareLevel++;
				}
				if (Marks.PareCl.Equals(token))
				{
					pareLevel--;
					if (pareLevel == 0)
					{
						addFlag = false;
						var p = new PareBlock(inner.ToArray());
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
		static object[] GroupToOptionBlock(object[] line)
		{
			var ret = new List<object>();
			var inner = new List<object>();
			bool inOption = false;
			Action generate = () =>
			{
				ret.Add(new OptionBlock(inner.ToArray()));
				inner.Clear();

			};
			for (int i = 0; i < line.Length; i++)
			{
				object token = line[i];
				if (Marks.Colon.Equals(token))
				{
					if (inOption)
					{
						generate();
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
				generate();
			}
			return ret.ToArray();
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
				if (Marks.Return.Equals(token))
				{
					ret.Add(line.ToArray());
					line.Clear();
					comment = false;
				}
				else
				{
					if (Marks.Semicolon.Equals(token))
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
