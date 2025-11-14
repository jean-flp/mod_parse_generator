using Antlr4.Runtime.misc;

public class GoSemanticVisitor : GoGameBaseVisitor<object>
{
    private TabuleiroGo Tabuleiro = new TabuleiroGo(19);
    private Pecas ultimoJogador = Pecas.White;

    public override object VisitMove(GoGameParser.MoveContext context)
    {
        Stone jogadorAtual = 
            context.player().GetText() == "B"
            ? Pecas.Black
            : Pecas.White;

            if(jogadorAtual == ultimoJogador)
                throw new Exception($"Jogador {jogadorAtual} jogou fora da vez!");
            
            ultimoJogador = jogadorAtual;

            if(context.pos().GetText() == "PASS")
                return null;
            string col = context.pos().LETTER().GetText();
            int row = int.Parse(context.pos().NUMBER().GetText());

            int x = ColumnToIndex(col);
            int y = row - 1;

            if (!Board.IsInside(x, y))
                throw new Exception($"Posição {col}{row} está fora do tabuleiro!");

            if (Board.Get(x, y) != Stone.Empty)
                throw new Exception($"Posição {col}{row} já está ocupada!");

            // Aplicar jogada
            Board.Set(x, y, current);

            // Aqui poderia implementar capturas, suicídio, Ko

            return null;


    }

}