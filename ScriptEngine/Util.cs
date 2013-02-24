using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;
using System.Reflection;
using Masa.Lib;

namespace Masa.ScriptEngine
{
	using Value = System.Single;
	using Vector2 = Microsoft.Xna.Framework.Vector2;
	using Vector3 = Microsoft.Xna.Framework.Vector3;

	//[AttributeUsage(AttributeTargets.Method, Inherited = true)]
	//public class ScriptDefinedMethodAttribute : Attribute
	//{

	//}

	class Line
	{
		/// <summary>
		/// テキストエディタ上での行番号
		/// 1からスタートし、コメントや空行も数える
		/// </summary>
		public int Number;
		/// <summary>
		/// 処理上の行番号で、インデックスに対応。0からスタート
		/// コメントや空行を除いた行の番号
		/// </summary>
		public int Index;
		public int Level;
		public object[] Tokens;

		public Line(int num, int lev, object[] t)
		{
			Number = num;
			Level = lev;
			Tokens = t;
			Index = int.MinValue;
		}

		public bool IsEmpty
		{
			get { return Tokens == null || Tokens.Length == 0; }
		}

		public override string ToString()
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			foreach (var item in Tokens)
			{
				sb.Append(item.ToString() + " ");
			}
			return sb.ToString();
		}
	}

	[Serializable]
	internal class ParseException : Exception
	{
		internal ParseException(string msg, Line line)
			: base(MakeMessage(msg, line))
		{

		}

		internal ParseException(string msg)
			: base(MakeMessage(msg, null))
		{

		}

		static string MakeMessage(string msg, Line line)
		{
			if (line == null)
			{
				return "ParseException:" + msg;
			}
			else
			{
				return "ParseException at Line " + line.Number + ":" + msg + "\n" + line.ToString();
			}
		}
	}

	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
	public class ScriptMemberAttribute : Attribute
	{
		string name;
		public ScriptMemberAttribute(string scriptName)
		{
			name = scriptName;
			IsOverride = false;
		}
		public ScriptMemberAttribute(string scriptName, string[] optionNames, int[] optionArgs)
			: this(scriptName)
		{
			OptionName = optionNames;
			OptionArgNum = optionArgs;
		}
		public string Name
		{
			get { return name; }
		}
		public string[] OptionName
		{
			get;
			set;
		}
		public int[] OptionArgNum
		{
			get;
			set;
		}

		/// <summary>
		/// 基底クラスで宣言された同名のスクリプト要素を上書きするか
		/// </summary>
		public bool IsOverride { get; set; }
	}

	internal class ScriptMethodInfo
	{
		public readonly MethodInfo MethodInfo;
		public readonly ScriptMemberAttribute Attribute;
		public readonly int DefaultParameterCount;

		public ScriptMethodInfo(MethodInfo method)
			: this(method, method.GetCustomAttributes<ScriptMemberAttribute>(true).First())
		{
			
		}

		internal ScriptMethodInfo(MethodInfo method, ScriptMemberAttribute atr)
		{
			MethodInfo = method;
		
			Attribute = atr;
			var paramNum = method.GetParameters().Length;
			if (atr.OptionName == null)
			{
				DefaultParameterCount = paramNum;
			}
			else
			{
				DefaultParameterCount = paramNum - atr.OptionArgNum.Sum();
			}
		}
	}

	struct ClassReflectionInfo
	{
		public readonly Dictionary<string, ScriptMethodInfo> MethodDict;
		public readonly Dictionary<string, PropertyInfo> PropertyDict;
		public ClassReflectionInfo(Dictionary<string, ScriptMethodInfo> md, Dictionary<string, PropertyInfo> pd)
		{
			MethodDict = md;
			PropertyDict = pd;
		}
	}

}
