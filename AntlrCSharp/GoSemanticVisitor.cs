using System;
using Generated;
using AntlrCSharp.TabuleiroGo;

public class GoSemanticVisitor : ggoBaseVisitor<object>
{
    private readonly TabuleiroGo Tabuleiro = new TabuleiroGo(19);

    // Último jogador que jogou – começa como White para Preto jogar primeiro
    private Pecas ultimoJogador = Pecas.White;

    // Converte 'A' → 0, 'B' → 1 ...
    private int ColumnToIndex(string c)
    {
        if (string.IsNullOrEmpty(c))
            throw new ArgumentException("Coluna vazia");

        char letra = char.ToUpperInvariant(c[0]);

        if (letra < 'A' || letra > 'T' || letra == 'I')
            throw new Exception($"Coluna inválida: {letra}");

        // Ajuste: pula o I
        if (letra > 'I')
            return letra - 'A' - 1;

        return letra - 'A';
    }

    public override object VisitMove_decl(ggoParser.Move_declContext context)
    {
        // Determina jogador a partir do token (ajuste conforme o texto exato gerado pelo lexer)
        var jogadorText = context.JOGADOR().GetText();
        Pecas jogadorAtual = jogadorText == "Preto" ? Pecas.Black : Pecas.White;

        // Valida turno (entrada já especifica jogador)
        if (jogadorAtual == ultimoJogador)
            throw new Exception($"Jogador {jogadorAtual} jogou fora da vez!");

        ultimoJogador = jogadorAtual;

        // PASSA (token PASSA na gramática)
        if (context.pos().GetText().Equals("PASSA", StringComparison.OrdinalIgnoreCase))
        {
            // usa API do tabuleiro para passar
            Tabuleiro.PlayPass(jogadorAtual);
            Tabuleiro.PrintBoard();
            return null;
        }

        // Lê coluna e linha
        string colText = context.pos().LETRA().GetText();
        int row = int.Parse(context.pos().LINHA().GetText());

        int x = ColumnToIndex(colText);
        int y = row - 1;

        // Valida limites
        if (!Tabuleiro.IsInside(x, y))
            throw new Exception($"Posição {colText}{row} está fora do tabuleiro!");

        try
        {
            Tabuleiro.PlayMove(x, y, jogadorAtual);
        }
        catch (Exception ex)
        {
            // Enriquecer a exceção com contexto (mantendo a inner exception)
            throw new Exception($"Erro aplicando jogada {jogadorText}|{colText}{row}: {ex.Message}", ex);
        }

        // Mostra tabuleiro após a jogada
        Tabuleiro.PrintBoard();

        return null;
    }

    public override object VisitSeq_move(ggoParser.Seq_moveContext context)
    {
        base.VisitSeq_move(context);
        Console.WriteLine("Jogo concluído. Estado final do tabuleiro:");
        Tabuleiro.PrintBoard();

        var (blackPoints, whitePoints) = Tabuleiro.CountFinalPoints();

        Console.WriteLine($"Pontuação Final - Preto: {blackPoints}");
        Console.WriteLine($"Pontuação Final - Branco: {whitePoints}");

        if (blackPoints > whitePoints)
        {
            Console.WriteLine("Vencedor: Preto");
        }
        else if (whitePoints > blackPoints)
        {
            Console.WriteLine("Vencedor: Branco");
        }
        else
        {
            Console.WriteLine("Empate!");
        }
        return null;
    }
}
