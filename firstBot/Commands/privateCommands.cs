using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace firstBot.Commands
{
    [Group("admin")]
    [Description("Administrative Commands")]
    //[Hidden]
    //[RequirePermissions(Permissions.ManageGuild)]
    class privateCommands 
    {
        [Command("sudo"), Description("Executes a command as another user."), RequireOwner]
        public async Task Sudo(CommandContext ctx,
            [Description("Member to execute as.")] DiscordMember discordMember,
            [RemainingText, Description("command text to execute")] string command)
        {
            await ctx.TriggerTypingAsync();

            var cmds = ctx.CommandsNext;
            await cmds.SudoAsync(discordMember, ctx.Channel, command);
        }

    }
}
