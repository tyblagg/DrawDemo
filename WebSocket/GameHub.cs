using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DrawDemo.WebSocket
{
    public interface IGameClient
    {
        Task ReadyPlayer(Player player);
        Task Joined(Player player);
        Task Waiting(List<Player> players);
        Task Turn(Player player);
        Task UpdateCanvas(List<SketchPadItem> item);
        Task ConnectCode(string connectCode);
        Task SendCategory(string category);
        Task TimesUpSent();
        Task Round2Start();
        Task VotingRound(List<Player> vote);
        Task PlayersVotedFor(Player player);
        Task VotesNotMet();
        Task Concede();
    }

    public class GameHub : Hub<IGameClient>
    {
        private IGameRepository _repository;
        private readonly Random _random;

        public GameHub(IGameRepository repository, Random random)
        {
            _repository = repository;
            _random = random;
        }

        public async Task TimesUp()
        {
            var game = _repository.Games.FirstOrDefault(g => g.Host.ConnectionId == Context.ConnectionId);
            if (game is null)
            {
                //Ignore click if no game exists
                return;
            }

            await Clients.Client(game.CurrentPlayer.ConnectionId).TimesUpSent();
        }

        public async Task SubmitDrawing(List<SketchPadItem> items)
        {
            var game = _repository.Games.FirstOrDefault(g => g.HasPlayer(Context.ConnectionId));
            if (game is null)
            {
                //Ignore click if no game exists
                return;
            }

            if (Context.ConnectionId != game.CurrentPlayer.ConnectionId)
            {
                //Ignore player if it's not their turn
                return;
            }

            //ignore games that havent started
            if (!game.InProgress) return;

            // Send drawing to other canvases
            await Clients.Group(game.Id.ToString()).UpdateCanvas(items);

            // Select the next Player
            game.NextPlayer();
            
            // Check of two rounds are over and voting should begin
            if (game.VotingRound)
            {
                var votingPlayers = new List<Player>();
                game.PlayerOrder.ForEach(delegate (Player p)
                {
                    p.Word = "";
                    votingPlayers.Add(p);
                });
                await Clients.Group(game.Id).VotingRound(votingPlayers);
            }
            else if (game.CurrentPlayer == game.PlayerOrder.FirstOrDefault() && game.Round2)
            {
                await Clients.Group(game.Id).Round2Start();
                await Clients.Group(game.Id).Turn(game.CurrentPlayer);
            }
            else
            {
                await Clients.Group(game.Id).Turn(game.CurrentPlayer);
            }
        }

        public async Task SubmitVote(string voteConnectionId)
        {
            var game = _repository.Games.FirstOrDefault(g => g.HasPlayer(Context.ConnectionId));
            if (game != null)
            {
                var player = game.GetPlayer(Context.ConnectionId);
                if (player != null)
                {
                    player.Vote = voteConnectionId;
                    if (!game.PlayerVotes.Contains(player))
                    {
                        game.PlayerVotes.Add(player);
                    }

                    if (game.PlayerVotes.Count() == game.PlayerOrder.Count())
                    {
                        // End Voting
                        await EndVoting(game);
                    }
                }
            }
        }

        public async Task EndVoting(Game game)
        {                        
            if (game != null)
            {
                var majority = game.PlayerVotes.Count()/2;
                var votes = game.PlayerVotes.GroupBy(x => x.Vote).OrderByDescending(x => x.Count());                
                var mostVoted = votes.FirstOrDefault();
                var numberOfVotes = mostVoted.Count();
                if (numberOfVotes > majority)
                {
                    // Most voted for player
                    await Clients.Group(game.Id).PlayersVotedFor(mostVoted.FirstOrDefault());
                }
                else
                {
                    // Majority not made, faker wins.
                    await Clients.Group(game.Id).VotesNotMet();
                }
            }
        }

        public async Task Join(string connectCode, string name)
        {
            // Check if the game is full and not started
            var game = _repository.Games.FirstOrDefault(g => g.ConnectCode == connectCode && !g.InProgress); 
            if (game != null)
            {
                // Add player is the game is not full
                var player = game.JoinPlayer(Context.ConnectionId, name);
                if (player != null)
                {
                    // Set player to the hub group
                    await Groups.AddToGroupAsync(Context.ConnectionId, game.Id);
                    var joinedPlayer = player;
                    joinedPlayer.Word = "";

                    await Clients.Client(Context.ConnectionId).Joined(joinedPlayer);
                    await this.PlayersWaiting(game);
                }
            }
        }

        public async Task Start()
        {
            var game = _repository.Games.FirstOrDefault(g => g.Host.ConnectionId == Context.ConnectionId);
            if (game != null)
            {
                game.InProgress = true;
                await SetupPlayers(game);
                //await Clients.Group(game.Id.ToString()).Begin(game.Board);
            }
        }

        public async Task Create()
        {
            //Create a new game
            var game = new Game();
            game.Host.ConnectionId = Context.ConnectionId;
            _repository.Games.Add(game);

            await Groups.AddToGroupAsync(Context.ConnectionId, game.Id);
            await Clients.Group(game.Id).ConnectCode(game.ConnectCode);
        }

        public override async Task OnConnectedAsync()
        {
            var game = _repository.Games.FirstOrDefault(g => g.HasPlayer(Context.ConnectionId));
            if (game is null)
            {
                // Connect the host
                game = _repository.Games.FirstOrDefault(g => g.Host.ConnectionId == Context.ConnectionId);
                if (game != null)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, game.Id);
                    await Clients.Group(game.Id).ConnectCode(game.ConnectCode);
                }

                //Ignore click if no game exists
                return;
            }

            // Get player add them back into the game
            var player = game.GetPlayer(Context.ConnectionId);
            if (player != null)
            {
                // Set player to the hub group
                await Groups.AddToGroupAsync(Context.ConnectionId, game.Id);

                if (game.InProgress)
                {
                    await Clients.Group(game.Id).Turn(game.CurrentPlayer);
                }
                else
                {
                    await Clients.Client(Context.ConnectionId).Joined(player);
                    await this.PlayersWaiting(game);
                }
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            //If game is complete delete it
            var game = _repository.Games.FirstOrDefault(g => g.Host.ConnectionId == Context.ConnectionId);
            if (!(game is null))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, game.Id);                
                _repository.Games.Remove(game);
            }

            await base.OnDisconnectedAsync(exception);
        }

        private async Task PlayersWaiting(Game game)
        {
            game.PlayerOrder = new List<Player>();
            if (game.Player1 != null && !string.IsNullOrEmpty(game.Player1.ConnectionId))
            {
                game.PlayerOrder.Add(game.Player1);
            }
            if (game.Player2 != null && !string.IsNullOrEmpty(game.Player2.ConnectionId))
            {
                game.PlayerOrder.Add(game.Player2);
            }
            if (game.Player3 != null && !string.IsNullOrEmpty(game.Player3.ConnectionId))
            {
                game.PlayerOrder.Add(game.Player3);
            }
            if (game.Player4 != null && !string.IsNullOrEmpty(game.Player4.ConnectionId))
            {
                game.PlayerOrder.Add(game.Player4);
            }
            if (game.Player5 != null && !string.IsNullOrEmpty(game.Player5.ConnectionId))
            {
                game.PlayerOrder.Add(game.Player5);
            }
            if (game.Player6 != null && !string.IsNullOrEmpty(game.Player6.ConnectionId))
            {
                game.PlayerOrder.Add(game.Player6);
            }
            if (game.Player7 != null && !string.IsNullOrEmpty(game.Player7.ConnectionId))
            {
                game.PlayerOrder.Add(game.Player7);
            }
            if (game.Player8 != null && !string.IsNullOrEmpty(game.Player8.ConnectionId))
            {
                game.PlayerOrder.Add(game.Player8);
            }

            // Shuffle the list of players
            game.PlayerOrder = game.PlayerOrder.OrderBy(x => System.Guid.NewGuid()).ToList();
            await Clients.Client(game.Host.ConnectionId).Waiting(game.PlayerOrder);
        }

        private async Task SetupPlayers(Game game)
        {
            game.PlayerOrder = new List<Player>();
            if (game.Player1 != null && !string.IsNullOrEmpty(game.Player1.ConnectionId))
            {                
                game.PlayerOrder.Add(game.Player1);
            }
            if (game.Player2 != null && !string.IsNullOrEmpty(game.Player2.ConnectionId))
            {
                game.PlayerOrder.Add(game.Player2);
            }
            if (game.Player3 != null && !string.IsNullOrEmpty(game.Player3.ConnectionId))
            {
                game.PlayerOrder.Add(game.Player3);
            }
            if (game.Player4 != null && !string.IsNullOrEmpty(game.Player4.ConnectionId))
            {
                game.PlayerOrder.Add(game.Player4);
            }
            if (game.Player5 != null && !string.IsNullOrEmpty(game.Player5.ConnectionId))
            {
                game.PlayerOrder.Add(game.Player5);
            }
            if (game.Player6 != null && !string.IsNullOrEmpty(game.Player6.ConnectionId))
            {
                game.PlayerOrder.Add(game.Player6);
            }
            if (game.Player7 != null && !string.IsNullOrEmpty(game.Player7.ConnectionId))
            {
                game.PlayerOrder.Add(game.Player7);
            }
            if (game.Player8 != null && !string.IsNullOrEmpty(game.Player8.ConnectionId))
            {
                game.PlayerOrder.Add(game.Player8);
            }

            // Shuffle the list of players
            game.PlayerOrder = game.PlayerOrder.OrderBy(x => System.Guid.NewGuid()).ToList();
            game.CurrentPlayer = game.PlayerOrder[0];

            var result = _random.Next(game.PlayerOrder.Count());
            var faker = game.PlayerOrder[result];
            
            // Get Random Word
            var realWord = WordList.GetWord();
            await Clients.Client(game.Host.ConnectionId).SendCategory(realWord.Category);

            game.FakerConnectionId = faker.ConnectionId;
            if (game.Player1 != null && game.Player1.ConnectionId == game.FakerConnectionId)
            {
                game.Player1.Word = Game.FakerText;
                await Clients.Client(game.Player1.ConnectionId).ReadyPlayer(game.Player1);
            }
            else if (game.Player1 != null && !string.IsNullOrEmpty(game.Player1.ConnectionId))
            {
                game.Player1.Word = realWord.Word;
                await Clients.Client(game.Player1.ConnectionId).ReadyPlayer(game.Player1);
            }

            if (game.Player2 != null && game.Player2.ConnectionId == game.FakerConnectionId)
            {
                game.Player2.Word = Game.FakerText;
                await Clients.Client(game.Player2.ConnectionId).ReadyPlayer(game.Player2);
            }
            else if (game.Player2 != null && !string.IsNullOrEmpty(game.Player2.ConnectionId))
            {
                game.Player2.Word = realWord.Word;
                await Clients.Client(game.Player2.ConnectionId).ReadyPlayer(game.Player2);
            }

            if (game.Player3 != null && game.Player3.ConnectionId == game.FakerConnectionId)
            {
                game.Player3.Word = Game.FakerText;
                await Clients.Client(game.Player3.ConnectionId).ReadyPlayer(game.Player3);
            }
            else if (game.Player3 != null && !string.IsNullOrEmpty(game.Player3.ConnectionId))
            {
                game.Player3.Word = realWord.Word;
                await Clients.Client(game.Player3.ConnectionId).ReadyPlayer(game.Player3);
            }

            if (game.Player4 != null && game.Player4.ConnectionId == game.FakerConnectionId)
            {
                game.Player4.Word = Game.FakerText;
                await Clients.Client(game.Player4.ConnectionId).ReadyPlayer(game.Player4);
            }
            else if (game.Player4 != null && !string.IsNullOrEmpty(game.Player4.ConnectionId))
            {
                game.Player4.Word = realWord.Word;
                await Clients.Client(game.Player4.ConnectionId).ReadyPlayer(game.Player4);
            }

            if (game.Player5 != null && game.Player5.ConnectionId == game.FakerConnectionId)
            {
                game.Player5.Word = Game.FakerText;
                await Clients.Client(game.Player5.ConnectionId).ReadyPlayer(game.Player5);
            }
            else if (game.Player5 != null && !string.IsNullOrEmpty(game.Player5.ConnectionId))
            {
                game.Player5.Word = realWord.Word;
                await Clients.Client(game.Player5.ConnectionId).ReadyPlayer(game.Player5);
            }

            if (game.Player6 != null && game.Player6.ConnectionId == game.FakerConnectionId)
            {
                game.Player6.Word = Game.FakerText;
                await Clients.Client(game.Player6.ConnectionId).ReadyPlayer(game.Player6);
            }
            else if (game.Player6 != null && !string.IsNullOrEmpty(game.Player6.ConnectionId))
            {
                game.Player6.Word = realWord.Word;
                await Clients.Client(game.Player6.ConnectionId).ReadyPlayer(game.Player6);
            }

            if (game.Player7 != null && game.Player7.ConnectionId == game.FakerConnectionId)
            {
                game.Player7.Word = Game.FakerText;
                await Clients.Client(game.Player7.ConnectionId).ReadyPlayer(game.Player7);
            }
            else if (game.Player7 != null && !string.IsNullOrEmpty(game.Player7.ConnectionId))
            {
                game.Player7.Word = realWord.Word;
                await Clients.Client(game.Player7.ConnectionId).ReadyPlayer(game.Player7);
            }

            if (game.Player8 != null && game.Player8.ConnectionId == game.FakerConnectionId)
            {
                game.Player8.Word = Game.FakerText;
                await Clients.Client(game.Player8.ConnectionId).ReadyPlayer(game.Player8);
            }
            else if (game.Player8 != null && !string.IsNullOrEmpty(game.Player8.ConnectionId))
            {
                game.Player8.Word = realWord.Word;
                await Clients.Client(game.Player8.ConnectionId).ReadyPlayer(game.Player8);
            }            
                        
            await Clients.Group(game.Id).Turn(game.CurrentPlayer);
        }

    }
}
