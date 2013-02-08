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
	using Vector2 = Microsoft.Xna.Framework.Vector2;
	using Vector3 = Microsoft.Xna.Framework.Vector3;

	internal class ExpressionTreeMakerHelper
	{
		static Type ValueType = ExpressionTreeMaker.ValueType;

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

		/// <summary>
		/// 用意されたメソッドの定義
		/// </summary>
		/// <returns></returns>
		public static Dictionary<string, MethodInfo> GetStaticMethodInfo()
		{
			//var ValueType = ExpressionTreeMaker.ValueType;
			var ret = new Dictionary<string, MethodInfo>();
			var mu = typeof(Masa.Lib.MathUtil);
			var xmath = typeof(Masa.Lib.XNA.MathUtilXNA);
			var vals = typeof(ValueCreaterFunctions);
			Type vec = typeof(Vector2);
			Type[] vecs = new[] { vec };
			Type[][] args = Enumerable.Range(0, 4).Select(x => Enumerable.Repeat(ValueType, x).ToArray()).ToArray();
			ret.Add("cos", mu.GetMethod("Cos"));
			ret.Add("sin", mu.GetMethod("Sin"));
			ret.Add("tan", mu.GetMethod("Tan"));
			ret.Add("atan", mu.GetMethod("Atan2"));
			ret.Add("pow", typeof(Masa.Lib.MathUtil).GetMethod("Pow", new[] { ValueType, ValueType }));
			ret.Add("abs", typeof(Math).GetMethod("Abs", new[] { ValueType }));
			ret.Add("max", typeof(Math).GetMethod("Max", new[] { ValueType, ValueType }));
			ret.Add("min", typeof(Math).GetMethod("Min", new[] { ValueType, ValueType }));
			ret["float2"] = vals.GetMethod("MakeVector2", args[2]);
			ret["float3"] = vals.GetMethod("MakeVector3", args[3]);
			ret["float"] = vals.GetMethod("MakeFloat", args[1]);
			ret["double"] = vals.GetMethod("MakeDouble", args[1]);
			ret["int"] = vals.GetMethod("MakeInteger", args[1]);
			ret["float2arc"] = xmath.GetMethod("GetVector", args[2]);
			ret["float2ang"] = xmath.GetMethod("Angle", vecs);
			ret["float2len"] = vals.GetMethod("GetVectorLength", vecs);
			ret["float2len2"] = vals.GetMethod("GetVectorLengthSquared", vecs);
			return ret;
		}

		public static Dictionary<string, Expression> GetConstantValueDictionary()
		{
			return new Dictionary<string, Expression>()
			{
				{"PI2", Expression.Constant((Value)Math.PI * 2f, ValueType)},
				{"PI", Expression.Constant((Value)Math.PI, ValueType)},
			};
		}

		public static Dictionary<string, Type> GetTypeNameDictionary()
		{
			return new Dictionary<string, Type>()
			{
				{"float", typeof(float)},
				{"float2", typeof(Vector2)},
				{"float3", typeof(Vector3)},
				{"double", typeof(double)},
				{"int", typeof(int)},
			};
		}

	}
}
