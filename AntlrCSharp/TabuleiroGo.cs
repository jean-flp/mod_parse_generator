namespace AntlrCSharp.TabuleiroGo;

public enum Pecas { Empty, Black, White }

public class TabuleiroGo
{
    public int Tamanho{get;}
    public Pecas[,] Celulas {get;}

    public TabuleiroGo(int tamanho = 19)
    {
        Tamanho = tamanho;
        Celulas = new Pecas[tamanho,tamanho];
    }

    public bool IsInside( int x, int y) => x>=0 && x< Tamanho && y>= 0 && y < Tamanho;

    public Pecas GetPosition(int x, int y) => Celulas[x,y];

    public void SetPosition(int x, int y, Pecas peca)
    {
        Celulas[x,y] = peca;
    }

    public void PrintBoard()
    {
        Console.WriteLine();

        // Cabeçalho (A, B, C...)
        Console.Write("   ");
        for (int x = 0; x < Tamanho; x++)
            Console.Write((char)('A' + x) + " ");
        Console.WriteLine();

        for (int y = 0; y < Tamanho; y++)
        {
            // Número da linha
            Console.Write($"{y + 1,2} ");

            for (int x = 0; x < Tamanho; x++)
            {
                var p = GetPosition(x, y);

                char c = p switch
                {
                    Pecas.Black => '●',   // pedra preta
                    Pecas.White => '○',   // pedra branca
                    _ => '+'              // vazio
                };

                Console.Write(c + " ");
            }
            Console.WriteLine();
        }
        Console.WriteLine();
    }
    
}