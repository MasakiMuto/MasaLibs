using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;
using System.Reflection;

namespace Masa.ScriptEngine
{
	using Value = System.Single;
	using Vector2 = Microsoft.Xna.Framework.Vector2;
	using Vector3 = Microsoft.Xna.Framework.Vector3;

	[AttributeUsage(AttributeTargets.Method, Inherited = true)]
	public class ScriptDefinedMethodAttribute : Attribute
	{

	}

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
	}

	struct ClassReflectionInfo
	{
		public readonly Dictionary<string, MethodInfo> MethodDict;
		public readonly Dictionary<string, PropertyInfo> PropertyDict;
		public ClassReflectionInfo(Dictionary<string, MethodInfo> md, Dictionary<string, PropertyInfo> pd)
		{
			MethodDict = md;
			PropertyDict = pd;
		}
	}



	internal static class ValueCreaterFunctions
	{
		public static Vector2 MakeVector2(Value x, Value y)
		{
			return new Vector2(x, y);
		}

		public static Vector3 MakeVector3(Value x, Value y, Value z)
		{
			return new Vector3(x, y, z);
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
	}

}
