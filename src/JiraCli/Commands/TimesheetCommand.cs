using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using JiraCli.Api;
using JiraCli.ViewModels;

namespace JiraCli.Commands
{
    public class TimesheetCommand
    {
        private static Period s_Period;
        private static string[] s_Users;

        public static int Run(string[] args)
        {
            var settings = Program.Configuration.JiraSettings;
            ParseArgs(args);

            //set defaults
            if (s_Period == null)
            {
                s_Period = Period.FromDays(15);
            }
            if (s_Users == null)
            {
                //getting timesheet for current user by default
                s_Users = new[] {settings.Login};
            }

            try
            {
                Console.WriteLine("Downloading worklogs for period of {0}.", s_Period);

                var jiraClient = new RestApiServiceClient(new HttpClientHandler(), settings.BaseUrl,
                    settings.Login,
                    settings.Password);
                var views = DownloadUsersWorklogs(jiraClient, s_Users, s_Period).Result;

                foreach (var view in views)
                {
                    Console.WriteLine("**** Logs for user \"{0}\".", view.Author.Name);
                    view.Print(new PlainTextOutput(Console.Out));
                    Console.WriteLine("******************************************************************");
                    Console.WriteLine();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return 1;
            }

            return 0;
        }

        private static void ParseArgs(string[] args)
        {
            for (int argIndex = 0; argIndex < args.Length; argIndex++)
            {
                if (Program.IsArg(args[argIndex], "h", "help"))
                {
                    PrintHelp();
                    throw new GracefulException();
                }
                else if (Program.IsArg(args[argIndex], "u", "users"))
                {
                    string users = Program.GetOptionArgValue(args, ++argIndex);
                    s_Users = users.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries);
                }
                else if (Program.IsArg(args[argIndex], "p", "period"))
                {
                    string periodText = Program.GetOptionArgValue(args, ++argIndex);
                    s_Period = periodText.All(char.IsDigit)
                        ? Period.FromDays(int.Parse(periodText))
                        : Period.FromString(periodText);
                }
                else
                {
                    Console.WriteLine($"Unknown option: {args[argIndex]}");
                    throw new ArgumentException();
                }
            }
        }

        private static async Task<IEnumerable<ProjectViewModel>> DownloadUsersWorklogs(RestApiServiceClient client,
            IEnumerable<string> users, Period period)
        {
            //NOTE: downloading timesheets for users not in single search request but simultaneously to increase speed 
            var downloadTasks = users.Select(user => client.DownloadTimesheetAsync(period, user)).ToList();
            return await Task.WhenAll(downloadTasks);
        }

        private static void PrintHelp()
        {
            const string usageText = @"Usage: jira timesheet [options]

Options:
  -h|--help                  Show help 
  -u|--users <USERS>         Comma separated usernames (user1, user2, user3)
  -p|--period <TIME_PERIOD>  Timesheet period for last days or specific time interval";

            Console.WriteLine(usageText);
        }
    }
}
