#if NAS && TEN_BIT_BLOCKS
using System.Collections.Generic;
namespace NotAwesomeSurvival
{
    public class Drop
    {
        public List<BlockStack> blockStacks = null;
        public List<Item> items = null;
        public int exp = 0;
        public Drop()
        {
        }
        public Drop(Drop parent)
        {
            if (parent.blockStacks != null)
            {
                blockStacks = new();
                foreach (BlockStack bs in parent.blockStacks)
                {
                    BlockStack bsClone = new(bs.ID, bs.amount);
                    blockStacks.Add(bsClone);
                }
            }
            if (parent.items != null)
            {
                items = new();
                foreach (Item item in parent.items)
                {
                    Item itemClone = new(item.name);
                    items.Add(itemClone);
                }
            }
        }
        public Drop(ushort clientushort, int amount = 1)
        {
            BlockStack bs = new(clientushort, amount);
            blockStacks = new()
            {
                bs
            };
        }
        public Drop(Item item)
        {
            items = new()
            {
                item
            };
        }
        public Drop(Inventory inv)
        {
            blockStacks = new();
            for (int i = 0; i < inv.blocks.Length; i++)
            {
                if (inv.blocks[i] == 0)
                {
                    continue;
                }
                blockStacks.Add(new((ushort)i, inv.blocks[i]));
            }
            if (blockStacks.Count == 0)
            {
                blockStacks = null;
            }
            items = new();
            foreach (Item item in inv.items)
            {
                if (item == null)
                {
                    continue;
                }
                items.Add(item);
            }
            if (items.Count == 0)
            {
                items = null;
            }
        }
    }
    public class BlockStack
    {
        public int amount;
        public ushort ID;
        public BlockStack(ushort ID, int amount = 1)
        {
            this.ID = ID;
            this.amount = amount;
        }
    }
}
#endif