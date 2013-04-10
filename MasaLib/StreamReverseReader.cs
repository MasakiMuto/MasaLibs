using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Masa.Lib
{
	public static class StreamReverseReader
	{
		/// <summary>
		/// 現在位置の一つ前に書かれているintを読む。読んだ後は現在地から4バイト前に戻る
		/// </summary>
		/// <param name="reader"></param>
		/// <returns></returns>
		public static int ReadInt32Reverse(this BinaryReader reader)
		{
			reader.BaseStream.Seek(-sizeof(int), SeekOrigin.Current);
			var val = reader.ReadInt32();
			reader.BaseStream.Seek(-sizeof(int), SeekOrigin.Current);
			return val;
		}

		/// <summary>
		/// 現在位置の一つ前に書かれているshortを読む。読んだ後は現在地から2バイト前に戻る
		/// </summary>
		/// <param name="reader"></param>
		/// <returns></returns>
		public static short ReadInt16Reverse(this BinaryReader reader)
		{
			reader.BaseStream.Seek(-sizeof(short), SeekOrigin.Current);
			var val = reader.ReadInt16();
			reader.BaseStream.Seek(-sizeof(short), SeekOrigin.Current);
			return val;
		}

		/// <summary>
		/// 現在位置の一つ前に書かれているfloatを読む。読んだ後は現在地から4バイト前に戻る
		/// </summary>
		/// <param name="reader"></param>
		/// <returns></returns>
		public static float ReadSingleReverse(this BinaryReader reader)
		{
			reader.BaseStream.Seek(-sizeof(float), SeekOrigin.Current);
			var val = reader.ReadSingle();
			reader.BaseStream.Seek(-sizeof(float), SeekOrigin.Current);
			return val;
		}

		/// <summary>
		/// 現在位置の一つ前に書かれているdoubleを読む。読んだ後は現在地から8バイト前に戻る
		/// </summary>
		/// <param name="reader"></param>
		/// <returns></returns>
		public static double ReadDoubleReverse(this BinaryReader reader)
		{
			reader.BaseStream.Seek(-sizeof(double), SeekOrigin.Current);
			var val = reader.ReadDouble();
			reader.BaseStream.Seek(-sizeof(double), SeekOrigin.Current);
			return val;
		}

		/// <summary>
		/// 現在位置の一つ前に書かれているbyteを読む。読んだ後は現在地から1バイト前に戻る
		/// </summary>
		/// <param name="reader"></param>
		/// <returns></returns>
		public static byte ReadByteReverse(this BinaryReader reader)
		{
			reader.BaseStream.Seek(-sizeof(byte), SeekOrigin.Current);
			var val = reader.ReadByte();
			reader.BaseStream.Seek(-sizeof(byte), SeekOrigin.Current);
			return val;
		}

		
	}
}
