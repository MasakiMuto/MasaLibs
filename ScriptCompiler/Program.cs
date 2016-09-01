using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using System.Xml.Linq;
using System.Reflection.Emit;

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
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

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
                //ScriptEngine.Compiler.Compile(args[0], Path.GetFileName(args[2]), dict, null, headers);
                Compile(args[0], Path.GetFileName(args[2]), dict, null, headers);
            }
            catch(Exception e)
            {
                Console.Error.WriteLine(Uri.EscapeDataString(e.ToString()));
                return;
            }


            File.Delete(args[2]);
			File.Move(Path.GetFileName(args[2]), args[2]);
            


			OutputDocument(Path.Combine(args[0], @"..\..\doc"));
		}

        static void Compile(string scriptDirectory, string outputFile, Dictionary<string, Type> typeDirectoryDict, string[] labels, Dictionary<string, string> header)
        {
            var name = new AssemblyName(Path.GetFileNameWithoutExtension(outputFile));
            var asm = AppDomain.CurrentDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.Save);
            var module = asm.DefineDynamicModule(name.Name, outputFile);
            NameDataDictionaryGenerator gen = new NameDataDictionaryGenerator();
            foreach (var item in typeDirectoryDict)
            {
                foreach (var file in Directory.EnumerateFiles(Path.Combine(scriptDirectory, item.Key), "*.mss", SearchOption.TopDirectoryOnly))
                {
                    try
                    {
                        var t = ScriptEngine.Compiler.DefineType(module, "Script." + item.Value.Name, file, item.Value, null, labels, header);
                        //BehaviourGenerator.Define(module, item.Value, t);
                        gen.Regist(Path.GetFileNameWithoutExtension(file), t, item.Value);
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Error in " + file, e);
                    }

                }
            }
            gen.Generate(module);
            asm.Save(outputFile);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.Error.WriteLine(Uri.EscapeDataString(e.ExceptionObject.ToString()));
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
