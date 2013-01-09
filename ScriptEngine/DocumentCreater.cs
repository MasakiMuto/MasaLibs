using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Masa.ScriptEngine
{
	public static class DocumentCreater
	{
		public static string OutputClass(Dictionary<string, MethodInfo> method, Dictionary<string, PropertyInfo> property)
		{
			var str = new StringBuilder();
			string[] itemTypes = {"Method", "Function", "Property" };
			foreach (var item in method.OrderBy(i=>i.Key))
			{
				PrintName(str, item.Value.ReturnType == typeof(float) ? "Function" : "Method", item.Key, item.Value.Name);
				//str.Append(itemTypes[item.Value.ReturnType == typeof(float) ? 1 : 0]);
				//str.Append(": ");
				//str.Append(item.Key);
				//str.Append(" from ");
				//str.AppendLine(item.Value.Name);
				var attr = item.Value.GetCustomAttributes(typeof(ScriptMemberAttribute), true).First() as ScriptMemberAttribute;
				var param = item.Value.GetParameters();
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
	}
}
