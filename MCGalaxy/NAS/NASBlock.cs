using MCGalaxy.Maths;
using System;
using System.Collections.Generic;
using NASBlockAction = MCGalaxy.NASAction<MCGalaxy.NASLevel, MCGalaxy.NASBlock, int, int, int>;
using NASBlockCollideAction =
    MCGalaxy.NASAction<MCGalaxy.NASEntity,
    MCGalaxy.NASBlock, bool, ushort, ushort, ushort>;
using NASBlockExistAction =
    MCGalaxy.NASAction<MCGalaxy.NASPlayer,
    MCGalaxy.NASBlock, bool, ushort, ushort, ushort>;
using NASBlockInteraction =
    MCGalaxy.NASAction<MCGalaxy.NASPlayer, MCGalaxy.Events.PlayerEvents.MouseButton, MCGalaxy.Events.PlayerEvents.MouseAction,
    MCGalaxy.NASBlock, ushort, ushort, ushort>;
namespace MCGalaxy
{
    public enum NASMaterial
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
    public partial class NASBlock
    {
        public static NASBlock[] blocks = new NASBlock[768], blocksIndexedByServerushort;
        public static NASBlock Default;
        public static int[] DefaultDurabilities = new int[(int)NASMaterial.Count];
        public ushort selfID,
            parentID,
            alternateID;
        public List<ushort> childIDs = null;
        public NASMaterial material;
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
        public NASFunc<NASPlayer, ushort, NASDrop> dropHandler;
        public NASStation station;
        public NASContainer container;
        public bool collides = true;
        public AABB bounds;
        public NASBlockAction disturbedAction = null,
            instantAction = null;
        public NASBlockInteraction interaction = null;
        public NASBlockExistAction existAction = null;
        public NASBlockCollideAction collideAction = null;
        public NASBlock(ushort id, NASMaterial mat)
        {
            selfID = id;
            parentID = id;
            alternateID = id;
            material = mat;
            tierOfToolNeededToBreak = 0;
            durability = DefaultDurabilities[(int)mat];
            damageDoneToTool = 1f;
            if (material == NASMaterial.Leaves || durability == 0)
            {
                damageDoneToTool = 0;
            }
            dropHandler = DefaultDropHandler;
            resourceCost = 1;
            station = null;
        }
        public NASBlock(ushort id, NASMaterial mat, int dur, int tierOfToolNeededToBreak = 0) : this(id, mat)
        {
            durability = dur;
            this.tierOfToolNeededToBreak = tierOfToolNeededToBreak;
        }
        public NASBlock(ushort id, NASBlock parent)
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
        public static NASBlock Get(ushort clientushort) => blocks[clientushort] ?? Default;
        public string GetName(NASPlayer np, ushort id = ushort.MaxValue)
        {
            if (id == ushort.MaxValue)
            {
                id = parentID;
            }
            string name;
            ushort block = NASPlugin.FromRaw(id);
            if (block >= 66 && block < 256)
            {
                name = "Physics block";
            }
            else
            {
                BlockDefinition def;
                if (!np.p.IsSuper)
                {
                    def = np.p.Level.GetBlockDef(block);
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
        public static NASDrop DefaultDropHandler(NASPlayer np, ushort id) => new(id);
    }
}
