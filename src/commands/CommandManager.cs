
using System.Text.Json;
using lib.debug;

public static class CommandManager
{
    private static List<CommandEntry> commandEntries = new List<CommandEntry>();


    public static void RegisterCommand(string commandName, string commandId, Delegate callback)
    {
        commandEntries.Add(new CommandEntry(commandName, commandId, callback));
    }

    public static CommandEntry GetCommandEntry(string commandId)
    {
        return commandEntries.FirstOrDefault(x => x.CommandId == commandId);
    }

    public static object ExecuteCommand(string commandId, params object[] args)
    {
        CommandEntry entry = null;
        /*
                if (!(args == null || args.Length == 0))
                {
                    if ((args != null || args.Length == 0) && args[0] is JsonElement)
                    {
                        FromJsonElement(ref args);
                    }
                }
        */
        foreach (CommandEntry k in commandEntries)
        {
            if (k.CommandId == commandId)
            {
                if (k.Types.Length == args.Length)
                {
                    if (k.Types.Zip(args, (expected, actual) => expected.IsAssignableFrom(actual.GetType())).All(match => match))
                    {
                        entry = k;
                        break;
                    }
                }
            }
        }

        if (entry == null)
        {
            return null;
        }

        if (entry.IsClientCommand)
        {
            DebugWriter.WriteLine("Commands", $"Running client command '{entry.CommandId}' for client '{entry.ClientId}'");
            /*
                        CommandPackage commandPackage = new CommandPackage()
                        {
                            CommandId = entry.CommandId,
                            Arguments = args
                        };
                        Extensions.WritePackageToClient(entry.ClientId, PackageTypes.Command, commandPackage);

                        return null; // Or some appropriate response
            */
        }

        return entry.Callback.DynamicInvoke(args);
    }
}