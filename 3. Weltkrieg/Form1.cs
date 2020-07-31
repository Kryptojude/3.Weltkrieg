using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace _3.Weltkrieg
{
    public partial class Form1 : Form
    {
        Rectangle[,] tiles = new Rectangle[30, 60];
        object tilesLock = new object();
        static SolidBrush[,] tileColors = new SolidBrush[30, 60];
        static int ts = 20;
        Random random = new Random();
        int[] seeds = new int[5];

        public class Player
        {
            public List<Rectangle> recs = new List<Rectangle>();
            public SolidBrush brush;
            public object ListLock = new object();

            public void Add(Rectangle rec)
            {
                recs.Add(rec);
                tileColors[rec.Y / ts, rec.X / ts] = brush;
            }
        }

        static Player[] players = new Player[5] { new Player { brush = new SolidBrush(Color.Blue) }, new Player { brush = new SolidBrush(Color.Orange) }, new Player { brush= new SolidBrush(Color.Black) }, new Player { brush = new SolidBrush(Color.Green) }, new Player { brush = new SolidBrush(Color.White) } };
        Dictionary<Color, Player> colorTranslator = new Dictionary<Color, Player> { { Color.Blue, players[0] }, { Color.White, players[4] }, { Color.Orange, players[1] }, { Color.Black, players[2] }, { Color.Green, players[3] } };

        public Form1()
        {
            InitializeComponent();

            DoubleBuffered = true;
            Height = ts * tiles.GetLength(0) + 39;
            Width = ts * tiles.GetLength(1) + 16;

            for (int y = 0; y < tiles.GetLength(0); y++)
                for (int x = 0; x < tiles.GetLength(1); x++)
                {
                    tiles[y, x] = new Rectangle( x * ts, y * ts, ts, ts );
                    tileColors[y, x] = new SolidBrush(Color.Gray);
                }


            for (int s = 0; s < seeds.Length; s++)
            {
                seeds[s] = random.Next();
            }

            players[0].Add(tiles[random.Next(30), random.Next(60)]);
            players[1].Add(tiles[random.Next(30), random.Next(60)]);
            players[2].Add(tiles[random.Next(30), random.Next(60)]);
            players[3].Add(tiles[random.Next(30), random.Next(60)]);
            players[4].Add(tiles[random.Next(30), random.Next(60)]);

            KeyDown += Form1_KeyDown;
            Paint += Form1_Paint;

            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer { Enabled = true, Interval = 200 };
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            Refresh();
        }

        private void Form1_KeyDown(object sender, EventArgs e)
        {
            Thread thread1 = new Thread(new ParameterizedThreadStart(AI));
            thread1.Start(0);
            Thread thread2 = new Thread(new ParameterizedThreadStart(AI));
            thread2.Start(1);
            Thread thread3 = new Thread(new ParameterizedThreadStart(AI));
            thread3.Start(2);
            Thread thread4 = new Thread(new ParameterizedThreadStart(AI));
            thread4.Start(3);
            Thread thread5 = new Thread(new ParameterizedThreadStart(AI));
            thread5.Start(4);
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            for (int y = 0; y < tiles.GetLength(0); y++)
                for (int x = 0; x < tiles.GetLength(1); x++)
                     e.Graphics.FillRectangle(tileColors[y, x], tiles[y, x]);
        }

        private void AI(object o)
        {
            Player p = players[(int)o];
            Thread.CurrentThread.Name = p.brush.Color.Name;
            Random random = new Random(seeds[(int)o]);
            Point[] directions = new Point[4] { new Point(0, -1), new Point(1, 0), new Point(0, 1), new Point(-1, 0) };
            bool stop2 = true;
            while (stop2)
            {
                bool stop = true;
                while (stop) //Check random tile of player
                {
                    //Thread.Sleep(1);
                    Rectangle randomTile;
                    lock (p.ListLock)
                    {

                        if (p.recs.Count > 0)
                            randomTile = p.recs[random.Next(0, p.recs.Count)]; //Get one of my tiles
                        else
                        {
                            randomTile = new Rectangle();
                            stop2 = false;
                            break;
                        }
                    }

                    Point labelPos = new Point(randomTile.Left / 20, randomTile.Top / 20);

                    foreach (Point dir in directions)
                    {
                        if (labelPos.Y + dir.Y >= 0 && labelPos.Y + dir.Y < 30 &&
                            labelPos.X + dir.X >= 0 && labelPos.X + dir.X < 60)
                        {
                            Rectangle newTile;
                            SolidBrush tileColor;
                            lock (tilesLock)
                            {
                                newTile = tiles[labelPos.Y + dir.Y, labelPos.X + dir.X]; //Get adjacent tile
                                tileColor = tileColors[labelPos.Y + dir.Y, labelPos.X + dir.X];
                            }
                            if (tileColor.Color != p.brush.Color) //empty or enemy
                            {
                                if (tileColor.Color != Color.Gray) //enemy
                                {
                                    Player enemy = colorTranslator[tileColor.Color];
                                    lock (enemy.ListLock)
                                        enemy.recs.Remove(newTile);
                                }

                                lock (p.ListLock)
                                    p.Add(newTile);
                                Invalidate(newTile);
                                stop = false;
                                //break;
                            }
                        }
                    }
                }
            }
        }
    }
}
