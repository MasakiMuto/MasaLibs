using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;
using System.Reflection;
using Masa.Lib;
using MoreLinq;

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

		public Type TargetType { get; set; }
	}

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = true, AllowMultiple = false)]
	public class ScriptTypeAttribute : Attribute
	{
		public string Name { get; private set; }
		
		public ScriptTypeAttribute(string name)
		{
			Name = name;
		}
	}

	internal class ScriptMethodInfo
	{
		public readonly string Name;
		public readonly MethodInfo MethodInfo;
		public readonly ScriptMemberAttribute Attribute;
		public readonly int DefaultParameterCount;

		/// <summary>
		/// ScriptMemberAttributeを探しだして作成
		/// </summary>
		/// <param name="method"></param>
		public ScriptMethodInfo(MethodInfo method)
			: this(method, method.GetCustomAttributes<ScriptMemberAttribute>(true).First())
		{
			
		}

		/// <summary>
		/// オプション引数なしで作成
		/// </summary>
		/// <param name="method"></param>
		/// <param name="name"></param>
		/// <param name="paramCount"></param>
		internal ScriptMethodInfo(MethodInfo method, string name, int paramCount)
		{
			MethodInfo = method;
			Name = name;
			DefaultParameterCount = paramCount;
			Attribute = null;
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
			Name = atr.Name;
		}

		public override string ToString()
		{
			return Name + ":" + MethodInfo.ToString();
		}
	}

	internal class ScriptPropertyInfo
	{
		public readonly string Name;
		public readonly PropertyInfo PropertyInfo;
		public readonly ScriptMemberAttribute Attribute;

		public ScriptPropertyInfo(PropertyInfo prop, ScriptMemberAttribute atr)
		{
			PropertyInfo = prop;
			Attribute = atr;
			Name = atr.Name;
		}
	}

	
}
