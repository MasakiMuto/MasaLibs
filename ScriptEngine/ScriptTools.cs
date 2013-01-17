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
		protected IExpressionTreeMaker Tree;
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

	public abstract class ScriptData : ScriptDataBase
	{
		protected string[] Table { get; private set; }
		protected string[] Labels { get; private set; }
		public Dictionary<string, string> HeaderDictionary { get; set; }
		
		protected ScriptData(Type target, string[] table = null, string[] labels = null)
			: base(target)
		{
			Table = table;
			Labels = labels;
			
			//Load();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="code">プリプロセス済みコード</param>
		/// <param name="name">ファイル名</param>
		protected void Load(string code, string name)
		{
			Scanner sc = null;
			try
			{
				sc = new Scanner(code, HeaderDictionary);
			}
			catch (Exception e)
			{
				throw new Exception(name + "でスキャナエラー:" + e.Message, e);
			}
			try
			{
				Tree = new ExpressionTreeMaker(sc.Tokens.ToArray(), TargetType, Table);
			}
			catch (Exception e)
			{
				throw new Exception(name + "で構文エラー:" + e.Message, e);
			}
			if (Labels != null)
			{
				foreach (var item in Labels)
				{
					Tree.GetLabelStatement(item);
				}
			}
		}

		public abstract void Load();
	}


	/// <summary>
	/// スクリプトの読み込み、リロード、及びScriptRunnerの生成をするクラス
	/// </summary>
	public class FileScriptData : ScriptData
	{
		string FileName;
		Func<string, string> codeMapper;

		/// <summary>
		/// 初期化して、かつLoadも行う
		/// </summary>
		/// <param name="file">スクリプトファイルへのパス</param>
		/// <param name="target">スクリプトを使用するクラスの型</param>
		/// <param name="table">文字列使用用テーブル</param>
		/// <param name="labels">init以外に外部から使用したいラベル。入れておくと読み込みと同時にキャッシュする</param>
		public FileScriptData(string file, Type target, string[] table = null, string[] labels = null)
			: this(file, target, s => s, table, labels)
		{
		}

		public FileScriptData(string file, Type target, Func<string, string> mapper, string[] table = null, string[] labels = null)
			: base(target, table, labels)
		{
			codeMapper = mapper;
			FileName = file;
			//Load();
		}

		/// <summary>
		/// リロードするときに明示的に呼び出す
		/// </summary>
		public override void Load()
		{
			Load(codeMapper(File.ReadAllText(FileName)), FileName);
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
