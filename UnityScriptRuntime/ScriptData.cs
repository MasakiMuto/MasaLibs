using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptEnvironment = Masa.ScriptEngine.Environment;
using ScriptMethod = System.Action<Masa.ScriptEngine.Environment>;

namespace UnityScriptRuntime
{
    public class ScriptData
    {
        public ScriptMethod Main, Init;
        public int GlobalVarNumber;
        public Dictionary<string, List<Action<ScriptEnvironment>>> CoroutineDict;
        public Dictionary<string, ScriptMethod> Labels;
    }
}
