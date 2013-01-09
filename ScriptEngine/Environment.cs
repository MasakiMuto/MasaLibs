using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Masa.ScriptEngine
{
	public class SimpleEnvironment
	{
		[ScriptMember("state")]
		public float State
		{
			get
			{
				return state;
			}
			set
			{
				state = value;
				StateFrame = 0;
			}
		}
		float state;


		[ScriptMember("count")]
		public float Frame;
		[ScriptMember("scount")]
		public float StateFrame;
		[ScriptMember("lcount")]
		public float LastFrame;
		[ScriptMember("slcount")]
		public float LastStateFrame;
		
		public void FrameUpdate()
		{
			LastFrame = Frame;
			LastStateFrame = StateFrame;
			Frame++;
			StateFrame++;
		}
	}

	public class Environment : SimpleEnvironment
	{
		internal static readonly FieldInfo Info_TargetObject = typeof(Environment).GetField("TargetObject", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
		internal static readonly FieldInfo Info_StateFrame = typeof(Environment).GetField("StateFrame");
		internal static readonly PropertyInfo Info_Item = typeof(Environment).GetProperty("Item", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
		internal static readonly PropertyInfo Info_State = typeof(Environment).GetProperty("State");

		float[] GlobalVar;
		internal object TargetObject;

	
		internal float this[int i]
		{
			get
			{
				return GlobalVar[i];
			}

			set
			{
				GlobalVar[i] = value;
			}
		}

		public Environment(object target, int globalNum) : base()
		{
			GlobalVar = new float[globalNum];
			TargetObject = target;
		}

	}
}
