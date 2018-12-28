using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace CampoMinado
{
    public class Input
    {
        readonly Game1 game;
        private MouseState lastMouseState;
        private MouseState currentMouseState;

        public bool MouseLeftPressed { get; private set; } = false;
        public bool MouseLeftHolding { get; private set; } = false;
        public bool MouseLeftReleased { get; private set; } = false;
        public bool MouseRightPressed { get; private set; } = false;
        public bool MouseRightHolding { get; private set; } = false;
        public bool MouseRightReleased { get; private set; } = false;
        public Vector2 MousePos { get; private set; } = Vector2.Zero;

        public Input(Game1 game)
        {
            this.game = game;
        }

        public void Update()
        {
            //if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            //    game.Exit();
            MouseInput();
        }

        private void MouseInput()
        {
            //O estado ativo do último frame
            lastMouseState = currentMouseState;

            //Estado relevanto do frame atual
            currentMouseState = Mouse.GetState();

            //Botão esquerdo do mouse
            MouseLeftReleased = lastMouseState.LeftButton == ButtonState.Pressed && currentMouseState.LeftButton == ButtonState.Released;
            MouseLeftPressed = lastMouseState.LeftButton == ButtonState.Released && currentMouseState.LeftButton == ButtonState.Pressed;
            MouseLeftHolding = lastMouseState.LeftButton == ButtonState.Pressed && currentMouseState.LeftButton == ButtonState.Pressed;

            //Botão direito
            MouseRightReleased = lastMouseState.RightButton == ButtonState.Pressed && currentMouseState.RightButton == ButtonState.Released;
            MouseRightPressed = lastMouseState.RightButton == ButtonState.Released && currentMouseState.RightButton == ButtonState.Pressed;
            MouseRightHolding = lastMouseState.RightButton == ButtonState.Pressed && currentMouseState.RightButton == ButtonState.Pressed;

            //Posição do mouse
            MousePos = new Vector2(currentMouseState.X, currentMouseState.Y);
        }
    }
}
