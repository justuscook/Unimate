//Thanks LunarLite ;)
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord.Rest;
using System.Linq;
using System.Threading.Tasks;

namespace Unimate.Modules.Public
{
	public class HelpModule : ModuleBase
	{
		private CommandService _service;

		public HelpModule(CommandService service)           // Create a constructor for the commandservice dependency
		{
			_service = service;
		}

		[Command("help")]
		[Remarks("General help command, returns this list.")]
		public async Task HelpAsync()
		{

			string prefix = "<>";
			var user = Context.User as SocketGuildUser;
			var DM = await user.CreateDMChannelAsync();
			var builder = new EmbedBuilder()
			{
				Color = new Color(114, 137, 218),
				Description = "These are the commands you can use:"
			};

			foreach (var module in _service.Modules)
			{
				string description = null;
				foreach (var cmd in module.Commands)
				{
					var result = await cmd.CheckPreconditionsAsync(Context);
					if (result.IsSuccess)
						description += $"{prefix}{cmd.Aliases.First()}: " + cmd.Remarks + "\n";
				}

				if (!string.IsNullOrWhiteSpace(description))
				{
					builder.AddField(x =>
					{
						x.Name = module.Name;
						x.Value = description;
						x.IsInline = false;
					});
				}
			}

			await DM.SendMessageAsync("", false, builder.Build());
		}

		[Command("help")]
		[Remarks("Type -help and a command to see more about it, example: -help hi")]
		public async Task HelpAsync(string command)
		{
			var result = _service.Search(Context, command);
			var user = Context.User as SocketGuildUser;
			var DM = await user.CreateDMChannelAsync();
			if (!result.IsSuccess)
			{
				await DM.SendMessageAsync($"Sorry, I couldn't find a command like **{command}**.");
				return;
			}

			var builder = new EmbedBuilder()
			{
				Color = new Color(114, 137, 218),
				Description = $"Here are some commands like **{command}**"
			};

			foreach (var match in result.Commands)
			{
				var cmd = match.Command;

				builder.AddField(x =>
				{
					x.Name = string.Join(", ", cmd.Aliases);
					x.Value = $"Parameters: {string.Join(", ", cmd.Parameters.Select(p => p.Name))}\n" +
							  $"Remarks: {cmd.Remarks}";
					x.IsInline = false;
				});
			}


			await DM.SendMessageAsync("", false, builder.Build());
		}
	}
}
