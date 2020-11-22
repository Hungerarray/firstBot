using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using firstBot.Games;
namespace firstBot.Commands
{
    
    [Group("Games")]
    [Description("List of playable games, try [help game <game_name>] to get the aliases for the game")]
    class GamesCommands
    {
        private static List<DiscordUser> CurrentlyPlaying = new List<DiscordUser>();
        [Command("TicTacToe")]
        [Description("Starts a game of TicTacToe")]
        [Aliases("ttt")]
        public async Task TicTacToe(CommandContext ctx)
        {
            var game = new Tictactoe(ctx);
            var GamePlayersList = await game.AddPlayers(CurrentlyPlaying);
            if(GamePlayersList.Count != 0)
            {
                await game.StartGame();
                CurrentlyPlaying.RemoveAll(usr =>
                {
                    return GamePlayersList.Contains(usr);
                });
            }
        }
    }
}
