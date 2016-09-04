using System;
using System.Collections.Generic;
using System.Linq;
using JiraCli.Commands;
using JiraCli.Configuration;
using Microsoft.Extensions.Configuration;

namespace JiraCli
{
    class Program
    {
        private static readonly Dictionary<string, Func<string[], int>> s_Commands =
            new Dictionary<string, Func<string[], int>>
            {
                ["timesheet"] = TimesheetCommand.Run,
            };

        public static AppConfiguration Configuration { get; set; }

        private static int Main(string[] args)
        {
            try
            {
                InitConfiguration();

                for (int argIndex = 0; argIndex < args.Length; argIndex++)
                {
                    if (IsArg(args[argIndex], "h", "help"))
                    {
                        ShowHelp();
                        return 0;
                    }

                    //override settings
                    if (IsArg(args[argIndex], "user"))
                    {
                        Configuration.JiraSettings.Login = GetOptionArgValue(args, ++argIndex);
                    }
                    else if (IsArg(args[argIndex], "pass"))
                    {
                        Configuration.JiraSettings.Password = GetOptionArgValue(args, ++argIndex);
                    }
                    else if (IsArg(args[argIndex], "url"))
                    {
                        Configuration.JiraSettings.BaseUrl = GetOptionArgValue(args, ++argIndex);
                    }

                    else if (args[argIndex].StartsWith("-"))
                    {
                        Console.WriteLine($"Unknown option: {args[argIndex]}");
                        return 1;
                    }
                    else
                    {
                        string runCommand = args[argIndex];
                        var commandArgs = (argIndex + 1) >= args.Length
                            ? Enumerable.Empty<string>().ToArray()
                            : args.Skip(argIndex + 1).ToArray();

                        Func<string[], int> commandAction;
                        if (s_Commands.TryGetValue(runCommand, out commandAction))
                        {
                            try
                            {
                                int exitCode = commandAction(commandArgs);
                                return exitCode;
                            }
                            catch (GracefulException)
                            {
                                return 0;
                            }
                        }

                        Console.WriteLine($"Unknown command \"{runCommand}\"");
                        return 1;
                    }
                }

                return Program.Main(new[] {"--help"});
            }
            catch (ArgumentException argEx)
            {
                Console.WriteLine(argEx.Message);
                return 1;
            }
        }

        private static void InitConfiguration()
        {
            ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddPersistenJsonFile("appsettings.json", true);

            var configRoot = configurationBuilder.Build();
            Configuration = new AppConfiguration();
            configRoot.Bind(Configuration);

            if (Configuration.IsFirstRun)
            {
                var jiraConfigSection = configRoot.GetSection(nameof(Configuration.JiraSettings));

                Console.WriteLine("Jira CommandLine interface setup.");
                Console.Write("Url: ");
                jiraConfigSection[nameof(JiraConfiguration.BaseUrl)] = Console.ReadLine();

                Console.Write("Username: ");
                jiraConfigSection[nameof(JiraConfiguration.Login)] = Console.ReadLine();

                Console.Write("Password: ");
                jiraConfigSection[nameof(JiraConfiguration.Password)] = Console.ReadLine();

                //rebing configuration
                configRoot.Bind(Configuration);
            }
        }

        private static void ShowHelp()
        {
            const string usageText = @"Usage: jira [host-options] [command] [arguments] [common-options]

Arguments:
  [command]           The command to execute
  [arguments]         Arguments to pass to the command
  [host-options]      Options specific to jira (host)
  [common-options]    Options common to all commands

Common options:
  -h|--help           Show help 

Host options (passed before the command):
  --url               Site url
  --user              Site username
  --pass              Site password

Commands:
  timesheet           Download timesheet report";

            Console.WriteLine(usageText);
        }

        public static bool IsArg(string value, string longName)
        {
            return IsArg(value, null, longName);
        }

        public static bool IsArg(string value, string shortName, string longName)
        {
            return (shortName != null && value.Equals("-" + shortName)) ||
                   (longName != null && value.Equals("--" + longName));
        }

        public static string GetOptionArgValue(string[] args, int index)
        {
            string errorMessage = $"Option value at {index} is mandatory.";
            if (index + 1 > args.Length)
                throw new ArgumentException(errorMessage);

            string argValue = args[index];

            if (argValue.StartsWith("-"))
                throw new ArgumentException(errorMessage);

            return argValue;
        }
    }
}
