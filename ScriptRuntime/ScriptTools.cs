using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

namespace Masa.ScriptEngine
{
	using Value = System.Single;

	public interface IExpressionTreeMaker
	{
		int GlobalVarNumber { get; }
		Action<Environment> Statement { get; }
		Action<Environment> InitStatement { get; }
		Action<Environment> GetLabelStatement(string label);

		string OutputClassInformation();
	}

	public abstract class ScriptDataBase
	{
		public IExpressionTreeMaker Tree { get; protected set; }
		public Type TargetType { get; private set; }

		protected ScriptDataBase(Type type)
		{
			TargetType = type;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="target">スクリプトを使用するオブジェクト</param>
		/// <returns></returns>
		public ScriptRunner GetScriptRunner(object target)
		{
			if (target.GetType() == TargetType || target.GetType().IsSubclassOf(TargetType))
			{
				return new ScriptRunner(Tree, target);
			}
			else
			{
				throw new Exception("スクリプトのターゲット型とターゲットオブジェクトの型が不一致");
			}
		}

		public Action<Environment> GetLabelAction(string label)
		{
			return Tree.GetLabelStatement(label);
		}

		public string GetDocument()
		{
			return Tree.OutputClassInformation();
		}

	}

	

	/// <summary>
	/// 読み込み済みのTreeを使ってスクリプトの実行をするクラス
	/// </summary>
	public class ScriptRunner
	{
		public readonly Environment Environment;
		readonly Action<Environment> UpdateAction;
		readonly IExpressionTreeMaker Tree;
		public Value State
		{
			get { return Environment.State; }
			set { Environment.State = value; }
		}

		/// <summary>
		/// このオブジェクトが生成されてからのUpdate呼び出し回数
		/// </summary>
		public Value Count
		{
			get { return Environment.Frame; }
		}
		/// <summary>
		/// 最後にStateが変更されてからのUpdate呼び出し回数
		/// </summary>
		public Value StateCount
		{
			get { return Environment.StateFrame; }
		}

		/// <summary>
		/// initステートメントがあれば自動で実行する
		/// </summary>
		/// <param name="tree"></param>
		/// <param name="target"></param>
		public ScriptRunner(IExpressionTreeMaker tree, object target)
		{
			Tree = tree;
			Environment = new Masa.ScriptEngine.Environment(target, tree.GlobalVarNumber);
			UpdateAction = tree.Statement;
			if (tree.InitStatement != null)
			{
				tree.InitStatement(Environment);
			}
		}

		public void Update()
		{
			Environment.FrameUpdate();
			UpdateAction(Environment);
		}

		public Action<Environment> GetLabelAction(string label)
		{
			return Tree.GetLabelStatement(label);
		}

		/// <summary>
		/// labelのブロックを実行する。存在しない場合は何もしない
		/// </summary>
		/// <param name="label"></param>
		public void InvokeLabelAction(string label)
		{
			var act = Tree.GetLabelStatement(label);
			if (act != null)
			{
				act(Environment);
			}
			
		}
	}
}
