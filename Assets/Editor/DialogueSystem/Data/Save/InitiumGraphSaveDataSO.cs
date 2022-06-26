using System.Collections.Generic;
using UnityEngine;

namespace Initium.Data.Save
{
    public class InitiumGraphSaveDataSO : ScriptableObject
    {
        [field: SerializeField] public string FileName { get; set; }
        [field: SerializeField] public List<InitiumGroupSaveData> Groups { get; set; }
        [field: SerializeField] public List<InitiumNodeSaveData> Nodes { get; set; }
        [field: SerializeField] public List<string> OldGroupNames { get; set; }
        [field: SerializeField] public List<string> OldUngroupedNodeNames { get; set; }
        [field: SerializeField] public SerializableDictionary<string, List<string>> OldGroupedNodeNames { get; set; }

        [field: SerializeField] public List<string> Speakers { get; set; }
        [field: SerializeField] public List<string> AlternativeName { get; set; }

        public void Initialize(string fileName)
        {
            FileName = fileName;

            Groups = new List<InitiumGroupSaveData>();
            Nodes = new List<InitiumNodeSaveData>();
        }
    }
}