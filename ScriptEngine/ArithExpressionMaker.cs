using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Masa.Lib.XNA;
using Masa.Lib;
using Microsoft.Xna.Framework;
using System.Linq.Expressions;
using System.Reflection;

namespace Masa.ScriptEngine
{
	using Value = System.Single;
	using ParseFunctions = System.Tuple<Func<string, Expression>, Func<PareBlock, Expression>>;

	internal static class ArithExpressionMaker
	{
		static readonly Type ValueType = ExpressionTreeMaker.ValueType;
		static readonly Expression ZeroExpression = ExpressionTreeMaker.ZeroExpression;
		static readonly Expression OneExpression = ExpressionTreeMaker.OneExpression;

		static readonly Marks[] UnaryOperators = new[] { Marks.Pos, Marks.Neg, Marks.Not };

		/// <summary>
		/// 単項演算子を処理してExpressionにする
		/// </summary>
		/// <param name="tokens">ExpressionとMarkの混合物</param>
		/// <returns>単項演算子処理済みのExpressionと(二項演算子であるはずの)Markの混合物</returns>
		static object[] ProcessUnaryExpression(object[] tokens)
		{
			List<object> l = new List<object>();
			for (int i = 0; i < tokens.Length; i++)
			{
				if (tokens[i] is Marks)
				{
					Marks m = (Marks)tokens[i];
					if (UnaryOperators.Contains(m) && (i == 0 || tokens[i - 1] is Marks))//単項演算子であれば式の先頭、もしくはその前がExpressionでない
					{
						if (!(tokens[i + 1] is Expression))
						{
							throw new ParseException("単項演算子の直後が式でない");
						}
						Expression e = (Expression)tokens[i + 1];
						i++;//一つ後の項は処理済み
						switch (m)
						{
							case Marks.Pos:
								l.Add(Expression.UnaryPlus(e));
								continue;
							case Marks.Neg:
								l.Add(Expression.Negate(e));
								continue;
							case Marks.Not:
								l.Add(BoolToFloat(Expression.Equal(e, ZeroExpression)));//val == 0 => 1, val != 0 => 0
								continue;
						}
					}
				}
				l.Add(tokens[i]);
			}
			return l.ToArray();
		}

		/// <summary>
		/// 戻り値は全てfloat型
		/// </summary>
		/// <param name="m"></param>
		/// <param name="l"></param>
		/// <param name="r"></param>
		/// <returns></returns>
		static Expression MakeBinaryExpression(Marks m, Expression l, Expression r)
		{
			switch (m)
			{
				case Marks.Pos:
					return Expression.Add(l, r);
				case Marks.Neg:
					return Expression.Subtract(l, r);
				case Marks.Mul:
					return Expression.Multiply(l, r);
				case Marks.Div:
					return Expression.Divide(l, r);
				case Marks.Mod:
					return Expression.Modulo(l, r);
				case Marks.And:
					return BoolToFloat(Expression.AndAlso(FloatToBool(l), FloatToBool(r)));
				case Marks.Or:
					return BoolToFloat(Expression.OrElse(FloatToBool(l), FloatToBool(r)));
				case Marks.Equal:
					return BoolToFloat(Expression.Equal(l, r));
				case Marks.NotEqual:
					return BoolToFloat(Expression.NotEqual(l, r));
				case Marks.Big:
					return BoolToFloat(Expression.GreaterThan(l, r));
				case Marks.BigEqual:
					return BoolToFloat(Expression.GreaterThanOrEqual(l, r));
				case Marks.Small:
					return BoolToFloat(Expression.LessThan(l, r));
				case Marks.SmallEqual:
					return BoolToFloat(Expression.LessThanOrEqual(l, r));
			}
			throw new ParseException("ありえない");
		}

		static readonly Marks[][] OperatorPriorityList = new[]
		{
			new[]{ Marks.Mul, Marks.Div, Marks.Mod },
			new[]{ Marks.Pos, Marks.Neg },
			new[]{ Marks.Equal, Marks.NotEqual, Marks.Big, Marks.BigEqual, Marks.Small, Marks.SmallEqual },
			new[]{Marks.And, Marks.Or, },
		};

		public static Expression ParseArithExpression(PareBlock p, Func<PareBlock, Expression> parsePareBlock, Func<string, Expression> parseVariable)
		{
			//多項式構築
			List<object>[] list = new List<object>[2];
			int ind = 0;
			list[0] = new List<object>(ProcessToExpressionAndMark(p, parsePareBlock, parseVariable));
			list[1] = new List<object>();
			for (int i = 0; i < OperatorPriorityList.Length; i++)
			{
				for (int j = 0; j < list[ind].Count; j++)
				{
					if (list[ind][j] is Marks && Array.Exists(OperatorPriorityList[i], (mk) => (Marks)list[ind][j] == mk))
					{
						Expression e = MakeBinaryExpression((Marks)list[ind][j], (Expression)list[1 - ind][list[1 - ind].Count - 1], (Expression)list[ind][j + 1]);
						list[1 - ind].RemoveAt(list[1 - ind].Count - 1);//1つ前の項は使用済み
						j++;
						list[1 - ind].Add(e);
					}
					else
					{
						list[1 - ind].Add(list[ind][j]);
					}
				}
				//リストをスワップ
				list[ind].Clear();
				ind = 1 - ind;
			}

			return (Expression)list[ind][0];
		}


		static Expression FloatToBool(Expression val)
		{
			return Expression.NotEqual(val, ZeroExpression);
		}

		static Expression BoolToFloat(Expression val)
		{
			return Expression.Condition(val, OneExpression, ZeroExpression);
		}


		/// <summary>
		/// 括弧やOptionを分離済みのトークンをExpressionと2項演算子Markの塊に変換
		/// </summary>
		/// <param name="p"></param>
		/// <returns></returns>
		static object[] ProcessToExpressionAndMark(PareBlock p, Func<PareBlock, Expression> parsePareBlock, Func<string, Expression> parseVariable)
		{
			object[] l = p.tokens;
			var tmp = new List<object>();
			for (int i = 0; i < l.Length; i++)
			{
				if (l[i] is Value)
				{
					tmp.Add(Expression.Constant(l[i], ValueType));
				}
				else if (l[i] is Marks)
				{
					tmp.Add(l[i]);
				}
				else if (l[i] is PareBlock)
				{
					tmp.Add(parsePareBlock((PareBlock)l[i]));
				}
				else if (l[i] is string)
				{
					tmp.Add(parseVariable((string)l[i]));
				}
				else//OptionBlockなど?
				{
					throw new ParseException("予期せぬトークンが多項式構築中に出現");
				}
			}
			return ProcessUnaryExpression(tmp.ToArray());
		}
	}
}
