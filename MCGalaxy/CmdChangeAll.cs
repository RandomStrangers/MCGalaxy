namespace MCGalaxy
{
    public class CmdChangeAll : Command
    {
        public override string Name => "ChangeAll";
        public override string Shortcut => "call";
        public override string Type => CommandTypes.Building;
        public override bool MuseumUsable => false;
        public override bool SuperUseable => false;
        public override sbyte DefaultRank => 100;
        public override void Use(Player p, string message)
        {
            if (message.Length == 0)
            {
                Help(p);
                return;
            }
            ushort block = Block.Parse(p, message);
            if (block == 0xff)
            {
                p.Message("&WThere is no block \"{0}\".", message);
            }
            if (block != 0xff && p.group.CanPlace[block])
            {
                int count = 0;
                for (ushort x = 0; x < p.Level.Length; x++)
                {
                    for (ushort y = 0; y < p.Level.Height; y++)
                    {
                        for (ushort z = 0; z < p.Level.Width; z++)
                        {
                            if (p.Level.GetBlock(x, y, z) != 0)
                            {
                                p.Level.UpdateBlock(Player.Console, x, y, z, block, 1 << 0, false);
                                count++;
                            }
                        }
                    }
                }
                p.Message("{0} blocks changed", count);
            }
        }
        public override void Help(Player p)
        {
            p.Message("&T/ChangeAll [block] - &HReplaces every non-air block with [block] in the whole map.");
        }
    }
}