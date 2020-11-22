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
        Tie
    };

    class Tictactoe : Basegame
    {
        private const uint MaxNoOfPlayers = 2;
        private DiscordUser StartingPlayer;
        public Tictactoe(CommandContext ctx) : base(MaxNoOfPlayers, ctx)
        {
            ctx.Client.DebugLogger.LogMessage(LogLevel.Info, ctx.Client.CurrentApplication.Name, "Started game of TicTactoe", DateTime.Now);
        }

        /// <summary>
        /// Initial Setup for game
        /// </summary>
        /// <returns></returns>
        private async Task GameSetup()
        {
            var msg = await ctx.RespondAsync("Setting up game...");
            await AddPlayers(msg);
            StartingPlayer = Players[new Random().Next(0, (int)MaxNoOfPlayers)];
        }

        /// <summary>
        /// Starts the game
        /// </summary>
        /// <returns></returns>
        public async Task StartGame()
        {
            await GameSetup();
            if (!await PlayersConfirm())
            {
                await ctx.RespondAsync("One or more player didn't agree to play, ending the game");
                return;
            }

            await PlayGame();
        }

        /// <summary>
        /// Gets confirmation on players if they are willing to play the game.
        /// </summary>
        /// <returns>true if all players are ready and willing to play</returns>
        private async Task<bool> PlayersConfirm()
        {
            await ctx.RespondAsync($"{Players.Select(player => player.Mention)} must reply with yes");
            var interactivity = ctx.Client.GetInteractivityModule();

            var ConfirmArray = new Task<bool>[MaxPlayers];
            for (int i = 0; i < MaxPlayers; ++i)
            {
                ConfirmArray[i] = GetConfirmAsync(Players[i]);
            }
            foreach (var player in ConfirmArray)
            {
                await player;
            }

            return ConfirmArray.All(player => player.Result);

            async Task<bool> GetConfirmAsync(DiscordUser usr)
            {
                var msg = await interactivity.WaitForMessageAsync(msg =>
                {
                    if (msg.Channel == ctx.Channel && msg.Author == ctx.User && msg.Content.ToLower().Contains("yes"))
                        return true;
                    return false;
                });
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
            string title = string.Join(" vs ", Players.Select(player => player.Username));
            GameState currState;
            DiscordUser winner;

            var board = GetNewBoard();
            var msg = await Render(board, title, null);

            // game loop
            do
            {
                var currPlayer = GetPlayer();
                var response = await GetPlayerResponse(currPlayer);
                UpdateBoard(response, board);
                await Render(board, title, msg);

            } while (CheckWinnerOrTie(board,out currState, out winner));

            return (GameState.Tie, null);
        }

        private byte[,] GetNewBoard()
        {
            return new byte[,]
            {
                { 1, 2, 3 },
                { 4, 5, 6 },
                { 7, 8, 9 }
            };
        }
        
        private async Task<DiscordMessage> Render(byte[,] board, string title, DiscordMessage prevMessage)
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

        private string GetItem(byte value)
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
                11 => ":x:",
                12 => ":o:",
                _ => ":no_entry_sign: "
            };
        }

        private DiscordUser GetPlayer()
        {
            
        }

        private Task<string> GetPlayerResponse(DiscordUser from)
        {

        }

        private void UpdateBoard(string pos, byte[,] board)
        {

        }

        private bool CheckWinnerOrTie(byte[,] board, out GameState curr, out DiscordUser winner)
        {

        }
    }
}
