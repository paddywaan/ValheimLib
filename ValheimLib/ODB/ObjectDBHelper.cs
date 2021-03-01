﻿using System;
using System.Collections.Generic;

namespace ValheimLib.ODB
{
    public static class ObjectDBHelper
    {
        private static readonly List<CustomItem> CustomItems = new List<CustomItem>();
        private static readonly List<CustomRecipe> CustomRecipes = new List<CustomRecipe>();
        private static readonly List<CustomStatusEffect> CustomStatusEffects = new List<CustomStatusEffect>();

        /// <summary>
        /// Event that get fired after the ObjectDB get init and filled with custom items.
        /// Your code will execute once unless you resub, the event get cleared after each fire.
        /// </summary>
        public static Action OnAfterInit;

        public static void Init()
        {
            On.ObjectDB.Awake += AddCustomData;

            ItemDropMockFix.Switch(true);
        }

        internal static bool IsValid(this ObjectDB self)
        {
            return self.m_items.Count > 0;
        }

        public static bool Add(CustomItem customItem)
        {
            if (customItem.IsValid())
            {
                CustomItems.Add(customItem);

                return true;
            }

            return false;
        }

        public static bool Add(CustomRecipe customRecipe)
        {
            CustomRecipes.Add(customRecipe);

            return true;
        }

        public static bool Add(CustomStatusEffect customStatusEffect)
        {
            CustomStatusEffects.Add(customStatusEffect);

            return true;
        }

        private static void AddCustomItems(this ObjectDB self)
        {
            foreach (var customItem in CustomItems)
            {
                var itemDrop = customItem.ItemDrop;
                if (customItem.FixReference)
                {
                    itemDrop.m_itemData.m_shared.FixReferences();
                    customItem.FixReference = false;
                }

                self.m_items.Add(customItem.ItemPrefab);
                Log.LogInfo($"Added custom item : {customItem.ItemDrop.TokenName()}");
            }
        }

        private static void AddCustomRecipes(this ObjectDB self)
        {
            foreach (var customRecipe in CustomRecipes)
            {
                var recipe = customRecipe.Recipe;

                if (customRecipe.FixReference)
                {
                    recipe.FixReferences();
                    customRecipe.FixReference = false;
                }
                
                if (customRecipe.FixRequirementReferences)
                {
                    foreach (var requirement in recipe.m_resources)
                    {
                        requirement.FixReferences();
                    }

                    customRecipe.FixRequirementReferences = false;
                }

                self.m_recipes.Add(recipe);
                Log.LogInfo($"Added recipe for : {recipe.m_item.TokenName()}");
            }
        }

        private static void AddCustomStatusEffects(this ObjectDB self)
        {
            foreach (var customStatusEffect in CustomStatusEffects)
            {
                var statusEffect = customStatusEffect.StatusEffect;
                if (customStatusEffect.FixReference)
                {
                    statusEffect.FixReferences();
                    customStatusEffect.FixReference = false;
                }

                self.m_StatusEffects.Add(statusEffect);
                Log.LogInfo($"Added status effect : {statusEffect.m_name}");
            }
        }

        private static void AddCustomData(On.ObjectDB.orig_Awake orig, ObjectDB self)
        {
            var isValid = self.IsValid();
            ItemDropMockFix.Switch(!isValid);

            orig(self);

            if (isValid)
            {
                self.AddCustomItems();
                self.AddCustomRecipes();
                self.AddCustomStatusEffects();

                self.UpdateItemHashes();

                OnAfterInit?.Invoke();
                OnAfterInit = null;
            }
        }
    }

    internal static class ItemDropMockFix
    {
        private static bool _enabled;

        internal static void Switch(bool enable)
        {
            if (enable)
            {
                if (!_enabled)
                {
                    On.ItemDrop.Awake += Hook;
                    _enabled = enable;
                }
            }
            else
            {
                On.ItemDrop.Awake -= Hook;
                _enabled = enable;
            }
        }

        private static void Hook(On.ItemDrop.orig_Awake orig, ItemDrop self)
        {
            self.m_myIndex = ItemDrop.m_instances.Count;
            ItemDrop.m_instances.Add(self);
        }
    }
}