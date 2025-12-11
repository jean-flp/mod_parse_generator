using Antlr4.Runtime;
using System;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        // Entrada – você pode trocar por File.ReadAllText("arquivo.go")
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: program <file.go>");
            return;
        }

        string filePath =   args[1];
        string input;

         if (!File.Exists(filePath))
        {
            Console.WriteLine($"File not found: {filePath}");
            return;
        }

        try
        {
            input = File.ReadAllText(filePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            return;
        }
        // "B|D4//P|Q16//B|PASSA//P|F3//";

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
