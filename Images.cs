using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http;
using ImageSharp;
using ImageSharp.Drawing.Pens;
using ImageSharp.Drawing.Brushes;
using ImageSharp.Processing;
using ImageSharp.Drawing;
using SixLabors.Fonts;
using ImageSharp.PixelFormats;
using System.Numerics;

namespace Unimate.Modules.Public
{
	public class Images : ModuleBase
	{
		string[] memeImage = new string[]
		{
			"http://www.madaboutmemes.com/uploads/memes/471.jpg",//Success Kid - 1
			"http://www.madaboutmemes.com/uploads/memes/467.jpg",//Overly Attached Girlfriend - 2
			"http://www.madaboutmemes.com/uploads/memes/615.png",//Frowning Obama - 3

		};
		string[] memeList = new string[]
		{
			"Success Kid","Overly Attached Girlfriend","Frowning Obama",
		};
		[Command("listMemes")]
		[Alias("memes","memeList")]
		[Remarks("Gives a list of all the current meme images.")]
		public async Task ListMemesAsync()
		{
			var embed = new EmbedBuilder();
			string list = "```";
			int num = 0;
			foreach (string meme in memeList)
			{
				num += 1;
				list += $"{num} - {meme}\n";
			}
			var bot = await Context.Client.GetApplicationInfoAsync();
			embed.ThumbnailUrl = bot.IconUrl;
			embed.AddField(x =>
			{
				x.Name = "Memes list:";
				x.Value = list + "```";
				x.IsInline = false;
			});
			await ReplyAsync("",embed: embed);
		}

		[Command("meme")]
		[Alias("makeMeme")]
		[Remarks("This command will generate a meme with user's text.  Ex: <>meme Line 1| Line 2.  the `  |  ` is `  Shift + \\  `")]
		public async Task MemeAsync(int memeNumber, [Remainder]string text)
		{
			var task = Task.Run(async () =>
			{
				if (memeNumber > memeImage.Length)
				{
					await ReplyAsync($"`ERROR: INCORRECT INTERGER VALUE DETECTED, THE NUMBER MUST BE {memeImage.Length} OR LESS`");
					return;
				}
				await ReplyAsync("");
				string top = null;
				string bottom = null;
				int charsIn = 0;
				var botStart = await ReplyAsync($"*BLEEP BLOOP*\n`GENERATING DANK MEME FOR USER: {Context.User.Username}`");
				foreach (char x in text)
				{
					if (x != '|')
					{
						top += x;
						charsIn += 1;
					}
					else
					{
						for (int i = charsIn + 1; i < text.Length; i++)
						{
							bottom += text.ElementAt(i);
						}
						break;
					}
				}
				top = top.Trim();
				bottom = bottom.Trim();
				ImageSharp.Image image = null;
				HttpClient httpClient = new HttpClient();
				HttpResponseMessage response = await httpClient.GetAsync(memeImage[memeNumber - 1]);
				Stream inputStream = await response.Content.ReadAsStreamAsync();
				image = ImageSharp.Image.Load(inputStream);
				Rgba32 white = new Rgba32(255, 255, 255, 255);
				Rgba32 black = new Rgba32(0, 0, 0, 255);
				FontCollection fonts = new FontCollection();
				Font font1 = fonts.Install("Images/impact.ttf");
				Font font2 = new Font(font1, 50f, FontStyle.Regular);
				TextMeasurer measurer = new TextMeasurer();
				SixLabors.Fonts.Size size = measurer.MeasureText(top, font2, 72);
				float scalingFactor = Math.Min((image.Width - 10) / size.Width, ((image.Height - 10) / 4) / (size.Height));
				Font scaledFont = new Font(font2, scalingFactor * font2.Size);

				SixLabors.Fonts.Size size2 = measurer.MeasureText(bottom, font2, 72);
				float scalingFactor2 = Math.Min((image.Width - 10) / size2.Width, ((image.Height - 10) / 4) / (size2.Height));
				Font scaledFont2 = new Font(font2, scalingFactor2 * font2.Size);

				Vector2 posTop = new Vector2(5, 5);
				Vector2 posBottom = new Vector2(5, (image.Height * (float)(.65)));

				var pen = new Pen(black, scalingFactor);
				var pen2 = new Pen(black, scalingFactor2);
				var brush = new SolidBrush(white);
				var topWidth = measurer.MeasureText(top, scaledFont, 72).Width;
				var botWidth = measurer.MeasureText(bottom, scaledFont2, 72).Width;
				if (topWidth < image.Width - 15) posTop.X += ((image.Width - topWidth) / 2);
				if (botWidth < image.Width - 15) posBottom.X += ((image.Width - botWidth) / 2);
				image.DrawText(top, scaledFont, brush, pen, posTop);
				image.DrawText(bottom, scaledFont2, brush, pen2, posBottom);
				Stream outputStream = new MemoryStream();
				image.SaveAsPng(outputStream);
				outputStream.Position = 0;
				string input = "abcdefghijklmnopqrstuvwxyz0123456789";
				char ch;
				string randomString = "";
				Random rand = new Random();
				for (int i = 0; i < 8; i++)
				{
					ch = input[rand.Next(0, input.Length)];
					randomString += ch;
				}
				var file = File.Create($"Images/{randomString}.png");
				await outputStream.CopyToAsync(file);
				file.Dispose();
				await Context.Channel.SendFileAsync(file.Name);
				var botDone = await ReplyAsync($"`IMAGE HES BEEN GENERATED FOR USER: {Context.User.Username}\nENJOY!!`\n*MURP*\n`BOT MESSAGES WILL DELETE IN 10 SECONDS.`");
				File.Delete(file.Name);
				await Task.Delay(10000);
				await botStart.DeleteAsync();
				await botDone.DeleteAsync();
			});
		}
	}
}
