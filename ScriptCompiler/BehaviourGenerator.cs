using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq.Expressions;

namespace Masa.ScriptCompiler
{
    using EnvScript = Masa.ScriptEngine.Environment;
    public static class BehaviourGenerator
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="module"></param>
        /// <param name="targetType">Enemyなど</param>
        /// <param name="containerType">Antなど</param>
        public static Type Define(ModuleBuilder module, Type targetType, Type containerType)
        {
            var builder = module.DefineType(GetBehaviourName(targetType, containerType), TypeAttributes.Public, typeof(UnityEngine.MonoBehaviour));
            builder.DefineField("env", typeof(EnvScript), FieldAttributes.Public);
            var thisParam = Expression.Parameter(builder.AsType());
            var envConstructor = typeof(EnvScript).GetConstructor(new[] { typeof(object), typeof(int) });
            var getCompMethod = typeof(UnityEngine.MonoBehaviour).GetMethod("GetComponent", new[] { typeof(Type) });
            var targetObject = Expression.Call(thisParam, typeof(UnityEngine.MonoBehaviour).GetMethod("GetComponent", new[] { typeof(Type) }), Expression.Constant(targetType));
            var varNumber = Expression.Field(null, containerType.GetField("GlobalVarNumber"));
            var envObject = Expression.New(envConstructor, targetObject, varNumber);

            //var envField = Expression.Field(thisParam, "env");
            var start = builder.DefineMethod("Start", MethodAttributes.Public).GetILGenerator();
            var initMethod = containerType.GetMethod("label_init");
            //new Environment(GetComponent(targetType), containerType.GlobalVarNumber?)
            //start.Emit(OpCodes.Ldarg_0);
            //start.EmitCall(OpCodes.Call, getCompMethod, null);


            //var startTree = Expression.Lambda(Expression.Block(
                //Expression.Empty()
                //Expression.Assign(envField, envObject),
                //initMethod != null ? Expression.Call(null, initMethod, envField) as Expression : Expression.Empty()
                //));
            //startTree.CompileToMethod(start);

            var update = builder.DefineMethod("FixedUpdate", MethodAttributes.Public);

            //var updateTree = Expression.Lambda(Expression.Block(
            //    Expression.Call(envField, typeof(EnvScript).GetMethod("FrameUpdate")),
            //    Expression.Call(null, containerType.GetMethod("main"), envField)
            //    ), thisParam);
            //updateTree.CompileToMethod(update);

            return builder.CreateType();
        }

        static string GetBehaviourName(Type targetType, Type containerType)
        {
            return "Script" + targetType.Name + containerType.Name + "Behaviour";
        }
    }

}
