using System.Text;

namespace SnakeGame
{
    public class Program
    {
        public int Height = 10;
        public int Width = 20;
        public StringBuilder builder = new StringBuilder();
        public (int x, int y) Direction = (1, 0);
        public SnakeCell Snake;
        public AppleCell apple;
        public bool gameover;
        public ulong Score;

        static void Main(string[] args)
        {
            Program program = new Program();

            program.Start();

            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = 200;
            timer.Elapsed += (sender, e) =>
            {
                Console.CursorVisible = false;

                program.ClearBuilder();

                program.Clear();

                program.Update();

                program.Flush();
            };
            timer.Start();

            while (true) { }
        }

        public void Start()
        {
            GameStart();
        }

        public void Update()
        {
            Input();

            WriteBoard();

            if (!gameover) MovePlayer();

            SetPlayer();

            if (!gameover) AppleDetect();

            SetApple();

            if (!gameover) WallDetect();

            if (!gameover) SnakeDetect();

            PrintScore();

            if (gameover) PrintGameOver();
        }

        /// <summary>
        /// ボードを描く
        /// </summary>
        public void WriteBoard()
        {
            for (int y = 0; y < Height + 2; y++)
            {
                for (int x = 0; x < Width + 2; x++)
                {
                    if (y == 0 || y == Height + 2 - 1)
                    {
                        SetPoint(x, y, '-');
                    }

                    if (x == 0 || x == Width + 2 - 1)
                    {
                        SetPoint(x, y, '|');
                    }

                    if ((x == 0 || x == Width +2 - 1) && (y == 0 || y == Height + 2 - 1))
                    {
                        SetPoint(x, y, '+');
                    }
                }
            }
        }

        /// <summary>
        /// builderを綺麗にする
        /// </summary>
        public void ClearBuilder()
        {
            builder = new StringBuilder();
            builder.Length = Console.WindowHeight * Console.WindowWidth;
        }

        /// <summary>
        /// 特定の位置に文字を打つ
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="c"></param>
        public void SetPoint(int x, int y, char c)
        {
            int index = (Console.WindowHeight - y - 1) * Console.WindowWidth + x;

            if (builder.Length > index && 0 <= index)
            {
                builder[index] = c;
            }
        }

        /// <summary>
        /// 特定の位置に文字列を打つ
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="s"></param>
        public void Write(int x, int y, string s)
        {
            for (int i = 0; i < s.Length; i++)
            {
                SetPoint(x + i, y, s[i]);
            }
        }

        /// <summary>
        /// builderを全て空白にする
        /// </summary>
        public void Clear()
        {
            for (int y = 0; y < Console.WindowHeight; y++)
            {
                for (int x = 0; x < Console.WindowWidth; x++)
                {
                    SetPoint(x, y, ' ');
                }
            }
        }

        /// <summary>
        /// 描画
        /// </summary>
        public void Flush()
        {
            Console.SetCursorPosition(0, 0);
            byte[] buffer = Encoding.UTF8.GetBytes(builder.ToString());

            using (Stream stdout = Console.OpenStandardOutput())
            {
                stdout.Write(buffer, 0, buffer.Length);
            }
        }

        /// <summary>
        /// プレイヤーを描画
        /// </summary>
        public void SetPlayer()
        {
            Snake.Write(this);
        }

        /// <summary>
        /// ディレクションを使ってプレイヤーを動かす
        /// </summary>
        public void MovePlayer()
        {
            Snake.ChangePos(Snake.CurrentPos.x + Direction.x, Snake.CurrentPos.y + Direction.y);
        }

        /// <summary>
        /// キー入力を受け取る
        /// </summary>
        public void Input()
        {
            if (Console.KeyAvailable)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                (int x, int y) d = (0, 0);

                switch (keyInfo.Key)
                {
                    case ConsoleKey.RightArrow:
                        d = (1, 0);
                        break;
                    case ConsoleKey.LeftArrow:
                        d = (-1, 0);
                        break;
                    case ConsoleKey.UpArrow:
                        d = (0, 1);
                        break;
                    case ConsoleKey.DownArrow:
                        d = (0, -1);
                        break;
                    case ConsoleKey.Spacebar:
                        GameStart();
                        d = Direction;
                        break;
                    default:
                        d = Direction;
                        break;
                }

                if (Direction != (-d.x, -d.y))
                {
                    Direction = d;
                }
            }
        }

        /// <summary>
        /// リンゴを描画
        /// </summary>
        public void SetApple()
        {
            apple.Write();
        }

        /// <summary>
        /// 餌に当たったときに自分を増やす
        /// </summary>
        public void AppleDetect()
        {
            if (Snake.CurrentPos == apple.Pos)
            {
                Snake.CreateChild();
                apple.ChangePosition();
                Score++;
            }
        }

        /// <summary>
        /// 壁に当たったときに処理を止める
        /// </summary>
        public void WallDetect()
        {
            (int x, int y) pos = Snake.CurrentPos;
            if (pos.x <= -1 || pos.x >= Width || pos.y <= -1 || pos.y >= Height) 
            {
                OnGameOver();
            }
        }

        /// <summary>
        /// スネーク自身に当たったときに処理を止める
        /// </summary>
        public void SnakeDetect()
        {
            if (Snake.OnHead())
            {
                OnGameOver();
            }
        }

        /// <summary>
        /// ゲームオーバー時の処理
        /// </summary>
        public void OnGameOver()
        {
            gameover = true;
        }

        /// <summary>
        /// ゲームをスタートする処理
        /// </summary>
        public void GameStart()
        {
            gameover = false;
            Snake = new SnakeCell(Width / 2, Height / 2, true);
            apple = new AppleCell(this);
            Score = 0;

            Direction = (1, 0);

            apple.ChangePosition();
            for (int i = 0; i < 3; i++)
            {
                Snake.CreateChild();
            }
        }

        /// <summary>
        /// スコアを表示する
        /// </summary>
        public void PrintScore()
        {
            Write(0, Height + 2, "SCORE : " + Score.ToString());
        }

        /// <summary>
        /// ゲームオーバーの文字を表示する
        /// </summary>
        public void PrintGameOver()
        {
            string s = " GAME OVER ";
            int w = Width / 2 - s.Length / 2;
            int h = Height / 2 + 1;

            string s1 = "";
            for (int i = 0; i < s.Length; i++) s1 += ' ';

            Write(w, h + 1, s1);
            Write(w, h, s);
            Write(w, h - 1, s1);
        }
    }

    /// <summary>
    /// スネーク本体のクラス
    /// </summary>
    public class SnakeCell
    {
        public bool isHead;
        public (int x, int y) NextPos;
        public (int x, int y) CurrentPos;
        public bool isFirstFrame;
        public SnakeCell? ChildCell;

        public SnakeCell(int x = 0, int y = 0, bool isHead = false)
        {
            CurrentPos = (x, y);
            this.isHead = isHead;
            isFirstFrame = true;
        }

        /// <summary>
        /// 位置を変える
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void ChangePos(int x, int y)
        {
            NextPos = (x, y);
            if (ChildCell != null)
            {
                ChildCell.ChangePos(CurrentPos.x, CurrentPos.y);
            }
            CurrentPos = NextPos;
        }

        /// <summary>
        /// セルを増やす
        /// </summary>
        public void CreateChild()
        {
            if (ChildCell == null)
            {
                ChildCell = new SnakeCell(CurrentPos.x, CurrentPos.y);
            }
            else
            {
                ChildCell.CreateChild();
            }
        }

        /// <summary>
        /// 自身を描画する
        /// </summary>
        /// <param name="program"></param>
        public void Write(Program program)
        {
            if (ChildCell != null)
            {
                ChildCell.Write(program);
            }

            char c = '*';
            if (isHead)
            {
                c = '@';
            }
            program.SetPoint(CurrentPos.x + 1, CurrentPos.y + 1, c);
        }

        /// <summary>
        /// 頭に当たっているか
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool OnHead(int x = 0, int y = 0)
        {
            if (isHead)
            {
                x = CurrentPos.x;
                y = CurrentPos.y;
            }
            else
            {
                if (!isFirstFrame)
                {
                    if (x == CurrentPos.x && y == CurrentPos.y)
                    {
                        return true;
                    }
                }
            }

            if (ChildCell != null)
            {
                return ChildCell.OnHead(x, y);
            }

            isFirstFrame = false;

            return false;
        }
    }

    /// <summary>
    /// スネークの餌
    /// </summary>
    public class AppleCell
    {
        public (int x, int y) Pos;
        public Program program;

        public AppleCell(Program program)
        {
            this.program = program;
        }

        public void ChangePosition()
        {
            Random r = new Random();

            Pos = (r.Next(0, program.Width), r.Next(0, program.Height));
        }

        public void Write()
        {
            program.SetPoint(Pos.x + 1, Pos.y + 1, 'o');
        }
    }
}