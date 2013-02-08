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
	internal static class ArithExpressionMaker
	{
		static readonly Marks[] UnaryOperators = new[] { Marks.Pos, Marks.Neg, Marks.Not };

		/// <summary>
		/// 単項演算子を処理してExpressionにする
		/// </summary>
		/// <param name="tokens">ExpressionとMarkの混合物</param>
		/// <returns>単項演算子処理済みのExpressionと(二項演算子であるはずの)Markの混合物</returns>
		object[] ProcessUnaryExpression(object[] tokens)
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
		Expression MakeBinaryExpression(Marks m, Expression l, Expression r)
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

	}
}
