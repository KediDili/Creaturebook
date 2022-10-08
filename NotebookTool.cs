using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley.Tools;
using System.Runtime;
using StardewValley;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Utilities;
using StardewModdingAPI;
using System.IO;
using System.Xml.Serialization;

namespace Creaturebook
{
    [XmlType("Mods_KediDili_NotebookTool")]
    public class NotebookTool : Tool
    {
        private Lazy<Texture2D> texture = new(() =>
        {
            return Game1.content.Load<Texture2D>(Path.Combine("KediDili.Creaturebook", "NoteItem"));
        });

        public NotebookTool() : base("NotebookTool", 0, 245, 0, false, numAttachmentSlots: 1)
        {                                         //245 is empty space on tools spritesheet, useful if you don't want your tool to be sprited

        }
        public override Item getOne()
        {
            return new NotebookTool();
        }

        protected override string loadDisplayName()
        {
            return ModEntry.Helper.Translation.Get("CB.Notebook.Item.Name");
        }

        protected override string loadDescription()
        {
            return ModEntry.Helper.Translation.Get("CB.Notebook.Item.Desc");
        }

        //who.FarmerSprite.StopAnimation(); method yeets farmer animation with tool, useful if you don't want your tool to have special farmer animation
        public override void endUsing(GameLocation location, Farmer who)
        {
            who.FarmerSprite.StopAnimation();
        }
        public override int attachmentSlots()
        {
            return 1;
        }

        public override bool onRelease(GameLocation location, int x, int y, Farmer who)
        {
            return true;
        }
        public override void drawAttachments(SpriteBatch b, int x, int y)
        {
            if (attachments[0] == null)
            {
                b.Draw(Game1.menuTexture, new Vector2(x + 5, y), Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 43), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.86f);
            }
            else
            {
                b.Draw(Game1.menuTexture, new Vector2(x + 5, y), Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 10), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.86f);
                attachments[0].drawInMenu(b, new Vector2(x + 5, y), 1f);
            }
        }

        public override StardewValley.Object attach(StardewValley.Object o)
        {
            if (attachments[0] == null)
            {
                attachments[0] = o;
                return null;
            }
            else
            {
                StardewValley.Object attachedObject = attachments[0];
                attachments[0] = null;
                return attachedObject;
            }
        }

        public override bool canThisBeAttached(StardewValley.Object o)
        {
            return true;
        }

        public override bool beginUsing(GameLocation location, int x, int y, Farmer who)
        {
            return base.beginUsing(location, x, y, who);
        }

        public override void DoFunction(GameLocation location, int x, int y, int power, Farmer who)
        {
            SDate CurrentDate = SDate.Now();
            string convertedCurrentDate = CurrentDate.DaysSinceStart.ToString();
            string hudMessage = ModEntry.Helper.Translation.Get("CB.discoveredHUDMessage");
            string hudMessage_AlreadyDiscovered = ModEntry.Helper.Translation.Get("CB.discoveredHUDMessage.Already");
            var mousePos = ModEntry.Helper.Input.GetCursorPosition().GrabTile;
            foreach (var Characters in Game1.currentLocation.characters)
            {
                foreach (var chapter in ModEntry.Chapters)
                {
                    for (int i = 0; i < chapter.Creatures.Count; i++)
                    {
                        ModEntry.monitor.Log("Yes this code is being run 1st method", LogLevel.Info);
                        string ID = Convert.ToString(chapter.Creatures[i].ID);

                        if ((Characters.Name.Equals(chapter.CreatureNamePrefix + "_" + ID) || chapter.Creatures[i].OverrideDefaultNaming.Contains(Characters.Name)) && Characters.getTileLocation() == mousePos && Game1.player.modData[ModEntry.MyModID + "_IsNotebookObtained"] == "true")
                        {
                            if (Game1.player.modData[ModEntry.MyModID + "_" + chapter.FromContentPack.Manifest.UniqueID + "." + chapter.CreatureNamePrefix + "_" + ID] == "null")
                            {
                                Game1.player.modData.Add(ModEntry.MyModID + "_" + chapter.FromContentPack.Manifest.UniqueID + "." + chapter.CreatureNamePrefix + "_" + ID, convertedCurrentDate);
                                Game1.addHUDMessage(new HUDMessage(hudMessage + chapter.Creatures[i].Name, 1));
                                return;
                            }
                            else if (Game1.player.modData[ModEntry.MyModID + "_" + chapter.FromContentPack.Manifest.UniqueID + "." + chapter.CreatureNamePrefix + "_" + ID] != "null")
                            {
                                Game1.addHUDMessage(new HUDMessage(hudMessage_AlreadyDiscovered, 1));
                                return;
                            }
                        }
                        else if (attachments[0] != null)
                        {
                            ModEntry.monitor.Log("Yes this code is being run 2nd method", LogLevel.Info);
                            if (attachments[0].ParentSheetIndex == chapter.Creatures[i].UseThisItem)
                            {
                                if (Game1.player.modData[ModEntry.MyModID + "_" + chapter.FromContentPack.Manifest.UniqueID + "." + chapter.CreatureNamePrefix + "_" + ID] == "null")
                                {
                                    Game1.player.modData.Add(ModEntry.MyModID + "_" + chapter.FromContentPack.Manifest.UniqueID + "." + chapter.CreatureNamePrefix + "_" + ID, convertedCurrentDate);
                                    Game1.addHUDMessage(new HUDMessage(hudMessage + chapter.Creatures[i].Name, 1));
                                    return;
                                }
                                else if (Game1.player.modData[ModEntry.MyModID + "_" + chapter.FromContentPack.Manifest.UniqueID + "." + chapter.CreatureNamePrefix + "_" + ID] != "null")
                                {
                                    Game1.addHUDMessage(new HUDMessage(hudMessage_AlreadyDiscovered, 1));
                                    return;
                                }
                            }
                            else if (chapter.EnableSets)
                            {
                                for (int l = 0; l < chapter.Sets.Count; l++)
                                {
                                    if (attachments[0].ParentSheetIndex == chapter.Sets[l].DiscoverWithThisItem && 0 != chapter.Sets[l].DiscoverWithThisItem)
                                    {
                                        int random2 = Game1.random.Next(chapter.Sets[l].CreaturesBelongingToThisSet.Length);
                                        if (Game1.player.modData[chapter.FromContentPack.Manifest.UniqueID + "." + chapter.CreatureNamePrefix + "_" + chapter.Sets[l].CreaturesBelongingToThisSet[random2]] == "null")
                                        {
                                            Game1.player.modData[chapter.FromContentPack.Manifest.UniqueID + "." + chapter.CreatureNamePrefix + "_" + chapter.Sets[l].CreaturesBelongingToThisSet[random2]] = convertedCurrentDate;
                                            Game1.addHUDMessage(new HUDMessage(hudMessage + chapter.Creatures[random2].Name, 1));
                                            attachments[0] = null;
                                            return;
                                        }
                                    }
                                }
                            }
                        }
                        foreach (var layer in Game1.currentLocation.Map.Layers)
                        {
                            foreach (var tiles in layer.Tiles.Array)
                            {
                                if (tiles is null)
                                    return;
                                foreach (var property in tiles.Properties)
                                {
                                    ModEntry.monitor.Log("Yes this code is being run 3rd method", LogLevel.Info);
                                    if (layer.Id == "Back" && property.Key == "Creaturebook" && property.Value.ToString().StartsWith("Discover"))
                                    {
                                        if (Game1.player.modData[ModEntry.MyModID + "_" + property.Value.ToString()[8..]] == "null")
                                        {
                                            Game1.player.modData[ModEntry.MyModID + "_" + property.Value.ToString()[8..]] = convertedCurrentDate;
                                            Game1.addHUDMessage(new HUDMessage(hudMessage + chapter.Creatures[i].Name, 1));
                                            return;
                                        }
                                        else
                                        {
                                            Game1.addHUDMessage(new HUDMessage(hudMessage_AlreadyDiscovered, 1));
                                            return;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        } //new Rectangle(108, 48, 64, 32)

        public override void draw(SpriteBatch b)
        {

        }

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
        {
            spriteBatch.Draw(texture.Value, location + new Vector2(32f, 32f), null, color * transparency, 0f, new Vector2(8f, 8f), 4f * scaleSize, SpriteEffects.None, layerDepth);

            if (attachments.Count != 0)
            {

            }
        }
    }
}
