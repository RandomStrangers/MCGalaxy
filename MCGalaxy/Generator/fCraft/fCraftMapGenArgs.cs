// Part of fCraft | Copyright 2009-2015 Matvei Stefarov <me@matvei.org> | BSD-3 | See LICENSE.txt
using System;
namespace MCGalaxy.Generator.fCraft
{
    /// <summary> Contains parameters for advanced map generation. </summary>
    public sealed class FCraftMapGenArgs
    {
        public string MapName, Biome = MapGenBiome.FOREST;
        public int Seed,
            MaxHeight = 20,
            MaxDepth = 12,
            MaxHeightVariation = 4,
            MaxDepthVariation; // 0
        public bool AddWater = true,
            MatchWaterCoverage; // false
        public int WaterLevel = 48;
        public float WaterCoverage = .5f;
        public bool UseBias,
            DelayBias;      // false
        public float Bias;           // 0
        public int RaisedCorners,
            LoweredCorners,
            MidPoint,
            DetailScale = 7,
            FeatureScale = 1;
        public float Roughness = .5f;
        public bool MarbledHeightmap,
            InvertHeightmap;  // false
        public float AboveFuncExponent = 1,
            BelowFuncExponent = 1;
        public bool AddTrees = true,
            AddGiantTrees; // false
        public int TreeSpacingMin = 7,
            TreeSpacingMax = 11;
        public bool AddSnow; // false
        public int SnowAltitude = 70,
            SnowTransition = 7;
        public bool CliffSmoothing = true;
        public float CliffThreshold = 1;
        public bool AddBeaches; // false
        public int BeachExtent = 6,
            BeachHeight = 2;
        public static FCraftMapGenArgs MakeTemplate(int template) => template switch
        {
            0 => new FCraftMapGenArgs
            {
                MaxHeight = 8,
                MaxDepth = 20,
                FeatureScale = 3,
                Roughness = .46f,
                MatchWaterCoverage = true,
                WaterCoverage = .85f
            },
            1 => new FCraftMapGenArgs
            {
                //Biome = MapGenBiome.SANDY, TODO maybe?
                MaxHeight = 2,
                MaxDepth = 39,
                UseBias = true,
                Bias = .9f,
                MidPoint = 1,
                LoweredCorners = 4,
                FeatureScale = 2,
                DetailScale = 5,
                MarbledHeightmap = true,
                InvertHeightmap = true,
                MatchWaterCoverage = true,
                WaterCoverage = .95f
            },
            2 => new FCraftMapGenArgs
            {
                MaxHeight = 22,
                MaxDepth = 12,
                UseBias = true,
                Bias = 1,
                MidPoint = -1,
                RaisedCorners = 3,
                LoweredCorners = 1,
                TreeSpacingMax = 12,
                TreeSpacingMin = 6,
                MarbledHeightmap = true,
                DelayBias = true
            },
            3 => new FCraftMapGenArgs
            {
                Biome = MapGenBiome.DESERT,
                MaxHeight = 12,
                MaxDepth = 7,
                FeatureScale = 2,
                DetailScale = 3,
                Roughness = .44f,
                MarbledHeightmap = true,
                InvertHeightmap = true
            },
            4 => new FCraftMapGenArgs
            {
                Biome = MapGenBiome.PLAINS,
                MaxHeight = 8,
                MaxDepth = 8,
                FeatureScale = 2,
                TreeSpacingMin = 7,
                TreeSpacingMax = 13
            },
            5 => new FCraftMapGenArgs
            {
                Biome = MapGenBiome.ARCTIC,
                MaxHeight = 2,
                MaxDepth = 2032,
                FeatureScale = 2,
                DetailScale = 7,
                Roughness = .64f,
                MarbledHeightmap = true,
                MatchWaterCoverage = true,
                WaterCoverage = .3f,
                MaxHeightVariation = 0
            },
            6 => new FCraftMapGenArgs
            {
                MaxHeight = 16,
                MaxDepth = 39,
                UseBias = true,
                Bias = .7f,
                MidPoint = 1,
                LoweredCorners = 4,
                FeatureScale = 3,
                DetailScale = 7,
                MarbledHeightmap = true,
                DelayBias = true,
                AddBeaches = true,
                Roughness = 0.45f
            },
            7 => new FCraftMapGenArgs
            {
                MaxHeight = 14,
                MaxDepth = 20,
                UseBias = true,
                Bias = .65f,
                MidPoint = -1,
                RaisedCorners = 4,
                FeatureScale = 2,
                Roughness = .56f,
                MatchWaterCoverage = true,
                WaterCoverage = .3f
            },
            8 => new FCraftMapGenArgs
            {
                Biome = MapGenBiome.PLAINS,
                MaxHeight = 40,
                MaxDepth = 10,
                FeatureScale = 1,
                DetailScale = 7,
                MarbledHeightmap = true,
                AddSnow = true,
                MatchWaterCoverage = true,
                WaterCoverage = .5f,
                Roughness = .55f,
                CliffThreshold = .9f
            },
            10 => new FCraftMapGenArgs(),
            11 => new FCraftMapGenArgs
            {
                MaxHeight = 22,
                MaxDepth = 8,
                FeatureScale = 0,
                DetailScale = 6,
                MarbledHeightmap = true,
                MatchWaterCoverage = true,
                WaterCoverage = .31f
            },
            12 => new FCraftMapGenArgs
            {
                MaxHeight = 5,
                MaxDepth = 4,
                FeatureScale = 2,
                DetailScale = 7,
                Roughness = .55f,
                MarbledHeightmap = true,
                MatchWaterCoverage = true,
                WaterCoverage = .25f,
                TreeSpacingMin = 8,
                TreeSpacingMax = 14
            },
            9 => new FCraftMapGenArgs
            {
                MaxHeight = 22,
                MaxDepth = 12,
                UseBias = true,
                Bias = .5f,
                MidPoint = -1,
                RaisedCorners = 3,
                LoweredCorners = 1,
                TreeSpacingMax = 12,
                TreeSpacingMin = 6,
                InvertHeightmap = true,
                WaterCoverage = .5f
            },
            _ => throw new ArgumentOutOfRangeException("template"),
        };
    }
}
