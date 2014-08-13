namespace ScriptFS

module Parser = 
    let line (w : Token list) : Line list = 
        let rec inner (w : Token list) buf i  j = 
            let line = { Tokens = List.rev buf; Number = i; Indent = j }
            if w.IsEmpty then [line] else
            match w.Head with
                | Mark Return -> if buf.Length = 0 then inner w.Tail [] (i+1) 0 else line :: inner w.Tail [] (i+1) 0
                | Mark Tab -> inner w.Tail buf i (j+1)
                | _ -> inner w.Tail (w.Head :: buf) i j
        in inner w [] 0 0

    type ParcelBlock = Token list

    let parse (l : Line) : Sentence = 
        let expr (t : Token list) : Expression = Variable ""
        match l.Tokens.Head with
            | Id x -> 
                match x with
                    | "var" -> Var ((l.Tokens.Tail.Head |> function Id n -> n | _ -> failwith ""), expr l.Tokens.Tail.Tail.Tail)
                    | _ -> Var ("a", Variable "")
            | _ -> failwith ""