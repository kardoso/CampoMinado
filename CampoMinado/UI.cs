using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CampoMinado
{
    public class UI
    {
        public Texture2D mushBox;
        public Vector2 boxPos;
        public Vector2 boxPos2;
        public Vector2 boxPos3;
        public int boxSize = 16;

        public UI(Vector2 mushroomBoxesPos)
        {
            boxPos = mushroomBoxesPos;
            boxPos2 = new Vector2(boxPos.X + boxSize, boxPos.Y);
            boxPos3 = new Vector2(boxPos.X + (boxSize*2), boxPos.Y);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(mushBox, boxPos, null, Color.White, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0);
            spriteBatch.Draw(mushBox, boxPos2, null, Color.White, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0);
            spriteBatch.Draw(mushBox, boxPos3, null, Color.White, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0);
        }
    }
}
