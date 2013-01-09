using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Reflection;

namespace Masa.ScriptEngine
{
	/// <summary>
	/// ScriptDataをさらにラップしたクラス。エラーログ出力機能付き
	/// </summary>
	public class ScriptManager
	{
		protected Dictionary<string, ScriptDataBase> items;
		public Func<string, string> PathToKey;
		//List<CompiledExpressionTree> CompiledTrees;
		Dictionary<string, System.Reflection.Assembly> assemblyDict;
		public string ErrorLogFileName
		{
			get;
			set;
		}
		PackedScriptManager PackedData;
		bool UsePackedData;

		string rootDirectory;
		/// <summary>
		/// ファイルシステムから直接スクリプトを読むときのroot
		/// </summary>
		public string RootDirectory
		{
			get { return rootDirectory; }
			set
			{
				rootDirectory = Path.Combine(value, "");
			}
		}
		string FullRoot
		{
			get { return Path.GetFullPath(rootDirectory); }
		}

		/// <summary>
		/// 非Packファイルからスクリプトを読む時の文字列変換関数
		/// </summary>
		public Func<string, string> CodeMapper { get; set; }

		Dictionary<string, string> HeaderDictionary;

		/// <summary>
		/// Rootと結合したパスを返す
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		string GetScriptDirectory(string path)
		{
			return Path.Combine(RootDirectory, path);
		}

		public ScriptManager()
		{
			CodeMapper = s => s;
			items = new Dictionary<string, ScriptDataBase>();
			//CompiledTrees = new List<CompiledExpressionTree>();
			assemblyDict = new Dictionary<string, System.Reflection.Assembly>();
			PathToKey = s => Path.GetFileNameWithoutExtension(s);
			ErrorLogFileName = "log.txt";
			RootDirectory = "";
		}

		public ScriptManager(string rootDirectory)
			: this()
		{
			RootDirectory = rootDirectory;
		}

		public void LoadHeaders(string directory)
		{
			if (UsePackedData)
			{
				HeaderDictionary = PackedData.Files
					.Where(g => Path.GetDirectoryName(g.Key) == directory)
					.ToDictionary(g => this.PathToKey(g.Key), g => g.Value);
			}
			else
			{
				HeaderDictionary = this.EnumrateDirectoryScript(directory)
					.ToDictionary(x => Path.GetFileNameWithoutExtension(x), x => File.ReadAllText(RootDirectory + x));
			}
		}

		public void LoadPackedScript(string fileName)
		{
			PackedData = new PackedScriptManager(fileName);
			UsePackedData = true;
		}

		/// <summary>
		/// スクリプト読み込み。失敗するとエラーログに記録して、その例外を投げる
		/// </summary>
		/// <param name="fileName">スクリプトファイル名</param>
		/// <param name="target">対象の型</param>
		/// <param name="table"></param>
		/// <param name="labels"></param>
		public void Load(string fileName, Type target, string[] table = null, string[] labels = null)
		{
			ScriptData data;
			string key = PathToKey(fileName);
			try
			{
				if (UsePackedData)
				{
					data = new PackedScriptData(fileName, PackedData.Files[fileName], target, table, labels)
					{
						HeaderDictionary = this.HeaderDictionary
					};
				}
				else
				{
					string p = Path.Combine(RootDirectory, fileName);
					p = RootDirectory + fileName;
					data = new FileScriptData(p, target, CodeMapper, table, labels)
					{
						HeaderDictionary = this.HeaderDictionary
					};
				}
			}
			catch (Exception e)
			{
				WriteLog(e.Message);
				throw;
			}
			//if (items.ContainsKey(key))
			//{
			//	string msg = fileName + ":同じKeyのスクリプトがすでに読み込まれている";
			//	WriteLog(msg);
			//	//throw new Exception(msg);

			//}
			items[key] = data;

		}

		/// <summary>
		/// root\dirのディレクトリ(当該ディレクトリひとつだけ)が含む*.mssファイルを全て読み込む
		/// </summary>
		/// <param name="dir"></param>
		/// <param name="target"></param>
		/// <param name="table"></param>
		/// <param name="labels"></param>
		public void LoadFromDirectory(string dir, Type target, string[] table = null, string[] labels = null)
		{
			IEnumerable<string> src = EnumrateDirectoryScript(dir);
			dir = dir.ToLower();
			//if (UsePackedData)
			//{
			//    var d = dir.Split('\\');
			//    src = PackedData.Files.Select(i => i.Key).Where(i => 
			//        {
			//            var sp = i.Split('\\');
			//            return (d.Length + 1 == sp.Length && d.All(pt=>sp.Contains(pt)));
			//        });//奥のディレクトリまでは取らない
			//}
			//else
			//{
			//    src = EnumrateDirectoryScript(dir);
			//}
			foreach (var item in src)
			{
				Load(item, target, table, labels);
			}
		}

#if ASSEMBLY

		public void LoadFromAssembly(string assembly, Type target)
		{
			//var trees = CompiledExpressionTree.LoadAll(System.Reflection.Assembly.LoadFile(assembly));
			Assembly asm;
			if (assemblyDict.ContainsKey(assembly))
			{
				asm = assemblyDict[assembly];
			}
			else
			{
				asm = LoadAssembly(assembly);
			}
			var trees = CompiledExpressionTree.LoadAll(asm);
			//CompiledTrees.AddRange());
			foreach (var item in trees)
			{
				items.Add(item.FileName, new CompiledScriptData(target, item));
			}
		}

#endif

		public string[] GetScriptNames(string assembly)
		{
			if (!assemblyDict.ContainsKey(assembly))
			{
				LoadAssembly(assembly);
			}
			return assemblyDict[assembly].GetExportedTypes().Select(t => t.Name).ToArray();
			//System.Reflection.Assembly.
		}

		/// <summary>
		/// Root\\directoryにあるスクリプトファイルを列挙
		/// </summary>
		/// <param name="directory"></param>
		/// <param name="option"></param>
		/// <returns></returns>
		public IEnumerable<string> EnumrateDirectoryScript(string directory)
		{
			directory = directory.ToLower();
			if (UsePackedData)
			{
				var d = directory.Split('\\');
				return PackedData.Files.Select(i => i.Key).Where(i =>
				{
					var sp = i.Split('\\');
					return (d.Length + 1 == sp.Length && d.All(pt => sp.Contains(pt)));
				});//奥のディレクトリまでは取らない
			}
			else
			{
				return Directory.EnumerateFiles(GetScriptDirectory(directory), "*.mss", SearchOption.TopDirectoryOnly).Select(i => Path.GetFullPath(i).Substring(FullRoot.Length));
			}

		}

		/// <summary>
		/// Root\\directoryにあるスクリプトファイルのKey(拡張子抜きファイル名)を列挙
		/// </summary>
		/// <param name="directory"></param>
		/// <param name="option"></param>
		/// <returns></returns>
		public IEnumerable<string> EnumrateDirectoryScriptByKey(string directory)
		{
			return EnumrateDirectoryScript(directory).Select(i => Path.GetFileNameWithoutExtension(i));
		}

		Assembly LoadAssembly(string asm)
		{
			var a = System.Reflection.Assembly.LoadFrom(asm);
			assemblyDict.Add(asm, a);
			return a;
		}

		public void Reload()
		{
			try
			{
				items
					.Where(i => i.Value is FileScriptData)
					.Select(i => (FileScriptData)i.Value)
					.AsParallel()
					.ForAll(s => s.Load());
				//items.AsParallel().ForAll(p => p.Value.Load());
			}
			catch (Exception e)
			{
				WriteLog(e.Message);
				throw;
			}
		}

		/// <summary>
		/// スクリプトを破棄する。指定したスクリプトが存在しなくても問題は起こさない
		/// </summary>
		/// <param name="name"></param>
		public void DeleteScript(string name)
		{
			if (items.ContainsKey(name))
			{
				items.Remove(name);
			}
		}

		ScriptDataBase GetScriptData(string key)
		{
			ScriptDataBase data;
			if (items.TryGetValue(key, out data))
			{
				return data;
			}
			throw new Exception("スクリプト " + key + "は読み込まれていない");
		}

		public ScriptRunner GetScript(object target, string key)
		{
			return GetScriptData(key).GetScriptRunner(target);
		}

		public List<string> GetLiteralTable(string key)
		{
			return GetScriptData(key).StringLiterals;
		}

		void WriteLog(string txt)
		{
			//return;
			try
			{
				File.AppendAllText(ErrorLogFileName, txt + "\n");
			}
			catch
			{

			}
		}

		public void OutputDocument(string fileName)
		{
			var str = new StringBuilder();
			foreach (var item in items.GroupBy(i => i.Value.TargetType))
			{
				str.AppendLine("*" + item.Key.ToString());
				str.Append(item.First().Value.GetDocument());
				str.AppendLine("-----");
				str.AppendLine();
			}
			File.WriteAllText(fileName, str.ToString());
		}
	}
}
