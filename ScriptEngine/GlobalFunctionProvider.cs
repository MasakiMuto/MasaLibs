using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Masa.Lib;
using Masa.Lib.XNA;
using System.Reflection;
using System.Linq.Expressions;

namespace Masa.ScriptEngine
{
	using Value = System.Single;
	using Vector2 = Microsoft.Xna.Framework.Vector2;
	using Vector3 = Microsoft.Xna.Framework.Vector3;


	static class GlobalFunctionProvider
	{

		static Type ValueType = ExpressionTreeMaker.ValueType;

		public static Vector2 MakeVector2(Value x, Value y)
		{
			return new Vector2(x, y);
		}

		public static Vector3 MakeVector3(Value x, Value y, Value z)
		{
			return new Vector3(x, y, z);
		}

		public static Vector4 MakeVector4(Value x, Value y, Value z, Value w)
		{
			return new Vector4(x, y, z, w);
		}

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

		public static float GetVectorLength(Vector2 vector)
		{
			return vector.Length();
		}

		public static float GetVectorLengthSquared(Vector2 vector)
		{
			return vector.LengthSquared();
		}

		public static float GetVectorX(Vector2 vector)
		{
			return vector.X;
		}

		public static float GetVectorY(Vector2 vector)
		{
			return vector.Y;
		}

		public static float InRange(float under, float value, float over)
		{
			return ((under <= value) && (value <= over)) ? 1 : 0;
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
			var math = typeof(Math);
			var vals = typeof(GlobalFunctionProvider);
			Type vec = typeof(Vector2);
			Type[] vecs = new[] { vec };
			
			Type[][] args = Enumerable.Range(0, 6).Select(x => Enumerable.Repeat(ValueType, x).ToArray()).ToArray();
			ret.Add("cos", mu.GetMethod("Cos"));
			ret.Add("sin", mu.GetMethod("Sin"));
			ret.Add("tan", mu.GetMethod("Tan"));
			ret.Add("atan", mu.GetMethod("Atan2"));
			ret.Add("pow", typeof(Masa.Lib.MathUtil).GetMethod("Pow", new[] { ValueType, ValueType }));
			ret.Add("abs", math.GetMethod("Abs", new[] { ValueType }));
			ret.Add("max", math.GetMethod("Max", new[] { ValueType, ValueType }));
			ret.Add("min", math.GetMethod("Min", new[] { ValueType, ValueType }));
			ret["sign"] = math.GetMethod("Sign", args[1]);
			ret["f2"] = ret["float2"] = vals.GetMethod("MakeVector2", args[2]);
			ret["f3"] = ret["float3"] = vals.GetMethod("MakeVector3", args[3]);
			ret["f4"] = ret["float4"] = vals.GetMethod("MakeVector4", args[4]);
			ret["float"] = vals.GetMethod("MakeFloat", args[1]);
			ret["double"] = vals.GetMethod("MakeDouble", args[1]);
			ret["int"] = vals.GetMethod("MakeInteger", args[1]);
			ret["bool"] = vals.GetMethod("MakeBool", args[1]);
			ret["float2arc"] = xmath.GetMethod("GetVector", args[2]);
			ret["float2ang"] = xmath.GetMethod("Angle", vecs);
			ret["float2len"] = vals.GetMethod("GetVectorLength", vecs);
			ret["float2len2"] = vals.GetMethod("GetVectorLengthSquared", vecs);
			ret["f2x"] = vals.GetMethod("GetVectorX", vecs);
			ret["f2y"] = vals.GetMethod("GetVectorY", vecs);
			ret["in"] = vals.GetMethod("InRange", args[3]);
			ret["log"] = mu.GetMethod("Log", args[2]);
			return ret;
		}

		public static Dictionary<string, Expression> GetConstantValueDictionary()
		{
			return new Dictionary<string, Expression>()
			{
				{"PI2", Expression.Constant((Value)Math.PI * 2f, ValueType)},
				{"PI", Expression.Constant((Value)Math.PI, ValueType)},
				{"PI_2", Expression.Constant((Value)MathHelper.PiOver2, ValueType)},
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
				{"float2", typeof(Vector2)},
				{"float3", typeof(Vector3)},
				{"double", typeof(double)},
				{"int", typeof(int)},
			};
		}
	}
}
