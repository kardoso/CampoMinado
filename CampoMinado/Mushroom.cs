using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CampoMinado
{
    public class Mushroom
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int W { get; set; }
        public int H { get; set; }
        public Vector2 Position { get { return new Vector2(X, Y); } }
        public MushroomType mushType;
        public Board board;

        private Texture2D sprite;

        public Mushroom(Board board, int x, int y, int w, int h, Random rand, Texture2D greenMush, Texture2D blueMush, Texture2D redMush)
        {
            this.board = board;
            W = w;
            H = h;
            X = x * W;
            Y = y * H;
            Array values = Enum.GetValues(typeof(MushroomType));
            mushType = (MushroomType)values.GetValue(rand.Next(0, values.Length));
            switch (mushType)
            {
                case MushroomType.Green:
                    sprite = greenMush;
                    break;
                case MushroomType.Blue:
                    sprite = blueMush;
                    break;
                case MushroomType.Red:
                    sprite = redMush;
                    break;  
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if(sprite != null)
                spriteBatch.Draw(sprite, Position, null, Color.White, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0);
        }

        public void FateSelector()
        {
            switch (mushType)
            {
                case MushroomType.Green:
                    board.RandomFlag();
                    break;
                case MushroomType.Blue:
                    board.AddChance();
                    break;
                case MushroomType.Red:
                    board.NewMine();
                    break;
            }
        }
    }

    public enum MushroomType
    {
        Green,
        Blue,
        Red
    }
}