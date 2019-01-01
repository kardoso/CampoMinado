using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.ViewportAdapters;

namespace CampoMinado
{
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        readonly int panelColumns = 18;
        readonly int panelRows = 10;
        readonly int gridSize = 16;

        Texture2D background;
        Texture2D selector;
        Texture2D filled;
        Texture2D empty;
        Texture2D flag;
        Texture2D mine;

        Texture2D greenMush;
        Texture2D blueMush;
        Texture2D redMush;

        Input input;
        Board board;
        Camera2D camera;

        Coroutines coroutines = new Coroutines();

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            //definir tamanho da janela
            graphics.PreferredBackBufferWidth = (panelColumns * gridSize) * 2;
            graphics.PreferredBackBufferHeight = (panelRows * gridSize) * 2;

            Window.AllowAltF4 = true;
            Window.AllowUserResizing = true;
            IsMouseVisible = true;

            input = new Input(this);
        }

        protected override void Initialize()
        {
            //Definir nova textura
            background = new Texture2D(graphics.GraphicsDevice, 18 * 18, 18 * 9);
            //Colorir a textura do background
            Color[] data = new Color[(18 * 18) * (18 * 9)];
            for (int i = 0; i < data.Length; ++i) data[i] = new Color(200, 212, 93);
            background.SetData(data);

            //Criar novo jogo
            board = new Board(coroutines, new Point(0, 0), panelColumns, panelRows - 1, (panelColumns * (panelRows - 1)) / 4, 12, 16, 16);

            //Criar camera 2D
            var viewportAdapter = new BoxingViewportAdapter(Window, GraphicsDevice, (panelColumns * gridSize), (panelRows * gridSize));
            camera = new Camera2D(viewportAdapter)
            {
                Position = Vector2.Zero
            };

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            //Carregar imagem dos números 
            var numbers = Content.Load<Texture2D>("sprites/numbers");
            List<Texture2D> splitnumbers = new List<Texture2D>();
            //"Cortar" imagem dos numeros e armazena-las em uma lista
            for (int x = 0; x < numbers.Width; x += numbers.Width / 10)
            {
                for (int y = 0; y < numbers.Height; y += numbers.Height / 1)
                {
                    Rectangle sourceRectangle = new Rectangle(x, y, 16, 16);
                    Texture2D newTexture = new Texture2D(graphics.GraphicsDevice, 16, 16);
                    Color[] data = new Color[sourceRectangle.Width * sourceRectangle.Height];
                    numbers.GetData(0, sourceRectangle, data, 0, data.Length);
                    newTexture.SetData(data);
                    splitnumbers.Add(newTexture);
                }
            }

            //Carregar imagens
            selector = Content.Load<Texture2D>("sprites/selector");
            filled = Content.Load<Texture2D>("sprites/filled");
            empty = Content.Load<Texture2D>("sprites/empty");
            flag = Content.Load<Texture2D>("sprites/flag");
            mine = Content.Load<Texture2D>("sprites/mine");

            greenMush = Content.Load<Texture2D>("sprites/green_mushroom");
            blueMush = Content.Load<Texture2D>("sprites/blue_mushroom");
            redMush = Content.Load<Texture2D>("sprites/red_mushroom");

            //Atribuir imagens
            foreach (Panel panel in board.Panels)
            {

                panel.numbers = splitnumbers;
                panel.selected = selector;
                panel.filled = filled;
                panel.empty = empty;
                panel.flag = flag;
                panel.mine = mine;
            }
            board.greenMush = greenMush;
            board.blueMush = blueMush;
            board.redMush = redMush;
        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        protected override void Update(GameTime gameTime)
        {
            //Atualizar corotinas
            coroutines.Update();
            //Atualizar input
            input.Update();

            //Posição do mouse relativo à tela
            Vector2 mPos = camera.ScreenToWorld(input.MousePos);

            if (board.Status == GameStatus.InProgress)
            {
                foreach (Panel panel in board.Panels)
                {
                    var value = panel.Update(mPos, input.MouseLeftReleased, input.MouseRightReleased);
                    if (value == 1)
                    {
                        board.Reveal(panel.X, panel.Y);
                    }
                    else if (value == 2)
                    {
                        board.Flag(panel.X, panel.Y);
                    }
                }
            }
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            //Desenhar na camera
            var transformMatrix = camera.GetViewMatrix();
            spriteBatch.Begin(transformMatrix: transformMatrix, blendState: null, samplerState: SamplerState.PointClamp);

            //Desenhar background
            spriteBatch.Draw(background, Vector2.Zero, Color.White);

            //Desenhar paineis
            foreach (Panel panel in board.Panels)
            {
                panel.Draw(spriteBatch);
            }

            //Desenhar cogumelos
            foreach (Mushroom mush in board.Mushrooms)
            {
                mush.Draw(spriteBatch);
            }

            spriteBatch.End();


            base.Draw(gameTime);
        }
    }
}