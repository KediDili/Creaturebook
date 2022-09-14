using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace Creaturebook
{
    public class ModConfig
    {
        public bool ShowScientificNames { get; set; } = true;
        public bool ShowDiscoveryDates { get; set; } = true;
        public KeybindList OpenMenuKeybind { get; set; } = KeybindList.Parse("LeftShift + B, LeftControl + B, LeftControl + LeftShift + B");
        public string WayToGetNotebook { get; set; } = "Letter";
    }

    public class ModEntry : Mod
    {
        internal static IModHelper Helper;

        public List<string> uniqueModIDs = new List<string>();

        internal static ModConfig modConfig = new ModConfig();
        List<Chapter> chapterData = new List<Chapter>();
        Creature creatureData = new Creature();
        internal static ModData singleModData = new ModData();
        internal static List<Chapter> chapterModels = new List<Chapter>();
        internal static List<Creature> creatures = new List<Creature>();
        internal static List<string> setsList = new List<string>();

        internal static ModData PeermodData = new ModData();
        internal long hostPlayerID;

        public override void Entry(IModHelper helper)
        {
            Helper = helper;

            modConfig = Helper.ReadConfig<ModConfig>();

            Helper.Events.Input.ButtonPressed += OnButtonPressed;
            Helper.Events.Input.ButtonsChanged += OnButtonsChanged;
            Helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            Helper.Events.GameLoop.Saving += OnSaving;
            Helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            Helper.Events.GameLoop.DayStarted += OnDayStarted;
            Helper.Events.Content.AssetRequested += OnAssetRequested;
            Helper.Events.Player.Warped += OnWarped;
            Helper.Events.Multiplayer.PeerConnected += OnPeerConnected;
            Helper.Events.Multiplayer.ModMessageReceived += OnModMessageReceived;
        }
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            foreach (IContentPack contentPack in Helper.ContentPacks.GetOwned())
            {
                Monitor.Log($"Reading content pack: {contentPack.Manifest.Name}, v{contentPack.Manifest.Version}");
                chapterData = new List<Chapter>();
                if (!contentPack.HasFile("chapter.json"))
                {
                    Monitor.Log($"{contentPack.Manifest.Name} seems to lack the 'chapter.json' file that is required. If you're the author please add the file or check your spelling in the filename, if you're a simple player please let the content pack author know of this error or reinstall the content pack. (If you read this at all, that is.)", LogLevel.Error);
                    continue;
                }
                chapterData = contentPack.ReadJsonFile<List<Chapter>>("chapter.json");

                if (chapterData == null)
                {
                    Monitor.Log($"{contentPack.Manifest.Name} seems to have the 'chapter.json', but it's empty. If I just wanted a null value I'd not ask for the file at all, right?", LogLevel.Warn);
                    continue;
                }
                foreach (var chapter in chapterData)
                {
                    string title = chapter.ChapterTitle;

                    var subfolders = new DirectoryInfo(Path.Combine(contentPack.DirectoryPath, title)).GetDirectories();
                    List<Creature> newCreatures = new List<Creature>();
                    if (chapter.CreatureAmount != subfolders.Length)
                    {
                        Monitor.Log($"{contentPack.Manifest.Name} seems to have a different number of creatures than specified in 'chapter.json'! Yes, of course it matters. Numbers don't calculate themselves!", LogLevel.Warn);
                        continue;
                    }
                    else if (subfolders.Length == 0)
                    {
                        Monitor.Log($"{contentPack.Manifest.Name} doesn't seem to have any creatures at all! O.o", LogLevel.Warn);
                        continue;
                    }

                    foreach (var subfolder in subfolders)
                    {
                        if (!File.Exists(Path.Combine(subfolder.FullName, "creature.json")))
                        {
                            Monitor.Log($"{contentPack.Manifest.Name} seems to lack a 'creature.json' under {subfolder.Name}. So add it or tell the author to.", LogLevel.Warn);
                            break;
                        }
                        creatureData = contentPack.ReadJsonFile<Creature>(Path.Combine(subfolder.Parent.Name, subfolder.Name, "creature.json"));
                        if (creatureData == null)
                        {
                            Monitor.Log($"{contentPack.Manifest.Name} seems to have the 'creature.json', under {subfolder.Name} but it's empty.", LogLevel.Warn);
                            break;
                        }

                        if (!File.Exists(Path.Combine(subfolder.FullName, "book-image.png")))
                        {
                            Monitor.Log($"{contentPack.Manifest.Name} seems to lack a 'book-image.png' under {subfolder.Name}. So add it or tell the author.", LogLevel.Warn);
                            break;
                        }
                        creatureData.Prefix = chapter.CreatureNamePrefix;
                        creatureData.Name = contentPack.Translation.Get(chapter.CreatureNamePrefix + "_" + creatureData.ID + "_name");
                        creatureData.Desc = contentPack.Translation.Get(chapter.CreatureNamePrefix + "_" + creatureData.ID + "_desc");
                        creatureData.FromContentPack = contentPack;
                        if (!File.Exists(Path.Combine(subfolder.FullName, "book-image_2.png")) && creatureData.HasExtraImages)
                        {
                            Monitor.Log($"{contentPack.Manifest.Name} seems to lack a 'book-image_2.png' under {subfolder.Name}.", LogLevel.Warn);
                            break;
                        }
                        creatureData.directory = PathUtilities.NormalizePath(subfolder.FullName);
                        newCreatures.Add(creatureData);
                    }
                    creatures.AddRange(newCreatures.OrderBy(o => o.ID).ToList());
                    newCreatures = new List<Creature>();
                    chapterModels.Add(chapter);
                    if (chapter.EnableSets)
                    {
                        foreach (var item in creatures)
                        {
                            if (!chapter.setsAndIDs.ContainsKey(creatureData.BelongsToSet))
                            {
                                chapter.setsAndIDs.Add(item.BelongsToSet, Convert.ToString(item.ID));
                            }
                            else
                            {
                                chapter.setsAndIDs[item.BelongsToSet] += "-" + Convert.ToString(item.ID);
                                int lastOccurence;
                                int firstOccurence;
                                string first = "";
                                string last = "";
                                lastOccurence = chapter.setsAndIDs[item.BelongsToSet].LastIndexOf('-');
                                firstOccurence = chapter.setsAndIDs[item.BelongsToSet].IndexOf('-');
                                first = chapter.setsAndIDs[item.BelongsToSet].Substring(lastOccurence);
                                last = chapter.setsAndIDs[item.BelongsToSet].Substring(firstOccurence);
                                chapter.setsAndIDs[item.BelongsToSet] = first + "-" + last;
                            }
                        }
                    }
                }
                uniqueModIDs.Add(contentPack.Manifest.UniqueID);
            }
            Monitor.Log($"All content packs have been found, cleaned from invalid files and added into the Creaturebook!", LogLevel.Info);

            HooktoGMCMAPI();
            HookToCPAPI();
        }
        private void HookToCPAPI()
        {
            var api = Helper.ModRegistry.GetApi<IContentPatcherAPI>("Pathoschild.ContentPatcher");
            int a = 0;
            if (api == null)
                return;

            foreach (var item in singleModData.DiscoveryDates)
            {
                if (item.Value != null)
                {
                    a++;
                }
            }

            api.RegisterToken(ModManifest, "AllDiscoveredCreatures", () =>
            {
                // save is loaded
                if (Context.IsWorldReady)
                    return new[] { Convert.ToString(a) };

                return null;
            });

            api.RegisterToken(ModManifest, "AllCreatures", () =>
            {
                // save is loaded
                if (Context.IsWorldReady)
                    return new[] { Convert.ToString(singleModData.DiscoveryDates.Count) };

                return null;
            });

            //should be used as if int
            api.RegisterToken(ModManifest, "DiscoveredCreaturesFromAChapter", new DiscoveredCreaturesFromAChapter());

            //should be used as if bool
            api.RegisterToken(ModManifest, "IsCreatureDiscovered", new IsCreatureDiscovered());
        }
        private void HooktoGMCMAPI() 
        {
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
            {
                Monitor.Log($"It appears either you haven't got Generic Mod Config Menu installed or an error occured while trying to hook up into its API. If the case is the first one, install it so that you can configure Creaturebook easier!", LogLevel.Info);
                return;
            }

            configMenu.Register(
                mod: ModManifest,
                reset: () => modConfig = new ModConfig(),
                save: () => Helper.WriteConfig(modConfig)
            );

            // add some config options
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get("CB.GMCM.ShowScientificNames.Name"),
                tooltip: () => Helper.Translation.Get("CB.GMCM.ShowScientificNames.Desc"),
                getValue: () => modConfig.ShowScientificNames,
                setValue: value => modConfig.ShowScientificNames = value
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get("CB.GMCM.ShowDiscoveryDates.Name"),
                tooltip: () => Helper.Translation.Get("CB.GMCM.ShowDiscoveryDates.Desc"),
                getValue: () => modConfig.ShowDiscoveryDates,
                setValue: value => modConfig.ShowDiscoveryDates = value
            );

            configMenu.AddKeybindList(
                mod: ModManifest,
                name: () => Helper.Translation.Get("CB.GMCM.OpenMenuKeybind.Name"),
                tooltip: () => Helper.Translation.Get("CB.GMCM.OpenMenuKeybind.Desc"),
                getValue: () => modConfig.OpenMenuKeybind,
                setValue: value => modConfig.OpenMenuKeybind = value
            );

            string Option1 = Helper.Translation.Get("CB.GMCM.WayToGetNotebook.Options.1");
            string Option2 = Helper.Translation.Get("CB.GMCM.WayToGetNotebook.Options.2");
            string Option3 = Helper.Translation.Get("CB.GMCM.WayToGetNotebook.Options.3");

            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get("CB.GMCM.WayToGetNotebook.Name"),
                tooltip: () => Helper.Translation.Get("CB.GMCM.WayToGetNotebook.Desc"),
                getValue: () => modConfig.WayToGetNotebook,
                setValue: value => modConfig.WayToGetNotebook = value,
                allowedValues: new string[] { Option1, Option2, Option3 }
            );
        }
        private void OnButtonsChanged(object sender, ButtonsChangedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            if (!Context.IsPlayerFree)
                return;

            if (modConfig.OpenMenuKeybind.JustPressed() && uniqueModIDs.Count > 0)
                Game1.activeClickableMenu = new NotebookMenu();
            else if (modConfig.OpenMenuKeybind.JustPressed() && uniqueModIDs.Count == 0)
                Game1.addHUDMessage(new HUDMessage(Helper.Translation.Get("CB.noContentPacks"), 2));
        }
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady || !Context.IsPlayerFree)
                return;

            if (e.Button == SButton.MouseLeft && Game1.player.CurrentItem?.ParentSheetIndex == 31)
            {
                foreach (var Characters in Game1.player.currentLocation.characters)
                {
                    foreach (var item in creatures)
                    {
                        string charName = Characters.Name;
                        var mousePos = e.Cursor.GrabTile;
                        string ID = Convert.ToString(item.ID);
                        if ((Characters.Name.Contains(item.Prefix + "_" + ID) || item.OverrideDefaultNaming.Contains(Characters.Name)) && Characters.getTileLocation() == mousePos && singleModData.IsNotebookObtained)
                        {
                            if (singleModData.DiscoveryDates[item.FromContentPack + "." + Characters.Name] == null)
                            {
                                 SDate currentDate = SDate.Now();
                                 string hudMessage = Helper.Translation.Get("CB.discoveredHUDMessage");
                                 singleModData.DiscoveryDates[item.FromContentPack + "." + charName] = currentDate;
                                 Game1.addHUDMessage(new HUDMessage(hudMessage + item.Name, 1));
                                 if (Context.HasRemotePlayers)
                                 {
                                     Helper.Multiplayer.SendMessage(singleModData, "ModData", modIDs: new[] { ModManifest.UniqueID });
                                 }
                                 return;
                            }
                            else if (singleModData.DiscoveryDates[item.FromContentPack + "." + charName] != null)
                            {
                                 string hudMessage_AlreadyDiscovered = Helper.Translation.Get("CB.discoveredHUDMessage.Already");
                                 Game1.addHUDMessage(new HUDMessage(hudMessage_AlreadyDiscovered, 1));
                                 return;
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
                            if (layer.Id == "Back" && property.Key == "Creaturebook" && property.Value.ToString().StartsWith("Discover"))
                            {
                                if (singleModData.DiscoveryDates[property.Value.ToString().Substring(8)] == null)
                                {
                                    foreach (var item in creatures)
                                    {
                                        SDate currentDate = SDate.Now();
                                        string hudMessage = Helper.Translation.Get("CB.discoveredHUDMessage");
                                        singleModData.DiscoveryDates[property.Value.ToString().Substring(8)] = currentDate;
                                        Game1.addHUDMessage(new HUDMessage(hudMessage + item.Name, 1));
                                        if (Context.HasRemotePlayers)
                                        {
                                            Helper.Multiplayer.SendMessage(singleModData, "ModData", modIDs: new[] { ModManifest.UniqueID });
                                        }
                                        return;
                                    }
                                }
                                else
                                {
                                    string hudMessage_AlreadyDiscovered = Helper.Translation.Get("CB.discoveredHUDMessage.Already");
                                    Game1.addHUDMessage(new HUDMessage(hudMessage_AlreadyDiscovered, 1));
                                    return;
                                }
                            }
                        }
                    }                
                }
            }
        }
        private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (e.Name.IsEquivalentTo(Path.Combine("KediDili.Creaturebook", "NotebookTexture")))
                e.LoadFromModFile<Texture2D>("assets/NotebookTexture.png", AssetLoadPriority.Medium);
            
            foreach (var item in creatures)
            {
                if (e.Name.IsEquivalentTo(Path.Combine("KediDili.Creaturebook", item.FromContentPack + "." + item.Prefix + "_" + item.ID + "_Image1")))
                {
                    Texture2D a()
                    {
                        return item.FromContentPack.ModContent.Load<Texture2D>(PathUtilities.NormalizeAssetName(Path.Combine(item.directory,"book-image.png")));
                    }
                    e.LoadFrom(a, AssetLoadPriority.Medium, onBehalfOf: item.FromContentPack.Manifest.UniqueID);
                    break;
                }
                if (e.Name.IsEquivalentTo(Path.Combine("KediDili.Creaturebook", item.FromContentPack + "." + item.Prefix + "_" + item.ID + "_Image2")))
                {
                    Texture2D a()
                    {
                        return item.FromContentPack.ModContent.Load<Texture2D>(PathUtilities.NormalizeAssetName(Path.Combine(item.directory, "book-image_2.png")));
                    }
                    e.LoadFrom(a, AssetLoadPriority.Medium, onBehalfOf: item.FromContentPack.Manifest.UniqueID);
                    break;
                }
                if (e.Name.IsEquivalentTo(Path.Combine("KediDili.Creaturebook", item.FromContentPack + "." + item.Prefix + "_" + item.ID + "_Image3")))
                {
                    Texture2D a()
                    {
                        return item.FromContentPack.ModContent.Load<Texture2D>(PathUtilities.NormalizeAssetName(Path.Combine(item.directory, "book-image_3.png")));
                    }
                    e.LoadFrom(a, AssetLoadPriority.Medium, onBehalfOf: item.FromContentPack.Manifest.UniqueID);
                    break;
                }
            }

            if (e.NameWithoutLocale.IsEquivalentTo("Data/ObjectInformation"))
            {
                e.Edit(asset =>
                {
                    var editorDictionary = asset.AsDictionary<int, string>();

                    string ItemName = Helper.Translation.Get("CB.Notebook.Item.Name");
                    string ItemDesc = Helper.Translation.Get("CB.Notebook.Item.Desc");

                    editorDictionary.Data[31] = "Creaturebook//-300/Quest /" + ItemName + "/" + ItemDesc;
                });
            }
            if (e.NameWithoutLocale.IsEquivalentTo("Maps/springobjects"))
            {
                e.Edit(asset =>
                {
                    var editorImage = asset.AsImage();

                    Texture2D sourceImage = Helper.ModContent.Load<Texture2D>("assets/NoteItem.png");
                    editorImage.PatchImage(sourceImage, targetArea: new Rectangle(112, 16, 16, 16));
                });
            }
            if (e.Name.IsEquivalentTo(Path.Combine("KediDili.Creaturebook", "SearchButton")))
                e.LoadFromModFile<Texture2D>("assets/SearchButton.png", AssetLoadPriority.Medium);

            if (e.NameWithoutLocale.IsEquivalentTo("Data/Events/Mountain") && modConfig.WayToGetNotebook == "Events")
            {
                e.Edit(asset =>
                {
                    var editorDictionary = asset.AsDictionary<string, string>();
                    
                    string code1 = "none/16 31/farmer 15 39 0/addObject 12 31 31/skippable/pause 1000/move farmer 0 -8 3 false/emote farmer 16/pause 750/move farmer -2 0 3 false/message \"";
                    string code2 = "\"/pause 1000/removeObject 12 31/message \"";
                    string code3 = "\"/pause 750 /end warpOut";

                    string text1 = Helper.Translation.Get("CB.ObtainNotebook.Event_1-1");
                    string text2 = Helper.Translation.Get("CB.ObtainNotebook.Event_1-2");

                    editorDictionary.Data["70030004/j 7/t 2000 2600/w rainy/H/Hl giveOutCreaturebook/c 1"] = code1 + text1 + code2 + text2 + code3;
                });
            }

            if (e.NameWithoutLocale.IsEquivalentTo("Data/Events/ScienceHouse") && modConfig.WayToGetNotebook == "Events")
            {
                e.Edit(asset =>
                {
                    var editorDictionary = asset.AsDictionary<string, string>();
                    
                    string code1 = "continue/6 17/Robin 8 18 2 farmer 6 23 0 Demetrius 22 21 1/skippable/move farmer 0 -3 0 false/move farmer 2 0 0 false/emote Robin 16/pause 500/speak Robin \"";
                    string code2 = "\"/emote farmer 40/pause 500/speak Robin \"";
                    string code3 = "\"/pause 500/emote farmer 32/move farmer 11 0 1 false/viewport move 3 0 4000/pause 1300/speak Demetrius \"";
                    string code4 = "\"/faceDirection Demetrius 3/emote Demetrius 16/pause 750/speak Demetrius \"";
                    string code5 = "\"/emote farmer 40/pause 750/addObject 20 20 31/pause 1500/speak Demetrius \"";
                    string code6 = "\"/emote farmer 60/removeObject 20 20 31/pause 250/faceDirection farmer 3/faceDirection Demetrius 1/move farmer -11 0 3 true/emote Demetrius 40/message \"";
                    string code7 = "\"/end warpOut";

                    string text1 = Helper.Translation.Get("CB.ObtainNotebook.Event_2-1");
                    string text2 = Helper.Translation.Get("CB.ObtainNotebook.Event_2-2");
                    string text3 = Helper.Translation.Get("CB.ObtainNotebook.Event_2-3");
                    string text4 = Helper.Translation.Get("CB.ObtainNotebook.Event_2-4");
                    string text5 = Helper.Translation.Get("CB.ObtainNotebook.Event_2-5");
                    string text6 = Helper.Translation.Get("CB.ObtainNotebook.Event_2-6");

                    editorDictionary.Data["70030005/e 70030004/i 31/H/t 0900 1700"] = code1 + text1 + code2 + text2 + code3 + text3 + code4 + text4 + code5 + text5 + code6 + text6 + code7;
                });
            }
            if (e.NameWithoutLocale.IsEquivalentTo("Data/mail") && modConfig.WayToGetNotebook == "Letter" && !Game1.player.eventsSeen.Contains(70030004) && !Game1.player.eventsSeen.Contains(70030005))
            {
                e.Edit(asset =>
                {
                    var editorDictionary = asset.AsDictionary<string,string>();

                    string text = Helper.Translation.Get("CB.ObtainNotebook.Mail");

                    editorDictionary.Data["giveOutCreaturebook"] = text;
                }); 
            }
        }
        private void OnSaving(object sender, SavingEventArgs e)
        {
            if (!Context.IsMainPlayer)
            {
                Helper.Multiplayer.SendMessage(singleModData, "ModData", modIDs: new[] { ModManifest.UniqueID }, playerIDs: new[] { hostPlayerID });
            }
            else if (Context.IsMainPlayer)
            {
                ModData c = Helper.Data.ReadSaveData<ModData>("KediDili.Creaturebook-DiscoveryProgress");
                List<string> s = Helper.Data.ReadSaveData<List<string>>("KediDili.Creaturebook-PreviouslyDownloadedPacks");

                if (s != null && c != null)
                {
                    foreach (string item in s)
                    {
                        foreach (var key in singleModData.DiscoveryDates.Keys)
                        {
                            if (key.Contains(item) && !Helper.ModRegistry.IsLoaded(item))
                            {
                                singleModData.DiscoveryDates.Remove(key);
                            }
                        }
                    }
                }
                Helper.Data.WriteSaveData("KediDili.Creaturebook-DiscoveryProgress", singleModData);
                Helper.Data.WriteSaveData("KediDili.Creaturebook-PreviouslyDownloadedPacks", uniqueModIDs);
            }
        }
        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            if (Context.IsMainPlayer)
            {
                ModData c = Helper.Data.ReadSaveData<ModData>("KediDili.Creaturebook-DiscoveryProgress");

                if (c != null)
                {
                    singleModData = c;
                }
                foreach (Creature creature in creatures)
                {
                    if (!singleModData.DiscoveryDates.ContainsKey(creature.FromContentPack + "." + creature.Prefix + "_" + Convert.ToString(creature.ID)))
                    {
                        singleModData.DiscoveryDates.Add(creature.FromContentPack + "." + creature.Prefix + "_" + creature.ID, null);
                    }
                }
            }
            if (Context.IsMultiplayer)
            {
                foreach (IMultiplayerPeer peer in Helper.Multiplayer.GetConnectedPlayers())
                {
                    if (peer.IsHost)
                    { 
                        hostPlayerID = peer.PlayerID;
                        if(Context.IsMainPlayer)
                            Helper.Multiplayer.SendMessage(singleModData, "ModData", modIDs: new[] { ModManifest.UniqueID });
                        Helper.Multiplayer.SendMessage(hostPlayerID, "long", modIDs: new[] { ModManifest.UniqueID });
                    }
                }
            }
        }
        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            SDate date = SDate.Now();
            if (Context.IsMainPlayer && !singleModData.IsNotebookObtained)
            {
                if (modConfig.WayToGetNotebook == "Inventory")
                {
                    Game1.player.addItemByMenuIfNecessary(new StardewValley.Object(31, 1, false));
                    singleModData.IsNotebookObtained = true;
                }
                else if (modConfig.WayToGetNotebook == "Letter" && !Game1.player.hasOrWillReceiveMail("giveOutCreaturebook") && date.DaysSinceStart > 7)
                {
                    Game1.addMailForTomorrow("giveOutCreaturebook");
                    singleModData.IsNotebookObtained = true;
                } 
            }
            else if (!Context.IsMainPlayer && !singleModData.IsNotebookObtained)
            {
                singleModData.IsNotebookObtained = true;
            }
        }
        private void OnPeerConnected(object sender, PeerConnectedEventArgs e)
        {
            if (Context.IsMainPlayer)
            {
                Helper.Multiplayer.SendMessage(singleModData, "ModData", modIDs: new[] { ModManifest.UniqueID });
            }
        }
        private void OnModMessageReceived(object sender, ModMessageReceivedEventArgs e)
        {
            if (e.FromModID == ModManifest.UniqueID && e.Type == "ModData")
            {
                singleModData = e.ReadAs<ModData>();
            }
            else if (e.FromModID == ModManifest.UniqueID && e.Type == "long")
            {
                hostPlayerID = e.ReadAs<long>();
            }
        }
        private void OnWarped(object sender, WarpedEventArgs e)
        {
            if (e.Player.eventsSeen.Contains(70030004) && !singleModData.IsNotebookObtained)
            {
                if (modConfig.WayToGetNotebook == "Events" && Context.IsMainPlayer)
                {
                    Game1.player.addItemByMenuIfNecessary(new StardewValley.Object(31, 1, false));
                    singleModData.IsNotebookObtained = true;
                }
            }
        }
    }
}