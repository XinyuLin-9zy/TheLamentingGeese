using System;
using System.Collections.Generic;
using UnityEngine;

namespace Utage
{
    [CreateAssetMenu(menuName = "Utage/ScenarioFileReader/Csv", fileName = "CsvFileReaderSettings")]
    public class AdvScenarioFileReaderSettingsCsv : ScenarioFileReaderSettings
    {
        [Serializable]
        public class FilePattern
        {
            public string ext;
            public char separator;
        }

        public List<FilePattern> FilePatternList { get; } = new()
        {
            new FilePattern() { ext = ".csv", separator = ',' },
            new FilePattern() { ext = ".tsv", separator = '\t' }
        };

        public override IAdvScenarioFileReader CreateReader()
        {
            return new AdvScenarioFileReaderCsv(this);
        }
    }
}
