using BepInEx;
using UnityEngine;

namespace ConsoleCommands
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public void Log(string message)
        {
            Logger.LogInfo(message);
            ChatBox.Instance.SendMessage(message);
        }

        public void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }

        public void Update()
        {
            //if f1 is pressed, Log("F1 is pressed") will be called
            if (Input.GetKeyDown(KeyCode.F1))
            {
                Log("F1 is pressed");
            }
            //if f2 is pressed,
            if (Input.GetKeyDown(KeyCode.F2))
            {
                PlayerStatus playerStatus = PlayerStatus.Instance;
                playerStatus.maxHp = 1000;
                playerStatus.hp = 1000;
                Log("Hp is set to 1000");

            }
            if (Input.GetKeyDown(KeyCode.F3))
            {
                PlayerStatus.Instance.Heal(100);
                Log("Healed 100 hp");
            }
        }
    }
}
