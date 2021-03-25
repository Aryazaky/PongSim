using System;
using System.Threading;

namespace Pong
{
    class Program
    {
        class Score
        {
            public int[] player_score = new int[2];
            public Score()
            {
                player_score[0] = 0;
                player_score[1] = 0;
            }
        }
        static Score score = new Score();
        class Player : ICloneable
        {
            public enum playerID
            {
                player1,
                player2
            }
            public Player() { }
            public void Set(playerID ID, string Name)
            {
                id = ID;
                name = Name;
            }
            public string name;
            public playerID id;
            public float hit;
            Random rand = new Random();
            public object Clone()
            {
                return this.MemberwiseClone();
            }
            public void RandomizeHit()
            {
                hit = rand.Next(101);
            }
        }

        static void SimulatePong(Player player, ref ManualResetEvent pauseEvent, ref ManualResetEvent otherPauseEvent, ref ManualResetEvent endEvent)
        {
            while (true)
            {
                pauseEvent.WaitOne(Timeout.Infinite);

                if (endEvent.WaitOne(0))
                {
                    break;
                }
                player.RandomizeHit();
                Console.WriteLine($"Giliran {player.name}. hit {player.hit}");
                if (player.hit <= 50)
                {
                    score.player_score[((int)player.id + 1) % 2] += 1;
                    Console.WriteLine($"Skor sekarang: Player1 {score.player_score[0]} | Player2 {score.player_score[1]}");
                }
                else
                {
                    otherPauseEvent.Set();
                    pauseEvent.Reset();
                }
            }
        }
        static void Main(string[] args)
        {
            Player player1 = new Player();
            Player player2 = (Player)player1.Clone();
            player1.Set(Player.playerID.player1, "A");
            player2.Set(Player.playerID.player2, "B");

            ManualResetEvent _pauseEvent1 = new ManualResetEvent(true);
            ManualResetEvent _pauseEvent2 = new ManualResetEvent(true);
            ManualResetEvent _endEvent = new ManualResetEvent(false);

            Thread thread1 = new Thread(() => SimulatePong(player1, ref _pauseEvent1, ref _pauseEvent2, ref _endEvent));
            Thread thread2 = new Thread(() => SimulatePong(player2, ref _pauseEvent2, ref _pauseEvent1, ref _endEvent));

            Random rand = new Random();
            if (rand.Next(2) > 1)
            {
                _pauseEvent1.Set();
                _pauseEvent2.Reset();
                Console.WriteLine($"{player1.name} duluan");
            }
            else
            {
                _pauseEvent1.Reset();
                _pauseEvent2.Set();
                Console.WriteLine($"{player2.name} duluan");
            }
            thread1.Start();
            thread2.Start();

            while (true)
            {
                if (score.player_score[0] >= 10 || score.player_score[1] >= 10)
                {
                    if(score.player_score[0] >= 10)
                    {
                        Console.WriteLine($"{player1.name} menang!");
                    }
                    else if(score.player_score[1] >= 10)
                    {
                        Console.WriteLine($"{player2.name} menang!");
                    }
                    _endEvent.Set();
                }
                if (_endEvent.WaitOne(0))
                {
                    thread1.Join();
                    thread2.Join();
                    break;
                }
            }
        }
    }
}
