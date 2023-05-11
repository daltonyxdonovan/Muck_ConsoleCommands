using BepInEx;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HarmonyLib;
using System.Reflection;
using UnityEngine.SceneManagement;

namespace ConsoleCommands
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public string[] item_names = new string[] {"Bark","Chest","Coal","Coin","Flint","Adamantite Boots","Chunkium Boots","Gold Boots","Mithril Boots","Obamium Boots","Steel Boots","Wolfskin Boots","Adamantite Helmet","Chunkium Helmet","Gold Helmet","Mithril Helmet","Obamium Helmet","Steel Helmet","Wolfskin Helmet","Adamantite Pants","Chunkium Pants","Gold Pants","Mithril Pants","Obamium Pants","Steel Pants","Wolfskin Pants","Adamantite Chestplate","Chunkium Chestplate","Gold Chestplate","Mithril Chestplate","Obamium Chestplate","Steel Chestplate","Wolfskin Chestplate","Wood Doorway","Wood Doorway","Wood Floor","Wood Pole","Wood Pole","Wood Roof","Wood stairs","Wood stairs thinn","Wood Wall","Wood Wall Half","Wood Wall Tilted","Torch","Red Apple","Bowl","Dough","Flax Fibers","Flax","Raw Meat","Gulpon Shroom","Ligon Shroom","Slurbon Shroom","Sugon Shroom","Wheat","Bread","Cooked Meat","Apple Pie","Meat Pie","Meat Soup","Purple Soup","Red Soup","Weird Soup","Yellow Soup","AncientCore","Adamantite bar","Chunkium bar","Gold bar","Iron bar","Mithril bar","Obamium bar","Ancient Bone","Dragonball","Fireball","Lightningball","Rock Projectile","Rock Projectile","Gronk Projectile","Spike Attack","Gronk Projectile","Waterball","Windball","Adamantite Ore","Chunkium Ore","Gold Ore","Iron Ore","Mithril Ore","Obamium Ore","Ruby","Rock","Birch Wood","Dark Oak Wood","Fir Wood","Wood","Oak Wood","Anvil","Cauldron","Fletching Table","Furnace","Workbench","Boat Map","Gem Map","Blue Gem","Green Gem","Pink Gem","Red Gem","Yellow Gem","Adamantite Axe","Gold Axe","Mithril Axe","Steel Axe","Wood Axe","Oak Bow","Wood Bow","Birch bow","Fir bow","Ancient Bow","Adamantite Pickaxe","Gold Pickaxe","Mithril Pickaxe","Steel Pickaxe","Wood Pickaxe","Rope","Shovel","Adamantite Sword","Gold Sword","Mithril Sword","Obamium Sword","Steel Sword","milk","Adamantite Arrow","Fire arrow","Flint Arrow","Lightning Arrow","Mithril Arrow","Steel Arrow","Water Arrow","Chiefs Spear","Chunky Hammer","Gronks Sword","Gronks Sword Projectile","Night Blade","Wyvern Dagger","Black Shard","Blade","Hammer Shaft","Spear Tip","Sword Hilt","Wolf Claws","Wolfskin","Wyvern Claws"};
        public string[] commands = new string[] {"/give", "/maxhp", "/maxstamina", "maxshield", "/maxhunger", "/totem", "/respawn", "/gold", "/nomobs", "/nobosses", "/heal", "/indestructible"};
        ChatBox chatBox;
        bool active = false;
        int ticker = 0;
        bool maxhp_active = false;
        public int maxhp_amount = 100;
        public static int maxhp_amount_static = 100;
        bool indestructible = false;
        bool noBosses = false;
        bool noMobs = false;




        public void Log(string message)
        {
            
            Logger.LogInfo(message);
            chatBox.SendMessage("<color=red>" + message + "</color>");
        }

        public void EarlyUpdate()
        {

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
                maxhp_amount_static = choice;
                playerStatus.maxHp = choice;
                playerStatus.hp = choice;
                Harmony.CreateAndPatchAll(typeof(PatchUpdateStats));
                Log($"set  maxHP to {choice}");
            }

            if (command.StartsWith("/nomobs"))
            {
                bool choice = bool.Parse(command.Split()[1]);
                noMobs = choice;
                Log("set noMobs to " + choice);
                
            }

            if (command.StartsWith("/nobosses"))
            {
                bool choice = bool.Parse(command.Split()[1]);
                noBosses = choice;
                Log("set noBosses to " + choice);
                
            }



            if (command.StartsWith("/respawn"))
            {
                PlayerStatus[] playerStatuses = FindObjectsOfType<PlayerStatus>();
                for (int i = 0; i < playerStatuses.Length; i++)
                {
                    if (playerStatuses[i].IsPlayerDead())
                    {
                        playerStatuses[i].Respawn();
                        Log("Respawned player");
                    }
                }
            }

            if (command.StartsWith("/maxstamina"))
            {
                int choice = int.Parse(command.Split()[1]);
                if (choice < 1)
                {
                    choice = 1;
                }
                PlayerStatus.Instance.maxStamina = choice;
                PlayerStatus.Instance.stamina = choice;
                Log("set maxStamina to " + choice);
            }

            if (command.StartsWith("/maxhunger"))
            {
                int choice = int.Parse(command.Split()[1]);
                if (choice < 1)
                {
                    choice = 1;
                }
                PlayerStatus.Instance.maxHunger = choice;
                PlayerStatus.Instance.hunger = choice;
                Log("set maxHunger to " + choice);
            }

            if (command.StartsWith("/maxshield"))
            {
                int choice = int.Parse(command.Split()[1]);
                if (choice < 1)
                {
                    choice = 1;
                }
                PlayerStatus.Instance.maxShield = choice;
                PlayerStatus.Instance.shield = choice;
                Log("set maxShield to " + choice);
            }

            if (command.StartsWith("/totem"))
            {
                GameObject totem = GameObject.Find("TotemRespawn(Clone)");
                if (totem == null)
                {
                    Log("no totem found");
                    return;
                }
                totem.transform.position = PlayerMovement.Instance.transform.position;
                Log("moved totem to player");
            }

            if (command.StartsWith("/indestructible"))
            {
                indestructible = true;
                Log("all buildings are now indestructible");
                GameObject[] all = FindObjectsOfType<GameObject>();
                for (int i = 0; i < all.Length; i++)
                {
                    if (all[i].name == "Planks_floor(Clone)")
                    {
                        Hitable hitable = all[i].GetComponent<Hitable>();
                        hitable.maxHp = 999999999;
                        hitable.hp = 999999999;
                        hitable.canHitMoreThanOnce = false;
                    }
                    else if (all[i].name == "Planks_Wall(Clone)")
                    {
                        Hitable hitable = all[i].GetComponent<Hitable>();
                        hitable.maxHp = 999999999;
                        hitable.hp = 999999999;
                        hitable.canHitMoreThanOnce = false;
                    }
                    else if (all[i].name == "Planks_Stairs(Clone)")
                    {
                        Hitable hitable = all[i].GetComponent<Hitable>();
                        hitable.maxHp = 999999999;
                        hitable.hp = 999999999;
                        hitable.canHitMoreThanOnce = false;
                    }
                }
            }

            if (command.StartsWith("/print"))
            {
                foreach (InventoryItem inventoryItem in ItemManager.Instance.allItems.Values)
                {
                    Log(inventoryItem.name);
                }
            }

            if (command.StartsWith("/give"))
            {
                //input as '/give (Wyvern Dagger) 33'
                string[] splitCommand = command.Split();
                string itemName = "";
                int itemAmount = 1;
                bool inParentheses = false;
                foreach (string word in splitCommand)
                {
                    if (inParentheses)
                    {
                        itemName += word + " ";
                        if (word.EndsWith(")"))
                        {
                            inParentheses = false;
                            itemName = itemName.TrimEnd(')');
                        }
                    }
                    if (word.StartsWith("("))
                    {
                        inParentheses = true;
                        itemName += word.Substring(1) + " ";
                    }
                    else if (!inParentheses && word != "/give")
                    {
                        
                    }
                }
                itemAmount = int.Parse(splitCommand[splitCommand.Length - 1]);
                
                itemName = itemName.Substring(0, itemName.IndexOf(')'));
                //Log(itemName);
                //Log(itemAmount.ToString());
                InventoryItem item = ItemManager.Instance.GetItemByName(itemName);
                if (item == null)
                {
                    Log("Item not found");
                    return;
                }
                item.amount = itemAmount;
                AddItemToInventory(item);
                Log($"gave {itemAmount} {itemName} to self");
            }

            if (command.StartsWith("/gold"))
            {
                int amount = command.Split().Length > 1 ? int.Parse(command.Split()[1]) : 999;
                InventoryCell[] cells = FindObjectsOfType<InventoryCell>();
                bool done = false;
                for (int i = 0; i < cells.Length; i++)
                {
                    if (!(cells[i].currentItem == null) && cells[i].currentItem.name == "Coin" && !done)
                    {
                        cells[i].currentItem.amount += amount;
                        done = true;
                    }
                }

                if (!done)
                {
                    InventoryItem gold = ItemManager.Instance.GetItemByName("Coin");
                    gold.amount = amount;
                    AddItemToInventory(gold);

                }

                Log($"added {amount} gold to inventory");
                
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
                        //playerStatus.hp = maxhp_amount;
                    }
                    if (noMobs)
                    {
                        GameObject[] all = FindObjectsOfType<GameObject>();
                        for (int i = 0; i < all.Length; i++)
                        {
                            if (all[i].name == "Goblin(Clone)")
                            {
                                Destroy(all[i]);
                            }
                            if (all[i].name == "lilDave(Clone)")
                            {
                                Destroy(all[i]);
                            }
                        }
                    }
                    if (noBosses)
                    {
                        GameObject[] all = FindObjectsOfType<GameObject>();
                        for (int i = 0; i < all.Length; i++)
                        {
                            if (all[i].name == "Gronk(Clone)")
                            {
                                Destroy(all[i]);
                            }
                            
                            if (all[i].name == "Chief(Clone)")
                            {
                                Destroy(all[i]);
                            }
                            if (all[i].name == "BigChonk(Clone)")
                            {
                                Destroy(all[i]);
                            }
                            if (all[i].name == "Guardian(Clone)")
                            {
                                Destroy(all[i]);
                            }
                            
                        }
                    }
                    if (indestructible)
                    {
                        GameObject[] all = FindObjectsOfType<GameObject>();
                        for (int i = 0; i < all.Length; i++)
                        {
                            if (all[i].name == "Planks_floor(Clone)")
                            {
                                Hitable hitable = all[i].GetComponent<Hitable>();
                                hitable.maxHp = 999999999;
                                hitable.hp = 999999999;
                                hitable.canHitMoreThanOnce = false;
                            }
                            else if (all[i].name == "Planks_Wall(Clone)")
                            {
                                Hitable hitable = all[i].GetComponent<Hitable>();
                                hitable.maxHp = 999999999;
                                hitable.hp = 999999999;
                                hitable.canHitMoreThanOnce = false;
                            }
                            else if (all[i].name == "Planks_Stairs(Clone)")
                            {
                                Hitable hitable = all[i].GetComponent<Hitable>();
                                hitable.maxHp = 999999999;
                                hitable.hp = 999999999;
                                hitable.canHitMoreThanOnce = false;
                            }
                        }
                    }
                    

                    ticker = 0;
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


    class PatchUpdateStats
    {
        [HarmonyPatch(typeof(PlayerStatus), "UpdateStats")] // Specify target method with HarmonyPatch attribute
        [HarmonyPrefix]

        
        public static void UpdateRealStats(ref bool __runOriginal)
        {
            
            __runOriginal = false;
            return; // Skip original method
        }
    }
}
