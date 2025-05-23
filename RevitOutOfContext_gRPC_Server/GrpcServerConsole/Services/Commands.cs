namespace GrpcServerConsole.Services
{
    public static class Commands
    {
        public static void AddCommand(string commandName)
        {
            commands.Clear();
            commands.Enqueue(commandName);
        }
        public static string GetCommand()
        {
            var command = "";
            if (commands.Count > 0)
            {
                command = commands.Dequeue();
            }
            return command;
        }
        public static void AddCommandResult(string commandName, string commandResult)
        {
            CommandsResult.Add(commandName, commandResult);
        }
        public static string GetCommandResult(string commandName)
        {
            var res = "";
            if (CommandsResult[commandName] != null)
            {
                res = CommandsResult[commandName];
            }
            return res;
        }
        public static Queue<string> commands = new Queue<string>();
        public static Dictionary<string, string> CommandsResult = new Dictionary<string, string>();
    }
}
