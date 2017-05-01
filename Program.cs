//Thanks for the help from CWS (Coding with Storm) Discord
using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;

namespace Unimate
{
	public class Program
	{
		// Convert our sync main to an async main.
		public static void Main(string[] args) =>
			new Program().Start().GetAwaiter().GetResult();



		private DiscordSocketClient client;
		private CommandHandler handler;

		public async Task Start()
		{
			// Define the DiscordSocketClient
			client = new DiscordSocketClient();

			var token = "Token here";

			// Login and connect to Discord.
			await client.LoginAsync(TokenType.Bot, token);
			await client.StartAsync();
			client.Ready += async () =>
			{
				await client.SetGameAsync($"-help | In guilds: {client.Guilds.Count}");
			};
			client.JoinedGuild += async (e) =>
			{
				await client.SetGameAsync($"-help | In guilds: {client.Guilds.Count}");
			};
			client.LeftGuild += async (e) =>
			{
				await client.SetGameAsync($"-help | In guilds: {client.Guilds.Count}");
			};


			var map = new DependencyMap();
			map.Add(client);

			handler = new CommandHandler();
			await handler.Install(map);
			Console.WriteLine($"{DateTime.UtcNow}: YourBot initiated...");
			await Task.Delay(-1);
		}

		private static Task DelayDelete()
		{
			return Task.Delay(-1);
		}

		private Task Log(LogMessage msg)
		{
			Console.WriteLine(msg.ToString());
			return Task.CompletedTask;
		}
	}
}
