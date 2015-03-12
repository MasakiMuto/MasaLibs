using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Masa.ScriptEngine;
using System.Reflection;
using ScriptRuntime.Util;

namespace Masa.ScriptCompiler
{
	public static class UnityScriptDefinition
	{
		static Type v3 = typeof(Vector3);
		static Type v2 = typeof(Vector2);
		static Type def = typeof(UnityScriptDefinition);
		static Type math = typeof(Mathf);

		[ScriptMember("float2angle")]
		public static Vector2 Float2Angle(float length, float angle)
		{
			return new Vector2(Mathf.Cos(angle) * length, Mathf.Sin(angle) * length);
		}

		public static Dictionary<string, ScriptMethodInfo> GetStaticMethodInfo()
		{
			var dict = new Dictionary<string, ScriptMethodInfo>();
			foreach (var item in def.GetMethods(BindingFlags.Static | BindingFlags.Public))
			{
				var atr = item.GetCustomAttributes(typeof(ScriptMemberAttribute), false).OfType<ScriptMemberAttribute>().FirstOrDefault();
				if (atr != null)
				{
					dict[atr.Name] = new ScriptMethodInfo(item, atr);
				}
			}
			dict["deltaangle"] = new ScriptMethodInfo(math.GetMethod("DeltaAngle"), "deltaangle", 2);
			return dict;
		}

		public static Dictionary<Type, ClassReflectionInfo> GetLibraryClassScriptInfo()
		{
			var dict = new Dictionary<Type, ClassReflectionInfo>();
			//Func<string, int, Type, Dictionary<string, ScriptMethodInfo>> getMethod = 
			Func<Type, IEnumerable<Tuple<string, string, int>>, Dictionary<string, ScriptMethodInfo>> getMethod =
				(t, l) => l.ToDictionary(x => x.Item1, x => new ScriptMethodInfo(t.GetMethod(x.Item2), x.Item1, x.Item3));
			//Tuple.Create("angle", "Angle", 2)
			//Func<Type, IEnumerable<Tuple<string, string>>, Dictionary<string, ScriptPropertyInfo>> getProperty = 


			dict[v2] = new ClassReflectionInfo(v2,
				new Dictionary<string,ScriptMethodInfo>(),
				new Dictionary<string, ScriptPropertyInfo>(){
					{"len", new ScriptPropertyInfo(v2.GetProperty("magnitude"), "len")},
					{"len2", new ScriptPropertyInfo(v2.GetProperty("sqrMagnitude"), "len2")},
					{"norm", new ScriptPropertyInfo(v2.GetProperty("normalized"), "norm")},
				},
				new Dictionary<string, System.Reflection.FieldInfo>()
				{
					{"x", v2.GetField("x")},
					{"y", v2.GetField("y")},
				},
				getMethod(v2, new[] 
				{
					Tuple.Create("angle", "Angle", 2),
					Tuple.Create("dot", "Dot", 2),
				})
			);
			
			dict[v3] = new ClassReflectionInfo(v3,
				new Dictionary<string, ScriptMethodInfo>(),
				new Dictionary<string, ScriptPropertyInfo>(),
				new Dictionary<string, System.Reflection.FieldInfo>()
				{
					{"x", v3.GetField("x")},
					{"y", v3.GetField("y")},
					{"z", v3.GetField("z")},
				},
				new Dictionary<string, ScriptMethodInfo>()
			);

			//var behavior = typeof(MonoBehaviour);
			//dict[behavior] = new ClassReflectionInfo(behavior,
			//	new Dictionary<string, ScriptMethodInfo>(),
			//	ScriptDefinitionHelper.GetPropertys(behavior, new[]
			//	{
			//		Tuple.Create("transform", "transform")
			//	}),
			//	new Dictionary<string, System.Reflection.FieldInfo>(),
			//	new Dictionary<string, ScriptMethodInfo>());

			var transform = typeof(Transform);
			dict[transform] = new ClassReflectionInfo(transform, 
				new Dictionary<string,ScriptMethodInfo>()
				{
					{"rotate", new ScriptMethodInfo(transform.GetMethod("Rotate", Enumerable.Repeat(typeof(float), 3).ToArray()), "rotate", 3)},
				},
				new Dictionary<string, ScriptPropertyInfo>(){
					{"pos", new ScriptPropertyInfo(transform.GetProperty("transform"), "pos")}
				},
				new Dictionary<string, System.Reflection.FieldInfo>(),
				new Dictionary<string,ScriptMethodInfo>());

			return dict;
		}

		public static Dictionary<string, Type> GetTypeNameDictionary()
		{
			return new Dictionary<string, Type>()
			{
				{"float2", v2},
				{"float3", v3}
			};
		}


	}
}
