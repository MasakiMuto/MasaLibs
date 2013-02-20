using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Xml.Linq;

namespace Masa.ScriptEngine
{
	internal static class DocumentCreater
	{
		internal static string OutputClass(Dictionary<string, ScriptMethodInfo> method, Dictionary<string, PropertyInfo> property)
		{
			var str = new StringBuilder();
			string[] itemTypes = { "Method", "Function", "Property" };
			foreach (var item in method.OrderBy(i => i.Key))
			{
				PrintName(str, item.Value.MethodInfo.ReturnType == typeof(float) ? "Function" : "Method", item.Key, item.Value.MethodInfo.Name);
				//str.Append(itemTypes[item.Value.ReturnType == typeof(float) ? 1 : 0]);
				//str.Append(": ");
				//str.Append(item.Key);
				//str.Append(" from ");
				//str.AppendLine(item.Value.Name);
				var attr = item.Value.Attribute;
				var param = item.Value.MethodInfo.GetParameters();
				int index = 0;
				ParameterInfo[] normalParams;
				if (attr.OptionName != null)
				{
					normalParams = param.Take(param.Length - attr.OptionArgNum.Sum()).ToArray();
				}
				else
				{
					normalParams = param;
				}
				foreach (var p in normalParams)
				{
					str.Append(p.Name);
					str.Append(", ");
				}
				if (normalParams.Length != 0)
				{
					str.AppendLine();
				}
				index = normalParams.Length;
				if (attr.OptionName != null)
				{
					for (int i = 0; i < attr.OptionName.Length; i++)
					{
						str.Append(attr.OptionName[i]);
						str.Append(" : ");
						for (int j = 0; j < attr.OptionArgNum[i]; j++)
						{
							str.Append(param[index].Name);
							str.Append(", ");
							index++;
						}
						str.AppendLine();

					}
				}
				str.AppendLine();
			}
			foreach (var item in property)
			{
				//str.Append(itemTypes[2]);
				//str.Append(": ");
				//str.Append(item.Key);
				PrintName(str, "Property", item.Key, item.Value.Name);
				str.AppendLine();
			}
			return str.ToString();

		}


		static void PrintName(StringBuilder builder, string typeName, string scriptName, string baseName)
		{
			builder.Append(typeName);
			builder.Append(": ");
			builder.Append(scriptName);
			builder.Append("(");
			builder.Append(baseName);
			builder.AppendLine(")");
		}

		internal static XElement GlobalsToXml(Dictionary<string, MethodInfo> method)
		{
			var root = new XElement("class");
			root.Add(NameToXml("global"));
			var methodRoot = new XElement("methods");
			methodRoot.Add(
				method
				.Select(x => new ScriptMethodInfo(x.Value, new ScriptMemberAttribute(x.Key)))
				.Select(x=>MethodToXml(x))
				.ToArray()
			);
			root.Add(methodRoot);

			return root;

		}

		internal static XElement ClassToXml(Type target, Dictionary<string, ScriptMethodInfo> method, Dictionary<string, PropertyInfo> property)
		{
			var root = new XElement("class");
			root.Add(NameToXml(target.Name));
			var methodRoot = new XElement("methods");
			methodRoot.Add(method.Select(x => MethodToXml(x.Value)).ToArray());
			var propRoot = new XElement("propertys");
			propRoot.Add(property.Select(x => PropertyToXml(x.Value)).ToArray());
			root.Add(methodRoot, propRoot);

			return root;
		}

		static XElement MemberToXml(string type, MemberInfo member, ScriptMemberAttribute atr)
		{
			var root = new XElement(type);
			root.Add(NameToXml(atr.Name));
			root.Add(new XElement("source", member.DeclaringType.Name + "." + member.Name));
			return root;
		}

		static XElement MethodToXml(ScriptMethodInfo method)
		{
			var root = MemberToXml("method", method.MethodInfo, method.Attribute);
			//root.Add(NameToXml(method.Attribute.Name));
			root.Add(TypeToXml(method.MethodInfo.ReturnType));
			//root.Add(new XElement("source", method.MethodInfo.Name));
			var param = new XElement("params");
			if (method.DefaultParameterCount > 0)
			{
				var defparam = new XElement("default");
				foreach (var item in method.MethodInfo.GetParameters().Take(method.DefaultParameterCount))
				{
					defparam.Add(ParameterToXml(item)); 
				}
				param.Add(defparam);
			}
			if (method.Attribute.OptionName != null && method.Attribute.OptionName.Count() > 0)
			{
				var opt = new XElement("options");
				int index = method.DefaultParameterCount;
				foreach (var item in method.Attribute.OptionName.Zip(method.Attribute.OptionArgNum, (name, num)=>new{name, num}))
				{
					var option = new XElement("option", NameToXml(item.name));
					for (int i = 0; i < item.num; i++)
					{
						option.Add(ParameterToXml(method.MethodInfo.GetParameters()[index]));
						index++;
					}
					opt.Add(option);

					//opt.Add(new XElement(,  ParameterToXml(method.MethodInfo.GetParameters()[index])
				}
				param.Add(opt);
			}
			root.Add(param);
			return root;
		}

		static XElement PropertyToXml(PropertyInfo prop)
		{
			var root = MemberToXml("property", prop, ExpressionTreeMakerHelper.GetScriptMemberAttribute(prop));
			root.Add(TypeToXml(prop.PropertyType));
			root.Add(new XElement("get", prop.CanRead), new XElement("set", prop.CanWrite));
			return root;
		}


		static XElement TypeToXml(Type type)
		{
			return new XElement("type", type.Name);
		}

		static XElement ParameterToXml(ParameterInfo param)
		{
			return new XElement("param", NameToXml(param.Name), TypeToXml(param.ParameterType));
		}

		static XElement NameToXml(string name)
		{
			return new XElement("name", name);
		}
	}
}
