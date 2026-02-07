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
    public delegate int HandleDelete(Player p, ushort oldBlock, ushort x, ushort y, ushort z);
    /// <summary> Handles the player placing a block at the given coordinates. </summary>
    /// <remarks> Use p.ChangeBlock to do a normal player block change (adds to BlockDB, updates dirt/grass beneath) </remarks>
    public delegate int HandlePlace(Player p, ushort newBlock, ushort x, ushort y, ushort z);
    /// <summary> Returns whether this block handles the player walking through this block at the given coordinates. </summary>
    /// <remarks> If this returns false, continues trying other walkthrough blocks the player is touching. </remarks>
    public delegate bool HandleWalkthrough(Player p, ushort block, ushort x, ushort y, ushort z);
    /// <summary> Called to handle the physics for this particular block. </summary>
    public delegate void HandlePhysics(Level lvl, ref PhysInfo C);
    public static class BlockBehaviour
    {
        /// <summary> Retrieves the default place block handler for the given block. </summary>
        internal static HandlePlace GetPlaceHandler(ushort block, BlockProps[] props)
        {
            switch (block)
            {
                case 74: return PlaceBehaviour.C4;
                case 75: return PlaceBehaviour.C4Det;
            }
            if (props[block].GrassBlock != 0xff) return PlaceBehaviour.DirtGrow;
            if (props[block].DirtBlock != 0xff) return PlaceBehaviour.GrassDie;
            if (props[block].StackBlock != 0) return PlaceBehaviour.Stack;
            return null;
        }
        // NOTE: These static declarations are just to save a few memory allocations
        //  Behind the scenes, 'return XYZ;' is actually compiled into 'return new HandleDelete(XYZ);'
        //  So by declaring a static variable, 'new HandleDelete(XYZ)' is only ever called once
        //   instead of over and over - thereby slightly reducing memory usage by a few KB per Level
        static readonly HandleDelete DB_revert = DeleteBehaviour.RevertDoor;
        static readonly HandleDelete DB_oDoor = DeleteBehaviour.ODoor;
        static readonly HandleDelete DB_Door = DeleteBehaviour.Door;
        /// <summary> Retrieves the default delete block handler for the given block. </summary>
        internal static HandleDelete GetDeleteHandler(ushort block, BlockProps[] props)
        {
            switch (block)
            {
                case 187: return DeleteBehaviour.RocketStart;
                case 189: return DeleteBehaviour.Firework;
                case 75: return DeleteBehaviour.C4Det;
                case 201: return DB_revert;
                case 212: return DB_revert;
                case 211: return DB_revert;
            }
            // NOTE: If this gets changed, make sure to change BlockOptions.cs too
            if (props[block].IsMessageBlock) return DeleteBehaviour.DoMessageBlock;
            if (props[block].IsPortal) return DeleteBehaviour.DoPortal;
            if (props[block].IsTDoor) return DB_revert;
            if (props[block].oDoorBlock != 0xff) return DB_oDoor;
            if (props[block].IsDoor) return DB_Door;
            return null;
        }
        /// <summary> Retrieves the default walkthrough block handler for the given block. </summary>
        internal static HandleWalkthrough GetWalkthroughHandler(ushort block, BlockProps[] props, bool nonSolid)
        {
            switch (block)
            {
                case 197: return WalkthroughBehaviour.Checkpoint;
                case 165: return WalkthroughBehaviour.Door;
                case 166: return WalkthroughBehaviour.Door;
                case 167: return WalkthroughBehaviour.Door;
                case 230: return WalkthroughBehaviour.Train;
            }
            if (props[block].IsMessageBlock && nonSolid) return WalkthroughBehaviour.DoMessageBlock;
            if (props[block].IsPortal && nonSolid) return WalkthroughBehaviour.DoPortal;
            return null;
        }
        // See comments noted above for reasoning behind static declaration of some HandleDelete handlers
        static readonly HandlePhysics PH_do_Door = DoorPhysics.Do;
        static readonly HandlePhysics PH_do_oDoor = DoorPhysics.ODoor;
        static readonly HandlePhysics PH_do_Other = OtherPhysics.DoOther;
        /// <summary> Retrieves the default physics block handler for the given block. </summary>
        internal static HandlePhysics GetPhysicsHandler(ushort block, BlockProps[] props)
        {
            switch (block)
            {
                case 201: return PH_do_Door;
                case 212: return PH_do_Door;
                case 211: return PH_do_Door;
                case 252: return SnakePhysics.DoTail;
                case 251: return SnakePhysics.Do;
                case 188: return RocketPhysics.Do;
                case 189: return FireworkPhysics.Do;
                case 232: return ZombiePhysics.Do;
                case 233: return ZombiePhysics.DoHead;
                case 231: return ZombiePhysics.Do;
                case 8: return SimpleLiquidPhysics.DoWater;
                case 193: return SimpleLiquidPhysics.DoWater;
                case 10: return SimpleLiquidPhysics.DoLava;
                case 194: return SimpleLiquidPhysics.DoLava;
                case 140: return ExtLiquidPhysics.DoWaterfall;
                case 141: return ExtLiquidPhysics.DoLavafall;
                case 143:
                    return (Level lvl, ref PhysInfo C) =>
                    ExtLiquidPhysics.DoFaucet(lvl, ref C, 140);
                case 144:
                    return (Level lvl, ref PhysInfo C) =>
                    ExtLiquidPhysics.DoFaucet(lvl, ref C, 141);
                case 145: return FinitePhysics.DoWaterOrLava;
                case 146: return FinitePhysics.DoWaterOrLava;
                case 147: return FinitePhysics.DoFaucet;
                case 195: return ExtLiquidPhysics.DoMagma;
                case 196: return ExtLiquidPhysics.DoGeyser;
                case 112: return SimpleLiquidPhysics.DoFastLava;
                case 73: return SimpleLiquidPhysics.DoFastLava;
                case 0: return AirPhysics.DoAir;
                case 18: return LeafPhysics.DoLeaf;
                case 17: return LeafPhysics.DoLog;
                case 6: return OtherPhysics.DoShrub;
                case 54: return FirePhysics.Do;
                case 185: return FirePhysics.Do;
                case 12: return OtherPhysics.DoFalling;
                case 13: return OtherPhysics.DoFalling;
                case 110: return OtherPhysics.DoFloatwood;
                case 19:
                    return (Level lvl, ref PhysInfo C) =>
                    OtherPhysics.DoSponge(lvl, ref C, false);
                case 109:
                    return (Level lvl, ref PhysInfo C) =>
                    OtherPhysics.DoSponge(lvl, ref C, true);
                // Special blocks that are not saved
                case 200:
                    return (Level lvl, ref PhysInfo C) =>
                    AirPhysics.DoFlood(lvl, ref C, 0, 200);
                case 202:
                    return (Level lvl, ref PhysInfo C) =>
                    AirPhysics.DoFlood(lvl, ref C, 1, 202);
                case 203:
                    return (Level lvl, ref PhysInfo C) =>
                    AirPhysics.DoFlood(lvl, ref C, 2, 203);
                case 204:
                    return (Level lvl, ref PhysInfo C) =>
                    AirPhysics.DoFlood(lvl, ref C, 3, 204);
                case 182: return TntPhysics.DoSmallTnt;
                case 183: return TntPhysics.DoBigTnt;
                case 186: return TntPhysics.DoNukeTnt;
                case 184: return TntPhysics.DoTntExplosion;
                case 230: return TrainPhysics.Do;
            }
            HandlePhysics animalAI = AnimalAIHandler(props[block].AnimalAI);
            if (animalAI != null) return animalAI;
            if (props[block].oDoorBlock != 0xff) return PH_do_oDoor;
            if (props[block].GrassBlock != 0xff) return OtherPhysics.DoDirtGrow;
            if (props[block].DirtBlock != 0xff) return OtherPhysics.DoGrassDie;
            // TODO: should this be checking WaterKills/LavaKills
            // Adv physics updating anything placed next to water or lava
            if ((block >= 21 && block <= 40) || block == 5 || block == 17 || block == 47)
            {
                return PH_do_Other;
            }
            return null;
        }
        /// <summary> Retrieves the default physics block handler for the given block. </summary>
        internal static HandlePhysics GetPhysicsDoorsHandler(ushort block, BlockProps[] props)
        {
            if (block == 0) return PH_do_Door;
            if (block == 201) return PH_do_Door;
            if (block == 212) return PH_do_Door;
            if (block == 211) return PH_do_Door;
            if (props[block].oDoorBlock != 0xff) return PH_do_oDoor;
            return null;
        }
        static HandlePhysics AnimalAIHandler(int ai)
        {
            if (ai == 1) return BirdPhysics.Do;
            if (ai == 2)
            {
                return (Level lvl, ref PhysInfo C) => HunterPhysics.Do(lvl, ref C, 0, -1);
            }
            else if (ai == 4)
            {
                return (Level lvl, ref PhysInfo C) => HunterPhysics.Do(lvl, ref C, 8, -1);
            }
            else if (ai == 6)
            {
                return (Level lvl, ref PhysInfo C) => HunterPhysics.Do(lvl, ref C, 10, -1);
            }
            if (ai == 3)
            {
                return (Level lvl, ref PhysInfo C) => HunterPhysics.Do(lvl, ref C, 0, 1);
            }
            else if (ai == 5)
            {
                return (Level lvl, ref PhysInfo C) => HunterPhysics.Do(lvl, ref C, 8, 1);
            }
            else if (ai == 7)
            {
                return (Level lvl, ref PhysInfo C) => HunterPhysics.Do(lvl, ref C, 10, 1);
            }
            return null;
        }
    }
}
