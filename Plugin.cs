using BepInEx;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Text.RegularExpressions;
using UnityEngine.SceneManagement;

namespace ConsoleCommands
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {

        ChatBox chatBox;
        bool active = false;
        int ticker = 0;
        bool maxhp_active = false;
        int maxhp_amount = 100;
        public void Log(string message)
        {
            
            Logger.LogInfo(message);
            chatBox.SendMessage("<color=red>" + message + "</color>");
        }

        public void DropItemIntoWorld(InventoryItem item)
        {
            if (item == null)
            {
                return;
            }
            ClientSend.DropItem(item.id, item.amount);
        }

        public int AddItemToInventory(InventoryItem item)
        {
            InventoryItem inventoryItem = ScriptableObject.CreateInstance<InventoryItem>();
            inventoryItem.Copy(item, item.amount);
            InventoryCell inventoryCell = null;
            UiSfx.Instance.PlayPickup();
            if (AchievementManager.Instance)
            {
                AchievementManager.Instance.PickupItem(item);
            }
            InventoryUI inventoryUI = InventoryUI.Instance;
            foreach (InventoryCell inventoryCell2 in inventoryUI.cells)
            {
                if (inventoryCell2.currentItem == null)
                {
                    if (!(inventoryCell != null))
                    {
                        inventoryCell = inventoryCell2;
                    }
                }
                else if (inventoryCell2.currentItem.Compare(inventoryItem) && inventoryCell2.currentItem.stackable)
                {
                    if (inventoryCell2.currentItem.amount + inventoryItem.amount <= inventoryCell2.currentItem.max)
                    {
                        inventoryCell2.currentItem.amount += inventoryItem.amount;
                        inventoryCell2.UpdateCell();
                        UiEvents.Instance.AddPickup(inventoryItem);
                        return 0;
                    }
                    int num = inventoryCell2.currentItem.max - inventoryCell2.currentItem.amount;
                    inventoryCell2.currentItem.amount += num;
                    inventoryItem.amount -= num;
                    inventoryCell2.UpdateCell();
                }
            }
            if (inventoryCell)
            {
                inventoryCell.currentItem = inventoryItem;
                inventoryCell.UpdateCell();
                MonoBehaviour.print("added to available cell");
                UiEvents.Instance.AddPickup(inventoryItem);
                return 0;
            }
            UiEvents.Instance.AddPickup(inventoryItem);
            return inventoryItem.amount;
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
            
            chatBox.inputField.text = "";
		    chatBox.inputField.interactable = false;
        }

        public void CustomSendMessage(string message)
        {
            
            chatBox.typing = false;
            message = TrimMessage(message);
            if (message == "")
            {
                return;
            }
            if (message[0] == '/')
            {
               
                // If the last character of the message is '/', remove it and return the updated message
                if (message[message.Length - 1] == '/')
                {
                    message.Substring(0, message.Length - 1);
                }
                ChatCommand(message);
                return;
            }
            
            chatBox.AppendMessage(0, message, GameManager.players[LocalClient.instance.myId].username);
            ClientSend.SendChatMessage(message);
            ClearMessage();
        }

        public void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
            //find the object named chatBox
        }

        public void ChatCommand(string message)
        {
            
            if (message.Length <= 0)
            {
                return;
            }
            string command = message;
            chatBox.inputField.text = "";
		    chatBox.inputField.interactable = false;
            


            if (command.StartsWith("/heal"))
            {
                
                int choice = int.Parse(command.Split()[1]);
                
                if (choice < 1)
                {
                    choice = 1;
                    Log("cannot use a value less than 1");
                }
                
                

                PlayerStatus.Instance.Heal(choice);
                Log($"healed {choice} hp");
                return;
            }

            if (command.StartsWith("/maxhp"))
            {
                maxhp_active = true;


                PlayerStatus playerStatus = PlayerStatus.Instance;
                int choice = int.Parse(command.Split()[1]);
                
                if (choice < 1)
                {
                    choice = 1;
                }
                maxhp_amount = choice;

                playerStatus.maxHp = choice;
                playerStatus.hp = choice;
                Log($"set their maxHP to {choice}");
            }

            if (command.StartsWith("/gold"))
            {
                
                InventoryCell[] cells = FindObjectsOfType<InventoryCell>();
                bool done = false;
                for (int i = 0; i < cells.Length; i++)
                {
                    if (!(cells[i].currentItem == null) && cells[i].currentItem.name == "Coin" && !done)
                    {
                        cells[i].currentItem.amount += 99999;
                        done = true;
                    }
                }

                if (!done)
                {
                    InventoryItem gold = ItemManager.Instance.GetItemByName("Coin");
                    AddItemToInventory(gold);

                }

                Log($"added gold to inventory");
                
            }
        }


        public void Update()
        {
            if (SceneManager.GetActiveScene().name != "GameAfterLobby") return;
            
            //if active and scene name is 'GameAfterLobby'
            if (!active && ticker == 0)
            {
                //try to reference chat without causing an error
                chatBox = GameObject.Find("Chat").GetComponent<ChatBox>();
                if (chatBox == null)
                    active = false;
                if (chatBox != null)
                    active = true;
            }

            if (!active)
            {
                ticker++;
                if (ticker > 120)
                {
                    
                    ticker = 0;
                }
                   
                
            }
            else if (active)
            {
                ticker++;
                if (ticker > 600)
                {
                    if (maxhp_active)
                    {
                        PlayerStatus playerStatus = PlayerStatus.Instance;
                        playerStatus.maxHp = maxhp_amount;
                        playerStatus.hp = maxhp_amount;
                    }
                    
                }
                if (Input.GetKeyDown(KeyCode.Slash))
                {

                    
                    if (chatBox.typing)
                    {
                        // If the last character of the message is '/', remove it and return the updated message
                        if (chatBox.inputField.text[chatBox.inputField.text.Length - 1] == '/')
                        {
                            //trim the last character
                            chatBox.inputField.text = chatBox.inputField.text.Substring(0, chatBox.inputField.text.Length - 1);

                        }
                        CustomSendMessage(chatBox.inputField.text);
                    }
                    else
                    {
                        if (chatBox.typing)
                        {
                            return;
                        }
                        chatBox.typing = false;
                        chatBox.overlay.CrossFadeAlpha(0f, 1f, true);
                        chatBox.messages.CrossFadeAlpha(0f, 1f, true);
                        chatBox.inputField.GetComponent<Image>().CrossFadeAlpha(0f, 1f, true);
                        chatBox.inputField.GetComponentInChildren<TextMeshProUGUI>().CrossFadeAlpha(0f, 1f, true);
                        chatBox.inputField.interactable = true;
                        chatBox.inputField.Select();
                        chatBox.typing = true;
                    }
                }
                if (chatBox.typing && !chatBox.inputField.isFocused)
                {
                    chatBox.inputField.Select();
                    //add a '/' to text box
                    chatBox.inputField.MoveTextEnd(false);
                    chatBox.inputField.text = "/";
                    //move caret to teh right
                    chatBox.inputField.caretPosition = 1;

                }
                if (Input.GetKeyDown(KeyCode.Escape) && chatBox.typing)
                {
                    chatBox.inputField.text = "";
                    chatBox.inputField.interactable = false;
                    chatBox.typing = false;
                    base.CancelInvoke("HideChat");
                    base.Invoke("HideChat", 5f);
                }


            }



        }
    }
}
