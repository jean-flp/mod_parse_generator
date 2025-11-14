public enum Pecas { Empty, Black, White }

public class TabuleiroGo
{
    public int Tamanho{get;}
    public Pecas[,] Celulas {get;}

    public TabuleiroGo(int tamanho = 19){
        Tamanho = tamanho;
        Celulas = new Pecas[tamanho,tamanho];
    }

    public bool IsInside( int x, int y) => x>=0 && x< Tamanho && y>= 0 && y < Tamanho;

    public Pecas GetPosition(int x, int y) => Celulas[x,y];

    public void SetPosition(int x, int y, Pecas peca)
    {
        Celulas[x,y] = peca;
    }
    
}