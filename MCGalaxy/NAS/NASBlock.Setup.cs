using System;
namespace MCGalaxy
{
    public partial class NASBlock
    {
        public static Random r = new();
        public static void Setup()
        {
            DefaultDurabilities[(int)NASMaterial.None] = 1;
            DefaultDurabilities[(int)NASMaterial.Gas] = 0;
            DefaultDurabilities[(int)NASMaterial.Stone] = 16;
            DefaultDurabilities[(int)NASMaterial.Earth] = 5;
            DefaultDurabilities[(int)NASMaterial.Wood] = 8;
            DefaultDurabilities[(int)NASMaterial.Plant] = 1;
            DefaultDurabilities[(int)NASMaterial.Leaves] = 3;
            DefaultDurabilities[(int)NASMaterial.Organic] = 5;
            DefaultDurabilities[(int)NASMaterial.Glass] = 3;
            DefaultDurabilities[(int)NASMaterial.Metal] = 32;
            Default = new(0, NASMaterial.Earth)
            {
                collideAction = AirCollideAction()
            };
            float fallSpeed = 0.325f,
                waterDisturbDelayMin = 0.5f,
                waterDisturbDelayMax = 0.5f,
                pieRestore = 2.5f,
                breadRestore = 1f,
                sugarTotalSeconds = 10f * 60f,
                sugarMaxAddedSeconds = 15f * 60f,
                sugarGrowMin = sugarTotalSeconds / 2f,
                sugarGrowMax = (sugarTotalSeconds + sugarMaxAddedSeconds) / 2f,
                wheatTotalSeconds = 20f * 60f,
                wheatMaxAddedSeconds = 20f * 60f,
                wheatGrowMin = wheatTotalSeconds / 3f,
                wheatGrowMax = (wheatTotalSeconds + wheatMaxAddedSeconds) / 3f,
                leafShrivelDelayMin = 0.2f,
                leafShrivelDelayMax = 0.4f,
                grassDelayMin = 10,
                grassDelayMax = 60,
                lavaDisturbDelayMin = 1.5f,
                lavaDisturbDelayMax = 1.5f,
                treeDelayMin = 30f,
                treeDelayMax = 60f;
            int stonebrickDurMulti = 2;
            ushort[] set = new ushort[]
            {
                NASPlugin.FromRaw(648)
            },
            set2 = new ushort[]
            {
                NASPlugin.FromRaw(702)
            },
            set3 = new ushort[]
            {
                NASPlugin.FromRaw(478)
            },
            set4 = new ushort[]
            {
                NASPlugin.FromRaw(604)
            };
            ushort i;
            i = 8;
            blocks[i] = new(i, NASMaterial.Liquid, int.MaxValue)
            {
                disturbDelayMin = 1f,
                disturbDelayMax = 5f,
                disturbedAction = FloodAction(waterSet),
                collideAction = LiquidCollideAction()
            };
            i = 643;
            blocks[i] = new(i, NASMaterial.Wood, DefaultDurabilities[(int)NASMaterial.Wood] * 2)
            {
                childIDs = new()
            };
            blocks[i].childIDs.Add(9);
            i = 9;
            blocks[i] = new(i, NASMaterial.Liquid, int.MaxValue)
            {
                existAction = WaterExistAction(),
                disturbDelayMin = waterDisturbDelayMin,
                disturbDelayMax = waterDisturbDelayMax,
                disturbedAction = LimitedFloodAction(waterSet, 1),
                collideAction = LiquidCollideAction(),
                parentID = 643
            };
            i = 632;
            blocks[i] = new(i, NASMaterial.Liquid, int.MaxValue)
            {
                disturbDelayMin = waterDisturbDelayMin,
                disturbDelayMax = waterDisturbDelayMax,
                disturbedAction = LimitedFloodAction(waterSet, 3),
                collideAction = LiquidCollideAction()
            };
            i++;
            blocks[i] = new(i, NASMaterial.Liquid, int.MaxValue)
            {
                disturbDelayMin = waterDisturbDelayMin,
                disturbDelayMax = waterDisturbDelayMax,
                disturbedAction = LimitedFloodAction(waterSet, 4),
                collideAction = LiquidCollideAction()
            };
            i++;
            blocks[i] = new(i, NASMaterial.Liquid, int.MaxValue)
            {
                disturbDelayMin = waterDisturbDelayMin,
                disturbDelayMax = waterDisturbDelayMax,
                disturbedAction = LimitedFloodAction(waterSet, 5),
                collideAction = LiquidCollideAction()
            };
            i++;
            blocks[i] = new(i, NASMaterial.Liquid, int.MaxValue)
            {
                disturbDelayMin = waterDisturbDelayMin,
                disturbDelayMax = waterDisturbDelayMax,
                disturbedAction = LimitedFloodAction(waterSet, 6),
                collideAction = LiquidCollideAction()
            };
            i++;
            blocks[i] = new(i, NASMaterial.Liquid, int.MaxValue)
            {
                disturbDelayMin = waterDisturbDelayMin,
                disturbDelayMax = waterDisturbDelayMax,
                disturbedAction = LimitedFloodAction(waterSet, 7),
                collideAction = LiquidCollideAction()
            };
            i++;
            blocks[i] = new(i, NASMaterial.Liquid, int.MaxValue)
            {
                disturbDelayMin = waterDisturbDelayMin,
                disturbDelayMax = waterDisturbDelayMax,
                disturbedAction = LimitedFloodAction(waterSet, 8),
                collideAction = LiquidCollideAction()
            };
            i++;
            blocks[i] = new(i, NASMaterial.Liquid, int.MaxValue)
            {
                disturbDelayMin = waterDisturbDelayMin,
                disturbDelayMax = waterDisturbDelayMax,
                disturbedAction = LimitedFloodAction(waterSet, 9),
                collideAction = LiquidCollideAction()
            };
            i = 639;
            blocks[i] = new(i, NASMaterial.Liquid, int.MaxValue)
            {
                disturbDelayMin = fallSpeed,
                disturbDelayMax = fallSpeed,
                disturbedAction = LimitedFloodAction(waterSet, 2),
                collideAction = LiquidCollideAction()
            };
            i = 10;
            blocks[i] = new(i, NASMaterial.Liquid, int.MaxValue)
            {
                existAction = LavaExistAction(),
                disturbDelayMin = lavaDisturbDelayMin,
                disturbDelayMax = lavaDisturbDelayMax,
                disturbedAction = LimitedFloodAction(lavaSet, 1),
                collideAction = LavaCollideAction(),
                parentID = 696
            };
            i = 691;
            blocks[i] = new(i, NASMaterial.Liquid, int.MaxValue)
            {
                disturbDelayMin = lavaDisturbDelayMin,
                disturbDelayMax = lavaDisturbDelayMax,
                disturbedAction = LimitedFloodAction(lavaSet, 3),
                collideAction = LavaCollideAction()
            };
            i++;
            blocks[i] = new(i, NASMaterial.Liquid, int.MaxValue)
            {
                disturbDelayMin = lavaDisturbDelayMin,
                disturbDelayMax = lavaDisturbDelayMax,
                disturbedAction = LimitedFloodAction(lavaSet, 4),
                collideAction = LavaCollideAction()
            };
            i++;
            blocks[i] = new(i, NASMaterial.Liquid, int.MaxValue)
            {
                disturbDelayMin = lavaDisturbDelayMin,
                disturbDelayMax = lavaDisturbDelayMax,
                disturbedAction = LimitedFloodAction(lavaSet, 5),
                collideAction = LavaCollideAction()
            };
            i++;
            blocks[i] = new(i, NASMaterial.Liquid, int.MaxValue)
            {
                disturbDelayMin = lavaDisturbDelayMin,
                disturbDelayMax = lavaDisturbDelayMax,
                disturbedAction = LimitedFloodAction(lavaSet, 6),
                collideAction = LavaCollideAction()
            };
            i = 695;
            blocks[i] = new(i, NASMaterial.Liquid, int.MaxValue)
            {
                disturbDelayMin = lavaDisturbDelayMin,
                disturbDelayMax = lavaDisturbDelayMax,
                disturbedAction = LimitedFloodAction(lavaSet, 2),
                collideAction = LavaCollideAction()
            };
            i = 11;
            blocks[i] = new(i, NASMaterial.Liquid, int.MaxValue)
            {
                collideAction = LavaCollideAction(),
                disturbedAction = FloodAction(new ushort[] { 11 })
            };
            i = 1;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Stone], 1);
            i = 596;
            blocks[i] = new(i++, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Stone], 1);
            blocks[i] = new(i, blocks[596]);
            i = 598;
            blocks[i] = new(i++, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Stone], 1);
            blocks[i] = new(i++, blocks[598]);
            blocks[i] = new(i++, blocks[598]);
            blocks[i] = new(i++, blocks[598]);
            i = 70;
            blocks[i] = new(i++, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Stone], 1);
            blocks[i] = new(i++, blocks[70]);
            blocks[i] = new(i++, blocks[70]);
            blocks[i] = new(i++, blocks[70]);
            i = 579;
            blocks[i] = new(i++, blocks[70]);
            blocks[i] = new(i++, blocks[70]);
            blocks[i] = new(i++, blocks[70]);
            blocks[i] = new(i++, blocks[70]);
            i = 162;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Stone], 1);
            i = 181;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Stone], 1);
            i = 163;
            blocks[i] = new(i++, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Stone], 1);
            blocks[i] = new(i, blocks[163]);
            i = 64;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Stone] * stonebrickDurMulti, 1);
            i = 65;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Stone] * stonebrickDurMulti, 1);
            i = 180;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Stone] * stonebrickDurMulti, 1);
            i = 86;
            blocks[i] = new(i++, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Stone] * stonebrickDurMulti, 1);
            blocks[i] = new(i++, blocks[86]);
            i = 75;
            blocks[i] = new(i++, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Stone], 1);
            blocks[i] = new(i++, blocks[75]);
            blocks[i] = new(i++, blocks[75]);
            i = 278;
            blocks[i] = new(i++, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Stone] * stonebrickDurMulti, 1);
            blocks[i] = new(i++, blocks[278]);
            blocks[i] = new(i++, blocks[278]);
            blocks[i] = new(i++, blocks[278]);
            i = 477;
            blocks[i] = new(i++, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Stone] * stonebrickDurMulti, 1);
            i = 211;
            blocks[i] = new(i++, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Stone], 1);
            blocks[i] = new(i++, blocks[211]);
            blocks[i] = new(i++, blocks[211]);
            i = 214;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Stone] / 2, 0);
            i = 194;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Stone] / 2, 0);
            i = 236;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Stone] / 2, 0);
            i = 48;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Stone] / 2, 1);
            i = 155;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Stone], 1);
            i = 157;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Stone], 1);
            i = 156;
            blocks[i] = new(i, blocks[157]);
            i = 452;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Stone], 1)
            {
                alternateID = 1
            };
            i = 458;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Stone], 1);
            i = 460;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Stone], 1);
            i--;
            blocks[i] = new(i, blocks[460]);
            i = 466;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Stone], 1);
            i = 468;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Stone], 1);
            i--;
            blocks[i] = new(i, blocks[468]);
            i = 469;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Stone], 1);
            i = 474;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Stone], 1);
            i = 475;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Stone], 1);
            i = 2;
            blocks[i] = new(i, NASMaterial.Earth)
            {
                disturbDelayMin = grassDelayMin,
                disturbDelayMax = grassDelayMax,
                disturbedAction = GrassBlockAction(2, 3),
                dropHandler = (NasPlayer, dropID) =>
                {
                    NASDrop grassDrop = new()
                    {
                        blockStacks = new()
                    };
                    grassDrop.blockStacks.Add(new(3, 1));
                    return grassDrop;
                }
            };
            i = 129;
            blocks[i] = new(i, NASMaterial.Earth)
            {
                disturbDelayMin = grassDelayMin,
                disturbDelayMax = grassDelayMax,
                disturbedAction = GrassBlockAction(256 | 129, 3),
                interaction = StripInteraction(NASPlugin.FromRaw(547), "Shovel"),
                dropHandler = (NasPlayer, dropID) =>
                {
                    if (NasPlayer.inventory.HeldItem.name == "Shears")
                    {
                        return new(129, 1);
                    }
                    return new(3, 1);
                }
            };
            i = 139;
            blocks[i] = new(i, NASMaterial.Earth)
            {
                disturbedAction = GrassBlockAction(256 | 139, 3),
                interaction = StripInteraction(NASPlugin.FromRaw(547), "Shovel"),
                dropHandler = (NasPlayer, dropID) =>
                {
                    if (NasPlayer.inventory.HeldItem.name == "Shears")
                    {
                        return new(139, 1);
                    }
                    return new(3, 1);
                }
            };
            i = 547;
            blocks[i] = new(i, NASMaterial.Earth)
            {
                disturbDelayMin = 10f,
                disturbDelayMax = 20f,
                disturbedAction = GrassBlockAction(256 | 129, 3),
                dropHandler = (NasPlayer, dropID) =>
                {
                    if (NasPlayer.inventory.HeldItem.name == "Shears")
                    {
                        return new(547, 1);
                    }
                    return new(3, 1);
                }
            };
            i = 3;
            blocks[i] = new(i, NASMaterial.Earth)
            {
                disturbDelayMin = grassDelayMin,
                disturbDelayMax = grassDelayMax,
                interaction = StripInteraction(NASPlugin.FromRaw(547), "Shovel"),
                disturbedAction = DirtBlockAction(grassSet, 3)
            };
            i = 685;
            blocks[i] = new(i, NASMaterial.Earth)
            {
                collideAction = AirCollideAction()
            };
            i = 740;
            blocks[i] = new(i, NASMaterial.Earth)
            {
                collideAction = AirCollideAction()
            };
            i = 4; 
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Stone], 1);
            i = 50;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Stone], 1);
            i = 59;
            blocks[i] = new(i, blocks[50]);
            i = 133;
            blocks[i] = new(i++, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Stone], 1);
            blocks[i] = new(i, blocks[133]);
            i = 98;
            blocks[i] = new(i, NASMaterial.Wood)
            {
                alternateID = 5
            };
            i = 101;
            blocks[i] = new(i, NASMaterial.Wood)
            {
                alternateID = 56
            };
            i = 102;
            blocks[i] = new(i, blocks[101])
            {
                alternateID = 57
            };
            i = 186;
            blocks[i] = new(i++, NASMaterial.Wood);
            blocks[i - 1].alternateID = 182;
            blocks[i] = new(i++, blocks[186]);
            blocks[i - 1].alternateID = 183;
            blocks[i] = new(i++, blocks[186]);
            blocks[i - 1].alternateID = 184;
            blocks[i] = new(i++, blocks[186]);
            blocks[i - 1].alternateID = 185;
            i = 262;
            blocks[i] = new(i++, NASMaterial.Wood);
            blocks[i - 1].alternateID = 66;
            blocks[i] = new(i++, blocks[262]);
            blocks[i - 1].alternateID = 67;
            blocks[i] = new(i++, blocks[262]);
            blocks[i - 1].alternateID = 68;
            blocks[i] = new(i++, blocks[262]);
            blocks[i - 1].alternateID = 69;
            i = 575;
            blocks[i] = new(i++, blocks[262]);
            blocks[i - 1].alternateID = 567;
            blocks[i] = new(i++, blocks[262]);
            blocks[i - 1].alternateID = 568;
            blocks[i] = new(i++, blocks[262]);
            blocks[i - 1].alternateID = 569;
            blocks[i] = new(i++, blocks[262]);
            blocks[i - 1].alternateID = 570;
            i = 255;
            blocks[i] = new(i++, NASMaterial.Wood);
            blocks[i - 1].alternateID = 78;
            blocks[i] = new(i++, blocks[255]);
            blocks[i - 1].alternateID = 79;
            blocks[i] = new(i++, blocks[255]);
            blocks[i - 1].alternateID = 80;
            i = 260;
            blocks[i] = new(i++, NASMaterial.Wood);
            blocks[i] = new(i, blocks[260]);
            i = 243;
            blocks[i] = new(i, NASMaterial.Wood);
            i = 242;
            blocks[i] = new(i, NASMaterial.Wood)
            {
                alternateID = 17,
                interaction = StripInteraction(NASPlugin.FromRaw(546))
            };
            i = 240;
            blocks[i] = new(i, blocks[242])
            {
                alternateID = 15,
                interaction = StripInteraction(NASPlugin.FromRaw(544))
            };
            i = 241;
            blocks[i] = new(i, blocks[242])
            {
                alternateID = 16,
                interaction = StripInteraction(NASPlugin.FromRaw(545))
            };
            i = 546;
            blocks[i] = new(i, NASMaterial.Wood);
            i = 544;
            blocks[i] = new(i, blocks[546]);
            i = 545;
            blocks[i] = new(i, blocks[546]);
            i = 97;
            blocks[i] = new(i, NASMaterial.Wood)
            {
                alternateID = 5
            };
            i = 99;
            blocks[i] = new(i, NASMaterial.Wood)
            {
                alternateID = 56
            };
            i = 100;
            blocks[i] = new(i, blocks[99])
            {
                alternateID = 57
            };
            i = 190;
            blocks[i] = new(i++, NASMaterial.Wood);
            blocks[i - 1].alternateID = 182;
            blocks[i] = new(i++, blocks[190]);
            blocks[i - 1].alternateID = 183;
            blocks[i] = new(i++, blocks[190]);
            blocks[i - 1].alternateID = 184;
            blocks[i] = new(i++, blocks[190]);
            blocks[i - 1].alternateID = 185;
            i = 266;
            blocks[i] = new(i++, NASMaterial.Wood);
            blocks[i - 1].alternateID = 66;
            blocks[i] = new(i++, blocks[266]);
            blocks[i - 1].alternateID = 67;
            blocks[i] = new(i++, blocks[266]);
            blocks[i - 1].alternateID = 68;
            blocks[i] = new(i++, blocks[266]);
            blocks[i - 1].alternateID = 69;
            i = 571;
            blocks[i] = new(i++, blocks[266]);
            blocks[i - 1].alternateID = 567;
            blocks[i] = new(i++, blocks[266]);
            blocks[i - 1].alternateID = 568;
            blocks[i] = new(i++, blocks[266]);
            blocks[i - 1].alternateID = 569;
            blocks[i] = new(i++, blocks[266]);
            blocks[i - 1].alternateID = 570;
            i = 252;
            blocks[i] = new(i++, NASMaterial.Wood);
            blocks[i - 1].alternateID = 78;
            blocks[i] = new(i++, blocks[252]);
            blocks[i - 1].alternateID = 79;
            blocks[i] = new(i++, blocks[252]);
            blocks[i - 1].alternateID = 80;
            i = 258;
            blocks[i] = new(i++, NASMaterial.Wood);
            blocks[i] = new(i, blocks[258]);
            i = 251;
            blocks[i] = new(i, NASMaterial.Wood);
            i = 250;
            blocks[i] = new(i, NASMaterial.Wood)
            {
                alternateID = 17,
                interaction = StripInteraction(NASPlugin.FromRaw(621))
            };
            i = 248;
            blocks[i] = new(i, blocks[250])
            {
                alternateID = 15,
                interaction = StripInteraction(NASPlugin.FromRaw(619))
            };
            i = 249;
            blocks[i] = new(i, blocks[250])
            {
                alternateID = 16,
                interaction = StripInteraction(NASPlugin.FromRaw(620))
            };
            i = 621;
            blocks[i] = new(i, NASMaterial.Wood);
            i = 619;
            blocks[i] = new(i, blocks[621]);
            i = 620;
            blocks[i] = new(i, blocks[621]);
            i = 5;
            blocks[i] = new(i, NASMaterial.Wood);
            i = 56;
            blocks[i] = new(i, NASMaterial.Wood);
            i = 57;
            blocks[i] = new(i, blocks[56]);
            i = 182;
            blocks[i] = new(i++, NASMaterial.Wood);
            blocks[i] = new(i++, blocks[182]);
            blocks[i] = new(i++, blocks[182]);
            blocks[i] = new(i++, blocks[182]);
            i = 66;
            blocks[i] = new(i++, NASMaterial.Wood);
            blocks[i] = new(i++, blocks[66]);
            blocks[i] = new(i++, blocks[66]);
            blocks[i] = new(i++, blocks[66]);
            i = 567;
            blocks[i] = new(i++, blocks[66]);
            blocks[i] = new(i++, blocks[66]);
            blocks[i] = new(i++, blocks[66]);
            blocks[i] = new(i++, blocks[66]);
            i = 78;
            blocks[i] = new(i++, NASMaterial.Wood);
            blocks[i] = new(i++, blocks[78]);
            blocks[i] = new(i++, blocks[78]);
            i = 94;
            blocks[i] = new(i++, NASMaterial.Wood);
            blocks[i] = new(i, blocks[94]);
            i = 168;
            blocks[i] = new(i++, NASMaterial.Wood);
            i = 524;
            blocks[i] = new(i++, NASMaterial.Wood);
            blocks[i] = new(i++, blocks[524]);
            blocks[i] = new(i++, blocks[524]);
            blocks[i] = new(i++, blocks[524]);
            i = 17;
            blocks[i] = new(i, NASMaterial.Wood)
            {
                interaction = StripInteraction(NASPlugin.FromRaw(585))
            };
            i = 15;
            blocks[i] = new(i, blocks[17])
            {
                interaction = StripInteraction(NASPlugin.FromRaw(583))
            };
            i = 16;
            blocks[i] = new(i, blocks[17])
            {
                interaction = StripInteraction(NASPlugin.FromRaw(584))
            };
            i = 585;
            blocks[i] = new(i, NASMaterial.Wood);
            i = 583;
            blocks[i] = new(i, blocks[585]);
            i = 584;
            blocks[i] = new(i, blocks[585]);
            i = 676;
            blocks[i] = new(i, NASMaterial.Wood)
            {
                existAction = SmithingAction(),
                interaction = SmithingTableAction()
            };
            i = 657;
            blocks[i] = new(i, NASMaterial.Wood)
            {
                disturbDelayMin = fallSpeed,
                disturbDelayMax = fallSpeed,
                disturbedAction = FallingBlockAction(NASPlugin.FromRaw(i))
            };
            i = 656;
            blocks[i] = new(i, NASMaterial.Wood)
            {
                disturbDelayMin = 0.75f,
                disturbDelayMax = 0.75f,
                disturbedAction = FallingBlockAction(NASPlugin.FromRaw(i))
            };
            i = 6;
            blocks[i] = new(i, NASMaterial.Plant)
            {
                disturbDelayMin = treeDelayMin,
                disturbDelayMax = treeDelayMax,
                disturbedAction = OakSaplingAction()
            };
            i = 154;
            blocks[i] = new(i, NASMaterial.Plant)
            {
                disturbDelayMin = treeDelayMin,
                disturbDelayMax = treeDelayMax,
                disturbedAction = BirchSaplingAction(),
                alternateID = 6
            };
            i = 450;
            blocks[i] = new(i, NASMaterial.Plant)
            {
                disturbDelayMin = treeDelayMin,
                disturbDelayMax = treeDelayMax,
                disturbedAction = SwampSaplingAction(),
                alternateID = 6
            };
            i = 689;
            blocks[i] = new(i, NASMaterial.Plant)
            {
                disturbDelayMin = treeDelayMin,
                disturbDelayMax = treeDelayMax,
                disturbedAction = SpruceSaplingAction(),
                alternateID = 6
            };
            i = 7;
            blocks[i] = new(i, NASMaterial.Stone, int.MaxValue, 5);
            i = 767;
            blocks[i] = new(i, NASMaterial.Stone, int.MaxValue - 1, 6)
            {
                collideAction = AirCollideAction()
            };
            i = 673;
            blocks[i] = new(i, NASMaterial.Stone, int.MaxValue, 5);
            i = 690;
            blocks[i] = new(i, NASMaterial.Stone, 512, 3);
            i = 457;
            blocks[i] = new(i, NASMaterial.Stone, 512, 3)
            {
                interaction = PortalInteraction()
            };
            i = 659;
            blocks[i] = new(i, NASMaterial.Wood)
            {
                interaction = ChangeInteraction(NASPlugin.FromRaw(658)),
                dropHandler = CustomDrop(659, 1)
            };
            i = 658;
            blocks[i] = new(i, blocks[659])
            {
                interaction = ChangeInteraction(NASPlugin.FromRaw(659)),
                dropHandler = CustomDrop(659, 1)
            };
            i = 660;
            blocks[i] = new(i, blocks[659])
            {
                interaction = ChangeInteraction(NASPlugin.FromRaw(661)),
                dropHandler = CustomDrop(659, 1)
            };
            i = 661;
            blocks[i] = new(i, blocks[659])
            {
                interaction = ChangeInteraction(NASPlugin.FromRaw(660)),
                dropHandler = CustomDrop(659, 1)
            };
            i = 662;
            blocks[i] = new(i, blocks[659])
            {
                interaction = ChangeInteraction(NASPlugin.FromRaw(663)),
                dropHandler = CustomDrop(659, 1)
            };
            i = 663;
            blocks[i] = new(i, blocks[659])
            {
                interaction = ChangeInteraction(NASPlugin.FromRaw(662)),
                dropHandler = CustomDrop(659, 1)
            };
            i = 664;
            blocks[i] = new(i, blocks[659])
            {
                interaction = ChangeInteraction(NASPlugin.FromRaw(665)),
                dropHandler = CustomDrop(659, 1)
            };
            i = 665;
            blocks[i] = new(i, blocks[659])
            {
                interaction = ChangeInteraction(NASPlugin.FromRaw(664)),
                dropHandler = CustomDrop(659, 1)
            };
            i = 12;
            blocks[i] = new(i, NASMaterial.Earth, 3)
            {
                disturbDelayMin = fallSpeed,
                disturbDelayMax = fallSpeed,
                disturbedAction = FallingBlockAction(12)
            };
            i = 451;
            blocks[i] = new(i, NASMaterial.Earth, 3)
            {
                disturbDelayMin = fallSpeed,
                disturbDelayMax = fallSpeed
            };
            i = 13;
            blocks[i] = new(i, NASMaterial.Earth)
            {
                disturbDelayMin = 0.7f,
                disturbDelayMax = 0.7f,
                disturbedAction = FallingBlockAction(13)
            };
            i = 18;
            blocks[i] = new(i, NASMaterial.Leaves)
            {
                disturbedAction = LeafBlockAction(logSet, 18),
                disturbDelayMin = leafShrivelDelayMin,
                disturbDelayMax = leafShrivelDelayMax,
                dropHandler = (NasPlayer, dropID) =>
                {
                    NASDrop drop = new(18, 1);
                    if (r.Next(0, 8) == 0)
                    { 
                        drop.blockStacks.Add(new(6, 1));
                    }
                    else
                    {
                        drop = new(18, 1);
                    }
                    return drop;
                }
            };
            i = 103;
            blocks[i] = new(i, NASMaterial.Leaves)
            {
                disturbedAction = LeafBlockAction(logSet, 256 | 103),
                disturbDelayMin = leafShrivelDelayMin,
                disturbDelayMax = leafShrivelDelayMax,
                dropHandler = (NasPlayer, dropID) =>
                {
                    NASDrop drop = new(103, 1);
                    if (r.Next(0, 8) == 0)
                    {
                        drop.blockStacks.Add(new(154, 1));
                    }
                    else
                    {
                        drop = new(103, 1);
                    }
                    return drop;
                }
            };
            i = 666;
            blocks[i] = new(i, NASMaterial.Leaves)
            {
                disturbedAction = LeafBlockAction(logSet, NASPlugin.FromRaw(i)),
                disturbDelayMin = 1f,
                disturbDelayMax = 1.5f
            };
            i = 686;
            blocks[i] = new(i, NASMaterial.Leaves)
            {
                disturbedAction = LeafBlockAction(logSet, NASPlugin.FromRaw(i)),
                disturbDelayMin = 1f,
                disturbDelayMax = 1.5f
            };
            i = 105;
            blocks[i] = new(i, NASMaterial.Leaves);
            i = 246;
            blocks[i] = new(i, NASMaterial.Leaves);
            i = 19;
            blocks[i] = new(i, NASMaterial.Leaves)
            {
                disturbedAction = LeafBlockAction(logSet, 19),
                disturbDelayMin = leafShrivelDelayMin,
                disturbDelayMax = leafShrivelDelayMax,
                dropHandler = (NasPlayer, dropID) =>
                {
                    NASDrop drop = new(19, 1);
                    if (r.Next(0, 8) == 0)
                    {
                        drop.blockStacks.Add(new(689, 1));
                    }
                    else
                    {
                        drop = new(19, 1);
                    }
                    return drop;
                }
            };
            i = 20;
            blocks[i] = new(i, NASMaterial.Glass);
            i = 136;
            blocks[i] = new(i++, NASMaterial.Glass);
            blocks[i] = new(i, blocks[136]);
            i = 687;
            blocks[i] = new(i, NASMaterial.Glass)
            {
                disturbedAction = LampAction(NASPlugin.FromRaw(688), NASPlugin.FromRaw(687), NASPlugin.FromRaw(687))
            };
            i++;
            blocks[i] = new(i, NASMaterial.Glass)
            {
                disturbedAction = LampAction(NASPlugin.FromRaw(688), NASPlugin.FromRaw(687), NASPlugin.FromRaw(688)),
                dropHandler = CustomDrop(687, 1)
            };
            i = 178;
            blocks[i] = new(i, NASMaterial.Stone)
            {
                disturbedAction = LampAction(NASPlugin.FromRaw(179), NASPlugin.FromRaw(178), NASPlugin.FromRaw(178))
            };
            i = 179;
            blocks[i] = new(i, NASMaterial.Stone)
            {
                disturbedAction = LampAction(NASPlugin.FromRaw(179), NASPlugin.FromRaw(178), NASPlugin.FromRaw(179)),
                collideAction = SpikeCollideAction(),
                dropHandler = CustomDrop(178, 1)
            };
            i = 476;
            blocks[i] = new(i, NASMaterial.Stone, 128, 4)
            {
                collideAction = SpikeCollideAction()
            };
            i = 203;
            blocks[i] = new(i, NASMaterial.Glass);
            i = 209;
            blocks[i] = new(i++, NASMaterial.Glass);
            blocks[i] = new(i, blocks[209]);
            i = 471;
            blocks[i] = new(i, NASMaterial.Glass);
            i = 472;
            blocks[i] = new(i++, NASMaterial.Glass);
            blocks[i] = new(i, blocks[472]);
            i = 37;
            blocks[i] = new(i, NASMaterial.Plant)
            {
                dropHandler = (NasPlayer, dropID) =>
                {
                    if (NasPlayer.inventory.HeldItem.name == "Shears")
                    {
                        return new(37, 1);
                    }
                    else
                    {
                        return (r.Next(0, 2) == 0) ? new(35, 1) : null;
                    }
                },
                disturbedAction = GenericPlantAction()
            };
            i = 38;
            blocks[i] = new(i, NASMaterial.Plant, 1)
            {
                dropHandler = (NasPlayer, dropID) =>
                {
                    if (NasPlayer.inventory.HeldItem.name == "Shears")
                    {
                        return new(38, 1);
                    }
                    else
                    {
                        return (r.Next(0, 2) == 0) ? new(27, 1) : null;
                    }
                },
                disturbedAction = GenericPlantAction()
            };
            i = 39;
            blocks[i] = new(i, NASMaterial.Plant)
            {
                disturbedAction = NeedsSupportAction()
            };
            i = 40;
            blocks[i] = new(i, NASMaterial.Plant)
            {
                disturbedAction = GenericPlantAction()
            };
            i = 130;
            blocks[i] = new(i, NASMaterial.Plant)
            {
                dropHandler = (NasPlayer, dropID) =>
                {
                    if (NasPlayer.inventory.HeldItem.name == "Shears")
                    {
                        return new(130, 1);
                    }
                    else
                    {
                        return (r.Next(0, 8) == 0) ? new(644, 1) : null;
                    }
                },
                disturbedAction = GenericPlantAction()
            };
            i = 41;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Metal], 2);
            i = 42;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Metal], 1);
            i = 631;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Metal], 3);
            i = 650;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Metal], 4);
            i = 148;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Metal], 1);
            i = 208;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Metal], 1);
            i = 149;
            blocks[i] = new(i++, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Metal], 1);
            blocks[i] = new(i, blocks[149]);
            i = 294;
            blocks[i] = new(i++, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Metal], 1);
            blocks[i] = new(i++, blocks[294]);
            blocks[i] = new(i++, blocks[294]);
            blocks[i] = new(i, blocks[294]);
            i = 159;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Metal], 1)
            {
                collideAction = AirCollideAction()
            };
            i++;
            blocks[i] = new(i, blocks[159]);
            i = 161;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Metal], 1)
            {
                collideAction = AirCollideAction()
            };
            i = 44;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Stone] / 2, 1)
            {
                damageDoneToTool = 0.5f
            };
            i = 58;
            blocks[i] = new(i, blocks[44]);
            i = 43;
            blocks[i] = new(i, blocks[44])
            {
                durability = DefaultDurabilities[(int)NASMaterial.Stone],
                dropHandler = (NasPlayer, dropID) =>
                {
                    return new(dropID, 2);
                },
                resourceCost = 2,
                damageDoneToTool = 1f
            };
            i = 45;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Stone], 1);
            i = 282;
            blocks[i] = new(i++, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Stone], 1);
            blocks[i] = new(i++, blocks[282]);
            blocks[i] = new(i++, blocks[282]);
            blocks[i] = new(i++, blocks[282]);
            i = 549;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Stone], 1);
            i = 135;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Stone] / 2, 1);
            i = 270;
            blocks[i] = new(i++, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Stone], 1);
            blocks[i] = new(i++, blocks[270]);
            blocks[i] = new(i++, blocks[270]);
            blocks[i] = new(i, blocks[270]);
            i = 587;
            blocks[i] = new(i++, blocks[270]);
            blocks[i] = new(i++, blocks[270]);
            blocks[i] = new(i++, blocks[270]);
            blocks[i] = new(i, blocks[270]);
            i = 480;
            blocks[i] = new(i++, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Stone], 1);
            blocks[i] = new(i++, blocks[480]);
            blocks[i] = new(i++, blocks[480]);
            blocks[i] = new(i, blocks[480]);
            i = 453;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Stone], 1);
            i = 298;
            blocks[i] = new(i, blocks[453]);
            i = 49;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Stone], 1);
            i = 51;
            blocks[i] = new(i, NASMaterial.Wood);
            i = 52;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Stone], 1);
            i = 299;
            blocks[i] = new(i++, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Stone], 1);
            blocks[i] = new(i, blocks[299]);
            i = 53;
            blocks[i] = new(i, NASMaterial.Earth)
            {
                disturbedAction = NeedsSupportAction()
            };
            i = 140;
            blocks[i] = new(i, NASMaterial.Earth);
            i = 677;
            blocks[i] = new(i, NASMaterial.Organic, 1);
            i = 54;
            blocks[i] = new(i, NASMaterial.None)
            {
                disturbDelayMin = 10f,
                disturbDelayMax = 15f,
                disturbedAction = FireAction(),
                dropHandler = (NasPlayer, dropID) =>
                {
                    return (r.Next(0, 100) == 0) ? new(131, 1) : null;
                },
                collideAction = FireCollideAction()
            };
            i = 55;
            blocks[i] = new(i, NASMaterial.Wood);
            i = 470;
            blocks[i] = new(i, NASMaterial.Wood);
            i = 131;
            blocks[i] = new(i, NASMaterial.Plant)
            {
                disturbDelayMin = fallSpeed,
                disturbDelayMax = fallSpeed,
                disturbedAction = FallingBlockAction(NASPlugin.FromRaw(i))
            };
            i = 60;
            blocks[i] = new(i, NASMaterial.Stone, 8);
            i = 681;
            blocks[i] = new(i, NASMaterial.Stone, 12);
            i = 61;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Stone], 1);
            i = 62;
            blocks[i] = new(i, NASMaterial.Glass);
            i = 63;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Stone], 1);
            i = 166;
            blocks[i] = new(i, blocks[63]);
            i = 167;
            blocks[i] = new(i, blocks[63]);
            i = 235;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Stone], 1);
            i = 84;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Stone], 1);
            i = 85;
            blocks[i] = new(i, blocks[84]);
            i = 286;
            blocks[i] = new(i++, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Stone], 1);
            blocks[i] = new(i++, blocks[286]);
            blocks[i] = new(i++, blocks[286]);
            blocks[i] = new(i++, blocks[286]);
            i = 274;
            blocks[i] = new(i++, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Stone], 1);
            blocks[i] = new(i++, blocks[274]);
            blocks[i] = new(i++, blocks[274]);
            blocks[i] = new(i++, blocks[274]);
            i = 591;
            blocks[i] = new(i++, blocks[274]);
            blocks[i] = new(i++, blocks[274]);
            blocks[i] = new(i++, blocks[274]);
            blocks[i] = new(i++, blocks[274]);
            i = 104;
            blocks[i] = new(i, NASMaterial.Leaves)
            {
                disturbedAction = LeafBlockAction(logSet, NASPlugin.FromRaw(i)),
                disturbDelayMin = leafShrivelDelayMin,
                disturbDelayMax = leafShrivelDelayMax
            };
            i = 198;
            blocks[i] = new(i, blocks[17])
            {
                station = new NASCrafting.NASStation
                {
                    name = "Crafting Table",
                    type = NASCrafting.NASStation.NASStationType.Normal,
                    ori = NASCrafting.NASStation.NASOrientation.NS
                },
                existAction = CraftingExistAction(),
                interaction = CraftingInteraction()
            };
            i = 199;
            blocks[i] = new(i, blocks[198]);
            blocks[i].station.ori = NASCrafting.NASStation.NASOrientation.WE;
            i = 413;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Stone], 1)
            {
                station = new NASCrafting.NASStation
                {
                    name = "Auto Crafter",
                    type = NASCrafting.NASStation.NASStationType.Normal,
                    ori = NASCrafting.NASStation.NASOrientation.NS
                },
                existAction = AutoCraftExistAction(),
                interaction = AutoCraftInteraction(),
                disturbedAction = AutoCraftingAction(),
                container = new NASContainer
                {
                    type = NASContainer.NASContainerType.AutoCraft
                }
            };
            i = 414;
            blocks[i] = new(i, blocks[413]);
            blocks[i].station.ori = NASCrafting.NASStation.NASOrientation.WE;
            i = 462;
            blocks[i] = new(i, blocks[242])
            {
                station = new NASCrafting.NASStation
                {
                    name = "Crafting Table",
                    type = NASCrafting.NASStation.NASStationType.Normal,
                    ori = NASCrafting.NASStation.NASOrientation.NS
                },
                existAction = CraftingExistAction(),
                interaction = CraftingInteraction(),
                alternateID = 198
            };
            i = 463;
            blocks[i] = new(i, blocks[462]);
            blocks[i].station.ori = NASCrafting.NASStation.NASOrientation.WE;
            blocks[i].alternateID = 199;
            i = 464;
            blocks[i] = new(i, blocks[250])
            {
                station = new NASCrafting.NASStation
                {
                    name = "Crafting Table",
                    type = NASCrafting.NASStation.NASStationType.Normal,
                    ori = NASCrafting.NASStation.NASOrientation.NS
                },
                existAction = CraftingExistAction(),
                interaction = CraftingInteraction(),
                alternateID = 198
            };
            i = 465;
            blocks[i] = new(i, blocks[464]);
            blocks[i].station.ori = NASCrafting.NASStation.NASOrientation.WE;
            blocks[i].alternateID = 199;
            i = 625;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Stone], 1)
            {
                station = new NASCrafting.NASStation
                {
                    name = "Furnace",
                    type = NASCrafting.NASStation.NASStationType.Furnace,
                    ori = NASCrafting.NASStation.NASOrientation.WE
                },
                existAction = CraftingExistAction(),
                interaction = CraftingInteraction()
            };
            i = 626;
            blocks[i] = new(i, blocks[625]);
            blocks[i].station.ori = NASCrafting.NASStation.NASOrientation.NS;
            i = 239;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Stone], 1)
            {
                collideAction = FireCollideAction()
            };
            i = 204;
            blocks[i] = new(i++, NASMaterial.Metal, 3);
            blocks[i] = new(i++, blocks[204]);
            blocks[i] = new(i++, blocks[204]);
            blocks[i] = new(i, blocks[204]);
            i = 142;
            blocks[i] = new(i, NASMaterial.Wood, DefaultDurabilities[(int)NASMaterial.Wood] * 2)
            {
                interaction = CrateInteraction("You can't open it. It's just for decoration.")
            };
            i = 132;
            blocks[i] = new(i, NASMaterial.Wood, DefaultDurabilities[(int)NASMaterial.Wood] * 2)
            {
                interaction = BookshelfInteraction()
            };
            i = 143;
            blocks[i] = new(i, NASMaterial.Wood, DefaultDurabilities[(int)NASMaterial.Wood] * 2)
            {
                container = new NASContainer
                {
                    type = NASContainer.NASContainerType.Barrel
                },
                existAction = ContainerExistAction(),
                interaction = ContainerInteraction()
            };
            i = 602;
            blocks[i] = new(i++, blocks[143]);
            blocks[i] = new(i, blocks[143]);
            i = 216;
            blocks[i] = new(i, NASMaterial.Wood, DefaultDurabilities[(int)NASMaterial.Wood] * 2)
            {
                container = new NASContainer
                {
                    type = NASContainer.NASContainerType.Chest
                },
                existAction = ContainerExistAction(),
                interaction = ContainerInteraction()
            };
            i++;
            blocks[i] = new(i, blocks[216]);
            i++;
            blocks[i] = new(i, blocks[216]);
            i++;
            blocks[i] = new(i, blocks[216]);
            i = 647;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Stone], 0)
            {
                container = new NASContainer
                {
                    type = NASContainer.NASContainerType.Gravestone
                },
                existAction = ContainerExistAction(),
                interaction = ContainerInteraction()
            };
            i = 586;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Stone] + 2, 1)
            {
                dropHandler = CustomDrop(61, 1),
                expGivenMin = 4,
                expGivenMax = 10
            };
            i = 454;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Stone] + 2, 1)
            {
                dropHandler = CustomDrop(61, 1),
                expGivenMin = 4,
                expGivenMax = 10
            };
            i = 455;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Stone] + 2, 1)
            {
                dropHandler = CustomDrop(672, 1),
                expGivenMin = 6,
                expGivenMax = 12
            };
            i = 627;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Stone] + 2, 1)
            {
                dropHandler = (NasPlayer, dropID) =>
                {
                    return new(197, r.Next(2, 5));
                },
                expGivenMin = 0,
                expGivenMax = 4
            };
            i = 197;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Stone] + 2, 1);
            i = 628;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Stone] + 4, 1);
            i = 629;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Stone] + 6, 2);
            i = 630;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Stone] + 6, 3);
            i = 649;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Stone] + 7, 4);
            i = 644;
            blocks[i] = new(i, NASMaterial.Plant)
            {
                disturbedAction = CropAction(wheatSet, 0),
                disturbDelayMin = wheatGrowMin,
                disturbDelayMax = wheatGrowMax,
                dropHandler = CustomDrop(644, 1)
            };
            i = 645;
            blocks[i] = new(i, NASMaterial.Plant)
            {
                disturbedAction = CropAction(wheatSet, 1),
                disturbDelayMin = wheatGrowMin,
                disturbDelayMax = wheatGrowMax,
                dropHandler = CustomDrop(644, 1)
            };
            i = 646;
            blocks[i] = new(i, NASMaterial.Plant)
            {
                disturbedAction = CropAction(wheatSet, 2),
                disturbDelayMin = wheatGrowMin,
                disturbDelayMax = wheatGrowMax,
                dropHandler = CustomDrop(644, 1)
            };
            i = 461;
            blocks[i] = new(i, NASMaterial.Plant)
            {
                disturbedAction = CropAction(wheatSet, 3),
                dropHandler = (NasPlayer, dropID) =>
                {
                    if (r.Next(0, 2) == 0)
                    {
                        return new(145, 1);
                    }
                    return new(644, r.Next(2, 5));
                }
            };
            i = 624;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Metal], 1)
            {
                disturbDelayMin = fallSpeed / 2f,
                disturbDelayMax = fallSpeed / 2f,
                disturbedAction = FallingBlockAction(NASPlugin.FromRaw(i))
            };
            i = 729;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Metal], 1)
            {
                disturbedAction = IronCropAction(ironSet, 0),
                disturbDelayMin = wheatGrowMin * 2,
                disturbDelayMax = wheatGrowMax * 2,
                dropHandler = CustomDrop(729, 1)
            };
            i = 730;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Metal], 1)
            {
                disturbedAction = IronCropAction(ironSet, 1),
                disturbDelayMin = wheatGrowMin * 2,
                disturbDelayMax = wheatGrowMax * 2,
                dropHandler = CustomDrop(729, 1)
            };
            i = 731;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Metal], 1)
            {
                disturbedAction = IronCropAction(ironSet, 2),
                disturbDelayMin = wheatGrowMin * 2,
                disturbDelayMax = wheatGrowMax * 2,
                dropHandler = CustomDrop(729, 1)
            };
            i = 479;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Metal], 1)
            {
                disturbedAction = IronCropAction(ironSet, 3),
                dropHandler = (NasPlayer, dropID) =>
                {
                    NASDrop finalDrop = new(729, r.Next(1, 4));
                    if (r.Next(0, 2) == 0)
                    {
                        finalDrop.blockStacks.Add(new(624, 1));
                    }
                    return finalDrop;
                }
            };
            i = 667;
            blocks[i] = new(i, NASMaterial.Plant)
            {
                disturbDelayMin = sugarGrowMin,
                disturbDelayMax = sugarGrowMax,
                disturbedAction = GrowAction(NASPlugin.FromRaw(i))
            };
            i = 106;
            blocks[i] = new(i, NASMaterial.Plant)
            {
                disturbDelayMin = sugarGrowMin,
                disturbDelayMax = sugarGrowMax,
                disturbedAction = GrowAction(NASPlugin.FromRaw(i))
            };
            i = 107;
            blocks[i] = new(i, NASMaterial.Plant)
            {
                disturbDelayMin = sugarGrowMin / 5f,
                disturbDelayMax = sugarGrowMax / 5f,
                disturbedAction = VineGrowAction(NASPlugin.FromRaw(i)),
                instantAction = VineDeathAction()
            };
            i = 146;
            blocks[i] = new(i, NASMaterial.Leaves)
            {
                disturbedAction = LeafBlockAction(logSet, NASPlugin.FromRaw(146)),
                disturbDelayMin = leafShrivelDelayMin,
                disturbDelayMax = leafShrivelDelayMax,
                dropHandler = (NasPlayer, dropID) =>
                {
                    NASDrop drop = new(146, 1);
                    if (r.Next(0, 8) == 0)
                    {
                        drop.blockStacks.Add(new(450, 1));
                    }
                    else
                    {
                        drop = new(146, 1);
                    }
                    return drop;
                }
            };
            i = 449;
            blocks[i] = new(i, NASMaterial.Plant)
            {
                disturbedAction = LilyAction()
            };
            i = 171;
            blocks[i] = new(i, NASMaterial.None, DefaultDurabilities[(int)NASMaterial.Wood])
            {
                existAction = MessageExistAction(),
                interaction = MessageInteraction()
            };
            i = 145;
            blocks[i] = new(i, NASMaterial.Organic, 3)
            {
                fallDamageMultiplier = 0.1f
            };
            i = 622;
            blocks[i] = new(i++, blocks[145]);
            blocks[i] = new(i++, blocks[145]);
            i = 640;
            blocks[i] = new(i, NASMaterial.Organic, 3)
            {
                interaction = EatInteraction(breadSet, 0, breadRestore * 2)
            };
            i++;
            blocks[i] = new(i, NASMaterial.Organic, 3)
            {
                interaction = EatInteraction(breadSet, 1, breadRestore)
            };
            i++;
            blocks[i] = new(i, NASMaterial.Organic, 3)
            {
                interaction = EatInteraction(breadSet, 2, breadRestore)
            };
            i = 542;
            blocks[i] = new(i, NASMaterial.Organic, 3)
            {
                interaction = EatInteraction(waffleSet, 0, pieRestore)
            };
            i++;
            blocks[i] = new(i, NASMaterial.Organic, 3)
            {
                interaction = EatInteraction(waffleSet, 1, pieRestore)
            };
            i = 668;
            blocks[i] = new(i, NASMaterial.Organic, 3)
            {
                interaction = EatInteraction(pieSet, 0, pieRestore)
            };
            i++;
            blocks[i] = new(i, NASMaterial.Organic, 3)
            {
                interaction = EatInteraction(pieSet, 1, pieRestore)
            };
            i++;
            blocks[i] = new(i, NASMaterial.Organic, 3)
            {
                interaction = EatInteraction(pieSet, 2, pieRestore)
            };
            i++;
            blocks[i] = new(i, NASMaterial.Organic, 3)
            {
                interaction = EatInteraction(pieSet, 3, pieRestore)
            };
            i = 698;
            blocks[i] = new(i, NASMaterial.Organic, 3)
            {
                interaction = EatInteraction(peachPieSet, 0, pieRestore)
            };
            i++;
            blocks[i] = new(i, NASMaterial.Organic, 3)
            {
                interaction = EatInteraction(peachPieSet, 1, pieRestore)
            };
            i++;
            blocks[i] = new(i, NASMaterial.Organic, 3)
            {
                interaction = EatInteraction(peachPieSet, 2, pieRestore)
            };
            i++;
            blocks[i] = new(i, NASMaterial.Organic, 3)
            {
                interaction = EatInteraction(peachPieSet, 3, pieRestore)
            };
            i = 654;
            blocks[i] = new(i, NASMaterial.Organic, 3)
            {
                interaction = EatInteraction(set, 0, -6f)
            };
            i = 652;
            blocks[i] = new(i, NASMaterial.Organic, 3)
            {
                interaction = EatInteraction(set, 0, -5f)
            };
            i = 648;
            blocks[i] = new(i, NASMaterial.Organic, 3)
            {
                disturbDelayMin = fallSpeed,
                disturbDelayMax = fallSpeed,
                disturbedAction = FallingBlockAction(NASPlugin.FromRaw(i)),
                interaction = EatInteraction(set, 0, 1f)
            };
            i = 702;
            blocks[i] = new(i, NASMaterial.Organic, 3)
            {
                disturbDelayMin = fallSpeed,
                disturbDelayMax = fallSpeed,
                disturbedAction = FallingBlockAction(NASPlugin.FromRaw(i)),
                interaction = EatInteraction(set2, 0, 1f)
            };
            i = 478;
            blocks[i] = new(i, NASMaterial.Organic, 3)
            {
                disturbDelayMin = fallSpeed / 2f,
                disturbDelayMax = fallSpeed / 2f,
                disturbedAction = FallingBlockAction(NASPlugin.FromRaw(i)),
                interaction = EatInteraction(set3, 0, 10f, 0.5f)
            };
            i = 36;
            blocks[i] = new(i, NASMaterial.Organic, 4);
            i = 27;
            blocks[i] = new(i, NASMaterial.Organic, 4)
            {
                alternateID = 36
            };
            i = 35;
            blocks[i] = new(i, NASMaterial.Organic, 4)
            {
                alternateID = 36
            };
            i = 30;
            blocks[i] = new(i, NASMaterial.Organic, 4)
            {
                alternateID = 36
            };
            i = 138;
            blocks[i] = new(i, NASMaterial.Organic, 4)
            {
                alternateID = 36
            };
            i = 23;
            blocks[i] = new(i, NASMaterial.Organic, 4)
            {
                alternateID = 36
            };
            i = 29;
            blocks[i] = new(i, NASMaterial.Organic, 4)
            {
                alternateID = 36
            };
            i = 34;
            blocks[i] = new(i, NASMaterial.Organic, 4)
            {
                alternateID = 36
            };
            i = 26;
            blocks[i] = new(i, NASMaterial.Organic, 4)
            {
                alternateID = 36
            };
            i = 32;
            blocks[i] = new(i, NASMaterial.Organic, 4)
            {
                alternateID = 36
            };
            i = 200;
            blocks[i] = new(i, NASMaterial.Organic, 4)
            {
                alternateID = 36
            };
            i = 22;
            blocks[i] = new(i, NASMaterial.Organic, 4)
            {
                alternateID = 36
            };
            i = 21;
            blocks[i] = new(i, NASMaterial.Organic, 4)
            {
                alternateID = 36
            };
            i = 25;
            blocks[i] = new(i, NASMaterial.Organic, 4)
            {
                alternateID = 36
            };
            i = 31;
            blocks[i] = new(i, NASMaterial.Organic, 4)
            {
                alternateID = 36
            };
            i = 28;
            blocks[i] = new(i, NASMaterial.Organic, 4)
            {
                alternateID = 36
            };
            i = 703;
            blocks[i] = new(i, NASMaterial.Organic, 4)
            {
                interaction = BedInteraction()
            };
            i = 96;
            blocks[i] = new(i, NASMaterial.Plant)
            {
                dropHandler = (NasPlayer, dropID) =>
                {
                    if (NasPlayer.inventory.HeldItem.name == "Shears")
                    {
                        return new(96, 1);
                    }
                    else
                    {
                        return (r.Next(0, 2) == 0) ? new(36, 1) : null;
                    }
                },
                disturbedAction = GenericPlantAction()
            };
            i = 651;
            blocks[i] = new(i, NASMaterial.Plant)
            {
                dropHandler = (NasPlayer, dropID) =>
                {
                    if (NasPlayer.inventory.HeldItem.name == "Shears")
                    {
                        return new(651, 1);
                    }
                    else
                    {
                        return (r.Next(0, 3) == 0) ? new(23, 1) : null;
                    }
                },
                disturbedAction = GenericPlantAction()
            };
            i = 201;
            blocks[i] = new(i, NASMaterial.Plant)
            {
                dropHandler = (NasPlayer, dropID) =>
                {
                    if (NasPlayer.inventory.HeldItem.name == "Shears")
                    {
                        return new(201, 1);
                    }
                    else
                    {
                        return (r.Next(0, 2) == 0) ? new(138, 1) : null;
                    }
                },
                disturbedAction = GenericPlantAction()
            };
            i = 604;
            blocks[i] = new(i, NASMaterial.Organic, 3)
            {
                interaction = EatInteraction(set4, 0, 1f)
            };
            i = 456;
            blocks[i] = new(i, NASMaterial.Organic, 3)
            {
                interaction = EatInteraction(set4, 0, 1f)
            };
            i = 653;
            blocks[i] = new(i, NASMaterial.Organic, 3)
            {
                interaction = EatInteraction(set4, 0, -2.5f)
            };
            for (i = 484; i < 524; i++)
            {
                blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Stone], 1);
            }
            for (i = 713; i <= 728; i++)
            {
                blocks[i] = new(i, NASMaterial.Organic, 2);
            }
            i = 655;
            blocks[i] = new(i, NASMaterial.Glass)
            {
                disturbDelayMin = fallSpeed,
                disturbDelayMax = fallSpeed,
                disturbedAction = FallingBlockAction(NASPlugin.FromRaw(i)),
                collideAction = AirCollideAction()
            };
            i = 696;
            blocks[i] = new(i, NASMaterial.Stone, 384, 4)
            {
                childIDs = new()
            };
            blocks[i].childIDs.Add(10);
            i = 697;
            blocks[i] = new(i, NASMaterial.Stone, 384, 4)
            {
                existAction = LavaBarrelAction()
            };
            i = 672;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Metal], 1)
            {
                existAction = WireExistAction(0, 4),
                disturbedAction = UnrefinedGoldAction(NASPlugin.FromRaw(237), NASPlugin.FromRaw(672), NASPlugin.FromRaw(672))
            };
            i = 237;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Metal], 1)
            {
                existAction = WireBreakAction(),
                disturbedAction = UnrefinedGoldAction(NASPlugin.FromRaw(237), NASPlugin.FromRaw(672), NASPlugin.FromRaw(237)),
                dropHandler = CustomDrop(672, 1)
            };
            i = 674;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Metal], 1)
            {
                interaction = ChangeInteraction(NASPlugin.FromRaw(675)),
                existAction = WireExistAction(0, 4)
            };
            i = 675;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Metal], 1)
            {
                interaction = ChangeInteraction(NASPlugin.FromRaw(674)),
                dropHandler = CustomDrop(674, 1),
                existAction = WireExistAction(15, 4)
            };
            i = 74;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Metal], 1)
            {
                disturbedAction = PowerSourceAction(4),
                existAction = WireExistAction(15, 4)
            };
            i = 195;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Metal], 1)
            {
                interaction = ChangeInteraction(NASPlugin.FromRaw(196)),
                existAction = WireExistAction(0, 4)
            };
            i = 196;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Metal], 1)
            {
                disturbDelayMin = 1.2f,
                disturbDelayMax = 1.2f,
                disturbedAction = TurnOffAction(),
                dropHandler = CustomDrop(195, 1),
                existAction = WireExistAction(15, 4)
            };
            i = 704;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Metal], 1)
            {
                disturbDelayMin = 0.125f,
                disturbDelayMax = 0.125f,
                disturbedAction = PistonAction("off", 0, 1, 0, pistonUp)
            };
            i = 705;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Metal], 1)
            {
                disturbDelayMin = 0.125f,
                disturbDelayMax = 0.125f,
                disturbedAction = PistonAction("body", 0, 1, 0, pistonUp),
                dropHandler = CustomDrop(704, 1)
            };
            i = 706;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Metal], 1)
            {
                disturbedAction = PistonAction("head", 0, 1, 0, pistonUp)
            };
            blocks[i].dropHandler = (NasPlayer, dropID) =>
            {
                blocks[i].collideAction = AirCollideAction();
                return (1 == 0) ? new(1, 1) : null;
            };
            i = 707;
            blocks[i] = new(i, blocks[704])
            {
                disturbedAction = PistonAction("off", 0, -1, 0, pistonDown)
            };
            i = 708;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Metal], 1)
            {
                disturbDelayMin = 0.125f,
                disturbDelayMax = 0.125f,
                disturbedAction = PistonAction("body", 0, -1, 0, pistonDown),
                dropHandler = CustomDrop(704, 1)
            };
            i = 709;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Metal], 1)
            {
                disturbedAction = PistonAction("head", 0, -1, 0, pistonDown)
            };
            blocks[i].dropHandler = (NasPlayer, dropID) =>
            {
                blocks[i].collideAction = AirCollideAction();
                return (1 == 0) ? new(1, 1) : null;
            };
            i = 678;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Metal], 1)
            {
                disturbDelayMin = 0.125f,
                disturbDelayMax = 0.125f,
                disturbedAction = StickyPistonAction("off", 0, 1, 0, stickyPistonUp)
            };
            i = 679;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Metal], 1)
            {
                disturbDelayMin = 0.125f,
                disturbDelayMax = 0.125f,
                disturbedAction = StickyPistonAction("body", 0, 1, 0, stickyPistonUp),
                dropHandler = CustomDrop(678, 1)
            };
            i = 680;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Metal], 1)
            {
                disturbedAction = StickyPistonAction("head", 0, 1, 0, stickyPistonUp)
            };
            blocks[i].dropHandler = (NasPlayer, dropID) =>
            {
                blocks[i].collideAction = AirCollideAction();
                return (1 == 0) ? new(1, 1) : null;
            };
            i = 710;
            blocks[i] = new(i, blocks[678])
            {
                disturbedAction = StickyPistonAction("off", 0, -1, 0, stickyPistonDown)
            };
            i = 711;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Metal], 1)
            {
                disturbDelayMin = 0.125f,
                disturbDelayMax = 0.125f,
                disturbedAction = StickyPistonAction("body", 0, -1, 0, stickyPistonDown),
                dropHandler = CustomDrop(678, 1)
            };
            i = 712;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Metal], 1)
            {
                disturbedAction = StickyPistonAction("head", 0, -1, 0, stickyPistonDown)
            };
            blocks[i].dropHandler = (NasPlayer, dropID) =>
            {
                blocks[i].collideAction = AirCollideAction();
                return (1 == 0) ? new(1, 1) : null;
            };
            DefinePiston(389, pistonNorth, "z", 1, 704);
            DefinePiston(392, pistonEast, "x", -1, 704);
            DefinePiston(395, pistonSouth, "z", -1, 704);
            DefinePiston(398, pistonWest, "x", 1, 704);
            DefinePiston(401, stickyPistonNorth, "z", 1, 678, true);
            DefinePiston(404, stickyPistonEast, "x", -1, 678, true);
            DefinePiston(407, stickyPistonSouth, "z", -1, 678, true);
            DefinePiston(410, stickyPistonWest, "x", 1, 678, true);
            i = 609;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Metal], 1)
            {
                existAction = BeaconInteractAction()
            };
            i = 445;
            blocks[i] = new(i, blocks[1])
            {
                existAction = BeaconInteractAction(),
                durability = DefaultDurabilities[(int)NASMaterial.Metal]
            };
            i = 612;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Metal], 1)
            {
                existAction = BedBeaconAction()
            };
            i = 550;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Metal], 1)
            {
                existAction = WireExistAction(0, 1),
                disturbedAction = WireAction(wireSetActive, wireSetInactive, 1, NASPlugin.FromRaw(550))
            };
            i++;
            blocks[i] = new(i, blocks[550])
            {
                existAction = WireExistAction(0, 0),
                disturbedAction = WireAction(wireSetActive, wireSetInactive, 0, NASPlugin.FromRaw(551))
            };
            i++;
            blocks[i] = new(i, blocks[550])
            {
                existAction = WireExistAction(0, 2),
                disturbedAction = WireAction(wireSetActive, wireSetInactive, 2, NASPlugin.FromRaw(552))
            };
            i = 682;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Metal], 1)
            {
                disturbedAction = WireAction(wireSetActive, wireSetInactive, 1, NASPlugin.FromRaw(682)),
                existAction = WireBreakAction(),
                dropHandler = CustomDrop(550, 1)
            };
            i++;
            blocks[i] = new(i, blocks[682])
            {
                disturbedAction = WireAction(wireSetActive, wireSetInactive, 0, NASPlugin.FromRaw(683))
            };
            i++;
            blocks[i] = new(i, blocks[682])
            {
                disturbedAction = WireAction(wireSetActive, wireSetInactive, 2, NASPlugin.FromRaw(684))
            };
            i = 732;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Metal], 1)
            {
                existAction = WireExistAction(0, 12),
                disturbedAction = WireAction(fixedWireSetActive, fixedWireSetInactive, 1, NASPlugin.FromRaw(732))
            };
            i++;
            blocks[i] = new(i, blocks[732])
            {
                existAction = WireExistAction(0, 11),
                disturbedAction = WireAction(fixedWireSetActive, fixedWireSetInactive, 0, NASPlugin.FromRaw(733))
            };
            i++;
            blocks[i] = new(i, blocks[732])
            {
                existAction = WireExistAction(0, 13),
                disturbedAction = WireAction(fixedWireSetActive, fixedWireSetInactive, 2, NASPlugin.FromRaw(734))
            };
            i++;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Metal], 1)
            {
                disturbedAction = WireAction(fixedWireSetActive, fixedWireSetInactive, 1, NASPlugin.FromRaw(735)),
                existAction = WireBreakAction(),
                dropHandler = CustomDrop(732, 1)
            };
            i++;
            blocks[i] = new(i, blocks[735])
            {
                disturbedAction = WireAction(fixedWireSetActive, fixedWireSetInactive, 0, NASPlugin.FromRaw(736))
            };
            i++;
            blocks[i] = new(i, blocks[735])
            {
                disturbedAction = WireAction(fixedWireSetActive, fixedWireSetInactive, 2, NASPlugin.FromRaw(737))
            };
            i = 172;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Metal], 1)
            {
                existAction = WireExistAction(0, 5),
                disturbDelayMin = 0f,
                disturbDelayMax = 0f,
                disturbedAction = RepeaterAction(5, NASPlugin.FromRaw(172))
            };
            i++;
            blocks[i] = new(i, blocks[172])
            {
                existAction = WireExistAction(0, 6),
                disturbDelayMin = 0f,
                disturbDelayMax = 0f,
                disturbedAction = RepeaterAction(6, NASPlugin.FromRaw(173))
            };
            i++;
            blocks[i] = new(i, blocks[172])
            {
                existAction = WireExistAction(0, 7),
                disturbDelayMin = 0f,
                disturbDelayMax = 0f,
                disturbedAction = RepeaterAction(7, NASPlugin.FromRaw(174))
            };
            i++;
            blocks[i] = new(i, blocks[172])
            {
                existAction = WireExistAction(0, 8),
                disturbDelayMin = 0f,
                disturbDelayMax = 0f,
                disturbedAction = RepeaterAction(8, NASPlugin.FromRaw(175))
            };
            i++;
            blocks[i] = new(i, blocks[172])
            {
                existAction = WireExistAction(0, 9),
                disturbDelayMin = 0f,
                disturbDelayMax = 0f,
                disturbedAction = RepeaterAction(9, NASPlugin.FromRaw(176))
            };
            i++;
            blocks[i] = new(i, blocks[172])
            {
                existAction = WireExistAction(0, 10),
                disturbDelayMin = 0f,
                disturbDelayMax = 0f,
                disturbedAction = RepeaterAction(10, NASPlugin.FromRaw(177))
            };
            i = 613;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Metal], 1)
            {
                existAction = WireBreakAction(),
                disturbDelayMin = 0f,
                disturbDelayMax = 0f,
                disturbedAction = RepeaterAction(5, NASPlugin.FromRaw(613)),
                dropHandler = CustomDrop(172, 1)
            };
            i++;
            blocks[i] = new(i, blocks[613])
            {
                disturbDelayMin = 0f,
                disturbDelayMax = 0f,
                disturbedAction = RepeaterAction(6, NASPlugin.FromRaw(614)),
                dropHandler = CustomDrop(172, 1)
            };
            i++;
            blocks[i] = new(i, blocks[613])
            {
                disturbDelayMin = 0f,
                disturbDelayMax = 0f,
                disturbedAction = RepeaterAction(7, NASPlugin.FromRaw(615)),
                dropHandler = CustomDrop(172, 1)
            };
            i++;
            blocks[i] = new(i, blocks[613])
            {
                disturbDelayMin = 0f,
                disturbDelayMax = 0f,
                disturbedAction = RepeaterAction(8, NASPlugin.FromRaw(616)),
                dropHandler = CustomDrop(172, 1)
            };
            i++;
            blocks[i] = new(i, blocks[613])
            {
                disturbDelayMin = 0f,
                disturbDelayMax = 0f,
                disturbedAction = RepeaterAction(9, NASPlugin.FromRaw(617)),
                dropHandler = CustomDrop(172, 1)
            };
            i++;
            blocks[i] = new(i, blocks[613])
            {
                disturbDelayMin = 0f,
                disturbDelayMax = 0f,
                disturbedAction = RepeaterAction(10, NASPlugin.FromRaw(618)),
                dropHandler = CustomDrop(172, 1)
            };
            i = 610;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Stone], 1)
            {
                existAction = WireExistAction(0, 4),
                collideAction = PressureCollideAction()
            };
            i = 611;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Stone], 1)
            {
                existAction = WireBreakAction(),
                disturbDelayMin = 1f,
                disturbDelayMax = 1f,
                disturbedAction = PressurePlateAction(),
                dropHandler = CustomDrop(610, 1)
            };
            i = 415;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Stone], 1)
            {
                existAction = WireExistAction(0, 9),
                disturbDelayMin = 0.2f,
                disturbDelayMax = 0.2f,
                disturbedAction = ObserverActivateAction(9)
            };
            i++;
            blocks[i] = new(i, blocks[415])
            {
                existAction = WireExistAction(0, 10),
                disturbDelayMin = 0.2f,
                disturbDelayMax = 0.2f,
                disturbedAction = ObserverActivateAction(10)
            };
            i++;
            blocks[i] = new(i, blocks[415])
            {
                existAction = WireExistAction(0, 7),
                disturbDelayMin = 0.2f,
                disturbDelayMax = 0.2f,
                disturbedAction = ObserverActivateAction(7)
            };
            i++;
            blocks[i] = new(i, blocks[415])
            {
                existAction = WireExistAction(0, 8),
                disturbDelayMin = 0.2f,
                disturbDelayMax = 0.2f,
                disturbedAction = ObserverActivateAction(8)
            };
            i++;
            blocks[i] = new(i, blocks[415])
            {
                existAction = WireExistAction(0, 5),
                disturbDelayMin = 0.2f,
                disturbDelayMax = 0.2f,
                disturbedAction = ObserverActivateAction(5)
            };
            i++;
            blocks[i] = new(i, blocks[415])
            {
                existAction = WireExistAction(0, 6),
                disturbDelayMin = 0.2f,
                disturbDelayMax = 0.2f,
                disturbedAction = ObserverActivateAction(6)
            };
            i = 421;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Stone], 1)
            {
                existAction = WireBreakAction(),
                disturbDelayMin = 0.2f,
                disturbDelayMax = 0.2f,
                disturbedAction = ObserverDeactivateAction(9),
                dropHandler = CustomDrop(415, 1)
            };
            i++;
            blocks[i] = new(i, blocks[421])
            {
                existAction = WireBreakAction(),
                disturbDelayMin = 0.2f,
                disturbDelayMax = 0.2f,
                disturbedAction = ObserverDeactivateAction(10),
                dropHandler = CustomDrop(415, 1)
            };
            i++;
            blocks[i] = new(i, blocks[421])
            {
                existAction = WireBreakAction(),
                disturbDelayMin = 0.2f,
                disturbDelayMax = 0.2f,
                disturbedAction = ObserverDeactivateAction(7),
                dropHandler = CustomDrop(415, 1)
            };
            i++;
            blocks[i] = new(i, blocks[421])
            {
                existAction = WireBreakAction(),
                disturbDelayMin = 0.2f,
                disturbDelayMax = 0.2f,
                disturbedAction = ObserverDeactivateAction(8),
                dropHandler = CustomDrop(415, 1)
            };
            i++;
            blocks[i] = new(i, blocks[421])
            {
                existAction = WireBreakAction(),
                disturbDelayMin = 0.2f,
                disturbDelayMax = 0.2f,
                disturbedAction = ObserverDeactivateAction(5),
                dropHandler = CustomDrop(415, 1)
            };
            i++;
            blocks[i] = new(i, blocks[421])
            {
                existAction = WireBreakAction(),
                disturbDelayMin = 0.2f,
                disturbDelayMax = 0.2f,
                disturbedAction = ObserverDeactivateAction(6),
                dropHandler = CustomDrop(415, 1)
            };
            i = 427;
            blocks[i] = new(i, NASMaterial.Leaves)
            {
                disturbedAction = SpongeAction()
            };
            i++;
            blocks[i] = new(i, NASMaterial.Leaves);
            i = 429;
            blocks[i] = new(i, NASMaterial.Stone, 24, 1)
            {
                dropHandler = CustomDrop(430, 1)
            };
            i++;
            blocks[i] = new(i, NASMaterial.Stone, 24, 1)
            {
                alternateID = 1
            };
            i = 431;
            blocks[i] = new(i, NASMaterial.Stone, 24, 1);
            i++;
            blocks[i] = new(i, blocks[431]);
            i = 433;
            blocks[i] = new(i, NASMaterial.Stone, 24, 1);
            i++;
            blocks[i] = new(i, NASMaterial.Stone, 24, 1);
            i++;
            blocks[i] = new(i, NASMaterial.Stone, 24, 1);
            i++;
            blocks[i] = new(i, NASMaterial.Stone, 24, 1);
            i = 437;
            blocks[i] = new(i, NASMaterial.Stone, 24, 1);
            i++;
            blocks[i] = new(i, blocks[437]);
            i = 439;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Stone], 1)
            {
                existAction = ContainerExistAction(),
                disturbedAction = DispenserAction(0, 0, 1),
                interaction = ContainerInteraction(),
                container = new NASContainer
                {
                    type = NASContainer.NASContainerType.Dispenser
                }
            };
            i++;
            blocks[i] = new(i, blocks[439])
            {
                disturbedAction = DispenserAction(-1, 0, 0)
            };
            i++;
            blocks[i] = new(i, blocks[439])
            {
                disturbedAction = DispenserAction(0, 0, -1)
            };
            i++;
            blocks[i] = new(i, blocks[439])
            {
                disturbedAction = DispenserAction(1, 0, 0)
            };
            i++;
            blocks[i] = new(i, blocks[439])
            {
                disturbedAction = DispenserAction(0, -1, 0)
            };
            i++;
            blocks[i] = new(i, blocks[439])
            {
                disturbedAction = DispenserAction(0, 1, 0)
            };
        }
        public static NASFunc<NASPlayer, ushort, NASDrop> CustomDrop(ushort clientushort, int amount) => (NasPlayer, dropID) =>
                                                                                                            {
                                                                                                                return new(clientushort, amount);
                                                                                                            };
        public static void DefinePiston(ushort startID, ushort[] set, string axis, int direction, int parent, bool sticky = false)
        {
            ushort i = startID;
            blocks[i] = new(i, blocks[parent])
            {
                disturbedAction = SidewaysPistonAction("off", axis, direction, set, sticky)
            };
            i++;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Metal], 1)
            {
                disturbDelayMin = 0.125f,
                disturbDelayMax = 0.125f,
                disturbedAction = SidewaysPistonAction("body", axis, direction, set, sticky),
                dropHandler = CustomDrop((ushort)parent, 1)
            };
            i++;
            blocks[i] = new(i, NASMaterial.Stone, DefaultDurabilities[(int)NASMaterial.Metal], 1)
            {
                disturbedAction = SidewaysPistonAction("head", axis, direction, set, sticky),
                collideAction = AirCollideAction(),
                dropHandler = (NasPlayer, dropID) =>
                {
                    return (1 == 0) ? new(1, 1) : null;
                }
            };
        }
    }
}
