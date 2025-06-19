#if NAS && !NET_20 && TEN_BIT_BLOCKS
using System;
using System.Collections.Generic;
using MCGalaxy;
using MCGalaxy.Maths;
using NasBlockAction = System.Action<NotAwesomeSurvival.NasLevel, NotAwesomeSurvival.NasBlock, int, int, int>;
using NasBlockInteraction =
    System.Action<NotAwesomeSurvival.NasPlayer, MCGalaxy.Events.PlayerEvents.MouseButton, MCGalaxy.Events.PlayerEvents.MouseAction,
    NotAwesomeSurvival.NasBlock, ushort, ushort, ushort>;
using NasBlockExistAction =
    System.Action<NotAwesomeSurvival.NasPlayer,
    NotAwesomeSurvival.NasBlock, bool, ushort, ushort, ushort>;
using NasBlockCollideAction =
    System.Action<NotAwesomeSurvival.NasEntity,
    NotAwesomeSurvival.NasBlock, bool, ushort, ushort, ushort>;
namespace NotAwesomeSurvival
{
    public partial class NasBlock
    {
        public static NasBlock[] blocks = new NasBlock[Block.MaxRaw + 1];
        public static NasBlock[] blocksIndexedByServerushort;
        public static NasBlock Default;
        public static int[] DefaultDurabilities = new int[(int)Material.Count];
        public static NasBlock Get(ushort clientushort)
        {
            return blocks[clientushort] ?? Default;
        }
        /// <summary>
        /// Leave id arg blank to use parent's name
        /// </summary>
        public string GetName(Player p, ushort id = ushort.MaxValue)
        {
            if (id == ushort.MaxValue)
            {
                id = parentID;
            }
            return GetBlockName(p, Block.FromRaw(id)).Split('-')[0];
        }
        public static string GetBlockName(Player p, ushort block)
        {
            if (Block.IsPhysicsType(block))
            {
                return "Physics block";
            }
            BlockDefinition def;
            if (!p.IsSuper)
            {
                def = p.level.GetBlockDef(block);
                if (def == null)
                {
                    def = BlockDefinition.GlobalDefs[block];
                }
            }
            else
            {
                def = BlockDefinition.GlobalDefs[block];
            }
            if (def != null)
            {
                return def.Name;
            }
            return "Unknown";
        }
        public static Drop DefaultDropHandler(NasPlayer np, ushort id)
        {
            return new Drop(id);
        }
        //value is default durability, which is considered in terms of how many "hits" it takes to break
        public enum Material
        {
            None,
            Gas,
            Stone,
            Earth,
            Wood,
            Plant,
            Leaves,
            Organic,
            Glass,
            Metal,
            Liquid,
            Lava,
            Count
        }
        public ushort selfID;
        public ushort parentID;
        public ushort alternateID;
        public List<ushort> childIDs = null;
        public Material material;
        public int tierOfToolNeededToBreak;
        public Type type;
        public int durability;
        public float damageDoneToTool;
        public Func<NasPlayer, ushort, Drop> dropHandler;
        public int resourceCost;
        public Crafting.Station station;
        public Container container;
        public bool collides = true;
        public AABB bounds;
        public float fallDamageMultiplier = -1;
        public float disturbDelayMax = 0f;
        public float disturbDelayMin = 0f;
        public int expGivenMax = 0;
        public int expGivenMin = 0;
        public float beginDelayMax = 0f;
        public float beginDelayMin = 0f;
        public NasBlockAction disturbedAction = null;
        public NasBlockAction instantAction = null;
        //public NasBlockAction beginAction = null;
        public NasBlockInteraction interaction = null;
        public NasBlockExistAction existAction = null;
        public NasBlockCollideAction collideAction = null;//DefaultCollideAction();
        public NasBlock(ushort id, Material mat)
        {
            selfID = id;
            parentID = id;
            alternateID = id;
            material = mat;
            tierOfToolNeededToBreak = 0;
            durability = DefaultDurabilities[(int)mat];
            damageDoneToTool = 1f;
            if (material == Material.Leaves || durability == 0)
            {
                damageDoneToTool = 0;
            }
            dropHandler = DefaultDropHandler;
            resourceCost = 1;
            station = null;
        }
        public NasBlock(ushort id, Material mat, int dur, int tierOfToolNeededToBreak = 0) : this(id, mat)
        {
            durability = dur;
            this.tierOfToolNeededToBreak = tierOfToolNeededToBreak;
        }
        public NasBlock(ushort id, NasBlock parent)
        {
            selfID = id;
            alternateID = id;
            if (blocks[parent.parentID].childIDs == null)
            {
                blocks[parent.parentID].childIDs = new List<ushort>();
            }
            blocks[parent.parentID].childIDs.Add(id);
            parentID = parent.parentID;
            material = parent.material;
            tierOfToolNeededToBreak = parent.tierOfToolNeededToBreak;
            durability = parent.durability;
            damageDoneToTool = parent.damageDoneToTool;
            dropHandler = parent.dropHandler;
            resourceCost = parent.resourceCost;
            if (parent.station != null)
            {
                station = new Crafting.Station(parent.station);
            }
            if (parent.container != null)
            {
                container = new Container(parent.container);
            }
            if (parent.disturbedAction != null)
            {
                disturbedAction = parent.disturbedAction;
            }
            if (parent.interaction != null)
            {
                interaction = parent.interaction;
            }
            if (parent.existAction != null)
            {
                existAction = parent.existAction;
            }
        }
    }
}
#endif