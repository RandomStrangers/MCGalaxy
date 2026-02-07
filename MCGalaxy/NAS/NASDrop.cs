using System.Collections.Generic;
namespace MCGalaxy
{
    public class NASDrop
    {
        public List<NASBlockStack> blockStacks = null;
        public List<NASItem> items = null;
        public int exp = 0;
        public NASDrop()
        {
        }
        public NASDrop(NASDrop parent)
        {
            if (parent.blockStacks != null)
            {
                blockStacks = new();
                foreach (NASBlockStack bs in parent.blockStacks)
                {
                    NASBlockStack bsClone = new(bs.ID, bs.amount);
                    blockStacks.Add(bsClone);
                }
            }
            if (parent.items != null)
            {
                items = new();
                foreach (NASItem item in parent.items)
                {
                    NASItem itemClone = new(item.name);
                    items.Add(itemClone);
                }
            }
        }
        public NASDrop(ushort clientushort, int amount = 1)
        {
            NASBlockStack bs = new(clientushort, amount);
            blockStacks = new()
            {
                bs
            };
        }
        public NASDrop(NASItem item) => items = new()
            {
                item
            };
        public NASDrop(NASInventory inv)
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
            foreach (NASItem item in inv.items)
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
    public class NASBlockStack
    {
        public int amount;
        public ushort ID;
        public NASBlockStack(ushort ID, int amount = 1)
        {
            this.ID = ID;
            this.amount = amount;
        }
    }
}
