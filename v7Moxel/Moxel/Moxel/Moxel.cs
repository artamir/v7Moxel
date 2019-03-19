﻿using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System;
using System.Xml.Serialization;

namespace Moxel
{
    [ComVisible(true)]
    public enum SaveFormat
    {
        Excel = 1,
        Html,
        PDF,
        XML
    }

    [ComVisible(false)]
    [Serializable]
    public partial class Moxel
    {
        const int UNITS_PER_INCH = 1440;
        const int UNITS_PER_PIXEL = UNITS_PER_INCH / 96;
        const int LOGPIXELSX = 88;
        const int LOGPIXELSY = 90;
        const int MOXEL_UNITS_PER_INCH = 288;
        const float CoordsCoeff = UNITS_PER_INCH / MOXEL_UNITS_PER_INCH;

        /// <summary>
        /// Версия формата
        /// </summary>
        public int Version = 6;

        /// <summary>
        /// всего колонок в таблице
        /// </summary>
        public int nAllColumnCount;

        /// <summary>
        /// всего строк в таблице
        /// </summary>
        public int nAllRowCount;

        /// <summary>
        /// Всего объектов
        /// </summary>
        public int nAllObjectsCount;

        /// <summary>
        /// Формат ячейки по умолчанию для всей таблицы
        /// </summary>
        public Cellv6 DefFormat;

        /// <summary>
        /// Список шрифтов
        /// </summary>
        public SerializableDictionary<int, LOGFONT> FontList = new SerializableDictionary<int, LOGFONT>();

        //public FontList FontList;

        /// <summary>
        /// Список строк
        /// </summary>
        public SerializableDictionary<int, string> stringTable = new SerializableDictionary<int, string>();
        /// <summary>
        /// Верхний колонтитул
        /// </summary>
        public DataCell TopColon;

        /// <summary>
        /// Нижний колонтитул
        /// </summary>
        public DataCell BottomColon;

        /// <summary>
        /// Форматные ячейки колонок
        /// </summary>
        public SerializableDictionary<int, DataCell> Columns = new SerializableDictionary<int, DataCell>();

        /// <summary>
        /// Строки
        /// </summary>
        public SerializableDictionary<int, MoxelRow> Rows = new SerializableDictionary<int, MoxelRow>();

        /// <summary>
        /// Список встроенных объектов
        /// </summary>
        public List<EmbeddedObject> Objects = new List<EmbeddedObject>();

        /// <summary>
        /// Объединенные ячейки
        /// </summary>
        public List<CellsUnion> Unions = new List<CellsUnion>();

        /// <summary>
        /// Вертикальные секции
        /// </summary>
        public List<Section> VerticalSections = new List<Section>();

        /// <summary>
        /// Горизонтальные секции
        /// </summary>
        public List<Section> HorisontalSections = new List<Section>();

        /// <summary>
        /// Горизонтальные разрывы
        /// </summary>
        public int[] HorisontalPageBreaks = new int[0];

        /// <summary>
        /// Вертикальные разрывы
        /// </summary>
        public int[] VerticalPageBreaks = new int[0];

        /// <summary>
        /// Именованные области
        /// </summary>
        public List<MoxelArea> AreaNames = new List<MoxelArea>();

        public int GetColumnWidth(int ColNumber)
        {
            int result = 0;
            if (DefFormat.dwFlags.HasFlag(MoxelCellFlags.ColumnWidth))
                result = DefFormat.wWidth;

            if (Columns.ContainsKey(ColNumber))
                if (Columns[ColNumber].FormatCell.dwFlags.HasFlag(MoxelCellFlags.ColumnWidth))
                    result = Columns[ColNumber].FormatCell.wWidth;

            if (result == 0)
                result = 40;

            return result;
        }

        public int GetWidth(int x1, int x2)
        {
            int width = 0;
            for (int i = x1; i < x2; i++)
                width += GetColumnWidth(i);

            return width;
        }

        public int GetRowHeight(int RowNumber)
        {
            int result = 0;

            if (Rows.ContainsKey(RowNumber))
                result = Rows[RowNumber].Height;

            if (result == 0)
                if (DefFormat.dwFlags.HasFlag(MoxelCellFlags.RowHeight))
                    result = DefFormat.wHeight;

            if (result == 0)
                result = 45;

            return result;
        }

        public int GetHeight(int y1, int y2)
        {
            int height = 0;
            for (int i = y1; i < y2; i++)
                height += GetRowHeight(i);

            return height;
        }

        const short BMPSignature = 0x4D42; // "BM"
        const uint WMFSignature = 0x9AC6CDD7; // placeable WMF

        public Moxel()
        { }

        public Moxel(string FileName)
        {
            Load(FileName);
        }

        public Moxel(ref byte[] buf)
        {
            Load(ref buf);
        }

        public void Load(string FileName) 
        {
            if (new FileInfo(FileName).Length <= 1024)
            {
                byte[] buffer = File.ReadAllBytes(FileName);
                Load(ref buffer);
                buffer = null;
            }
            else
            {
                using (var fs = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    Load(fs);
                }
            }
        }

        public void Load(ref byte[] buf)
        {
            using (var ms = new MemoryStream(buf))
            {
                Load(ms);
            }
        }

        public void Load(Stream ms)
        {
            using (var br = new BinaryReader(ms))
            {
                Load(br);
            }
        }

        public void Load(BinaryReader br)
        {
            stringTable = new SerializableDictionary<int, string>();
            br.BaseStream.Seek(0xb, SeekOrigin.Begin);
            Version = br.ReadInt16();

            nAllColumnCount = br.ReadInt32();
            //Всего строк
            nAllRowCount = br.ReadInt32();
            //Всего объектов
            nAllObjectsCount = br.ReadInt32();
            DefFormat = br.Read<DataCell>(this);
            FontList = br.ReadDictionary<LOGFONT>();

            int[] strnums = br.ReadIntArray();
            int stlCount = br.ReadCount();
            foreach (int num in strnums)
                stringTable.Add(num, br.ReadCString());

            TopColon = br.Read<DataCell>(this);
            BottomColon = br.Read<DataCell>(this);

            Columns = br.ReadDictionary<DataCell>(this);
            Rows = br.ReadDictionary<MoxelRow>(this);
            Objects = br.ReadList<EmbeddedObject>(this);
            Unions = br.ReadList<CellsUnion>();
            VerticalSections = br.ReadList<Section>();
            HorisontalSections = br.ReadList<Section>();
            HorisontalPageBreaks = br.ReadIntArray();
            VerticalPageBreaks = br.ReadIntArray();
            AreaNames = br.ReadList<MoxelArea>();
        }

         ~Moxel()
        {
            stringTable = null;
            FontList = null;
            Columns = null;
            Rows = null;
            Objects = null;
            Unions = null;
            VerticalSections = null;
            HorisontalSections = null;
            HorisontalPageBreaks = null;
            VerticalPageBreaks = null;
            AreaNames = null;
        }

        public bool SaveAs(string filename, SaveFormat format)
        {
            switch(format)
            {
                case SaveFormat.Excel:
                    return ExcelWriter.Save(this, filename);
                case SaveFormat.Html:
                    return HtmlWriter.Save(this, filename);
                case SaveFormat.PDF:
                    return PDFWriter.Save(this, filename);
                case SaveFormat.XML:
                    return this.Save(filename);
                default:
                    throw new System.Exception("Формат сохранения не поддерживается.");
            }
            
        }

        private bool Save(string filename)
        {
            using (FileStream fs = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                XmlSerializer formatter = new XmlSerializer(this.GetType());
                formatter.Serialize(fs, this);
            }
            return true;
        }
    }
}
