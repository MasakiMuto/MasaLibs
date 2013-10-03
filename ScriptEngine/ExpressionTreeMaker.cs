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
	using System.Diagnostics;
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
		List<string> TypeUndefinedVarList;
		Dictionary<string, ParameterExpression> VarDict;
		Dictionary<string, Expression> LabelDict;
		ClassReflectionInfo ClassInfo;
		ParameterExpression Environment;
		string[] NameValueTable;
		Line[] Lines;
		
		public readonly Type TargetType;

		static readonly LabelTarget ExitLabel = Expression.Label("_ScriptExit");
		internal static readonly Type ValueType = typeof(Value);
		internal static readonly Expression ZeroExpression = Expression.Constant(0f, ValueType);
		internal static readonly Expression OneExpression = Expression.Constant(1f, ValueType);
		static readonly Expression NanExpression = Expression.Constant(Value.NaN, ValueType);
		//static readonly LabelTarget LOOPEND = Expression.Label("EndLoop");
		//static readonly Dictionary<string, FieldInfo> EnvironmentField = ExpressionTreeMakerHelper.GetEnvironmentFieldInfo();
		static readonly Dictionary<string, PropertyInfo> EnvironmentProperty = ExpressionTreeMakerHelper.GetEnvironmentPropertyInfo();
		static readonly Dictionary<string, ScriptMethodInfo> StaticMethodDict = GlobalFunctionProvider.GetStaticMethodInfo();
		static readonly Dictionary<Type, ClassReflectionInfo> ReflectionCashe = GlobalFunctionProvider.GetLibraryClassScriptInfo();
		static readonly Dictionary<string, Expression> ConstantValueDict = GlobalFunctionProvider.GetConstantValueDictionary();
		static readonly Dictionary<string, Type> TypeNameDictionary = GlobalFunctionProvider.GetTypeNameDictionary();//組み込み型のスクリプト内名称

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
			TypeUndefinedVarList = new List<string>();
			//MethodDict = new Dictionary<string, ScriptMethodInfo>();
			//PropertyDict = new Dictionary<string, PropertyInfo>();
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
			var atr = Attribute.GetCustomAttribute(target, typeof(ScriptTypeAttribute)) as ScriptTypeAttribute;
			if (atr != null)
			{
				TypeNameDictionary[atr.Name] = target;
			}
		}

		void GetTargetInfo()
		{
			if (!ReflectionCashe.ContainsKey(TargetType))
			{
				MakeTargetInfoCache(TargetType);
			}
			ClassInfo = ReflectionCashe[TargetType];
			
		}

		public string OutputClassInformation()
		{
			return DocumentCreater.OutputClass(ClassInfo);
		}

		public System.Xml.Linq.XElement OutputClassXml()
		{
			return DocumentCreater.ClassToXml(TargetType, ClassInfo);
		}

		public static System.Xml.Linq.XElement OutputGlobalXml()
		{
			return DocumentCreater.GlobalsToXml(StaticMethodDict.ToDictionary(x=>x.Key, x=>x.Value.MethodInfo));
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
					LabelStatementCashe[label] = Expression.Lambda<Action<ScriptEngine.Environment>>(
						Expression.Block(VarDict.Values, LabelDict[label], Expression.Label(ExitLabel)), Environment).Compile();
				}
				else
				{
					return null;
				}
			}
			return LabelStatementCashe[label];
		}

		#region Compiler


		///// <summary>
		///// スクリプト全体をコンパイルする
		///// </summary>
		///// <param name="mtd">出力先</param>
		//public void Compile(System.Reflection.Emit.MethodBuilder mtd)
		//{
		//	var lambda = Expression.Lambda<Action<ScriptEngine.Environment>>(Expression.Block(VarDict.Values, TotalBlock), Environment);
		//	lambda.CompileToMethod(mtd);
		//}

		//public bool CompileLabel(string label, System.Reflection.Emit.MethodBuilder mtd)
		//{
		//	if (!LabelDict.ContainsKey(label)) return false;
		//	var lambda = Expression.Lambda<Action<ScriptEngine.Environment>>(Expression.Block(VarDict.Values, LabelDict[label]), Environment);
		//	lambda.CompileToMethod(mtd);
		//	return true;
		//}

		//public Type CompileToClass(Type original, string className, System.Reflection.Emit.ModuleBuilder mb)
		//{
		//	TypeBuilder tp = mb.DefineType(className, TypeAttributes.Public, original);

		//	Func<string, MethodBuilder> define = (n) => tp.DefineMethod(n, MethodAttributes.Family | MethodAttributes.HideBySig | MethodAttributes.ReuseSlot | MethodAttributes.Virtual, null, new[] { typeof(Environment) });
		//	Compile(define("ScriptMain"));
		//	//foreach (var item in original.GetMethods().Where(m=>m.GetCustomAttributes(typeof(ScriptDefinedMethodAttribute), true).Count() > 0))
		//	//{
		//	//    if (LabelExist(item.Name))
		//	//    {
		//	//        CompileLabel(item.Name, define(item.Name));
		//	//    }
		//	//}			
		//	foreach (var item in LabelDict)
		//	{
		//		CompileLabel(item.Key, define("Script" + item.Key));
		//	}
		//	var getter = tp.DefineMethod("get_GlobalVarNumber", MethodAttributes.Family | MethodAttributes.SpecialName | MethodAttributes.HideBySig);
		//	Expression.Lambda<Func<int>>(Expression.Constant(GlobalVarNumber)).CompileToMethod(getter);
		//	tp.DefineProperty("GlobalVarNumber", PropertyAttributes.None, typeof(int), null).SetGetMethod(getter);
		//	//tp.DefineField("GlobalVarNumber", typeof(int), FieldAttributes.Private | FieldAttributes.Literal).SetConstant(GlobalVarNumber);
		//	return tp.CreateType();
		//}

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
			if (VarDict.TryGetValue(id, out prm))//スクリプトローカル引数
			{
				return prm;
			}

			PropertyInfo prp;
			if (ClassInfo.PropertyDict.TryGetValue(id, out prp))//外部プロパティ
			{
				return Expression.Property(Expression.Convert(Expression.Field(Environment, ScriptEngine.Environment.Info_TargetObject), TargetType), prp);
			}
			FieldInfo fld;
			if(ClassInfo.FieldDict.TryGetValue(id, out fld))
			{
				return Expression.Field(Expression.Convert(Expression.Field(Environment, ScriptEngine.Environment.Info_TargetObject), TargetType), fld);
			}

			//if (EnvironmentField.ContainsKey(id))
			//{
			//	return Expression.Field(Environment, EnvironmentField[id]);
			//}
			if (EnvironmentProperty.ContainsKey(id))
			{
				return Expression.Property(Environment, EnvironmentProperty[id]);
			}
			
			if (ConstantValueDict.ContainsKey(id))//スクリプト側定数
			{
				return ConstantValueDict[id];
			}
			
			if (NameValueTable != null && NameValueTable.Contains(id))
			{
				return MakeConstantExpression(Array.FindIndex(NameValueTable, s => s == id));
			}
			
			int gvar = GlobalVarList.FindIndex(k => k == id);
			if (gvar != -1)//スクリプトglobal変数
			{
				return Expression.Property(Environment, ScriptEngine.Environment.Info_Item, Expression.Constant(gvar, typeof(int)));
			}

			if (ClassInfo.MethodDict.Any(p => p.Value.DefaultParameterCount == 0 && p.Key == id))//必須引数が0個の関数を呼び出す
			{
				return CallExternalMethod(id, null);
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
			if (line == null)
			{
				return new Option[0];
			}
			return line.OfType<OptionBlock>().Select((o) => ParseOptionBlock(o)).ToArray();
		}

		Option ParseOptionBlock(OptionBlock opt)
		{
			return new Option(opt.Name, GetArgs(opt.Tokens).ToArray());
		}

		#endregion

		#region 式の生成
		Expression ProcessStatement(Line line)
		{
			//line.Tokens = ParseLine(line.Tokens);//括弧やオプションをまとめる
			//string id = (string)line.Tokens[0];
			var id = line.Tokens[0] as string;
			if (id == null)//pare
			{
				line.Tokens[0] = ParsePareBlock((PareBlock)line.Tokens[0]);
			}
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

			var assignMarks = new[]
			{
				Marks.Sub,
				Marks.SubNeg,
				Marks.SubPos,
				Marks.SubMul,
				Marks.SubDiv,
				Marks.Inc,
				Marks.Dec
			};
			var mark = line.Tokens.OfType<Marks>().Intersect(assignMarks).SingleOrDefault();
			if (mark != Marks.No)//代入系演算子が行に含まれている
			{
				return ProcessAssign(line, mark);
			}

			

			//if (line.Tokens.Length > 1 && line.Tokens[1] is Marks)
			//{
			//	Marks m = (Marks)line.Tokens[1];
			//	if (m == Marks.Sub || m == Marks.SubNeg || m == Marks.SubPos || m == Marks.SubMul || m == Marks.SubDiv || m == Marks.Inc || m == Marks.Dec)
			//	{
			//		return ProcessAssign(line);
			//	}
			//	else if (m == Marks.Dot)
			//	{
			//		return ParseDotAccess(line.Tokens);
			//	}
			//	else//line[1]がMarkかつ代入系でない→ありえない
			//	{
			//		//throw new Exception("トークンの2番目が不正なマーク Line" + line.Number + ":" + m.ToString());
			//		throw new ParseException("トークンの2番目が不正なマーク", line);
			//	}
			//}
			//line[1]がMarkでない && var系でない
			return ProcessNormalStatement(line);

		}

		Expression DefineVariable(Line line)
		{
			ParameterExpression v;
			string name = (string)line.Tokens[1];
			Type type = typeof(Value);
			
			Expression target = null;
			if (line.Tokens.Length >= 4)// var hoge = 1
			{
				int rightPos = Array.FindIndex(line.Tokens, x => x.Equals(Marks.Sub));
				if (rightPos != -1)
				{
					target = ParsePareBlock(new PareBlock(line.Tokens.Skip(rightPos + 1).ToArray()));
					type = target.Type;//右辺値からの型推論
				}
			}
			if (line.Tokens.Length >= 4 && line.Tokens[2].Equals(Marks.Dollar))// var hoge $ float2
			{
				type = TypeNameDictionary[line.Tokens[3] as string];
				if (line.Tokens.Contains(Marks.Sub))//型明示かつ初期化あり
				{
					target = ParsePareBlock(new PareBlock(line.Tokens.SkipWhile(x => !x.Equals(Marks.Sub)).Skip(1).ToArray()));
					target = Expression.Convert(target, type);//強制キャストを試みる
				}
			}
			else
			{
				if (target == null)//型の明示なし、初期化なし
				{
					TypeUndefinedVarList.Add(name);
					return null;
				}
			}
			
			v = Expression.Parameter(type, name);
			VarDict.Add(name, v);
			
			if (target != null)
			{
				return Expression.Assign(v, target);
			}
			else
			{
				return null;
			}
			
		}

		Expression ProcessAssign(Line line, Marks mark)
		{
			var left = line.Tokens.TakeWhile(x => !x.Equals(mark)).ToArray();
			var right = line.Tokens.Skip(left.Length + 1);
			Expression target, value = null;
			if (right.Any())
			{
				value = ParsePareBlock(new PareBlock(right.ToArray()));
			}
			if (left.Length == 1)
			{
				if (left[0] is Expression)
				{
					target = left[0] as Expression;
				}
				else
				{
					var name = left[0] as string;
					Debug.Assert(name != null);
					if (value != null)
					{
						ResolveTypeUndefinedVariable(name, value);
					}
					target = ParseVariable(name);
				}
			}
			else
			{
				target = ParsePareBlock(new PareBlock(left));
			}
			if (mark == Marks.Inc || mark == Marks.Dec)
			{
				Debug.Assert(value == null);
				if (mark == Marks.Inc)
				{
					return Expression.PostIncrementAssign(target);
				}
				else
				{
					return Expression.PostDecrementAssign(target);
				}
			}
			else
			{
				return Assign(mark, target, value);
			}
		}

		void ResolveTypeUndefinedVariable(string name, Expression value)
		{
			if (TypeUndefinedVarList.Contains(name))
			{
				VarDict.Add(name, Expression.Parameter(value.Type, name));
				TypeUndefinedVarList.Remove(name);
			}
		}

		/// <summary>
		/// 代入処理
		/// </summary>
		/// <param name="line"></param>
		/// <returns></returns>
		//Expression ProcessAssign(Line line)
		//{
		//	Marks m = (Marks)line.Tokens[1];
		//	var name = line.Tokens[0] as string;
		//	if (m == Marks.Inc)
		//	{
		//		//return Expression.Assign(target, Expression.Increment(target));
		//		return Expression.PostIncrementAssign(ParseVariable(name));
		//	}
		//	else if (m == Marks.Dec)
		//	{
		//		//return Expression.Assign(target, Expression.Decrement(target));
		//		return Expression.PostDecrementAssign(ParseVariable(name));
		//	}
		//	else
		//	{
		//		Expression r = ParsePareBlock(new PareBlock(line.Tokens.Skip(2).ToArray()));//代入される値
		//		if (TypeUndefinedVarList.Contains(name))
		//		{
		//			VarDict.Add(name, Expression.Parameter(r.Type, name));
		//			TypeUndefinedVarList.Remove(name);
		//		}
		//		Expression target = ParseVariable(name);//代入先の変数/プロパティ
			
				
		//		return Assign(m, target, r);

		//	}
		//	//throw new Exception("Line " + line.Number + "がおかしい");
		//	throw new ParseException("おかしい", line);


		//}

		Expression Assign(Marks mark, Expression target, Expression value)
		{
			if (!target.Type.IsAssignableFrom(value.Type))
			{
				if(target.Type == typeof(bool))
				{
					value = ExpressionTreeMakerHelper.ExpressionToBool(value);
				}
				value = Expression.Convert(value, target.Type);
			}
			switch (mark)
			{
				case Marks.Sub:
					return Expression.Assign(target, value);
				case Marks.SubPos:
					return Expression.AddAssign(target, value);
				case Marks.SubNeg:
					return Expression.SubtractAssign(target, value);
				case Marks.SubMul:
					return Expression.MultiplyAssign(target, value);
				case Marks.SubDiv:
					return Expression.DivideAssign(target, value);
			
			}
			throw new Exception();
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
			string id = line.Tokens[0] as string;
			Func<Expression[]> args = () => GetArgs(line.Tokens.Skip(1)).ToArray();
			switch (id)
			{
				case "if":
					return MakeIfStatement(line, args()[0]);
				case "else"://if - else の流れで処理するため単体ではスルー
					return null;
				case "while":
					goto case "repeat";
				case "repeat":
					LabelTarget label = Expression.Label(line.Number.ToString());
					var pred = Expression.Not( ExpressionTreeMakerHelper.ExpressionToBool(args()[0]));
					//pred = Expression.Equal(args()[0], ZeroExpression);
					return Expression.Loop(GetBlockWithBreak(line, pred, label), label);
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
					if (id != null)
					{
						if (StaticMethodDict.ContainsKey(id))//オプション無しの前提
						{
							//return Expression.Call(StaticMethodDict[id], args());
							return CallExternalMethodInner(StaticMethodDict[id], line.Tokens.Skip(1).ToArray(), null);
						}
						if (ClassInfo.MethodDict.ContainsKey(id))
						{
							return CallExternalMethod(id, line.Tokens.Slice(1, line.Tokens.Length - 1));
						}
					}
					if (line.Tokens[1].Equals(Marks.Dot))
					{
						return ParseDotAccess(line.Tokens);
					}
					throw new ParseException(": 未定義のステートメント " + id, line);
			}
		}

		Expression MakeIfStatement(Line line, Expression pred)
		{
			pred = ExpressionTreeMakerHelper.ExpressionToBool(pred);
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
				frame = Expression.Property(Environment, ScriptEngine.Environment.Info_StateFrame);
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
			return CallExternalMethodInner(ClassInfo.MethodDict[id], l, Expression.Convert(Expression.Field(Environment, ScriptEngine.Environment.Info_TargetObject), TargetType));
		
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="method"></param>
		/// <param name="tokens">名前を除いたトークン列</param>
		/// <param name="caller">呼び出し元のインスタンス</param>
		/// <returns></returns>
		Expression CallExternalMethodInner(ScriptMethodInfo method, object[] tokens, Expression caller)
		{
			List<Expression> args = GetArgs(tokens);
			Option[] options = GetOptions(tokens);
			var attribute = method.Attribute;
			var param = method.MethodInfo.GetParameters();
			int index = 0;

			if (args.Count != method.DefaultParameterCount)
			{
				throw new ParseException("外部メソッド呼び出しで必須引数の数が不一致" + method.ToString() + String.Format(" need {0} params but {1} args.", method.DefaultParameterCount, args.Count));
			}
			index += args.Count;

			if (attribute != null && attribute.OptionName != null)//オプションが定義されていれば
			{
				string[] name = attribute.OptionName;
				int[] num = attribute.OptionArgNum;
				var less = options.Select(o => o.Name).Except(name);
				if (less.Any())
				{
					throw new ParseException(method.Name + "メソッド呼び出しに無効なオプション指定 : " + less.Aggregate((src, dst) => dst + ", " + src));
				}

				for (int i = 0; i < name.Length; i++)
				{
					Option op = options.FirstOrDefault(o => o.Name == name[i]);
					int argCount = num[i];
					if (op != null)
					{
						args.AddRange(op.Args.ToArray());
						index += op.Args.Count();
						argCount -= op.Args.Count();
					}
					if (argCount > 0)
					{
						//ValueならNaN, 引数のデフォルト値があればその値、参照型ならNull, それ以外ならValue型の0で埋める
						args.AddRange(Enumerable.Range(index, argCount).Select(x =>
						{
							if (param[x].ParameterType == ValueType) return NanExpression;
							if (param[x].DefaultValue != DBNull.Value) return Expression.Constant(param[x].DefaultValue);
							if (param[x].ParameterType.IsClass) return Expression.Constant(null);
							//return Expression.Constant(param[x].ParameterType.GetConstructor(Type.EmptyTypes).Invoke(null));
							return Expression.Constant(Activator.CreateInstance(param[x].ParameterType), param[x].ParameterType);
							//return ZeroExpression;
						}));
						index += argCount;
					}

				}
			}
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


			if (caller == null)
			{
				return Expression.Call(method.MethodInfo, args);
			}
			return Expression.Call(caller, method.MethodInfo, args);
		}

		Expression CallConstructor(Type type, IEnumerable<object> tokens)
		{
			List<Expression> args = GetArgs(tokens);
			var constructor = type.GetConstructor(args.Select(x => x.Type).ToArray());
			if (constructor == null)
			{
				throw new ParseException(type.ToString() + "型で引数に一致するコンストラクタが存在しない");
			}
			return Expression.New(constructor, args);
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
		List<Expression> GetArgs(IEnumerable<object> line)
		{
			
			var ret = new List<Expression>();
			if (line == null)
			{
				return ret;
			}
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
			return ret;
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
					//Expression[] args = GetArgs(l.Slice(1, l.Length - 1)).ToArray();
					//return Expression.Call(StaticMethodDict[(string)l[0]], args);
					return CallExternalMethodInner(StaticMethodDict[l[0] as string], pare.tokens.Skip(1).ToArray(), null);
				}
				if (ClassInfo.MethodDict.ContainsKey((string)l[0]))//関数の時
				{
					return CallExternalMethod((string)l[0], l.Slice(1, l.Length - 1));
					//Expression[] args = GetArgs(l.Slice(1, l.Length - 1));
					//return Expression.Call(Expression.Field(Environment, ScriptEngineEx.Environment.Info_TargetObject), MethodDict[(string)l[0]], args);
				}
				//関数を実行するExpression

			}
			if (l.Length >= 3 && l[1].Equals(Marks.Dot))
			{
				return ParseDotAccess(l);
			}
			return ArithExpressionMaker.ParseArithExpression(pare, ParsePareBlock, ParseVariable);//多項式の時
		}

		/// <summary>
		/// hoge.piyoの形式を処理
		/// </summary>
		/// <param name="tokens"></param>
		/// <returns></returns>
		Expression ParseDotAccess(object[] tokens)
		{
			var src = tokens[0];
			var call = tokens[2] as string;
			Expression obj;
			if (src is string)
			{
				Type t = ParseStringAsType(src as string);
				if (t != null)
				{
					if (call == "new")
					{
						return CallConstructor(t, tokens.Skip(3));
					}
					else
					{
						ScriptMethodInfo method;
						if (ClassInfo.StaticMethodDict.TryGetValue(call, out method))
						{
							CallExternalMethodInner(method, tokens.Skip(3).ToArray(), null);
						}
					}
					throw new ParseException("型名後の識別子が不正");

				}
			}
			if (src is PareBlock)
			{
				obj = ParsePareBlock((PareBlock)src);
			}
			else if (src is string)
			{
				obj = ParseVariable(src as string);
			}
			else if (src is Expression)
			{
				obj = src as Expression;
			}
			else
			{
				throw new ParseException("ドット前にあるトークンが不正");
			}
			var info = GetObjectInfo(obj.Type);
			if (call == null)
			{
				throw new ParseException("ドット後にあるトークンが不正");
			}
			
			if (info.PropertyDict.ContainsKey(call))
			{
				var prop = info.PropertyDict[call];
				return Expression.Property(obj, prop);
			}
			if (info.MethodDict.ContainsKey(call))
			{
				return CallExternalMethodInner(info.MethodDict[call], tokens.Skip(3).ToArray(), obj);
			}
			if (info.FieldDict.ContainsKey(call))
			{
				return Expression.Field(obj, info.FieldDict[call]);
			}

			throw new ParseException("ドット後にあるトークンが不正");
		}

		/// <summary>
		/// 文字列を形名に変換する。できなければnull
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		Type ParseStringAsType(string name)
		{
			Type t;
			if (TypeNameDictionary.TryGetValue(name, out t))
			{
				return t;
			}
			else
			{
				return null;
			}
		}

		ClassReflectionInfo GetObjectInfo(Type type)
		{
			ClassReflectionInfo info;
			if (!ReflectionCashe.TryGetValue(type, out info))
			{
				MakeTargetInfoCache(type);
				info = ReflectionCashe[type];
			}
			return info;
		}





	}
}
