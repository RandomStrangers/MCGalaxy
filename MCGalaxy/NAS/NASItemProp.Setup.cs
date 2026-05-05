using System.Collections.Generic;
namespace MCGalaxy
{
    public partial class NASItemProp
    {
        public static Dictionary<string, bool> nothing = new(),
                toolEnchants = new(){
                {"Efficiency",true},
                {"Fortune",true},
                {"Mending",true},
                {"Silk Touch",true},
                {"Unbreaking",true},
            },
            swordEnchants = new(){
                {"Knockback",true},
                {"Mending",true},
                {"Sharpness",true},
                {"Unbreaking",true},
            },
            bootEnchants = new(){
                {"Feather Falling",true},
                {"Mending",true},
                {"Protection",true},
                {"Thorns",true},
                {"Unbreaking",true},
            }, 
            helmetEnchants = new(){
                {"Aqua Affinity",true},
                {"Mending",true},
                {"Protection",true},
                {"Respiration",true},
                {"Thorns",true},
                {"Unbreaking",true},
            },
            armorEnchants = new(){
                {"Mending",true},
                {"Protection",true},
                {"Thorns",true},
                {"Unbreaking",true},
            };
        public static NASItemProp bedrockPick,etheriumPick,
            bedrockSword, etheriumHelmet, etheriumChest, 
            etheriumLegs, etheriumBoots, etheriumSword;
        public static void SetupDevItems()
        {
            bedrockPick = new("Bedrock Pickaxe|m|╟", nothing, NASMaterial.None, -1f, 5)
            {
                baseHP = int.MaxValue
            };
            bedrockSword = new("Bedrock Sword|0|α", swordEnchants, NASMaterial.Leaves, 1f, 6)
            {
                damage = 50f,
                knockback = 2f,
                recharge = 1,
                baseHP = int.MaxValue
            };
            etheriumSword = new("Etherium Sword|`|α", swordEnchants, NASMaterial.Leaves, 1f, 6)
            {
                damage = int.MaxValue,
                knockback = 0f,
                recharge = 0,
                baseHP = int.MaxValue
            };
            etheriumPick = new("Etherium Pickaxe|`|ß", toolEnchants, NASMaterial.Stone, 1f, 6)
            {
                baseHP = int.MaxValue
            };
            etheriumPick.materialsEffectiveAgainst.Add(NASMaterial.Stone);
            etheriumPick.materialsEffectiveAgainst.Add(NASMaterial.Earth);
            etheriumPick.materialsEffectiveAgainst.Add(NASMaterial.Wood);
            etheriumPick.materialsEffectiveAgainst.Add(NASMaterial.Plant);
            etheriumPick.materialsEffectiveAgainst.Add(NASMaterial.Leaves);
            etheriumPick.materialsEffectiveAgainst.Add(NASMaterial.Organic);
            etheriumPick.materialsEffectiveAgainst.Add(NASMaterial.Glass);
            etheriumPick.materialsEffectiveAgainst.Add(NASMaterial.Metal);
            etheriumPick.materialsEffectiveAgainst.Add(NASMaterial.Liquid);
            etheriumPick.materialsEffectiveAgainst.Add(NASMaterial.Lava);
            etheriumChest = new("Etherium Chestplate|`|Φ", armorEnchants, NASMaterial.None, 0, 0)
            {
                baseHP = int.MaxValue,
                armor = int.MaxValue
            };
            etheriumHelmet = new("Etherium Helmet|`|τ", helmetEnchants, NASMaterial.None, 0, 0)
            {
                baseHP = int.MaxValue,
                armor = int.MaxValue
            };
            etheriumLegs = new("Etherium Leggings|`|Θ", armorEnchants, NASMaterial.None, 0, 0)
            {
                baseHP = int.MaxValue,
                armor = int.MaxValue
            };
            etheriumBoots = new("Etherium Boots|`|Ω", bootEnchants, NASMaterial.None, 0, 0)
            {
                baseHP = int.MaxValue,
                armor = int.MaxValue
            };
        }
        public static void Setup()
        {
            SetupDevItems();
            int ironBaseHP = 200 * 8,
                goldBaseHP = 200 * 64,
                diamondBaseHP = 200 * 128,
                emeraldBaseHP = 200 * 192;
            NASItemProp fist = new("Fist|f|¬", nothing, NASMaterial.None, 0, 0)
            {
                baseHP = int.MaxValue
            };
            NASItem.Fist = new("Fist");
            NASItemProp key = new("Key|f|σ", nothing, NASMaterial.None, 0, 0)
            {
                baseHP = int.MaxValue
            };
            NASItemProp fishing = new("Fishing Rod|f|δ", nothing, NASMaterial.None, 0, 0)
            {
                baseHP = 200 * 7,
                damage = 0f,
                knockback = -1f
            };
            NASItemProp shears = new("Shears|f|µ", nothing, NASMaterial.Organic, 0.75f, 1)
            {
                baseHP = 200 * 8
            };
            NASItemProp woodPick = new("Wood Pickaxe|s|ß", toolEnchants, NASMaterial.Stone, 0.0f, 1)
            {
                baseHP = 12
            };
            NASItemProp stonePick = new("Stone Pickaxe|7|ß", toolEnchants, NASMaterial.Stone, 0.75f, 1),
                stoneShovel = new("Stone Shovel|7|Γ", toolEnchants, NASMaterial.Earth, 0.50f, 1),
                stoneAxe = new("Stone Axe|7|π", toolEnchants, NASMaterial.Wood, 0.60f, 1);
            stoneAxe.materialsEffectiveAgainst.Add(NASMaterial.Leaves);
            NASItemProp stoneSword = new("Stone Sword|7|α", swordEnchants, NASMaterial.Leaves, 0.50f, 1)
            {
                damage = 2.5f,
                recharge = 750
            };
            NASItemProp ironPick = new("Iron Pickaxe|f|ß", toolEnchants, NASMaterial.Stone, 0.85f, 3)
            {
                baseHP = ironBaseHP
            };
            NASItemProp ironShovel = new("Iron Shovel|f|Γ", toolEnchants, NASMaterial.Earth, 0.60f, 3)
            {
                baseHP = ironBaseHP
            };
            NASItemProp ironAxe = new("Iron Axe|f|π", toolEnchants, NASMaterial.Wood, 0.75f, 3)
            {
                baseHP = ironBaseHP
            };
            ironAxe.materialsEffectiveAgainst.Add(NASMaterial.Leaves);
            NASItemProp ironSword = new("Iron Sword|f|α", swordEnchants, NASMaterial.Leaves, 0.75f, 3)
            {
                damage = 3f,
                recharge = 750,
                knockback = 1.25f,
                baseHP = ironBaseHP
            };
            NASItemProp goldPick = new("Gold Pickaxe|6|ß", toolEnchants, NASMaterial.Stone, 0.90f, 3)
            {
                baseHP = goldBaseHP
            };
            NASItemProp goldShovel = new("Gold Shovel|6|Γ", toolEnchants, NASMaterial.Earth, 0.85f, 3)
            {
                baseHP = goldBaseHP
            };
            NASItemProp goldAxe = new("Gold Axe|6|π", toolEnchants, NASMaterial.Wood, 0.90f, 3)
            {
                baseHP = goldBaseHP
            };
            goldAxe.materialsEffectiveAgainst.Add(NASMaterial.Leaves);
            NASItemProp goldSword = new("Gold Sword|6|α", swordEnchants, NASMaterial.Leaves, 0.85f, 3)
            {
                damage = 3f,
                recharge = 750,
                knockback = 1.25f,
                baseHP = goldBaseHP
            };
            NASItemProp diamondPick = new("Diamond Pickaxe|b|ß", toolEnchants, NASMaterial.Stone, 0.95f, 4)
            {
                baseHP = diamondBaseHP
            };
            NASItemProp diamondShovel = new("Diamond Shovel|b|Γ", toolEnchants, NASMaterial.Earth, 1f, 4)
            {
                baseHP = diamondBaseHP
            };
            NASItemProp diamondAxe = new("Diamond Axe|b|π", toolEnchants, NASMaterial.Wood, 0.95f, 4)
            {
                baseHP = diamondBaseHP
            };
            diamondAxe.materialsEffectiveAgainst.Add(NASMaterial.Leaves);
            NASItemProp diamondSword = new("Diamond Sword|b|α", swordEnchants, NASMaterial.Leaves, 1f, 4)
            {
                damage = 3.5f,
                recharge = 750,
                knockback = 1.25f,
                baseHP = diamondBaseHP
            };
            NASItemProp emeraldPick = new("Emerald Pickaxe|2|ß", toolEnchants, NASMaterial.Stone, 0.975f, 5)
            {
                baseHP = emeraldBaseHP
            };
            emeraldPick.allowedEnchants["Efficiency"] = true;
            NASItemProp emeraldShovel = new("Emerald Shovel|2|Γ", toolEnchants, NASMaterial.Earth, 1f, 5)
            {
                baseHP = emeraldBaseHP
            };
            NASItemProp emeraldAxe = new("Emerald Axe|2|π", toolEnchants, NASMaterial.Wood, 0.975f, 5)
            {
                baseHP = emeraldBaseHP
            };
            emeraldAxe.materialsEffectiveAgainst.Add(NASMaterial.Leaves);
            NASItemProp emeraldSword = new("Emerald Sword|2|α", swordEnchants, NASMaterial.Leaves, 1f, 5)
            {
                damage = 4f,
                recharge = 750,
                knockback = 1.25f,
                baseHP = emeraldBaseHP
            };
            NASItemProp ironHelmet = new("Iron Helmet|f|τ", helmetEnchants, NASMaterial.None, 0, 0)
            {
                baseHP = ironBaseHP / 2f,
                armor = 2f
            };
            NASItemProp ironChest = new("Iron Chestplate|f|Φ", armorEnchants, NASMaterial.None, 0, 0)
            {
                baseHP = ironBaseHP / 2f,
                armor = 5f
            };
            NASItemProp ironLegs = new("Iron Leggings|f|Θ", armorEnchants, NASMaterial.None, 0, 0)
            {
                baseHP = ironBaseHP / 2f,
                armor = 4f
            };
            NASItemProp ironBoots = new("Iron Boots|f|Ω", bootEnchants, NASMaterial.None, 0, 0)
            {
                baseHP = ironBaseHP / 2f,
                armor = 1f
            };
            NASItemProp goldHelmet = new("Gold Helmet|6|τ", helmetEnchants, NASMaterial.None, 0, 0)
            {
                baseHP = goldBaseHP / 2f,
                armor = 2f
            };
            NASItemProp goldChest = new("Gold Chestplate|6|Φ", armorEnchants, NASMaterial.None, 0, 0)
            {
                baseHP = goldBaseHP / 2f,
                armor = 6f
            };
            NASItemProp goldLegs = new("Gold Leggings|6|Θ", armorEnchants, NASMaterial.None, 0, 0)
            {
                baseHP = goldBaseHP / 2f,
                armor = 5f
            };
            NASItemProp goldBoots = new("Gold Boots|6|Ω", bootEnchants, NASMaterial.None, 0, 0)
            {
                baseHP = goldBaseHP / 2f,
                armor = 2f
            };
            NASItemProp diamondHelmet = new("Diamond Helmet|b|τ", helmetEnchants, NASMaterial.None, 0, 0)
            {
                baseHP = diamondBaseHP / 2f,
                armor = 3f
            };
            NASItemProp diamondChest = new("Diamond Chestplate|b|Φ", armorEnchants, NASMaterial.None, 0, 0)
            {
                baseHP = diamondBaseHP / 2f,
                armor = 7f
            };
            NASItemProp diamondLegs = new("Diamond Leggings|b|Θ", armorEnchants, NASMaterial.None, 0, 0)
            {
                baseHP = diamondBaseHP / 2f,
                armor = 5f
            };
            NASItemProp diamondBoots = new("Diamond Boots|b|Ω", bootEnchants, NASMaterial.None, 0, 0)
            {
                baseHP = diamondBaseHP / 2f,
                armor = 3f
            };
            NASItemProp emeraldHelmet = new("Emerald Helmet|2|τ", helmetEnchants, NASMaterial.None, 0, 0)
            {
                baseHP = emeraldBaseHP / 2f,
                armor = 3f
            };
            NASItemProp emeraldChest = new("Emerald Chestplate|2|Φ", armorEnchants, NASMaterial.None, 0, 0)
            {
                baseHP = emeraldBaseHP / 2f,
                armor = 8f
            };
            NASItemProp emeraldLegs = new("Emerald Leggings|2|Θ", armorEnchants, NASMaterial.None, 0, 0)
            {
                baseHP = emeraldBaseHP / 2f,
                armor = 6f
            };
            NASItemProp emeraldBoots = new("Emerald Boots|2|Ω", bootEnchants, NASMaterial.None, 0, 0)
            {
                baseHP = emeraldBaseHP / 2f,
                armor = 3f
            };
        }
    }
}