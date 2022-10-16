using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using StardewModdingAPI.Utilities;
using System;
using System.IO;
using System.Collections.Generic;

namespace Creaturebook
{
    public class NotebookMenu : IClickableMenu
    {
        readonly ClickableTextureComponent RightArrow; readonly ClickableTextureComponent LeftArrow;
        readonly ClickableTextureComponent SearchButton; readonly ClickableTextureComponent CloseButton;
        readonly ClickableTextureComponent Button_1; readonly ClickableTextureComponent Button_2; readonly ClickableTextureComponent Button_3;

        ClickableComponent Sticky_Purple; ClickableComponent Sticky_Yellow;
        ClickableComponent Sticky_Blue; ClickableComponent Sticky_Green;

        readonly ClickableTextureComponent SwitchToNormal_1; readonly ClickableTextureComponent SwitchToNormal_2; readonly ClickableTextureComponent ShowSetView;

        internal static string[] PagesWithStickies = new string[4];

        public int currentID = 0;
        public int actualID = 0;
        public int currentChapter = 0;
        public int currentSetPage = 0;

        string fullCreatureID;
        string modID;

        readonly TextBox textBox;

        float Stickyrotation;

        // sets the corner, The lower Y, the lower left-up corner
        static readonly Vector2 TopLeftCorner = Utility.getTopLeftPositionForCenteringOnScreen(960, 520);

        bool willSearch = false;
        bool IsFirstActive = true;
        bool IsSecondActive = false;
        bool IsThirdActive = false;
        bool showSetPaging = ModEntry.Chapters[0].EnableSets;
        bool IsHeaderPage = true;
        bool WasHeaderPage = false;
        List<bool> pageOrder = new();

        readonly string unknownLabel;
        readonly string unknownDesc;

        string latinName;
        string description;
        string localizedName;
        string authorchapterTitle;

        readonly Texture2D NotebookTexture = ModEntry.Helper.GameContent.Load<Texture2D>(Path.Combine("KediDili.Creaturebook", "NotebookTexture"));
        readonly Texture2D ButtonTexture = ModEntry.Helper.GameContent.Load<Texture2D>(Path.Combine("KediDili.Creaturebook", "SearchButton"));
        readonly Texture2D Stickies = ModEntry.Helper.GameContent.Load<Texture2D>(Path.Combine("KediDili.Creaturebook", "Stickies"));

        Texture2D CreatureTexture;
        Texture2D CreatureTexture_2;
        Texture2D CreatureTexture_3;

        readonly string[] menuTexts;
        readonly Texture2D[] MenuTextures;
        public NotebookMenu()
        {
            CreatureTexture = ModEntry.Helper.GameContent.Load<Texture2D>(Path.Combine("KediDili.Creaturebook", ModEntry.Chapters[0].FromContentPack.Manifest.UniqueID + "." + ModEntry.Chapters[0].CreatureNamePrefix + "_" + ModEntry.Chapters[0].Creatures[0].ID + "_Image1"));

            MenuTextures = new Texture2D[] { NotebookTexture, CreatureTexture, ButtonTexture, CreatureTexture_2, CreatureTexture_3, Stickies };
            if (ModEntry.modConfig.EnableStickies)
            {
                PagesWithStickies = ModEntry.Helper.Data.ReadJsonFile<string[]>(PathUtilities.NormalizeAssetName("localData/stickies.json"));
                if (PagesWithStickies is null)
                {
                    PagesWithStickies = new string[4];
                    Sticky_Yellow = new ClickableComponent(new Rectangle((int)TopLeftCorner.X - 200, (int)TopLeftCorner.Y + 200, 240, 84), "");
                    Sticky_Green = new ClickableComponent(new Rectangle((int)TopLeftCorner.X - 200, (int)TopLeftCorner.Y + 260, 240, 84), "");
                    Sticky_Blue = new ClickableComponent(new Rectangle((int)TopLeftCorner.X - 200, (int)TopLeftCorner.Y + 320, 240, 84), "");
                    Sticky_Purple = new ClickableComponent(new Rectangle((int)TopLeftCorner.X - 200, (int)TopLeftCorner.Y + 380, 240, 84), "");
                }
                else
                {
                    Sticky_Yellow = PagesWithStickies[0] is null ? new ClickableComponent(new Rectangle((int)TopLeftCorner.X - 200, (int)TopLeftCorner.Y + 200, 240, 84), "") : new ClickableComponent(new Rectangle((int)TopLeftCorner.X - 150, (int)TopLeftCorner.Y + 200, 240, 84), "");
                    Sticky_Green = PagesWithStickies[1] is null ? new ClickableComponent(new Rectangle((int)TopLeftCorner.X - 200, (int)TopLeftCorner.Y + 260, 240, 84), "") : new ClickableComponent(new Rectangle((int)TopLeftCorner.X - 150, (int)TopLeftCorner.Y + 260, 240, 84), "");
                    Sticky_Blue = PagesWithStickies[2] is null ? new ClickableComponent(new Rectangle((int)TopLeftCorner.X - 200, (int)TopLeftCorner.Y + 320, 240, 84), "") : new ClickableComponent(new Rectangle((int)TopLeftCorner.X - 150, (int)TopLeftCorner.Y + 320, 240, 84), "");
                    Sticky_Purple = PagesWithStickies[3] is null ? new ClickableComponent(new Rectangle((int)TopLeftCorner.X - 200, (int)TopLeftCorner.Y + 380, 240, 84), "") : new ClickableComponent(new Rectangle((int)TopLeftCorner.X - 150, (int)TopLeftCorner.Y + 380, 240, 84), "");
                }

            }
            if (ModEntry.Chapters[0].Creatures[0].HasScientificName)
            latinName = ModEntry.Helper.Translation.Get("CB.LatinName") + ModEntry.Chapters[0].Creatures[0].ScientificName;
            else
            latinName = null;
            if (ModEntry.Chapters[0].Creatures[0].HasFunFact)
            description = ModEntry.Chapters[0].Creatures[0].Desc;
            if (ModEntry.Chapters[0].Creatures[0].HasExtraImages)
            {
                Button_2 = new ClickableTextureComponent(new Rectangle((int)TopLeftCorner.X + 50, (int)TopLeftCorner.Y + 50, 50, 50), Game1.mouseCursors, new Rectangle(528, 128, 8, 8), 4f);
                if (File.Exists(PathUtilities.NormalizeAssetName(ModEntry.Chapters[0].Creatures[0].Directory + "\\book-image_3.png")))
                Button_3 = new ClickableTextureComponent(new Rectangle((int)TopLeftCorner.X + 50, (int)TopLeftCorner.Y + 100, 50, 50), Game1.mouseCursors, new Rectangle(520, 128, 8, 8), 4f);
                else
                Button_3 = null;
            }
            else
            {
                Button_1 = null;
                Button_2 = null;
                Button_3 = null;
            }

            localizedName = ModEntry.Chapters[0].Creatures[0].Name;
            unknownLabel = ModEntry.Helper.Translation.Get("CB.UnknownLabel");
            unknownDesc = ModEntry.Helper.Translation.Get("CB.UnknownDesc");
            authorchapterTitle = ModEntry.Helper.Translation.Get("CB.Chapter") + "1: " + ModEntry.Chapters[0].Title + "\n" + ModEntry.Helper.Translation.Get("CB.ChapterAuthorBy") + ModEntry.Chapters[0].Author;
            
            RightArrow = new ClickableTextureComponent(new Rectangle((int)TopLeftCorner.X + 1282 - 372, (int)TopLeftCorner.Y + 718 - 242, 48, 44), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f);
            LeftArrow = new ClickableTextureComponent(new Rectangle((int)TopLeftCorner.X, (int)TopLeftCorner.Y + 718 - 242, 48, 44), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f);
            SearchButton = new ClickableTextureComponent(new Rectangle((int)TopLeftCorner.X + 982, (int)TopLeftCorner.Y + 718 - 220, 68, 64), MenuTextures[2], new Rectangle(0, 0, 17, 16), 4f);
            CloseButton = new ClickableTextureComponent(new Rectangle((int)TopLeftCorner.X + 982, (int)TopLeftCorner.Y, 48, 48), Game1.mouseCursors, new Rectangle(337, 494, 12, 12), 4f);
            ShowSetView = new ClickableTextureComponent(new Rectangle((int)TopLeftCorner.X + 982, (int)TopLeftCorner.Y + 200, 48, 48), Game1.mouseCursors, new Rectangle(337, 494, 12, 12), 4f);

            textBox = new TextBox(Game1.content.Load<Texture2D>(PathUtilities.NormalizePath("LooseSprites\\textBox")), null, Game1.smallFont, Color.Black)
            {
                X = (int)TopLeftCorner.X + 1282 - 300,
                Y = (int)TopLeftCorner.Y + 718 - 300,
                Width = (int)TopLeftCorner.X + 1282 - 250
            };

            menuTexts = new string[] { latinName, description, localizedName, unknownLabel, unknownDesc, authorchapterTitle };

            textBox.OnEnterPressed += TextBoxEnter;
            Game1.keyboardDispatcher.Subscriber = textBox;
            UpdateNotebookPage();
            if (ModEntry.Chapters[currentChapter].EnableSets)
            {
                SwitchToNormal_1 = new ClickableTextureComponent(new Rectangle((int)TopLeftCorner.X + 15, (int)TopLeftCorner.Y + 70, 64, 64), Game1.mouseCursors, new Rectangle(162, 440, 16, 16), 4f);
                SwitchToNormal_2 = new ClickableTextureComponent(new Rectangle((int)TopLeftCorner.X + 600, (int)TopLeftCorner.Y + 70, 64, 64), Game1.mouseCursors, new Rectangle(162, 440, 16, 16), 4f);
                CalculateSetPages(ModEntry.Chapters[0]);
            }
        }
        public override void draw(SpriteBatch b)
        {
            b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);

            if (PagesWithStickies[2] is not null && PagesWithStickies[2] != modID + "." + fullCreatureID && ModEntry.modConfig.EnableStickies)
            b.Draw(MenuTextures[5], new Vector2(Sticky_Blue.bounds.X, Sticky_Blue.bounds.Y), new Rectangle(0, 42, 60, 21), Color.White, Stickyrotation, Vector2.Zero, 3f, SpriteEffects.None, layerDepth: 0.5f);   
            
            b.Draw(MenuTextures[0], TopLeftCorner, null, Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, layerDepth: 0.5f);
            
            if (PagesWithStickies[2] == modID + "." + fullCreatureID || PagesWithStickies[2] is null && ModEntry.modConfig.EnableStickies)
            b.Draw(MenuTextures[5], new Vector2(Sticky_Blue.bounds.X, Sticky_Blue.bounds.Y), new Rectangle(0, 42, 60, 21), Color.White, Stickyrotation, Vector2.Zero, 3f, SpriteEffects.None, layerDepth: 0.5f);

            Sticky_Yellow.visible = true;
            Sticky_Green.visible = true;
            Sticky_Blue.visible = true;
            Sticky_Purple.visible = true;

            CloseButton.draw(b);
            if (!IsHeaderPage)
            {
                if (!showSetPaging)
                {
                    SearchButton.draw(b);
                    if (Game1.player.modData[ModEntry.MyModID + "_" + modID + "." + fullCreatureID] is not "null")
                    {
                        string Date = Game1.player.modData[ModEntry.MyModID + "_" + ModEntry.Chapters[currentChapter].FromContentPack.Manifest.UniqueID + "." + fullCreatureID];
                        int count = Convert.ToInt32(Date);
                        SDate convertedDate = SDate.FromDaysSinceStart(count);
                        string translatedDate = convertedDate.ToLocaleString();
                        string dateDiscovered = ModEntry.Helper.Translation.Get("CB.dateDiscovered") + translatedDate;
                        if (Button_2 is not null)
                        {
                            Button_1.draw(b);
                            Button_2.draw(b);
                            if (Button_3 != null)
                                Button_3.draw(b);
                        }
                        if (IsFirstActive)
                            b.Draw(MenuTextures[1], TopLeftCorner, null, Color.White, 0f, new Vector2(ModEntry.Chapters[currentChapter].Creatures[currentID].ImageOffsets[0].X, ModEntry.Chapters[currentChapter].Creatures[currentID].ImageOffsets[0].Y), ModEntry.Chapters[currentChapter].Creatures[currentID].ImageScales[0], SpriteEffects.None, layerDepth: 0.5f);

                        else if (IsSecondActive)
                            b.Draw(MenuTextures[2], TopLeftCorner, null, Color.White, 0f, new Vector2(ModEntry.Chapters[currentChapter].Creatures[currentID].ImageOffsets[1].X, ModEntry.Chapters[currentChapter].Creatures[currentID].ImageOffsets[1].Y), ModEntry.Chapters[currentChapter].Creatures[currentID].ImageScales[1], SpriteEffects.None, layerDepth: 0.5f);

                        else if (IsThirdActive)
                            b.Draw(MenuTextures[3], TopLeftCorner, null, Color.White, 0f, new Vector2(ModEntry.Chapters[currentChapter].Creatures[currentID].ImageOffsets[2].X, ModEntry.Chapters[currentChapter].Creatures[currentID].ImageOffsets[2].Y), ModEntry.Chapters[currentChapter].Creatures[currentID].ImageScales[2], SpriteEffects.None, layerDepth: 0.5f);

                        b.DrawString(Game1.smallFont, menuTexts[2], new Vector2(TopLeftCorner.X + 15, TopLeftCorner.Y + 310), Color.Black);

                        if (ModEntry.modConfig.ShowScientificNames && ModEntry.Chapters[currentChapter].Creatures[currentID].HasScientificName)
                            b.DrawString(Game1.smallFont, menuTexts[0], new Vector2(TopLeftCorner.X + 15, TopLeftCorner.Y + 350), Color.Black);

                        if (ModEntry.modConfig.ShowDiscoveryDates)
                            b.DrawString(Game1.smallFont, dateDiscovered, new Vector2(TopLeftCorner.X + 15, TopLeftCorner.Y + 390), Color.Black);

                        if (ModEntry.Chapters[currentChapter].Creatures[currentID].HasFunFact)
                            SpriteText.drawString(b, menuTexts[1], (int)TopLeftCorner.X + 910 - 371, (int)TopLeftCorner.Y + 254 - 230, width: 420, height: 490);
                    }
                    else if (Game1.player.modData[ModEntry.MyModID + "_" + modID + "." + fullCreatureID] is "null")
                    {
                        b.Draw(CreatureTexture, TopLeftCorner, null, Color.Black * 0.8f, 0f, new Vector2(ModEntry.Chapters[currentChapter].Creatures[currentID].ImageOffsets[0].X, ModEntry.Chapters[currentChapter].Creatures[currentID].ImageOffsets[0].Y), ModEntry.Chapters[currentChapter].Creatures[currentID].ImageScales[0], SpriteEffects.None, layerDepth: 0.5f);
                        b.DrawString(Game1.smallFont, menuTexts[3], new Vector2(TopLeftCorner.X + 15, TopLeftCorner.Y + 310), Color.Black);
                        b.DrawString(Game1.smallFont, menuTexts[4], new Vector2(TopLeftCorner.X + 15, TopLeftCorner.Y + 350), Color.Black);
                    }
                }
                else
                {
                     if ((!pageOrder[currentSetPage] && currentSetPage == pageOrder.Count - 1) ||(!pageOrder[currentSetPage] && !pageOrder[currentSetPage + 1]))
                        DrawCreatureIcons(b, false, currentSetPage);
                        
                     else if ((!pageOrder[currentSetPage] && pageOrder[currentSetPage + 1]) || pageOrder[currentSetPage])
                        DrawCreatureIcons(b, true, currentSetPage);
                }
            }
            /*else
            {
                if (ModEntry.Chapters[currentChapter].EnableSets)
                    b.DrawString(Game1.smallFont, menuTexts[7], new Vector2(TopLeftCorner.X + 910 - 371, TopLeftCorner.Y + 100), Color.Black);
                
                else
                    b.DrawString(Game1.smallFont, menuTexts[6], new Vector2(TopLeftCorner.X + 910 - 371, TopLeftCorner.Y + 100), Color.Black);
                
                SpriteText.drawString(b, menuTexts[5], (int)TopLeftCorner.X + 30, (int)TopLeftCorner.Y + 54, width: 420, height: 490, scroll_text_alignment: SpriteText.ScrollTextAlignment.Center);
            }*/
            if (actualID == 0 && IsHeaderPage && currentID == 0)
            {
                LeftArrow.visible = false;
                RightArrow.draw(b);
            }
            else if (actualID == ModEntry.Chapters[currentChapter].Creatures.Count - 1)
            {
                RightArrow.visible = false;
                LeftArrow.draw(b);
            }
            else
            {
                LeftArrow.visible = true;
                RightArrow.visible = true;
                LeftArrow.draw(b);
                RightArrow.draw(b);
            }

            if (willSearch)
                textBox.Draw(b);
            drawMouse(b);
        }
        public void TextBoxEnter(TextBox sender)
        {
            if (sender.Text.Length >= 1 && willSearch)
            {
                int result = -1;
                if (int.TryParse(sender.Text, out result))
                {
                    if (-1 < Convert.ToInt32(sender.Text) && Convert.ToInt32(sender.Text) < ModEntry.Chapters[currentChapter].Creatures.Count)
                    {
                        currentID = Convert.ToInt32(sender.Text);
                        willSearch = false;
                        textBox.Text = "";
                        
                        if (currentChapter is 0 && currentID != 0)
                        actualID = currentID;
                        
                        else if (currentChapter is 0 && currentID == 0)
                        actualID = 0;
                        
                        else
                        {
                            actualID = 0;
                            for (int i = 0; i < currentChapter - 1; i++)
                            actualID += ModEntry.Chapters[i].Creatures.Count;
                            actualID += currentID + ModEntry.Chapters.Count - 1;
                        }
                        UpdateNotebookPage();
                    }
                }
                else if (!int.TryParse(sender.Text, out result))
                {
                    foreach (var item in ModEntry.Chapters[currentChapter].Creatures)
                    {
                        if (!string.IsNullOrEmpty(item.ScientificName))
                        {
                            if (item.ScientificName.StartsWith(sender.Text, StringComparison.OrdinalIgnoreCase))
                            {
                                currentID = item.ID;
                                willSearch = false;
                                textBox.Text = "";
                                
                                if (currentChapter is 0 && currentID != 0)
                                actualID = currentID;
                                
                                else if (currentChapter is 0 && currentID == 0)
                                actualID = 0;
                                
                                else if (currentChapter > 0 && currentID > 0)
                                {
                                    actualID = 0;
                                    for (int a = 0; a < currentChapter - 1; a++)
                                    actualID += ModEntry.Chapters[a].Creatures.Count;
                                    actualID += currentID + ModEntry.Chapters.Count - 1;
                                }
                                UpdateNotebookPage();
                                break;
                            }
                        }
                        if (item.Name.StartsWith(sender.Text, StringComparison.OrdinalIgnoreCase))
                        {
                            currentID = item.ID;
                            willSearch = false;
                            textBox.Text = "";

                            if (currentChapter is 0 && currentID != 0)
                            actualID = currentID;
                            
                            else if (currentChapter is 0 && currentID == 0)
                            actualID = 0;
                            
                            else if (currentChapter > 0 && currentID > 0)
                            {
                                actualID = 0;
                                for (int a = 0; a < currentChapter - 1; a++)
                                actualID += ModEntry.Chapters[a].Creatures.Count;
                                actualID += currentID + ModEntry.Chapters.Count - 1;
                            }
                            UpdateNotebookPage();
                            break;
                        }
                    }
                }
            }
        }
        public override void receiveKeyPress(Keys key) { }
            //Leave this here and empty so M and E buttons don't yeet your menu
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (Sticky_Blue.containsPoint(x, y))
                UpdateStickies(2);
            if (SwitchToNormal_1.containsPoint(x , y) && showSetPaging)
                showSetPaging = false;
            if (SwitchToNormal_2.containsPoint(x, y) && showSetPaging)
            {
                showSetPaging = false;
                currentID += ModEntry.Chapters[currentChapter].Sets[currentSetPage + 1].CreaturesBelongingToThisSet[0];
                actualID += ModEntry.Chapters[currentChapter].Sets[currentSetPage + 1].CreaturesBelongingToThisSet[0];
            }
            if (ShowSetView.containsPoint(x, y) && !showSetPaging)
            {
                showSetPaging = true;
            }
            if (CloseButton.containsPoint(x, y))
            {
                ModEntry.Helper.Data.WriteJsonFile(PathUtilities.NormalizeAssetName("localData/stickies.json"), PagesWithStickies);
                Game1.activeClickableMenu.exitThisMenu();
            }
            willSearch = SearchButton.containsPoint(x, y) && !willSearch && textBox.Text == "";

            if (Button_2 != null || Button_3 != null)
            {
                IsFirstActive = Button_1.containsPoint(x, y);
                IsSecondActive = Button_2.containsPoint(x, y);
                IsThirdActive = Button_3.containsPoint(x, y);
            }
            if (LeftArrow.containsPoint(x, y) && LeftArrow.visible && !willSearch)
            {
                if (IsHeaderPage)
                    IsHeaderPage = false;
                else if (actualID > 0 && !showSetPaging)
                {
                    actualID--;
                    if (currentID == 0 && currentChapter != 0 )
                    {
                        currentChapter--;
                        currentID = ModEntry.Chapters[currentChapter].Creatures.Count - 1;
                        WasHeaderPage = true;
                        IsHeaderPage = true;
                        CalculateSetPages(ModEntry.Chapters[currentChapter]);
                    }
                    else if (currentID > 0)
                        currentID--;
                }
                else if (currentID == 0)
                    IsHeaderPage = true;

                else if (showSetPaging)
                {
                    if (currentSetPage > 0)
                    {
                        actualID -= ModEntry.Chapters[currentChapter].Sets[currentSetPage - 1].CreaturesBelongingToThisSet.Length;
                        currentID -= ModEntry.Chapters[currentChapter].Sets[currentSetPage - 1].CreaturesBelongingToThisSet.Length;
                        if (currentSetPage is not 1 && !pageOrder[currentSetPage - 2] && !pageOrder[currentSetPage - 1])
                        {
                            actualID -= ModEntry.Chapters[currentChapter].Sets[currentSetPage - 2].CreaturesBelongingToThisSet.Length;
                            currentID -= ModEntry.Chapters[currentChapter].Sets[currentSetPage - 2].CreaturesBelongingToThisSet.Length;
                            currentSetPage--; currentSetPage--;
                        }
                    }
                    if (currentSetPage is 0 && currentChapter != 0)
                    {
                        currentChapter--;
                        currentID = ModEntry.Chapters[currentChapter].Creatures.Count - 1;
                        WasHeaderPage = true;
                        IsHeaderPage = true;
                        CalculateSetPages(ModEntry.Chapters[currentChapter]);
                        currentSetPage = pageOrder.Count - 1;
                    }
                }
                for (int i = 0; i < 4; i++)
                    FindPageAndCheckSides(PagesWithStickies[i], false, i);
            }
            else if (RightArrow.containsPoint(x, y) && RightArrow.visible && !willSearch)
            {
                if (IsHeaderPage)
                    IsHeaderPage = false;
                else if (actualID + 1 != ModEntry.Chapters[currentChapter].Creatures.Count && !showSetPaging)
                {
                    actualID++;
                    if (currentID + 1 == ModEntry.Chapters[currentChapter].Creatures.Count && currentChapter < ModEntry.Chapters.Count)
                    {
                        currentChapter++;
                        currentID = 0;
                        IsHeaderPage = true;
                        currentSetPage = 0;
                        CalculateSetPages(ModEntry.Chapters[currentChapter]);
                    }
                    else
                        currentID++;
                }
                else if (showSetPaging)
                {
                    actualID += ModEntry.Chapters[currentChapter].Sets[currentSetPage].CreaturesBelongingToThisSet.Length;
                    currentID += ModEntry.Chapters[currentChapter].Sets[currentSetPage].CreaturesBelongingToThisSet.Length;
                    if (currentSetPage == pageOrder.Count - 1 && currentChapter < ModEntry.Chapters.Count)
                    {
                        currentChapter++;
                        currentID = 0;
                        IsHeaderPage = true;
                        currentSetPage = 0;
                        CalculateSetPages(ModEntry.Chapters[currentChapter]);
                    }
                    else if (currentSetPage < pageOrder.Count - 2 && !pageOrder[currentSetPage] && !pageOrder[currentSetPage + 1])
                    {
                        actualID += ModEntry.Chapters[currentChapter].Sets[currentSetPage + 1].CreaturesBelongingToThisSet.Length;
                        currentID += ModEntry.Chapters[currentChapter].Sets[currentSetPage + 1].CreaturesBelongingToThisSet.Length;
                        currentSetPage++; currentSetPage++;
                    }
                    else
                        currentSetPage++;
                }
                for (int i = 0; i < 4; i++)
                    FindPageAndCheckSides(PagesWithStickies[i], false, i);
            }
            textBox.Update();
            Game1.keyboardDispatcher.Subscriber = textBox;
            Game1.playSound("shwip");
            UpdateNotebookPage();
        }
        private void UpdateNotebookPage()
        {
            fullCreatureID = ModEntry.Chapters[currentChapter].CreatureNamePrefix + "_" + currentID.ToString();
            modID = ModEntry.Chapters[currentChapter].FromContentPack.Manifest.UniqueID;
            menuTexts[2] = ModEntry.Chapters[currentChapter].Creatures[currentID].Name;
            menuTexts[1] = ModEntry.Chapters[currentChapter].Creatures[currentID].Desc;
            MenuTextures[1] = ModEntry.Helper.GameContent.Load<Texture2D>(Path.Combine("KediDili.Creaturebook", ModEntry.Chapters[currentChapter].FromContentPack.Manifest.UniqueID + "." + ModEntry.Chapters[currentChapter].CreatureNamePrefix + "_" + ModEntry.Chapters[currentChapter].Creatures[currentID].ID + "_Image1"));
            menuTexts[5] = ModEntry.Helper.Translation.Get("CB.Chapter") + Convert.ToString(currentChapter + 1) + ": " + ModEntry.Chapters[currentChapter].Title + "\n" + ModEntry.Helper.Translation.Get("CB.ChapterAuthorBy") + ModEntry.Chapters[currentChapter].Author;
            
            if (ModEntry.Chapters[currentChapter].Creatures[currentID].HasScientificName)
                menuTexts[0] = ModEntry.Helper.Translation.Get("CB.LatinName") + ModEntry.Chapters[currentChapter].Creatures[currentID].ScientificName; 
            else
                latinName = "";
            
            if (WasHeaderPage)
            {
                menuTexts[5] = authorchapterTitle = ModEntry.Helper.Translation.Get("CB.Chapter") + Convert.ToString(currentChapter + 2) + ": " + ModEntry.Chapters[currentChapter + 1].Title + "\n" + ModEntry.Helper.Translation.Get("CB.ChapterAuthorBy") + ModEntry.Chapters[currentChapter + 1].Author;
                WasHeaderPage = false;
            }
            if (ModEntry.Chapters[currentChapter].Creatures[currentID].HasExtraImages)
            {
                MenuTextures[2] = ModEntry.Helper.GameContent.Load<Texture2D>(Path.Combine("KediDili.Creaturebook", ModEntry.Chapters[currentChapter].FromContentPack.Manifest.UniqueID + "." + ModEntry.Chapters[currentChapter].CreatureNamePrefix + "_" + ModEntry.Chapters[currentChapter].Creatures[currentID].ID + "_Image2"));
                if (File.Exists(PathUtilities.NormalizeAssetName(ModEntry.Chapters[currentChapter].Creatures[currentID].Directory + "\\book-image_3.png")))
                    MenuTextures[3] = ModEntry.Helper.GameContent.Load<Texture2D>(Path.Combine("KediDili.Creaturebook", ModEntry.Chapters[currentChapter].FromContentPack.Manifest.UniqueID + "." + ModEntry.Chapters[currentChapter].CreatureNamePrefix + "_" + ModEntry.Chapters[currentChapter].Creatures[currentID].ID + "_Image3"));
                else
                    MenuTextures[3] = null;
            }
            else
                MenuTextures[2] = null;
        }
        private void CalculateSetPages(Chapter chapter)
        {
            pageOrder = new List<bool>();
            foreach (Set set in chapter.Sets)
                pageOrder.Add(set.NeedsSecondPage);
        }
        private void DrawCreatureIcons(SpriteBatch b, bool needsSecond, int i) 
        {
            for (int l = 0; l < ModEntry.Chapters[currentChapter].Sets[i].OffsetsInMenu.Length; l++)
            {
                if (Game1.player.modData[ModEntry.MyModID + "_" + modID + "." + ModEntry.Chapters[currentChapter].CreatureNamePrefix + "_" + ModEntry.Chapters[currentChapter].Sets[i].CreaturesBelongingToThisSet[l]] is not "null")
                    b.Draw(MenuTextures[1], TopLeftCorner, null, Color.White, 0f, new Vector2((int)TopLeftCorner.X + ModEntry.Chapters[currentChapter].Creatures[currentID].ImageOffsets[0].X, (int)TopLeftCorner.Y + ModEntry.Chapters[currentChapter].Creatures[currentID].ImageOffsets[0].Y), ModEntry.Chapters[currentChapter].Creatures[currentID].ImageScales[0], SpriteEffects.None, layerDepth: 0.5f);
                else
                    b.Draw(MenuTextures[1], TopLeftCorner, null, Color.Black * 0.8f, 0f, new Vector2((int)TopLeftCorner.X + ModEntry.Chapters[currentChapter].Sets[i].OffsetsInMenu[l].X, (int)TopLeftCorner.Y + ModEntry.Chapters[currentChapter].Sets[i].OffsetsInMenu[l].Y), ModEntry.Chapters[currentChapter].Sets[i].ScalesInMenu[l], SpriteEffects.None, layerDepth: 0.5f);
                SwitchToNormal_1.draw(b);
            }
            if (!needsSecond && i < pageOrder.Count - 1)
            {
                for (int l = 0; l < ModEntry.Chapters[currentChapter].Sets[i + 1].OffsetsInMenu.Length; l++)
                {
                     if (Game1.player.modData[ModEntry.MyModID + "_" + modID + "." + ModEntry.Chapters[currentChapter].CreatureNamePrefix + "_" + ModEntry.Chapters[currentChapter].Sets[i + 1].CreaturesBelongingToThisSet[l]] is not "null")
                        b.Draw(MenuTextures[1], TopLeftCorner, null, Color.White, 0f, new Vector2((int)TopLeftCorner.X + ModEntry.Chapters[currentChapter].Sets[i + 1].OffsetsInMenu[l].X, (int)TopLeftCorner.Y + ModEntry.Chapters[currentChapter].Sets[i + 1].OffsetsInMenu[l].Y), ModEntry.Chapters[currentChapter].Sets[i + 1].ScalesInMenu[l], SpriteEffects.None, layerDepth: 0.5f);
                     else
                        b.Draw(MenuTextures[1], TopLeftCorner, null, Color.Black * 0.8f, 0f, new Vector2((int)TopLeftCorner.X + ModEntry.Chapters[currentChapter].Sets[i + 1].OffsetsInMenu[l].X, (int)TopLeftCorner.Y + ModEntry.Chapters[currentChapter].Sets[i + 1].OffsetsInMenu[l].Y), ModEntry.Chapters[currentChapter].Sets[i + 1].ScalesInMenu[l], SpriteEffects.None, layerDepth: 0.5f);
                     SwitchToNormal_2.draw(b);
                }
            }
        }
        private void UpdateStickies(int WhichSticky)
        {
            if (PagesWithStickies[WhichSticky] is null && !IsHeaderPage && !WasHeaderPage)
            {
                Sticky_Yellow.bounds = WhichSticky is 0 ? new Rectangle((int)TopLeftCorner.X - 200, (int)TopLeftCorner.Y + 200, 240, 84) : Sticky_Yellow.bounds;
                Sticky_Green.bounds = WhichSticky is 1 ? new Rectangle((int)TopLeftCorner.X - 200, (int)TopLeftCorner.Y + 260, 240, 84) : Sticky_Green.bounds;
                Sticky_Blue.bounds = WhichSticky is 2 ? new Rectangle((int)TopLeftCorner.X, (int)TopLeftCorner.Y + 320, 240, 84) : Sticky_Blue.bounds;
                Sticky_Purple.bounds = WhichSticky is 3 ? new Rectangle((int)TopLeftCorner.X - 200, (int)TopLeftCorner.Y + 320, 240, 84) : Sticky_Purple.bounds;
                PagesWithStickies[WhichSticky] = modID + "." + fullCreatureID;
            }
            else if (PagesWithStickies[WhichSticky] == modID + "." + fullCreatureID && !IsHeaderPage && !WasHeaderPage)
            {
                Sticky_Yellow.bounds = WhichSticky is 0 ? new Rectangle((int)TopLeftCorner.X - 200, (int)TopLeftCorner.Y + 200, 240, 84) : Sticky_Yellow.bounds;
                Sticky_Yellow.bounds = WhichSticky is 1 ? new Rectangle((int)TopLeftCorner.X - 200, (int)TopLeftCorner.Y + 260, 240, 84) : Sticky_Green.bounds;
                Sticky_Blue.bounds = WhichSticky is 2 ? new Rectangle((int)TopLeftCorner.X, (int)TopLeftCorner.Y + 320, 240, 84) : Sticky_Blue.bounds;
                Sticky_Yellow.bounds = WhichSticky is 3 ? new Rectangle((int)TopLeftCorner.X - 200, (int)TopLeftCorner.Y + 320, 240, 84) : Sticky_Purple.bounds;
                PagesWithStickies[WhichSticky] = null;
            }
            else if (PagesWithStickies[WhichSticky] is not null && PagesWithStickies[WhichSticky] != modID + "." + fullCreatureID)
                FindPageAndCheckSides(PagesWithStickies[WhichSticky], true, 5);
            Sticky_Yellow.visible = false;
            Sticky_Yellow.visible = false;
            Sticky_Blue.visible = false;
            Sticky_Yellow.visible = false;

            Sticky_Yellow.visible = true;
            Sticky_Yellow.visible = true;
            Sticky_Blue.visible = true;
            Sticky_Yellow.visible = true;
        }
        private void FindPageAndCheckSides(string CreatureID, bool WillTravel, int SentFromWhichSticky)
        {
            if (string.IsNullOrEmpty(CreatureID))
                return;
            for (int i = 0; i < ModEntry.Chapters.Count; i++)
            {
                for (int c = 0; c < ModEntry.Chapters[i].Creatures.Count; c++)
                {
                    if (CreatureID.Contains(ModEntry.Chapters[i].FromContentPack.Manifest.UniqueID) && CreatureID.Contains(ModEntry.Chapters[i].CreatureNamePrefix) && CreatureID.Contains(c.ToString()))
                    {
                        if (WillTravel)
                        {
                            currentID = c;
                            currentChapter = i;
                            actualID = 0;

                            for (int a = 0; a < currentChapter - 1; a++)
                                actualID += ModEntry.Chapters[a].Creatures.Count;

                            actualID += currentID + ModEntry.Chapters.Count - 1;

                            IsHeaderPage = false;
                            WasHeaderPage = false;
                            UpdateNotebookPage();
                            return;
                        }
                        else
                        {
                            if (i < currentChapter || (i == currentChapter && c < currentID) || CreatureID == modID + "." + fullCreatureID)
                                Stickyrotation = (float)Math.PI;

                            else if (i > currentChapter || (i == currentChapter && c > currentID) && SentFromWhichSticky is not 5)
                            {
                                Stickyrotation = 0f;
                                Sticky_Yellow.bounds = SentFromWhichSticky is 0 ? new Rectangle((int)TopLeftCorner.X - 200, (int)TopLeftCorner.Y + 200, 240, 84) : Sticky_Yellow.bounds;
                                Sticky_Green.bounds = SentFromWhichSticky is 1 ? new Rectangle((int)TopLeftCorner.X - 200, (int)TopLeftCorner.Y + 260, 240, 84) : Sticky_Green.bounds;
                                Sticky_Blue.bounds = SentFromWhichSticky is 2 ? new Rectangle(NotebookTexture.Width + 50, (int)TopLeftCorner.Y + 320, 240, 84) : Sticky_Blue.bounds;
                                Sticky_Purple.bounds = SentFromWhichSticky is 3 ? new Rectangle((int)TopLeftCorner.X - 200, (int)TopLeftCorner.Y + 320, 240, 84) : Sticky_Purple.bounds;
                            }
                        }
                    }
                }
            }
        }
    } 
}
