using System;
using System.Linq;
using JiraCli.Configuration;

namespace JiraCli.Commands.Timesheet
{
    public partial class TimesheetCommand
    {
        public static int Run(string[] args)
        {
            var settings = Program.Configuration.JiraSettings;
            var command = new TimesheetCommand();
            command.ParseArgs(settings, args);

            try
            {
                command.Run(settings, Console.Out);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return 1;
            }

            return 0;
        }

        private void ParseArgs(JiraConfiguration settings, string[] args)
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
                    Users = users.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries);
                }
                else if (Program.IsArg(args[argIndex], "p", "period"))
                {
                    string periodText = Program.GetOptionArgValue(args, ++argIndex);
                    Period = periodText.All(char.IsDigit)
                        ? Period.FromDays(int.Parse(periodText))
                        : Period.FromString(periodText);
                }
                else if (Program.IsArg(args[argIndex], "f", "file"))
                {
                    OutputFileName = Program.GetOptionArgValue(args, ++argIndex);
                }
                else
                {
                    Console.WriteLine($"Unknown option: {args[argIndex]}");
                    throw new ArgumentException();
                }
            }

            //set defaults if none passed as arguments
            if (Period == null)
            {
                Period = Period.FromDays(15);
            }
            if (Users == null)
            {
                //getting timesheet for current user by default
                Users = new[] {settings.Login};
            }
        }

        private static void PrintHelp()
        {
            const string usageText = @"Usage: jira timesheet [options]

Options:
  -h|--help                  Show help 
  -u|--users <USERS>         Comma separated usernames (user1, user2, user3)
  -p|--period <TIME_PERIOD>  Timesheet period for last days or specific time interval
  -f|--file <FILE_NAME>      Output file name";

            Console.WriteLine(usageText);
        }
    }
}
