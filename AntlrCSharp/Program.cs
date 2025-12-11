using Antlr4.Runtime;
using System;

class Program
{
    static void Main(string[] args)
    {
        // Entrada – você pode trocar por File.ReadAllText("arquivo.go")
        string input = "B|D4//P|Q16//B|PASSA//P|F3//";

        AntlrInputStream stream = new AntlrInputStream(input);

        // Lexer → Tokens
        var lexer = new ggoLexer(stream);
        var tokens = new CommonTokenStream(lexer);

        // Parser → Parse Tree
        var parser = new ggoParser(tokens);

        var tree = parser.seq_move(); // regra inicial da sua gramática

        Console.WriteLine(tree.ToStringTree(parser));

        // Listener OU Visitor
        // Escolha aqui o que você estiver usando:

        // 1) LISTENER:
        // var listener = new GoSemanticListener();
        // ParseTreeWalker.Default.Walk(listener, tree);

        // 2) VISITOR:
        var visitor = new GoSemanticVisitor();
        visitor.Visit(tree);
        //Imprimir o tabuleiro apenas no final
        //visitor.Tabuleiro.PrintBoard();

        Console.WriteLine("Teste concluído!");
    }
}
