using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CampoMinado
{
    public class Panel
    {
        public int ID { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int W { get; set; }
        public int H { get; set; }
        public bool IsMine { get; set; }
        public bool IsMushroom { get; set; }
        public int AdjacentMines { get; set; }
        public bool IsRevealed { get; set; }
        public bool IsFlagged { get; set; }
        public Vector2 Position { get { return new Vector2(X * W, Y * H); } }
        public Board board;
        public bool ShowSelection = false;

        public List<Texture2D> numbers;
        public Texture2D selected;
        public Texture2D filled;
        public Texture2D empty;
        public Texture2D flag;
        public Texture2D mine;

        public Panel(Board _board, int id, int x, int y, int w, int h)
        {
            board = _board;
            ID = id;
            X = x;
            Y = y;
            W = w;
            H = h;
        }

        public int Update(Vector2 mousePos, bool mouseLeft, bool mouseRight)
        {
            if (!IsRevealed)
            {
                if ((mousePos.X > Position.X && mousePos.X < Position.X + W) && (mousePos.Y > Position.Y && mousePos.Y < Position.Y + H))
                {
                    //Se a posição do mouse está dentro da posição do painel
                    //Mostrar a caixa de seleção no painel
                    ShowSelection = true;
                    if (mouseLeft)
                    {
                        //Retorna 1 se o botão esquerdo do mouse for solto
                        return 1;
                    }
                    if (mouseRight)
                    {
                        //Retorna 1 se o botão direito do mouse for solto
                        return 2;
                    }
                }
                else
                {
                    //Se a posição do mouse não está dentro da posição do painel
                    //Não mostrar a caixa de seleção no painel
                    ShowSelection = false;
                    return 0;
                }
            }

            return 0;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (!IsRevealed)
            {
                spriteBatch.Draw(filled, Position, null, Color.White, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0);
                if (IsFlagged)
                {
                    spriteBatch.Draw(flag, Position, null, Color.White, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0);
                }
            }
            else
            {
                spriteBatch.Draw(empty, Position, null, Color.White, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0);

                if (IsMine)
                {
                    spriteBatch.Draw(mine, Position, null, Color.White, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0);
                }
                else
                {
                    if (AdjacentMines != 0)
                        spriteBatch.Draw(numbers[AdjacentMines], Position, null, Color.White, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0);
                }
            }

            if (ShowSelection)
            {
                spriteBatch.Draw(selected, Position, null, Color.White, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0);
            }

        }
    }
}