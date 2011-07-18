using System;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using Terraria;

namespace Buildaria
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Core : Main
    {
        public static Texture2D DefaultTexture { get; set; }
        public static Version Version;
        public static string VersionString { get; set; }
        public static Color SelectionOverlay { get; set; }
        public static Vector2 TileSize { get; set; }
        public static List<Item[]> Inventories = new List<Item[]>();

        public static Type WorldGenWrapper { get; set; }
        public static Type MainWrapper { get; set; }

        Vector2 sel1 = Vector2.Zero;
        Vector2 sel2 = Vector2.Zero;

        Point SelectionSize = new Point(0, 0);
        Point SelectionPosition = new Point(0, 0);
        bool[,] SelectedTiles = new bool[1,1];

        Point CopiedSize = new Point(0, 0);
        Tile[,] Copied = new Tile[1, 1];

        SpriteBatch spriteBatch;

        public Core()
            : base()
        {
            /*if (Steam.SteamInit)
            {
                // OKAY!
            }
            else
            {
                Steam.Init();
                if (!Steam.SteamInit)
                {
                    
                }
            }*/
            Version = Assembly.GetExecutingAssembly().GetName().Version;
            VersionString = Version.Major + "." + Version.Minor;
        }

        protected override void Initialize()
        {
            base.Initialize();
            spriteBatch = new SpriteBatch(base.GraphicsDevice);
            // base.GraphicsDevice.BlendState = BlendState.

            Texture2D t = new Texture2D(base.GraphicsDevice, 1, 1);
            t.SetData<Color>(new Color[] { new Color(255, 255, 255, 255) });
            DefaultTexture = t;
            TileSize = new Vector2(16, 16);

            Window.Title = "Buildaria " + VersionString + "";
            Main.versionNumber = "Running on Terraria " + Main.versionNumber + " =)";

            SelectionOverlay = new Color(255, 100, 0, 50);

            MemoryStream stream = new MemoryStream();
            Assembly asm = Assembly.Load(new AssemblyName("Terraria"));
            /*string Path = Environment.CurrentDirectory + @"\";
            Module terrariaAsm = AssemblyFactory.GetAssembly(Path + "Terraria.exe");
            module = terrariaAsm.MainModule;*/
            WorldGenWrapper = asm.GetType("Terraria.WorldGen");
            MainWrapper = asm.GetType("Terraria.Main");
        }

        public void TileFrame(int x, int y, bool reset = false, bool breaks = true)
        {
            WorldGenWrapper.GetMethod("TileFrame").Invoke(null, new object[] { x, y, reset, !breaks });
        }

        public void SquareWallFrame(int x, int y, bool reset = false)
        {
            WorldGenWrapper.GetMethod("SquareWallFrame").Invoke(null, new object[] { x, y, reset });
        }

        public void SetBuildMode()
        {
            FieldInfo tilex = player[myPlayer].GetType().GetField("tileRangeX");
            FieldInfo tiley = player[myPlayer].GetType().GetField("tileRangeY");
            tilex.SetValue(player[myPlayer], 1000);
            tiley.SetValue(player[myPlayer], 1000);
            Inventories.Add(CreateInventory(0));
            Inventories.Add(CreateInventory(1));
            Inventories.Add(CreateInventory(2));
            LoadInventory(0);
        }

        public float inventoryScale
        {
            get
            {
                object o = MainWrapper.GetField("inventoryScale", BindingFlags.Static | BindingFlags.NonPublic).GetValue(this);
                return (float)o;
            }
        }

        int inventoryType = 0;
        public Item[] CreateInventory(int preset)
        {
            Item[] i = new Item[40];

            for (int it = 0; it < i.Length; it++)
            {
                i[it] = new Item();
            }

            i[0].SetDefaults("Copper Pickaxe");
            i[1].SetDefaults("Copper Hammer");
            i[2].SetDefaults(0xc6);
            i[2].useStyle = 0;
            i[3].SetDefaults(0xb9);

            if (preset < 0)
            {
                preset = 2;
            }
            else if (preset > 2)
            {
                preset = 0;
            }

            switch (preset)
            {
                //case 1: // Blocks2 ?

                    //break;
                case 1: // Walls
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
                    break;
                case 2: // Decor
                    
                    i[4].SetDefaults(0x19);
                    i[5].SetDefaults(0x1f);
                    i[6].SetDefaults(0x20);
                    i[7].SetDefaults(0x21);
                    i[8].SetDefaults(0x22);
                    i[9].SetDefaults(0x23);

                    i[10].SetDefaults(0x24);
                    i[11].SetDefaults(0x30);
                    i[12].SetDefaults(0x3f);
                    i[13].SetDefaults(0x57);
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
                    i[32].SetDefaults(50);
                    i[32].mana = 0;
                    i[33].SetDefaults(0);
                    i[34].SetDefaults(0);
                    i[35].SetDefaults(0);
                    i[36].SetDefaults(0);
                    i[37].SetDefaults(0);
                    i[38].SetDefaults(0);
                    break; // Blocks
                default:
                    preset = 0;
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
                    break;
            }
            // i[47].SetDefaults(0x36);

            return i;
        }

        public int LoadInventory(int id)
        {
            if (id < 0)
            {
                id = 2;
            }
            else if (id > 2)
            {
                id = 0;
            }

            for (int i = 0; i < Inventories[id].Length; i++)
            {
                player[myPlayer].inventory[i].SetDefaults(Inventories[id][i].type);
            }

            return inventoryType = id;
        }

        int oldMenuMode = 0;
        bool hover = false;
        Vector2 lastPosition = Vector2.Zero;
        KeyboardState oldKeyState = Keyboard.GetState();
        protected override void Update(GameTime gameTime)
        {
            player[myPlayer].armor[3].SetDefaults(0x36);
            player[myPlayer].armor[4].SetDefaults(0x35);
            player[myPlayer].armor[5].SetDefaults(0x9f);
            player[myPlayer].armor[6].SetDefaults(0x80);
            player[myPlayer].armor[7].SetDefaults(0x9e);

            try
            {
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

                    if (it.name != "Magic Mirror")
                    {
                        it.stack = 255;
                        it.autoReuse = true;
                        it.useTime = 0;
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

            if (netMode == 2)
                return;
            if (menuMultiplayer)
            {
                menuMultiplayer = false;
                menuMode = 0;
            }
            if (menuMode == 10 && oldMenuMode != menuMode)
            {
                SetBuildMode();
            }
            else if (menuMode != 10 && oldMenuMode == 10)
            {

            }
            else if (menuMode == 10)
            {
                try
                {
                    bool shift = keyState.IsKeyDown(Keys.LeftShift) || keyState.IsKeyDown(Keys.RightShift);
                    bool alt = keyState.IsKeyDown(Keys.LeftAlt) || keyState.IsKeyDown(Keys.RightAlt);
                    bool ctrl = keyState.IsKeyDown(Keys.LeftControl) || keyState.IsKeyDown(Keys.RightControl);

                    bool onItem = false;
                    for (int i = 0; i < 11; i++)
                    {
                        int x = (int)(20f + ((i * 0x38) * inventoryScale));
                        int y = (int)(20f + ((0 * 0x38) * inventoryScale));
                        int index = x;
                        if (((mouseState.X >= x) && (mouseState.X <= (x + (inventoryBackTexture.Width * inventoryScale)))) && ((mouseState.Y >= y) && (mouseState.Y <= (y + (inventoryBackTexture.Height * inventoryScale) + 2))))
                        {
                            i = 11;
                            onItem = true;
                            break;
                        }
                    }

                    if (mouseState.LeftButton == ButtonState.Pressed && oldMouseState.LeftButton == ButtonState.Released && player[myPlayer].inventory[player[myPlayer].selectedItem].name.ToLower().Contains("phaseblade") && !playerInventory && !onItem)
                    {
                        int x = (int)((Main.mouseState.X + Main.screenPosition.X) / 16f);
                        int y = (int)((Main.mouseState.Y + Main.screenPosition.Y) / 16f);
                        sel1 = new Vector2(x, y);
                    }
                    if (mouseState.LeftButton == ButtonState.Pressed && player[myPlayer].inventory[player[myPlayer].selectedItem].name.ToLower().Contains("phaseblade") && !playerInventory && !onItem)
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

                    if (mouseState.RightButton == ButtonState.Pressed && player[myPlayer].inventory[player[myPlayer].selectedItem].name.ToLower().Contains("phaseblade") && !playerInventory && !onItem)
                    {
                        sel1 = -Vector2.One;
                        sel2 = -Vector2.One;
                    }

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

                    if (alt && mouseState.LeftButton == ButtonState.Released)
                    {
                        // circle!
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
                                /*double angle = Math.Atan2(y - center.Y, x - center.X);
                                double xmax = (Math.Cos(angle) * center.X) + center.X;
                                double ymax = (Math.Sin(angle) * center.Y) + center.Y;
                                if ((x >= xmax && y >= ymax && x < center.X + 1 && y < center.Y + 1) ||
                                    (x <= xmax && y >= ymax && x > center.X - 1 && y < center.Y + 1) ||
                                    (x >= xmax && y <= ymax && x < center.X + 1 && y > center.Y - 1) ||
                                    (x <= xmax && y <= ymax && x > center.X - 1 && y > center.Y - 1))
                                {
                                    SelectedTiles[x, y] = true;
                                }*/
                                double dx = (x - center.X + 1) / center.X;
                                double dy = (y - center.Y + 1) / center.Y;
                                if (dx * dx + dy * dy < 1)
                                {
                                    SelectedTiles[x, y] = true;
                                }
                                /*if (
                                {
                                }
                                else
                                {
                                    SelectedTiles[x, y] = false;
                                }*/
                            }
                        }
                    }
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

                    if (keyState.IsKeyDown(Keys.OemOpenBrackets) && !oldKeyState.IsKeyDown(Keys.OemOpenBrackets))
                    {
                        for (int i = 0; i < Inventories[inventoryType].Length; i++)
                        {
                            player[myPlayer].inventory[i].SetDefaults(Inventories[inventoryType][i].type);
                        }
                        LoadInventory(inventoryType - 1);
                    }
                    if (keyState.IsKeyDown(Keys.OemCloseBrackets) && !oldKeyState.IsKeyDown(Keys.OemCloseBrackets))
                    {
                        for (int i = 0; i < Inventories[inventoryType].Length; i++)
                        {
                            player[myPlayer].inventory[i].SetDefaults(Inventories[inventoryType][i].type);
                        }
                        LoadInventory(inventoryType + 1);
                    }

                    if (keyState.IsKeyDown(Keys.N) && !oldKeyState.IsKeyDown(Keys.N))
                    {
                        if (dayTime)
                            time = dayLength + 1;
                        else
                            time = nightLength;
                    }

                    if (ctrl && keyState.IsKeyDown(Keys.C))
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

                    if (ctrl && keyState.IsKeyDown(Keys.V))
                    {
                        if (sel1 != -Vector2.One && sel2 != -Vector2.One)
                        {
                            for (int x = 0; x < CopiedSize.X; x++)
                            {
                                for (int y = 0; y < CopiedSize.Y; y++)
                                {
                                    try
                                    {
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

                    player[myPlayer].statLife = 400;
                    player[myPlayer].statMana = 200;
                    player[myPlayer].immune = false;
                    player[myPlayer].immuneTime = 0;
                    player[myPlayer].dead = false;
                    player[myPlayer].rocketTime = 1000;
                    player[myPlayer].fallStart = (int)player[myPlayer].position.Y;
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
                    if (keyState.IsKeyDown(Keys.P) && !oldKeyState.IsKeyDown(Keys.P))
                    {
                        hover = !hover;
                        player[myPlayer].fallStart = (int)player[myPlayer].position.Y;
                        player[myPlayer].immune = true;
                        player[myPlayer].immuneTime = 1000;
                    }

                    if (mouseState.LeftButton == ButtonState.Pressed && player[myPlayer].inventory[player[myPlayer].selectedItem].createTile >= 0 && !playerInventory && !onItem)
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
                    else if (mouseState.LeftButton == ButtonState.Pressed && player[myPlayer].inventory[player[myPlayer].selectedItem].createWall >= 0 && !playerInventory && !onItem)
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
                    else if (mouseState.RightButton == ButtonState.Pressed && player[myPlayer].inventory[player[myPlayer].selectedItem].name.ToLower().Contains("phaseblade") && !playerInventory && !onItem)
                    {
                        sel1 = -Vector2.One;
                        sel2 = -Vector2.One;
                    }
                        // Wipes
                    else if (mouseState.RightButton == ButtonState.Pressed && player[myPlayer].inventory[player[myPlayer].selectedItem].pick >= 55 && !playerInventory && !onItem)
                    {
                        for (int xp = 0; xp < SelectionSize.X; xp++)
                        {
                            for (int yp = 0; yp < SelectionSize.Y; yp++)
                            {
                                int x = xp + SelectionPosition.X;
                                int y = yp + SelectionPosition.Y;
                                if (SelectedTiles[xp, yp])
                                {
                                    if (Main.tile[x, y] == null)
                                        Main.tile[x, y] = new Tile();

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
                    else if (mouseState.RightButton == ButtonState.Pressed && player[myPlayer].inventory[player[myPlayer].selectedItem].hammer >= 55 && !playerInventory && !onItem)
                    {
                        for (int xp = 0; xp < SelectionSize.X; xp++)
                        {
                            for (int yp = 0; yp < SelectionSize.Y; yp++)
                            {
                                int x = xp + SelectionPosition.X;
                                int y = yp + SelectionPosition.Y;
                                if (SelectedTiles[xp, yp])
                                {
                                    if (Main.tile[x, y] == null)
                                        Main.tile[x, y] = new Tile();

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
                        // Liquids
                    else if (mouseState.RightButton == ButtonState.Pressed && oldMouseState.RightButton == ButtonState.Released && player[myPlayer].inventory[player[myPlayer].selectedItem].type == 0xcf && !playerInventory && !onItem)
                    {
                        for (int xp = 0; xp < SelectionSize.X; xp++)
                        {
                            for (int yp = 0; yp < SelectionSize.Y; yp++)
                            {
                                int x = xp + SelectionPosition.X;
                                int y = yp + SelectionPosition.Y;
                                if (SelectedTiles[xp, yp])
                                {
                                    if (Main.tile[x, y] == null)
                                        Main.tile[x, y] = new Tile();

                                    Main.tile[x, y].liquid = 255;
                                    Main.tile[x, y].lava = true;
                                    TileFrame(x, y);
                                    SquareWallFrame(x, y, true);
                                }
                            }
                        }
                    }
                    else if (mouseState.RightButton == ButtonState.Pressed && oldMouseState.RightButton == ButtonState.Released && player[myPlayer].inventory[player[myPlayer].selectedItem].type == 0xce && !playerInventory && !onItem)
                    {
                        for (int xp = 0; xp < SelectionSize.X; xp++)
                        {
                            for (int yp = 0; yp < SelectionSize.Y; yp++)
                            {
                                int x = xp + SelectionPosition.X;
                                int y = yp + SelectionPosition.Y;
                                if (SelectedTiles[xp, yp])
                                {
                                    if (Main.tile[x, y] == null)
                                        Main.tile[x, y] = new Tile();

                                    Main.tile[x, y].liquid = 255;
                                    Main.tile[x, y].lava = false;
                                    TileFrame(x, y);
                                    SquareWallFrame(x, y, true);
                                }
                            }
                        }
                    }
                    else if (mouseState.RightButton == ButtonState.Pressed && oldMouseState.RightButton == ButtonState.Released && player[myPlayer].inventory[player[myPlayer].selectedItem].type == 0xcd && !playerInventory && !onItem)
                    {
                        for (int xp = 0; xp < SelectionSize.X; xp++)
                        {
                            for (int yp = 0; yp < SelectionSize.Y; yp++)
                            {
                                int x = xp + SelectionPosition.X;
                                int y = yp + SelectionPosition.Y;
                                if (SelectedTiles[xp, yp])
                                {
                                    if (Main.tile[x, y] == null)
                                        Main.tile[x, y] = new Tile();

                                    Main.tile[x, y].liquid = 0;
                                    Main.tile[x, y].lava = false;
                                    TileFrame(x, y);
                                    SquareWallFrame(x, y, true);
                                }
                            }
                        }
                    }
                        // Tile/Walls
                    else if (mouseState.RightButton == ButtonState.Pressed && oldMouseState.RightButton == ButtonState.Released && player[myPlayer].inventory[player[myPlayer].selectedItem].createTile >= 0 && !playerInventory && !onItem)
                    {
                        for (int xp = 0; xp < SelectionSize.X; xp++)
                        {
                            for (int yp = 0; yp < SelectionSize.Y; yp++)
                            {
                                int x = xp + SelectionPosition.X;
                                int y = yp + SelectionPosition.Y;
                                if (SelectedTiles[xp, yp])
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
                        }
                    }
                    else if (mouseState.RightButton == ButtonState.Pressed && oldMouseState.RightButton == ButtonState.Released && player[myPlayer].inventory[player[myPlayer].selectedItem].createWall >= 0 && !playerInventory && !onItem)
                    {
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
                                        Main.tile[x, y].type = 0;
                                    }

                                    Main.tile[x, y].wall = (byte)player[myPlayer].inventory[player[myPlayer].selectedItem].createWall;
                                    TileFrame(x, y);
                                    SquareWallFrame(x, y, true);
                                }
                            }
                        }
                    }

                    /*if (sel1.X > sel2.X && sel1.Y > sel2.Y)
                    {
                        Vector2 tmp = sel1;
                        sel1 = sel2;
                        sel2 = tmp;
                    }
                    else if (sel1.X > sel2.X && sel2.Y > sel1.Y)
                    {
                        sel1 = new Vector2(sel2.X, sel1.Y);
                        sel2 = new Vector2(sel1.X, sel2.Y);
                    }
                    else if (sel1.Y > sel2.Y && sel2.X > sel1.X)
                    {
                        sel1 = new Vector2(sel1.X, sel2.Y);
                        sel2 = new Vector2(sel2.X, sel1.Y);
                    }*/

                    foreach (NPC n in npc)
                    {
                        if (!n.friendly)
                        {
                            n.life = 0;
                            n.UpdateNPC(0);
                        }
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

        public void DrawSelectionOverlay()
        {
            /*Vector2 topLeft = new Vector2(Math.Min(sel1.X, sel2.X), Math.Min(sel2.Y, sel1.Y));
            Vector2 botRight = new Vector2(Math.Max(sel1.X, sel2.X), Math.Max(sel2.Y, sel1.Y));
            Vector2 size = (botRight - topLeft) * TileSize;
            Vector2 offset = new Vector2(((int)(screenPosition.X)), ((int)(screenPosition.Y)));
            Vector2 pos = ((topLeft * TileSize)) - offset;
            int x = (int)((Main.mouseState.X + Main.screenPosition.X) / 16f);
            int y = (int)((Main.mouseState.Y + Main.screenPosition.Y) / 16f);
            spriteBatch.Draw(DefaultTexture, pos, null, SelectionOverlay, 0, Vector2.Zero, size, SpriteEffects.None, 0);
            Color alter = new Color(SelectionOverlay.R, SelectionOverlay.G, SelectionOverlay.B, 255);
            spriteBatch.Draw(DefaultTexture, pos, null, alter, 0, Vector2.Zero, new Vector2(1, size.Y), SpriteEffects.None, 0);
            spriteBatch.Draw(DefaultTexture, pos, null, alter, 0, Vector2.Zero, new Vector2(size.X, 1), SpriteEffects.None, 0);
            spriteBatch.Draw(DefaultTexture, new Vector2((int)pos.X, (int)pos.Y + (int)size.Y), null, alter, 0, Vector2.Zero, new Vector2((int)size.X + 1, 1), SpriteEffects.None, 0);
            spriteBatch.Draw(DefaultTexture, new Vector2((int)pos.X + (int)size.X, (int)pos.Y), null, alter, 0, Vector2.Zero, new Vector2(1, (int)size.Y + 1), SpriteEffects.None, 0);*/

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
    }
}
