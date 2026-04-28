using MCGalaxy.Generator.Foliage;
using System;
namespace MCGalaxy
{
    public static class NASTree
    {
        public static void GenOakTree(NASLevel nl, Random r, int x, int y, int z, bool broadcastChange = false)
        {
            Tree oak = new OakTree();
            oak.SetData(r, r.Next(0, 8));
            PlaceBlocks(nl.lvl, oak, x, y, z, broadcastChange);
        }
        public static void GenSwampTree(NASLevel nl, Random r, int x, int y, int z, bool broadcastChange = false)
        {
            Tree swamp = new SwampTree();
            swamp.SetData(r, r.Next(4, 8));
            PlaceBlocks(nl.lvl, swamp, x, y, z, broadcastChange);
        }
        public static void GenBirchTree(NASLevel nl, Random r, int x, int y, int z, bool broadcastChange = false)
        {
            Tree birch = new BirchTree();
            birch.SetData(r, r.Next(5, 8));
            PlaceBlocks(nl.lvl, birch, x, y, z, broadcastChange);
        }
        public static void GenSpruceTree(NASLevel nl, Random r, int x, int y, int z, bool broadcastChange = false)
        {
            Tree spruce = new SpruceTree();
            spruce.SetData(r, r.Next(0, 8));
            PlaceBlocks(nl.lvl, spruce, x, y, z, broadcastChange);
        }
        public static void PlaceBlocks(Level lvl, Tree tree, int x, int y, int z, bool broadcastChange) => tree.Generate((ushort)x, (ushort)y, (ushort)z, (X, Y, Z, block) =>
                                                                                                                    {
                                                                                                                        ushort here = lvl.GetBlock(X, Y, Z);
                                                                                                                        if (NASBlock.CanPhysicsKillThis(here) || NASBlock.IsPartOfSet(NASBlock.leafSet, here) != -1)
                                                                                                                        {
                                                                                                                            lvl.SetBlock(X, Y, Z, block);
                                                                                                                            if (broadcastChange)
                                                                                                                                lvl.BroadcastChange(X, Y, Z, block);
                                                                                                                        }
                                                                                                                    });
    }
}