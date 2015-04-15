using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Masa.ScriptEngine
{
	using Value = System.Single;
	using TaskUnit = Action<Environment>;

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
					LastStateFrame = -1;
					StateFrame = 0;
				}
			}
		}
		Value state;


		[ScriptMember("count")]
		public Value Frame { get; set; }
		[ScriptMember("scount")]
		public Value StateFrame { get; set; }
		[ScriptMember("lcount")]
		public Value LastFrame { get; set; }
		[ScriptMember("slcount")]
		public Value LastStateFrame { get; set; }
		
		public virtual void FrameUpdate()
		{
			LastFrame = Frame;
			LastStateFrame = StateFrame;
			Frame++;
			StateFrame++;
		}
	}

	public class Environment : SimpleEnvironment
	{
		public static readonly FieldInfo Info_TargetObject = typeof(Environment).GetField("TargetObject", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
		public static readonly PropertyInfo Info_StateFrame = typeof(Environment).GetProperty("StateFrame");
		public static readonly PropertyInfo InfoStateLastFrame = typeof(Environment).GetProperty("LastStateFrame");
		public static readonly PropertyInfo Info_Item = typeof(Environment).GetProperty("Item", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
		public static readonly PropertyInfo Info_State = typeof(Environment).GetProperty("State");
		public static readonly ScriptMethodInfo CoroutineBegin = new ScriptMethodInfo(typeof(Environment).GetMethod("BeginCoroutine"), "begin", 1);
		public static readonly ScriptMethodInfo CoroutineBreak = new ScriptMethodInfo(typeof(Environment).GetMethod("BreakCoroutine"), "break", 0);
		public static readonly ScriptMethodInfo CoroutineWait = new ScriptMethodInfo(typeof(Environment).GetMethod("SetCoroutineWait"), "wait", 1);

		float[] GlobalVar;
		public object TargetObject;

		public Dictionary<string, List<TaskUnit>> CoroutineDict { get; set; }
		LinkedList<Coroutine> coroutines;
		Coroutine currentCoroutine;

		public float this[int i]
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
			LastFrame = -1;
		}

		public void BeginCoroutine(string name)
		{
			if (coroutines == null)
			{
				coroutines = new LinkedList<Coroutine>();
			}
			coroutines.AddLast(new Coroutine(CoroutineDict[name], this));
		}

		public void BreakCoroutine()
		{
			currentCoroutine.ExitFlag = true;
		}

		public void SetCoroutineWait(int time)
		{
			currentCoroutine.SetWait(time);
		}

		public override void FrameUpdate()
		{
			base.FrameUpdate();
			if (coroutines != null)
			{
				UpdateCoroutines();
			}
		}

		void UpdateCoroutines()
		{
			var head = coroutines.First;
			while (head != null)
			{
				currentCoroutine = head.Value;
				currentCoroutine.Update();
				if (currentCoroutine.ExitFlag)
				{
					var tmp = head;
					head = head.Next;
					coroutines.Remove(tmp);
				}
				else
				{
					head = head.Next;
				}
			}
			currentCoroutine = null;
		}

	}
}
