﻿using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using MoreClocks.IridiumClock;
using MoreClocks.RadioactiveClock;
using Netcode;
using StardewValley.TerrainFeatures;
using StardewValley.Menus;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using StardewValley.Objects;

namespace MoreClocks
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod, IAssetEditor, IAssetLoader
    {
        /*********
        ** Fields
        *********/
        /// <summary>The in-game event detected on the last update tick.</summary>
        private Texture2D iridiumClockTexture;
        private Texture2D radioactiveClockTexture;
        private bool isGoldClockBuilt = false;
        private bool isIridiumClockBuilt = false;
        private bool isRadioactiveClockBuilt = false;
        private Random randomvalue;
        private float originalDifficultyModifier = 1f;
        private readonly float EPSILON = 0.01f;
        private List<string> machineNames;
        public uint machineUpdateInterval = 10;
        public float machineTime = 75f;

        /*********
        ** Accessors
        *********/
        public static Mod Instance;

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            helper.Events.World.BuildingListChanged += this.OnBuildingListChanged;
            helper.Events.GameLoop.DayStarted += this.OnDayStarted;
            helper.Events.GameLoop.DayEnding += this.OnDayEnding;
            helper.Events.Display.MenuChanged += this.OnMenuChanged;
            helper.Events.GameLoop.Saving += this.OnSaving;
            helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            helper.Events.GameLoop.UpdateTicking += this.OnUpdateTicking;
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;

            this.iridiumClockTexture = this.Helper.Content.Load<Texture2D>("assets/IridiumClock.png");
            this.radioactiveClockTexture = this.Helper.Content.Load<Texture2D>("assets/RadioactiveClock.png");
            this.randomvalue = new Random();
            this.machineNames = new List<string> {
                "Bee House", "Cask", "Charcoal Kiln", "Cheese Press", "Crystalarium",
                "Furnace", "Incubator", "Keg", "Lightning Rod", "Loom", "Mayonnaise Machine", "Oil Maker",
                "Preserves Jar", "Recycling Machine", "Seed Maker", "Slime Egg-Press", "Slime Incubator",
                "Tapper", "Worm Bin"};
        }

        /*********
        ** Private methods
        *********/

        /// <summary>The method called after a new day starts.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            NetCollection<Building> buildings = ((BuildableGameLocation)Game1.getFarm()).buildings;
            foreach (Building building in buildings)
            {
                if (building.buildingType.ToString() == "Gold Clock")
                {
                    this.isGoldClockBuilt = true;
                    if (CheckGameContext() == true)
                    {
                        originalDifficultyModifier = Game1.player.difficultyModifier;
                        Game1.player.difficultyModifier = 1.25f;
                    }
                }
                if (building.buildingType.ToString() == "Iridium Clock")
                {
                    Game1.getFarm().IsGreenhouse = true;
                    this.isIridiumClockBuilt = true;
                }
                if (building.buildingType.ToString() == "Radioactive Clock")
                {
                    this.isRadioactiveClockBuilt = true;
                }
            }
        }

        private void OnBuildingListChanged(object sender, BuildingListChangedEventArgs e)
        {
            foreach (Building building in e.Added)
            {
                if (building.buildingType.ToString() == "Gold Clock")
                {
                    this.isGoldClockBuilt = true;
                    if (CheckGameContext() == true)
                    {
                        originalDifficultyModifier = Game1.player.difficultyModifier;
                        Game1.player.difficultyModifier = 1.25f;
                    }
                }
                if (building.buildingType.ToString() == "Iridium Clock")
                {
                    Game1.getFarm().IsGreenhouse = true;
                    this.isIridiumClockBuilt = true;
                }
                if (building.buildingType.ToString() == "Radioactive Clock")
                {
                    this.isRadioactiveClockBuilt = true;
                }
            }
            foreach (Building building in e.Removed)
            {
                if (building.buildingType.ToString() == "Gold Clock")
                {
                    this.isGoldClockBuilt = false;
                    if (CheckGameContext() == true)
                    {
                        originalDifficultyModifier = Game1.player.difficultyModifier;
                        Game1.player.difficultyModifier = 1f;
                    }
                }
                if (building.buildingType.ToString() == "Iridium Clock")
                {
                    Game1.getFarm().IsGreenhouse = false;
                    this.isIridiumClockBuilt = false;
                }
                if (building.buildingType.ToString() == "Radioactive Clock")
                {
                    this.isRadioactiveClockBuilt = false;
                }
            }
        }

        /// <inheritdoc cref="IGameLoopEvents.DayEnding"/>
        /// <summary>Raised before the game ends the current day. This happens before it starts setting up the next day and before <see cref="IGameLoopEvents.Saving"/>.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnDayEnding(object sender, DayEndingEventArgs e)
        {
            if (this.isIridiumClockBuilt == true)
            {
                var chanceToSpeedUpMachines = this.randomvalue.Next(0, 100);
                if (chanceToSpeedUpMachines < 25) // 25% chance to trigger
                {
                    SpeedUpAllMachines();
                }
            }
            if (this.isRadioactiveClockBuilt == true) {
                GameLocation location = Game1.getFarm();
                foreach (var pair in location.terrainFeatures.Pairs)
                {
                    if (pair.Value is HoeDirt)
                    {
                        var dirt = pair.Value as HoeDirt;
                        // A state of 1 for dirt means it's watered.
                        if (dirt.crop != null && !dirt.crop.dead.Value && dirt.state.Value == HoeDirt.watered)
                        {
                            var randomChance = this.randomvalue.Next(0, 100);
                            if (randomChance < 25) // 25% chance to trigger
                            {
                                if (dirt.crop.currentPhase.Value != dirt.crop.phaseDays.Count - 1)
                                {
                                    dirt.crop.growCompletely();
                                }
                            }
                        }
                    }
                }
                var chanceToTurnIntoGiantCrop = this.randomvalue.Next(0, 100);
                if (chanceToTurnIntoGiantCrop < 25) // 25% chance to trigger
                {
                    MutateCrops(chanceToTurnIntoGiantCrop, location);
                }
            }
        }

        private void OnSaving(object sender, SavingEventArgs args)
        {
            if (CheckGameContext() == true)
            {
                Game1.player.difficultyModifier = originalDifficultyModifier;
            }
        }

        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            //if (CheckGameContext() == true) {
                //SpeedUpAllMachines();
            //}
        }

        private void OnUpdateTicking(object sender, UpdateTickingEventArgs e)
        {
            //if (CheckGameContext() == true)
            //{
                //SpeedUpAllMachines();
            //}
        }


        private void MutateCrops(int chance, GameLocation environment)
        {
            foreach (Tuple<Vector2, Crop> tuple in this.GetValidCrops(environment))
            {
                int xTile = (int)tuple.Item1.X;
                int yTile = (int)tuple.Item1.Y;

                Crop crop = tuple.Item2;

                double rand = new Random((int)Game1.uniqueIDForThisGame + (int)Game1.stats.DaysPlayed + xTile * 2000 +
                                      yTile).NextDouble();

                bool okCrop = true;
                if (crop.currentPhase.Value == crop.phaseDays.Count - 1 &&
                    (crop.indexOfHarvest.Value == 276 || crop.indexOfHarvest.Value == 190 || crop.indexOfHarvest.Value == 254) &&
                    rand < chance)
                {
                    for (int index1 = xTile - 1; index1 <= xTile + 1; ++index1)
                    {
                        for (int index2 = yTile - 1; index2 <= yTile + 1; ++index2)
                        {
                            Vector2 key = new Vector2(index1, index2);
                            if (!environment.terrainFeatures.ContainsKey(key) ||
                                !(environment.terrainFeatures[key] is HoeDirt) ||
                                (environment.terrainFeatures[key] as HoeDirt).crop == null ||
                                (environment.terrainFeatures[key] as HoeDirt).crop.indexOfHarvest !=
                                crop.indexOfHarvest)
                            {
                                okCrop = false;

                                break;
                            }
                        }

                        if (!okCrop)
                            break;
                    }

                    if (!okCrop)
                        continue;

                    for (int index1 = xTile - 1; index1 <= xTile + 1; ++index1)
                        for (int index2 = yTile - 1; index2 <= yTile + 1; ++index2)
                        {
                            var index3 = new Vector2(index1, index2);
                            (environment.terrainFeatures[index3] as HoeDirt).crop = null;
                        }
                    (environment as Farm).resourceClumps.Add(new GiantCrop(crop.indexOfHarvest.Value,
                        new Vector2(xTile - 1, yTile - 1)));
                }
            }
        }

        private List<Tuple<Vector2, Crop>> GetValidCrops(GameLocation environment)
        {
            List<Tuple<Vector2, Crop>> validCrops = new List<Tuple<Vector2, Crop>>();
            foreach (var pair in environment.terrainFeatures.Pairs)
            {
                if (pair.Value is HoeDirt)
                {
                    var tile = pair.Value.currentTileLocation;
                    var dirt = pair.Value as HoeDirt;
                    var crop = dirt?.crop;
                    if (crop != null && !crop.dead.Value && dirt.state.Value == HoeDirt.watered)
                    {
                        validCrops.Add(Tuple.Create(tile, dirt.crop));
                    }
                }
            }
            return validCrops;
        }

        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (e.NewMenu is CarpenterMenu carp)
            {
                if (this.isGoldClockBuilt == true)
                {
                    var blueprints = this.Helper.Reflection.GetField<List<BluePrint>>(carp, "blueprints").GetValue();
                    blueprints.Add(new BluePrint("Iridium Clock"));
                    blueprints.Add(new BluePrint("Radioactive Clock"));
                }
            }
        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            //if (!Context.IsPlayerFree || Game1.currentMinigame != null)
            //    return;

            //// Reload config
            //if (e.Button.ToString() == "Z")
            //{
            //    Game1.hudMessages.Add(new HUDMessage("Machine Speed Configuration Reloaded", 2));
            //    SpeedUpAllMachines();
            //}
        }

        private bool CheckGameContext()
        {
            if (!Context.IsMainPlayer)
            {
                return false;
            }
            else if (Context.IsMultiplayer)
            {
                return false;
            }
            return true;
        }

        // Sweep through all the machines in the world and speeds them up.
        private void SpeedUpAllMachines()
        {
            IEnumerable<GameLocation> locations = GetLocations();
            foreach (string name in this.machineNames)
            {
                foreach (GameLocation loc in locations)
                {
                    Func<KeyValuePair<Vector2, StardewValley.Object>, bool> matchingName = p => p.Value.name == name;
                    foreach (KeyValuePair<Vector2, StardewValley.Object> pair in loc.objects.Pairs)
                    {
                        if (matchingName(pair))
                        {
                            var obj = pair.Value;
                            if (obj.MinutesUntilReady is <= 0 or 999999 || obj.Name == "Stone")
                                continue;
                            SpeedUpMachine(obj);
                        }
                    }
                }
            }
        }

        // Speeds Ups the machine.
        private void SpeedUpMachine(StardewValley.Object obj)
        {
            string text = (obj.MinutesUntilReady / 10).ToString();
            
            // If machine hasn't been configured yet.   
            if (obj is Cask c && obj.heldObject.Value != null)
            {
                float agingRate = 1f;
                switch (c.heldObject.Value.ParentSheetIndex)
                {
                    case 426:
                        agingRate = 4f;
                        break;
                    case 424:
                        agingRate = 4f;
                        break;
                    case 459:
                        agingRate = 2f;
                        break;
                    case 303:
                        agingRate = 1.66f;
                        break;
                    case 346:
                        agingRate = 2f;
                        break;
                }
                // Configure casks
                if (Math.Abs(machineTime - 100f) > EPSILON && (int)Math.Round(c.agingRate.Value * 1000) % 10 != 1)
                {
                    // By percentage
                    c.agingRate.Value = agingRate * 100 / machineTime;
                    c.agingRate.Value = (float)Math.Round(c.agingRate.Value, 2);
                    c.agingRate.Value += 0.001f;
                }
            }
            else if (obj.MinutesUntilReady > 0)
            {
                // Configure all machines other than casks
                if (Math.Abs(machineTime - 100f) > EPSILON)
                {
                    // By percentage
                    obj.MinutesUntilReady = Math.Max(((int)(obj.MinutesUntilReady * machineTime / 100 / 10)) * 10 - 2, 8);
                }
            }
        }

        /// Get all game locations.
        /// Copied with permission from Pathoschild
        public IEnumerable<GameLocation> GetLocations()
        {
            return Game1.locations
                .Concat(
                    from location in Game1.locations.OfType<BuildableGameLocation>()
                    from building in location.buildings
                    where building.indoors.Value != null
                    select building.indoors.Value
                );
        }

        public bool CanEdit<T>(IAssetInfo asset)
        {
            if (asset.AssetNameEquals("Data\\Blueprints"))
            {
                return true;
            }
            return false;
        }

        public void Edit<T>(IAssetData asset)
        {
            asset.AsDictionary<string, string>().Data.Add("Iridium Clock", "/3/2/-1/-1/-2/-1/null/Iridium Clock/Makes all seeds plantable on your farm regardless of the season./Buildings/none/48/80/-1/null/Farm/15000/true");
            asset.AsDictionary<string, string>().Data.Add("Radioactive Clock", "/3/2/-1/-1/-2/-1/null/Radioactive Clock/Improves speed and quality of machines and crops on your farm. Giant Crops spawn more often./Buildings/none/48/80/-1/null/Farm/20000/true");
        }

        public bool CanLoad<T>(IAssetInfo asset)
        {
            if (asset.AssetNameEquals("Buildings\\Iridium Clock"))
            {
                return true;
            }
            if (asset.AssetNameEquals("Buildings\\Radioactive Clock"))
            {
                return true;
            }
            return false;
        }

        public T Load<T>(IAssetInfo asset)
        {
            if (asset.AssetNameEquals("Buildings\\Iridium Clock"))
            {
                return (T)(object)this.iridiumClockTexture;
            }
            if (asset.AssetNameEquals("Buildings\\Radioactive Clock"))
            {
                return (T)(object)this.radioactiveClockTexture;
            }
            return (T)(object)null;
        }
    }
}