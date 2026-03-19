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
using MCGalaxy.Blocks;
namespace MCGalaxy
{
    public static partial class Block
    {
        public static BlockProps[] Props = new BlockProps[1024];
        internal static BlockProps MakeDefaultProps(ushort b)
        {
            BlockProps props = BlockProps.MakeEmpty();
            switch (b)
            {
                case >= Op_Glass and <= Op_Lava:
                case Invalid:
                case RocketStart:
                case Bedrock:
                    props.OPBlock = true;
                    break;
            }
            switch (b)
            {
                case >= tDoor_Log and <= tDoor_Green:
                case >= tDoor_TNT and <= tDoor_Lava:
                    props.IsTDoor = true;
                    break;
            }
            if (b >= MB_White && b <= MB_Lava)
                props.IsMessageBlock = true;
            switch (b)
            {
                case Portal_Blue:
                case Portal_Orange:
                case >= Portal_Air and <= Portal_Lava:
                    props.IsPortal = true;
                    break;
            }
            if (b >= oDoor_Log && b <= oDoor_Wood)
                props.oDoorBlock = (ushort)(oDoor_Log_air + (b - oDoor_Log));
            if (b >= oDoor_Green && b <= oDoor_Water)
                props.oDoorBlock = (ushort)(oDoor_Green_air + (b - oDoor_Green));
            if (b >= oDoor_Log_air && b <= oDoor_Wood_air)
                props.oDoorBlock = (ushort)(oDoor_Log + (b - oDoor_Log_air));
            if (b >= oDoor_Green_air && b <= oDoor_Water_air)
                props.oDoorBlock = (ushort)(oDoor_Green + (b - oDoor_Green_air));
            props.LavaKills = b == Wood || b == Log
                || b == Sponge || b == Bookshelf || b == Leaves || b == Crate;
            switch (b)
            {
                case >= Red and <= White:
                case >= LightPink and <= Turquoise:
                    props.LavaKills = true;
                    break;
            }
            switch (b)
            {
                case Air:
                case Sapling:
                case >= Dandelion and <= RedMushroom:
                    props.LavaKills = true;
                    props.WaterKills = true;
                    break;
            }
            props.IsDoor = IsDoor(b);
            props.AnimalAI = GetAI(b);
            props.IsRails = b == Red || b == Op_Air;
            props.Drownable = b >= Water && b <= StillLava;
            switch (b)
            {
                case Water:
                case StillWater:
                    props.DeathMessage = "@p &S&cdrowned.";
                    break;
            }
            switch (b)
            {
                case Lava:
                case StillLava:
                    props.DeathMessage = "@p &Sburnt to a &ccrisp.";
                    break;
            }
            if (b == Air) props.DeathMessage = "@p &Shit the floor &chard.";
            string deathMsg = GetDeathMessage(b);
            if (deathMsg != null)
            {
                props.DeathMessage = deathMsg;
                props.KillerBlock = true;
            }
            if (b == Slab) props.StackBlock = DoubleSlab;
            if (b == CobblestoneSlab) props.StackBlock = Cobblestone;
            if (b == Dirt) props.GrassBlock = Grass;
            if (b == Grass) props.DirtBlock = Dirt;
            return props;
        }
        static bool IsDoor(ushort b) => b switch
        {
            >= Door_Obsidian and <= Door_Slab => true,
            >= Door_Iron and <= Door_Bookshelf => true,
            >= Door_Orange and <= Door_White => true,
            >= Door_Air and <= Door_Lava => true,
            _ => b == Door_Cobblestone || b == Door_Red || b == Door_Log || b == Door_Gold,
        };
        static AnimalAI GetAI(ushort b) => b switch
        {
            Bird_Black or Bird_White or Bird_Lava or Bird_Water => AnimalAI.Fly,
            Bird_Red or Bird_Blue or Bird_Killer => AnimalAI.KillerAir,
            Fish_Betta or Fish_Shark => AnimalAI.KillerWater,
            Fish_LavaShark => AnimalAI.KillerLava,
            Fish_Gold or Fish_Salmon or Fish_Sponge => AnimalAI.FleeWater,
            _ => AnimalAI.None,
        };
        static string GetDeathMessage(ushort b) => b switch
        {
            TNT_Explosion => "@p &S&cblew into pieces.",
            Deadly_Air => "@p &Swalked into &cnerve gas and suffocated.",
            Deadly_Water or Deadly_ActiveWater => "@p &Sstepped in &dcold water and froze.",
            Deadly_Lava or Deadly_ActiveLava or Deadly_FastLava => "@p &Sstood in &cmagma and melted.",
            Magma => "@p &Swas hit by &cflowing magma and melted.",
            Geyser => "@p &Swas hit by &cboiling water and melted.",
            Bird_Killer => "@p &Swas hit by a &cphoenix and burnt.",
            Train => "@p &Swas hit by a &ctrain.",
            Fish_Shark => "@p &Swas eaten by a &cshark.",
            LavaFire => "@p &Sburnt to a &ccrisp.",
            RocketHead => "@p &Swas &cin a fiery explosion.",
            ZombieBody => "@p &Sdied due to lack of &5brain.",
            Creeper => "@p &Swas killed &cb-SSSSSSSSSSSSSS",
            Fish_LavaShark => "@p &Swas eaten by a ... LAVA SHARK?!",
            Snake => "@p &Swas bit by a deadly snake.",
            _ => null,
        };
    }
}