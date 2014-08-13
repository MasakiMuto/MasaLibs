namespace ScriptFS

type OperatorMark =
    | Pos
    | Neg
    | Mul
    | Div
    | Mod
    | Not
    | And
    | Or
    | Equal
    | NotEqual
    | Big
    | BigEqual
    | Small
    | SmallEqual

type AssignMark =
    | Sub
    | SubPos
    | SubNeg
    | SubMul
    | SubDiv
    | Inc
    | Dec

type Mark =
    | No
    | Operator of OperatorMark
    | AssignOp of AssignMark
    | Open
    | Close
    | Colon
    | Dot
    | Dollar
    | Tab
    | Return
    | Semicolon
    | Define
    | Include

type Token =
    | Number of float32
    | Id of string
    | Mark of Mark

type Expression =
    | Literal
    | Variable of string
    | Binary of Expression * OperatorMark * Expression
    | Function of string * Expression list * (string * Expression list) list


type Sentence = 
    | Var of string * Expression
    | If of Expression * Sentence list * Option<Sentence list>
    | Assign of string * AssignMark * Expression
    | Method of string * Expression list * (string * Expression list) list
    | Block of Sentence list

type Line = {
    Tokens : Token list;
    Number : int;
    Indent : int;
}

module Scanner =
    let toString (c : char list) = System.String.Concat(Array.ofList(c))

    let mark x = 
        match x with
            | "\n" -> Return
            | "\t" -> Tab
            | ":" -> Colon
            | "$" -> Dollar
            | "." -> Dot
            | ";" -> Semicolon
            | "#define" -> Define
            | "#include" -> Include
            | "(" -> Open
            | ")" -> Close
            | "=" -> AssignOp Sub
            | "+=" -> AssignOp SubPos
            | "-=" -> AssignOp SubNeg
            | "*=" -> AssignOp SubMul
            | "/=" -> AssignOp SubDiv
            | "++" -> AssignOp Inc
            | "--" -> AssignOp Dec
            | "+" -> Operator Pos
            | "-" -> Operator Neg
            | "*" -> Operator Mul
            | "/" -> Operator Div
            | "%" -> Operator Mod
            | "!" -> Operator Not
            | "&" -> Operator And
            | "|" -> Operator Or
            | "==" -> Operator Equal
            | "!=" -> Operator NotEqual
            | ">" -> Operator Big
            | ">=" -> Operator BigEqual
            | "<" -> Operator Small
            | "<=" -> Operator SmallEqual
            | _ -> No

    let split (text : string) : string list = 
        let rec inner (buf : char list) (i : int) : string list = 
            let inner_ x = inner x (i+1)
            let tokenize = if buf.Length = 0 then [] else [toString (List.rev buf)]
            if text.Length = i then tokenize
            else
            match text.Chars i with
                | ' ' -> tokenize @ inner_ [] 
                | '(' | ')' | '\n' | '\t' | ':' | '$' | '.' | ';' as c -> tokenize @ c.ToString() :: inner_ []
                | '+' | '-' | '*' | '/' | '%' | '!' | '=' | '>' | '<' as c -> let x = mark ((text.Chars (i + 1)).ToString()) in if x = No then tokenize @ c.ToString() :: inner_ [] else tokenize @ inner_ [c]
                | _ -> inner_ (text.Chars i :: buf)
        in inner [] 0

    let comment (words : Token list) : Token list = 
        let rec inner (w : Token list) c = 
            if w.IsEmpty then [] 
            else
                match w.Head with
                    | Mark Semicolon -> inner w.Tail true 
                    | Mark Return -> w.Head :: inner w.Tail false
                    | _ -> if c then inner w.Tail c else w.Head :: inner w.Tail c
        in inner words false

    let scan (text : string) : Token list = 
        let tokenize x = 
            let m = mark x in 
            if m <> No
            then Mark m 
            else let ret = ref 0.0f in if System.Single.TryParse(x, ret) then Number !ret else Id x
        in List.map tokenize (split text) |> comment
       