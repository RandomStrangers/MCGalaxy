using MCGalaxy.Blocks;
using MCGalaxy.Maths;
namespace MCGalaxy
{
    public static class Collision
    {
        public static void Setup() => SetupBlockBounds(Server.mainLevel);
        public static void SetupBlockBounds(Level lvl)
        {
            NASBlock.blocksIndexedByServerushort = new NASBlock[1024];
            for (ushort blockID = 0; blockID < 1024; blockID++)
            {
                NASBlock.blocksIndexedByServerushort[blockID] = GetNasBlockAndFillInCollisionInformation(blockID, lvl);
            }
            NASLevel.OnLevelLoaded(lvl);
        }
        public static NASBlock GetNasBlockAndFillInCollisionInformation(ushort serverushort, Level lvl)
        {
            bool collides = true;
            AABB bounds;
            float fallDamageMultiplier = 1;
            Action<NASEntity, NASBlock, bool, ushort, ushort, ushort> collideAction = NASBlock.DefaultSolidCollideAction();
            BlockDefinition def = lvl.GetBlockDef(serverushort);
            if (def != null)
            {
                bounds = new(def.MinX * 2, def.MinZ * 2, def.MinY * 2,
                                def.MaxX * 2, def.MaxZ * 2, def.MaxY * 2);
                switch (def.CollideType)
                {
                    case 7:
                    case 5:
                    case 1:
                        bounds.Max.Y -= 4;
                        fallDamageMultiplier = 0;
                        collideAction = null;
                        break;
                    case 0:
                        collideAction = null;
                        collides = false;
                        break;
                    default:
                        break;
                }
            }
            else if (serverushort >= 256)
            {
                bounds = new(0, 0, 0, 32, 32, 32);
            }
            else
            {
                ushort core = NASPlugin.Convert(serverushort);
                bounds = new(0, 0, 0, 32, DefaultSet.Height(core) * 2, 32);
            }
            NASBlock nb = NASBlock.Get(ConvertToClientushort(serverushort));
            if (nb.fallDamageMultiplier == -1)
            {
                nb.collides = collides;
                nb.bounds = bounds;
                nb.fallDamageMultiplier = fallDamageMultiplier;
                nb.collideAction ??= collideAction;
            }
            return nb;
        }
        public static ushort ConvertToClientushort(ushort serverushort)
        {
            ushort clientushort;
            if (serverushort >= 256)
            {
                clientushort = NASPlugin.ToRaw(serverushort);
            }
            else
            {
                clientushort = NASPlugin.Convert(serverushort);
                if (clientushort >= 66)
                {
                    clientushort = 2;
                }
            }
            return clientushort;
        }
        public static bool TouchesGround(Level lvl, AABB entityAABB, Position entityPos, out float fallDamageMultiplier)
        {
            AABB worldAABB = entityAABB.OffsetPosition(entityPos);
            worldAABB.Min.X++;
            worldAABB.Min.Z++;
            entityPos.X += entityAABB.Max.X;
            entityPos.Z += entityAABB.Max.Z;
            if (NASTouchesGround(lvl, worldAABB, entityPos, out fallDamageMultiplier))
            {
                return true;
            }
            entityPos.X += entityAABB.Min.X * 2;
            if (NASTouchesGround(lvl, worldAABB, entityPos, out fallDamageMultiplier))
            {
                return true;
            }
            entityPos.Z += entityAABB.Min.Z * 2;
            if (NASTouchesGround(lvl, worldAABB, entityPos, out fallDamageMultiplier))
            {
                return true;
            }
            entityPos.X += entityAABB.Max.X * 2;
            if (NASTouchesGround(lvl, worldAABB, entityPos, out fallDamageMultiplier))
            {
                return true;
            }
            return false;
        }
        public static bool NASTouchesGround(Level lvl, AABB entityAABB, Position posToGetBlock, out float fallDamageMultiplier)
        {
            fallDamageMultiplier = 1;
            int x = posToGetBlock.FeetBlockCoords.X,
                y = posToGetBlock.FeetBlockCoords.Y,
                z = posToGetBlock.FeetBlockCoords.Z;
            ushort serverushort = lvl.GetBlock((ushort)x,
                                                 (ushort)y,
                                                 (ushort)z);
            if (serverushort == 0)
            {
                return false;
            }
            NASBlock nasBlock = NASBlock.blocksIndexedByServerushort[serverushort];
            if (!nasBlock.collides)
            {
                return false;
            }
            fallDamageMultiplier = nasBlock.fallDamageMultiplier;
            AABB blockAABB = nasBlock.bounds;
            blockAABB.Max.Y = 32;
            blockAABB = blockAABB.Offset(x * 32, y * 32, z * 32);
            if (AABB.Intersects(ref entityAABB, ref blockAABB))
            {
                return true;
            }
            return false;
        }
    }
}
