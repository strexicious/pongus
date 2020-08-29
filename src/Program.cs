using System;
using SFML.Window;
using SFML.System;
using SFML.Graphics;

namespace pongus
{
    class Paddle
    {
        public uint Width { get; }
        public uint Height { get; }
        public Vector2f pos;

        public Paddle(uint width, uint height, Vector2f pos)
        {
            this.Width = width;
            this.Height = height;
            this.pos = pos;
        }
    }

    class Ball
    {
        public uint Radius { get; }
        public Vector2f pos;
        public Vector2f dir;

        public Ball(uint radius, Vector2f pos, Vector2f dir)
        {
            this.Radius = radius;
            this.pos = pos;
            this.dir = dir;
        }
    }

    class BallPaddleCollider
    {
        // rect is axis align rectangle with 4 points from top-left clockwise
        bool CircleInRectangle(Vector2f center, Vector2f[] rect)
        {
            return rect[0].X < center.X && center.X < rect[1].X
                && rect[0].Y > center.Y && center.Y > rect[3].Y;
        }

        // line is defined by two points
        bool LineIntersectsCircle(Vector2f center, float radius, Vector2f p0, Vector2f p1)
        {
            // calculate for line-segment ranges
            float[] xRange = {
                Math.Min(p0.X, p1.X),
                Math.Max(p0.X, p1.X),
            };
            float[] yRange = {
                Math.Min(p0.Y, p1.Y),
                Math.Max(p0.Y, p1.Y),
            };

            // treat the edge case where line is vertical
            if (p1.X - p0.X == 0)
            {
                return radius > Math.Abs(center.X - p0.X)
                    && yRange[0] < center.Y
                    && yRange[1] > center.Y;
            }

            // treat the edge case where line is horizontal
            if (p1.Y - p0.Y == 0)
            {
                return radius > Math.Abs(center.Y - p0.Y)
                    && xRange[0] < center.X
                    && xRange[1] > center.X;
            }

            // line equation
            var m1 = (p1.Y - p0.Y) / (p1.X - p0.X);
            var n1 = p0.Y - m1 * p0.X;

            // perpendicular line equation passing through center
            var m2 = -1.0f / m1;
            var n2 = center.Y - m2 * center.X;

            // solve for intersection
            var x = (n2 - n1) / (m1 - m2);
            var y = m1 * x + n1;

            // calculate distance
            var d = (float)Math.Sqrt(Math.Pow(x - center.X, 2.0) + Math.Pow(y - center.Y, 2.0));

            return radius > d
                && xRange[0] < center.X
                && xRange[1] > center.X
                && yRange[0] < center.Y
                && yRange[1] > center.Y;
        }

        public bool Collides(Ball ball, Paddle padd)
        {
            var halfW = padd.Width / 2.0f;
            var halfH = padd.Height / 2.0f;
            Vector2f[] rect = {
                padd.pos + new Vector2f(-halfW, -halfH),
                padd.pos + new Vector2f( halfW, -halfH),
                padd.pos + new Vector2f( halfW,  halfH),
                padd.pos + new Vector2f(-halfW,  halfH),
            };

            return CircleInRectangle(ball.pos, rect)
                || LineIntersectsCircle(ball.pos, (float)ball.Radius, rect[0], rect[1])
                || LineIntersectsCircle(ball.pos, (float)ball.Radius, rect[1], rect[2])
                || LineIntersectsCircle(ball.pos, (float)ball.Radius, rect[2], rect[3])
                || LineIntersectsCircle(ball.pos, (float)ball.Radius, rect[3], rect[0]);
        }
    }

    class Program
    {
        const uint WINDOW_WIDTH = 800;
        const uint WINDOW_HEIGHT = 600;
        const uint PADDLE_WIDTH = 20;
        const uint PADDLE_HEIGHT = 100;
        const float PADDLE_SPEED = WINDOW_HEIGHT;
        const float BALL_SPEED = WINDOW_WIDTH * 0.6f;

        static Ball ball;
        static SoundAnalyzer analyzer = new SoundAnalyzer("res/copyrighted/Mr. Jazzek - Alla Turca-jH1ooHogiXM.wav");
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
            };

            var rand = new Random(12345);
            var randX = (float)rand.NextDouble(); // just gonna assume it's not 0.0
            var randY = (float)rand.NextDouble();
            var dirLen = (float)Math.Sqrt(Math.Pow(randX, 2.0) + Math.Pow(randY, 2.0));
            var randDir = new Vector2f(randX, randY);
            ball = new Ball(8, new Vector2f(WINDOW_WIDTH / 2.0f, WINDOW_HEIGHT / 2.0f), randDir / dirLen);

            var clock = new Clock();
            while (mainWindow.IsOpen)
            {
                delta = clock.Restart();
                mainWindow.DispatchEvents();

                Update();

                mainWindow.Clear(Color.Black);
                Draw();
                mainWindow.Display();
            }
        }

        static void MovePaddle(float distance, Paddle padd)
        {
            padd.pos.Y = Math.Min(WINDOW_HEIGHT - PADDLE_HEIGHT / 2.0f, Math.Max(PADDLE_HEIGHT / 2.0f, padd.pos.Y + distance));
        }

        static void MoveBall(float distance, Ball ball)
        {
            ball.pos += distance * ball.dir;

            // check collision with appropriate paddle according to cosine sign
            var hyp = Math.Sqrt(Math.Pow(ball.dir.X, 2.0) + Math.Pow(ball.dir.Y, 2.0));
            var padd = (ball.dir.X / hyp) > 0.0 ? padds[1] : padds[0];

            var collider = new BallPaddleCollider();
            if (collider.Collides(ball, padd))
            {
                ball.dir.X *= -1.0f;
            }

            // check if goes outside the laterals of the window
            // for now just reset ball position
            if (ball.pos.X - ball.Radius < 0 || ball.pos.X + ball.Radius >= WINDOW_WIDTH)
            {
                ball.pos.X = WINDOW_WIDTH / 2.0f;
                ball.pos.Y = WINDOW_HEIGHT / 2.0f;
                ball.dir.X *= -1.0f;
            }
            
            // check if bounces on non-laterals of the window
            if (ball.pos.Y - ball.Radius < 0)
            {
                ball.pos.Y = ball.Radius;
                ball.dir.Y *= -1.0f;
            }

            if (ball.pos.Y + ball.Radius >= WINDOW_HEIGHT)
            {
                ball.pos.Y = WINDOW_HEIGHT - 1 - ball.Radius;
                ball.dir.Y *= -1.0f;
            }
        }

        static void Update()
        {
            if (Keyboard.IsKeyPressed(Keyboard.Key.Up))
            {
                MovePaddle(delta.AsSeconds() * -PADDLE_SPEED, padds[controlledPaddle]);
            }

            if (Keyboard.IsKeyPressed(Keyboard.Key.Down))
            {
                MovePaddle(delta.AsSeconds() * PADDLE_SPEED, padds[controlledPaddle]);
            }

            MoveBall(delta.AsSeconds() * BALL_SPEED, ball);
        }

        static void DrawPaddle(Paddle padd)
        {
            var paddleShape = new RectangleShape(new Vector2f(PADDLE_WIDTH, PADDLE_HEIGHT));
            paddleShape.FillColor = Color.White;
            paddleShape.Origin = new Vector2f(PADDLE_WIDTH / 2.0f, PADDLE_HEIGHT / 2.0f);
            paddleShape.Position = (Vector2f)padd.pos;

            mainWindow.Draw(paddleShape);
        }

        static void DrawBall(Ball ball)
        {
            var circleShape = new CircleShape(ball.Radius);
            circleShape.FillColor = Color.White;
            circleShape.Origin = new Vector2f(ball.Radius, ball.Radius);
            circleShape.Position = (Vector2f)ball.pos;

            mainWindow.Draw(circleShape);
        }

        static void Draw()
        {
            DrawPaddle(padds[0]);
            DrawPaddle(padds[1]);
            DrawBall(ball);
        }
    }
}
