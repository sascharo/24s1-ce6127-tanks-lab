using System;
using System.IO;
using System.Linq;
#if USE_OPENXML
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
#endif
using Newtonsoft.Json;

using Debug = UnityEngine.Debug;

namespace CE6127.Tanks.AI
{
    /// <summary>
    /// Class <c>FileExtensions</c> extends the string class with some file related methods.
    /// </summary>
    public static class FileExtensions
    {
        /// <summary>
        /// Method <c>CombinePath</c> combines a filepath and a filename.
        /// </summary>
        public static string CombinePath(this string filepath, string filename)
        {
            string[] paths = { filepath, filename };
            return Path.Combine(paths);
        }

        /// <summary>
        /// Method <c>PrependTimestamp</c> prepends a timestamp to a filename.
        /// </summary>
        public static string PrependTimestamp(this string filepath)
        {
            string[] fileparts = { DateTime.Now.ToString(@"yyyyMMddHHmmssfff"), Path.GetFileName(filepath)};
            var aggregated = fileparts.Aggregate((partialPath, word) => $@"{partialPath}_{word}");
            return FileExtensions.CombinePath(Path.GetDirectoryName(filepath), aggregated);
        }

        /// <summary>
        /// Method <c>AppendTimestamp</c> appends a timestamp to a filename.
        /// </summary>
        public static string AppendTimestamp(this string filepath)
        {
            var path = FileExtensions.CombinePath(Path.GetDirectoryName(filepath), Path.GetFileNameWithoutExtension(filepath));
            string[] fileparts = { path, DateTime.Now.ToString(@"yyyyMMddHHmmssfff") };
            var aggregated = fileparts.Aggregate((partialPath, word) => $@"{partialPath}_{word}");
            return $@"{aggregated}.{Path.GetExtension(filepath)}";
        }
    }

#if USE_OPENXML
    /// <summary>
    /// Class <c>MatchExportXLSX</c> exports the match data to an xlsx file.
    /// </summary>
    internal class MatchExportXLSX
    {
        /// <summary>
        /// Class <c>ConfigData</c> reflecting the configdata.
        /// </summary>
        protected internal class ConfigData
        {
            /// <summary>
            /// Class <c>XLSData</c> reflecting the xlsx data.
            /// </summary>
            protected internal class XLSXData
            {
                [JsonProperty("directory")] public string Directory { get; set; }               // Implicit field for the directory to save the xlsx files to.
                [JsonProperty("timestamp")] public bool Timestamp { get; set; }                 // Implicit field for whether to add a timestamp to the xlsx file or not.
                [JsonProperty("timestampPrepend")] public bool TimestampPrepend { get; set; }   // Implicit field for whether to prepend or append the timestamp.
            }

            public XLSXData XLSX { get; set; } // Implicit field holding XLSX data.
        }

        public bool IsConfigValid = false;  // Indicates if the config file is valid.
        public string ConfigFile;           // Path to the config file.
        public ConfigData Config;           // Config data.

        // Headers of the XLSX file.
        protected string[] Headers = new[] { @"Match",
                                             @"AI Team Number", @"AI Number of Tanks", @"AI Acc. Points",
                                             @"Player Team Number", @"Player Number of Tanks", @"Player Acc. Points",
                                             @"Numer of Rounds", @"Acc. Minutes",
                                             @"Machine Name" }; // @"User Name"
        protected string SheetName;         // Name of the sheet.
        protected string Filename;          // Name of the file.
        protected string Filepath;          // Path to the file.

        private string extension = @"xlsx"; // File extension of the file.
        private MatchData m_Match;          // Match data.
        private int m_Index;                // Index of the match data.

#nullable enable
        /// <summary>
        /// Constructor <c>MatchExportXLSX</c>
        /// </summary>
        public MatchExportXLSX(string configFile, ref MatchData matchData)
        {
            ConfigFile = configFile;

            // Get a list of invalid path characters.
            char[] invalidPathChars = Path.GetInvalidPathChars();
            foreach(char inv in invalidPathChars)
                if (ConfigFile.Contains(inv))
                    return;

            IsConfigValid = File.Exists(ConfigFile);

            if (IsConfigValid)
                ReadConfig();

            m_Match = matchData;
        }
#nullable disable

        /// <summary>
        /// Method <c>ReadConfig</c> reads the config file.
        /// </summary>
        private void ReadConfig()
        {
            Config = new ConfigData();

            using (StreamReader r = new StreamReader(ConfigFile))
            {
                var jsonText = r.ReadToEnd();

                Config = JsonConvert.DeserializeObject<ConfigData>(jsonText);
            }

            // Check if the directory exists and create it if not.
            if (!Directory.Exists(Config.XLSX.Directory))
                Directory.CreateDirectory(Config.XLSX.Directory);
        }

#nullable enable
        /// <summary>
        /// Method <c>Write</c> writes the data to the xlsx file.
        /// </summary>
        public void Write(int idx, string? sheetName = null, MatchData? matchData = null, string? filename = null, string? filepath = null)
        {
            m_Index = idx;

            SheetName = sheetName == null
                ? $@"AITeam {m_Match.Results[m_Index].TeamAI.Number} vs PlayerTeam {m_Match.Results[m_Index].TeamPlayer.Number}"
                : SheetName = sheetName;
            
            if (matchData != null)
                m_Match = matchData;
            
            Filename = filename == null
                ? $@"AITeam{m_Match.Results[m_Index].TeamAI.Number}-vs-PlayerTeam{m_Match.Results[m_Index].TeamPlayer.Number}.{extension}"
                : Filename = filename;

            if (filepath == null)
            {
                Filepath = FileExtensions.CombinePath(Config.XLSX.Directory, Filename);
                if (Config.XLSX.Timestamp)
                {
                    if (Config.XLSX.TimestampPrepend)
                        Filepath = FileExtensions.PrependTimestamp(Filepath);
                    else
                        Filepath = FileExtensions.AppendTimestamp(Filepath);
                }
            }
            else
                Filepath = filepath;
#nullable disable

            SpreadsheetDocument xlsxDoc = SpreadsheetDocument.Create(Filepath, SpreadsheetDocumentType.Workbook);

            WorkbookPart workbookPart = xlsxDoc.AddWorkbookPart();
            workbookPart.Workbook = new Workbook();

            WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
            worksheetPart.Worksheet = new Worksheet();
            
            Columns columns = new Columns();
            var headersLenl = (UInt32)Headers.Length;
            columns.Append(new Column() { Min = 1, Max = 1, Width = 7.25, CustomWidth = true });
            columns.Append(new Column() { Min = 2, Max = headersLenl - 3, Width = 25, CustomWidth = true });
            columns.Append(new Column() { Min = headersLenl - 2, Max = headersLenl - 2, Width = 17.5, CustomWidth = true });
            columns.Append(new Column() { Min = headersLenl - 1, Max = headersLenl - 1, Width = 15, CustomWidth = true });
            columns.Append(new Column() { Min = headersLenl, Max = headersLenl, Width = 30, CustomWidth = true });
            worksheetPart.Worksheet.Append(columns);

            SheetData sheetData = FillData();
            worksheetPart.Worksheet.Append(sheetData);

            // Add Sheets to the Workbook.
            Sheets sheets = xlsxDoc.WorkbookPart.Workbook.AppendChild<Sheets>(new Sheets());
            // Append a new worksheet and associate it with the workbook.
            sheets.Append(new Sheet()
            {
                Id = xlsxDoc.WorkbookPart.GetIdOfPart(worksheetPart),
                SheetId = 1,
                Name = SheetName
            });

            // Save the document.
            workbookPart.Workbook.Save();
            // Close the document.
            xlsxDoc.Dispose();
        }

        /// <summary>
        /// Method <c>FillData</c> fills the data into the xlsx file.
        /// </summary>
        private SheetData FillData()
        {
            SheetData sheetData = new SheetData();
            int rowIdx = 0;
            Row row = new();
            for(var i = 0; i < Headers.Length; ++i)
                row.InsertAt<Cell>(new Cell() { DataType = CellValues.InlineString, InlineString = new InlineString() { Text = new Text(Headers[i]) } }, i);
            sheetData.InsertAt(row, rowIdx++);

            row = new();
            var matchNum = m_Index + 1;
            var idx = 0;
            InsertNumCell(row, (matchNum).ToString(), idx++);
            InsertNumCell(row, m_Match.Results[m_Index].TeamAI.Number.ToString(), idx++);
            InsertNumCell(row, m_Match.Results[m_Index].TeamAI.Size.ToString(), idx++);
            InsertNumCell(row, m_Match.Results[m_Index].TeamAI.AccPoints.ToString(), idx++);
            InsertNumCell(row, m_Match.Results[m_Index].TeamPlayer.Number.ToString(), idx++);
            InsertNumCell(row, m_Match.Results[m_Index].TeamPlayer.Size.ToString(), idx++);
            InsertNumCell(row, m_Match.Results[m_Index].TeamPlayer.AccPoints.ToString(), idx++);
            InsertNumCell(row, m_Match.Results[m_Index].NumOfRounds.ToString(), idx++);
            InsertNumCell(row, (m_Match.Results[m_Index].AccTimeInSec / 60f).ToString(), idx++);
            // Insert the machine (and user) names.
            InsertStrCell(row, Environment.MachineName, idx++);
            // InsertStrCell(row, Environment.UserName, idx++);

            // Insert the row into the sheet.
            sheetData.InsertAt(row, rowIdx++);

            return sheetData;
        }

        /// <summary>
        /// Method <c>InsertNumCell</c> inserts a number cell.
        /// </summary>
        private void InsertNumCell(Row row, string str, int idx) => row.InsertAt<Cell>(new Cell() { DataType = CellValues.Number, CellValue = new CellValue(str) }, idx);

        /// <summary>
        /// Method <c>InsertStrCell</c> inserts a string cell.
        /// </summary>
        private void InsertStrCell(Row row, string str, int idx) => row.InsertAt<Cell>(new Cell() { DataType = CellValues.String, CellValue = new CellValue(str) }, idx);
    }
#endif
}
