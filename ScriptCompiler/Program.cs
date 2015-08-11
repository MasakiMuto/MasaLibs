using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using System.Xml.Linq;

namespace Masa.ScriptCompiler
{
	class Program
	{
		/// <summary>
		/// arg0:script directory
		/// arg1:script reference assembly file name
		/// arg2:output file name
		/// arg3...:exclude directorys's name from script directory
		/// </summary>
		/// <param name="args"></param>
		static void Main(string[] args)
		{
			Masa.ScriptEngine.ExpressionTreeMaker.AddDictionarys(UnityScriptDefinition.GetStaticMethodInfo(), UnityScriptDefinition.GetLibraryClassScriptInfo(), null, UnityScriptDefinition.GetTypeNameDictionary());

			var asm = Assembly.Load(File.ReadAllBytes(args[1]));
			var dict = Directory.EnumerateDirectories(args[0], "*", SearchOption.TopDirectoryOnly)
				.Select(x => x.Split('\\').Last())
				.Except(args.Skip(3).Concat(new[] {"header"}))
				.Select(x => asm.GetType(x, false, true))
				.Where(x => x != null)
				.ToDictionary(x => x.Name, x => x);

            Dictionary<string, string> headers = null;
            try
            {
                headers = Directory.EnumerateFiles(Path.Combine(args[0], "header")).Where(x => Path.GetExtension(x).Contains("mss"))
                    .ToDictionary(x => Path.GetFileNameWithoutExtension(x), x => File.ReadAllText(x));
            }
            catch(Exception e)
            {
                Console.Error.WriteLine("header impport error\n" + e);
                return;
            }

            try
            {
                ScriptEngine.Compiler.Compile(args[0], Path.GetFileName(args[2]), dict, null, headers);
            }
            catch(Exception e)
            {
                
                Console.Error.WriteLine(Uri.EscapeDataString(e.ToString()));
                return;
            }
			

			File.Copy(Path.GetFileName(args[2]), args[2], true);


			OutputDocument(Path.Combine(args[0], @"..\..\doc"));
		}

		static void OutputDocument(string dir)
		{
			Func<string, string> fileName = x => Path.Combine(dir, x);
			if (!Directory.Exists(dir))
			{
				Directory.CreateDirectory(dir);
			}
			foreach (var item in ScriptEngine.ExpressionTreeMaker.OutputClassesXml())
			{
				SaveDocument(fileName(item.Key.Name + ".xml"), item.Value);
			}
			SaveDocument(fileName("global.xml"), ScriptEngine.ExpressionTreeMaker.OutputGlobalXml());
			SaveDocument(fileName("list.html"), ScriptEngine.ExpressionTreeMaker.OutputIndex());
			
		}

		static void SaveDocument(string fn, XElement elm)
		{
			var doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), elm);
			doc.Add(new XProcessingInstruction("xml-stylesheet", "type='text/css' href='doc.css'"));
			doc.Save(fn);
		}


	}
}
