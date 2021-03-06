﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;

namespace Vintagestory.GameContent
{
    public class BlockEntityTransient : BlockEntity
    {
        double lastCheckAtTotalDays = 0;
        double transitionHoursLeft = -1;

        public virtual int CheckIntervalMs { get; set; } = 2000;

        long listenerId;

        double? transitionAtTotalDaysOld = null; // old v1.12 data format, here for backwards compatibility

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);
            if (Block.Attributes?["inGameHours"].Exists != true) return;

            if (transitionHoursLeft <= 0)
            {
                transitionHoursLeft = Block.Attributes["inGameHours"].AsFloat(24);
            }

            if (api.Side == EnumAppSide.Server)
            {
                if (listenerId != 0)
                {
                    throw new InvalidOperationException("Initializing BETransient twice would create a memory and performance leak");
                }
                listenerId = RegisterGameTickListener(CheckTransition, CheckIntervalMs);

                if (transitionAtTotalDaysOld != null)
                {
                    transitionHoursLeft = ((double)transitionAtTotalDaysOld - Api.World.Calendar.TotalDays) * Api.World.Calendar.HoursPerDay;
                    lastCheckAtTotalDays = Api.World.Calendar.TotalDays;
                }
            }
        }

        public override void OnBlockPlaced(ItemStack byItemStack = null)
        {
            lastCheckAtTotalDays = Api.World.Calendar.TotalDays;
        }

        public virtual void CheckTransition(float dt)
        {
            Block block = Api.World.BlockAccessor.GetBlock(Pos);
            if (block.Attributes == null)
            {
                Api.World.Logger.Error("BETransient exiting at {0} cannot find block attributes. Will stop transient timer", Pos);
                UnregisterGameTickListener(listenerId);
                return;
            }

            // In case this block was imported from another older world. In that case lastCheckAtTotalDays would be a future date.
            lastCheckAtTotalDays = Math.Min(lastCheckAtTotalDays, Api.World.Calendar.TotalDays);


            while (Api.World.Calendar.TotalDays - lastCheckAtTotalDays > 1f / Api.World.Calendar.HoursPerDay)
            {
                lastCheckAtTotalDays += 1f / Api.World.Calendar.HoursPerDay;
                transitionHoursLeft -= 1f;

                ClimateCondition conds = Api.World.BlockAccessor.GetClimateAt(Pos, EnumGetClimateMode.ForSuppliedDateValues, lastCheckAtTotalDays);
                bool reset = conds.Temperature < Block.Attributes["resetBelowTemperature"].AsFloat(-999);
                bool stop = conds.Temperature < Block.Attributes["stopBelowTemperature"].AsFloat(-999);

                if (stop || reset)
                {
                    transitionHoursLeft += 1f;

                    if (reset)
                    {
                        transitionHoursLeft = Block.Attributes["inGameHours"].AsFloat(24);
                    }

                    continue;
                }

                if (transitionHoursLeft <= 0) { 
                    string toCode = block.Attributes["convertTo"].AsString();
                    tryTransition(toCode);
                    break;
                }
            }
        }

        public void tryTransition(string toCode) 
        { 
            Block block = Api.World.BlockAccessor.GetBlock(Pos);
            Block tblock;

            if (block.Attributes == null) return;

            string fromCode = block.Attributes["convertFrom"].AsString();
            if (fromCode == null || toCode == null) return;

            if (fromCode.IndexOf(":") == -1) fromCode = block.Code.Domain + ":" + fromCode;
            if (toCode.IndexOf(":") == -1) toCode = block.Code.Domain + ":" + toCode;


            if (fromCode == null || !toCode.Contains("*"))
            {
                tblock = Api.World.GetBlock(new AssetLocation(toCode));
                if (tblock == null) return;

                Api.World.BlockAccessor.SetBlock(tblock.BlockId, Pos);
                return;
            }
            
            AssetLocation blockCode = block.WildCardReplace(
                new AssetLocation(fromCode), 
                new AssetLocation(toCode)
            );

            tblock = Api.World.GetBlock(blockCode);
            if (tblock == null) return;

            Api.World.BlockAccessor.SetBlock(tblock.BlockId, Pos);
        }

        public override void FromTreeAtributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
        {
            base.FromTreeAtributes(tree, worldForResolving);

            transitionHoursLeft = tree.GetDouble("transitionHoursLeft");

            if (tree.HasAttribute("transitionAtTotalDays")) // Pre 1.13 format
            {
                transitionAtTotalDaysOld = tree.GetDouble("transitionAtTotalDays");
            }

            lastCheckAtTotalDays = tree.GetDouble("lastCheckAtTotalDays");
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);

            tree.SetDouble("transitionHoursLeft", transitionHoursLeft);
            tree.SetDouble("lastCheckAtTotalDays", lastCheckAtTotalDays);
        }


        public void SetPlaceTime(double totalHours)
        {
            Block block = Api.World.BlockAccessor.GetBlock(Pos);
            float hours = block.Attributes["inGameHours"].AsFloat(24);

            transitionHoursLeft = hours + totalHours - Api.World.Calendar.TotalHours;
        }

        public bool IsDueTransition()
        {
            return transitionHoursLeft <= 0;
        }
    }
}
