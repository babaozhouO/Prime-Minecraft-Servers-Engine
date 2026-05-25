/*Copyright 2025 八宝粥(1749861851@qq.com)

   Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.*/
using System.IO;

namespace Prime_Minecraft_Servers_Engine.Modules
{
    internal static class MCServerTypeRecognizerClass
    {
        internal static string RecognizeMCServerType(string ServerDir)
        {
            if (Directory.Exists(ServerDir))
            {
                bool SpigotYml = false;
                bool BukkitYml = false;
                bool LeavesYml = false;
                bool PaperYml = false;
                bool Forge = false;
                bool NeoForge = false;
                bool dotFabric = false;
                bool FabricMC = false;
                string[] ChlidDir = Directory.GetDirectories(ServerDir);
                string[] RootDirFiles = Directory.GetFiles(ServerDir);
                if (ChlidDir.Length == 0 && RootDirFiles.Length == 0)
                {
                    return "Unknow";
                }
                if (RootDirFiles.Contains(Path.Combine(ServerDir, "leaves.yml")))
                {
                    LeavesYml = true;
                }
                if (RootDirFiles.Contains(Path.Combine(ServerDir, "spigot.yml")))
                {
                    SpigotYml = true;
                }
                if (RootDirFiles.Contains(Path.Combine(ServerDir, "bukkit.yml")))
                {
                    BukkitYml = true;
                }
                if (ChlidDir.Contains(Path.Combine(ServerDir, ".fabric")))
                {
                    dotFabric = true;
                }
                string configPath = Path.Combine(ServerDir, "config");
                if (ChlidDir.Contains(configPath))
                {
                    if (RootDirFiles.Contains(Path.Combine(configPath, "paper-global.yml")))
                    {
                        PaperYml = true;
                    }
                    if (RootDirFiles.Contains(Path.Combine(configPath, "paper-world-defaults.yml")))
                    {
                        PaperYml = true;
                    }
                }
                switch (SpigotYml, PaperYml, BukkitYml, LeavesYml, Forge, NeoForge, dotFabric, FabricMC)
                {
                    case (true, true, true, true, false, false, false, false):
                        return "Leaves";
                    case (true, false, true, false, false, false, false, false):
                        return "Spigot";
                    case (false, false, false, false, false, false, false, false):
                        return "Vanilla";
                    case (true, true, true, false, false, false, false, false):
                        return "Paper";
                }
                return "";
            }
            else
            {
                return "Unknow";
            }
        }
    }
}
