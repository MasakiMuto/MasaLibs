using System;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;
using ScriptMethod = System.Action<Masa.ScriptEngine.Environment>;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Masa.ScriptEngine
{
	/// <summary>
	/// スクリプトファイルをアセンブリにコンパイルする
	/// </summary>
	public static class Compiler
	{
		public static void Compile(string asm, string mdl, string nameSpace, Type target, string[] table, string[] labels, string[] files, Dictionary<string, string> header)
		{
			AssemblyName asmName = new AssemblyName(asm);
			AssemblyBuilder asmBldr = AppDomain.CurrentDomain.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.RunAndSave);
			ModuleBuilder mdlBldr = asmBldr.DefineDynamicModule(mdl, asmName.Name + ".dll");
			foreach (var f in files)
			{
				DefineType(mdlBldr, nameSpace, f, target, table, labels, header);
			}

			asmBldr.Save(asmName.Name + ".dll");
		}

		static void DefineType(ModuleBuilder module, string nameSpace, string scriptFileName, Type targetType, string[] table, string[] labels, Dictionary<string, string> header)
		{
			var builder = module.DefineType(nameSpace + "." + Path.GetFileNameWithoutExtension(scriptFileName), TypeAttributes.Public | TypeAttributes.Abstract);
			var scan = new Scanner(File.ReadAllText(scriptFileName), header);
			var tree = new ExpressionTreeMaker(scan.Tokens.ToArray(), targetType, table, false);
			tree.Compile(GetMethodBuilder(builder, "main"));
            foreach (var item in tree.EnumrateLabels())
            {
                tree.CompileLabel(item, GetMethodBuilder(builder, "label_" + item));
            }
			//if (labels != null)
			//{
			//	foreach (var l in labels)
			//	{
			//		if (tree.LabelExist(l))
			//		{
			//			tree.CompileLabel(l, GetMethodBuilder(builder, l));
			//		}
			//	}
			//}
			tree.CompileCoroutineNames(builder.DefineMethod("GetCoroutineNames", MethodAttributes.Public | MethodAttributes.Static, typeof(string[]), Type.EmptyTypes));
			int i = 0;
			foreach (var coroutine in tree.CoroutineDict)
			{
				i = 0;
				foreach (var item in coroutine.Value)
				{
					tree.CompileCoroutineTask(GetMethodBuilder(builder, "coroutine_" + coroutine.Key + "_" + i), item);
					i++;
				}
			}
			builder.DefineField("GlobalVarNumber", typeof(int), FieldAttributes.Public | FieldAttributes.Static).SetConstant(tree.GlobalVarNumber);
			builder.CreateType();
			
		}

		

		static MethodBuilder GetMethodBuilder(TypeBuilder tp, string name)
		{
			return tp.DefineMethod(name, MethodAttributes.Static | MethodAttributes.Public, typeof(void), new[] { typeof(Environment) });
		}

		/// <summary>
		/// 指定されたディレクトリのスクリプトを一括コンパイルして固める。
		/// 出力はカレントディレクトリに"ディレクトリ名.dll"という形で出力
		/// 出力クラスはScript.(targetの型名).(fileName)
		/// メソッド名はファイル名に一致
		/// </summary>
		/// <param name="directry"></param>
		/// <param name="target"></param>
		/// <param name="table"></param>
		public static void Compile(string directry, string name, Type target, string[] table, string[] labels, Dictionary<string, string> header)
		{
			Compile(name, name, "Script." + target.Name, target, table, labels, Directory.EnumerateFiles(directry, "*.mss", SearchOption.TopDirectoryOnly).ToArray(), header);
			//Compiler.Compile(directry, directry, "Script." + target.Name + "." + , target, table, labels, Directory.EnumerateFiles(directry, "*.msc", SearchOption.TopDirectoryOnly).ToArray());
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="scriptDirectory">スクリプトのルートディレクトリ</param>
		/// <param name="outputFile">アセンブリのファイル名(パスを含まない)</param>
		/// <param name="typeDirectoryDict">ディレクトリ名と型名の対応辞書</param>
		/// <param name="labels">init込みのラベルリスト</param>
		/// <param name="header">拡張子抜きファイル名とその中身のテキストの辞書</param>
		public static void Compile(string scriptDirectory, string outputFile, Dictionary<string, Type> typeDirectoryDict, string[] labels, Dictionary<string, string> header)
		{
			var name = new AssemblyName(Path.GetFileNameWithoutExtension(outputFile));
			var asm = AppDomain.CurrentDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.Save);
			var module = asm.DefineDynamicModule(name.Name, outputFile);
			foreach (var item in typeDirectoryDict)
			{
				foreach (var file in Directory.EnumerateFiles(Path.Combine(scriptDirectory, item.Key), "*.mss", SearchOption.TopDirectoryOnly))
				{
					try
					{
						DefineType(module, "Script." + item.Value.Name, file, item.Value, null, labels, header);
					}
					catch(Exception e)
					{
						throw new Exception("Error in " + file, e);
					}
					
				}
			}
			asm.Save(outputFile);
		}
	}

	public class CompiledScriptData : ScriptDataBase
	{
		public CompiledScriptData(Type compiledType, Type targetType)
			: base(targetType)
		{
			Tree = new CompiledExpressionTree(compiledType);
		}
	}

	class CompiledExpressionTree : IExpressionTreeMaker
	{
		readonly Type compiledType;

		public CompiledExpressionTree(Type compiledType)
		{
			this.compiledType = compiledType;
			GlobalVarNumber = (int)compiledType.GetMethod("GlobalVarNumber").Invoke(null, null);
			Statement = GetMethod("ScriptMain");
			InitStatement = GetMethod("init");
			if (InitStatement == null)
			{
				InitStatement = x => { };
			}

		}

		ScriptMethod GetMethod(string name)
		{
			return Delegate.CreateDelegate(typeof(ScriptMethod), compiledType, name, false, false) as ScriptMethod;
		}

		public int GlobalVarNumber
		{
			get;
			private set;
		}

		public ScriptMethod Statement
		{
			get;
			private set;
		}

		public ScriptMethod InitStatement
		{
			get;
			private set;
		}

		public ScriptMethod GetLabelStatement(string label)
		{
			return GetMethod(label);
		}

		public string OutputClassInformation()
		{
			throw new NotImplementedException();
		}
	}
}
