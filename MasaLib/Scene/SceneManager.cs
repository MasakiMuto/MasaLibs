using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Masa.Lib.Scene
{
	public class SceneManager : IDisposable
	{
		LinkedList<SceneBase> scenes;
		Queue<SceneBase> addQueue, removeQueue;
		bool needSuspend, needResume;

		public SceneManager()
		{
			addQueue = new Queue<SceneBase>();
			removeQueue = new Queue<SceneBase>();
			scenes = new LinkedList<SceneBase>();

		}

		/// <summary>
		/// remove-resume-suspend-addd-update
		/// </summary>
		public void Update()
		{
			while (removeQueue.Any())
			{
				var rem = removeQueue.Dequeue();
				scenes.Remove(rem);
				rem.Dispose();
			}
			if (needResume)
			{
				ResumeAllInnner();
			}
			if (needSuspend)
			{
				SuspendAllInnner();
			}
			while (addQueue.Any())
			{
				scenes.AddFirst(addQueue.Dequeue());
			}
			var first = true;
			foreach (var item in scenes)
			{
				if (!item.IsSuspended)
				{
					item.Update(first);
					if (item.IsDestroyed)
					{
						removeQueue.Enqueue(item);
					}
				}
				first = false;
			}
		}

		/// <summary>
		/// 下のレイヤーも描画する
		/// </summary>
		public void Draw()
		{
			foreach (var item in scenes.Skip(1).Reverse())
			{
				if (!item.IsSuspended)
				{
					item.Draw(false);
				}
			}
			scenes.First().Draw(true);
		}


		public void RemoveAll()
		{
			foreach (var item in scenes)
			{
				item.Exit();
			}
		}

		public void Add(SceneBase scene)
		{
			addQueue.Enqueue(scene);
		}

		public void SuspendAll()
		{
			needSuspend = true;
		}

		void SuspendAllInnner()
		{
			foreach (var item in scenes)
			{
				item.Suspend();
			}
			needSuspend = false;
		}

		public void ResumeAll()
		{
			needResume = true;
		}

		void ResumeAllInnner()
		{
			foreach (var item in scenes)
			{
				item.Resume();
			}
			needResume = false;
		}

		public void Dispose()
		{
			if (addQueue != null)
			{
				foreach (var item in addQueue)
				{
					item.Dispose();
				}
			}
			if (scenes != null)
			{
				foreach (var item in scenes)
				{
					item.Dispose();
				}
			}
			addQueue = null;
			scenes = null;
			removeQueue = null;
			GC.SuppressFinalize(this);
		}

		~SceneManager()
		{
			Dispose();
		}
	}

}
