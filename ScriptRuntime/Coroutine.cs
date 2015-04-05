using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Masa.ScriptEngine
{
	using TaskUnit = Action<Masa.ScriptEngine.Environment>;

	public class Coroutine
	{
		LinkedList<TaskUnit> tasks;

		int waitCount;
		int count;
		Masa.ScriptEngine.Environment environment;

		public bool ExitFlag { get; set; }

		public Coroutine(IEnumerable<TaskUnit> tasks, Masa.ScriptEngine.Environment env)
		{
			this.tasks = new LinkedList<TaskUnit>(tasks);
			waitCount = 0;
			count = 0;
			environment = env;
		}


		public void Update()
		{
			count++;
			if (waitCount > 0)
			{
				waitCount--;
				return;
			}
			var head = tasks.First;
			while (head != null && waitCount == 0)
			{
				head.Value(environment);
				head = head.Next;
				tasks.RemoveFirst();
			}
			if (!tasks.Any())
			{
				ExitFlag = true;
			}
		}

		public void SetWait(int count)
		{
			System.Diagnostics.Debug.Assert(waitCount == 0);
			waitCount = count;
		}

	}
}
