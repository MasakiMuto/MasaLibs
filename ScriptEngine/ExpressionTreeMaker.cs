using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using Masa.Lib;

//using ScriptEngineEx.Environment = ScriptEngineEx.Environment;

namespace Masa.ScriptEngine
{
	using Value = System.Single;
	using Vector2 = Microsoft.Xna.Framework.Vector2;
	using Vector3 = Microsoft.Xna.Framework.Vector3;

	public class ExpressionTreeMaker : IExpressionTreeMaker
	{
		public Action<ScriptEngine.Environment> Statement { get; private set; }
		public Action<ScriptEngine.Environment> InitStatement { get; private set; }
		public int GlobalVarNumber//Enviromentにあげる
		{
			get { return GlobalVarList.Count; }
		}

		Dictionary<string, Action<Environment>> LabelStatementCashe;
		Expression[] TotalBlock;
		List<string> GlobalVarList;
		Dictionary<string, ParameterExpression> VarDict;
		Dictionary<string, Expression> LabelDict;
		Dictionary<string, MethodInfo> MethodDict;
		Dictionary<string, PropertyInfo> PropertyDict;
		ParameterExpression Environment;
		string[] NameValueTable;
		Line[] Lines;
		Type TargetType;

		static readonly LabelTarget ExitLabel = Expression.Label("_ScriptExit");
		internal static readonly Type ValueType = typeof(Value);
		internal static readonly Expression ZeroExpression = Expression.Constant(0f, ValueType);
		internal static readonly Expression OneExpression = Expression.Constant(1f, ValueType);
		static readonly Expression NanExpression = Expression.Constant(Value.NaN, ValueType);
		//static readonly LabelTarget LOOPEND = Expression.Label("EndLoop");
		static readonly Dictionary<string, FieldInfo> EnvironmentField = ExpressionTreeMakerHelper.GetEnvironmentFieldInfo();
		static readonly Dictionary<string, PropertyInfo> EnvironmentProperty = ExpressionTreeMakerHelper.GetEnvironmentPropertyInfo();
		static readonly Dictionary<string, MethodInfo> StaticMethodDict = ExpressionTreeMakerHelper.GetStaticMethodInfo();
		static readonly Dictionary<Type, ClassReflectionInfo> ReflectionCashe = new Dictionary<Type, ClassReflectionInfo>();
		static readonly Dictionary<string, Expression> ConstantValueDict = ExpressionTreeMakerHelper.GetConstantValueDictionary();
		static readonly Dictionary<string, Type> TypeNameDictionary = ExpressionTreeMakerHelper.GetTypeNameDictionary();

		public ExpressionTreeMaker(object[] token, Type targetType)
			: this(token, targetType, null)
		{

		}

		/// <summary>
		/// 文字列列挙付き
		/// </summary>
		/// <param name="token"></param>
		/// <param name="targetType"></param>
		/// <param name="nameValueTable"></param>
		public ExpressionTreeMaker(object[] token, Type targetType, string[] nameValueTable)
		{
			NameValueTable = nameValueTable;
			TargetType = targetType;
			VarDict = new Dictionary<string, ParameterExpression>();
			LabelDict = new Dictionary<string, Expression>();
			Environment = Expression.Parameter(typeof(ScriptEngine.Environment));
			GlobalVarList = new List<string>();
			MethodDict = new Dictionary<string, MethodInfo>();
			PropertyDict = new Dictionary<string, PropertyInfo>();
			GetTargetInfo();
			Parse(token);
		}

		#region 準備系

		/// <summary>
		/// 型情報のキャッシュを作る。明示的に呼ばなくても必要なら作られる
		/// </summary>
		public static void MakeTargetInfoCache(Type target)
		{
			if (ReflectionCashe.ContainsKey(target)) return;
			ReflectionCashe[target] = ExpressionTreeMakerHelper.MakeTargetInfoCache(target);
		}

		void GetTargetInfo()
		{
			if (!ReflectionCashe.ContainsKey(TargetType))
			{
				MakeTargetInfoCache(TargetType);
			}
			MethodDict = ReflectionCashe[TargetType].MethodDict;
			PropertyDict = ReflectionCashe[TargetType].PropertyDict;
		}

		public string OutputClassInformation()
		{
			return DocumentCreater.OutputClass(MethodDict, PropertyDict);
		}

		#endregion

		/// <summary>
		/// ラベルの塊をActionとして返す。ラベルの初回読み込みでは式木をコンパイルするので速度に注意。以降はキャッシュされる
		/// </summary>
		/// <param name="label"></param>
		/// <returns>ラベルが存在しなければnull</returns>
		public Action<Environment> GetLabelStatement(string label)
		{
			if (LabelStatementCashe == null)
			{
				LabelStatementCashe = new Dictionary<string, Action<Environment>>();
			}
			if (!LabelStatementCashe.ContainsKey(label))
			{
				if (LabelDict.ContainsKey(label))
				{
					LabelStatementCashe.Add(label, Expression.Lambda<Action<ScriptEngine.Environment>>(Expression.Block(VarDict.Values, LabelDict[label], Expression.Label(ExitLabel)), Environment).Compile());
				}
				else
				{
					return null;
				}
			}
			return LabelStatementCashe[label];
		}

		#region Compiler

		/// <summary>
		/// スクリプト全体をコンパイルする
		/// </summary>
		/// <param name="mtd">出力先</param>
		public void Compile(System.Reflection.Emit.MethodBuilder mtd)
		{
			var lambda = Expression.Lambda<Action<ScriptEngine.Environment>>(Expression.Block(VarDict.Values, TotalBlock), Environment);
			lambda.CompileToMethod(mtd);
		}

		public bool CompileLabel(string label, System.Reflection.Emit.MethodBuilder mtd)
		{
			if (!LabelDict.ContainsKey(label)) return false;
			var lambda = Expression.Lambda<Action<ScriptEngine.Environment>>(Expression.Block(VarDict.Values, LabelDict[label]), Environment);
			lambda.CompileToMethod(mtd);
			return true;
		}

		public Type CompileToClass(Type original, string className, System.Reflection.Emit.ModuleBuilder mb)
		{
			TypeBuilder tp = mb.DefineType(className, TypeAttributes.Public, original);

			Func<string, MethodBuilder> define = (n) => tp.DefineMethod(n, MethodAttributes.Family | MethodAttributes.HideBySig | MethodAttributes.ReuseSlot | MethodAttributes.Virtual, null, new[] { typeof(Environment) });
			Compile(define("ScriptMain"));
			//foreach (var item in original.GetMethods().Where(m=>m.GetCustomAttributes(typeof(ScriptDefinedMethodAttribute), true).Count() > 0))
			//{
			//    if (LabelExist(item.Name))
			//    {
			//        CompileLabel(item.Name, define(item.Name));
			//    }
			//}			
			foreach (var item in LabelDict)
			{
				CompileLabel(item.Key, define("Script" + item.Key));
			}
			var getter = tp.DefineMethod("get_GlobalVarNumber", MethodAttributes.Family | MethodAttributes.SpecialName | MethodAttributes.HideBySig);
			Expression.Lambda<Func<int>>(Expression.Constant(GlobalVarNumber)).CompileToMethod(getter);
			tp.DefineProperty("GlobalVarNumber", PropertyAttributes.None, typeof(int), null).SetGetMethod(getter);
			//tp.DefineField("GlobalVarNumber", typeof(int), FieldAttributes.Private | FieldAttributes.Literal).SetConstant(GlobalVarNumber);
			return tp.CreateType();
		}

		#endregion

		public bool LabelExist(string label)
		{
			return LabelDict.ContainsKey(label);
		}

		#region Parse

		void Parse(object[] tokens)
		{
			var topStatements = new List<Expression>();
			Lines = Parser.Parse(tokens);
			for (int i = 0; i < Lines.Length; i++)
			{
				if (Lines[i].Level == 0)
				{
					var e = ProcessStatement(Lines[i]);
					if (e != null) topStatements.Add(e);
				}
			}

			//var returnTarget = Expression.Label(ExitLabel);
			topStatements.Add(Expression.Label(ExitLabel));
			TotalBlock = topStatements.ToArray();

			var lambda = Expression.Lambda<Action<ScriptEngine.Environment>>(Expression.Block(VarDict.Values, TotalBlock), Environment);
			//Console.WriteLine(lambda.ToString());

			Statement = lambda.Compile();

			InitStatement = GetLabelStatement("init");
		}


		/// <summary>
		/// 文字列を変数(内部変数、Global変数、外部変数、列挙文字列すべて)としてパース。
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		Expression ParseVariable(string id)
		{
			if (id[0] == Scanner.StringLiteralMark)//@エスケープ文字列の場合
			{
				return RegistLiteral(id.Substring(1));
			}
			ParameterExpression prm;
			if (VarDict.TryGetValue(id, out prm))
			{
				return prm;
			}
			PropertyInfo prp;
			if (PropertyDict.TryGetValue(id, out prp))
			{
				return Expression.Property(Expression.Convert(Expression.Field(Environment, ScriptEngine.Environment.Info_TargetObject), TargetType), prp);
			}

			if (EnvironmentField.ContainsKey(id))
			{
				return Expression.Field(Environment, EnvironmentField[id]);
			}
			if (EnvironmentProperty.ContainsKey(id))
			{
				return Expression.Property(Environment, EnvironmentProperty[id]);
			}
			if (ConstantValueDict.ContainsKey(id))
			{
				return ConstantValueDict[id];
			}
			if (NameValueTable != null && NameValueTable.Contains(id))
			{
				return MakeConstantExpression(Array.FindIndex(NameValueTable, s => s == id));
			}
			int gvar = GlobalVarList.FindIndex(k => k == id);
			if (gvar != -1)
			{
				return Expression.Property(Environment, ScriptEngine.Environment.Info_Item, Expression.Constant(gvar, typeof(int)));
			}
			return RegistLiteral(id);
		}

		Expression RegistLiteral(string id)
		{
			return Expression.Constant(id, typeof(string));
			//int i = StringLiterals.FindIndex(s => s == id);
			//if (i == -1)
			//{
			//	StringLiterals.Add(id);
			//	return MakeConstantExpression(StringLiterals.Count - 1);
			//}
			//else
			//{
			//	return MakeConstantExpression(i);
			//}
		}

		static ConstantExpression MakeConstantExpression(float value)
		{
			return Expression.Constant(value, ValueType);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="line">引数とオプションがくっついたトークン列</param>
		/// <returns></returns>
		Option[] GetOptions(object[] line)
		{
			return line.OfType<OptionBlock>().Select((o) => ParseOptionBlock(o)).ToArray();
		}

		Option ParseOptionBlock(OptionBlock opt)
		{
			return new Option(opt.Name, GetArgs(opt.Tokens));
		}

		#endregion

		#region 式の生成
		Expression ProcessStatement(Line line)
		{
			//line.Tokens = ParseLine(line.Tokens);//括弧やオプションをまとめる
			string id = (string)line.Tokens[0];
			if (id == "var")
			{
				return DefineVariable(line);
			}
			if (id == "varg")
			{
				if (line.Tokens.Length > 2)
				{
					throw new ParseException("varg宣言の後に無効なトークン(初期化不可能)", line);
				}
				//return new GlobalVar((string)line[1]);
				GlobalVarList.Add((string)line.Tokens[1]);
				return null;
			}

			if (line.Tokens.Length > 1 && line.Tokens[1] is Marks)
			{
				Marks m = (Marks)line.Tokens[1];
				if (m == Marks.Sub || m == Marks.SubNeg || m == Marks.SubPos || m == Marks.SubMul || m == Marks.SubDiv || m == Marks.Inc || m == Marks.Dec)
				{
					return ProcessAssign(line);
				}
				else//line[1]がMarkかつ代入系でない→ありえない
				{
					//throw new Exception("トークンの2番目が不正なマーク Line" + line.Number + ":" + m.ToString());
					throw new ParseException("トークンの2番目が不正なマーク", line);
				}
			}
			//line[1]がMarkでない && var系でない
			return ProcessNormalStatement(line);

		}

		Expression DefineVariable(Line line)
		{
			ParameterExpression v;
			string name = (string)line.Tokens[1];
			Type type = typeof(Value);
			if (line.Tokens.Length >= 4 && line.Tokens[2].Equals(Marks.Dollar))// var hoge $ float2
			{
				type = TypeNameDictionary[line.Tokens[3] as string];
			}

			v = Expression.Parameter(type, name);
			VarDict.Add(name, v);
			if (line.Tokens.Length >= 4)// var hoge = 1
			{
				int rightPos = Array.FindIndex(line.Tokens, x => x.Equals(Marks.Sub));
				if (rightPos == -1)
				{
					return null;//初期化なし
				}
				return Expression.Assign(v, ParsePareBlock(new PareBlock(line.Tokens.Skip(rightPos + 1).ToArray())));
			}
			else
			{
				return null; //初期化なし
			}
		}

		/// <summary>
		/// 代入処理
		/// </summary>
		/// <param name="line"></param>
		/// <returns></returns>
		Expression ProcessAssign(Line line)
		{
			Marks m = (Marks)line.Tokens[1];
			Expression target = ParseVariable((string)line.Tokens[0]);//代入先の変数/プロパティ
			if (m == Marks.Inc)
			{
				//return Expression.Assign(target, Expression.Increment(target));
				return Expression.PostIncrementAssign(target);
			}
			else if (m == Marks.Dec)
			{
				//return Expression.Assign(target, Expression.Decrement(target));
				return Expression.PostDecrementAssign(target);
			}
			else
			{
				Expression r = ParsePareBlock(new PareBlock(line.Tokens.Skip(2).ToArray()));//代入される値
				//Expression r = ParsePareBlock(new PareBlock(new[] { line.Tokens[2] }));
				switch (m)
				{
					case Marks.Sub:
						return Expression.Assign(target, r);
					case Marks.SubPos:
						return Expression.AddAssign(target, r);
					case Marks.SubNeg:
						return Expression.SubtractAssign(target, r);
					case Marks.SubMul:
						return Expression.MultiplyAssign(target, r);
					case Marks.SubDiv:
						return Expression.DivideAssign(target, r);
				}
			}
			//throw new Exception("Line " + line.Number + "がおかしい");
			throw new ParseException("おかしい", line);


		}

		/// <summary>
		/// Global Varの処理
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		//IndexExpression GetEnvironmentValue(string key)
		//{
		//	int i = GlobalVarList.FindIndex((k) => k == key);
		//	if (i == -1)
		//	{
		//		throw new ScriptEngine.ParseException("変数" + key + "は未定義");
		//	}
		//	return Expression.Property(Environment, ScriptEngine.Environment.Info_Item, Expression.Constant(i, typeof(int)));
		//}

		/// <summary>
		/// if ループ stateなどの制御文や、fireなどの外部命令を処理
		/// 一般文、つまり代入文や宣言文以外の、「識別子 引数リスト・・・」となる文
		/// </summary>
		/// <param name="line">PareBlockやOptionBlockに整形済みのトークン行</param>
		/// <returns></returns>
		Expression ProcessNormalStatement(Line line)
		{
			string id = (string)line.Tokens[0];
			Func<Expression[]> args = () => GetArgs(line.Tokens.Skip(1));
			switch (id)
			{
				case "if":
					return MakeIfStatement(line, Expression.NotEqual(args()[0], ZeroExpression));
				//return Expression.IfThen(Expression.NotEqual(args()[0], ZERO), GetBlock(line));
				case "else"://if - else の流れで処理するため単体ではスルー
					return null;
				case "while":
					goto case "repeat";
				case "repeat":
					LabelTarget label = Expression.Label(line.Number.ToString());
					return Expression.Loop(GetBlockWithBreak(line, Expression.Equal(args()[0], ZeroExpression), label), label);
				case "loop":
					return MakeLoopStatement(line);
				case "goto":
					var assign = Expression.Assign(Expression.Property(Environment, ScriptEngine.Environment.Info_State), args()[0]);
					return Expression.Block(assign, Expression.Return(ExitLabel));
				//return Expression.Assign(Expression.Property(Environment, ScriptEngine.Environment.Info_State), args()[0]);
				case "state":
					return Expression.IfThen(Expression.Equal(Expression.Property(Environment, ScriptEngine.Environment.Info_State), args()[0]), GetBlock(line));
				case "label":
					LabelDict.Add((string)line.Tokens[1], GetBlock(line));
					return null;
				case "jump":
					string s = (string)line.Tokens[1];
					if (LabelDict.ContainsKey(s))
					{
						return LabelDict[s];
					}
					else
					{
						throw new ParseException("未定義のラベル " + s, line);
					}
				case "blank":
					//return null;
					return Expression.Empty();
				default:
					if (StaticMethodDict.ContainsKey(id))//オプション無しの前提
					{
						return Expression.Call(StaticMethodDict[id], args());
					}
					if (MethodDict.ContainsKey(id))
					{
						return CallExternalMethod(id, line.Tokens.Slice(1, line.Tokens.Length - 1));
					}
					throw new ParseException(": 未定義のステートメント " + id, line);
			}
		}

		Expression MakeIfStatement(Line line, Expression pred)
		{
			var index = line.Index;
			var ifBlock = GetBlock(line);

			//return Expression.IfThen(Expression.NotEqual(args()[0], ZERO), GetBlock(line));
			if (Lines.Length > index + 2)
			{
				//var elseLine = Lines.Skip(index).FirstOrDefault(l => l.Level == line.Level && l.Tokens.Length == 1 && l.Tokens[0].Equals("else"));
				var elseLine = Lines.Skip(index + 1).FirstOrDefault(l => l.Level == line.Level && l.Tokens.Length >= 1);
				if (elseLine != null && elseLine.Tokens[0].Equals("else"))
				{
					return Expression.IfThenElse(pred, ifBlock, GetBlock(elseLine));
				}
			}
			return Expression.IfThen(pred, ifBlock);
		}

		//loop (times) (freq) (from)
		Expression MakeLoopStatement(Line line)
		{
			string id = (string)line.Tokens[0];
			Option[] opt = GetOptions(line.Tokens.Slice(1, line.Tokens.Length - 1));
			var arg = GetArgs(line.Tokens.Slice(1, line.Tokens.Length - 1));
			var times = arg[0];
			var freq = arg[1];
			var from = arg[2];
			Expression frame = null;
			Expression last = null;
			Expression firstSentence;

			if (opt.Length == 0)
			{
				frame = Expression.Field(Environment, ScriptEngine.Environment.Info_StateFrame);
			}
			else
			{
				Option o = opt.FirstOrDefault((op) => op.Name == "counter");
				if (o == null)
				{
					throw new ParseException("Loop文に無効なオプション " + opt.First().Name + "が指定された", line);
				}
				if (o.Args.Length > 2)
				{
					throw new ParseException("Loop文のcounterオプションの引数の数が変", line);
				}
				frame = o.Args[0];
				if (o.Args.Length == 2)
				{
					last = o.Args[1];
				}
				else
				{
					last = Expression.Subtract(frame, Expression.Constant(1f, ValueType));
				}
			}
			firstSentence = Expression.AndAlso
								(
									Expression.GreaterThanOrEqual(frame, from),
									Expression.OrElse
									(
										Expression.Equal(times, ZeroExpression),
										Expression.LessThan(frame, Expression.Add(from, Expression.Multiply(freq, times)))
									)
								);
			if (opt.Length == 0)
			{
				//Expression fr = Expression.Field(Environment, ScriptEngine.Environment.Info_StateFrame);

				//if ((stateFrame >= from &&
				//(times == 0 || stateFrame < from + freq * times)) &&
				//((stateFrame - from) % freq == 0))
				return Expression.IfThen(Expression.AndAlso
					(
						firstSentence,
						Expression.Equal(Expression.Modulo(Expression.Subtract(frame, from), freq), ZeroExpression)
					),
					GetBlock(line));
			}
			else
			{
				//Option o = opt.FirstOrDefault((op) => op.Name == "counter");
				//if (o == null)
				//{
				//    throw new ParseException("Loop文に無効なオプション " + opt.First().Name + "が指定された", line);
				//}
				//if (o.Args.Length != 2)
				//{
				//    throw new ParseException("Loop文のcounterオプションの引数の数が変", line);
				//}
				//Expression fr = o.Args[0];
				//Expression lfr = o.Args[1];

				//times = arg[0], freq = arg[1], from = arg[2]
				//if (
				//			fr >= from &&
				//        (times == 0 || freq * times > (fr - from))  &&
				//        (
				//            lafr < from 
				//            ||
				//            ((int)((fr - from) / freq) > (int)((lafr - from) / freq))
				//        )
				//    )
				Func<Expression, Expression> div = (counter) => Expression.Convert
					(
						Expression.Divide
						(
							Expression.Subtract(Expression.Subtract(counter, Expression.Constant(0.1f, typeof(float))), from),
							freq
						),
						typeof(int)
					);
				return Expression.IfThen
					(
						Expression.AndAlso
						(
							firstSentence,
							Expression.OrElse
							(
									Expression.LessThan(last, from),
									Expression.GreaterThan(div(frame), div(last))
							)
						),
					//Expression.AndAlso
					//(
					//    Expression.GreaterThanOrEqual(fr, from),
					//    Expression.AndAlso
					//    (
					//        Expression.OrElse
					//        (
					//            Expression.Equal(arg[0], ZERO),
					//            Expression.GreaterThan(Expression.Multiply(freq, times), Expression.Subtract(fr, from))
					//        ),
					//        Expression.OrElse
					//        (
					//            Expression.LessThan(lfr, from),
					//            Expression.GreaterThan(
					//                Expression.Convert(
					//                    Expression.Divide(
					//                        Expression.Subtract(fr, from),
					//                        freq
					//                    ),
					//                    typeof(int)
					//                ),
					//                Expression.Convert(
					//                    Expression.Divide(
					//                        Expression.Subtract(lfr, arg[2]),
					//                        arg[1]
					//                    ),
					//                    typeof(int)
					//                )
					//            )
					//        )
					//    )
					//),
						GetBlock(line)
					);
			}
		}

		/// <summary>
		/// Targetの持つメソッドや関数を呼ぶ
		/// </summary>
		/// <param name="id">メソッド名</param>
		/// <param name="l">名前を除いたトークン列</param>
		/// <returns></returns>
		Expression CallExternalMethod(string id, object[] l)
		{
			//object[] l = line.Tokens.Slice(1, line.Tokens.Length - 1);
			List<Expression> args = GetArgs(l).ToList();
			Option[] options = GetOptions(l);
			ScriptMemberAttribute atrribute = (ScriptMemberAttribute)MethodDict[id].GetCustomAttributes(typeof(ScriptMemberAttribute), true).First();
			if (atrribute.OptionName != null)//オプションが定義されていれば
			{
				string[] name = atrribute.OptionName;
				int[] num = atrribute.OptionArgNum;
				var less = options.Select(o => o.Name).Except(name);
				if (less.Any())
				{
					throw new ParseException(id + "メソッド呼び出しに無効なオプション指定 : " + less.Aggregate((src, dst) => dst + ", " + src));
				}
				for (int i = 0; i < name.Length; i++)
				{
					Option op = options.FirstOrDefault(o => o.Name == name[i]);
					if (op == null)//オプションが指定されていなければNaNで埋める
					{
						args.AddRange(Enumerable.Repeat(NanExpression, num[i]));
					}
					else
					{
						IEnumerable<Expression> addition = op.Args.ToArray();
						if (op.Args.Count() < num[i])//不足はNaNで埋める
						{
							addition = addition.Concat(Enumerable.Repeat(NanExpression, num[i] - addition.Count()));
						}
						args.AddRange(addition.Take(num[i]));
					}
				}
			}
			var method = MethodDict[id];
			var param = method.GetParameters();
			if (param.Length != args.Count)
			{
				throw new ParseException("外部メソッド呼び出しで引数とパラメータの数の不一致\n" + method.ToString() + String.Format(" need {0} params but {1} args.", param.Length, args.Count));
			}
			for (int i = 0; i < param.Length; i++)
			{
				if (!param[i].ParameterType.IsAssignableFrom(args[i].Type))
				{
					args[i] = Expression.Convert(args[i], param[i].ParameterType);
				}
			}


			return Expression.Call(Expression.Convert(Expression.Field(Environment, ScriptEngine.Environment.Info_TargetObject), TargetType), method, args);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="user">ブロックを必要としているLine</param>
		/// <returns></returns>
		Expression GetBlock(Line user)
		{
			return InnerGetBlock(new List<Expression>(), user);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="user">whileなどのExpression</param>
		/// <param name="test">ループの終了条件</param>
		/// <param name="label"></param>
		/// <returns></returns>
		Expression GetBlockWithBreak(Line user, Expression test, LabelTarget label)
		{
			var list = new List<Expression>();
			list.Add(Expression.IfThen(test, Expression.Break(label)));
			return InnerGetBlock(list, user);
		}

		Expression InnerGetBlock(List<Expression> list, Line user)
		{
			if (Lines[user.Index + 1].Level != user.Level + 1)
			{
				throw new ParseException("ブロックの書き方が不正", user);
			}
			for (int i = user.Index + 1; i < Lines.Length; i++)
			{
				if (Lines[i].Level <= user.Level) break;
				if (Lines[i].Level == user.Level + 1)
				{
					var e = ProcessStatement(Lines[i]);
					if (e != null) list.Add(e);
				}
			}
			return Expression.Block(list.ToArray());
		}


		#endregion



		/// <summary>
		/// 
		/// </summary>
		/// <param name="line">引数とオプションがくっついたトークン列</param>
		/// <returns></returns>
		Expression[] GetArgs(IEnumerable<object> line)
		{
			var ret = new List<Expression>();
			foreach (var item in line)
			{
				if (item is PareBlock)
				{
					ret.Add(ParsePareBlock((PareBlock)item));
				}
				else if (item is OptionBlock)
				{
				}
				else if (item is string)//変数か文字列?
				{
					ret.Add(ParseVariable((string)item));
				}
				else if (item is Value)//数値リテラル
				{
					ret.Add(Expression.Constant(item, typeof(float)));
				}
				else if (item is Marks)
				{
					throw new ParseException("引数リスト内に不正なMark " + Enum.GetName(typeof(Marks), item));
				}
				else
				{
					throw new ParseException("予期せぬエラー");
				}
			}
			return ret.ToArray();
		}

		class Option
		{
			public string Name;
			public Expression[] Args;

			public Option(string name, Expression[] arg)
			{
				Name = name;
				Args = arg;
			}

		}



		/// <summary>
		/// PareBlockをパースしてひとつの値のExpressionにする
		/// </summary>
		/// <param name="pare"></param>
		/// <returns></returns>
		Expression ParsePareBlock(PareBlock pare)
		{
			object[] l = pare.tokens;

			if (l[0] is string)
			{

				if (StaticMethodDict.ContainsKey((string)l[0]))
				{
					Expression[] args = GetArgs(l.Slice(1, l.Length - 1));
					return Expression.Call(StaticMethodDict[(string)l[0]], args);
				}
				if (MethodDict.ContainsKey((string)l[0]))//関数の時
				{
					return CallExternalMethod((string)l[0], l.Slice(1, l.Length - 1));
					//Expression[] args = GetArgs(l.Slice(1, l.Length - 1));
					//return Expression.Call(Expression.Field(Environment, ScriptEngineEx.Environment.Info_TargetObject), MethodDict[(string)l[0]], args);
				}
				//関数を実行するExpression

			}
			return ArithExpressionMaker.ParseArithExpression(pare, ParsePareBlock, ParseVariable);//多項式の時
		}





	}
}
