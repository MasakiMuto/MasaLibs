using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Masa.Lib.Scene
{
	public class SceneManager : IDisposable
	{
		protected LinkedList<SceneBase> Scenes { get; private set; }
		Queue<SceneBase> addQueue, removeQueue;
		bool needSuspend, needResume;

		public SceneManager()
		{
			addQueue = new Queue<SceneBase>();
			removeQueue = new Queue<SceneBase>();
			Scenes = new LinkedList<SceneBase>();

		}

		/// <summary>
		/// remove-resume-suspend-addd-update
		/// </summary>
		public void Update()
		{
			var lastItem = Scenes.First;
			while (removeQueue.Any())
			{
				var rem = removeQueue.Dequeue();
				Scenes.Remove(rem);
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
				Scenes.AddFirst(addQueue.Dequeue());
			}
			var firstItem = Scenes.First;
			if (lastItem != firstItem)
			{
				if (lastItem != null)
				{
					lastItem.Value.OnLostFocus();
				}
				if (firstItem != null)
				{
					firstItem.Value.OnGetFocus();
				}
			}
			var first = true;
			foreach (var item in Scenes)
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
		public virtual void Draw()
		{
			foreach (var item in Scenes.Skip(1).Reverse())
			{
				if (!item.IsSuspended)
				{
					item.Draw(false);
				}
			}
			Scenes.First().Draw(true);
		}


		public void RemoveAll()
		{
			foreach (var item in Scenes)
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
			foreach (var item in Scenes)
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
			foreach (var item in Scenes)
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
			if (Scenes != null)
			{
				foreach (var item in Scenes)
				{
					item.Dispose();
				}
			}
			addQueue = null;
			Scenes = null;
			removeQueue = null;
			GC.SuppressFinalize(this);
		}

		~SceneManager()
		{
			Dispose();
		}

		public bool Any()
		{
			return Scenes.Any() || addQueue.Any();
		}

		/// <summary>
		/// ある型のシーンが存在もしくは追加キューに入っているか
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public bool AnyOfType<T>()
		{
			return Scenes.Any(x => x is T) || addQueue.Any(x => x is T);
		}
	}

}
