using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Masa.Lib;
using System.Linq.Expressions;
using System.Reflection;
using MoreLinq;

namespace Masa.ScriptEngine
{
	
	internal static class ExpressionTreeMakerHelper
	{
		public static Dictionary<string, FieldInfo> GetEnvironmentFieldInfo()
		{
			var ret = new Dictionary<string, FieldInfo>();
			foreach (var item in typeof(Environment).GetFields())
			{
				var atr = item.GetCustomAttributes(typeof(ScriptMemberAttribute), true);
				foreach (ScriptMemberAttribute a in atr)
				{
					ret.Add(a.Name, item);
				}
			}
			return ret;
		}

		public static Dictionary<string, PropertyInfo> GetEnvironmentPropertyInfo()
		{
			var ret = new Dictionary<string, PropertyInfo>();
			foreach (var item in typeof(Environment).GetProperties())
			{
				var atr = item.GetCustomAttributes(typeof(ScriptMemberAttribute), true);
				foreach (ScriptMemberAttribute a in atr)
				{
					ret.Add(a.Name, item);
				}
			}
			return ret;
		}

		
		internal static Expression ExpressionToBool(Expression value)
		{
			if (value.Type == typeof(bool))
			{
				return value;
			}
			var validType = new[]
			{
				typeof(int),
				typeof(float),
				typeof(double),
				typeof(short),
			}
			.FirstOrDefault(t => value.Type == t);
			if (validType == null)
			{
				throw new ParseException("boolに変換不能な値が渡された" + value);
			}
			var zero = Expression.Convert(Expression.Constant(0), validType);
			return Expression.NotEqual(value, zero);
		}

	}
}
