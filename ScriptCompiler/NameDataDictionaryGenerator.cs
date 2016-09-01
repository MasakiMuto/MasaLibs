using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityScriptRuntime;
using System.Reflection;
using System.Linq.Expressions;
using ScriptMethod = System.Action<Masa.ScriptEngine.Environment>;

namespace Masa.ScriptCompiler
{
    public class NameDataDictionaryGenerator
    {
        Dictionary<string, ScriptData> data;
        Dictionary<string, Type> names;

        public NameDataDictionaryGenerator()
        {
            names = new Dictionary<string, Type>();
            data = new Dictionary<string, ScriptData>();
        }

        public ScriptData Regist(string name, Type containerType, Type targetType)
        {
            var fullName = targetType.Name + name;
            names[fullName] = containerType;
            return null;
            var d = new ScriptData();

            d.GlobalVarNumber = (int)containerType.GetField("GlobalVarNumber").GetRawConstantValue();
            d.Main = CreateDelegate(containerType.GetMethod("main"));
            d.Init = CreateDelegate(containerType.GetMethod("label_init"));
            d.Labels = containerType.GetMethods().Where(x => x.Name.StartsWith("label_"))
                .ToDictionary(x => x.Name.Substring(6), x => CreateDelegate(x));


            var coroutines = containerType.GetMethod("GetCoroutineNames").Invoke(null, null) as string[];
            d.CoroutineDict = new Dictionary<string, List<ScriptMethod>>();
            foreach (var c in coroutines)
            {
                var list = new List<ScriptMethod>();
                d.CoroutineDict[c] = list;
                int i = 0;
                while (true)
                {
                    var method = containerType.GetMethod("coroutine_" + c + "_" + i);
                    if (method == null)
                    {
                        break;
                    }
                    list.Add(CreateDelegate(method));
                    i++;
                }
            }
            data[fullName] = d;
            return d;
        }

        ScriptMethod CreateDelegate(MethodInfo method)
        {
            if (method == null)
            {
                return null;
            }
            return (ScriptMethod)Delegate.CreateDelegate(typeof(ScriptMethod), method);
        }

        //NameDataDictionary.Get(string) -> ScriptData (ex Attachable.Ant -> Attachableant
        //Cache(Type) -> ScriptData
        //dict<string, ScriptData>
        public Type Generate(ModuleBuilder module)
        {
            var type = module.DefineType("NameDataDictionary", TypeAttributes.Public);
            var dictType = typeof(Dictionary<string, ScriptData>);
            /*
            var method = type.DefineMethod("Get", MethodAttributes.Public | MethodAttributes.Static, typeof(ScriptData), new[] { typeof(string) });
            var cache = type.DefineMethod("Cache", MethodAttributes.Private | MethodAttributes.Static, typeof(ScriptData), new[] { typeof(Type) });
            var dict = type.DefineField("dict", dictType, FieldAttributes.Static | FieldAttributes.Private);
            var nameType = type.DefineMethod("NameToType", MethodAttributes.Private | MethodAttributes.Static, typeof(Type), new[] { typeof(string) });

            var nameParam = Expression.Parameter(typeof(string), "name");
            var typeParam = Expression.Parameter(typeof(Type), "type");
            var dictField = Expression.Field(null, dict);
            var dataType = typeof(ScriptData);
            var returnLabel = Expression.Label(typeof(int));

            var nameTypeList = names.Select(x =>
                Expression.IfThen(
                    Expression.Equal(Expression.Constant(x.Key), nameParam),
                    Expression.Return(returnLabel, Expression.Constant(x.Value), typeof(Type))
                ) as Expression);
           
                
            var nameTypeTree = Expression.Lambda<Func<string, Type>>(Expression.Block(nameTypeList.Concat(new[] { Expression.Label(returnLabel) })), nameParam);
            nameTypeTree.CompileToMethod(nameType);

            //method.GetILGenerator().Emit(OpCodes.Ldtoken, typeof(int));
            var dataVariable = Expression.Parameter(dataType, "data");
            var createDelegate = typeof(Delegate).GetMethod("CreateDelegate", new[] { typeof(Type), typeof(string) });
            var cacheTree = Expression.Lambda<Func<Type, ScriptData>>(Expression.Block(
                    Expression.RuntimeVariables(dataVariable),
                    Expression.Assign(dataVariable, Expression.New(dataType)),
                    Expression.Assign(Expression.Field(dataVariable, "Main"), Expression.Call(createDelegate, typeParam, Expression.Constant("main")))
                ), typeParam);
            //new Action<>( Attachableant.main.MethodHandle.GetFunctionPointer())

            cacheTree.CompileToMethod(cache);

            //if(dict == null) dict = new Dictionary();
            //if(!dict.ContainsKey(name)) dict.Add(Cache(NameToType(name)));
            //return dict[name];
            var methodTree = Expression.Lambda<Func<string, ScriptData>>(Expression.Block(
                    Expression.IfThen(
                        Expression.Equal(dictField, Expression.Constant(null, dictType)),
                        Expression.Assign(dictField, Expression.New(dictType))
                    ),
                    Expression.IfThen(
                        Expression.Not(Expression.Call(dictField, dictType.GetMethod("ContainsKey"), nameParam)),
                        Expression.Call(dictField, dictType.GetMethod("Add"), nameParam, 
                            Expression.Call(cache, Expression.Call(nameType, nameParam)))
                    ),
                    Expression.Return(returnLabel, Expression.Property(dictField, dictType.GetProperty("Item"), nameParam)),
                    Expression.Label(returnLabel)
                ), nameParam);
            methodTree.CompileToMethod(method);
            */

            var method = type.DefineMethod("GetData", MethodAttributes.Public | MethodAttributes.Static, dictType, Type.EmptyTypes);
            EmitDataMethod(method.GetILGenerator());

            return type.CreateType();
        }

        /// <summary>
        /// Type->ScriptData
        /// </summary>
        /// <param name="gen"></param>
        void EmitCacheMethod(ILGenerator gen)
        {
            Type dataType = typeof(ScriptData);
            gen.DeclareLocal(dataType);
            gen.Emit(OpCodes.Newobj, dataType.GetConstructor(Type.EmptyTypes));
            gen.Emit(OpCodes.Stloc_0);

            gen.Emit(OpCodes.Ldarg_0);

        }

        /// <summary>
        /// void->Dict!string, ScriptData
        /// </summary>
        /// <param name="gen"></param>
        void EmitDataMethod(ILGenerator gen)
        {
            Type dataType = typeof(ScriptData);
            Type retType = typeof(Dictionary<string, ScriptData>);
            ConstructorInfo methodCons = typeof(ScriptMethod).GetConstructor(new[] { typeof(object), typeof(IntPtr) });
            gen.DeclareLocal(dataType);
            gen.DeclareLocal(retType);
            gen.Emit(OpCodes.Newobj, retType.GetConstructor(Type.EmptyTypes));
            gen.Emit(OpCodes.Stloc_1);
            foreach (var item in names)
            {
                gen.Emit(OpCodes.Newobj, dataType.GetConstructor(Type.EmptyTypes));
                gen.Emit(OpCodes.Stloc_0);

                gen.Emit(OpCodes.Ldloc_0);
                gen.Emit(OpCodes.Ldnull);
                gen.Emit(OpCodes.Ldftn, item.Value.GetMethod("main"));
                gen.Emit(OpCodes.Newobj, methodCons);
                gen.Emit(OpCodes.Stfld, dataType.GetField("Main"));//data.Main = ContainerType.main;

                gen.Emit(OpCodes.Ldloc_0);
                gen.Emit(OpCodes.Ldnull);
                gen.Emit(OpCodes.Ldftn, item.Value.GetMethod("label_init"));
                gen.Emit(OpCodes.Newobj, methodCons);
                gen.Emit(OpCodes.Stfld, dataType.GetField("Init"));

                //gen.Emit(OpCodes.Ldloc_0);
                //gen.Emit(OpCodes.Ldsfld, item.Value.GetField("GlobalVarNumber"));
                //gen.Emit(OpCodes.Stfld, dataType.GetField("GlobalVarNumber"));
                gen.Emit(OpCodes.Ldloc_0);
                gen.Emit(OpCodes.Call, item.Value.GetMethod("GetGlobalVarNumberEx"));
                gen.Emit(OpCodes.Stfld, dataType.GetField("GlobalVarNumber"));

                gen.Emit(OpCodes.Ldloc_1);//dict
                gen.Emit(OpCodes.Ldstr, item.Key);
                gen.Emit(OpCodes.Ldloc_0);//data
                gen.Emit(OpCodes.Call, retType.GetMethod("Add"));
            }
            gen.Emit(OpCodes.Ldloc_1);
            gen.Emit(OpCodes.Ret);
        }


    }
}
