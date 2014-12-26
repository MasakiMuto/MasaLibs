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
			ReflectionCashe[target] = new ClassReflectionInfo(target);
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

		public static Dictionary<Type, System.Xml.Linq.XElement> OutputClassesXml()
		{
			return ReflectionCashe.ToDictionary(x=>x.Key ,x => DocumentCreater.ClassToXml(x.Key, x.Value));
		}

		public static System.Xml.Linq.XElement OutputGlobalXml()
		{
			return DocumentCreater.GlobalsToXml(StaticMethodDict.ToDictionary(x=>x.Key, x=>x.Value.MethodInfo));
		}

		public static System.Xml.Linq.XElement OutputIndex()
		{
			return DocumentCreater.CreateIndex(ReflectionCashe.Keys);
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

		/// <summary>
		/// 存在するlabelを列挙
		/// </summary>
		/// <returns></returns>
		public IEnumerable<string> EnumrateLabels()
		{
			return LabelDict.Keys;
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
		/// scriptedのプロパティorフィールド、non-scriptedのプロパティを取得。なければnull
		/// </summary>
		/// <param name="id"></param>
		/// <param name="left"></param>
		/// <returns></returns>
		MemberExpression TryGetPropertyOrField(string id, Expression left)
		{
			var info = GetObjectInfo(left.Type);
			ScriptPropertyInfo prp;
			if (info.PropertyDict.TryGetValue(id, out prp))//外部プロパティ
			{
				return Expression.Property(left, prp.PropertyInfo);
			}
			FieldInfo fld;
			if (info.FieldDict.TryGetValue(id, out fld))
			{
				return Expression.Field(left, fld);
			}
			var flags = BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
			var p = left.Type.GetProperty(id, flags);
			if (p != null)
			{
				return Expression.Property(left, p);
			}
			return null;
		}

		/// <summary>
		/// 文字列を変数(内部変数、Global変数、外部変数、列挙文字列すべて)としてパース。
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		Expression ParseIdentifier(string id)
		{
			if (id[0] == Scanner.StringLiteralMark)//""文字列の場合
			{
				return ProcessStringLiteral(id.Substring(1));//末尾のマークはスキャナで処理済み
			}

			ParameterExpression prm;
			if (VarDict.TryGetValue(id, out prm))//スクリプトローカル引数
			{
				return prm;
			}

			var ret = TryGetPropertyOrField(id, GetThis());
			if (ret != null)
			{
				return ret;
			}

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

			return ProcessStringLiteral(id);
		}

		Expression ProcessStringLiteral(string id)
		{
			return Expression.Constant(id, typeof(string));
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
		/// <summary>
		/// 行単位の処理ルート
		/// </summary>
		/// <param name="line"></param>
		/// <returns></returns>
		Expression ProcessStatement(Line line)
		{
			try
			{
				var id = line.Tokens[0] as string;
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

				var mark = IsAssignLine(line);
				if (mark != Marks.No)
				{
					return ProcessAssign(line, mark);
				}
				//line[1]がMarkでない && var系でない
				return ProcessNormalStatement(line);
			}
			catch(Exception e)
			{
				throw new ParseException("Statement Error", line, e);
			}
		}

		/// <summary>
		/// 一つの式(返り値なし含む)を表すトークン列を受ける
		/// </summary>
		/// <param name="tokens"></param>
		/// <returns></returns>
		Expression ProcessExpression(IEnumerable<object> tokens)
		{
			object[] objs = tokens.ToArray();
			if (objs[0] is string)
			{
				var id = objs[0] as string;
				if (StaticMethodDict.ContainsKey(id))
				{
					return CallExternalMethodInner(StaticMethodDict[id], tokens.Skip(1).ToArray(), null);
				}
				//if (ClassInfo.MethodDict.ContainsKey(id))//ターゲットオブジェクト関数の時
				//{
				//	return CallExternalMethod(id, tokens.Skip(1).ToArray());
				//}
				var res = TryCallInstanceMethod(id, tokens.Skip(1), GetThis());
				if(res != null)
				{
					return res;
				}
			}

			return ArithExpressionMaker.ParseArithExpression(objs, ProcessSingleToken);//多項式の時
		}

		/// <summary>
		/// scripted, non-scriptedのインスタンスメソッド呼び出し
		/// </summary>
		/// <param name="id"></param>
		/// <param name="tokens"></param>
		/// <param name="caller"></param>
		/// <returns></returns>
		Expression TryCallInstanceMethod(string id, IEnumerable<object> tokens, Expression caller)
		{
			var info = GetObjectInfo(caller.Type);
			ScriptMethodInfo m;
			if (info.MethodDict.TryGetValue(id, out m))
			{
				return CallExternalMethodInner(m, (tokens == null ? new object[0] : tokens.ToArray()), caller);
			}
			return CallNonScriptMethod(caller, id, tokens);
			
		}

		/// <summary>
		/// PareblockやDotBlock,数値や文字列トークン
		/// </summary>
		/// <param name="token"></param>
		/// <returns></returns>
		Expression ProcessSingleToken(object token)
		{
			if (token is PareBlock)
			{
				return ProcessExpression((token as PareBlock).tokens);
			}
			else if (token is DotBlock)
			{
				return ProcessDotBlock(token as DotBlock);
			}
			else if(token is string)
			{
				return ParseIdentifier(token as string);
			}
			else if (token is Value)
			{
				return Expression.Constant((Value)token, ValueType);
			}
			throw new ParseException("不明なトークン:" + token.ToString());
		}

		Expression ProcessDotBlock(DotBlock dot)
		{
			Type leftType;
			Expression left;
			ClassReflectionInfo info;
			var id = dot.Left as string;
			if (id != null && TypeNameDictionary.ContainsKey(id))//静的メンバアクセス
			{
				left = null;
				leftType = TypeNameDictionary[id];
			}
			else
			{
				left = ProcessSingleToken(dot.Left);
				leftType = left.Type;
			}
			info = GetObjectInfo(leftType);
			string op;
			object[] args;
			if (dot.Right is string)
			{
				op = dot.Right as string;
				args = null;
			}
			else if (dot.Right is PareBlock)
			{
				var pare = dot.Right as PareBlock;
				op = pare.tokens[0] as string;
				args = pare.tokens.Skip(1).ToArray();
			}
			else
			{
				throw new ParseException("ドット演算子の右に不正なトークン:" + dot.ToString());
			}
			if (left != null)
			{
				var pf = TryGetPropertyOrField(op, left);
				if (pf != null)
				{
					return pf;
				}
				
				var res = TryCallInstanceMethod(op, args, left);
				if (res != null)
				{
					return res;
				}
			}
			else
			{
				if (info.StaticMethodDict.ContainsKey(op))
				{
					return CallExternalMethodInner(info.StaticMethodDict[op], args, null);
				}
				else if (op == "new")
				{
					return CallConstructor(leftType, args);
				}
				else
				{
					var res = CallStaticNonScriptMethod(leftType, op, args);
					if (res != null)
					{
						return res;
					}
				}
			}
		
			throw new ParseException("ドット演算子右辺の識別子が不明:" + dot.ToString());
		}

		/// <summary>
		/// その行が代入文の行か
		/// </summary>
		/// <param name="line"></param>
		/// <returns></returns>
		Marks IsAssignLine(Line line)
		{
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
			return line.Tokens.OfType<Marks>().Intersect(assignMarks).SingleOrDefault();
		}

		/// <summary>
		/// ローカル変数の定義
		/// </summary>
		/// <param name="line"></param>
		/// <returns></returns>
		Expression DefineVariable(Line line)
		{
			//ParameterExpression v;
			string id = (string)line.Tokens[1];
			Type type = ValueType;
			
			//Expression target = null;

			if (line.Tokens.Length == 2)//var hoge
			{
				TypeUndefinedVarList.Add(id);
				return null;
			}
			else if (line.Tokens.Length == 3)
			{
				throw new ParseException("ローカル変数宣言行のトークン数が不正", line);
			}
			else
			{
				if (line.Tokens[2].Equals(Marks.Dollar))//var hoge $ foo
				{
					var typeName = line.Tokens[3] as string;
					if (!TypeNameDictionary.TryGetValue(typeName, out type))
					{
						throw new ParseException("ローカル変数宣言の型注釈の型名が不正:" + typeName, line);
					}
					VarDict.Add(id, Expression.Parameter(type, id));
				}
				else//型注釈なし
				{
					TypeUndefinedVarList.Add(id);
				}
				int rightPos = Array.FindIndex(line.Tokens, x => x.Equals(Marks.Sub));
				if (rightPos == -1)
				{
					return null;//初期化なし
				}
				return ProcessAssign(Marks.Sub, new[] { id }, line.Tokens.Skip(rightPos + 1).ToArray());
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="mark"></param>
		/// <param name="left">単純なidかDotBlockかPareblock</param>
		/// <param name="right"></param>
		/// <returns></returns>
		Expression ProcessAssign(Marks mark, object[] left, object[] right)
		{
			Expression value;
			Type leftType = null;
			Expression target;
			if (mark == Marks.Dec || mark == Marks.Inc)
			{
				Debug.Assert(!right.Any());
				leftType = ValueType;//インクリメント・デクリメントで初期化ならValue型とする
				value = null;
			}
			else
			{
				value = ProcessExpression(right);
				leftType = value.Type;
			}

			if (left.Length == 1 && left[0] is string)
			{
				var id = left[0] as string;
				ResolveTypeUndefinedVariable(id, leftType);
				target = ParseIdentifier(id);
			}
			else
			{
				target = ProcessExpression(left);
			}
			if (mark == Marks.Inc)
			{
				return Expression.PostIncrementAssign(target);
			}
			else if (mark == Marks.Dec)
			{
				return Expression.PostDecrementAssign(target);
			}
			else return Assign(mark, target, value);
		}

		Expression ProcessAssign(Line line, Marks mark)
		{
			var left = line.Tokens.TakeWhile(x => !x.Equals(mark)).ToArray();
			var right = line.Tokens.Skip(left.Length + 1);
			return ProcessAssign(mark, left, right.ToArray());
		}

		void ResolveTypeUndefinedVariable(string name, Type type)
		{
			if (TypeUndefinedVarList.Contains(name))
			{
				VarDict.Add(name, Expression.Parameter(type, name));
				TypeUndefinedVarList.Remove(name);
			}
		}

		/// <summary>
		/// 代入Expressionを作成する
		/// </summary>
		/// <param name="mark"></param>
		/// <param name="target"></param>
		/// <param name="value"></param>
		/// <returns></returns>
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
		/// 代入文でも宣言文でもない文
		/// if ループ stateなどの制御文や、fireなどの外部命令を処理
		/// 一般文、つまり代入文や宣言文以外の、「識別子 引数リスト・・・」となる文
		/// </summary>
		/// <param name="line">PareBlockやOptionBlockに整形済みのトークン行</param>
		/// <returns></returns>
		Expression ProcessNormalStatement(Line line)
		{
			Func<Expression> exp = () => ProcessExpression(line.Tokens.Skip(1));
			switch (line.Tokens[0] as string)
			{
				case "if":
					return MakeIfStatement(line, exp());
				case "else"://if - else の流れで処理するため単体ではスルー
					return null;
				case "while":
					goto case "repeat";
				case "repeat":
					LabelTarget label = Expression.Label(line.Number.ToString());
					var pred = Expression.Not(ExpressionTreeMakerHelper.ExpressionToBool(exp()));
					return Expression.Loop(GetBlockWithBreak(line, pred, label), label);
				case "loop":
					return MakeLoopStatement(line);
				case "goto"://state change
					var assign = Expression.Assign(Expression.Property(Environment, ScriptEngine.Environment.Info_State), exp());
					return assign;
					//return Expression.Block(assign, Expression.Return(ExitLabel));
				case "state":
					return Expression.IfThen(Expression.Equal(Expression.Property(Environment, ScriptEngine.Environment.Info_State), exp()), GetBlock(line));
				case "label":
					LabelDict.Add((string)line.Tokens[1], GetBlock(line));
					return null;
				case "jump":
					var s = line.Tokens[1] as string;
					if (LabelDict.ContainsKey(s))
					{
						return LabelDict[s];
					}
					else
					{
						throw new ParseException("未定義のラベル " + s, line);
					}
				case "blank":
					return Expression.Empty();
				default:
					return ProcessExpression(line.Tokens);
			}
		}

		Expression MakeIfStatement(Line line, Expression pred)
		{
			pred = ExpressionTreeMakerHelper.ExpressionToBool(pred);
			var index = line.Index;
			var ifBlock = GetBlock(line);

			if (Lines.Length > index + 2)
			{
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
			var arg = GetArgs(line.Tokens.Skip(1));
			Expression times = arg[0];
			Expression freq = arg[1];
			Expression from = arg[2];
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
			firstSentence = Expression.AndAlso(
				Expression.GreaterThanOrEqual(frame, from),
				Expression.OrElse(
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
				return Expression.IfThen(
					Expression.AndAlso(
						firstSentence,
						Expression.OrElse(
							Expression.LessThan(last, from),
							Expression.GreaterThan(div(frame), div(last))
						)
					),
					GetBlock(line)
				);
			}
		}

		/// <summary>
		/// 実行中インスタンスの取得
		/// </summary>
		/// <returns></returns>
		Expression GetThis()
		{
			return Expression.Convert(Expression.Field(Environment, ScriptEngine.Environment.Info_TargetObject), TargetType);
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
			return CallExternalMethodInner(ClassInfo.MethodDict[id], l, GetThis());
		
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
							return Expression.Constant(Activator.CreateInstance(param[x].ParameterType), param[x].ParameterType);
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

		Expression CallNonScriptMethod(Expression obj, string name, IEnumerable<object> tokens)
		{
			return CallNonScriptMethodInner(obj, obj.Type, name, tokens);
		}

		Expression CallStaticNonScriptMethod(Type type, string name, IEnumerable<object> tokens)
		{
			return CallNonScriptMethodInner(null, type, name, tokens);
		}

		Expression CallNonScriptMethodInner(Expression obj, Type type, string name, IEnumerable<object> tokens)
		{
			if (!type.GetMethods().Where(x => x.Name == name).Any())
			{
				return null;
			}
			var args = GetArgs(tokens);
			var method = type.GetMethod(name, args.Select(x => x.Type).ToArray());
			if (method == null)
			{
				return null;
			}
			if (obj != null && !method.IsStatic)
			{
				return Expression.Call(obj, method, args);
			}
			else
			{
				return Expression.Call(method, args);
			}
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
				if (item is OptionBlock)
				{
				}
				else
				{
					ret.Add(ProcessSingleToken(item));
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

		/// <summary>
		/// 型のリフレクション情報を返す
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
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
