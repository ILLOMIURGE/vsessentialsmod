﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;

namespace Vintagestory.ServerMods.NoObf
{
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class CollectibleType : RegistryObjectType
    {
        [JsonProperty]
        public float RenderAlphaTest = 0.01f;
        [JsonProperty]
        public int StorageFlags = 1;
        [JsonProperty]
        public int MaxStackSize = 1;
        [JsonProperty]
        public float AttackPower = 0.5f;
        [JsonProperty]
        public int Durability;
        [JsonProperty]
        public EnumItemDamageSource[] DamagedBy;
        [JsonProperty]
        public EnumTool? Tool = null;

        [JsonProperty]
        public float AttackRange = GlobalConstants.DefaultAttackRange;
        [JsonProperty]
        public Dictionary<EnumBlockMaterial, float> MiningSpeed;
        [JsonProperty]
        public int ToolTier;

        [JsonProperty]
        [Obsolete("Use tool tier")]
        public int MiningTier { get { return ToolTier; } set { ToolTier = value; } }

        [JsonProperty]
        public EnumMatterState MatterState = EnumMatterState.Solid;
        [JsonProperty]
        public HeldSounds HeldSounds;

        // Determines on whether an object floats on liquids or not
        // Water has a density of 1000
        [JsonProperty]
        public int MaterialDensity = 9999;

        [JsonProperty, JsonConverter(typeof(JsonAttributesConverter))]
        public JsonObject Attributes;

        [JsonProperty]
        public CompositeShape Shape = null;

        [JsonProperty]
        public ModelTransform GuiTransform;
        [JsonProperty]
        public ModelTransform FpHandTransform;
        [JsonProperty]
        public ModelTransform TpHandTransform;
        [JsonProperty]
        public ModelTransform GroundTransform;

        [JsonProperty]
        public CompositeTexture Texture;
        [JsonProperty]
        public Dictionary<string, CompositeTexture> Textures = new Dictionary<string, CompositeTexture>();

        [JsonProperty]
        public CombustibleProperties CombustibleProps = null;
        [JsonProperty]
        public FoodNutritionProperties NutritionProps = null;
        [JsonProperty]
        public TransitionableProperties[] TransitionableProps = null;
        [JsonProperty]
        public GrindingProperties GrindingProps = null;

        [JsonProperty]
        public bool LiquidSelectable = false;

        [JsonProperty]
        public Dictionary<string, string[]> CreativeInventory = new Dictionary<string, string[]>();

        [JsonProperty]
        public CreativeTabAndStackList[] CreativeInventoryStacks;

        [JsonProperty]
        public string HeldTpHitAnimation = "breakhand";

        [JsonProperty]
        public string HeldRightTpIdleAnimation;

        [JsonProperty]
        public string HeldLeftTpIdleAnimation;

        [JsonProperty("heldTpIdleAnimation")]
#pragma warning disable IDE0044 // Add readonly modifier
        private string HeldOldTpIdleAnimation;
#pragma warning restore IDE0044 // Add readonly modifier

        [JsonProperty]
        public string HeldTpUseAnimation = "placeblock";


        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            OnDeserialized();
        }

        virtual internal void OnDeserialized()
        {
            GuiTransform.EnsureDefaultValues();
            FpHandTransform.EnsureDefaultValues();
            TpHandTransform.EnsureDefaultValues();
            GroundTransform.EnsureDefaultValues();

            if (Texture != null)
            {
                Textures["all"] = Texture;
            }

            if (HeldOldTpIdleAnimation != null && HeldRightTpIdleAnimation == null)
            {
                HeldRightTpIdleAnimation = HeldOldTpIdleAnimation;
            }
        }
    }
}
