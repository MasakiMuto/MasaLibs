using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Masa.ScriptEngine
{
	using Value = System.Single;

	enum Marks
	{
		No,///なし

		Pos,/// + 演算子にも符号にも
		Neg,/// -

		Mul,/// *
		Div,/// /
		Mod,/// %

		Not,/// !
		And,/// &
		Or,/// |

		Equal,///比較等号 ==
		NotEqual,/// !=
		Big, /// >
		BigEqual,/// >=
		Small, /// <
		SmallEqual, /// <=

		Sub,///代入 =
		Inc, /// ++
		Dec, /// --
		SubPos, /// +=
		SubNeg, /// -=
		SubMul, /// *=
		SubDiv, /// /=

		PareOp,/// (
		PareCl,/// )

		Colon, /// : オプション文マーカー

		Dollar, /// $ varでの型指定

		Tab,
		Return,
		Semicolon,///コメント

		Define,//#define
		Include,//#include
	}

	/// <summary>
	/// スクリプトコードを読み込んで単語単位にばらしてデータ化するひと。
	/// </summary>
	public sealed class Scanner
	{
		/// <summary>
		/// string,Mark,floatのどれか
		/// </summary>
		public List<object> Tokens
		{
			get;
			private set;
		}

		readonly Dictionary<string, string> headerDictionary;

		static readonly Dictionary<string, Marks> MarkNameDict;
		public const char StringLiteralMark = '@';


		static Scanner()
		{
			MarkNameDict = new Dictionary<string, Marks>();
			MarkNameDict.Add("\t", Marks.Tab);
			MarkNameDict.Add("\n", Marks.Return);
			MarkNameDict.Add(":", Marks.Colon);
			MarkNameDict.Add("+", Marks.Pos);
			MarkNameDict.Add("-", Marks.Neg);
			MarkNameDict.Add("*", Marks.Mul);
			MarkNameDict.Add("/", Marks.Div);
			MarkNameDict.Add("%", Marks.Mod);
			MarkNameDict.Add("!", Marks.Not);
			MarkNameDict.Add("&", Marks.And);
			MarkNameDict.Add("|", Marks.Or);
			MarkNameDict.Add("=", Marks.Sub);
			MarkNameDict.Add("!=", Marks.NotEqual);
			MarkNameDict.Add("<", Marks.Small);
			MarkNameDict.Add("<=", Marks.SmallEqual);
			MarkNameDict.Add(">", Marks.Big);
			MarkNameDict.Add(">=", Marks.BigEqual);
			MarkNameDict.Add("++", Marks.Inc);
			MarkNameDict.Add("--", Marks.Dec);
			MarkNameDict.Add("==", Marks.Equal);
			MarkNameDict.Add("+=", Marks.SubPos);
			MarkNameDict.Add("-=", Marks.SubNeg);
			MarkNameDict.Add("*=", Marks.SubMul);
			MarkNameDict.Add("/=", Marks.SubDiv);
			MarkNameDict.Add("(", Marks.PareOp);
			MarkNameDict.Add(")", Marks.PareCl);
			MarkNameDict["#define"] = Marks.Define;
			MarkNameDict["#include"] = Marks.Include;
			MarkNameDict["$"] = Marks.Dollar;
		}

		//TODO 例外が出たところのファイル名や行を表示できるようにする
		public Scanner(string code)
			: this(code, null)
		{

		}

		public Scanner(string code, Dictionary<string, string> headerDict)
		{
			this.headerDictionary = headerDict;
			Tokens = new List<object>();
			Scan(code);
		}


		/// <summary>
		/// 単語に分割する(;含めてコメント除去、@@リテラルを@始まりの1単語に)
		/// </summary>
		/// <param name="code"></param>
		/// <returns></returns>
		string[] Split(string code)
		{

			var ret = new List<string>();
			bool isLiteral = false;
			bool isComment = false;
			StringBuilder builder = new StringBuilder(256);
			Action enter = () =>
				{
					if (builder.Length != 0)
					{
						ret.Add(builder.ToString());
						builder.Clear();
					}
				};

			code += "\n";//末行処理
			code = code.Replace(System.Environment.NewLine, "\n");


			for (int i = 0; i < code.Length; i++)
			{
				if (isComment)
				{
					if (code[i] == '\n')
					{
						isComment = false;
						ret.Add("\n");
					}
					continue;
				}
				if (isLiteral)
				{
					if (code[i] == StringLiteralMark)
					{
						isLiteral = false;
						enter();
					}
					else
					{
						builder.Append(code[i]);
					}
					continue;
				}
				switch (code[i])
				{
					case ' ':
						enter();
						break;
					case ';':
						enter();
						isComment = true;
						break;
					case '\n':
						goto case ')';
					case '\t':
						goto case ')';
					case '(':
						goto case ')';
					case ')':
						enter();
						ret.Add(code[i].ToString());
						break;
					case StringLiteralMark:
						enter();
						isLiteral = true;
						builder.Append(StringLiteralMark);
						break;
					default:
						builder.Append(code[i].ToString());
						break;
				}
			}
			return ret.ToArray();
		}

		//string Include(string code)
		//{
		//	var lines = code.Split('\n');
		//	string[] tokens;
		//	foreach (var item in lines)
		//	{
		//		tokens = item.Split(null);
		//		if (tokens.Length == 2 && tokens[0] == "#define")
		//		{

		//		}
		//	}
		//}

		string[] Include(string[] tokens)
		{
			List<string> result = null;
			int head = 0;
			for (int i = 0; i < tokens.Length; i++)
			{
				if (tokens[i] == "#include")
				{
					if (result == null)
					{
						result = new List<string>(tokens.Length);
					}
					result.AddRange(tokens.Skip(head).Take(i - head));
					string value;
					if (headerDictionary.TryGetValue(tokens[i + 1], out value))
					{
						result.AddRange(Split(value));
					}
					else
					{
						throw new ParseException("インクルードに失敗:" + tokens[i + 1]);
					}
					i += 2;
					head = i;
				}
			}
			if (result != null)
			{
				result.AddRange(tokens.Skip(head).Take(tokens.Length - head));
				return result.ToArray();
			}
			else
			{
				return tokens;
			}
		}

		void Scan(string code)
		{

			//if (headerDictionary != null)
			//{
			//	code = Include(code);
			//}
			var t = Split(code);
			if (headerDictionary != null)
			{
				t = Include(t);
			}
			foreach (var item in t)
			{
				if (item.Length == 0)
				{
				}
				else if (MarkNameDict.ContainsKey(item))
				{
					Tokens.Add(MarkNameDict[item]);
				}
				else if (IsIdLetter(item[0]) || item[0] == StringLiteralMark)//general string
				{
					Tokens.Add(item);
				}
				else if (Char.IsDigit(item[0]))
				{
					try
					{
						Tokens.Add(Value.Parse(item));
					}
					catch
					{
						throw new Exception("不正な文字列 " + item);
					}
				}
				else if (IsSingleMark(item[0]))
				{
					if (item.Length > 1 && IsIdLetter(item[1]))//単項演算子 -hoge
					{
						Tokens.Add(MarkNameDict[item[0].ToString()]);
						Tokens.Add(item.Substring(1));
					}
					else//-1 etc.
					{
						try
						{
							Tokens.Add(Single.Parse(item));
						}
						catch
						{
							throw new Exception("不正な文字列 " + item);
						}
					}
				}
				else
				{
					throw new Exception("不正な文字列 " + item);
				}
			}
		}

		void ProcessDefine()
		{
			var definition = new Dictionary<string, List<object>>();
			for (int i = 0; i < Tokens.Count; i++)
			{
				if (Tokens[i].Equals(Marks.Define))
				{
					var list = new List<object>();
					for (int j = i + 2; true; j++)
					{
						if (Tokens[j].Equals(Marks.Return))
						{
							break;
						}
						else
						{
							list.Add(Tokens[j]);
						}
					}
					definition[Tokens[i + 1] as string] = list;
				}
			}

		}

		/// <summary>
		/// 単項演算子か
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		bool IsSingleMark(char i)
		{
			return i == '+' || i == '-' || i == '!';
			//return (i == '+' || i == '-' || i == '!');
		}

		/// <summary>
		/// 識別子の1文字目として使える文字か
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		bool IsIdLetter(char i)
		{
			return (char.IsLetter(i) || i == '_');
		}


	}
}
