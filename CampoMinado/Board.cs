using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CampoMinado
{
    public class Board
    {
        private int Columns { get; set; }
        private int Rows { get; set; }
        private int MineCount { get; set; }
        private int MushCount { get; set; }
        public List<Panel> Panels { get; set; }
        public List<Mushroom> Mushrooms = new List<Mushroom>();
        public GameStatus Status { get; set; }

        private int chances = 0;

        public Texture2D greenMush;
        public Texture2D blueMush;
        public Texture2D redMush;

        Coroutines coroutines;
        UI ui;

        Random rand = new Random();

        public Board(UI ui, Coroutines coroutines, Point startCell, int cols, int rows, int mines, int mushrooms, int width, int height)
        {
            this.ui = ui;
            this.coroutines = coroutines;
            Columns = cols;
            Rows = rows;
            MineCount = mines;
            MushCount = mushrooms;
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

            //2. Apenas revelar se não estiver marcado
            if (!selectedPanel.IsFlagged)
            {
                var first = false;

                //3. Se não há nenhum painel revelado é o primeiro movimento
                if (Panels.All(panel => !panel.IsRevealed))
                {
                    //O primeiro movimento adiciona as minas
                    FirstMove(x, y, rand);
                    first = true;
                }

                //4. Spawnar cogumelo
                if (selectedPanel.IsMushroom)
                {
                    coroutines.Start(SpawnMushroom(x, y, rand));
                }

                //Revelar o painel
                selectedPanel.IsRevealed = true;
                selectedPanel.ShowSelection = false;

                //5. Checar se o painel clicado é mina
                if (selectedPanel.IsMine)
                {
                    //Se o jogador não tiver mais chances terminar o jogo
                    if (chances <= 0)
                    {
                        GameOver();
                    }
                    else
                    {
                        chances--;
                    }
                }

                //6. se o painel é um zero, revelar vizinhos em cascata
                if (!selectedPanel.IsMine && selectedPanel.AdjacentMines == 0)
                {
                    RevealZeros(x, y);
                }

                //7. Adicionar Cogumelos se for o primeiro movimento
                if (first)
                {
                    PlaceMushrooms(rand);
                }

                //8. se o movimento causar o fim do jogo, terminar o jogo
                if (!selectedPanel.IsMine)
                    CompletionCheck();
            }
        }

        public void FirstMove(int x, int y, Random rand)
        {
            //Para qualquer painel, pegar o primeiro painél clicado e os vizinhos numa profundidade X
            //E marca-los como indisponíveis para colocar mina
            var depth = 0.105 * Columns;
            var neighbors = Neighbors(x, y, (int)depth); //Pegar os vizinhos numa certa profundidade
            neighbors.Add(GetPanel(x, y));

            //Selecionar aleatóriamente painéis que não foram revelados
            var emptyList = Panels.Except(neighbors).OrderBy(p => rand.Next());
            //Montar lista com os paineis para colocar minas
            var mineSlots = emptyList.Take(MineCount).ToList().Select(z => new { z.X, z.Y });

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

        public void PlaceMushrooms(Random rand)
        {
            var mushSlots = Panels.Where(p => !p.IsMine && !p.IsRevealed).OrderBy(p => rand.Next()).Take(MushCount);

            foreach (var mushCoord in mushSlots)
            {
                Panels.Single(panel => panel.X == mushCoord.X && panel.Y == mushCoord.Y).IsMushroom = true;
                System.Diagnostics.Debug.WriteLine(mushCoord.X + ", " + mushCoord.Y);
            }
        }

        public void RevealZeros(int x, int y)
        {
            var neighborPanels = Neighbors(x, y).Where(panel => !panel.IsRevealed);
            foreach (var neighbor in neighborPanels)
            {
                if (!neighbor.IsFlagged && !neighbor.IsMushroom)
                {
                    Reveal(neighbor.X, neighbor.Y);
                    if (neighbor.AdjacentMines == 0)
                    {
                        RevealZeros(neighbor.X, neighbor.Y);
                    }
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

        //Marcar painel
        public void Flag(int x, int y)
        {
            var panel = Panels.First(z => z.X == x && z.Y == y);
            if (!panel.IsRevealed)
            {
                panel.IsFlagged = !panel.IsFlagged;
            }
        }

        //Ajudar o jogador marcando uma bomba
        public void RandomFlag()
        {
            var x = 0;
            var y = 0;
            var flagged = false;
            while (!flagged)
            {
                var panels = Panels.Where(p => p.IsMine && !p.IsFlagged).OrderBy(p => Guid.NewGuid()).Take(2);
                var panel = panels.First();
                if (!panel.IsRevealed)
                {
                    x = panel.X;
                    y = panel.Y;
                    flagged = true;
                }
            }
            Flag(x, y);
        }

        //Ajudar o jogador dando uma nova chance ao revelar uma mina
        public void AddChance()
        {
            chances++;
        }

        //Atrapalhar o jogador adicionando duas novas minas ao jogo
        public void NewMine()
        {
            //Pegar os paineis não revelados e não marcados
            var notRevealed = Panels.Where(p => !p.IsRevealed && !p.IsFlagged);

            //Dos paineis não revelados pegar dois que não sao minas nem cogumelos
            var panels = notRevealed.Where(p => !p.IsMine && !p.IsMushroom).OrderBy(p => Guid.NewGuid()).Take(2);

            //Colocar as minas
            foreach (var panel in panels)
            {
                Panels.Single(p => p.X == panel.X && p.Y == panel.Y).IsMine = true;
                MineCount++;
            }

            //Para cada painel que não é mina, determinar e salvar minas adjacentes
            foreach (var openPanel in Panels.Where(panel => !panel.IsMine))
            {
                var nearbyPanels = Neighbors(openPanel.X, openPanel.Y);
                openPanel.AdjacentMines = nearbyPanels.Count(z => z.IsMine);
            }
        }

        //Spawnar cogumelo
        public IEnumerator SpawnMushroom(int x, int y, Random rand)
        {
            Mushrooms.Add(new Mushroom(this, x, y, 16, 16, rand, greenMush, blueMush, redMush));
            //TODO: Animar - Movendo para a UI
            yield return Coroutines.Pause(0);
            var mush = Mushrooms.Last();
            Vector2 posToGo = Vector2.Zero;
            if (Mushrooms.Count == 1)
            {
                posToGo = new Vector2((int)ui.boxPos.X, (int)ui.boxPos.Y);
            }
            if (Mushrooms.Count == 2)
            {
                posToGo = new Vector2((int)ui.boxPos2.X, (int)ui.boxPos2.Y);
            }
            if (Mushrooms.Count == 3)
            {
                posToGo = new Vector2((int)ui.boxPos3.X, (int)ui.boxPos3.Y);
            }
            var timeToMove = 5;
            var currentPos = mush.Position;
            var t = 0f;
            while (t < 1)
            {
                //Tempo em movimento
                t += 0.2f / timeToMove;
                //Musica
                //mover o boss
                mush.X = MathHelper.Lerp(currentPos.X, posToGo.X, t);
                mush.Y = MathHelper.Lerp(currentPos.Y, posToGo.Y, t);
                //balancar a cameravfx.ShakeCamera(0.2f);
                yield return null;
            }
            if (Mushrooms.Count == 3)
            {
                Mushrooms.Last().FateSelector();
                ResetMushrooms(Mushrooms.Last());
            }
        }

        //Resetar lista de cogumelos
        public void ResetMushrooms(Mushroom lastMushroom)
        {
            for (int i = Mushrooms.Count - 2; i >= 0; i--)
            {
                //TODO: Animar
                Mushrooms.RemoveAt(i);
            }

            Mushrooms.Remove(lastMushroom);
        }

        //Terminar o jogo
        public void GameOver()
        {
            Status = GameStatus.Failed;
            var remainingPanels = Panels.Where(p => !p.IsRevealed);

            //Revelar painéis restantes
            foreach (var panel in remainingPanels)
            {
                if (!panel.IsFlagged)
                {
                    panel.IsRevealed = true;
                }
            }
            //TODO: Remover e adicionar função de fim de jogo
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