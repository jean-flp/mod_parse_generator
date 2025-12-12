using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AntlrCSharp.TabuleiroGo
{
    public enum Pecas { Empty = 0, Black = 1, White = 2 }

    public class TabuleiroGo
    {
        public int Tamanho { get; }
        public Pecas[,] Celulas { get; }

        // Histórico de hashes (usado para detectar Ko / Superko)
        private HashSet<string> HistoricoHashes = new HashSet<string>();

        // Último hash (estado atual) — mantido atualizado
        private string ultimoHash;

        // Se true, impede qualquer repetição de estado (superko). Se false, ainda evita repetições exatas mas
        // normalmente a checagem é equivalente (manter true é razoável).
        public bool EnforceSuperKo { get; set; } = true;

        public TabuleiroGo(int tamanho = 19)
        {
            if (tamanho <= 0) throw new ArgumentException("Tamanho do tabuleiro deve ser positivo");
            Tamanho = tamanho;
            Celulas = new Pecas[tamanho, tamanho];
            // Inicializa vazio (já vem zero, mas para clareza:)
            for (int x = 0; x < Tamanho; x++)
                for (int y = 0; y < Tamanho; y++)
                    Celulas[x, y] = Pecas.Empty;

            ultimoHash = BoardHash();
            HistoricoHashes.Add(ultimoHash);
        }

        #region Acesso simples
        public bool IsInside(int x, int y) => x >= 0 && x < Tamanho && y >= 0 && y < Tamanho;

        public Pecas GetPosition(int x, int y)
        {
            if (!IsInside(x, y)) throw new ArgumentOutOfRangeException($"Posição {x},{y} fora do tabuleiro");
            return Celulas[x, y];
        }

        public void SetPosition(int x, int y, Pecas peca)
        {
            if (!IsInside(x, y)) throw new ArgumentOutOfRangeException($"Posição {x},{y} fora do tabuleiro");
            Celulas[x, y] = peca;
        }
        #endregion

        #region Jogadas públicas

        // Jogada normal em (x,y). Lança Exception se jogada ilegal.
        public void PlayMove(int x, int y, Pecas jogador)
        {
            if (jogador == Pecas.Empty) throw new ArgumentException("Jogador inválido");

            if (!IsInside(x, y))
                throw new Exception($"Posição {IndexToColumn(x)}{y + 1} está fora do tabuleiro!");

            if (GetPosition(x, y) != Pecas.Empty)
                throw new Exception($"Posição {IndexToColumn(x)}{y + 1} já está ocupada!");

            // Faz backup do estado atual (para restaurar em caso de ilegalidade)
            var backup = CloneCells();

            // Coloca a pedra provisoriamente
            SetPosition(x, y, jogador);

            bool capturedAny = false;

            // Para cada vizinho inimigo, se o grupo ficar sem liberdades, captura
            foreach (var (nx, ny) in Neighbors(x, y))
            {
                if (!IsInside(nx, ny)) continue;
                var p = GetPosition(nx, ny);
                if (p == Pecas.Empty) continue;
                if (p == Inimigo(jogador))
                {
                    var grupo = GetGroup(nx, ny);
                    if (CountLiberties(grupo) == 0)
                    {
                        RemoveGroup(grupo);
                        capturedAny = true;
                    }
                }
            }

            // Após capturas, verificar suicídio do próprio grupo (se não capturou nada e o grupo ficou sem liberdades)
            var meuGrupo = GetGroup(x, y);
            if (CountLiberties(meuGrupo) == 0)
            {
                // suicídio só é permitido se capturou algo (o que já teria removido libertades do inimigo)
                if (!capturedAny)
                {
                    // restaura e lança
                    RestoreCells(backup);
                    throw new Exception($"Jogada ilegal: suicídio em {IndexToColumn(x)}{y + 1}!");
                }
            }

            // Agora calcula hash e checa Ko / SuperKo
            var hashAtual = BoardHash();
            if (HistoricoHashes.Contains(hashAtual))
            {
                // repetição detectada -> ilegal
                RestoreCells(backup);
                throw new Exception($"Jogada ilegal: violação da regra de Ko / repetição do estado (posição {IndexToColumn(x)}{y + 1}).");
            }

            // Aceita jogada: adiciona hash ao histórico e atualiza último hash
            HistoricoHashes.Add(hashAtual);
            ultimoHash = hashAtual;
        }

        // PASS (jogador passa)
        public void PlayPass(Pecas jogador)
        {
            // Representa pass apenas salvando o hash atual no histórico (passa não altera tabuleiro,
            // mas a regra de superko trata repetição de estados do tabuleiro — um pass não muda o tabuleiro,
            // então se alguém tentar repetir o estado exato que já existia, será detectado).
            // Geralmente, pass não muda o board, mas adicionar o mesmo hash novamente não altera HashSet.
            // Para permitir distinguir sequências com passes, você poderia incluir também quem passou no hash.
            // Implementação simples: adiciona um "pass marker" incorporado ao hash.

            // Criamos um hash que incorpora um marcador de pass para impedir repetições exatas que envolvam passes.
            string passHash = BoardHash() + "#PASS";
            if (HistoricoHashes.Contains(passHash))
            {
                // isso significaria que essa mesma situação com pass já ocorreu
                throw new Exception("Jogada ilegal: repetição de estado com PASS (superko).");
            }
            HistoricoHashes.Add(passHash);
            ultimoHash = passHash;
        }

        #endregion

        #region Utilitários de grupos e captura

        private Pecas Inimigo(Pecas p) =>
            p == Pecas.Black ? Pecas.White : (p == Pecas.White ? Pecas.Black : Pecas.Empty);

        // Retorna lista de 4-vizinhos (x +/- 1, y) e (x, y +/- 1)
        private IEnumerable<(int x, int y)> Neighbors(int x, int y)
        {
            yield return (x - 1, y);
            yield return (x + 1, y);
            yield return (x, y - 1);
            yield return (x, y + 1);
        }

        // DFS para obter todas as pedras do grupo conectado a (x,y)
        public List<(int x, int y)> GetGroup(int x, int y)
        {
            var color = GetPosition(x, y);
            var group = new List<(int x, int y)>();
            if (color == Pecas.Empty) return group;

            var visited = new bool[Tamanho, Tamanho];
            var stack = new Stack<(int x, int y)>();
            stack.Push((x, y));
            visited[x, y] = true;

            while (stack.Count > 0)
            {
                var cur = stack.Pop();
                group.Add(cur);

                foreach (var (nx, ny) in Neighbors(cur.x, cur.y))
                {
                    if (!IsInside(nx, ny)) continue;
                    if (visited[nx, ny]) continue;
                    var p = GetPosition(nx, ny);
                    if (p == color)
                    {
                        visited[nx, ny] = true;
                        stack.Push((nx, ny));
                    }
                }
            }

            return group;
        }

        // Conta liberdades de um grupo (número de interseções vazias adjacentes ao grupo)
        public int CountLiberties(List<(int x, int y)> group)
        {
            var seen = new HashSet<(int x, int y)>();
            int liberties = 0;
            foreach (var (gx, gy) in group)
            {
                foreach (var (nx, ny) in Neighbors(gx, gy))
                {
                    if (!IsInside(nx, ny)) continue;
                    if (GetPosition(nx, ny) == Pecas.Empty)
                    {
                        if (!seen.Contains((nx, ny)))
                        {
                            seen.Add((nx, ny));
                            liberties++;
                        }
                    }
                }
            }
            return liberties;
        }

        // Remove o grupo (seta para Empty)
        public void RemoveGroup(List<(int x, int y)> group)
        {
            foreach (var (gx, gy) in group)
                SetPosition(gx, gy, Pecas.Empty);
        }

        #endregion

        #region Histórico / hash do tabuleiro

        // Cria um hash simples em string (row-major). Simples e eficaz para propósitos de Ko/superko.
        // Para melhor performance pode-se usar Zobrist hashing; aqui optamos por simplicidade.
        public string BoardHash()
        {
            var sb = new StringBuilder(Tamanho * Tamanho + 4);
            for (int y = 0; y < Tamanho; y++)
            {
                for (int x = 0; x < Tamanho; x++)
                {
                    sb.Append((int)Celulas[x, y]); // 0/1/2
                }
                sb.Append(','); // separador por linha somente para legibilidade
            }
            return sb.ToString();
        }

        #endregion

        #region Debug / Impressão

        // Converte índice de coluna (0-based) para letra (A, B, C, ...)
        private string IndexToColumn(int x)
        {
            return ((char)('A' + x)).ToString();
        }

        // Imprime o tabuleiro no terminal (ASCII), 1..Tamanho nas linhas e A.. nas colunas
        public void PrintBoard()
        {
            Console.WriteLine();
            // Cabeçalho
            Console.Write("   ");
            for (int x = 0; x < Tamanho; x++)
            {
                Console.Write($"{(char)('A' + x)} ");
            }
            Console.WriteLine();

            for (int y = 0; y < Tamanho; y++)
            {
                Console.Write($"{y + 1,2} ");
                for (int x = 0; x < Tamanho; x++)
                {
                    var p = GetPosition(x, y);
                    char c = p switch
                    {
                        Pecas.Black => '●',   // pedra preta
                        Pecas.White => '○',   // pedra branca
                        _ => '+'
                    };
                    Console.Write(c);
                    Console.Write(' ');
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }

        #endregion

        #region Helpers de cópia / restauração

        private Pecas[,] CloneCells()
        {
            var copy = new Pecas[Tamanho, Tamanho];
            for (int x = 0; x < Tamanho; x++)
                for (int y = 0; y < Tamanho; y++)
                    copy[x, y] = Celulas[x, y];
            return copy;
        }

        private void RestoreCells(Pecas[,] snapshot)
        {
            for (int x = 0; x < Tamanho; x++)
                for (int y = 0; y < Tamanho; y++)
                    Celulas[x, y] = snapshot[x, y];
            ultimoHash = BoardHash();
        }

        #endregion
    }
}
