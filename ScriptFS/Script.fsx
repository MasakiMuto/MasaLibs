// F# の詳細については、http://fsharp.net を参照してください。F# プログラミングのガイダンスについては、
// 'F# チュートリアル' プロジェクトを参照してください。

#load "Scanner.fs"
#load "Parser.fs"

open ScriptFS

//let x = Scanner.split "hohfg()aaa" |> Parser.line

// ここでライブラリ スクリプト コードを定義します

let f = Scanner.scan >> Parser.line

