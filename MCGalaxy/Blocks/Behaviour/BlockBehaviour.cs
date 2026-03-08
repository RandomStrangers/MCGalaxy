/*
    Copyright 2015-2024 MCGalaxy
    Dual-licensed under the Educational Community License, Version 2.0 and
    the GNU General Public License, Version 3 (the "Licenses"); you may
    not use this file except in compliance with the Licenses. You may
    obtain a copy of the Licenses at
    https://opensource.org/license/ecl-2-0/
    https://www.gnu.org/licenses/gpl-3.0.html
    Unless required by applicable law or agreed to in writing,
    software distributed under the Licenses are distributed on an "AS IS"
    BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express
    or implied. See the Licenses for the specific language governing
    permissions and limitations under the Licenses.
 */
using MCGalaxy.Blocks.Physics;
namespace MCGalaxy.Blocks
{
    /// <summary> Handles the player deleting a block at the given coordinates. </summary>
    /// <remarks> Use p.ChangeBlock to do a normal player block change (adds to BlockDB, updates dirt/grass beneath) </remarks>
    public delegate ChangeResult HandleDelete(Player p, ushort oldBlock, ushort x, ushort y, ushort z);
    /// <summary> Handles the player placing a block at the given coordinates. </summary>
    /// <remarks> Use p.ChangeBlock to do a normal player block change (adds to BlockDB, updates dirt/grass beneath) </remarks>
    public delegate ChangeResult HandlePlace(Player p, ushort newBlock, ushort x, ushort y, ushort z);
    /// <summary> Returns whether this block handles the player walking through this block at the given coordinates. </summary>
    /// <remarks> If this returns false, continues trying other walkthrough blocks the player is touching. </remarks>
    public delegate bool HandleWalkthrough(Player p, ushort block, ushort x, ushort y, ushort z);
    /// <summary> Called to handle the physics for this particular block. </summary>
    public delegate void HandlePhysics(Level lvl, ref PhysInfo C);
    public static class BlockBehaviour
    {
        /// <summary> Retrieves the default place block handler for the given block. </summary>
        internal static HandlePlace GetPlaceHandler(ushort block, BlockProps[] props) => block switch
        {
            Block.C4 => PlaceBehaviour.C4,
            Block.C4Detonator => PlaceBehaviour.C4Det,
            _ => props[block].GrassBlock != Block.Invalid
                            ? PlaceBehaviour.DirtGrow
                            : props[block].DirtBlock != Block.Invalid
                            ? PlaceBehaviour.GrassDie
                            : props[block].StackBlock != Block.Air ? PlaceBehaviour.Stack : null,
        };
        static readonly HandleDelete DB_revert = DeleteBehaviour.RevertDoor,
            DB_oDoor = DeleteBehaviour.ODoor,
            DB_Door = DeleteBehaviour.Door;
        /// <summary> Retrieves the default delete block handler for the given block. </summary>
        internal static HandleDelete GetDeleteHandler(ushort block, BlockProps[] props)
        {
            switch (block)
            {
                case Block.RocketStart:
                    return DeleteBehaviour.RocketStart;
                case Block.Fireworks: 
                    return DeleteBehaviour.Firework;
                case Block.C4Detonator: 
                    return DeleteBehaviour.C4Det;
                case Block.Door_Log_air:
                    return DB_revert;
                case Block.Door_TNT_air: 
                    return DB_revert;
                case Block.Door_Green_air:
                    return DB_revert;
            }
            if (props[block].IsMessageBlock) return DeleteBehaviour.DoMessageBlock;
            return props[block].IsPortal
                ? DeleteBehaviour.DoPortal
                : props[block].IsTDoor ? DB_revert : props[block].oDoorBlock != Block.Invalid ? DB_oDoor : props[block].IsDoor ? DB_Door : null;
        }
        /// <summary> Retrieves the default walkthrough block handler for the given block. </summary>
        internal static HandleWalkthrough GetWalkthroughHandler(ushort block, BlockProps[] props, bool nonSolid) => block switch
        {
            Block.Checkpoint => WalkthroughBehaviour.Checkpoint,
            Block.Door_AirActivatable => WalkthroughBehaviour.Door,
            Block.Door_Water => WalkthroughBehaviour.Door,
            Block.Door_Lava => WalkthroughBehaviour.Door,
            Block.Train => WalkthroughBehaviour.Train,
            _ => props[block].IsMessageBlock && nonSolid
                            ? WalkthroughBehaviour.DoMessageBlock
                            : props[block].IsPortal && nonSolid ? WalkthroughBehaviour.DoPortal : null,
        };
        static readonly HandlePhysics PH_do_Door = DoorPhysics.Do,
            PH_do_oDoor = DoorPhysics.ODoor,
            PH_do_Other = OtherPhysics.DoOther;
        /// <summary> Retrieves the default physics block handler for the given block. </summary>
        internal static HandlePhysics GetPhysicsHandler(ushort block, BlockProps[] props)
        {
            switch (block)
            {
                case Block.Door_Log_air:
                    return PH_do_Door;
                case Block.Door_TNT_air:
                    return PH_do_Door;
                case Block.Door_Green_air:
                    return PH_do_Door;
                case Block.SnakeTail:
                    return SnakePhysics.DoTail;
                case Block.Snake:
                    return SnakePhysics.Do;
                case Block.RocketHead:
                    return RocketPhysics.Do;
                case Block.Fireworks:
                    return FireworkPhysics.Do;
                case Block.ZombieBody:
                    return ZombiePhysics.Do;
                case Block.ZombieHead:
                    return ZombiePhysics.DoHead;
                case Block.Creeper:
                    return ZombiePhysics.Do;
                case Block.Water:
                    return SimpleLiquidPhysics.DoWater;
                case Block.Deadly_ActiveWater:
                    return SimpleLiquidPhysics.DoWater;
                case Block.Lava:
                    return SimpleLiquidPhysics.DoLava;
                case Block.Deadly_ActiveLava:
                    return SimpleLiquidPhysics.DoLava;
                case Block.WaterDown:
                    return ExtLiquidPhysics.DoWaterfall;
                case Block.LavaDown: 
                    return ExtLiquidPhysics.DoLavafall;
                case Block.WaterFaucet:
                    return (Level lvl, ref PhysInfo C) =>
                    ExtLiquidPhysics.DoFaucet(lvl, ref C, Block.WaterDown);
                case Block.LavaFaucet:
                    return (Level lvl, ref PhysInfo C) => ExtLiquidPhysics.DoFaucet(lvl, ref C, Block.LavaDown);
                case Block.FiniteWater:
                    return FinitePhysics.DoWaterOrLava;
                case Block.FiniteLava:
                    return FinitePhysics.DoWaterOrLava;
                case Block.FiniteFaucet:
                    return FinitePhysics.DoFaucet;
                case Block.Magma:
                    return ExtLiquidPhysics.DoMagma;
                case Block.Geyser:
                    return ExtLiquidPhysics.DoGeyser;
                case Block.FastLava: 
                    return SimpleLiquidPhysics.DoFastLava;
                case Block.Deadly_FastLava: 
                    return SimpleLiquidPhysics.DoFastLava;
                case Block.Air: 
                    return AirPhysics.DoAir;
                case Block.Leaves:
                    return LeafPhysics.DoLeaf;
                case Block.Log: 
                    return LeafPhysics.DoLog;
                case Block.Sapling:
                    return OtherPhysics.DoShrub;
                case Block.Fire:
                    return FirePhysics.Do;
                case Block.LavaFire:
                    return FirePhysics.Do;
                case Block.Sand:
                    return OtherPhysics.DoFalling;
                case Block.Gravel: 
                    return OtherPhysics.DoFalling;
                case Block.FloatWood: 
                    return OtherPhysics.DoFloatwood;
                case Block.Sponge:
                    return (Level lvl, ref PhysInfo C) => OtherPhysics.DoSponge(lvl, ref C, false);
                case Block.LavaSponge:
                    return (Level lvl, ref PhysInfo C) => OtherPhysics.DoSponge(lvl, ref C, true);
                case Block.Air_Flood:
                    return (Level lvl, ref PhysInfo C) => AirPhysics.DoFlood(lvl, ref C, AirFlood.Full, Block.Air_Flood);
                case Block.Air_FloodLayer:
                    return (Level lvl, ref PhysInfo C) => AirPhysics.DoFlood(lvl, ref C, AirFlood.Layer, Block.Air_FloodLayer);
                case Block.Air_FloodDown:
                    return (Level lvl, ref PhysInfo C) => AirPhysics.DoFlood(lvl, ref C, AirFlood.Down, Block.Air_FloodDown);
                case Block.Air_FloodUp:
                    return (Level lvl, ref PhysInfo C) => AirPhysics.DoFlood(lvl, ref C, AirFlood.Up, Block.Air_FloodUp);
                case Block.TNT_Small:
                    return TntPhysics.DoSmallTnt;
                case Block.TNT_Big: 
                    return TntPhysics.DoBigTnt;
                case Block.TNT_Nuke:
                    return TntPhysics.DoNukeTnt;
                case Block.TNT_Explosion:
                    return TntPhysics.DoTntExplosion;
                case Block.Train: 
                    return TrainPhysics.Do;
            }
            HandlePhysics animalAI = AnimalAIHandler(props[block].AnimalAI);
            if (animalAI != null) return animalAI;
            if (props[block].oDoorBlock != Block.Invalid) return PH_do_oDoor;
            if (props[block].GrassBlock != Block.Invalid) return OtherPhysics.DoDirtGrow;
            if (props[block].DirtBlock != Block.Invalid) return OtherPhysics.DoGrassDie;
            return (block >= Block.Red && block <= Block.RedMushroom) || block == Block.Wood || block == Block.Log || block == Block.Bookshelf
                ? PH_do_Other
                : null;
        }
        /// <summary> Retrieves the default physics block handler for the given block. </summary>
        internal static HandlePhysics GetPhysicsDoorsHandler(ushort block, BlockProps[] props) => block switch
        {
            Block.Air => PH_do_Door,
            _ => block == Block.Door_Log_air
            ? PH_do_Door
            : block == Block.Door_TNT_air
            ? PH_do_Door
            : block == Block.Door_Green_air ? PH_do_Door : props[block].oDoorBlock != Block.Invalid ? PH_do_oDoor : null
        };
        static HandlePhysics AnimalAIHandler(AnimalAI ai)
        {
            if (ai == AnimalAI.Fly) return BirdPhysics.Do;
            if (ai == AnimalAI.FleeAir)
            {
                return (Level lvl, ref PhysInfo C) => HunterPhysics.Do(lvl, ref C, Block.Air, -1);
            }
            else if (ai == AnimalAI.FleeWater)
            {
                return (Level lvl, ref PhysInfo C) => HunterPhysics.Do(lvl, ref C, Block.Water, -1);
            }
            else if (ai == AnimalAI.FleeLava)
            {
                return (Level lvl, ref PhysInfo C) => HunterPhysics.Do(lvl, ref C, Block.Lava, -1);
            }
            if (ai == AnimalAI.KillerAir)
            {
                return (Level lvl, ref PhysInfo C) => HunterPhysics.Do(lvl, ref C, Block.Air, 1);
            }
            else if (ai == AnimalAI.KillerWater)
            {
                return (Level lvl, ref PhysInfo C) => HunterPhysics.Do(lvl, ref C, Block.Water, 1);
            }
            else if (ai == AnimalAI.KillerLava)
            {
                return (Level lvl, ref PhysInfo C) => HunterPhysics.Do(lvl, ref C, Block.Lava, 1);
            }
            return null;
        }
    }
}
