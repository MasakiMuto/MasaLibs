using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Masa.Lib;
using System.Reflection;
using System.Linq.Expressions;

namespace Masa.ScriptEngine
{
	using Value = System.Single;

	public static class ScriptDefinitionHelper
	{
		/// <summary>
		/// tuple of (scriptName, MethodName, argments)
		/// </summary>
		/// <param name="type"></param>
		/// <param name="names"></param>
		/// <returns></returns>
		public static Dictionary<string, ScriptMethodInfo> GetMethods(Type type, IEnumerable<Tuple<string, string, int>> names)
		{
			return names.ToDictionary(x => x.Item1, x => new ScriptMethodInfo(type.GetMethod(x.Item2), x.Item1, x.Item3));
		}

		public static Dictionary<string, ScriptPropertyInfo> GetPropertys(Type type, IEnumerable<Tuple<string, string>> names)
		{
			return names.ToDictionary(x => x.Item1, x => new ScriptPropertyInfo(type.GetProperty(x.Item2), x.Item1));
		}

	}

	static class GlobalFunctionProvider
	{
		static Type ValueType = typeof(Single);

		/// <summary>
		/// 用意されたメソッドの定義
		/// </summary>
		/// <returns></returns>
		public static Dictionary<string, ScriptMethodInfo> GetStaticMethodInfo()
		{
			//var ValueType = ExpressionTreeMaker.ValueType;
			var ret = new Dictionary<string, MethodInfo>();
			var mu = typeof(Masa.Lib.MathUtil);
			
			var math = typeof(Math);
			
			var util = typeof(Masa.Lib.Utility);
			foreach (var item in typeof( ScriptRuntime.GlobalFunction).GetMethods(BindingFlags.Static | BindingFlags.Public))
			{
				ret[item.GetCustomAttributes<ScriptMemberAttribute>(false).First().Name] = item;
			}	
			
			Type[][] args = Enumerable.Range(0, 6).Select(x => Enumerable.Repeat(ValueType, x).ToArray()).ToArray();
			ret.Add("cos", mu.GetMethod("Cos"));
			ret.Add("sin", mu.GetMethod("Sin"));
			ret.Add("tan", mu.GetMethod("Tan"));
			ret.Add("atan", mu.GetMethod("Atan2"));
			ret.Add("pow", typeof(Masa.Lib.MathUtil).GetMethod("Pow", new[] { ValueType, ValueType }));
			ret.Add("abs", math.GetMethod("Abs", new[] { ValueType }));
			ret.Add("max", math.GetMethod("Max", new[] { ValueType, ValueType }));
			ret.Add("min", math.GetMethod("Min", new[] { ValueType, ValueType }));
			

			ret["log"] = mu.GetMethod("Log", args[2]);
			
			ret["limit"] = util.GetMethod("LimitChange");
			ret["vibrate"] = util.GetMethod("Vibrate");
			ret["pmod"] = util.GetMethod("PositiveMod", args[2]);
			ret["isnan"] = mu.GetMethod("IsNan", args[1]);
			
			return ret.ToDictionary(x => x.Key, x => new ScriptMethodInfo(x.Value, x.Key, x.Value.GetParameters().Count()));
			//return ret;
		}

		

		public static Dictionary<string, Expression> GetConstantValueDictionary()
		{
			return new Dictionary<string, Expression>()
			{
				{"PI2", Expression.Constant((Value)Math.PI * 2f, ValueType)},
				{"PI", Expression.Constant((Value)Math.PI, ValueType)},
				{"PI_2", Expression.Constant((Value)Math.PI * .5f, ValueType)},
				{"true", Expression.Constant(true)},
				{"false", Expression.Constant(false)},
				{"NAN", Expression.Constant(Value.NaN)}
			};
		}

		public static Dictionary<string, Type> GetTypeNameDictionary()
		{
			return new Dictionary<string, Type>()
			{
				{"float", typeof(float)},
				
				{"double", typeof(double)},
				{"int", typeof(int)},
			};
		}
	}
}
