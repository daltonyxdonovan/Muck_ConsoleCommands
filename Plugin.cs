using BepInEx;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Text.RegularExpressions;

namespace ConsoleCommands
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public void Log(string message)
        {
            Logger.LogInfo(message);
            ChatBox.Instance.SendMessage("<color=red>" + message + "</color>");
        }

        public string TrimMessage(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return "";
            }
            return message.Substring(0, Mathf.Min(message.Length, 99999));
        }

        public void ClearMessage()
        {
            ChatBox.Instance.inputField.text = "";
		    ChatBox.Instance.inputField.interactable = false;
        }

        public void CustomSendMessage(string message)
        {
            ChatBox.Instance.typing = false;
            message = TrimMessage(message);
            if (message == "")
            {
                return;
            }
            if (message[0] == '/')
            {
                ChatCommand(message);
                return;
            }
            
            ChatBox.Instance.AppendMessage(0, message, GameManager.players[LocalClient.instance.myId].username);
            ClientSend.SendChatMessage(message);
            ClearMessage();
        }

        public void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }

        public void ChatCommand(string message)
        {
            if (message.Length <= 0)
            {
                return;
            }
            string a = message.Split(new char[]
            {
                ' '
            })[0].Substring(1);
            ChatBox.Instance.inputField.text = "";
		    ChatBox.Instance.inputField.interactable = false;
            string text = "#FFFFFF";
            if (a == "seed")
            {
                int seed = GameManager.gameSettings.Seed;
                ChatBox.Instance.AppendMessage(-1, string.Concat(new object[]
                {
                    "<color=",
                    text,
                    ">Seed: ",
                    seed,
                    " (copied to clipboard)<color=white>"
                }), "");
                GUIUtility.systemCopyBuffer = string.Concat(seed);
                return;
            }
            if (a == "ping")
            {
                ChatBox.Instance.AppendMessage(-1, "<color=" + text + ">pong<color=white>", "");
                return;
            }
            if (a == "debug")
            {
                DebugNet.Instance.ToggleConsole();
                return;
            }
            if (a == "kill")
            {
                PlayerStatus.Instance.Damage(0, 0, true);
                return;
            }

            /*
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                            CUSTOM COMMANDS HERE    vvv
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            */

            if (a.StartsWith("heal"))
            {

                int amount = 100;
                if (a.Length > 4)
                {
                    if (!int.TryParse(a.Substring(5), out amount))
                    {
                        Log("used an invalid heal amount");
                        return;
                    }
                }
                else
                {
                    Log("healed with a default amount of 100");
                }

                PlayerStatus.Instance.Heal(amount);
                Log($"healed {amount} hp");
                return;
            }

            if (a.StartsWith("maxhp"))
            {
                PlayerStatus playerStatus = PlayerStatus.Instance;
                int amount = 1000;

                if (a.Length > 5)
                {
                    if (!int.TryParse(a.Substring(6), out amount))
                    {
                        Log("used an invalid maxHP amount");
                        return;
                    }
                }
                

                playerStatus.maxHp = amount;
                playerStatus.hp = amount;
                Log($"set their maxHP to {amount}");
            }

            if (!(a == "kick"))
            {
                return;
            }
            int startIndex = message.IndexOf(" ", StringComparison.Ordinal) + 1;
            string username = message.Substring(startIndex);
            if (!GameManager.instance.KickPlayer(username))
            {
                ChatBox.Instance.AppendMessage(0, "Failed to kick player...", GameManager.players[LocalClient.instance.myId].username);
            }
        }

        public void EarlyUpdate()
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                if (ChatBox.Instance.typing)
                {
                    CustomSendMessage(ChatBox.Instance.inputField.text);
                }
                else
                {
                    if (ChatBox.Instance.typing)
                    {
                        return;
                    }
                    ChatBox.Instance.typing = false;
                    ChatBox.Instance.overlay.CrossFadeAlpha(0f, 1f, true);
                    ChatBox.Instance.messages.CrossFadeAlpha(0f, 1f, true);
                    ChatBox.Instance.inputField.GetComponent<Image>().CrossFadeAlpha(0f, 1f, true);
                    ChatBox.Instance.inputField.GetComponentInChildren<TextMeshProUGUI>().CrossFadeAlpha(0f, 1f, true);
                    ChatBox.Instance.inputField.interactable = true;
                    ChatBox.Instance.inputField.Select();
                    ChatBox.Instance.typing = true;
                }
            }
            if (ChatBox.Instance.typing && !ChatBox.Instance.inputField.isFocused)
            {
                ChatBox.Instance.inputField.Select();
            }
            if (Input.GetKeyDown(KeyCode.Escape) && ChatBox.Instance.typing)
            {
                ChatBox.Instance.inputField.text = "";
		        ChatBox.Instance.inputField.interactable = false;
                ChatBox.Instance.typing = false;
                base.CancelInvoke("HideChat");
                base.Invoke("HideChat", 5f);
            }
        }

        public void Update()
        {
            //if f1 is pressed, Log("F1 is pressed") will be called
            if (Input.GetKeyDown(KeyCode.F1))
            {
                Log("F1 is pressed");
            }
            
            
        }
    }
}
