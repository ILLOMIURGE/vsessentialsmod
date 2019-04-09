﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

namespace Vintagestory.GameContent
{
    public class GuiDialogCarcassContents : GuiDialog
    {
        public override string ToggleKeyCombinationCode => null;

        InventoryGeneric inv;
        EntityAgent owningEntity;

        protected double FloatyDialogPosition => 0.6;
        protected double FloatyDialogAlign => 0.8;

        public GuiDialogCarcassContents(InventoryGeneric inv, EntityAgent owningEntity, ICoreClientAPI capi) : base(capi)
        {
            this.inv = inv;
            this.owningEntity = owningEntity;
            

            double pad = GuiElementItemSlotGrid.unscaledSlotPadding;

            int rows = (int)Math.Ceiling(inv.Count / 4f);

            ElementBounds slotBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, pad, 40 + pad, 4, rows).FixedGrow(2 * pad, 2 * pad);

            ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
            bgBounds.BothSizing = ElementSizing.FitToChildren;

            ElementBounds dialogBounds = ElementStdBounds
                .AutosizedMainDialog.WithAlignment(EnumDialogArea.RightMiddle)
                .WithFixedAlignmentOffset(-GuiStyle.DialogToScreenPadding, 0);


            SingleComposer =
                capi.Gui
                .CreateCompo("carcasscontents" + owningEntity.EntityId, dialogBounds)
                .AddShadedDialogBG(bgBounds, true)
                .AddDialogTitleBar("Contents", OnTitleBarClose)
                .BeginChildElements(bgBounds)
                    .AddItemSlotGrid(inv, DoSendPacket, 4, slotBounds, "slots")
                .EndChildElements()
                .Compose()
            ;
        }
        
        
        private void DoSendPacket(object p)
        {
            capi.Network.SendEntityPacket(owningEntity.EntityId, p);
        }

        private void OnTitleBarClose()
        {
            TryClose();
        }

        public override void OnGuiOpened()
        {
            base.OnGuiOpened();
        }

        public override void OnGuiClosed()
        {
            base.OnGuiClosed();

            capi.Network.SendPacketClient(capi.World.Player.InventoryManager.CloseInventory(inv));
            SingleComposer.GetSlotGrid("slots").OnGuiClosed(capi);
        }

        // TODO: Fix code duplication from GuiDialogBlockEntity
        /// <summary>
        /// Render's the object in Orthographic mode.
        /// </summary>
        /// <param name="deltaTime">The time elapsed.</param>
        public override void OnRenderGUI(float deltaTime)
        {
            if (capi.Settings.Bool["immersiveMouseMode"])
            {
                EntityPlayer entityPlayer = capi.World.Player.Entity;

                double offX = owningEntity.CollisionBox.X2 - owningEntity.OriginCollisionBox.X2;
                double offZ = owningEntity.CollisionBox.Z2 - owningEntity.OriginCollisionBox.Z2;

                Vec3d aboveHeadPos = new Vec3d(owningEntity.Pos.X + offX, owningEntity.Pos.Y + FloatyDialogPosition, owningEntity.Pos.Z + offZ);
                Vec3d pos = MatrixToolsd.Project(aboveHeadPos, capi.Render.PerspectiveProjectionMat, capi.Render.PerspectiveViewMat, capi.Render.FrameWidth, capi.Render.FrameHeight);

                // Z negative seems to indicate that the name tag is behind us \o/
                if (pos.Z < 0) return;

                SingleComposer.Bounds.Alignment = EnumDialogArea.None;
                SingleComposer.Bounds.fixedOffsetX = 0;
                SingleComposer.Bounds.fixedOffsetY = 0;
                SingleComposer.Bounds.absFixedX = pos.X - SingleComposer.Bounds.OuterWidth / 2;
                SingleComposer.Bounds.absFixedY = capi.Render.FrameHeight - pos.Y - SingleComposer.Bounds.OuterHeight * FloatyDialogAlign;
                SingleComposer.Bounds.absMarginX = 0;
                SingleComposer.Bounds.absMarginY = 0;
            }

            base.OnRenderGUI(deltaTime);
        }

        public override bool RequiresUngrabbedMouse()
        {
            return false;
        }
    }

}
