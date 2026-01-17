#if NAS && TEN_BIT_BLOCKS
using MCGalaxy;
using MCGalaxy.Maths;
using System;
using System.Collections.Generic;
using NasBlockAction = NotAwesomeSurvival.Action<NotAwesomeSurvival.NasLevel, NotAwesomeSurvival.NasBlock, int, int, int>;
using NasBlockCollideAction =
    NotAwesomeSurvival.Action<NotAwesomeSurvival.NasEntity,
    NotAwesomeSurvival.NasBlock, bool, ushort, ushort, ushort>;
using NasBlockExistAction =
    NotAwesomeSurvival.Action<NotAwesomeSurvival.NasPlayer,
    NotAwesomeSurvival.NasBlock, bool, ushort, ushort, ushort>;
using NasBlockInteraction =
    NotAwesomeSurvival.Action<NotAwesomeSurvival.NasPlayer, MCGalaxy.Events.PlayerEvents.MouseButton, MCGalaxy.Events.PlayerEvents.MouseAction,
    NotAwesomeSurvival.NasBlock, ushort, ushort, ushort>;
namespace NotAwesomeSurvival
{
    public partial class NasBlock
    {
        public static NasBlock[] blocks = new NasBlock[768], blocksIndexedByServerushort;
        public static NasBlock Default;
        public static int[] DefaultDurabilities = new int[(int)Material.Count];
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
        public ushort selfID,
            parentID,
            alternateID;
        public List<ushort> childIDs = null;
        public Material material;
        public int tierOfToolNeededToBreak,
            durability,
            resourceCost,
            expGivenMax = 0,
            expGivenMin = 0;
        public Type type;
        public float damageDoneToTool,
            fallDamageMultiplier = -1,
            disturbDelayMax = 0f,
            disturbDelayMin = 0f,
            beginDelayMax = 0f,
            beginDelayMin = 0f;
        public Func<NasPlayer, ushort, Drop> dropHandler;
        public Crafting.Station station;
        public Container container;
        public bool collides = true;
        public AABB bounds;
        public NasBlockAction disturbedAction = null,
            instantAction = null;
        public NasBlockInteraction interaction = null;
        public NasBlockExistAction existAction = null;
        public NasBlockCollideAction collideAction = null;
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
                blocks[parent.parentID].childIDs = new();
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
                station = new(parent.station);
            }
            if (parent.container != null)
            {
                container = new(parent.container);
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
        public static NasBlock Get(ushort clientushort)
        {
            return blocks[clientushort] ?? Default;
        }
        public string GetName(NasPlayer np, ushort id = ushort.MaxValue)
        {
            if (id == ushort.MaxValue)
            {
                id = parentID;
            }
            string name;
            ushort block = Nas.FromRaw(id);
            if (Nas.IsPhysicsType(block))
            {
                name = "Physics block";
            }
            else
            {
                BlockDefinition def;
                if (!np.p.IsSuper)
                {
                    def = np.p.level.GetBlockDef(block);
                    def ??= BlockDefinition.GlobalDefs[block];
                }
                else
                {
                    def = BlockDefinition.GlobalDefs[block];
                }
                if (def != null)
                {
                    name = def.Name;
                }
                else
                {
                    name = "Unknown";
                }
            }
            return name.Split('-')[0];
        }
        public static Drop DefaultDropHandler(NasPlayer np, ushort id)
        {
            return new(id);
        }
    }
}
#endif