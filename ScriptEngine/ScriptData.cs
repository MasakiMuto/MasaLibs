using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Masa.ScriptEngine
{
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
}
