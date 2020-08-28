using System;
using SFML.Window;
using SFML.System;
using SFML.Graphics;

namespace pongus
{
    class Paddle
    {
        public uint width { get; }
        public uint height { get; }
        // from -1 to 1 indicating how far from vertical center
        public Vector2f pos;

        public Paddle(uint width, uint height, Vector2f pos)
        {
            this.width = width;
            this.height = height;
            this.pos = pos;
        }
    }

    class Program
    {
        const uint WINDOW_WIDTH = 800;
        const uint WINDOW_HEIGHT = 600;
        const uint PADDLE_WIDTH = 20;
        const uint PADDLE_HEIGHT = 100;
        const float PADDLE_SPEED = WINDOW_HEIGHT;

        static RenderWindow mainWindow = new RenderWindow(new VideoMode(WINDOW_WIDTH, WINDOW_HEIGHT), "pongus reborn", Styles.Default & ~Styles.Resize);
        static Paddle[] padds = {
            // left
            new Paddle(PADDLE_WIDTH, PADDLE_HEIGHT, new Vector2f(10 + PADDLE_WIDTH / 2.0f, WINDOW_HEIGHT / 2.0f)),

            // right
            new Paddle(PADDLE_WIDTH, PADDLE_HEIGHT, new Vector2f(WINDOW_WIDTH - (10 + PADDLE_WIDTH / 2.0f), WINDOW_HEIGHT / 2.0f)),
        };
        static Time delta = Time.Zero;
        static int controlledPaddle = 0;

        static void Main(string[] args)
        {
            mainWindow.SetVerticalSyncEnabled(true);
            
            mainWindow.Closed += (object sender, EventArgs args) =>
            {
                mainWindow.Close();
            };

            mainWindow.KeyPressed += (object sender, KeyEventArgs args) =>
            {
                if (args.Code == Keyboard.Key.Tab)
                {
                    controlledPaddle = (controlledPaddle + 1) % 2;
                }

                if (args.Code == Keyboard.Key.Up)
                {
                    MovePaddle(delta.AsSeconds() * -PADDLE_SPEED, padds[controlledPaddle]);
                }

                if (args.Code == Keyboard.Key.Down)
                {
                    MovePaddle(delta.AsSeconds() * PADDLE_SPEED, padds[controlledPaddle]);
                }
            };

            var clock = new Clock();
            while (mainWindow.IsOpen)
            {
                delta = clock.Restart();
                mainWindow.DispatchEvents();

                mainWindow.Clear(Color.Black);
                Draw();
                mainWindow.Display();
            }
        }

        static void MovePaddle(float distance, Paddle padd)
        {
            padd.pos.Y = Math.Min(WINDOW_HEIGHT - PADDLE_HEIGHT / 2.0f, Math.Max(PADDLE_HEIGHT / 2.0f, padd.pos.Y - distance));
        }

        static void DrawPaddle(Paddle padd)
        {
            var paddle_shape = new RectangleShape(new Vector2f(PADDLE_WIDTH, PADDLE_HEIGHT));
            paddle_shape.FillColor = Color.White;
            paddle_shape.Origin = new Vector2f(PADDLE_WIDTH / 2.0f, PADDLE_HEIGHT / 2.0f);
            paddle_shape.Position = (Vector2f)padd.pos;

            mainWindow.Draw(paddle_shape);
        }

        static void Draw()
        {
            DrawPaddle(padds[0]);
            DrawPaddle(padds[1]);
        }
    }
}
