﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

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
			UnityScriptDefinition.AppendDefinition();

			var asm = Assembly.Load(File.ReadAllBytes(args[1]));
			var dict = Directory.EnumerateDirectories(args[0], "*", SearchOption.TopDirectoryOnly)
				.Select(x => x.Split('\\').Last())
				.Except(args.Skip(3))
				.Select(x => asm.GetType(x, false, true))
				.Where(x => x != null)
				.ToDictionary(x => x.Name, x => x);

			ScriptEngine.Compiler.Compile(args[0], Path.GetFileName(args[2]), dict, new[] { "init" }, null);

			File.Copy(Path.GetFileName(args[2]), args[2], true);
		}


	}
}
