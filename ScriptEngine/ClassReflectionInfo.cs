using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoreLinq;
using System.Reflection;
using Masa.Lib;

namespace Masa.ScriptEngine
{
	using MethodDict = Dictionary<string, ScriptMethodInfo>;
	using PropDict = Dictionary<string, ScriptPropertyInfo>;
	using FieldDict = Dictionary<string, FieldInfo>;

	public class ClassReflectionInfo
	{
		public readonly MethodDict StaticMethodDict;
		public readonly MethodDict MethodDict;
		public readonly PropDict PropertyDict;
		public readonly FieldDict FieldDict;
		public readonly Type TargetType;

		public ClassReflectionInfo (Type target, MethodDict method, PropDict prop, FieldDict field, MethodDict staticMethod)
		{
			TargetType = target;
			MethodDict = method;
			StaticMethodDict = staticMethod;
			PropertyDict = prop;
			FieldDict = field;
		}

		public ClassReflectionInfo(Type target)
		{
			var md = new MethodDict();
			var pd = new PropDict();
			var fd = new FieldDict();
			var overrideItems = new Dictionary<string, List<Tuple<ScriptMemberAttribute, Maybe<ScriptMethodInfo, ScriptPropertyInfo>>>>();

			foreach (var item in target.GetMembers(BindingFlags.NonPublic | BindingFlags.Public
				| BindingFlags.Instance | BindingFlags.FlattenHierarchy))
			{
				var attribute = GetScriptMemberAttribute(item);
				if (attribute == null)
				{
					continue;
				}
				
				Maybe<ScriptMethodInfo, ScriptPropertyInfo> maybe = null;

				if (!overrideItems.ContainsKey(attribute.Name))
				{
					overrideItems[attribute.Name] = new List<Tuple<ScriptMemberAttribute, Maybe<ScriptMethodInfo, ScriptPropertyInfo>>>();
				}
				if (item.MemberType == MemberTypes.Method)
				{

					var info = new ScriptMethodInfo(item as MethodInfo);
					md[attribute.Name] = info;
					maybe = new Maybe<ScriptMethodInfo, ScriptPropertyInfo>(info);
				}
				else if (item.MemberType == MemberTypes.Field)
				{
					fd[attribute.Name] = item as FieldInfo;//no override
				}
				else if (item.MemberType == MemberTypes.Property)
				{
					var info = new ScriptPropertyInfo(item as PropertyInfo, attribute);
					pd[attribute.Name] = info;
					maybe = new Maybe<ScriptMethodInfo, ScriptPropertyInfo>(info);
				}
				overrideItems[attribute.Name].Add(new Tuple<ScriptMemberAttribute, Maybe<ScriptMethodInfo, ScriptPropertyInfo>>(attribute, maybe));
			}

			foreach (var item in overrideItems.Where(i => i.Value.Count > 1))//スクリプト名が重複しているものの処理
			{
				var exception = new Exception("スクリプト名の重複" + item.Key + item.Value.Select(x => x.Item2.ToString()));
				if (!item.Value.Any(x => x.Item1.IsOverride))
				{
					throw exception;
				}
				var head = item.Value
					.Select(x => new { x = x, info = x.Item2.IsT() ? x.Item2.Value1.MethodInfo as MemberInfo : x.Item2.Value2.PropertyInfo as MemberInfo })
					.MaxBy(x => x.info.DeclaringType.GetBaseTypeTree().Count).x;//一番継承関係が深いもの
				//	.OrderByDescending(x => x, (x, y) => x.info.DeclaringType.GetBaseTypeTree().Count - y.info.DeclaringType.GetBaseTypeTree().Count)
				//	.First().x;
				if (!head.Item1.IsOverride)
				{
					throw exception;
				}
				else
				{
					if (head.Item2.IsT())
					{
						md[head.Item1.Name] = head.Item2.Value1;
					}
					else
					{
						pd[head.Item1.Name] = head.Item2.Value2;
					}
				}


			}
			MethodDict = md;
			PropertyDict = pd;
			FieldDict = fd;
			StaticMethodDict = GetStaticMethods(target);
		}

		/// <summary>
		/// 単一の属性を返す
		/// </summary>
		/// <param name="member"></param>
		/// <returns></returns>
		internal static ScriptMemberAttribute GetScriptMemberAttribute(MemberInfo member)
		{
			return Attribute.GetCustomAttribute(member, typeof(ScriptMemberAttribute), true) as ScriptMemberAttribute;
		}

		static MethodDict GetStaticMethods(Type t)
		{
			var ret = new MethodDict();
			foreach (var item in t.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
			{
				var attr = item.GetCustomAttributes<ScriptMemberAttribute>(false).FirstOrDefault();
				if (attr != null)
				{
					ret[attr.Name] = new ScriptMethodInfo(item, attr);
				}
			}
			return ret;
		}

	}
}
