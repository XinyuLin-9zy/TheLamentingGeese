// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura

using System;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Utage
{

	//「Utage」のシナリオデータ用のインポーター
	public class AdvScenarioFileReaderCsv : IAdvScenarioFileReader
	{
		AdvScenarioFileReaderSettingsCsv Settings { get; }

		public AdvScenarioFileReaderCsv(AdvScenarioFileReaderSettingsCsv settings)
		{
			Settings = settings;
		}

		public bool IsTargetFile(string path)
		{
			if (string.IsNullOrEmpty(path)) return false;
			string extension = Path.GetExtension(path);
			return Settings.FilePatternList.Exists(x=> x.ext.Equals(extension, StringComparison.OrdinalIgnoreCase));
		}

		public bool TryReadFile(string path, out StringGridDictionary stringGridDictionary)
		{
			if (!IsTargetFile(path))
			{
				stringGridDictionary = null;
				return false;
			}

			string extension = Path.GetExtension(path);
			var pattern = Settings.FilePatternList.Find(x => x.ext.Equals(extension, StringComparison.OrdinalIgnoreCase));

			var csvParser = new CsvParser() { Delimiter = pattern.separator };
			StringGrid grid = csvParser.ReadFile(path);
			grid.ParseHeader();
			stringGridDictionary = new StringGridDictionary();
			stringGridDictionary.Add(grid.SheetName,grid);
			return true;
		}
	}
}
