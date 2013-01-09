using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Masa.ScriptEngine
{
	public class PackedScriptManager
	{
		public Dictionary<string, string> Files { get; private set; }

		public PackedScriptManager(string fileName)
		{
			UnpackScript(fileName);
		}

		void UnpackScript(string file)
		{
			var format = new BinaryFormatter();
			Files = new Dictionary<string, string>();

			using (var f = File.OpenRead(file))
			{
				var tmp = (Dictionary<string, string>)format.Deserialize(f);
				foreach (var item in tmp.Keys)
				{
					Files[item] = Decrypt(tmp[item]);
				}
			}

		}

		public bool ContainsKey(string name)
		{
			return Files.ContainsKey(name);
		}

		static string Encrypt(string code)
		{
			return new string(code.Select(c => (char)(c + 1)).ToArray());
		}

		static string Decrypt(string code)
		{
			return new string(code.Select(c => (char)(c - 1)).ToArray());
		}

		/// <summary>
		/// 指定パス以下の*.mssファイルを全てパッキングする。パックの中のキーは指定パスからの相対パス
		/// </summary>
		/// <param name="output">出力ファイル名</param>
		/// <param name="directory">パックするスクリプトファイルを格納しているディレクトリ</param>
		public static void PackScript(string output, params string[] directory)
		{
			PackScript(output, s => s, directory);
		}

		/// <summary>
		/// 指定パス以下の*.mssファイルを全てパッキングする。パックの中のキーは指定パスからの相対パス(script\hoge\piyo.mssならhoge\piyo.mss)
		/// </summary>
		/// <param name="output">出力ファイル名</param>
		/// <param name="codeMapper">スクリプトファイルの生テキストから実際に使用するもの文字列への変換関数。プリプロセッサなど</param>
		/// <param name="directory">パックするスクリプトファイルを格納しているディレクトリ</param>
		public static void PackScript(string output, Func<string, string> codeMapper, params string[] directory)
		{
			var dict = new Dictionary<string, string>();

			foreach (var dir in directory)
			{
				string abs = Path.GetFullPath(dir);
				foreach (var item in Directory.EnumerateFiles(dir, "*.mss", SearchOption.AllDirectories))
				{
					dict.Add(Path.GetFullPath(item).Substring(abs.Length + 1).ToLower(), Encrypt(codeMapper(File.ReadAllText(item))));
				}
			}
			var format = new BinaryFormatter();
			using (var file = File.OpenWrite(output))
			{
				format.Serialize(file, dict);
			}
		}
	}

	public class PackedScriptData : ScriptData
	{
		string Name;
		string Code;
		public PackedScriptData(string name, string code, Type target, string[] table = null, string[] labels = null)
			: base(target, table, labels)
		{
			Name = name;
			Code = code;
			//Load();
		}

		public override void Load()
		{
			Load(Code, Name);
		}
	}
}
