using System;
using System.Collections.Generic;
using System.Linq;
using SharpDX.DirectInput;

namespace Masa.Lib.XNA.Input
{
	/// <summary>
	/// デバイスに直接対応するもの。DirectInputを使う。
	/// </summary>
	internal class GamePadDevice : IDisposable
	{
		Joystick Stick { get; set; }

		static IList<DeviceInstance> GetDevices(DirectInput directInput)
		{
			return directInput.GetDevices(DeviceClass.GameControl, DeviceEnumerationFlags.AttachedOnly);
		}

		/// <summary>
		/// 接続されているパッドの数を取得する
		/// </summary>
		/// <returns></returns>
		public static int GetPadNumber(DirectInput directInput)
		{
			return GetDevices(directInput).Count;
		}

		/// <summary>
		/// 複数のゲームパッドのボタン入力の和を取る
		/// </summary>
		/// <param name="pads"></param>
		/// <returns>各ボタンが押されている/いないの配列。状態をとれるパッドが存在しない場合はnull</returns>
		public static bool[] ZipPadsStates(GamePadDevice[] pads)
		{
			var states = pads.Select(p => p.GetState()).Where(s => s != null);
			if (states.Any())
			{
				bool[] bt = states.Select(s => s.Buttons)
					.Aggregate((a, b) => a.Zip(b, (x, y) => x || y).ToArray()).ToArray();//複数パッドを合成
				return bt;
			}
			else return null;
		}

		/// <summary>
		/// 番号指定でデバイス生成。番号のデバイスが存在しなければ例外を投げる。
		/// </summary>
		/// <param name="window"></param>
		/// <param name="padNumber">0始まり</param>
		public GamePadDevice(int padNumber, DirectInput directInput)
		{
			var devices = GetDevices(directInput);
			if (devices.Count <= padNumber)
			{
				throw new Exception("指定された数のパッドがつながれていない");
			}
			try
			{
				Stick = new Joystick(directInput, devices[padNumber].InstanceGuid);
				//Stick.SetCooperativeLevel(window, CooperativeLevel.Exclusive | CooperativeLevel.Foreground);
			}
			catch (Exception e)
			{
				throw new Exception("パッドの初期化に失敗", e);
			}

			///スティックの範囲設定
			foreach (var item in Stick.GetObjects())
			{
				//if ((item.ObjectType & .Axis) != 0)
				//{
				//	Stick.GetObjectPropertiesById((int)item.ObjectType).SetRange(-1000, 1000);
				//}

			}
			Stick.Acquire();
		}

		/// <summary>
		/// ゲームパッドのデバイスを生成。1番目のパッドを使用。
		/// </summary>
		/// <param name="window"></param>
		public GamePadDevice(DirectInput directInput)
			: this(0, directInput)
		{

		}

		public JoystickState GetState()
		{
			if (Stick == null)
			{
				return null;
			}
			Stick.Acquire();
			Stick.Poll();
			//if (Stick.Acquire().IsFailure)
			//{
			//	return null;
			//}
			//if (Stick.Poll().IsFailure)
			//{
			//	throw new Exception();
			//}

			return Stick.GetCurrentState();
		}

		public string GetDeviceName()
		{
			return Stick.Information.InstanceName;
		}

		public void Dispose()
		{
			if (Stick != null)
			{
				try
				{
					Stick.Unacquire();
					Stick.Dispose();
				}
				catch { }
				finally
				{
					Stick = null;
				}
			}
			GC.SuppressFinalize(this);
		}

		~GamePadDevice()
		{
			this.Dispose();
		}
	}
}
