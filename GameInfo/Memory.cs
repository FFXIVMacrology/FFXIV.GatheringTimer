using Sharlayan.Enums;
using Sharlayan.Models;
using Sharlayan;
using System.Diagnostics;
using Sharlayan.Core;
using Sharlayan.Models.ReadResults;

namespace GatheringTimer.GameInfo
{
    public static class Memory
    {

        private static MemoryHandler memoryHandler;

        public static bool HookGameProcess()
        {
            Logger.Info("Try to hook process(ffxiv_dx11.exe)");

            // DX11
            Process[] processes = Process.GetProcessesByName("ffxiv_dx11");
            if (processes.Length > 0)
            {
                // supported: Global, Chinese, Korean
                GameRegion gameRegion = GameRegion.China;
                GameLanguage gameLanguage = GameLanguage.Chinese;
                // whether to always hit API on start to get the latest sigs based on patchVersion, or use the local json cache (if the file doesn't exist, API will be hit)
                bool useLocalCache = true;
                // patchVersion of game, or latest
                string patchVersion = "latest";
                Process process = processes[0];
                ProcessModel processModel = new ProcessModel
                {
                    Process = process
                };
                SharlayanConfiguration configuration = new SharlayanConfiguration
                {
                    ProcessModel = processModel,
                    GameLanguage = gameLanguage,
                    GameRegion = gameRegion,
                    PatchVersion = patchVersion,
                    UseLocalCache = useLocalCache
                };
                memoryHandler = SharlayanMemoryManager.Instance.AddHandler(configuration);
                Logger.Info("Hook process(ffxiv_dx11.exe) success");
                return true;
            }

            Logger.Info("Could not found process(ffxiv_dx11.exe),make sure you are on DirectX11");
            return false;

        }

        public static ActorItem GetPlayer()
        {
            return memoryHandler.Reader.GetCurrentPlayer().Entity;
        }

    }
}
