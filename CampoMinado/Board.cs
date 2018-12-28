using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;

namespace CampoMinado
{
    public class Board
    {
        private int Columns { get; set; }
        private int Rows { get; set; }
        private int MineCount { get; set; }
        public List<Panel> Panels { get; set; }
        public GameStatus Status { get; set; }

        public Board(Point startCell, int cols, int rows, int mines, int width, int height)
        {
            Columns = cols;
            Rows = rows;
            MineCount = mines;
            Panels = new List<Panel>();

            int id = 0;
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    Panels.Add(new Panel(this, id, j + startCell.X, i + startCell.Y, width, height));
                    id++;
                }
            }
            //Iniciar Jogo
        }

        // Retorna vizinhos do painel com 1 de profundidade(apenas os maix próximos)
        public List<Panel> Neighbors(int x, int y)
        {
            return Neighbors(x, y, 1);
        }

        // Retorna vizinhos do painel com profundidade ~depth~
        public List<Panel> Neighbors(int x, int y, int depth)
        {
            var nearbyPanels = Panels.Where(panel => panel.X >= (x - depth) && panel.X <= (x + depth)
                                            && panel.Y >= (y - depth) && panel.Y <= (y + depth));
            var currentPanel = Panels.Where(panel => panel.X == x && panel.Y == y);
            return nearbyPanels.Except(currentPanel).ToList();
        }

        public void Reveal(int x, int y)
        {
            //1. Encontrar o painel
            var selectedPanel = Panels.First(panel => panel.X == x && panel.Y == y);

            //2. Se não há nenhum painel revelado é o primeiro movimento
            if (Panels.All(panel => !panel.IsRevealed))
            {
                FirstMove(x, y, new Random());
            }

            //Revelar o painel
            selectedPanel.IsRevealed = true;
            selectedPanel.ShowSelection = false;

            //3. terminar jogo se painel for uma mina
            if (selectedPanel.IsMine)
            {
                GameOver();
            }

            //3. se o painel é um zero, revelar vizinhos em cascata
            if (!selectedPanel.IsMine && selectedPanel.AdjacentMines == 0)
            {
                RevealZeros(x, y);
            }

            //4. se o movimento causar o fim do jogo, terminar o jogo
            if (!selectedPanel.IsMine)
                CompletionCheck();
        }

        public void FirstMove(int x, int y, Random rand)
        {
            //Para qualquer painel, pegar o primeiro painél clicado e os vizinhos numa profundidade X
            //E marca-los como indisponíveis para colocar mina
            var depth = 0.125 * Columns;
            var neighbors = Neighbors(x, y, (int)depth); //Pegar os vizinhos numa certa profundidade
            neighbors.Add(GetPanel(x, y));

            //Selecionar painéis aleatóriamente dos que não foram excluidos
            var mineList = Panels.Except(neighbors).OrderBy(user => rand.Next());
            var mineSlots = mineList.Take(MineCount).ToList().Select(z => new { z.X, z.Y });

            //Colocar as minas
            foreach (var mineCoord in mineSlots)
            {
                Panels.Single(panel => panel.X == mineCoord.X && panel.Y == mineCoord.Y).IsMine = true;
            }

            //Para cada painel que não é mina, determinar e salvar minas adjacentes
            foreach (var openPanel in Panels.Where(panel => !panel.IsMine))
            {
                var nearbyPanels = Neighbors(openPanel.X, openPanel.Y);
                openPanel.AdjacentMines = nearbyPanels.Count(z => z.IsMine);
            }
        }

        public void RevealZeros(int x, int y)
        {
            var neighborPanels = Neighbors(x, y).Where(panel => !panel.IsRevealed);
            foreach (var neighbor in neighborPanels)
            {
                neighbor.IsRevealed = true;
                if (neighbor.AdjacentMines == 0)
                {
                    RevealZeros(neighbor.X, neighbor.Y);
                }
            }
        }

        //Conferir se o jogo foi completo
        private void CompletionCheck()
        {
            var hiddenPanels = Panels.Where(x => !x.IsRevealed).Select(x => x.ID);
            var minePanels = Panels.Where(x => x.IsMine).Select(x => x.ID);
            if (!hiddenPanels.Except(minePanels).Any())
            {
                Status = GameStatus.Completed;
            }
        }

        public Panel GetPanel(int x, int y)
        {
            return Panels.First(z => z.X == x && z.Y == y);
        }

        public void Flag(int x, int y)
        {
            var panel = Panels.Where(z => z.X == x && z.Y == y).First();
            if (!panel.IsRevealed)
            {
                panel.IsFlagged = true;
            }
        }

        //Terminar o jogo
        public void GameOver()
        {
            Status = GameStatus.Failed;
            var remainingPanels = Panels.Where(p => !p.IsRevealed);

            //Revelar painéis restantes
            foreach (var panel in remainingPanels)
            {
                panel.IsRevealed = true;
            }

            Debug.WriteLine("Perdeu");
        }
    }

    public enum GameStatus
    {
        InProgress,
        Failed,
        Completed
    }
}


