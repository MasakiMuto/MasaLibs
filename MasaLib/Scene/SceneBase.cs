using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Masa.Lib.Scene
{
	public abstract class SceneBase : IDisposable
	{

		readonly int InputWaitTime = 5;

		/// <summary>
		/// Update数、裏にあっても回る
		/// </summary>
		protected int Count { get; private set; }
		int InputWaitCount { get; set; }
		public bool IsDestroyed { get; private set; }//SceneManagerにより破棄される待ちフラグ
		public bool IsSuspended { get; private set; }//

		

		public SceneBase()
		{

		}

		public virtual void Update(bool isActive)
		{
			Count++;
			if (isActive)
			{
				InputWaitCount++;
				Control();
			}
			else
			{
				InputWaitCount = 0;
			}
		}

		/// <summary>
		/// 決定ボタンを受け入れるか
		/// </summary>
		/// <returns></returns>
		protected bool CanAcceptInput()
		{
			return InputWaitCount >= InputWaitTime;
		}


		public abstract void Control();


		public abstract void Draw(bool isActive);

		public virtual void Suspend()
		{
			Debug.Assert(!IsSuspended);
			IsSuspended = true;
		}

		public virtual void Resume()
		{
			Debug.Assert(IsSuspended);
			IsSuspended = false;
		}

		/// <summary>
		/// 自身を閉じて下のものにActiveを譲る
		/// </summary>
		public virtual void Exit()
		{
			IsDestroyed = true;
		}

		public abstract void Dispose();

		/// <summary>
		/// トップに来たらUpdate前に呼ばれる。Lost前Get後
		/// </summary>
		public virtual void OnGetFocus()
		{
		}

		/// <summary>
		/// トップから外れたらUpdate前に呼ばれる。Lost前Get後
		/// </summary>
		public virtual void OnLostFocus()
		{
		}
	}
}
