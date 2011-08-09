#region .NET References

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;
using System.IO;

#endregion 

#region XNA References

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

#endregion

#region Terraria References

using Terraria;

#endregion

namespace Buildaria
{
    public class Core : Main
    {
        #region Static Properties

        #region The "Not-Really-Properties" Static Fields

        public static SpriteBatch spriteBatch; // It's not really a property, but... meh
        public static Version Version; // Same here
        public static GraphicsDeviceManager Graphics; // ... You guessed it

        #endregion 

        public static Texture2D DefaultTexture { get; set; } // A single white pixel texture
        public static string VersionString { get; set; } // Contains the version information as a simple string
        public static Color SelectionOverlay { get; set; } // The color of the drawn selection overlay
        public static Vector2 TileSize { get; set; } // The size of the tiles... I don't know why I use this
        public static List<Item[]> Inventories = new List<Item[]>(); // The list of inventories

        public static Type WorldGenWrapper { get; set; } // For accessing WorldGen functions
        public static Type MainWrapper { get; set; } // For accessing private Main members

        #endregion 

        #region Selection Fields

        Vector2 sel1 = Vector2.Zero;
        Vector2 sel2 = Vector2.Zero;

        Point SelectionSize = new Point(0, 0);
        Point SelectionPosition = new Point(0, 0);
        bool[,] SelectedTiles = new bool[1,1];

        Point CopiedSize = new Point(0, 0);
        Tile[,] Copied = new Tile[1, 1];
        
        Point UndoSize = new Point(0, 0);
        Point UndoPosition = new Point(0, 0);
        Tile[,] Undo = new Tile[1, 1];

        #endregion

        #region BuildMode

        bool buildMode = true;
        public bool BuildMode
        {
            set
            {
                buildMode = value;
            }
            get
            {
                return buildMode;
            }
        }

        #endregion

        #region Various Private Fields

        int inventoryType = 0;
        int oldMenuMode = 0;
        bool hover = false;
        Vector2 lastPosition = Vector2.Zero;
        KeyboardState oldKeyState = Keyboard.GetState();
        bool itemHax = true;
        bool b_godMode = true; // I just put the suffix there since my 1.0.6 test version has an existing "godMode"
        bool npcsEnabled = false;

        #endregion

        #region Constructors

        public Core()
        { 
            // Load version information
            Version = Assembly.GetExecutingAssembly().GetName().Version;
            VersionString = Version.Major + "." + Version.Minor;
        }

        #endregion

        #region XNA Overrides

        protected override void Initialize()
        {
            base.Initialize();
            spriteBatch = new SpriteBatch(base.GraphicsDevice);

            Texture2D t = new Texture2D(base.GraphicsDevice, 1, 1);
            t.SetData<Color>(new Color[] { new Color(255, 255, 255, 255) });
            DefaultTexture = t;
            TileSize = new Vector2(16, 16);

            Window.Title = "Buildaria " + VersionString + "";
            Main.versionNumber = "Running on Terraria " + Main.versionNumber + " =)";

            SelectionOverlay = new Color(255, 100, 0, 50);

            MemoryStream stream = new MemoryStream();
            Assembly asm = Assembly.Load(new AssemblyName("Terraria"));
            WorldGenWrapper = asm.GetType("Terraria.WorldGen");
            MainWrapper = asm.GetType("Terraria.Main");
            CreateInventories();
        }

        protected override void Update(GameTime gameTime)
        {
            #region BuildMode + Item Hax

            if (!editSign)
            {
                if (keyState.IsKeyDown(Keys.T) && oldKeyState.IsKeyUp(Keys.T))
                {
                    itemHax = !itemHax;
                }
            }

            if (BuildMode && itemHax)
            {
                try
                {
                    FieldInfo tilex = player[myPlayer].GetType().GetField("tileRangeX");
                    FieldInfo tiley = player[myPlayer].GetType().GetField("tileRangeY");
                    tilex.SetValue(player[myPlayer], 1000);
                    tiley.SetValue(player[myPlayer], 1000);

                    for (int i = 0; i < player[myPlayer].inventory.Length; i++)
                    {
                        Item it = player[myPlayer].inventory[i];

                        if (i == 39)
                        {
                            player[myPlayer].inventory[i].SetDefaults(0);
                            player[myPlayer].inventory[i].name = "";
                            player[myPlayer].inventory[i].stack = 0;
                            player[myPlayer].inventory[i].UpdateItem(0);
                        }
                        else if (it.name != "Magic Mirror")
                        {
                            it.SetDefaults(it.type);
                            it.stack = 255;
                            if (itemHax)
                            {
                                it.autoReuse = true;
                                it.useTime = 0;
                            }
                            if (it.hammer > 0 || it.axe > 0)
                            {
                                it.hammer = 100;
                                it.axe = 100;
                            }
                            if (it.pick > 0)
                                it.pick = 100;
                        }
                        else
                        {
                            it.SetDefaults(50);
                        }

                        player[myPlayer].inventory[i] = it;
                    }
                }
                catch
                {

                }
            }
            else
            {
                FieldInfo tilex = player[myPlayer].GetType().GetField("tileRangeX");
                FieldInfo tiley = player[myPlayer].GetType().GetField("tileRangeY");
                tilex.SetValue(player[myPlayer], 5);
                tiley.SetValue(player[myPlayer], 4);
            }

            #endregion 

            #region Bucket Management

            bool[] lavaBuckets = new bool[40];
            bool[] waterBuckets = new bool[40];
            bool[] emptyBuckets = new bool[40];
            for (int i = 0; i < player[myPlayer].inventory.Length; i++)
            {
                if (player[myPlayer].inventory[i].type == 0xcf)
                {
                    lavaBuckets[i] = true;
                }
                else if (player[myPlayer].inventory[i].type == 0xce)
                {
                    waterBuckets[i] = true;
                }
                else if (player[myPlayer].inventory[i].type == 0xcd)
                {
                    emptyBuckets[i] = true;
                }
            }

            base.Update(gameTime);

            if (gamePaused)
                return;

            for (int i = 0; i < player[myPlayer].inventory.Length; i++)
            {
                if (player[myPlayer].inventory[i].type == 0xcd)
                {
                    if (lavaBuckets[i] == true)
                    {
                        player[myPlayer].inventory[i].type = 0xcf;
                    }
                    else if (waterBuckets[i] == true)
                    {
                        player[myPlayer].inventory[i].type = 0xce;
                    }
                    else if (emptyBuckets[i] == true)
                    {
                        player[myPlayer].inventory[i].type = 0xcd;
                    }
                }
            }

            #endregion

            #region Multiplayer Block

            if (menuMultiplayer)
            {
                // Disabling this code still won't let you map edit in multiplayer worlds.
                // You'll be able to toss items everywhere, but don't spoil the game for others!
                // However, sharing basic building materials shouldn't have that effect.
                menuMultiplayer = false;
                menuMode = 0;
            }

            #endregion

            #region NPC Spawning

            if (keyState.IsKeyDown(Keys.C) && oldKeyState.IsKeyUp(Keys.C))
            {
                npcsEnabled = !npcsEnabled;
            }

            if (!npcsEnabled)
            {
                foreach (NPC n in npc)
                {
                    if (!n.friendly)
                    {
                        n.life = 0;
                        n.UpdateNPC(0);
                    }
                }
            }

            #endregion

            if (menuMode != oldMenuMode && menuMode == 10)
            {
                LoadInventory(Inventories.Count - 1);
            }

            else if (menuMode == 10) // if in-game ...
            {
                bool allowStuff = true; // Disallows most buildaria functionality in-game
                // Set to true if the user may not want certain functions to be happening
                try
                {
                    #region Inventory Change

                    if (!editSign)
                    {
                        if (keyState.IsKeyDown(Keys.OemOpenBrackets) && !oldKeyState.IsKeyDown(Keys.OemOpenBrackets))
                        {
                            SaveInventory(inventoryType);
                            /*for (int i = 0; i < Inventories[inventoryType].Length; i++)
                            {
                                player[myPlayer].inventory[i].SetDefaults(Inventories[inventoryType][i].type);
                            }*/
                            LoadInventory(inventoryType - 1);
                        }
                        if (keyState.IsKeyDown(Keys.OemCloseBrackets) && !oldKeyState.IsKeyDown(Keys.OemCloseBrackets))
                        {
                            SaveInventory(inventoryType);
                            /*for (int i = 0; i < Inventories[inventoryType].Length; i++)
                            {
                                player[myPlayer].inventory[i].SetDefaults(Inventories[inventoryType][i].type);
                            }*/
                            LoadInventory(inventoryType + 1);
                        }
                    }

                    #endregion

                    #region Modifier Keys

                    // Detect modifier keys
                    bool shift = keyState.IsKeyDown(Keys.LeftShift) || keyState.IsKeyDown(Keys.RightShift);
                    bool alt = keyState.IsKeyDown(Keys.LeftAlt) || keyState.IsKeyDown(Keys.RightAlt);
                    bool ctrl = keyState.IsKeyDown(Keys.LeftControl) || keyState.IsKeyDown(Keys.RightControl);

                    #endregion

                    #region Allow Stuff Detection

                    // Detect if mouse is on a hotbar or inventory is open
                    for (int i = 0; i < 11; i++)
                    {
                        int x = (int)(20f + ((i * 0x38) * inventoryScale));
                        int y = (int)(20f + ((0 * 0x38) * inventoryScale));
                        int index = x;
                        if (((mouseState.X >= x) && (mouseState.X <= (x + (inventoryBackTexture.Width * inventoryScale)))) && ((mouseState.Y >= y) && (mouseState.Y <= (y + (inventoryBackTexture.Height * inventoryScale) + 2))))
                        {
                            i = 11;
                            allowStuff = false;
                            break;
                        }
                    }
                    if (playerInventory || !BuildMode || editSign) // Inventory is open
                        allowStuff = false;

                    #endregion

                    #region Ghost/Hover Mode

                    if (hover)
                    {
                        player[myPlayer].position = lastPosition;
                        float magnitude = 6f;
                        if (shift)
                        {
                            magnitude *= 4;
                        }
                        if (player[myPlayer].controlUp || player[myPlayer].controlJump)
                        {
                            player[myPlayer].position = new Vector2(player[myPlayer].position.X, player[myPlayer].position.Y - magnitude);
                        }
                        if (player[myPlayer].controlDown)
                        {
                            player[myPlayer].position = new Vector2(player[myPlayer].position.X, player[myPlayer].position.Y + magnitude);
                        }
                        if (player[myPlayer].controlLeft)
                        {
                            player[myPlayer].position = new Vector2(player[myPlayer].position.X - magnitude, player[myPlayer].position.Y);
                        }
                        if (player[myPlayer].controlRight)
                        {
                            player[myPlayer].position = new Vector2(player[myPlayer].position.X + magnitude, player[myPlayer].position.Y);
                        }
                    }

                    if (keyState.IsKeyDown(Keys.P) && !oldKeyState.IsKeyDown(Keys.P) && allowStuff)
                    {
                        hover = !hover;
                        player[myPlayer].fallStart = (int)player[myPlayer].position.Y;
                        player[myPlayer].immune = true;
                        player[myPlayer].immuneTime = 1000;
                    }

                    #endregion

                    if (allowStuff)
                    {
                        UpdateSelection();

                        #region Alt For Circles

                        if (alt && mouseState.LeftButton == ButtonState.Released)
                        {
                            for (int x = 0; x < SelectionSize.X; x++)
                            {
                                for (int y = 0; y < SelectionSize.Y; y++)
                                {
                                    SelectedTiles[x, y] = false;
                                }
                            }
                            Vector2 center = new Vector2(SelectionSize.X / 2f, SelectionSize.Y / 2f);
                            for (int x = 0; x < SelectionSize.X; x++)
                            {
                                for (int y = 0; y < SelectionSize.Y; y++)
                                {
                                    double dx = (x - center.X + 1) / center.X;
                                    double dy = (y - center.Y + 1) / center.Y;
                                    if (dx * dx + dy * dy < 1)
                                    {
                                        SelectedTiles[x, y] = true;
                                    }
                                }
                            }
                        }

                        #endregion

                        #region Shift For Outline

                        if (shift && mouseState.LeftButton == ButtonState.Released)
                        {
                            bool[,] tempTiles = new bool[SelectionSize.X, SelectionSize.Y];
                            for (int x = 0; x < SelectionSize.X; x++)
                            {
                                for (int y = 0; y < SelectionSize.Y; y++)
                                {
                                    tempTiles[x, y] = SelectedTiles[x, y];
                                }
                            }
                            for (int x = 0; x < SelectionSize.X; x++)
                            {
                                bool found1 = false;
                                bool found2 = false;
                                for (int y = 0; y < SelectionSize.Y; y++)
                                {
                                    if (!found1)
                                    {
                                        found1 = SelectedTiles[x, y];
                                        continue;
                                    }
                                    else if (!found2)
                                    {
                                        if (y + 1 > SelectionSize.Y - 1)
                                        {
                                            found2 = SelectedTiles[x, y];
                                            break;
                                        }
                                        else if (!found2 && !SelectedTiles[x, y + 1])
                                        {
                                            found2 = SelectedTiles[x, y];
                                            break;
                                        }
                                        else
                                        {
                                            SelectedTiles[x, y] = false;
                                        }
                                    }
                                    else if (found1 && found2)
                                        break;
                                }
                            }
                            for (int y = 0; y < SelectionSize.Y; y++)
                            {
                                bool found1 = false;
                                bool found2 = false;
                                for (int x = 0; x < SelectionSize.X; x++)
                                {
                                    if (!found1)
                                    {
                                        found1 = tempTiles[x, y];
                                        continue;
                                    }
                                    else if (!found2)
                                    {
                                        if (x + 1 > SelectionSize.X - 1)
                                        {
                                            found2 = tempTiles[x, y];
                                            break;
                                        }
                                        else if (!found2 && !tempTiles[x + 1, y])
                                        {
                                            found2 = tempTiles[x, y];
                                            break;
                                        }
                                        else
                                        {
                                            tempTiles[x, y] = false;
                                        }
                                    }
                                    else if (found1 && found2)
                                        break;
                                }
                            }
                            for (int x = 0; x < SelectionSize.X; x++)
                            {
                                for (int y = 0; y < SelectionSize.Y; y++)
                                {
                                    SelectedTiles[x, y] = SelectedTiles[x, y] || tempTiles[x, y];
                                }
                            }
                        }

                        #endregion

                        #region Day/Night Skip

                        if (keyState.IsKeyDown(Keys.N) && !oldKeyState.IsKeyDown(Keys.N))
                        {
                            if (dayTime)
                                time = dayLength + 1;
                            else
                                time = nightLength;
                        }

                        #endregion

                        #region God Mode

                        if (keyState.IsKeyDown(Keys.G) && oldKeyState.IsKeyUp(Keys.G))
                        {
                            b_godMode = !b_godMode;
                        }

                        if (b_godMode)
                        {
                            player[myPlayer].accWatch = 3;
                            player[myPlayer].accDepthMeter = 3;
                            player[myPlayer].statLife = 400;
                            player[myPlayer].statMana = 200;
                            player[myPlayer].dead = false;
                            player[myPlayer].rocketTimeMax = 1000000;
                            player[myPlayer].rocketTime = 1000;
                            player[myPlayer].canRocket = true;
                            player[myPlayer].fallStart = (int)player[myPlayer].position.Y;
                            player[myPlayer].accFlipper = true;
                        }
                        else
                        {
                            
                        }

                        #endregion

                        #region Place Anywhere

                        if (mouseState.LeftButton == ButtonState.Pressed && player[myPlayer].inventory[player[myPlayer].selectedItem].createTile >= 0 && itemHax)
                        {
                            int x = (int)((Main.mouseState.X + Main.screenPosition.X) / 16f);
                            int y = (int)((Main.mouseState.Y + Main.screenPosition.Y) / 16f);

                            if (Main.tile[x, y].active == false)
                            {
                                byte wall = Main.tile[x, y].wall;
                                Main.tile[x, y] = new Tile();
                                Main.tile[x, y].type = (byte)player[myPlayer].inventory[player[myPlayer].selectedItem].createTile;
                                Main.tile[x, y].wall = wall;
                                Main.tile[x, y].active = true;
                                TileFrame(x, y);
                                SquareWallFrame(x, y, true);
                            }
                        }
                        else if (mouseState.LeftButton == ButtonState.Pressed && player[myPlayer].inventory[player[myPlayer].selectedItem].createWall >= 0 && itemHax)
                        {
                            int x = (int)((Main.mouseState.X + Main.screenPosition.X) / 16f);
                            int y = (int)((Main.mouseState.Y + Main.screenPosition.Y) / 16f);

                            if (Main.tile[x, y].wall == 0)
                            {
                                if (Main.tile[x, y] == null)
                                {
                                    Main.tile[x, y] = new Tile();
                                    Main.tile[x, y].type = 0;
                                }

                                Main.tile[x, y].wall = (byte)player[myPlayer].inventory[player[myPlayer].selectedItem].createWall;
                                TileFrame(x, y);
                                SquareWallFrame(x, y, true);
                            }
                        }

                        #endregion

                        #region Selection Modifications

                        #region Copy/Paste

                        if (ctrl && keyState.IsKeyDown(Keys.C) && oldKeyState.IsKeyUp(Keys.C))
                        {
                            Copied = new Tile[SelectionSize.X, SelectionSize.Y];
                            CopiedSize = new Point(SelectionSize.X, SelectionSize.Y);
                            for (int x = 0; x < SelectionSize.X; x++)
                            {
                                for (int y = 0; y < SelectionSize.Y; y++)
                                {
                                    Copied[x, y] = new Tile();
                                    Copied[x, y].type = tile[x + SelectionPosition.X, y + SelectionPosition.Y].type;
                                    Copied[x, y].active = tile[x + SelectionPosition.X, y + SelectionPosition.Y].active;
                                    Copied[x, y].wall = tile[x + SelectionPosition.X, y + SelectionPosition.Y].wall;
                                    Copied[x, y].liquid = tile[x + SelectionPosition.X, y + SelectionPosition.Y].liquid;
                                    Copied[x, y].lava = tile[x + SelectionPosition.X, y + SelectionPosition.Y].lava;
                                }
                            }
                        }

                        if (ctrl && keyState.IsKeyDown(Keys.V) && oldKeyState.IsKeyUp(Keys.V))
                        {
                            if (sel1 != -Vector2.One && sel2 != -Vector2.One)
                            {
                                Undo = new Tile[CopiedSize.X, CopiedSize.Y];
                                UndoPosition = new Point((int)sel1.X, (int)sel1.Y);
                                UndoSize = new Point(CopiedSize.X, CopiedSize.Y);
                                for (int x = 0; x < CopiedSize.X; x++)
                                {
                                    for (int y = 0; y < CopiedSize.Y; y++)
                                    {
                                        try
                                        {
                                            if (Main.tile[x, y] == null)
                                            {
                                                Main.tile[x, y] = new Tile();
                                                Undo[x, y] = null;
                                            }
                                            else
                                            {
                                                Undo[x, y] = new Tile();
                                                Undo[x, y].type = Main.tile[x, y].type;
                                                Undo[x, y].liquid = Main.tile[x, y].liquid;
                                                Undo[x, y].lava = Main.tile[x, y].lava;
                                                Undo[x, y].wall = Main.tile[x, y].wall;
                                                Undo[x, y].active = Main.tile[x, y].active;
                                            }

                                            tile[(int)sel1.X + x, (int)sel1.Y + y] = new Tile();
                                            tile[(int)sel1.X + x, (int)sel1.Y + y].type = Copied[x, y].type;
                                            tile[(int)sel1.X + x, (int)sel1.Y + y].active = Copied[x, y].active;
                                            tile[(int)sel1.X + x, (int)sel1.Y + y].wall = Copied[x, y].wall;
                                            tile[(int)sel1.X + x, (int)sel1.Y + y].liquid = Copied[x, y].liquid;
                                            tile[(int)sel1.X + x, (int)sel1.Y + y].lava = Copied[x, y].lava;
                                            TileFrame((int)sel1.X + x, (int)sel1.Y + y);
                                            SquareWallFrame((int)sel1.X + x, (int)sel1.Y + y);
                                        }
                                        catch
                                        {

                                        }
                                    }
                                }
                            }
                        }

                        #endregion

                        #region Erasers

                        else if (mouseState.RightButton == ButtonState.Pressed && oldMouseState.RightButton == ButtonState.Released && player[myPlayer].inventory[player[myPlayer].selectedItem].pick >= 55)
                        {
                            Undo = new Tile[SelectionSize.X, SelectionSize.Y];
                            UndoPosition = new Point((int)sel1.X, (int)sel1.Y);
                            UndoSize = new Point(SelectionSize.X, SelectionSize.Y);
                            for (int xp = 0; xp < SelectionSize.X; xp++)
                            {
                                for (int yp = 0; yp < SelectionSize.Y; yp++)
                                {
                                    int x = xp + SelectionPosition.X;
                                    int y = yp + SelectionPosition.Y;
                                    if (SelectedTiles[xp, yp])
                                    {
                                        if (Main.tile[x, y] == null)
                                        {
                                            Main.tile[x, y] = new Tile();
                                            Undo[xp, yp] = null;
                                        }
                                        else
                                        {
                                            Undo[xp, yp] = new Tile();
                                            Undo[xp, yp].type = Main.tile[x, y].type;
                                            Undo[xp, yp].liquid = Main.tile[x, y].liquid;
                                            Undo[xp, yp].lava = Main.tile[x, y].lava;
                                            Undo[xp, yp].wall = Main.tile[x, y].wall;
                                            Undo[xp, yp].active = Main.tile[x, y].active;
                                        }

                                        byte wall = Main.tile[x, y].wall;
                                        Main.tile[x, y].type = 0;
                                        Main.tile[x, y].active = false;
                                        Main.tile[x, y].wall = wall;
                                        TileFrame(x, y);
                                        TileFrame(x, y - 1);
                                        TileFrame(x, y + 1);
                                        TileFrame(x - 1, y);
                                        TileFrame(x + 1, y);
                                        SquareWallFrame(x, y, true);
                                    }
                                }
                            }
                        }
                        else if (mouseState.RightButton == ButtonState.Pressed && oldMouseState.RightButton == ButtonState.Released && player[myPlayer].inventory[player[myPlayer].selectedItem].hammer >= 55)
                        {
                            Undo = new Tile[SelectionSize.X, SelectionSize.Y];
                            UndoPosition = new Point((int)sel1.X, (int)sel1.Y);
                            UndoSize = new Point(SelectionSize.X, SelectionSize.Y);
                            for (int xp = 0; xp < SelectionSize.X; xp++)
                            {
                                for (int yp = 0; yp < SelectionSize.Y; yp++)
                                {
                                    int x = xp + SelectionPosition.X;
                                    int y = yp + SelectionPosition.Y;
                                    if (SelectedTiles[xp, yp])
                                    {
                                        if (Main.tile[x, y] == null)
                                        {
                                            Main.tile[x, y] = new Tile();
                                            Undo[xp, yp] = null;
                                        }
                                        else
                                        {
                                            Undo[xp, yp] = new Tile();
                                            Undo[xp, yp].type = Main.tile[x, y].type;
                                            Undo[xp, yp].liquid = Main.tile[x, y].liquid;
                                            Undo[xp, yp].lava = Main.tile[x, y].lava;
                                            Undo[xp, yp].wall = Main.tile[x, y].wall;
                                            Undo[xp, yp].active = Main.tile[x, y].active;
                                        }

                                        Main.tile[x, y].wall = 0;
                                        TileFrame(x, y);
                                        TileFrame(x, y - 1);
                                        TileFrame(x, y + 1);
                                        TileFrame(x - 1, y);
                                        TileFrame(x + 1, y);
                                        SquareWallFrame(x, y, true);
                                    }
                                }
                            }
                        }

                        #endregion

                        #region Liquid (Fill/Erase)

                        else if (mouseState.RightButton == ButtonState.Pressed && oldMouseState.RightButton == ButtonState.Released && player[myPlayer].inventory[player[myPlayer].selectedItem].type == 0xcf)
                        {
                            Undo = new Tile[SelectionSize.X, SelectionSize.Y];
                            UndoPosition = new Point((int)sel1.X, (int)sel1.Y);
                            UndoSize = new Point(SelectionSize.X, SelectionSize.Y);
                            for (int xp = 0; xp < SelectionSize.X; xp++)
                            {
                                for (int yp = 0; yp < SelectionSize.Y; yp++)
                                {
                                    int x = xp + SelectionPosition.X;
                                    int y = yp + SelectionPosition.Y;
                                    if (SelectedTiles[xp, yp])
                                    {
                                        if (Main.tile[x, y] == null)
                                        {
                                            Main.tile[x, y] = new Tile();
                                            Undo[xp, yp] = null;
                                        }
                                        else
                                        {
                                            Undo[xp, yp] = new Tile();
                                            Undo[xp, yp].type = Main.tile[x, y].type;
                                            Undo[xp, yp].liquid = Main.tile[x, y].liquid;
                                            Undo[xp, yp].lava = Main.tile[x, y].lava;
                                            Undo[xp, yp].wall = Main.tile[x, y].wall;
                                            Undo[xp, yp].active = Main.tile[x, y].active;
                                        }

                                        Main.tile[x, y].liquid = 255;
                                        Main.tile[x, y].lava = true;
                                        TileFrame(x, y);
                                        SquareWallFrame(x, y, true);
                                    }
                                }
                            }
                        }
                        else if (mouseState.RightButton == ButtonState.Pressed && oldMouseState.RightButton == ButtonState.Released && player[myPlayer].inventory[player[myPlayer].selectedItem].type == 0xce)
                        {
                            Undo = new Tile[SelectionSize.X, SelectionSize.Y];
                            UndoPosition = new Point((int)sel1.X, (int)sel1.Y);
                            UndoSize = new Point(SelectionSize.X, SelectionSize.Y);
                            for (int xp = 0; xp < SelectionSize.X; xp++)
                            {
                                for (int yp = 0; yp < SelectionSize.Y; yp++)
                                {
                                    int x = xp + SelectionPosition.X;
                                    int y = yp + SelectionPosition.Y;
                                    if (SelectedTiles[xp, yp])
                                    {
                                        if (Main.tile[x, y] == null)
                                        {
                                            Main.tile[x, y] = new Tile();
                                            Undo[xp, yp] = null;
                                        }
                                        else
                                        {
                                            Undo[xp, yp] = new Tile();
                                            Undo[xp, yp].type = Main.tile[x, y].type;
                                            Undo[xp, yp].liquid = Main.tile[x, y].liquid;
                                            Undo[xp, yp].lava = Main.tile[x, y].lava;
                                            Undo[xp, yp].wall = Main.tile[x, y].wall;
                                            Undo[xp, yp].active = Main.tile[x, y].active;
                                        }

                                        Main.tile[x, y].liquid = 255;
                                        Main.tile[x, y].lava = false;
                                        TileFrame(x, y);
                                        SquareWallFrame(x, y, true);
                                    }
                                }
                            }
                        }
                        else if (mouseState.RightButton == ButtonState.Pressed && oldMouseState.RightButton == ButtonState.Released && player[myPlayer].inventory[player[myPlayer].selectedItem].type == 0xcd)
                        {
                            Undo = new Tile[SelectionSize.X, SelectionSize.Y];
                            UndoPosition = new Point((int)sel1.X, (int)sel1.Y);
                            UndoSize = new Point(SelectionSize.X, SelectionSize.Y);
                            for (int xp = 0; xp < SelectionSize.X; xp++)
                            {
                                for (int yp = 0; yp < SelectionSize.Y; yp++)
                                {
                                    int x = xp + SelectionPosition.X;
                                    int y = yp + SelectionPosition.Y;
                                    if (SelectedTiles[xp, yp])
                                    {
                                        if (Main.tile[x, y] == null)
                                        {
                                            Main.tile[x, y] = new Tile();
                                            Undo[xp, yp] = null;
                                        }
                                        else
                                        {
                                            Undo[xp, yp] = new Tile();
                                            Undo[xp, yp].type = Main.tile[x, y].type;
                                            Undo[xp, yp].liquid = Main.tile[x, y].liquid;
                                            Undo[xp, yp].lava = Main.tile[x, y].lava;
                                            Undo[xp, yp].wall = Main.tile[x, y].wall;
                                            Undo[xp, yp].active = Main.tile[x, y].active;
                                        }

                                        Main.tile[x, y].liquid = 0;
                                        Main.tile[x, y].lava = false;
                                        TileFrame(x, y);
                                        SquareWallFrame(x, y, true);
                                    }
                                }
                            }
                        }

                        #endregion

                        #region Fills

                        if (mouseState.RightButton == ButtonState.Pressed && oldMouseState.RightButton == ButtonState.Released && player[myPlayer].inventory[player[myPlayer].selectedItem].createTile >= 0)
                        {
                            Undo = new Tile[SelectionSize.X, SelectionSize.Y];
                            UndoPosition = new Point((int)sel1.X, (int)sel1.Y);
                            UndoSize = new Point(SelectionSize.X, SelectionSize.Y);
                            for (int xp = 0; xp < SelectionSize.X; xp++)
                            {
                                for (int yp = 0; yp < SelectionSize.Y; yp++)
                                {
                                    int x = xp + SelectionPosition.X;
                                    int y = yp + SelectionPosition.Y;
                                    if (SelectedTiles[xp, yp])
                                    {
                                        if (Main.tile[x, y] == null)
                                        {
                                            Undo[xp, yp] = null;
                                        }
                                        else
                                        {
                                            Undo[xp, yp] = new Tile();
                                            Undo[xp, yp].type = Main.tile[x, y].type;
                                            Undo[xp, yp].liquid = Main.tile[x, y].liquid;
                                            Undo[xp, yp].lava = Main.tile[x, y].lava;
                                            Undo[xp, yp].wall = Main.tile[x, y].wall;
                                            Undo[xp, yp].active = Main.tile[x, y].active;
                                        }

                                        byte wall = Main.tile[x, y].wall;
                                        Main.tile[x, y] = new Tile();
                                        Main.tile[x, y].type = (byte)player[myPlayer].inventory[player[myPlayer].selectedItem].createTile;
                                        Main.tile[x, y].wall = wall;
                                        Main.tile[x, y].active = true;
                                        TileFrame(x, y);
                                        SquareWallFrame(x, y, true);
                                    }
                                }
                            }
                        }
                        else if (mouseState.RightButton == ButtonState.Pressed && oldMouseState.RightButton == ButtonState.Released && player[myPlayer].inventory[player[myPlayer].selectedItem].createWall >= 0)
                        {
                            Undo = new Tile[SelectionSize.X, SelectionSize.Y];
                            UndoPosition = new Point((int)sel1.X, (int)sel1.Y);
                            UndoSize = new Point(SelectionSize.X, SelectionSize.Y);
                            for (int xp = 0; xp < SelectionSize.X; xp++)
                            {
                                for (int yp = 0; yp < SelectionSize.Y; yp++)
                                {
                                    int x = xp + SelectionPosition.X;
                                    int y = yp + SelectionPosition.Y;

                                    if (Main.tile[x, y] == null)
                                    {
                                        Undo[xp, yp] = null;
                                    }
                                    else
                                    {
                                        Undo[xp, yp] = new Tile();
                                        Undo[xp, yp].type = Main.tile[x, y].type;
                                        Undo[xp, yp].liquid = Main.tile[x, y].liquid;
                                        Undo[xp, yp].lava = Main.tile[x, y].lava;
                                        Undo[xp, yp].wall = Main.tile[x, y].wall;
                                        Undo[xp, yp].active = Main.tile[x, y].active;
                                    }

                                    if (SelectedTiles[xp, yp])
                                    {
                                        if (Main.tile[x, y] == null)
                                        {
                                            Main.tile[x, y] = new Tile();
                                            Main.tile[x, y].type = 0;
                                        }

                                        Main.tile[x, y].wall = (byte)player[myPlayer].inventory[player[myPlayer].selectedItem].createWall;
                                        TileFrame(x, y);
                                        SquareWallFrame(x, y, true);
                                    }
                                }
                            }
                        }

                        #endregion

                        #region Undo

                        if (ctrl && keyState.IsKeyDown(Keys.Z) && oldKeyState.IsKeyUp(Keys.Z))
                        {
                            for (int xp = 0; xp < UndoSize.X; xp++)
                            {
                                for (int yp = 0; yp < UndoSize.Y; yp++)
                                {
                                    int x = xp + UndoPosition.X;
                                    int y = yp + UndoPosition.Y;

                                    if (Undo[xp, yp] == null)
                                        tile[x, y] = null;
                                    else
                                    {
                                        tile[x, y] = new Tile();
                                        tile[x, y].type = Undo[xp, yp].type;
                                        tile[x, y].active = Undo[xp, yp].active;
                                        tile[x, y].wall = Undo[xp, yp].wall;
                                        tile[x, y].liquid = Undo[xp, yp].liquid;
                                        tile[x, y].lava = Undo[xp, yp].lava;
                                        TileFrame(x, y);
                                        SquareWallFrame(x, y);
                                    }
                                }
                            }
                        }

                        #endregion 

                        #endregion
                    }
                }
                catch
                {

                }
            }
            oldMenuMode = menuMode;
            lastPosition = player[myPlayer].position;
            oldKeyState = keyState;
        }

        protected override void Draw(GameTime gameTime)
        {
            if (menuMode == 10)
            {
                try
                {
                    /*int minx = (int)((screenPosition.X / 16) - ((screenWidth / 2) / 16) - 1);
                    int maxx = (int)((screenPosition.X / 16) + ((screenWidth / 2) / 16) + 1);
                    int miny = (int)((screenPosition.Y / 16) - ((screenHeight / 2) / 16) - 1);
                    int maxy = (int)((screenPosition.Y / 16) + ((screenHeight / 2) / 16) + 1);

                    if (minx < 0)
                        minx = 0;

                    if (miny < 0)
                        miny = 0;

                    if (maxx > maxTilesX)
                        maxx = maxTilesX - 1;

                    if (maxy > maxTilesY)
                        maxy = maxTilesY - 1;

                    for (int x = minx; x < maxx; x++)
                    {
                        for (int y = miny; y < maxy; y++)
                        {
                            try
                            {
                                if (tile[x, y] == null)
                                    continue;

                                tile[x, y].lighted = true;
                            }
                            catch
                            {
                                continue;
                            }
                        }
                    }*/
                }
                catch
                {

                }
            }

            base.Draw(gameTime);
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.NonPremultiplied);

            if (menuMode == 10)
            {
                DrawSelectionOverlay();
            }

            spriteBatch.End();
        }

        #endregion

        #region Reflection Haxessors

        public float inventoryScale
        {
            // Accesses Main's private field "inventoryScale" for checking if the mouse is in the hotbar
            get
            {
                object o = MainWrapper.GetField("inventoryScale", BindingFlags.Static | BindingFlags.NonPublic).GetValue(this);
                return (float)o;
            }
        }

        public void TileFrame(int x, int y, bool reset = false, bool breaks = true)
        {
            // Accesses the WorldGen's TileFrame() method for keeping tiles looking presentable when placed with hax
            WorldGenWrapper.GetMethod("TileFrame").Invoke(null, new object[] { x, y, reset, !breaks });
        }

        public void SquareWallFrame(int x, int y, bool reset = false)
        {
            // It's the above, but for walls
            WorldGenWrapper.GetMethod("SquareWallFrame").Invoke(null, new object[] { x, y, reset });
        }

        #endregion

        #region Inventory Functions

        public void CreateInventories()
        {
            #region Tester
            {
                Item[] i = new Item[53];

                for (int it = 0; it < i.Length; it++)
                {
                    i[it] = new Item();
                }
                i[0].SetDefaults(84);
                i[1].SetDefaults(114);
                i[2].SetDefaults(87);

                i[4].SetDefaults(205);
                i[5].SetDefaults(185);
                i[6].SetDefaults(0);
                i[7].SetDefaults(0);
                i[8].SetDefaults(0);
                i[9].SetDefaults(285);

                i[10].SetDefaults(49);
                i[11].SetDefaults(50);
                i[12].SetDefaults(53);
                i[13].SetDefaults(111);
                i[14].SetDefaults(88);
                i[15].SetDefaults(54);
                i[16].SetDefaults(128);
                i[17].SetDefaults(186);
                i[18].SetDefaults(187);
                i[19].SetDefaults(233);

                i[20].SetDefaults(211);
                i[21].SetDefaults(212);
                i[22].SetDefaults(100);
                i[23].SetDefaults(101);
                i[24].SetDefaults(102);
                i[25].SetDefaults(123);
                i[26].SetDefaults(124);
                i[27].SetDefaults(125);
                i[28].SetDefaults(151);
                i[29].SetDefaults(152);

                i[30].SetDefaults(153);
                i[31].SetDefaults(156);
                i[32].SetDefaults(213);
                i[33].SetDefaults(223);
                i[34].SetDefaults(228);
                i[35].SetDefaults(229);
                i[36].SetDefaults(230);
                i[37].SetDefaults(231);
                i[38].SetDefaults(232);

                Inventories.Add(i);
            }
            #endregion

            #region Potions
            {
                Item[] i = new Item[53];

                for (int it = 0; it < i.Length; it++)
                {
                    i[it] = new Item();
                }

                i[0].SetDefaults("Copper Pickaxe");
                i[1].SetDefaults("Copper Hammer");
                i[2].SetDefaults(0xc6);
                i[2].useStyle = 0;
                i[3].SetDefaults(0);

                i[4].SetDefaults(291);
                i[5].SetDefaults(292);
                i[6].SetDefaults(293);
                i[7].SetDefaults(294);
                i[8].SetDefaults(295);
                i[9].SetDefaults(296);

                i[10].SetDefaults(297);
                i[11].SetDefaults(298);
                i[12].SetDefaults(299);
                i[13].SetDefaults(300);
                i[14].SetDefaults(301);
                i[15].SetDefaults(302);
                i[16].SetDefaults(303);
                i[17].SetDefaults(304);
                i[18].SetDefaults(305);
                i[19].SetDefaults(288);

                i[20].SetDefaults(289);
                i[21].SetDefaults(290);
                i[22].SetDefaults(291);
                i[23].SetDefaults(0);
                i[24].SetDefaults(0);
                i[25].SetDefaults(0);
                i[26].SetDefaults(0);
                i[27].SetDefaults(0);
                i[28].SetDefaults(0);
                i[29].SetDefaults(0);

                i[30].SetDefaults(0);
                i[31].SetDefaults(0);
                i[32].SetDefaults(0);
                i[33].SetDefaults(0);
                i[34].SetDefaults(0);
                i[35].SetDefaults(0);
                i[36].SetDefaults(0);
                i[37].SetDefaults(0);
                i[38].SetDefaults(0);

                i[44].SetDefaults(88);
                i[45].SetDefaults("Tuxedo Shirt");
                i[46].SetDefaults("Tuxedo Pants");

                i[47].SetDefaults(0x35);
                i[48].SetDefaults(0x9f);
                i[49].SetDefaults(0x80);
                i[50].SetDefaults(0x9e);
                i[51].SetDefaults(0x36);

                i[52].SetDefaults("Sunglasses");

                Inventories.Add(i);
            }
            #endregion

            #region Alchemy 
            {
                Item[] i = new Item[53];

                for (int it = 0; it < i.Length; it++)
                {
                    i[it] = new Item();
                }

                i[0].SetDefaults("Copper Pickaxe");
                i[1].SetDefaults("Copper Hammer");
                i[2].SetDefaults(0xc6);
                i[2].useStyle = 0;
                i[3].SetDefaults(275);

                i[4].SetDefaults(307);
                i[5].SetDefaults(308);
                i[6].SetDefaults(309);
                i[7].SetDefaults(310);
                i[8].SetDefaults(311);
                i[9].SetDefaults(312);

                i[10].SetDefaults(313);
                i[11].SetDefaults(314);
                i[12].SetDefaults(315);
                i[13].SetDefaults(316);
                i[14].SetDefaults(317);
                i[15].SetDefaults(318);
                i[16].SetDefaults(319);
                i[17].SetDefaults(320);
                i[18].SetDefaults(323);
                i[19].SetDefaults(276);

                i[20].SetDefaults(31);
                i[21].SetDefaults(32);
                i[22].SetDefaults(36);
                i[23].SetDefaults(283);
                i[24].SetDefaults(27);
                i[25].SetDefaults(38);
                i[26].SetDefaults(59);
                i[27].SetDefaults(60);
                i[28].SetDefaults(62);
                i[29].SetDefaults(63);

                i[30].SetDefaults(66);
                i[31].SetDefaults(67);
                i[32].SetDefaults(68);
                i[33].SetDefaults(69);
                i[34].SetDefaults(154);
                i[35].SetDefaults(208);
                i[36].SetDefaults(209);
                i[37].SetDefaults(210);
                i[38].SetDefaults(222);

                i[44].SetDefaults(88);
                i[45].SetDefaults("Tuxedo Shirt");
                i[46].SetDefaults("Tuxedo Pants");

                i[47].SetDefaults(0x35);
                i[48].SetDefaults(0x9f);
                i[49].SetDefaults(0x80);
                i[50].SetDefaults(0x9e);
                i[51].SetDefaults(0x36);

                i[52].SetDefaults("Sunglasses");

                Inventories.Add(i);
            }
            #endregion

            #region Walls
            {
                Item[] i = new Item[53];

                for (int it = 0; it < i.Length; it++)
                {
                    i[it] = new Item();
                }
                i[0].SetDefaults("Copper Pickaxe");
                i[1].SetDefaults("Copper Hammer");
                i[2].SetDefaults(0xc6);
                i[2].useStyle = 0;

                i[4].SetDefaults(30);
                i[5].SetDefaults(0x1a);
                i[6].SetDefaults(0x5d);
                i[7].SetDefaults(0);
                i[8].SetDefaults(0);
                i[9].SetDefaults(0);

                i[10].SetDefaults(130);
                i[11].SetDefaults(0x84);
                i[12].SetDefaults(0x87);
                i[13].SetDefaults(0x8a);
                i[14].SetDefaults(140);
                i[15].SetDefaults(0x8e);
                i[16].SetDefaults(0x90);
                i[17].SetDefaults(0x92);
                i[18].SetDefaults(0xd6);
                i[19].SetDefaults(0);

                i[20].SetDefaults(0);
                i[21].SetDefaults(0);
                i[22].SetDefaults(0);
                i[23].SetDefaults(0);
                i[24].SetDefaults(0);
                i[25].SetDefaults(0);
                i[26].SetDefaults(0);
                i[27].SetDefaults(0);
                i[28].SetDefaults(0);
                i[29].SetDefaults(0);

                i[30].SetDefaults(0);
                i[31].SetDefaults(0);
                i[32].SetDefaults(0);
                i[33].SetDefaults(0);
                i[34].SetDefaults(0);
                i[35].SetDefaults(0);
                i[36].SetDefaults(0);
                i[37].SetDefaults(0);
                i[38].SetDefaults(0);

                i[44].SetDefaults(88);
                i[45].SetDefaults("Tuxedo Shirt");
                i[46].SetDefaults("Tuxedo Pants");

                i[47].SetDefaults(0x35);
                i[48].SetDefaults(0x9f);
                i[49].SetDefaults(0x80);
                i[50].SetDefaults(0x9e);
                i[51].SetDefaults(0x36);

                i[52].SetDefaults("Sunglasses");

                Inventories.Add(i);
            }
            #endregion

            #region Decor
            {
                Item[] i = new Item[53];

                for (int it = 0; it < i.Length; it++)
                {
                    i[it] = new Item();
                }
                i[0].SetDefaults("Copper Pickaxe");
                i[1].SetDefaults("Copper Hammer");
                i[2].SetDefaults(0xc6);
                i[2].useStyle = 0;

                i[3].SetDefaults(0x19);
                i[4].SetDefaults(0x1f);
                i[5].SetDefaults(0x20);
                i[6].SetDefaults(0x21);
                i[7].SetDefaults(0x22);
                i[8].SetDefaults(0x23);
                i[9].SetDefaults(0x24);

                i[10].SetDefaults("Gold Chest");
                i[11].SetDefaults(0x30);
                i[12].SetDefaults(0x57);
                i[13].SetDefaults("Water Candle");
                i[14].SetDefaults(0x69);
                i[15].SetDefaults(0x6a);
                i[16].SetDefaults(0x6b);
                i[17].SetDefaults(0x6c);
                i[18].SetDefaults(0x88);
                i[19].SetDefaults(0x93);

                i[20].SetDefaults(0x95);
                i[21].SetDefaults(150);
                i[22].SetDefaults(0xab);
                i[23].SetDefaults(0xb1);
                i[24].SetDefaults(0xb2);
                i[25].SetDefaults(0xb3);
                i[26].SetDefaults(180);
                i[27].SetDefaults(0xb5);
                i[28].SetDefaults(0xb6);
                i[29].SetDefaults(0xdd);

                i[30].SetDefaults(0xde);
                i[31].SetDefaults(0xe0);
                i[32].SetDefaults(0x3f);
                i[33].SetDefaults(50);
                i[33].mana = 0;
                i[34].SetDefaults(0);
                i[35].SetDefaults(0);
                i[36].SetDefaults(0);
                i[37].SetDefaults(0);
                i[38].SetDefaults(0);

                i[44].SetDefaults(88);
                i[45].SetDefaults("Tuxedo Shirt");
                i[46].SetDefaults("Tuxedo Pants");

                i[47].SetDefaults(0x35);
                i[48].SetDefaults(0x9f);
                i[49].SetDefaults(0x80);
                i[50].SetDefaults(0x9e);
                i[51].SetDefaults(0x36);

                i[52].SetDefaults("Sunglasses");

                Inventories.Add(i);
            }
            #endregion

            // I recommend adding new inventories here!

            #region Blocks
            {
                Item[] i = new Item[53];

                for (int it = 0; it < i.Length; it++)
                {
                    i[it] = new Item();
                }
                i[0].SetDefaults("Copper Pickaxe");
                i[1].SetDefaults("Copper Hammer");
                i[2].SetDefaults(0xc6);

                i[2].useStyle = 0;
                i[4].SetDefaults(2);
                i[5].SetDefaults(3);
                i[6].SetDefaults(8);
                i[7].SetDefaults(9);
                i[8].SetDefaults(0x5e);
                i[9].SetDefaults(170);

                i[10].SetDefaults(0xce);
                i[11].SetDefaults(0xcf);
                i[12].SetDefaults(11);
                i[13].SetDefaults(0x91);
                i[14].SetDefaults(12);
                i[15].SetDefaults(14);
                i[16].SetDefaults(0x8f);
                i[17].SetDefaults(13);
                i[18].SetDefaults(0x8d);
                i[19].SetDefaults(0x74);

                i[20].SetDefaults(0x3d);
                i[21].SetDefaults(0x3e);
                i[22].SetDefaults(0x42);
                i[23].SetDefaults(0x38);
                i[24].SetDefaults(0xcd);
                i[25].SetDefaults(0x81);
                i[26].SetDefaults(0x83);
                i[27].SetDefaults(0x85);
                i[28].SetDefaults(0x86);
                i[29].SetDefaults(0x89);

                i[30].SetDefaults(0x8b);
                i[31].SetDefaults(0xa9);
                i[32].SetDefaults(0xac);
                i[33].SetDefaults(0xad);
                i[34].SetDefaults(0xae);
                i[35].SetDefaults(0xb0);
                i[36].SetDefaults(0xc0);
                i[37].SetDefaults(0xc2);
                i[38].SetDefaults(0xc3);

                i[44].SetDefaults(88);
                i[45].SetDefaults("Tuxedo Shirt");
                i[46].SetDefaults("Tuxedo Pants");

                i[47].SetDefaults(0x35);
                i[48].SetDefaults(0x9f);
                i[49].SetDefaults(0x80);
                i[50].SetDefaults(0x9e);
                i[51].SetDefaults(0x36);

                i[52].SetDefaults("Sunglasses");

                Inventories.Add(i);
            }
            #endregion
        }

        public int LoadInventory(int id)
        {
            if (id < 0)
            {
                id = Inventories.Count - 1;
            }
            else if (id > Inventories.Count - 1)
            {
                id = 0;
            }

            if (id == 0)
                BuildMode = false;
            else
                BuildMode = true;

            for (int i = 0; i < Inventories[id].Length; i++)
            {
                if (i < 44)
                    player[myPlayer].inventory[i].SetDefaults(Inventories[id][i].type);
                else
                    player[myPlayer].armor[i - 44].SetDefaults(Inventories[id][i].type);
            }

            return inventoryType = id;
        }

        public int SaveInventory(int id)
        {
            if (id < 0)
            {
                id = Inventories.Count - 1;
            }
            else if (id > Inventories.Count - 1)
            {
                id = 0;
            }

            for (int i = 0; i < Inventories[id].Length; i++)
            {
                if (i < 44)
                    Inventories[id][i].SetDefaults(player[myPlayer].inventory[i].type);
                else
                    Inventories[id][i].SetDefaults(player[myPlayer].armor[i - 44].type);
            }

            return inventoryType = id;
        }

        #endregion

        #region Update Functions

        public void UpdateSelection()
        {
            // Button clicked, set first selection point 
            if (mouseState.LeftButton == ButtonState.Pressed && oldMouseState.LeftButton == ButtonState.Released && player[myPlayer].inventory[player[myPlayer].selectedItem].name.ToLower().Contains("phaseblade"))
            {
                int x = (int)((Main.mouseState.X + Main.screenPosition.X) / 16f);
                int y = (int)((Main.mouseState.Y + Main.screenPosition.Y) / 16f);
                sel1 = new Vector2(x, y);
            }

            // Button is being held down, set second point and make sure the selection points are in the right order
            if (mouseState.LeftButton == ButtonState.Pressed && player[myPlayer].inventory[player[myPlayer].selectedItem].name.ToLower().Contains("phaseblade"))
            {
                int x = (int)((Main.mouseState.X + Main.screenPosition.X) / 16f);
                int y = (int)((Main.mouseState.Y + Main.screenPosition.Y) / 16f);
                sel2 = new Vector2(x, y) + Vector2.One;
                if (keyState.IsKeyDown(Keys.LeftShift) || keyState.IsKeyDown(Keys.RightShift))
                {
                    Vector2 size = sel1 - sel2;
                    if (Math.Abs(size.X) != Math.Abs(size.Y))
                    {
                        float min = Math.Min(Math.Abs(size.X), Math.Abs(size.Y));
                        if (sel2.X > sel1.X)
                        {
                            sel2 = new Vector2(sel1.X + min, sel2.Y);
                        }
                        else
                        {
                            sel2 = new Vector2(sel1.X - min, sel2.Y);
                        }
                        if (sel2.Y > sel1.Y)
                        {
                            sel2 = new Vector2(sel2.X, sel1.Y + min);
                        }
                        else
                        {
                            sel2 = new Vector2(sel2.X, sel1.Y - min);
                        }
                    }
                }
            }

            // Clear selection
            if (mouseState.RightButton == ButtonState.Pressed && player[myPlayer].inventory[player[myPlayer].selectedItem].name.ToLower().Contains("phaseblade"))
            {
                sel1 = -Vector2.One;
                sel2 = -Vector2.One;
            }

            // Check inside the selection and set SelectedTiles accordingly
            int minx = (int)Math.Min(sel1.X, sel2.X);
            int maxx = (int)Math.Max(sel1.X, sel2.X);
            int miny = (int)Math.Min(sel1.Y, sel2.Y);
            int maxy = (int)Math.Max(sel1.Y, sel2.Y);
            SelectedTiles = new bool[maxx - minx, maxy - miny];
            SelectionSize = new Point(maxx - minx, maxy - miny);
            SelectionPosition = new Point(minx, miny);
            for (int x = 0; x < SelectionSize.X; x++)
            {
                for (int y = 0; y < SelectionSize.Y; y++)
                {
                    SelectedTiles[x, y] = true;
                }
            }
        }

        #endregion

        #region Draw Functions

        public void DrawSelectionOverlay()
        {
            // TODO: Properly cull the tiles so I'm not killing people trying to select massive areas
            // BROKEN: This code offsets the selection position as you move it off the screen to left - i.e, moving right

            if ((sel1 == -Vector2.One && sel2 == -Vector2.One) || (sel1 == Vector2.Zero && sel2 == Vector2.Zero && SelectionSize.X == 0 && SelectionSize.Y == 0))
                return;

            Vector2 offset = new Vector2(((int)(screenPosition.X)), ((int)(screenPosition.Y)));
            int minx = (int)Math.Max(SelectionPosition.X * TileSize.X, (SelectionPosition.X * TileSize.X) - ((int)(screenPosition.X / TileSize.X)) * TileSize.X);
            int diffx = (int)(SelectionPosition.X * TileSize.X) - minx;
            int maxx = minx + (int)Math.Max(SelectionSize.X * TileSize.X, screenWidth) + diffx;
            int miny = (int)Math.Max(SelectionPosition.Y * TileSize.Y, screenPosition.Y);
            int diffy = (int)(SelectionPosition.Y * TileSize.Y) - miny;
            int maxy = miny + (int)Math.Min(SelectionSize.Y * TileSize.Y, screenHeight) + diffy;
            for (int x = minx; x < maxx; x += (int)TileSize.X)
            {
                for (int y = miny; y < maxy; y += (int)TileSize.Y)
                {
                    int tx = (int)((x - minx) / TileSize.X);
                    int ty = (int)((y - miny) / TileSize.Y);
                    if (ty >= SelectionSize.Y)
                        continue;
                    if (tx >= SelectionSize.X)
                        break;
                    if (SelectedTiles[tx, ty])
                    {
                        Vector2 cull = (new Vector2(tx + (minx / TileSize.X), ty + (miny / TileSize.Y)) * TileSize) - offset;
                        spriteBatch.Draw(DefaultTexture, cull, null, SelectionOverlay, 0, Vector2.Zero, TileSize, SpriteEffects.None, 0);
                    }
                }
            }
        }

        #endregion 
    }
}
