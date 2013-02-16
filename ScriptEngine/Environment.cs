using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Masa.ScriptEngine
{
	using Value = System.Single;
	public class SimpleEnvironment
	{
		[ScriptMember("state")]
		public Value State
		{
			get
			{
				return state;
			}
			set
			{
				if (state != value)
				{
					state = value;
					StateFrame = 0;
				}
			}
		}
		Value state;


		[ScriptMember("count")]
		public Value Frame;
		[ScriptMember("scount")]
		public Value StateFrame;
		[ScriptMember("lcount")]
		public Value LastFrame;
		[ScriptMember("slcount")]
		public Value LastStateFrame;
		
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
			if (globalNum > 0)
			{
				GlobalVar = new float[globalNum];
			}
			TargetObject = target;
		}

	}
}
