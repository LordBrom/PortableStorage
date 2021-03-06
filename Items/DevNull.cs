using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PortableStorage.UI;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI;
using Terraria.UI.Chat;
using static TheOneLibrary.Utils.Utility;

namespace PortableStorage.Items
{
	public class DevNull : BaseBag
	{
		public List<Item> Items = new List<Item>();

		public int selectedIndex = -1;

		public override string Texture => PortableStorage.ItemTexturePath + "DevNull";

		public override ModItem Clone(Item item)
		{
			DevNull clone = (DevNull)base.Clone(item);
			clone.Items = Items;
			clone.gui = gui;
			return clone;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("/dev/null");
			Tooltip.SetDefault("Can only store items that place tiles" +
			                   "\nWill automatically pick them up and void them when there's more than max stack of them" +
			                   "\nCan be used to place tiles when an item has been selected in the UI via rightclicking");
		}

		public override void SetDefaults()
		{
			if (!Items.Any())
			{
				for (int i = 0; i < 7; i++) Items.Add(new Item());
			}

			SetupUI<DevNullUI>();

			item.width = 40;
			item.height = 40;
			item.useTime = 5;
			item.useAnimation = 5;
			item.useStyle = 1;
			item.rare = 1;
			item.autoReuse = true;
			item.useTurn = true;
		}

		public override bool CanUseItem(Player player) => selectedIndex >= 0 && Items[selectedIndex].stack > 1;

		public void SetItem(int index)
		{
			if (index == -1)
			{
				selectedIndex = -1;
				item.placeStyle = -1;
				item.createTile = -1;
			}
			else
			{
				selectedIndex = index;
				Item selectedItem = Items[selectedIndex];

				if (selectedItem.createTile >= 0)
				{
					item.createTile = selectedItem.createTile;
					item.createWall = -1;
					item.placeStyle = selectedItem.placeStyle;
				}
			}
		}

		public override bool CanRightClick() => true;

		public override void RightClick(Player player)
		{
			item.stack++;

			if (player.whoAmI == Main.LocalPlayer.whoAmI) HandleUI();
		}

		public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
		{
			if (selectedIndex >= 0)
			{
				Item selectedItem = Items[selectedIndex];
				Texture2D itemTexture = Main.itemTexture[selectedItem.type];
				Rectangle rect = Main.itemAnimations[selectedItem.type] != null ? Main.itemAnimations[selectedItem.type].GetFrame(itemTexture) : itemTexture.Frame();
				Color newColor = Color.White;
				float pulseScale = 1f;
				ItemSlot.GetItemLight(ref newColor, ref pulseScale, selectedItem);
				int height = rect.Height;
				int width = rect.Width;
				float drawScale = 1f;
				float availableWidth = 32;
				if (width > availableWidth || height > availableWidth)
				{
					if (width > height) drawScale = availableWidth / width;
					else drawScale = availableWidth / height;
				}

				drawScale *= scale;
				Vector2 vector = new Vector2(40, 40) * scale;
				Vector2 position2 = position + vector / 2f - rect.Size() * drawScale / 2f;

				if (ItemLoader.PreDrawInInventory(selectedItem, spriteBatch, position2, rect, selectedItem.GetAlpha(newColor), selectedItem.GetColor(Color.White), origin, drawScale * pulseScale))
				{
					spriteBatch.Draw(itemTexture, position2, rect, selectedItem.GetAlpha(newColor), 0f, origin, drawScale * pulseScale, SpriteEffects.None, 0f);
					if (selectedItem.color != Color.Transparent) spriteBatch.Draw(itemTexture, position2, rect, selectedItem.GetColor(Color.White), 0f, origin, drawScale * pulseScale, SpriteEffects.None, 0f);
				}

				ItemLoader.PostDrawInInventory(selectedItem, spriteBatch, position2, rect, selectedItem.GetAlpha(newColor), selectedItem.GetColor(Color.White), origin, drawScale * pulseScale);
				if (ItemID.Sets.TrapSigned[selectedItem.type]) spriteBatch.Draw(Main.wireTexture, position + new Vector2(40f, 40f) * scale, new Rectangle(4, 58, 8, 8), Color.White, 0f, new Vector2(4f), 1f, SpriteEffects.None, 0f);
				if (selectedItem.stack > 1) ChatManager.DrawColorCodedStringWithShadow(spriteBatch, Main.fontItemStack, selectedItem.stack.ToSI("F0"), position + new Vector2(10f, 26f) * scale, Color.White, 0f, Vector2.Zero, new Vector2(scale));
				return false;
			}

			return true;
		}

		public override TagCompound Save()
		{
			TagCompound tag = new TagCompound();
			tag["Items"] = Items.Save();
			tag["SelectedItem"] = selectedIndex;
			if (gui != null) tag["UIPosition"] = gui.ui.panelMain.GetDimensions().Position();
			return tag;
		}

		public override void Load(TagCompound tag)
		{
			Items = TheOneLibrary.Utils.Utility.Load(tag);
			selectedIndex = tag.GetInt("SelectedItem");

			if (gui != null && tag.ContainsKey("UIPosition"))
			{
				Vector2 vector = tag.Get<Vector2>("UIPosition");
				gui.ui.panelMain.Left.Set(vector.X, 0f);
				gui.ui.panelMain.Top.Set(vector.Y, 0f);
				gui.ui.panelMain.Recalculate();
			}
		}

		public override void NetSend(BinaryWriter writer) => writer.Write(Items);

		public override void NetRecieve(BinaryReader reader) => Items = TheOneLibrary.Utils.Utility.Read(reader);

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.StoneBlock, 100);
			recipe.AddTile(TileID.WorkBenches);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}

		public override void OnCraft(Recipe recipe)
		{
			Items.Clear();
			for (int i = 0; i < 7; i++) Items.Add(new Item());
		}

		public override Item GetItem(int slot) => Items[slot];

		public override void SetItem(int slot, Item value) => Items[slot] = value;

		public override void Sync(int slot) => SyncItem(item);

		public override List<Item> GetItems() => Items;

		public override ModItem GetModItem() => this;
	}
}