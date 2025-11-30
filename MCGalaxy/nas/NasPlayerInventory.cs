#if NAS && TEN_BIT_BLOCKS
using MCGalaxy;
using MCGalaxy.Blocks;
using MCGalaxy.Network;
using MCGalaxy.Tasks;
using Newtonsoft.Json;
using System;
namespace NotAwesomeSurvival
{
    public partial class Inventory
    {
        [JsonIgnore] public Player p;
        [JsonIgnore] public CpeMessageType whereHeldBlockIsDisplayed = CpeMessageType.BottomRight3;
        public int[] blocks = new int[768];
        public Inventory(Player p)
        {
            this.p = p;
        }
        public static void Log(string format, params object[] args)
        {
            Logger.Log(LogType.Debug, string.Format(format, args));
        }
        public void Message(string message, params object[] args)
        {
            p.Message(string.Format(message, args));
        }
        public void Send(byte[] buffer)
        {
            p.Socket.Send(buffer, SendFlags.None);
        }
        public void SendCpeMessage(CpeMessageType type, string message)
        {
            p.SendCpeMessage(type, message);
        }
        public void SetPlayer(Player p)
        {
            this.p = p;
        }
        public void Setup()
        {
            NasPlayer np = NasPlayer.GetNasPlayer(p);
            if (!np.SetInventoryNotif)
            {
                Log("Setting up inventory for {0}", np.p.truename);
                np.SetInventoryNotif = true; //Prevent spamming console
            }
            //hide all blocks
            for (ushort clientushort = 1; clientushort <= 767; clientushort++)
            {
                Send(Packet.BlockPermission(clientushort, false, false, true));
                Send(Packet.SetInventoryOrder(clientushort, 0, true));
            }
            //unhide blocks you have access to
            for (ushort clientushort = 1; clientushort <= 767; clientushort++)
            {
                if (GetAmount(clientushort) > 0)
                {
                    UnhideBlock(clientushort);
                }
            }
            SetupItems();
        }
        public void Setup(Player pl)
        {
            SetPlayer(pl);
            Setup();
        }
        public void ClearHotbar()
        {
            for (byte i = 0; i <= 9; i++)
            {
                Send(Packet.SetHotbar(0, i, true));
            }
        }
        /// <summary>
        /// Returns a drop that contains the items the player was unable to pickup due to full inventory. If the drop is null, the player fit everything.
        /// </summary>
        public Drop GetDrop(Drop drop, bool showToNormalChat = false, bool overrideBool = false)
        {
            if (drop == null)
            {
                return null;
            }
            if (drop.exp > 0)
            {
                NasPlayer.GetNasPlayer(p).GiveExp(drop.exp);
                drop.exp = 0;
            }
            if (drop.blockStacks != null)
            {
                for (int i = 0; i < drop.blockStacks.Count; i++)
                {
                    BlockStack bs = drop.blockStacks[i];
                    SetAmount(bs.ID, bs.amount, false);
                    DisplayInfo info = new()
                    {
                        inv = this,
                        nasBlock = NasBlock.Get(bs.ID),
                        amountChanged = bs.amount
                    };
                    if (drop.blockStacks.Count == 1 || overrideBool)
                    {
                        info.showToNormalChat = showToNormalChat;
                    }
                    else
                    {
                        info.showToNormalChat = true;
                    }
                    SchedulerTask taskDisplayHeldBlock;
                    taskDisplayHeldBlock = Server.MainScheduler.QueueOnce(DisplayHeldBlockTask, info, TimeSpan.FromMilliseconds(i * 125));
                }
            }
            Drop leftovers = null;
            if (drop.items != null)
            {
                foreach (Item item in drop.items)
                {
                    if (!GetItem(item))
                    {
                        if (leftovers == null)
                        {
                            leftovers = new(item);
                        }
                        else
                        {
                            leftovers.items.Add(item);
                        }
                    }
                }
                UpdateItemDisplay();
            }
            return leftovers;
        }
        public void SetAmount(ushort clientushort, int amount, bool displayChange = true, bool showToNormalChat = false)
        {
            //TODO threadsafe
            blocks[clientushort] += amount;
            if (displayChange)
            {
                NasBlock nb = NasBlock.Get(clientushort);
                DisplayHeldBlock(nb, amount, showToNormalChat);
            }
            if (blocks[clientushort] > 0)
            {
                //more than zero? unhide the block
                UnhideBlock(clientushort);
                return;
            }
            else
            {
                //0 or less? hide the block
                HideBlock(clientushort);
            }
        }
        public int GetAmount(ushort clientushort)
        {
            //TODO threadsafe
            return blocks[clientushort];
        }
        public void DisplayHeldBlock(NasBlock nasBlock, int amountChanged = 0, bool showToNormalChat = false)
        {
            string display = DisplayedBlockString(nasBlock);
            if (amountChanged > 0)
            {
                display = "&a+" + amountChanged + " &f" + display;
            }
            if (amountChanged < 0)
            {
                display = "&c" + amountChanged + " &f" + display;
            }
            if (showToNormalChat)
            {
                Message(display);
            }
            SendCpeMessage(whereHeldBlockIsDisplayed, display);
        }
        public string DisplayedBlockString(NasBlock nasBlock)
        {
            NasPlayer np = NasPlayer.GetNasPlayer(p);
            if (nasBlock.parentID == 0)
            {
                return "┤";
            }
            int amount = GetAmount(nasBlock.parentID);
            string hand = amount <= 0 ? "┤" : "╕¼";
            return "[" + amount + "] " + nasBlock.GetName(np) + " " + hand;
        }
        public class DisplayInfo
        {
            public Inventory inv;
            public NasBlock nasBlock;
            public int amountChanged;
            public bool showToNormalChat;
        }
        public static void DisplayHeldBlockTask(SchedulerTask task)
        {
            DisplayInfo info = (DisplayInfo)task.State;
            info.inv.DisplayHeldBlock(info.nasBlock, info.amountChanged, info.showToNormalChat);
        }
        public void HideBlock(ushort clientushort)
        {
            Send(Packet.BlockPermission(clientushort, false, false, true));
            Send(Packet.SetInventoryOrder(clientushort, 0, true));
            NasBlock nasBlock = NasBlock.blocks[clientushort];
            if (nasBlock.childIDs != null)
            {
                foreach (ushort childID in nasBlock.childIDs)
                {
                    Send(Packet.BlockPermission(childID, false, false, true));
                    Send(Packet.SetInventoryOrder(childID, 0, true));
                }
            }
        }
        public void UnhideBlock(ushort clientushort)
        {
            BlockDefinition def = BlockDefinition.GlobalDefs[Nas.FromRaw(clientushort)];
            if (def == null && clientushort < 66)
            {
                def = DefaultSet.MakeCustomBlock(Nas.FromRaw(clientushort));
            }
            if (def == null)
            {
                return;
            }
            Send(Packet.BlockPermission(clientushort, true, false, true));
            Send(Packet.SetInventoryOrder(clientushort, (def.InventoryOrder == -1) ? clientushort : (ushort)def.InventoryOrder, true));
            NasBlock nasBlock = NasBlock.blocks[clientushort];
            if (nasBlock.childIDs != null)
            {
                foreach (ushort childID in nasBlock.childIDs)
                {
                    def = BlockDefinition.GlobalDefs[Nas.FromRaw(childID)];
                    if (def == null && childID < 66)
                    {
                        def = DefaultSet.MakeCustomBlock(Nas.FromRaw(childID));
                    }
                    if (def == null)
                    {
                        continue;
                    }
                    Send(Packet.BlockPermission(childID, true, false, true));
                    Send(Packet.SetInventoryOrder(childID, (def.InventoryOrder == -1) ? childID : (ushort)def.InventoryOrder, true));
                }
            }
        }
    }
}
#endif