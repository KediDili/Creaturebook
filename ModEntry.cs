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
using Netcode;

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
        ChapterModel chapterData = new ChapterModel();
        CreatureModel creatureData = new CreatureModel();
        Creature sortedOutCreature = new Creature();
        internal static ModData singleModData = new ModData();

        internal static List<ChapterModel> chapterModels = new List<ChapterModel>();
        static List<Creature> creatures = new List<Creature>();
        internal static List<Creature> newCreatures = new List<Creature>();
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
            Helper.Events.GameLoop.SaveCreated += OnSaveCreated;
            Helper.Events.Content.AssetRequested += OnAssetRequested;
            Helper.Events.Multiplayer.PeerConnected += OnPeerConnected;
            Helper.Events.Multiplayer.ModMessageReceived += OnModMessageReceived;
        }
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            foreach (IContentPack contentPack in Helper.ContentPacks.GetOwned())
            {
                Monitor.Log($"Reading content pack: {contentPack.Manifest.Name}, v{contentPack.Manifest.Version}");

                if (!contentPack.HasFile("chapter.json"))
                {
                    Monitor.Log($"{contentPack.Manifest.Name} seems to lack the 'chapter.json' file that is required. If you're the author please add the file or check your spelling in the filename, if you're a simple player please let the content pack author know of this error or reinstall the content pack. (If you read this at all, that is.)", LogLevel.Error);
                    continue;
                }
                chapterData = contentPack.ReadJsonFile<ChapterModel>("chapter.json");

                if (chapterData == null)
                {
                    Monitor.Log($"{contentPack.Manifest.Name} seems to have the 'chapter.json', but it's empty. If I just wanted a null value I'd not ask for the file at all, right?", LogLevel.Warn);
                    continue;
                }
                string title = chapterData.ChapterTitle;

                var subfolders = new DirectoryInfo(Path.Combine(contentPack.DirectoryPath, title)).GetDirectories();

                if (chapterData.CreatureAmount != subfolders.Length)
                {
                    Monitor.Log($"{contentPack.Manifest.Name} seems to have a different number of creatures than specified in 'chapter.json'! Yes, of course it matters. Numbers don't calculate themselves!", LogLevel.Warn);
                    continue;
                }
                else if (subfolders.Length == 0)
                {
                    Monitor.Log($"{contentPack.Manifest.Name} doesn't seem to have any creatures at all! O.o", LogLevel.Warn);
                    continue;
                }
                chapterModels.Add(chapterData);

                foreach (var subfolder in subfolders)
                {
                    if (!File.Exists(Path.Combine(subfolder.FullName, "creature.json")))
                    {
                        Monitor.Log($"{contentPack.Manifest.Name} seems to lack a 'creature.json' under {subfolder.Name}. So add it or tell the author to.", LogLevel.Warn);
                        continue;
                    }
                    creatureData = contentPack.ReadJsonFile<CreatureModel>(Path.Combine(subfolder.Parent.Name, subfolder.Name, "creature.json"));

                    if (creatureData == null)
                    {
                        Monitor.Log($"{contentPack.Manifest.Name} seems to have the 'creature.json', under {subfolder.Name} but it's empty.", LogLevel.Warn);
                        continue;
                    }

                    if (!File.Exists(Path.Combine(subfolder.FullName, "book-image.png")))
                    {
                        Monitor.Log($"{contentPack.Manifest.Name} seems to lack a 'book-image.png' under {subfolder.Name}. So add it or tell the author.", LogLevel.Warn);
                        continue;
                    }
                    sortedOutCreature = new Creature();
                    sortedOutCreature.Image_1 = contentPack.ModContent.Load<Texture2D>(PathUtilities.NormalizeAssetName(Path.Combine(subfolder.Parent.Name, subfolder.Name, "book-image.png")));
                    sortedOutCreature.ID = creatureData.CreatureID;
                    sortedOutCreature.LatinName = creatureData.ScientificName;
                    sortedOutCreature.Prefix = chapterData.CreatureNamePrefix;
                    sortedOutCreature.Name = contentPack.Translation.Get(chapterData.CreatureNamePrefix + "_" + sortedOutCreature.ID + "_name");
                    sortedOutCreature.Desc = contentPack.Translation.Get(chapterData.CreatureNamePrefix + "_" + sortedOutCreature.ID + "_desc");
                    sortedOutCreature.FromContentPack = contentPack.Manifest.UniqueID;
                    sortedOutCreature.Scale_1 = creatureData.ImageScale_1;
                    sortedOutCreature.OffsetX = creatureData.OffsetX;
                    sortedOutCreature.OffsetY = creatureData.OffsetY;
                    if (!File.Exists(Path.Combine(subfolder.FullName, "book-image_2.png")) && creatureData.HasExtraImages)
                    {
                        Monitor.Log($"{contentPack.Manifest.Name} seems to lack a 'book-image_2.png' under {subfolder.Name}.", LogLevel.Warn);
                        continue;
                    }
                    else if (File.Exists(Path.Combine(subfolder.FullName, "book-image_2.png")) && creatureData.HasExtraImages)
                    {
                        sortedOutCreature.Image_2 = contentPack.ModContent.Load<Texture2D>(PathUtilities.NormalizeAssetName(Path.Combine(subfolder.Parent.Name, subfolder.Name, "book-image-2.png")));
                        sortedOutCreature.Scale_2 = creatureData.ImageScale_2;
                        sortedOutCreature.OffsetX_2 = creatureData.OffsetX_2;
                        sortedOutCreature.OffsetY_2 = creatureData.OffsetY_2;
                    }

                    if (File.Exists(Path.Combine(subfolder.Name, "book-image_3.png")) && creatureData.HasExtraImages)
                    {
                        sortedOutCreature.Image_3 = contentPack.ModContent.Load<Texture2D>(PathUtilities.NormalizeAssetName(Path.Combine(subfolder.Parent.Name, subfolder.Name, "book-image-3.png")));
                        sortedOutCreature.Scale_3 = creatureData.ImageScale_3;
                        sortedOutCreature.OffsetX_3 = creatureData.OffsetX_3;
                        sortedOutCreature.OffsetY_3 = creatureData.OffsetY_3;
                    }
                    creatures.Add(sortedOutCreature);
                }
                chapterData = new ChapterModel();
                uniqueModIDs.Add(contentPack.Manifest.UniqueID);
                List<Creature> orderFirst = creatures.OrderBy(o => o.ID).ToList();
                newCreatures.AddRange(orderFirst);
                creatures.Clear();
            }
            Monitor.Log($"All content packs have been found, cleaned from invalid files and added into the Creaturebook!", LogLevel.Info);

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
                NetCollection<NPC> list = Game1.player.currentLocation.characters;

                foreach (NPC Characters in list)
                {
                    foreach (var item in newCreatures)
                    {
                        for (int i = 0; i < newCreatures.Count; i++)
                        {
                            string charName = Characters.Name;
                            var mousePos = e.Cursor.GrabTile;
                            string ID = Convert.ToString(newCreatures[i].ID);
                            string prefix = newCreatures[i].Prefix;
                            if (Characters.Name.Contains(prefix + "_" + ID) && charName == prefix + "_" + ID && Characters.getTileLocation() == mousePos)
                            {
                                foreach (string modID in uniqueModIDs)
                                {
                                    SDate currentDate = SDate.Now();
                                    if (singleModData.DiscoveryDates[modID + "." + charName] == null)
                                    {
                                        string hudMessage = Helper.Translation.Get("CB.discoveredHUDMessage");
                                        singleModData.DiscoveryDates[modID + "." + charName] = currentDate;
                                        Game1.addHUDMessage(new HUDMessage(hudMessage + newCreatures[i].Name, 1));
                                        ID = null;
                                        prefix = null;
                                        if (Context.HasRemotePlayers)
                                        {
                                            Helper.Multiplayer.SendMessage(singleModData, "ModData", modIDs: new[] { ModManifest.UniqueID });
                                        }
                                        return;
                                    }
                                    else if (singleModData.DiscoveryDates[modID + "." + charName] != null)
                                    {
                                        string hudMessage_AlreadyDiscovered = Helper.Translation.Get("CB.discoveredHUDMessage.Already");
                                        Game1.addHUDMessage(new HUDMessage(hudMessage_AlreadyDiscovered, 1));
                                        return;
                                    }
                                }
                                break;
                            }
                        }
                    }
                }
            }
        }
        private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (e.Name.IsEquivalentTo(Path.Combine("Mods", "KediDili.Creaturebook", "NotebookTexture")))
                e.LoadFromModFile<Texture2D>("assets/NotebookTexture.png", AssetLoadPriority.Medium);

            if (e.NameWithoutLocale.IsEquivalentTo("Data/ObjectInformation"))
            {
                e.Edit(asset =>
                {
                    var editorDictionary = asset.AsDictionary<int, string>();

                    string ItemName = Helper.Translation.Get("CB.Notebook.Item.Name");
                    string ItemDesc = Helper.Translation.Get("CB.Notebook.Item.Desc");

                    editorDictionary.Data[31] = "Creature Notebook//-300/Basic /" + ItemName + "/" + ItemDesc;
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
            if (e.Name.IsEquivalentTo(Path.Combine("Mods", "KediDili.Creaturebook", "SearchButton")))
                e.LoadFromModFile<Texture2D>("assets/SearchButton.png", AssetLoadPriority.Medium);
        }
        private void OnSaveCreated(object sender, SaveCreatedEventArgs e)
        {
            if (modConfig.WayToGetNotebook == "Inventory" && Context.IsMainPlayer)
            {
                Game1.player.addItemByMenuIfNecessary(new StardewValley.Object(31, 1));
                singleModData.IsNotebookObtained = true;
            }
            else if (Context.IsMainPlayer)
                singleModData.IsNotebookObtained = true;
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
                foreach (Creature creature in newCreatures)
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
    }
}