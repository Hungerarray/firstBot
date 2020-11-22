using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using firstBot.Games;

namespace firstBot.Commands
{
    [Group("Games")]
    class GamesCommands
    {
        [Command("TicTacToe")]
        [Aliases("ttt")]
        public async Task TicTacToe(CommandContext ctx)
        {
            var game = new Tictactoe(ctx);
            await game.StartGame();
        }
    }
}
