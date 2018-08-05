## Set up
First, let's set up your API key and make sure you have all packages you need.

You can follow [this guide](https://github.com/reactiflux/discord-irc/wiki/Creating-a-discord-bot-&-getting-a-token) to get your token.

IMPORTANT: NEVER PUBLISH YOUR TOKEN IN ANY WAY. Make sure that when you commit changes the token is never included.

Next, you will need [DSarpPlus](https://github.com/DSharpPlus/DSharpPlus), the .NET wrapper for the Discord API used in this project, as well as [Newtonsoft Json](https://www.newtonsoft.com/json) to manage our Json files.

If you use Nuget, the commands are:
```
PM> Install-Package DSharpPlus -Version 3.2.3
PM> Install-Package Newtonsoft.Json -Version 11.0.2
```

After you cloned the repository, don't forget to setup your `config.json` and make sure that it is included in the build:
```json
{
	"discordToken": "<YOUR TOKEN HERE>",
	"prefix": "<YOUR COMMAND PREFIX HERE, e.g. !>"
}
```

## Styling guide
* Use CamelCase for naming
* Please use tabs with the length of 4 instead of spaces (sorry)

