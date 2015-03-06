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
	

	static class GlobalFunctionProvider
	{
		static Type ValueType = typeof(float);

		public static int MakeInteger(Value x)
		{
			return (int)x;
		}

		public static double MakeDouble(Value x)
		{
			return (double)x;
		}

		public static float MakeFloat(Value x)
		{
			return (float)x;
		}

		public static bool MakeBool(Value x)
		{
			return x != 0;
		}

		public static float InRange(float under, float value, float over)
		{
			return ((under <= value) && (value <= over)) ? 1 : 0;
		}

		public static string ToString(object obj)
		{
			return obj.ToString();
		}

		public static string ValueToString(Value val)
		{
			return val.ToString();
		}

		public static Value Round(Value val)
		{
			return (int)val;
		}

		public static Value Sign(Value val)
		{
			return Math.Sign(val);
		}

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
			var vals = typeof(GlobalFunctionProvider);
			var util = typeof(Masa.Lib.Utility);
			
			
			Type[][] args = Enumerable.Range(0, 6).Select(x => Enumerable.Repeat(ValueType, x).ToArray()).ToArray();
			ret.Add("cos", mu.GetMethod("Cos"));
			ret.Add("sin", mu.GetMethod("Sin"));
			ret.Add("tan", mu.GetMethod("Tan"));
			ret.Add("atan", mu.GetMethod("Atan2"));
			ret.Add("pow", typeof(Masa.Lib.MathUtil).GetMethod("Pow", new[] { ValueType, ValueType }));
			ret.Add("abs", math.GetMethod("Abs", new[] { ValueType }));
			ret.Add("max", math.GetMethod("Max", new[] { ValueType, ValueType }));
			ret.Add("min", math.GetMethod("Min", new[] { ValueType, ValueType }));
			ret["sign"] = vals.GetMethod("Sign", args[1]);
			
			
			ret["float"] = vals.GetMethod("MakeFloat", args[1]);
			ret["double"] = vals.GetMethod("MakeDouble", args[1]);
			ret["int"] = vals.GetMethod("MakeInteger", args[1]);
			ret["bool"] = vals.GetMethod("MakeBool", args[1]);
			

			ret["in"] = vals.GetMethod("InRange", args[3]);
			ret["log"] = mu.GetMethod("Log", args[2]);
			
			ret["limit"] = util.GetMethod("LimitChange");
			ret["vibrate"] = util.GetMethod("Vibrate");
			ret["pmod"] = util.GetMethod("PositiveMod", args[2]);
			ret["isnan"] = mu.GetMethod("IsNan", args[1]);
			ret["tostr"] = vals.GetMethod("ToString", new[]{typeof(object)});
			ret["valtostr"] = vals.GetMethod("ValueToString");
			ret["round"] = vals.GetMethod("Round");
			
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
