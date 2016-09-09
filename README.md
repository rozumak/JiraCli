###Jira command-line interface
[![Build status](https://ci.appveyor.com/api/projects/status/nfv0esrcjjbvdwd5?svg=true)](https://ci.appveyor.com/project/heXelium/jiracli)

####Description
The purpose of creating this tool is to have an easy way of making common tasks in Jira and getting different types of reports (which are mostly available via paid plugins and requires deep integration into the site) via command-line interface. The only thing that it currently supports it's getting detailed timesheet report for a period of time.

####Configuration
During the first run, JiraCli asks for username, password and Jira url. All values are stored in _appsettings.json_.

####Usage
```
jira [host-options] [command] [arguments] [common-options]

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
  timesheet           Download timesheet report`
```

####Examples

Get timesheet command help:

`jira timesheet -h`

Download worklogs and generate timesheet report for current user for last 15 days:

`jira timesheet`

Download worklogs and generate timesheet report for specific period of time and specific users:

`jira timesheet -p 2016/01/01-2016/01/30 -u user1,user2,user3`

Download worklogs and generate timesheet report for current user for last 10 days and save it to the file:

`jira timesheet -p 10 -f report.txt`

Override configured host options and execute timesheet command:

`jira --user some.user --pass securePass --url https://jira.cool.net timesheet`
