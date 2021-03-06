﻿using System;
using Microsoft.Xna.Framework;
using PortableStorage.TileEntities;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using TheOneLibrary.Base.UI;
using TheOneLibrary.Storage;
using TheOneLibrary.UI.Elements;
using TheOneLibrary.Utils;

namespace PortableStorage.UI
{
	public class QEChestUI : BaseUI, ITileEntityUI, IContainerUI
	{
		public UIText textLabel = new UIText("Quantum Entangled Chest");
		public UITextButton buttonClose = new UITextButton("X", 4);
		public UIGrid gridItems = new UIGrid(9);

		public TEQEChest qeChest;

		public UIColor[] colorFrequency = new UIColor[3];

		public override void OnInitialize()
		{
			panelMain.Width.Pixels = 408;
			panelMain.Height.Pixels = 172;
			panelMain.Center();
			Vector2? position = qeChest.UIPosition;
			if (position.HasValue)
			{
				panelMain.Left.Set(position.Value.X, 0f);
				panelMain.Top.Set(position.Value.Y, 0f);
			}

			panelMain.SetPadding(0);
			panelMain.BackgroundColor = TheOneLibrary.Utils.Utility.PanelColor;
			panelMain.OnMouseDown += DragStart;
			panelMain.OnMouseUp += DragEnd;
			Append(panelMain);

			textLabel.HAlign = 0.5f;
			textLabel.Top.Pixels = 8;
			panelMain.Append(textLabel);

			for (int i = 0; i < colorFrequency.Length; i++)
			{
				colorFrequency[i] = new UIColor(Color.White);
				colorFrequency[i].Width.Pixels = 16;
				colorFrequency[i].Height.Pixels = 16;
				colorFrequency[i].Left.Set(-52f - i * 20f, 1f);
				colorFrequency[i].Top.Pixels = 12;
				panelMain.Append(colorFrequency[i]);
			}

			buttonClose.Width.Pixels = 24;
			buttonClose.Height.Pixels = 24;
			buttonClose.Left.Set(-28, 1);
			buttonClose.Top.Pixels = 8;
			buttonClose.OnClick += (evt, element) =>
			{
				PortableStorage.Instance.CloseUI(qeChest.ID);
				Main.PlaySound(SoundID.DD2_EtherianPortalOpen.WithVolume(0.5f));
			};
			panelMain.Append(buttonClose);

			gridItems.Width.Set(-16, 1);
			gridItems.Height.Set(-36, 1);
			gridItems.Left.Pixels = 8;
			gridItems.Top.Pixels = 36;
			gridItems.ListPadding = 4;
			gridItems.OverflowHidden = true;
			panelMain.Append(gridItems);
		}

		public override void Load()
		{
			gridItems.Clear();
			for (int i = 0; i < qeChest.GetItems().Count; i++)
			{
				UIContainerSlot slot = new UIContainerSlot(qeChest, i);
				gridItems.Add(slot);
			}
		}

		public override void Unload()
		{
			qeChest.UIPosition = panelMain.GetDimensions().Position();
		}

		public override void Update(GameTime gameTime)
		{
			for (int i = 0; i < colorFrequency.Length; i++) colorFrequency[2 - i].color = typeof(Color).GetValue<Color>(Enum.GetName(typeof(Colors), qeChest.frequency.Colors[i]));

			base.Update(gameTime);
		}

		public void SetTileEntity(ModTileEntity tileEntity) => qeChest = (TEQEChest)tileEntity;

		public void SetContainer(IContainer container) => qeChest = (TEQEChest)container;

		public IContainer GetContainer() => qeChest;
	}
}