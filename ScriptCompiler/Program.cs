using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masa.ScriptCompiler
{
	class Program
	{
		static void Main(string[] args)
		{
			ScriptEngine.Compiler.Compile(@"D:\projects\OnryoNoita\Assets\Resources\Scripts\enemy", "hoge", typeof(Enemy), null, null, null);
			//ScriptEngine.Compiler.
		}


	}
}
