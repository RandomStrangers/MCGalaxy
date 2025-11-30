#if NAS && TEN_BIT_BLOCKS
using MCGalaxy;
using MCGalaxy.Network;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;
namespace NotAwesomeSurvival
{
    public partial class Inventory
    {
        [JsonIgnore] public ColorDesc[] selectorColors = DynamicColor.defaultColors;
        [JsonIgnore] public Item HeldItem { get { return items[selectedItemIndex] ?? Item.Fist; } }
        [JsonIgnore] public bool bagOpen = false;
        [JsonIgnore] public int slotToMoveTo = -1;
        [JsonIgnore] public bool deleting = false;
        public Item[] items = new Item[27];
        public int selectedItemIndex = 0;
        public void SetupItems()
        {
            MoveBar(0, ref selectedItemIndex);
        }
        public bool GetItem(Item item)
        {
            if (items[selectedItemIndex] == null)
            {
                items[selectedItemIndex] = item;
                Message("You got {0}&S!", item.displayName);
                return true;
            }
            for (int i = 0; i < 27; i++)
            {
                if (items[i] == null)
                {
                    items[i] = item;
                    Message("You got {0}&S!", item.displayName);
                    return true;
                }
            }
            Message("You can't get {0}&S because your tool bag is full.", item.displayName);
            return false;
        }
        public void ToggleBagOpen()
        {
            deleting = false;
            NasPlayer np = NasPlayer.GetNasPlayer(p);
            bagOpen = !bagOpen;
            if (bagOpen)
            {
                p.Send(Packet.Motd(p, "-hax horspeed=0.000001"));
                whereHeldBlockIsDisplayed = CpeMessageType.Status2;
                np.whereHealthIsDisplayed = CpeMessageType.Status3;
            }
            else
            {
                p.SendMapMotd();
                SendCpeMessage(CpeMessageType.Status2, "");
                SendCpeMessage(CpeMessageType.Status3, "");
                whereHeldBlockIsDisplayed = CpeMessageType.BottomRight3;
                np.whereHealthIsDisplayed = CpeMessageType.BottomRight2;
            }
            if (slotToMoveTo != -1)
            {
                MoveBar(0, ref slotToMoveTo);
            }
            else
            {
                MoveBar(0, ref selectedItemIndex);
            }
            DisplayHeldBlock(np.heldNasBlock);
            np.DisplayHealth();
        }
        public void DoItemMove()
        {
            if (slotToMoveTo == -1)
            {
                BeginItemMove();
            }
            else
            {
                FinalizeItemMove();
            }
            UpdateItemDisplay();
        }
        public void BeginItemMove()
        {
            slotToMoveTo = selectedItemIndex;
        }
        public void FinalizeItemMove()
        {
            Item gettingMoved = items[selectedItemIndex],
                to = items[slotToMoveTo];
            items[selectedItemIndex] = to;
            items[slotToMoveTo] = gettingMoved;
            selectedItemIndex = slotToMoveTo;
            slotToMoveTo = -1;
        }
        public void MoveItemBarSelection(int direction)
        {
            NasPlayer np = NasPlayer.GetNasPlayer(p);
            if (!np.hasBeenSpawned)
            {
                Message("&chasBeenSpawned is &cfalse&S, this shouldn't happen if you didn't just die.");
                Message("&bPlease report to randomstrangers on Discord what you were doing before this happened");
            }
            Item heldItemBeforeScrolled = HeldItem;
            deleting = false;
            if (slotToMoveTo != -1)
            {
                MoveBar(direction, ref slotToMoveTo);
                return;
            }
            MoveBar(direction, ref selectedItemIndex);
            if (heldItemBeforeScrolled != HeldItem)
            {
                NasPlayer.StartCooldown(p, np.inventory.HeldItem.Prop.recharge);
                np.ResetBreaking();
                NasEffect.UndefineEffect(p, NasBlockChange.BreakMeterID);
            }
        }
        public void MoveBar(int direction, ref int selection)
        {
            int length = bagOpen ? 27 : 9;
            if (bagOpen)
            {
                int offset = 0;
            thing:
                if (offset <= 18)
                {
                    if (selection == offset + 8 && selection + direction == offset + 9)
                    {
                        direction -= 9;
                    }
                    else if (selection == offset && selection + direction == offset - 1)
                    {
                        direction += 9;
                    }
                    offset += 9;
                    goto thing;
                }
            }
            selection += direction;
            selection %= length;
            if (selection < 0)
            {
                selection += length;
            }
            UpdateItemDisplay();
        }
        public void UpdateItemDisplay()
        {
            selectorColors = HeldItem.HealthColors;
            if (bagOpen)
            {
                DisplayItemBar(0, "&7↑ª", "&7ª↑", CpeMessageType.BottomRight3);
                DisplayItemBar(9, "&7←¥", "&7₧→", CpeMessageType.BottomRight2);
                DisplayItemBar(18, "&7↓º", "&7º↓", CpeMessageType.BottomRight1);
                return;
            }
            DisplayItemBar();
        }
        public void DisplayItemBar(int offset = 0, string prefix = "&7←«", string suffix = "%7»→",
                                   CpeMessageType location = CpeMessageType.BottomRight1)
        {
            StringBuilder builder = new(prefix);
            for (int i = offset; i < 9 + offset; i++)
            {
                bool moving = !(slotToMoveTo == -1),
                    handsHere = i == slotToMoveTo,
                    selectionHere = i == selectedItemIndex,
                    selectionNext = moving ? i + 1 == slotToMoveTo : i + 1 == selectedItemIndex;
                int itemIndex = i;
                if (handsHere)
                {
                    builder.Append("&h╣");
                }
                else if (selectionHere && !moving)
                {
                    if (deleting)
                    {
                        builder.Append("&h╙");
                    }
                    else
                    {
                        Item item2 = items[itemIndex];
                        if (item2 != null && item2.Prop.color == "`")
                        {
                            builder.Append("&`ƒ");
                        }
                        else
                            builder.Append("&hƒ");
                    }
                }
                else if (i == offset)
                {
                    builder.Append("⌐");
                }
                if (handsHere)
                {
                    itemIndex = selectedItemIndex;
                }
                else if (moving && !handsHere && selectionHere)
                {
                    itemIndex = slotToMoveTo;
                }
                Item item = items[itemIndex];
                if (item == null)
                {
                    if (itemIndex > 22)
                    {
                        builder.Append("&e¬");
                    }
                    else
                    {
                        builder.Append("¬");
                    }
                }
                else
                {
                    if (item.Prop.character == "¬" && itemIndex > 22)
                    {
                        builder.Append("&e¬");
                    }
                    else
                    {
                        builder.Append(item.ColoredIcon);
                    }
                }
                if (handsHere)
                {
                    builder.Append("&h╕");
                }
                else if (selectionHere && !moving)
                {
                    if (deleting)
                    {
                        builder.Append("&h╙");
                    }
                    else
                    {
                        if (item != null && item.Prop.color == "`")
                        {
                            builder.Append("&`" + "½");
                        }
                        else
                        {
                            builder.Append((item != null && item.Enchanted() ? "&5" : "&h") + "½");
                        }
                    }
                }
                else if (!selectionNext || i == 9 + offset - 1)
                {
                    builder.Append("⌐");
                }
            }
            builder.Append(suffix);
            string final = builder.ToString();
            SendCpeMessage(location, final);
        }
        public void DeleteItem(bool confirmed = false)
        {
            if (slotToMoveTo != -1)
            {
                return;
            }
            Item item = items[selectedItemIndex];
            if (item == null)
            {
                return;
            }
            if (deleting)
            {
                if (!confirmed)
                {
                    Message("Are you sure you want to delete {0}&S?", item.displayName);
                    Message("Press P to Put it in the trash.");
                    return;
                }
                Message("Deleted {0}.", item.name);
                items[selectedItemIndex] = null;
                deleting = false;
                UpdateItemDisplay();
                return;
            }
            if (confirmed)
            {
                return;
            }
            Message("Are you sure you want to delete {0}&S?", item.displayName);
            Message("Press P to Put it in the trash.");
            deleting = true;
            UpdateItemDisplay();
        }
        public void BreakItem(ref Item item)
        {
            for (int i = 0; i < 27; i++)
            {
                if (item == items[i])
                {
                    Message("Your {0}&S broke!", item.displayName);
                    items[i] = null;
                    break;
                }
            }
        }
        public void ToolInfo()
        {
            Dictionary<int, string> nums = new()
            {
                {1,"I"},
                {2,"II"},
                {3,"III"},
                {4,"IV"},
                {5,"V"},
                {6,"VI"},
                {7,"VII"},
                {8,"VIII"},
                {9,"IX"},
                {10,"X"},
            };
            Item item = items[selectedItemIndex];
            if (item == null)
            {
                return;
            }
            Message("Tool name: {0}", item.displayName);
            Message("Tool durability: {0}/{1}", item.HP, item.Prop.baseHP);
            foreach (KeyValuePair<string, int> x in item.enchants)
            {
                if (x.Value > 0)
                {
                    Message(x.Key + " " + nums[x.Value]);
                }
            }
        }
    }
}
#endif