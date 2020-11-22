using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;

namespace firstBot.Games
{
    public enum GameState
    {
        Won,
        Tie,
        Playing
    };

    class Tictactoe : Basegame
    {
        private const uint MaxNoOfPlayers = 2;

        public Tictactoe(CommandContext ctx) : base(MaxNoOfPlayers, ctx)
        {
            ctx.Client.DebugLogger.LogMessage(LogLevel.Info, ctx.Client.CurrentApplication.Name, "Started game of TicTactoe", DateTime.Now);
        }

        /// <summary>
        /// Starts the game
        /// </summary>
        /// <returns></returns>
        public async Task StartGame()
        {
            if (Players.Count != MaxNoOfPlayers)
                return;
            if (!await PlayersConfirm())
            {
                await ctx.RespondAsync("One or more player didn't agree to play, ending the game");
                return;
            }

            var (game, winner) = await PlayGame();
            await Finalize(game, winner);
        }

        /// <summary>
        /// Gets confirmation on players if they are willing to play the game.
        /// </summary>
        /// <returns>true if all players are ready and willing to play</returns>
        private async Task<bool> PlayersConfirm()
        {
            await ctx.RespondAsync($"{string.Join(" ",Players.Select(player => player?.Mention))} must reply with yes");
            var interactivity = ctx.Client.GetInteractivityModule();

            var ConfirmArray = new Task<bool>[MaxPlayers];
            for (int i = 0; i < MaxPlayers; ++i)
            {
                ConfirmArray[i] = GetConfirmAsync(Players[i]);
            }
            await Task.WhenAll(ConfirmArray);

            return ConfirmArray.All(player => player.Result);

            async Task<bool> GetConfirmAsync(DiscordUser usr)
            {
                var msg = await interactivity.WaitForMessageAsync(msg =>
                {
                    if (msg.Channel == ctx.Channel && msg.Author == usr && msg.Content.ToLower().Contains("yes"))
                        return true;
                    return false;
                }, TimeSpan.FromSeconds(35));
                if (msg != null)
                    return true;
                return false;
            }
        }


        /// <summary>
        /// Playes the game
        /// </summary>
        /// <returns>Gamestate: wheather someone won or the game ended in tie
        /// DiscordUser: the user who won or null if tie</returns>
        private async Task<(GameState, DiscordUser)> PlayGame()
        {
            string title = string.Join(" vs ", Players.Select(player => player.Username).Reverse());
            GameState currState;
            DiscordUser winner;
            int currPlayerIndex = 0;
            bool Default = false;
            var board = GetNewBoard();
            var msg = await Render(board, null);

            ShufflePlayers();
            // game loop
            do
            {
                currPlayerIndex = ++currPlayerIndex % (int)MaxPlayers;
                int response = await GetPlayerResponse();
                if (Default)
                {
                    return (GameState.Won, Players[++currPlayerIndex % (int)MaxNoOfPlayers]);
                }
                else
                {
                    UpdateBoard(response);
                }
                msg = await Render(board, msg);

            } while (!CheckWinnerOrTie(out currState, out winner));

            return (currState, winner);


            byte[,] GetNewBoard()
            {
                return new byte[,]
                {
                { 1, 2, 3 },
                { 4, 5, 6 },
                { 7, 8, 9 }
                };
            }

            async Task<DiscordMessage> Render(byte[,] board, DiscordMessage prevMessage)
            {
                var desc = new StringBuilder();
                string VerticalLine = "│";
                string HorizontalLine = "━";

                for (int i = 0; i < 3; ++i)
                {
                    desc.AppendFormat("\t{1}{0}{2}{0}{3}",
                        VerticalLine,
                        GetItem(board[i, 0]),
                        GetItem(board[i, 1]),
                        GetItem(board[i, 2]));
                    desc.AppendLine();
                    if (i == 2)
                        continue;
                    desc.AppendFormat("\t{0}{0}{0}{0}{0}",
                        HorizontalLine);
                    desc.AppendLine();
                }

                var embed = new DiscordEmbedBuilder()
                {
                    Title = title,
                    Description = desc.ToString(),
                    Color = DiscordColor.Blurple,
                };

                var _ = prevMessage?.DeleteAsync();
                return await ctx.RespondAsync(embed: embed);
            }

            // shuffle players using Fisher-Yates shuffle
            void ShufflePlayers()
            {
                var rand = new Random();
                int upperLimit = (int)MaxNoOfPlayers;
                while (upperLimit != 0)
                {
                    int pos = rand.Next(0, upperLimit);
                    --upperLimit;
                    if (pos == upperLimit)
                        continue;
                    (Players[pos], Players[upperLimit]) = (Players[upperLimit], Players[pos]);
                }
            }

            string GetItem(byte value)
            {
                return value switch
                {
                    1 => ":one:",
                    2 => ":two:",
                    3 => ":three:",
                    4 => ":four:",
                    5 => ":five:",
                    6 => ":six:",
                    7 => ":seven:",
                    8 => ":eight:",
                    9 => ":nine:",
                    (byte)Symbol.Cross => ":x:",
                    (byte)Symbol.Circle => ":o:",
                    _ => ":no_entry_sign: "
                };
            }

            (int row, int column) GetPosition(int pos)
            {
                --pos;
                return (pos / 3, pos % 3);
            }

            async Task<int> GetPlayerResponse()
            {
                var from = Players[currPlayerIndex];
                int response = 0, loopCount = 0, MaxTrial = 10;
                var interactivity = ctx.Client.GetInteractivityModule();
                await ctx.RespondAsync($"{from.Mention} Your turn: ");
                do
                {
                    ++loopCount;
                    if (loopCount != 1)
                    {
                        await ctx.RespondAsync($"{from.Mention} Invalid Position!! ({loopCount}/{MaxTrial})");
                    }

                    var msg = await interactivity.WaitForMessageAsync(msg =>
                    {
                        if (msg.Channel == ctx.Channel && msg.Author == from)
                        {
                            var input = msg.Content.Split(" ")[0];
                            return int.TryParse(input, out response);
                        }
                        return false;
                    }, TimeSpan.FromSeconds(45));

                    if (msg == null)
                    {
                        await ctx.RespondAsync($"{from.Mention} did not respond in time.");
                        Default = true;
                        break;
                    }
                } while (loopCount <= MaxTrial && !ValidPosition(response));

                if (loopCount > MaxTrial)
                {
                    await ctx.RespondAsync($"{from.Mention} was unable to give a valid position within 10 tries.");
                    Default = true;
                }

                return response;

                bool ValidPosition(int pos)
                {
                    if (pos < 1 || pos > 9)
                        return false;
                    var (row, col) = GetPosition(pos);
                    return !(board[row, col] == (byte)Symbol.Cross || board[row, col] == (byte)Symbol.Circle);
                }
            }

            void UpdateBoard(int pos)
            {
                var (row, col) = GetPosition(pos);

                board[row, col] = (currPlayerIndex % 2) switch
                {
                    0 => (int)Symbol.Circle,
                    1 => (int)Symbol.Cross,
                    _ => 0
                };
            }

            bool CheckWinnerOrTie(out GameState curr, out DiscordUser winner)
            {
                var WinCheck = new (int, int)[8, 3]
                {
                    { ( 0, 0) , ( 0, 1) , ( 0, 2) },
                    { ( 1, 0) , ( 1, 1) , ( 1, 2) },
                    { ( 2, 0) , ( 2, 1) , ( 2, 2) },
                    { ( 0, 0) , ( 1, 0) , ( 2, 0) },
                    { ( 0, 1) , ( 1, 1) , ( 2, 1) },
                    { ( 0, 2) , ( 1, 2) , ( 2, 2) },
                    { ( 0, 0) , ( 1, 1) , ( 2, 2) },
                    { ( 0, 2) , ( 1, 1) , ( 2, 0) }
                };

                // test for winner
                for (int i = 0; i < 8; ++i)
                {
                    var curr1 = WinCheck[i, 0];
                    var curr2 = WinCheck[i, 1];
                    var curr3 = WinCheck[i, 2];
                    if (board[curr1.Item1, curr1.Item2] == board[curr2.Item1, curr2.Item2] &&
                        board[curr2.Item1, curr2.Item2] == board[curr3.Item1, curr3.Item2])
                    {
                        curr = GameState.Won;
                        winner = Players[currPlayerIndex];
                        return true;
                    }
                }
                winner = null;

                // test for tie
                foreach(var elem in board)
                {
                    if (elem < 10)
                    {
                        curr = GameState.Playing;
                        return false;
                    }
                }

                curr = GameState.Tie;
                return true;
            }

        }

        private async Task Finalize(GameState state, DiscordUser winner)
        {
            string title = string.Join(" ", Players.Select(player => player.Username));
            string Fieldtitle = "Game Over!";
            string Fielddesc = "";
            if (state == GameState.Won)
            {
                Fielddesc = $":confetti_ball: {winner.Mention} has won!! :confetti_ball:";
            }
            else if (state == GameState.Tie)
            {
                Fielddesc = $"It's a Draw.";
            }

            var embd = new DiscordEmbedBuilder()
            {
                Title = title,
                Color = state switch
                {
                    GameState.Won => DiscordColor.Gold,
                    GameState.Tie => DiscordColor.Grayple,
                    _ => DiscordColor.Red
                }
            };

            embd.AddField(Fieldtitle, Fielddesc);
            await ctx.RespondAsync(embed: embd);
        }

        enum Symbol
        {
            Cross = 11,
            Circle = 12
        }

    }
}
