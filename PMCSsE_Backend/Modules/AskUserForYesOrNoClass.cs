

namespace PMCSsE_Backend.Modules
{
    internal static class AskUserForYesOrNoClass
    {
        internal static bool Ask(string Question)
        {
            while (true)
            {
                StaticTools.HandleLog($"{Question}（Y/N）");
                string? choice = Console.ReadLine();
                if (!string.IsNullOrEmpty(choice))
                {
                    if (choice == "Y")
                    {
                        return true;
                    }
                    else if (choice == "N")
                    {
                        return false;
                    }
                }
            }
        }
    }
}
