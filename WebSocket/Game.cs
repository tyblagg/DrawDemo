using System;
using System.Collections.Generic;
using System.Linq;

namespace DrawDemo.WebSocket
{
    public class Game
    {        
        public const string FakerText = "Fake It";

        public string Id { get; set; }
        public string ConnectCode { get; private set; }
        public Player Host { get; private set; }
        public Player Player1 { get; private set; }

        public Player Player2 { get; private set; }

        public Player Player3 { get; private set; }

        public Player Player4 { get; private set; }

        public Player Player5 { get; private set; }

        public Player Player6 { get; private set; }

        public Player Player7 { get; private set; }

        public Player Player8 { get; private set; }

        public Player CurrentPlayer { get; set; }

        public string[][] Board { get; private set; }
        public bool InProgress { get; set; }
        public bool Round2 { get; set; }
        public bool VotingRound { get; set; }
        public List<Player> PlayerOrder { get; set; }
        public List<Player> PlayerVotes { get; set; }
        public string FakerConnectionId { get; set; }

        public Game()
        {
            this.Id = Guid.NewGuid().ToString();
            this.ConnectCode = Guid.NewGuid().ToString().Substring(0, 5).ToUpper();
            this.Round2 = false;
            this.VotingRound = false;
            this.PlayerVotes = new List<Player>();

            Host = new Player();

            Player1 = new Player();
            Player1.Color = "#FF0000"; // Red

            Player2 = new Player();
            Player2.Color = "#00FF00"; // Lime

            Player3 = new Player();
            Player3.Color = "#0000FF"; // Blue

            Player4 = new Player();
            Player4.Color = "#FF00FF"; // Fuchsia

            Player5 = new Player();
            Player5.Color = "#800080"; // Purple

            Player6 = new Player();
            Player6.Color = "#FFFF00"; // Yellow

            Player7 = new Player();
            Player7.Color = "#008080"; // Teal

            Player8 = new Player();
            Player8.Color = "#008000"; // Green
        }

        public Player GetPlayer(string connectionId)
        {
            if (Player1 != null && Player1.ConnectionId == connectionId)
            {
                return Player1;
            }
            if (Player2 != null && Player2.ConnectionId == connectionId)
            {
                return Player2;
            }
            if (Player3 != null && Player3.ConnectionId == connectionId)
            {
                return Player3;
            }
            if (Player4 != null && Player4.ConnectionId == connectionId)
            {
                return Player4;
            }
            if (Player5 != null && Player5.ConnectionId == connectionId)
            {
                return Player5;
            }
            if (Player6 != null && Player6.ConnectionId == connectionId)
            {
                return Player6;
            }
            if (Player7 != null && Player7.ConnectionId == connectionId)
            {
                return Player7;
            }
            if (Player8 != null && Player8.ConnectionId == connectionId)
            {
                return Player8;
            }

            return null;
        }

        public bool HasPlayer(string connectionId)
        {
            if (Player1 != null && Player1.ConnectionId == connectionId)
            {
                return true;
            }
            if (Player2 != null && Player2.ConnectionId == connectionId)
            {
                return true;
            }
            if (Player3 != null && Player3.ConnectionId == connectionId)
            {
                return true;
            }
            if (Player4 != null && Player4.ConnectionId == connectionId)
            {
                return true;
            }
            if (Player5 != null && Player5.ConnectionId == connectionId)
            {
                return true;
            }
            if (Player6 != null && Player6.ConnectionId == connectionId)
            {
                return true;
            }
            if (Player7 != null && Player7.ConnectionId == connectionId)
            {
                return true;
            }
            if (Player8 != null && Player8.ConnectionId == connectionId)
            {
                return true;
            }
            return false;
        }

        public Player JoinPlayer(string connectionId, string name)
        {
            if (Player1 != null && string.IsNullOrEmpty(Player1.ConnectionId))
            {
                Player1.ConnectionId = connectionId;
                Player1.Name = name;
                return Player1;
            }
            if (Player2 != null && string.IsNullOrEmpty(Player2.ConnectionId))
            {
                Player2.ConnectionId = connectionId;
                Player2.Name = name;
                return Player2;
            }
            if (Player3 != null && string.IsNullOrEmpty(Player3.ConnectionId))
            {
                Player3.ConnectionId = connectionId;
                Player3.Name = name;
                return Player3;
            }
            if (Player4 != null && string.IsNullOrEmpty(Player4.ConnectionId))
            {
                Player4.ConnectionId = connectionId;
                Player4.Name = name;
                return Player4;
            }
            if (Player5 != null && string.IsNullOrEmpty(Player5.ConnectionId))
            {
                Player5.ConnectionId = connectionId;
                Player5.Name = name;
                return Player5;
            }
            if (Player6 != null && string.IsNullOrEmpty(Player6.ConnectionId))
            {
                Player6.ConnectionId = connectionId;
                Player6.Name = name;
                return Player6;
            }
            if (Player7 != null && string.IsNullOrEmpty(Player7.ConnectionId))
            {
                Player7.ConnectionId = connectionId;
                Player7.Name = name;
                return Player7;
            }
            if (Player8 != null && string.IsNullOrEmpty(Player8.ConnectionId))
            {
                Player8.ConnectionId = connectionId;
                Player8.Name = name;
                return Player8;
            }

            return null;
        }

        public void NextPlayer()
        {
            var i = PlayerOrder.IndexOf(CurrentPlayer);
            i++; // Get Next Index
            if (i >= PlayerOrder.Count())
            {
                if (!this.Round2)
                {
                    this.Round2 = true;
                }
                else if (!this.VotingRound)
                {
                    this.Round2 = false;
                    this.VotingRound = true;
                }

                i = 0;
            }

            CurrentPlayer = PlayerOrder[i];
        }
    }

    public class Player
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public string ConnectionId { get; set; }
        public string Color { get; set; }
        public string Word { get; set; }
        public string Vote { get; set; }
    }

    public class SketchPadPoint
    {
        public double x { get; set; }
        public double y { get; set; }
    }

    public class SketchPadItem
    {
        public string id { get; set; }
        public string tool { get; set; }
        public string color { get; set; }
        public int size { get; set; }
        public List<SketchPadPoint> points { get; set; }
    }
}