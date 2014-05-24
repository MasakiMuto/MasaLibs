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
				TypeBuilder typBldr = mdlBldr.DefineType(nameSpace + "." + Path.GetFileNameWithoutExtension(f), TypeAttributes.Public | TypeAttributes.Abstract);
				var scan = new Scanner(File.ReadAllText(f), header);
				var tree = new ExpressionTreeMaker(scan.Tokens.ToArray(), target, table);
				tree.Compile(GetMethodBuilder(typBldr, "main"));
				if (labels != null)
				{
					foreach (var l in labels)
					{
						if (tree.LabelExist(l))
						{
							tree.CompileLabel(l, GetMethodBuilder(typBldr, l));
						}
					}
				}
				int gvn = tree.GlobalVarNumber;
				//System.Linq.Expressions.Expression<Func<int>> gb = () => (gvn);
				//gb.CompileToMethod(GetMethodBuilder(typBldr, "GlobalVarNumber"));
				typBldr.DefineField("GlobalVarNumber", typeof(int), FieldAttributes.Public | FieldAttributes.Static).SetConstant(tree.GlobalVarNumber);
				var type = typBldr.CreateType();
			}

			asmBldr.Save(asmName.Name + ".dll");
		}

		/*
		/// <summary>
		/// 
		/// </summary>
		/// <param name="asm">アセンブリ名</param>
		/// <param name="mdl">モジュール名</param>
		/// <param name="cls">名前空間付きのクラス完全名</param>
		/// <param name="files">スクリプトのパス</param>
		public static void Compile(string asm, string mdl, string cls, Type target, string[] table, string[] labels, string[] files)
		{
			AssemblyName asmName = new AssemblyName(asm);
			AssemblyBuilder asmBldr = AppDomain.CurrentDomain.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.RunAndSave);
			ModuleBuilder mdlBldr = asmBldr.DefineDynamicModule(mdl, asm + ".dll");
			TypeBuilder typBldr = mdlBldr.DefineType(cls, TypeAttributes.Public | TypeAttributes.Abstract);
			foreach (var item in files)
			{
				string baseName = Path.GetFileNameWithoutExtension(item);
				MethodBuilder mtd = GetMethodBuilder(typBldr, baseName);
				var scan = new Scanner(File.ReadAllText(item));
				var tree = new ExpressionTreeMaker(scan.Tokens.ToArray(), target, table);
				tree.Compile(mtd);
				if (labels != null)
				{
					foreach (var l in labels)
					{
						tree.CompileLabel(l, GetMethodBuilder(typBldr, baseName + "_" + l));
					}
				}
				int gvn = tree.GlobalVarNumber;
				System.Linq.Expressions.Expression<Func<int>> gb = () => (gvn);
				gb.CompileToMethod(GetMethodBuilder(typBldr, baseName + "_" + "VarNumber"));
			}
			var type = typBldr.CreateType();
			asmBldr.Save(asmName.Name + ".dll");
		}
		 * */

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
