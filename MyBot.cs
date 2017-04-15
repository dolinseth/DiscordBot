using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace ConsoleApp1
{
	class MyBot
	{
		DiscordClient discord;

		public MyBot()
		{
			discord = new DiscordClient(x =>
			{
				x.LogLevel = LogSeverity.Info;
				x.LogHandler = Log;
			});

			discord.ExecuteAndWait(async () =>
			{
				await discord.Connect("Mjk0NTUzOTg0NTYyMjMzMzQ2.C7W-tg.UKM-0Quc8RPjOnWMq95g7Ma28QQ");
			});
		}

		private void Log(object sender, LogMessageEventArgs e)
		{
			Console.WriteLine(e.Message);
		}
	}
}
