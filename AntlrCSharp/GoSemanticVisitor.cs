//using Antlr4.Runtime;

using Generated;
using AntlrCSharp.TabuleiroGo;

public class GoSemanticVisitor : ggoBaseVisitor<object>
{
    private TabuleiroGo Tabuleiro = new TabuleiroGo(19);
    private Pecas ultimoJogador = Pecas.White;

    private int ColumnToIndex(string c) => c[0] - 'A';

    public override object VisitMove_decl(ggoParser.Move_declContext context)
    {
        Pecas jogadorAtual =
            context.JOGADOR().GetText() == "B" ? Pecas.Black : Pecas.White;

        if (jogadorAtual == ultimoJogador)
            throw new Exception($"Jogador {jogadorAtual} jogou fora da vez!");

        ultimoJogador = jogadorAtual;

        if (context.pos().GetText() == "PASSA")
            return null;

        string col = context.pos().LETRA().GetText();
        int row = int.Parse(context.pos().LINHA().GetText());

        int x = ColumnToIndex(col);
        int y = row - 1;

        if (!Tabuleiro.IsInside(x, y))
            throw new Exception($"Posição {col}{row} está fora do tabuleiro!");

        if (Tabuleiro.GetPosition(x, y) != Pecas.Empty)
            throw new Exception($"Posição {col}{row} já está ocupada!");

        Tabuleiro.SetPosition(x, y, jogadorAtual);
        Tabuleiro.PrintBoard();

        return null;
    }
}