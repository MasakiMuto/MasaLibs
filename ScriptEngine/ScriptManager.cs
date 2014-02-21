using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Xml.Linq;

namespace Masa.ScriptEngine
{
	using TypeScriptDictionary = Dictionary<string, ScriptDataBase>;

	/// <summary>
	/// ScriptDataをさらにラップしたクラス。エラーログ出力機能付き
	/// </summary>
	public class ScriptManager
	{
		protected Dictionary<Type, TypeScriptDictionary> scriptItems;
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
				//rootDirectory = Path.Combine(value, "");
				if (!(value.LastOrDefault() == Path.DirectorySeparatorChar || value.LastOrDefault() == Path.AltDirectorySeparatorChar))
				{
					rootDirectory = value + Path.DirectorySeparatorChar;
				}
				else
				{
					rootDirectory = value;
				}
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
			scriptItems = new Dictionary<Type, TypeScriptDictionary>();
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
			//try
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
					data = new FileScriptData(p, target, CodeMapper, table, labels)
					{
						HeaderDictionary = this.HeaderDictionary
					};
				}
				data.Load();

			}
			//catch (Exception e)
			//{
			//	WriteLog(e.Message);
			//	throw;
			//}

			//if (items.ContainsKey(key))
			//{
			//	string msg = fileName + ":同じKeyのスクリプトがすでに読み込まれている";
			//	WriteLog(msg);
			//	//throw new Exception(msg);

			//}
			TypeScriptDictionary dict;
			if (!scriptItems.TryGetValue(target, out dict))
			{
				dict = new TypeScriptDictionary();
				scriptItems[target] = dict;
			}
			dict[key] = data;
			//items[target][key] = data;

		}

		/// <summary>
		/// root\dirのディレクトリ(当該ディレクトリひとつだけ)が含む*.mssファイルを全て読み込む
		/// </summary>
		/// <param name="dir"></param>
		/// <param name="target"></param>
		/// <param name="table"></param>
		/// <param name="labels">init以外のlabelをActionとして使う場合は指定すること推奨.initは標準で読み込まれる</param>
		public void LoadFromDirectory(string dir, Type target, string[] table = null, string[] labels = null)
		{
			var src = EnumrateDirectoryScript(dir);

			//dir = dir.ToLower();
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
		/// <returns>hoge\file.mss</returns>
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
				return Directory.EnumerateFiles(GetScriptDirectory(directory), "*.mss", SearchOption.TopDirectoryOnly)
					.Where(x=>Path.GetExtension(x).Length == 4)//.mss_を列挙しない
					.Select(i => Path.GetFullPath(i).Substring(FullRoot.Length));
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
				scriptItems
					.SelectMany(pair => pair.Value.Values)
					.OfType<FileScriptData>()
					.AsParallel()
					.ForAll(s => s.Load());

				//items
				//	.Where(i => i.Value is FileScriptData)
				//	.Select(i => (FileScriptData)i.Value)
				//	.AsParallel()
				//	.ForAll(s => s.Load());

				//items.AsParallel().ForAll(p => p.Value.Load());
			}
			catch (Exception e)
			{
				WriteLog(e.Message);
				throw;
			}
		}

		/*
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
		 * */

		public ScriptDataBase GetScriptData(object target, string key)
		{

			ScriptDataBase data;
			TypeScriptDictionary dict;
			Type type = target.GetType();
			if (scriptItems.TryGetValue(type, out dict))
			{
				if (dict.TryGetValue(key, out data))
				{
					return data;
				}
				else
				{
					throw new Exception(type.ToString() + "のスクリプト " + key + "は読み込まれていない");
				}
			}
			else
			{
				throw new Exception(type.ToString() + "のスクリプトが存在しない");
			}

		}

		public ScriptRunner GetScript(object target, string key)
		{
			return GetScriptData(target, key).GetScriptRunner(target);
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

		public void OutputDocumentText(string fileName)
		{
			var str = new StringBuilder();
			foreach (var item in scriptItems)
			{
				str.AppendLine("*" + item.Key.ToString());
				str.Append(item.Value.First().Value.GetDocument());
				str.AppendLine("-----");
				str.AppendLine();
			}
			File.WriteAllText(fileName, str.ToString());
		}

		public void OutputDocument(string fileName)
		{
			//<?xml-stylesheet href="doc.css" type="text/css"?>
			var doc = new XDocument();
			doc.Add(new XProcessingInstruction("xml-stylesheet", "type='text/css' href='doc.css'"));
			doc.Add(new XElement("document",
				scriptItems.Values.Select(d => (d.First().Value.Tree as ExpressionTreeMaker).OutputClassXml()).ToArray()
				));
			// .OfType<ScriptData>().Select(x => (x.Tree as ExpressionTreeMaker).OutputClassXml()).ToArray()));
			doc.Save(fileName);
		}

		public void OutputDocumentsByClass(string outputDirectory)
		{
			if (!Directory.Exists(outputDirectory))
			{
				Directory.CreateDirectory(outputDirectory);
			}
			Func<string, string> path = name=>Path.Combine(outputDirectory, name + ".xml");
			//foreach (var item in GetTypeScripts())
			//{
			//	SaveXmlDocument(path(item.TargetType.Name), item.OutputClassXml());
			//}
			foreach (var item in ExpressionTreeMaker.OutputClassesXml())
			{
				SaveXmlDocument(path(item.Key.Name), item.Value);
			}
			SaveXmlDocument(path("global"), ExpressionTreeMaker.OutputGlobalXml());
			SaveList(Path.Combine(outputDirectory, "list.html"), ExpressionTreeMaker.OutputIndex());
		}

		static void SaveList(string outputName, XElement body)
		{
			var doc = new XDocument();
			doc.Add(body);
			doc.Save(outputName);
		}

		static void SaveXmlDocument(string outputName, XElement body)
		{
			var doc = new XDocument();
			doc.Add(new XProcessingInstruction("xml-stylesheet", "type='text/css' href='doc.css'"));
			doc.Add(new XElement("document", body));
			doc.Save(outputName);
		}

		/// <summary>
		/// 読み込み済みのスクリプトをそれぞれの型につき1つずつ返す
		/// </summary>
		/// <returns></returns>
		IEnumerable<ExpressionTreeMaker> GetTypeScripts()
		{
			return scriptItems.Values.Select(i => i.First().Value.Tree as ExpressionTreeMaker);
		}


	}
}
