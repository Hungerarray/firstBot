using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;

namespace firstBot.Games
{
    
    class Tictactoe : Basegame
    {
        private const uint MaxNoOfPlayers = 2;
        public Tictactoe(CommandContext ctx) : base(MaxNoOfPlayers)
        {
            ctx.Client.DebugLogger.LogMessage(LogLevel.Info, ctx.Client.CurrentApplication.Name, "Started game of TicTactoe", DateTime.Now);           
        }

        private async Task GameSetup(CommandContext ctx)
        {
            var msg = await ctx.RespondAsync("Setting up game...");
            await AddPlayers(ctx, msg);
            var ready = await PlayersConfirm(ctx);
            await PlayGames(ctx);
        }

        public async Task StartGame(CommandContext ctx)
        {
            await GameSetup(ctx);
            
        }
        protected override Task<bool> ValidInput()
        {
            throw new NotImplementedException();
        }

        private async Task<bool> PlayersConfirm(CommandContext ctx)
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
    }
}
