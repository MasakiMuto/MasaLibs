using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Reflection;
using System.Linq.Expressions;
using Masa.ScriptEngine;

namespace Masa.Lib.XNA
{
	using Vector2 = Microsoft.Xna.Framework.Vector2;
	using Vector3 = Microsoft.Xna.Framework.Vector3;
	using Value = Single;

	public static class ScriptDefinition
	{
		static Type ValueType = typeof(float);

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

		static Dictionary<string, Masa.ScriptEngine.ScriptMethodInfo> GetStaticMethodInfo()
		{
			var vals = typeof(ScriptDefinition);
			var xmath = typeof(Masa.Lib.XNA.MathUtilXNA);
			var ret = new Dictionary<string, MethodInfo>();
			Type vec = typeof(Vector2);
			Type[] vecs = new[] { vec };

			Type[][] args = Enumerable.Range(0, 6).Select(x => Enumerable.Repeat(ValueType, x).ToArray()).ToArray();

			ret["f2"] = ret["float2"] = vals.GetMethod("MakeVector2", args[2]);
			ret["f3"] = ret["float3"] = vals.GetMethod("MakeVector3", args[3]);
			ret["f4"] = ret["float4"] = vals.GetMethod("MakeVector4", args[4]);

			ret["float2arc"] = xmath.GetMethod("GetVector", args[2]);
			ret["float2ang"] = xmath.GetMethod("Angle", vecs);
			ret["float2len"] = vals.GetMethod("GetVectorLength", vecs);
			ret["float2len2"] = vals.GetMethod("GetVectorLengthSquared", vecs);
			ret["f2x"] = vals.GetMethod("GetVectorX", vecs);
			ret["f2y"] = vals.GetMethod("GetVectorY", vecs);
			ret["norm2"] = typeof(Vector2).GetMethod("Normalize", new[] { typeof(Vector2) });
			ret["norm3"] = typeof(Vector3).GetMethod("Normalize", new[] { typeof(Vector3) });
			ret["dot2"] = typeof(Vector2).GetMethod("Dot", new[] { typeof(Vector2), typeof(Vector2) });
			ret["dot3"] = typeof(Vector3).GetMethod("Dot", new[] { typeof(Vector3), typeof(Vector3) });

			ret["hsv"] = typeof(Masa.Lib.XNA.HSVColor).GetMethod("HSVToRGB");
			ret["wrapangle"] = typeof(Microsoft.Xna.Framework.MathHelper).GetMethod("WrapAngle");
			ret["laim"] = xmath.GetMethod("LimitedAim");

			return ret.ToDictionary(x => x.Key, x => new ScriptMethodInfo(x.Value, x.Key, x.Value.GetParameters().Length));
		}

		static Dictionary<Type, ClassReflectionInfo> GetLibraryClassScriptInfo()
		{
			var ret = new Dictionary<Type, ClassReflectionInfo>();
			var v2 = typeof(Vector2);
			var v3 = typeof(Vector3);
			Func<IEnumerable<Tuple<string, string>>, Type, Dictionary<string, ScriptMethodInfo>> getMethod = (keys, type) =>
				keys.ToDictionary(x => x.Item1, x => new ScriptMethodInfo(type.GetMethod(x.Item2), x.Item1, 0));//scriptName, originalName, type

			ret[v2] = new ClassReflectionInfo(v2,
				getMethod(new[]
				{
					Tuple.Create("len", "Length"),
					Tuple.Create("len2", "LengthSquared"),
				}, v2),
				//new Dictionary<string, ScriptMethodInfo>()
				//{
				//	{ "len", new ScriptMethodInfo(v2.GetMethod("Length"), "len", 0)},
				//	{ "len2", new ScriptMethodInfo(v2.GetMethod("LengthSquared"), "len2")},
				//},
				new Dictionary<string, ScriptPropertyInfo>()
				{

				},
				new Dictionary<string, FieldInfo>()
				{
					{"x", v2.GetField("X")},
					{"y", v2.GetField("Y")},
				},
				new Dictionary<string, ScriptMethodInfo>()
				{
				});
			ret[v3] = new ClassReflectionInfo(v3,
				//new Dictionary<string, ScriptMethodInfo>()
				//{
				//	{ "len", new ScriptMethodInfo(v3.GetMethod("Length"), "len")},
				//	{ "len2", new ScriptMethodInfo(v3.GetMethod("LengthSquared"), "len2")},
				//},
				getMethod(new[]
				{
					Tuple.Create("len", "Length"),
					Tuple.Create("len2", "LengthSquared"),
				}, v3),
				new Dictionary<string, ScriptPropertyInfo>()
				{

				},
				new Dictionary<string, FieldInfo>()
				{
					{"x", v3.GetField("X")},
					{"y", v3.GetField("Y")},
					{"z", v3.GetField("Z")},
				},
				new Dictionary<string, ScriptMethodInfo>()
				{
				});
			return ret;
		}

		static Dictionary<string, Type> GetTypeNameDictionary()
		{
			return new Dictionary<string, Type>()
			{
				{"float2", typeof(Vector2)},
				{"float3", typeof(Vector3)},
			};
		}

		public static void AppendDefinition()
		{
			ExpressionTreeMaker.AddDictionarys(GetStaticMethodInfo(), GetLibraryClassScriptInfo(), null, GetTypeNameDictionary());
		}
		

	}
}
