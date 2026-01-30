using Archipelago.Core.Util;
using Avalonia.Media;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAP.Models
{
    /* Item lots as they are written into memory */
    internal class ItemLotParam
    {

        [MemoryOffset(0x00)]
        public int lotItemId01 { get; set; }
        [MemoryOffset(0x04)]
        public int lotItemId02 { get; set; }
        [MemoryOffset(0x08)]
        public int lotItemId03 { get; set; }
        [MemoryOffset(0x0C)]
        public int lotItemId04 { get; set; }
        [MemoryOffset(0x10)]
        public int lotItemId05 { get; set; }
        [MemoryOffset(0x14)]
        public int lotItemId06 { get; set; }
        [MemoryOffset(0x18)]
        public int lotItemId07 { get; set; }
        [MemoryOffset(0x1C)]
        public int lotItemId08 { get; set; }

        [MemoryOffset(0x20)]
        public int lotItemCategory01 { get; set; }
        [MemoryOffset(0x24)]
        public int lotItemCategory02 { get; set; }
        [MemoryOffset(0x28)]
        public int lotItemCategory03 { get; set; }
        [MemoryOffset(0x2C)]
        public int lotItemCategory04 { get; set; }
        [MemoryOffset(0x30)]
        public int lotItemCategory05 { get; set; }
        [MemoryOffset(0x34)]
        public int lotItemCategory06 { get; set; }
        [MemoryOffset(0x38)]
        public int lotItemCategory07 { get; set; }
        [MemoryOffset(0x3C)]
        public int lotItemCategory08 { get; set; }

        [MemoryOffset(0x40)]
        public int lotItemBasePoint01 { get; set; }
        [MemoryOffset(0x42)]
        public int lotItemBasePoint02 { get; set; }
        [MemoryOffset(0x44)]
        public int lotItemBasePoint03 { get; set; }
        [MemoryOffset(0x46)]
        public int lotItemBasePoint04 { get; set; }
        [MemoryOffset(0x48)]
        public int lotItemBasePoint05 { get; set; }
        [MemoryOffset(0x4A)]
        public int lotItemBasePoint06 { get; set; }
        [MemoryOffset(0x4C)]
        public int lotItemBasePoint07 { get; set; }
        [MemoryOffset(0x4E)]
        public int lotItemBasePoint08 { get; set; }

        [MemoryOffset(0x50)]
        public int cumulateLotPoint01 { get; set; }
        [MemoryOffset(0x52)]
        public int cumulateLotPoint02 { get; set; }
        [MemoryOffset(0x54)]
        public int cumulateLotPoint03 { get; set; }
        [MemoryOffset(0x56)]
        public int cumulateLotPoint04 { get; set; }
        [MemoryOffset(0x58)]
        public int cumulateLotPoint05 { get; set; }
        [MemoryOffset(0x5A)]
        public int cumulateLotPoint06 { get; set; }
        [MemoryOffset(0x5C)]
        public int cumulateLotPoint07 { get; set; }
        [MemoryOffset(0x5E)]
        public int cumulateLotPoint08 { get; set; }

        [MemoryOffset(0x60)]
        public int getItemFlagId01 { get; set; }
        [MemoryOffset(0x64)]
        public int getItemFlagId02 { get; set; }
        [MemoryOffset(0x68)]
        public int getItemFlagId03 { get; set; }
        [MemoryOffset(0x6C)]
        public int getItemFlagId04 { get; set; }
        [MemoryOffset(0x70)]
        public int getItemFlagId05 { get; set; }
        [MemoryOffset(0x74)]
        public int getItemFlagId06 { get; set; }
        [MemoryOffset(0x78)]
        public int getItemFlagId07 { get; set; }
        [MemoryOffset(0x7C)]
        public int getItemFlagId08 { get; set; }

        [MemoryOffset(0x80)]
        public int getItemFlagId { get; set; }

        [MemoryOffset(0x84)]
        public int cumulateNumFlagId { get; set; }

        [MemoryOffset(0x88)]
        public byte cumulateNumMax { get; set; }
        [MemoryOffset(0x89)]
        public byte lotItem_Rarity { get; set; }

        [MemoryOffset(0x8A)]
        public byte lotItemNum01 { get; set; }
        [MemoryOffset(0x8B)]
        public byte lotItemNum02 { get; set; }
        [MemoryOffset(0x8C)]
        public byte lotItemNum03 { get; set; }
        [MemoryOffset(0x8D)]
        public byte lotItemNum04 { get; set; }
        [MemoryOffset(0x8E)]
        public byte lotItemNum05 { get; set; }
        [MemoryOffset(0x8F)]
        public byte lotItemNum06 { get; set; }
        [MemoryOffset(0x90)]
        public byte lotItemNum07 { get; set; }
        [MemoryOffset(0x91)]
        public byte lotItemNum08 { get; set; }

        [MemoryOffset(0x92)]
        public byte enableLuck { get; set; }
        [MemoryOffset(0x93)]
        public byte cumulateReset { get; set; }


        override public string ToString()
        {
            string result = $"{{\"ItemFlagId\":{getItemFlagId}, \"cumulateNumFlagId\":{cumulateNumFlagId}, \"cumulateNumMax\":{cumulateNumMax}, \"rarity\":{lotItem_Rarity}, \"items\": [";
            result += $"{{\"id\":{lotItemId01}, \"cat\":{lotItemCategory01.ToString("X")}, \"BasePoint\":{lotItemBasePoint01}, \"lotItemNum\":{lotItemNum01}, \"cumulateLotPoint\":{cumulateLotPoint01}, \"gifid\":{getItemFlagId01}}}, ";
            result += $"{{\"id\":{lotItemId02}, \"cat\":{lotItemCategory02.ToString("X")}, \"BasePoint\":{lotItemBasePoint02}, \"lotItemNum\":{lotItemNum02}, \"cumulateLotPoint\":{cumulateLotPoint02}, \"gifid\":{getItemFlagId02}}}, ";
            result += $"{{\"id\":{lotItemId03}, \"cat\":{lotItemCategory03.ToString("X")}, \"BasePoint\":{lotItemBasePoint03}, \"lotItemNum\":{lotItemNum03}, \"cumulateLotPoint\":{cumulateLotPoint03}, \"gifid\":{getItemFlagId03}}}, ";
            result += $"{{\"id\":{lotItemId04}, \"cat\":{lotItemCategory04.ToString("X")}, \"BasePoint\":{lotItemBasePoint04}, \"lotItemNum\":{lotItemNum04}, \"cumulateLotPoint\":{cumulateLotPoint04}, \"gifid\":{getItemFlagId04}}}, ";
            result += $"{{\"id\":{lotItemId05}, \"cat\":{lotItemCategory05.ToString("X")}, \"BasePoint\":{lotItemBasePoint05}, \"lotItemNum\":{lotItemNum05}, \"cumulateLotPoint\":{cumulateLotPoint05}, \"gifid\":{getItemFlagId05}}}, ";
            result += $"{{\"id\":{lotItemId06}, \"cat\":{lotItemCategory06.ToString("X")}, \"BasePoint\":{lotItemBasePoint06}, \"lotItemNum\":{lotItemNum06}, \"cumulateLotPoint\":{cumulateLotPoint06}, \"gifid\":{getItemFlagId06}}}, ";
            result += $"{{\"id\":{lotItemId07}, \"cat\":{lotItemCategory07.ToString("X")}, \"BasePoint\":{lotItemBasePoint07}, \"lotItemNum\":{lotItemNum07}, \"cumulateLotPoint\":{cumulateLotPoint07}, \"gifid\":{getItemFlagId07}}}, ";
            result += $"{{\"id\":{lotItemId08}, \"cat\":{lotItemCategory08.ToString("X")}, \"BasePoint\":{lotItemBasePoint08}, \"lotItemNum\":{lotItemNum08}, \"cumulateLotPoint\":{cumulateLotPoint08}, \"gifid\":{getItemFlagId08}}}";
            result += $"]}}";
            return result;
        }
        public string ToString(List<DarkSoulsItem> allitems)
        {
            string result = $"{{\"ItemFlagId\":{getItemFlagId}, \"cumulateNumFlagId\":{cumulateNumFlagId}, \"cumulateNumMax\":{cumulateNumMax}, \"rarity\":{lotItem_Rarity}, \"items\": [";
            if (lotItemId01 > 0)
            {
                string itemname = allitems.Find(x => x.Id == lotItemId01 && (int)x.Category == lotItemCategory01)?.Name ?? "unknown";
                result += $"{{\"name\":\"{itemname}\", \"id\":{lotItemId01}, \"cat\":{lotItemCategory01.ToString("X")}, \"lotItemNum\":{lotItemNum01}, \"BasePoint\":{lotItemBasePoint01}, \"cumulateLotPoint\":{cumulateLotPoint01}, \"gifid\":{getItemFlagId01}}}, ";
            }
            if (lotItemId02 > 0)
            {
                string itemname = allitems.Find(x => x.Id == lotItemId02 && (int)x.Category == lotItemCategory02)?.Name ?? "unknown";
                result += $"{{\"name\":\"{itemname}\", \"id\":{lotItemId02}, \"cat\":{lotItemCategory02.ToString("X")}, \"lotItemNum\":{lotItemNum02}, \"BasePoint\":{lotItemBasePoint02}, \"cumulateLotPoint\":{cumulateLotPoint02}, \"gifid\":{getItemFlagId02}}}, ";
            }
            if (lotItemId03 > 0)
            {
                string itemname = allitems.Find(x => x.Id == lotItemId03 && (int)x.Category == lotItemCategory03)?.Name ?? "unknown";
                result += $"{{\"name\":\"{itemname}\", \"id\":{lotItemId03}, \"cat\":{lotItemCategory03.ToString("X")}, \"lotItemNum\":{lotItemNum03}, \"BasePoint\":{lotItemBasePoint03}, \"cumulateLotPoint\":{cumulateLotPoint03}, \"gifid\":{getItemFlagId03}}}, ";
            }
            if (lotItemId04 > 0)
            {
                string itemname = allitems.Find(x => x.Id == lotItemId04 && (int)x.Category == lotItemCategory04)?.Name ?? "unknown";
                result += $"{{\"name\":\"{itemname}\", \"id\":{lotItemId04}, \"cat\":{lotItemCategory04.ToString("X")}, \"lotItemNum\":{lotItemNum04}, \"BasePoint\":{lotItemBasePoint04}, \"cumulateLotPoint\":{cumulateLotPoint04}, \"gifid\":{getItemFlagId04}}}, ";
            }
            if (lotItemId05 > 0)
            {
                string itemname = allitems.Find(x => x.Id == lotItemId05 && (int)x.Category == lotItemCategory05)?.Name ?? "unknown";
                result += $"{{\"name\":\"{itemname}\", \"id\":{lotItemId05}, \"cat\":{lotItemCategory05.ToString("X")}, \"lotItemNum\":{lotItemNum05}, \"BasePoint\":{lotItemBasePoint05}, \"cumulateLotPoint\":{cumulateLotPoint05}, \"gifid\":{getItemFlagId05}}}, ";
            }
            if (lotItemId06 > 0)
            {
                string itemname = allitems.Find(x => x.Id == lotItemId06 && (int)x.Category == lotItemCategory06)?.Name ?? "unknown";
                result += $"{{\"name\":\"{itemname}\", \"id\":{lotItemId06}, \"cat\":{lotItemCategory06.ToString("X")}, \"lotItemNum\":{lotItemNum06}, \"BasePoint\":{lotItemBasePoint06}, \"cumulateLotPoint\":{cumulateLotPoint06}, \"gifid\":{getItemFlagId06}}}, ";
            }
            if (lotItemId07 > 0)
            {
                string itemname = allitems.Find(x => x.Id == lotItemId07 && (int)x.Category == lotItemCategory07)?.Name ?? "unknown";
                result += $"{{\"name\":\"{itemname}\", \"id\":{lotItemId07}, \"cat\":{lotItemCategory07.ToString("X")}, \"lotItemNum\":{lotItemNum07}, \"BasePoint\":{lotItemBasePoint07}, \"cumulateLotPoint\":{cumulateLotPoint07}, \"gifid\":{getItemFlagId07}}}, ";
            }
            if (lotItemId08 > 0)
            {
                string itemname = allitems.Find(x => x.Id == lotItemId08 && (int)x.Category == lotItemCategory08)?.Name ?? "unknown";
                result += $"{{\"name\":\"{itemname}\", \"id\":{lotItemId08}, \"cat\":{lotItemCategory08.ToString("X")}, \"lotItemNum\":{lotItemNum08}, \"BasePoint\":{lotItemBasePoint08}, \"cumulateLotPoint\":{cumulateLotPoint08}, \"gifid\":{getItemFlagId08}}}, ";
            }
            result += $"]}}";
            return result;
        }
    }
}
