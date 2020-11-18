using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;

namespace firstBot.Commands
{
    class myCommands
    {
        [Command("ping")]
        [Description("returns the ping")]
        public async Task Ping(CommandContext ctx)
        {
            await ctx.RespondAsync(ctx.Client.Ping.ToString());
        }

        [Command("hi")]
        [Description("greet user")]
        [Hidden]
        public async Task Hi(CommandContext ctx)
        {
            await ctx.RespondAsync($":wave: Hello {ctx.User.Mention}!! (●'◡'●)");
        }

        [Command("random")]
        [Description("Return a random number between min and max")]
        public async Task Random(CommandContext ctx, 
            [Description("minimum limit")]int min, 
            [Description("maximum limit")] int max)
        {
            var rand = new Random();
            if (min > max)
                await ctx.RespondAsync("incorrect limit");
            else
                await ctx.RespondAsync(rand.Next(min, max).ToString());
        }
    }
}
