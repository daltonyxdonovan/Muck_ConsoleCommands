using BepInEx;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HarmonyLib;
using System.Reflection;
using System;
using UnityEngine.SceneManagement;

namespace ConsoleCommands
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
#region  Variables
        public string[] item_names = new string[] {"(Bark)","(Chest)","(Coal)","(Coin)","(Flint)","(Adamantite Boots)","(Chunkium Boots)","(Gold Boots)","(Mithril Boots)","(Obamium Boots)","(Steel Boots)","(Wolfskin Boots)","(Adamantite Helmet)","(Chunkium Helmet)","(Gold Helmet)","(Mithril Helmet)","(Obamium Helmet)","(Steel Helmet)","(Wolfskin Helmet)","(Adamantite Pants)","(Chunkium Pants)","(Gold Pants)","(Mithril Pants)","(Obamium Pants)","(Steel Pants)","(Wolfskin Pants)","(Adamantite Chestplate)","(Chunkium Chestplate)","(Gold Chestplate)","(Mithril Chestplate)","(Obamium Chestplate)","(Steel Chestplate)","(Wolfskin Chestplate)","(Wood Doorway)","(Wood Doorway)","(Wood Floor)","(Wood Pole)","(Wood Pole)","(Wood Roof)","(Wood stairs)","(Wood stairs thinn)","(Wood Wall)","(Wood Wall Half)","(Wood Wall Tilted)","(Torch)","(Red Apple)","(Bowl)","(Dough)","(Flax Fibers)","(Flax)","(Raw Meat)","(Gulpon Shroom)","(Ligon Shroom)","(Slurbon Shroom)","(Sugon Shroom)","(Wheat)","(Bread)","(Cooked Meat)","(Apple Pie)","(Meat Pie)","(Meat Soup)","(Purple Soup)","(Red Soup)","(Weird Soup)","(Yellow Soup)","(AncientCore)","(Adamantite bar)","(Chunkium bar)","(Gold bar)","(Iron bar)","(Mithril bar)","(Obamium bar)","(Ancient Bone)","(Dragonball)","(Fireball)","(Lightningball)","(Rock Projectile)","(Rock Projectile)","(Gronk Projectile)","(Spike Attack)","(Gronk Projectile)","(Waterball)","(Windball)","(Adamantite Ore)","(Chunkium Ore)","(Gold Ore)","(Iron Ore)","(Mithril Ore)","(Obamium Ore)","(Ruby)","(Rock)","(Birch Wood)","(Dark Oak Wood)","(Fir Wood)","(Wood)","(Oak Wood)","(Anvil)","(Cauldron)","(Fletching Table)","(Furnace)","(Workbench)","(Boat Map)","(Gem Map)","(Blue Gem)","(Green Gem)","(Pink Gem)","(Red Gem)","(Yellow Gem)","(Adamantite Axe)","(Gold Axe)","(Mithril Axe)","(Steel Axe)","(Wood Axe)","(Oak Bow)","(Wood Bow)","(Birch bow)","(Fir bow)","(Ancient Bow)","(Adamantite Pickaxe)","(Gold Pickaxe)","(Mithril Pickaxe)","(Steel Pickaxe)","(Wood Pickaxe)","(Rope)","(Shovel)","(Adamantite Sword)","(Gold Sword)","(Mithril Sword)","(Obamium Sword)","(Steel Sword)","(milk)","(Adamantite Arrow)","(Fire arrow)","(Flint Arrow)","(Lightning Arrow)","(Mithril Arrow)","(Steel Arrow)","(Water Arrow)","(Chiefs Spear)","(Chunky Hammer)","(Gronks Sword)","(Gronks Sword Projectile)","(Night Blade)","(Wyvern Dagger)","(Black Shard)","(Blade)","(Hammer Shaft)","(Spear Tip)","(Sword Hilt)","(Wolf Claws)","(Wolfskin)","(Wyvern Claws)"};
        public string[] first_commands = new string[] {"/give", "/maxhp", "/maxstamina", "maxshield", "/maxhunger", "/totem", "/respawn", "/gold", "/nomobs", "/nobosses", "/heal", "/indestructible", "/help"};
        public string[] help_commands = new string[] {"give", "maxhp", "maxstamina", "maxshield", "maxhunger", "totem", "respawn", "gold", "nomobs", "nobosses", "heal", "indestructible", "help"};
        ChatBox chatBox;
        GameObject myPanel;
        bool active = false;
        int ticker = 0;
        bool maxhp_active = false;
        public int maxhp_amount = 100;
        public static int maxhp_amount_static = 100;
        bool indestructible = false;
        bool noBosses = false;
        bool noMobs = false;

        Canvas canvas;
        TextMeshProUGUI ac_text;
        bool done = false;

        #endregion Variables
        
        
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
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
            
        }

        //ChatCommand is where we actually parse the commands that are sent to the chat
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
                    else if (all[i].name == "Planks_Pole_half(Clone)")
                    {
                        Hitable hitable = all[i].GetComponent<Hitable>();
                        hitable.maxHp = 999999999;
                        hitable.hp = 999999999;
                        hitable.canHitMoreThanOnce = false;
                    }
                    else if (all[i].name == "Planks_Pole(Clone)")
                    {
                        Hitable hitable = all[i].GetComponent<Hitable>();
                        hitable.maxHp = 999999999;
                        hitable.hp = 999999999;
                        hitable.canHitMoreThanOnce = false;
                    }
                    else if (all[i].name == "Planks_Tilt(Clone)")
                    {
                        Hitable hitable = all[i].GetComponent<Hitable>();
                        hitable.maxHp = 999999999;
                        hitable.hp = 999999999;
                        hitable.canHitMoreThanOnce = false;
                    }
                    else if (all[i].name == "Planks_Stairs_Half(Clone)")
                    {
                        Hitable hitable = all[i].GetComponent<Hitable>();
                        hitable.maxHp = 999999999;
                        hitable.hp = 999999999;
                        hitable.canHitMoreThanOnce = false;
                    }
                    else if (all[i].name == "Planks_Roof(Clone)")
                    {
                        Hitable hitable = all[i].GetComponent<Hitable>();
                        hitable.maxHp = 999999999;
                        hitable.hp = 999999999;
                        hitable.canHitMoreThanOnce = false;
                    }
                    else if (all[i].name == "Planks_Doorway(Clone)")
                    {
                        Hitable hitable = all[i].GetComponent<Hitable>();
                        hitable.maxHp = 999999999;
                        hitable.hp = 999999999;
                        hitable.canHitMoreThanOnce = false;
                    }
                    else if (all[i].name == "Planks_WallHalf(Clone)")
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
        
            if (command.StartsWith("/list"))
            {
                Log("/maxstamina, /maxhp, /maxhunger, /maxshield, /totem, /indestructible, /nobosses, /nomobs, /heal, /print, /give, /gold, /list, /help");
            }

            if (command.StartsWith("/help"))
            {
                if (command.Split().Length > 1)
                {
                    string choice = command.Split()[1];
                    if (choice == "maxstamina")
                        Log("/maxstamina <amount> - sets max stamina to <amount>");
                    if (choice == "maxhp")
                        Log("/maxhp <amount> - sets max hp to <amount>");
                    if (choice == "maxhunger")
                        Log("/maxhunger <amount> - sets max hunger to <amount>");
                    if (choice == "maxshield")
                        Log("/maxshield <amount> - sets max shield to <amount>");
                    if (choice == "totem")
                        Log("/totem - moves totem to player");
                    if (choice == "indestructible")
                        Log("/indestructible - makes all buildings indestructible");
                    if (choice == "nobosses")
                        Log("/nobosses <true/false> - removes all bosses");
                    if (choice == "nomobs")
                        Log("/nomobs <true/false> - removes all mobs");
                    if (choice == "heal")
                        Log("/heal <amount> - heals player for <amount> hp");
                    if (choice == "print")
                        Log("/print - prints all item names to bepinex log. go to bepinex/config/bepinex.cfg and change logging to true for complete usage.");
                    if (choice == "give")
                        Log("/give (<item name>) <amount> - gives <amount> of <item name> to player. Parentheses are needed!");
                    if (choice == "gold")
                        Log("/gold <amount> - gives <amount> gold to player");
                    if (choice == "list")
                        Log("/list - lists all commands");
                    if (choice == "help")
                        Log("/help <command> - gives info on <command>");
                }
                else
                {
                    Log("run /help <command> for more info");
                }
            }
        }


        public void Update()
        {
            if (SceneManager.GetActiveScene().name != "GameAfterLobby")
            {
                myPanel = null;
                ac_text = null;
                chatBox = null;
                canvas = null;
                active = false;
                ticker = 0;
                done = false;
                maxhp_active = false;
                indestructible = false;
                noBosses = false;
                noMobs = false;
                //reset all the variables to gracefully handle returning to the lobby

                return;
            }
            if (done)
            {
                if (chatBox.typing)
                {
                    myPanel.SetActive(true);
                    ac_text.text = "";
                    if (chatBox.inputField.text.Split().Length > 1)
                    {
                        if (chatBox.inputField.text.Split()[0] == "/give")
                        {
                            string[] textToDisplay = chatBox.inputField.text.Split();
                            for (int i = 0; i < item_names.Length; i++)
                            {
                                if (textToDisplay[1] != "" && textToDisplay[1] != " " && item_names[i].StartsWith(textToDisplay[1].ToString()))
                                {
                                    ac_text.text = ac_text.text + " " + item_names[i];
                                }
                            }

                            //this is for autocomplete for the second word
                            if (Input.GetKeyDown(KeyCode.Tab)) 
                            {
                                string[] strings = ac_text.text.Split();
                                string first_word = chatBox.inputField.text.Split()[0].ToString();
                                string needed_word = strings[1].ToString();
                                chatBox.inputField.text = first_word + " " + needed_word;
                                chatBox.inputField.caretPosition = chatBox.inputField.text.Length + 1;
                            }
                        }
                        else if (chatBox.inputField.text.Split()[0] == "/help")
                        {
                            string[] textToDisplay = chatBox.inputField.text.Split();
                            for (int i = 0; i < help_commands.Length; i++)
                            {
                                if (textToDisplay[1] != "" && textToDisplay[1] != " " && help_commands[i].StartsWith(textToDisplay[1].ToString()))
                                {
                                    ac_text.text = ac_text.text + " " + help_commands[i];
                                }
                            }

                            //this is for autocomplete for the second word
                            if (Input.GetKeyDown(KeyCode.Tab)) 
                            {
                                string[] strings = ac_text.text.Split();
                                string first_word = chatBox.inputField.text.Split()[0].ToString();
                                string needed_word = strings[1].ToString();
                                chatBox.inputField.text = first_word + " " + needed_word;
                                chatBox.inputField.caretPosition = chatBox.inputField.text.Length + 1;
                            }
                        }
                        else    
                        {
                            string[] textToDisplay = chatBox.inputField.text.Split();
                            for (int i = 0; i < first_commands.Length; i++)
                            {
                                if (textToDisplay[1] != "" && textToDisplay[1] != " " && item_names[i].StartsWith(textToDisplay[1].ToString()))
                                {
                                    ac_text.text = ac_text.text + " " + first_commands[i];
                                }
                            }

                            //this is for autocomplete for the second word
                            if (Input.GetKeyDown(KeyCode.Tab)) 
                            {
                                string[] strings = ac_text.text.Split();
                                string first_word = chatBox.inputField.text.Split()[0].ToString();
                                string needed_word = strings[1].ToString();
                                chatBox.inputField.text = first_word + " " + needed_word;
                                chatBox.inputField.caretPosition = chatBox.inputField.text.Length + 1;
                            }
                        }
                    }
                    else
                    {
                        if (chatBox.inputField.text == "") return;
                        for (int i = 0; i < first_commands.Length; i++)
                        {
                            if (first_commands[i].StartsWith(chatBox.inputField.text.ToString()))
                            {
                                ac_text.text = ac_text.text + " " + first_commands[i];
                            }
                        }
                        //autocomplete for first word
                        if (Input.GetKeyDown(KeyCode.Tab))
                        {
                            string[] strings = ac_text.text.Split();
                            string needed_word = strings[1].ToString();
                            chatBox.inputField.text = needed_word;
                            chatBox.inputField.caretPosition = chatBox.inputField.text.Length+1;
                        }
                    }
                }
                else
                {
                    myPanel.SetActive(false);
                }
            }
            if (Input.GetKeyDown(KeyCode.X))
            {
                PickupInteract pickupInteract = FindObjectOfType<PickupInteract>();
                if (pickupInteract != null)
                {   
                    if (pickupInteract.item.name == "Planks_Stairs(Clone)" || 
                        pickupInteract.item.name == "Planks_Wall(Clone)" || 
                        pickupInteract.item.name == "Planks_floor(Clone)" || 
                        pickupInteract.item.name == "Planks_Pole_half(Clone)" || 
                        pickupInteract.item.name == "Planks_Pole(Clone)" || 
                        pickupInteract.item.name == "Planks_Tilt(Clone)" || 
                        pickupInteract.item.name == "Planks_Stairs_Half(Clone)" || 
                        pickupInteract.item.name == "Planks_Roof(Clone)" || 
                        pickupInteract.item.name == "Planks_Doorway(Clone)" || 
                        pickupInteract.item.name == "Planks_WallHalf(Clone)")
                        
                        pickupInteract.RemoveObject();
                }
            }
            if (!active && ticker == 0)
            {
                chatBox = GameObject.Find("Chat").GetComponent<ChatBox>();
                if (chatBox == null)
                    active = false;
                if (chatBox != null)
                {
                    active = true;
                    canvas = GameObject.Find("UI (1)").GetComponent<Canvas>();
                }
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
                if (ticker > 120)
                {
                    
                    if (maxhp_active)
                    {
                        PlayerStatus playerStatus = PlayerStatus.Instance;
                        playerStatus.maxHp = maxhp_amount;
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
                            if (all[i].name == "LilDave(Clone)")
                            {
                                Destroy(all[i]);
                            }
                            if (all[i].name == "StoneGolem(Clone)")
                            {
                                Destroy(all[i]);
                            }
                            if (all[i].name == "Wolf(Clone)")
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
                                BuildDestruction buildDestruction = all[i].GetComponent<BuildDestruction>();
                                hitable.maxHp = 999999999;
                                hitable.hp = 999999999;
                                hitable.canHitMoreThanOnce = false;
                                buildDestruction.connectedToGround = true;
                                buildDestruction.directlyGrounded = true;
                                
                            }
                            else if (all[i].name == "Planks_Wall(Clone)")
                            {
                                Hitable hitable = all[i].GetComponent<Hitable>();
                                BuildDestruction buildDestruction = all[i].GetComponent<BuildDestruction>();
                                hitable.maxHp = 999999999;
                                hitable.hp = 999999999;
                                hitable.canHitMoreThanOnce = false;
                                buildDestruction.connectedToGround = true;
                                buildDestruction.directlyGrounded = true;
                            }
                            else if (all[i].name == "Planks_Stairs(Clone)")
                            {
                                Hitable hitable = all[i].GetComponent<Hitable>();
                                BuildDestruction buildDestruction = all[i].GetComponent<BuildDestruction>();
                                hitable.maxHp = 999999999;
                                hitable.hp = 999999999;
                                hitable.canHitMoreThanOnce = false;
                                buildDestruction.connectedToGround = true;
                                buildDestruction.directlyGrounded = true;
                            }
                        }
                    }
                    

                    ticker = 0;
                }
                if (Input.GetKeyDown(KeyCode.Slash))
                {
                    if (!done)
                    {
                        myPanel = new GameObject("daltonyx_panel");
                        myPanel.transform.SetParent(canvas.transform, false);
                        ac_text = myPanel.AddComponent<TextMeshProUGUI>();
                        ac_text.fontSize = 17;
                        ac_text.color = Color.white;
                        ac_text.fontStyle = FontStyles.Bold;
                        ac_text.alignment = TextAlignmentOptions.Left;
                        myPanel.transform.position = new Vector3(chatBox.inputField.transform.position.x-118, chatBox.inputField.transform.position.y-17, chatBox.inputField.transform.position.z);
                        ac_text.enableWordWrapping = false;
                        ac_text.overflowMode = TextOverflowModes.Overflow;
                        done = true;
                    }
                    
                    
                    if (chatBox.typing)
                    {
                        // If the last character of the message is '/', remove it and return the updated message
                        if (chatBox.inputField.text[chatBox.inputField.text.Length - 1] == '/')
                        {
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
