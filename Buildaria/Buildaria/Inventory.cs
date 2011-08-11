using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

using Terraria;

namespace Buildaria
{
    public struct InventoryItem
    {
        public string Name;
        public int ID;
    }

    public class Inventory
    {
        #region Static

        public const string INVENTORY_CONFIG_FILE = "Inventories.xml";

        public static List<Inventory> Inventories = new List<Inventory>();

        public static Inventory GetInventory(string name)
        {
            foreach (Inventory i in Inventories)
            {
                if (i.Name == name)
                {
                    return i;
                }
            }
            return new Inventory();
        }

        public static Inventory GetInventory(int id)
        {
            try 
            {
                return Inventories[id];
            }
            catch
            {
                return new Inventory();
            }
        }

        public static void AddInventory(Inventory i)
        {
            if (!Inventories.Contains(i))
                Inventories.Add(i);
        }

        public static void LoadInventories(string file = INVENTORY_CONFIG_FILE)
        {
            if (!File.Exists(file))
            {
                Core.CreateInventories();
                SaveInventories(file);
                return;
            }

            FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read);
            XmlSerializer x = new XmlSerializer(Inventories.GetType());
            Inventories = (List<Inventory>)x.Deserialize(fs);
            fs.Close();

            Main.NewText("Loaded Inventories File", 255, 255, 255);
        }

        public static void SaveInventories(string file = INVENTORY_CONFIG_FILE)
        {
            FileStream fs = new FileStream(file, FileMode.Create, FileAccess.Write);
            XmlSerializer x = new XmlSerializer(Inventories.GetType());
            x.Serialize(fs, Inventories);
            fs.Close();

            Main.NewText("Saved Inventories File", 255, 255, 255);
        }

        #endregion

        public string Name { get; set; }
        public bool ItemHax { get; set; }
        public bool GodMode { get; set; }
        public bool NPCs { get; set; }
        public bool BuildMode { get; set; }
        public bool ItemDrops { get; set; }
        public InventoryItem[] Items { get; set; }

        public void Default()
        {
            Name = "Inventory";
            ItemHax = true;
            GodMode = true;
            NPCs = false;
            BuildMode = true;
            ItemDrops = false;
            Items = new InventoryItem[0];
        }

        public Inventory()
        {
            Default();
        }

        public Inventory(Item[] items)
        {
            Default();
            Items = ItemArrayToIIArray(items);
        }

        public Inventory(string name)
        {
            Default();
            Name = name;
        }

        public Inventory(Item[] items, string name)
        {
            Default();
            Items = ItemArrayToIIArray(items);
            Name = name;
        }

        public static InventoryItem[] ItemArrayToIIArray(Item[] items)
        {
            InventoryItem[] iis = new InventoryItem[items.Length];

            for (int i = 0; i < iis.Length; i++)
            {
                iis[i].Name = items[i].name;
                iis[i].ID = items[i].type;
            }

            return iis;
        }

        public static Item[] IIArrayToItemArray(InventoryItem[] iis)
        {
            Item[] items = new Item[iis.Length];

            for (int i = 0; i < iis.Length; i++)
            {
                items[i] = new Item();
                items[i].SetDefaults(iis[i].Name);
                if (items[i].type == 0 || iis[i].Name == "")
                {
                    if (iis[i].ID != 0)
                    {
                        iis[i].ID = iis[i].ID;
                    }
                    items[i].SetDefaults(iis[i].ID);
                    if (items[i].type == 0 || items[i].name == "")
                    {
                        items[i].SetDefaults(0);
                        items[i].name = "";
                        items[i].stack = 0;
                    }
                }
            }

            return items;
        }
    }
}
