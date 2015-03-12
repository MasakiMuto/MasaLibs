using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Masa.ScriptEngine;

namespace ScriptRuntime
{
	using Value = Single;
	public static class GlobalFunction
	{
		[ScriptMember("int")]
		public static int MakeInteger(Value x)
		{
			return (int)x;
		}

		[ScriptMember("double")]
		public static double MakeDouble(Value x)
		{
			return (double)x;
		}

		[ScriptMember("float")]
		public static float MakeFloat(Value x)
		{
			return (float)x;
		}

		[ScriptMember("bool")]
		public static bool MakeBool(Value x)
		{
			return x != 0;
		}

		[ScriptMember("in")]
		public static float InRange(float under, float value, float over)
		{
			return ((under <= value) && (value <= over)) ? 1 : 0;
		}

		[ScriptMember("tostr")]
		public static string ToString(object obj)
		{
			return obj.ToString();
		}

		[ScriptMember("valtostr")]
		public static string ValueToString(Value val)
		{
			return val.ToString();
		}

		[ScriptMember("round")]
		public static Value Round(Value val)
		{
			return (int)val;
		}

		[ScriptMember("sign")]
		public static Value Sign(Value val)
		{
			return Math.Sign(val);
		}
	}
}
