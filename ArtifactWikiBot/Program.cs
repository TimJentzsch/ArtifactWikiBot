namespace ArtifactWikiBot
{
    class Program
    {
        static void Main(string[] args)
        {
            // Run the bot
            Bot.INSTANCE.RunAsync().GetAwaiter().GetResult();
        }
    }
}
