#if NAS && TEN_BIT_BLOCKS
using System.Collections.Generic;
namespace NotAwesomeSurvival
{
    public partial class ItemProp
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
        public static ItemProp bedrockPick,etheriumPick,
            bedrockSword, etheriumHelmet, etheriumChest, 
            etheriumLegs, etheriumBoots, etheriumSword;
        public static void Setup()
        {
            bedrockPick = new("Bedrock Pickaxe|m|╟", nothing, NasBlock.Material.None, -1f, 5)
            {
                baseHP = int.MaxValue
            };
            bedrockSword = new("Bedrock Sword|0|α", swordEnchants, NasBlock.Material.Leaves, 1f, 6)
            {
                damage = 50f,
                knockback = 2f,
                recharge = 1,
                baseHP = int.MaxValue
            };
            etheriumSword = new("Etherium Sword|`|α", swordEnchants, NasBlock.Material.Leaves, 1f, 6)
            {
                damage = int.MaxValue,
                knockback = 0f,
                recharge = 0,
                baseHP = int.MaxValue
            };
            etheriumPick = new("Etherium Pickaxe|`|ß", toolEnchants, NasBlock.Material.Stone, 1f, 6)
            {
                baseHP = int.MaxValue
            };
            etheriumPick.materialsEffectiveAgainst.Add(NasBlock.Material.Stone);
            etheriumPick.materialsEffectiveAgainst.Add(NasBlock.Material.Earth);
            etheriumPick.materialsEffectiveAgainst.Add(NasBlock.Material.Wood);
            etheriumPick.materialsEffectiveAgainst.Add(NasBlock.Material.Plant);
            etheriumPick.materialsEffectiveAgainst.Add(NasBlock.Material.Leaves);
            etheriumPick.materialsEffectiveAgainst.Add(NasBlock.Material.Organic);
            etheriumPick.materialsEffectiveAgainst.Add(NasBlock.Material.Glass);
            etheriumPick.materialsEffectiveAgainst.Add(NasBlock.Material.Metal);
            etheriumPick.materialsEffectiveAgainst.Add(NasBlock.Material.Liquid);
            etheriumPick.materialsEffectiveAgainst.Add(NasBlock.Material.Lava);
            etheriumChest = new("Etherium Chestplate|`|Φ", armorEnchants, NasBlock.Material.None, 0, 0)
            {
                baseHP = int.MaxValue,
                armor = int.MaxValue
            };
            etheriumHelmet = new("Etherium Helmet|`|τ", helmetEnchants, NasBlock.Material.None, 0, 0)
            {
                baseHP = int.MaxValue,
                armor = int.MaxValue
            };
            etheriumLegs = new("Etherium Leggings|`|Θ", armorEnchants, NasBlock.Material.None, 0, 0)
            {
                baseHP = int.MaxValue,
                armor = int.MaxValue
            };
            etheriumBoots = new("Etherium Boots|`|Ω", bootEnchants, NasBlock.Material.None, 0, 0)
            {
                baseHP = int.MaxValue,
                armor = int.MaxValue
            };
            ItemProp fist = new("Fist|f|¬", nothing, NasBlock.Material.None, 0, 0)
            {
                baseHP = int.MaxValue
            };
            Item.Fist = new("Fist");
            ItemProp key = new("Key|f|σ", nothing, NasBlock.Material.None, 0, 0)
            {
                baseHP = int.MaxValue
            };
            ItemProp fishing = new("Fishing Rod|f|δ", nothing, NasBlock.Material.None, 0, 0)
            {
                baseHP = 200 * 7,
                damage = 0f,
                knockback = -1f
            };
            ItemProp shears = new("Shears|f|µ", nothing, NasBlock.Material.Organic, 0.75f, 1)
            {
                baseHP = 200 * 8
            };
            ItemProp woodPick = new("Wood Pickaxe|s|ß", toolEnchants, NasBlock.Material.Stone, 0.0f, 1)
            {
                baseHP = 12
            };
            ItemProp stonePick = new("Stone Pickaxe|7|ß", toolEnchants, NasBlock.Material.Stone, 0.75f, 1),
                stoneShovel = new("Stone Shovel|7|Γ", toolEnchants, NasBlock.Material.Earth, 0.50f, 1),
                stoneAxe = new("Stone Axe|7|π", toolEnchants, NasBlock.Material.Wood, 0.60f, 1);
            stoneAxe.materialsEffectiveAgainst.Add(NasBlock.Material.Leaves);
            ItemProp stoneSword = new("Stone Sword|7|α", swordEnchants, NasBlock.Material.Leaves, 0.50f, 1)
            {
                damage = 2.5f,
                recharge = 750
            };
            const int ironBaseHP = 200 * 8;
            ItemProp ironPick = new("Iron Pickaxe|f|ß", toolEnchants, NasBlock.Material.Stone, 0.85f, 3)
            {
                baseHP = ironBaseHP
            };
            ItemProp ironShovel = new("Iron Shovel|f|Γ", toolEnchants, NasBlock.Material.Earth, 0.60f, 3)
            {
                baseHP = ironBaseHP
            };
            ItemProp ironAxe = new("Iron Axe|f|π", toolEnchants, NasBlock.Material.Wood, 0.75f, 3)
            {
                baseHP = ironBaseHP
            };
            ironAxe.materialsEffectiveAgainst.Add(NasBlock.Material.Leaves);
            ItemProp ironSword = new("Iron Sword|f|α", swordEnchants, NasBlock.Material.Leaves, 0.75f, 3)
            {
                damage = 3f,
                recharge = 750,
                knockback = 1.25f,
                baseHP = ironBaseHP
            };
            const int goldBaseHP = 200 * 64;
            ItemProp goldPick = new("Gold Pickaxe|6|ß", toolEnchants, NasBlock.Material.Stone, 0.90f, 3)
            {
                baseHP = goldBaseHP
            };
            ItemProp goldShovel = new("Gold Shovel|6|Γ", toolEnchants, NasBlock.Material.Earth, 0.85f, 3)
            {
                baseHP = goldBaseHP
            };
            ItemProp goldAxe = new("Gold Axe|6|π", toolEnchants, NasBlock.Material.Wood, 0.90f, 3)
            {
                baseHP = goldBaseHP
            };
            goldAxe.materialsEffectiveAgainst.Add(NasBlock.Material.Leaves);
            ItemProp goldSword = new("Gold Sword|6|α", swordEnchants, NasBlock.Material.Leaves, 0.85f, 3)
            {
                damage = 3f,
                recharge = 750,
                knockback = 1.25f,
                baseHP = goldBaseHP
            };
            const int diamondBaseHP = 200 * 128;
            ItemProp diamondPick = new("Diamond Pickaxe|b|ß", toolEnchants, NasBlock.Material.Stone, 0.95f, 4)
            {
                baseHP = diamondBaseHP
            };
            ItemProp diamondShovel = new("Diamond Shovel|b|Γ", toolEnchants, NasBlock.Material.Earth, 1f, 4)
            {
                baseHP = diamondBaseHP
            };
            ItemProp diamondAxe = new("Diamond Axe|b|π", toolEnchants, NasBlock.Material.Wood, 0.95f, 4)
            {
                baseHP = diamondBaseHP
            };
            diamondAxe.materialsEffectiveAgainst.Add(NasBlock.Material.Leaves);
            ItemProp diamondSword = new("Diamond Sword|b|α", swordEnchants, NasBlock.Material.Leaves, 1f, 4)
            {
                damage = 3.5f,
                recharge = 750,
                knockback = 1.25f,
                baseHP = diamondBaseHP
            };
            const int emeraldBaseHP = 200 * 192;
            ItemProp emeraldPick = new("Emerald Pickaxe|2|ß", toolEnchants, NasBlock.Material.Stone, 0.975f, 5)
            {
                baseHP = emeraldBaseHP
            };
            emeraldPick.allowedEnchants["Efficiency"] = true;
            ItemProp emeraldShovel = new("Emerald Shovel|2|Γ", toolEnchants, NasBlock.Material.Earth, 1f, 5)
            {
                baseHP = emeraldBaseHP
            };
            ItemProp emeraldAxe = new("Emerald Axe|2|π", toolEnchants, NasBlock.Material.Wood, 0.975f, 5)
            {
                baseHP = emeraldBaseHP
            };
            emeraldAxe.materialsEffectiveAgainst.Add(NasBlock.Material.Leaves);
            ItemProp emeraldSword = new("Emerald Sword|2|α", swordEnchants, NasBlock.Material.Leaves, 1f, 5)
            {
                damage = 4f,
                recharge = 750,
                knockback = 1.25f,
                baseHP = emeraldBaseHP
            };
            ItemProp ironHelmet = new("Iron Helmet|f|τ", helmetEnchants, NasBlock.Material.None, 0, 0)
            {
                baseHP = ironBaseHP / 2f,
                armor = 2f
            };
            ItemProp ironChest = new("Iron Chestplate|f|Φ", armorEnchants, NasBlock.Material.None, 0, 0)
            {
                baseHP = ironBaseHP / 2f,
                armor = 5f
            };
            ItemProp ironLegs = new("Iron Leggings|f|Θ", armorEnchants, NasBlock.Material.None, 0, 0)
            {
                baseHP = ironBaseHP / 2f,
                armor = 4f
            };
            ItemProp ironBoots = new("Iron Boots|f|Ω", bootEnchants, NasBlock.Material.None, 0, 0)
            {
                baseHP = ironBaseHP / 2f,
                armor = 1f
            };
            ItemProp goldHelmet = new("Gold Helmet|6|τ", helmetEnchants, NasBlock.Material.None, 0, 0)
            {
                baseHP = goldBaseHP / 2f,
                armor = 2f
            };
            ItemProp goldChest = new("Gold Chestplate|6|Φ", armorEnchants, NasBlock.Material.None, 0, 0)
            {
                baseHP = goldBaseHP / 2f,
                armor = 6f
            };
            ItemProp goldLegs = new("Gold Leggings|6|Θ", armorEnchants, NasBlock.Material.None, 0, 0)
            {
                baseHP = goldBaseHP / 2f,
                armor = 5f
            };
            ItemProp goldBoots = new("Gold Boots|6|Ω", bootEnchants, NasBlock.Material.None, 0, 0)
            {
                baseHP = goldBaseHP / 2f,
                armor = 2f
            };
            ItemProp diamondHelmet = new("Diamond Helmet|b|τ", helmetEnchants, NasBlock.Material.None, 0, 0)
            {
                baseHP = diamondBaseHP / 2f,
                armor = 3f
            };
            ItemProp diamondChest = new("Diamond Chestplate|b|Φ", armorEnchants, NasBlock.Material.None, 0, 0)
            {
                baseHP = diamondBaseHP / 2f,
                armor = 7f
            };
            ItemProp diamondLegs = new("Diamond Leggings|b|Θ", armorEnchants, NasBlock.Material.None, 0, 0)
            {
                baseHP = diamondBaseHP / 2f,
                armor = 5f
            };
            ItemProp diamondBoots = new("Diamond Boots|b|Ω", bootEnchants, NasBlock.Material.None, 0, 0)
            {
                baseHP = diamondBaseHP / 2f,
                armor = 3f
            };
            ItemProp emeraldHelmet = new("Emerald Helmet|2|τ", helmetEnchants, NasBlock.Material.None, 0, 0)
            {
                baseHP = emeraldBaseHP / 2f,
                armor = 3f
            };
            ItemProp emeraldChest = new("Emerald Chestplate|2|Φ", armorEnchants, NasBlock.Material.None, 0, 0)
            {
                baseHP = emeraldBaseHP / 2f,
                armor = 8f
            };
            ItemProp emeraldLegs = new("Emerald Leggings|2|Θ", armorEnchants, NasBlock.Material.None, 0, 0)
            {
                baseHP = emeraldBaseHP / 2f,
                armor = 6f
            };
            ItemProp emeraldBoots = new("Emerald Boots|2|Ω", bootEnchants, NasBlock.Material.None, 0, 0)
            {
                baseHP = emeraldBaseHP / 2f,
                armor = 3f
            };
        }
    }
}
#endif