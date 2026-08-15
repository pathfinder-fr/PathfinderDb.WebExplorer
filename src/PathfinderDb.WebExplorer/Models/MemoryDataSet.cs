using PathfinderDb.Schema;
using System;
using System.Xml;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DbBrowser.Models
{
    public class MemoryDataSet
    {
        private static readonly string[] DataSetNames = new[] {
            "apg",
            "pfrpg",
            "uc",
            "um"
        };

        private static DataSet dataSet;

        private static List<DataSet> dataSets;

        private static readonly object loadLock = new object();

        public static List<DataSet> DataSets
        {
            get { return dataSets; }
        }

        public static void Initialize(string contentRootPath)
        {
            LoadDataSet(contentRootPath);
        }

        public static DataSet LoadDataSet(string contentRootPath)
        {
            if (dataSet == null)
            {
                lock (loadLock)
                {
                    if (dataSet == null)
                    {
                        dataSet = new DataSet();
                        dataSets = new List<DataSet>();
                        var appDataPath = Path.Combine(contentRootPath, "App_Data");
                        foreach (var ds in Directory.GetFiles(appDataPath, "*.xml").Where(p => DataSetNames.Any(ds => ds == Path.GetFileNameWithoutExtension(p))).Select(p => LoadDataSetFromFile(p)))
                        {
                            dataSets.Add(ds);
                            dataSet.Add(ds);
                        }
                    }
                }
            }

            return dataSet;
        }

        public static DataSet LoadDataSetFromFile(string path)
        {
            using (var reader = XmlReader.Create(path))
            {
                return DataSet.Load(reader);
            }
        }
    }
}
