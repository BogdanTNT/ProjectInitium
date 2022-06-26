using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static McCommands.McCommands;

namespace McFunction
{


    public class McFunctions
    {
        public static string[] transitionTypes =
        {
            "DetailQuest", "Fetch", "GiveReward", "Custom"
        };

        public enum ScoreboardCommands
        {
            Set, Add, Reset, Operation, Remove
        }

        /// <summary>
        /// Scoreboard for 0:set, 1:add, 2:reset, 3:operation, 4:remove
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static string ScoreboardCommand(ScoreboardCommands index)
        {
            switch (index)
            {
                case ScoreboardCommands.Set:
                    return "scoreboard players set";
                case ScoreboardCommands.Add:
                    return "scoreboard players add";
                case ScoreboardCommands.Reset:
                    return "scoreboard players reset";
                case ScoreboardCommands.Operation:
                    return "scoreboard players operation";
                case ScoreboardCommands.Remove:
                    return "scoreboard players remove";
            }
            return "Esti prost sa moara Gibilan";
        }

        public enum scoreType
        {
            npcMarker, qTag
        }
        
        /// <summary>
        /// Score for 0:npcMarker, 1+npc:qTagNume, 2:convSB
        /// </summary>
        /// <param name="index"></param>
        /// <param name="npc"></param>
        /// <returns></returns>
        public static string scores(scoreType index, NPC npc = null)
        {
            switch (index)
            {
                case scoreType.npcMarker:
                    return "npcMarker";
                case scoreType.qTag:
                    return $"qTag{npc.name}";
            }
            return "Iar esti prost sa moara Gibilan";
        }

        public static string GetDialogueIndex(int conversation, int stage)
        {
            return stage > 9 ? $"{conversation}{stage}" : $"{conversation}0{stage}";
        }

        /// <summary>
        /// Functions for 0:q002, 1:checkQuestComplete, 2:q001
        /// </summary>
        /// <param name="index"></param>
        /// <param name="npc"></param>
        /// <returns></returns>
        public static string Function(int index, NPC npc = null, int conv = 0, int stage = 0)
        {
            conv++; stage++;
            switch (index)
            {
                case 0:
                    return $"function {ParentDirectoryForQuests(true)}npc/{NPC_NamesForFiles(npc.name)}/q002_updateNpcQuests";
                case 1:
                    return $"function {ParentDirectoryForQuests(true)}npc/{NPC_NamesForFiles(npc.name)}/mat/checkQuestComplete_q{conv}s{stage}";
                case 2:
                    return $"function {ParentDirectoryForQuests(true)}npc/{NPC_NamesForFiles(npc.name)}/q001_checkQuestStages";
            }
            return "Inca o data esti prost sa moara Gibilan";
        }

        public enum ExecuteType
        {
            OnMaster, OnqTag, OnNpcMarker, OnqProg
        }

        /// <summary>
        /// Execute for 0:master, 1:qTag, 2:npcMarker, 3:qProg_qn
        /// </summary>
        /// <param name="index"></param>
        /// <param name="npc"></param>
        /// <param name="conversation"></param>
        /// <param name="stage"></param>
        /// <returns></returns>
        public static string Execute(ExecuteType index, NPC npc = null, int conversation = 0, int stage = 0)
        {
            string executeFunction = "execute ";
            string here = " ~ ~ ~";
            conversation++;
            stage++;

            // qTagNume in functie de stagiul liniei de dialog la care a ajuns
            // Daca e peste 9 sa fie xy, altfel de fie x0y.
            string convSB = GetDialogueIndex(conversation, stage);

            switch (index)
            {
                case ExecuteType.OnMaster:
                    return executeFunction +
                        "@e[scores = {ijk = 0}]" +
                        here;
                case ExecuteType.OnqTag:
                    return executeFunction +
                        "@e[scores = {" +
                        $"qTag{npc.name} = {convSB}" + "}]" +
                        here +
                        " " +
                        Execute(ExecuteType.OnNpcMarker, npc) +
                        $" {ScoreboardCommand(0)} @s qProg_q{conversation} {stage}";
                case ExecuteType.OnNpcMarker:
                    return executeFunction +
                        "@e[scores = {npcMarker = " + npc.index + "}]" +
                        here;
                case ExecuteType.OnqProg:
                    return executeFunction +
                        "@s[scores = {qProg_q" + conversation + $"={stage}" + "}]" +
                        here;

            }
            return "Da tu chiar nu te opresti din a fi prost";
        }


        public enum Entities
        {
            Master, NoDelivery, HasDelivery, CustomScoreboard
        }

        /// <summary>
        /// Tipul de entitate 0:master 1:activeDelivery=-1.., 2:activeDelivery=0.., 3:custom + scoreboards
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static string entities(Entities index, List<string> scItem = null, List<int> scValue = null)
        {
            switch (index)
            {
                case Entities.Master:
                    return "@e[scores = {ijk = 0}]";
                case Entities.NoDelivery:
                    return "@e[scores = {activeDelivery = -1..}]";
                case Entities.HasDelivery:
                    return "@e[scores = {activeDelivery = 0..}]";
                case Entities.CustomScoreboard:
                    {
                        string scoreboards = "";

                        for (int i = 0; i < scItem.Count; i++)
                        {
                            if (i > 0)
                                scoreboards += ", ";
                            scoreboards += scItem[i] + " = " + scValue[i];
                        }

                        return "@e[scores = {" + scoreboards + "}]";
                    }
            }
            return "Nu ma asteptam la asta totusi";
        }

        /// <summary>
        /// Return position the way minecraft wants it
        /// </summary>
        /// <param name="index"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public static string pos(int index, Vector3 p)
        {
            switch (index)
            {
                case 0:
                    return $"{p.x} {p.y} {p.z}";
            }

            return "Da ba da ba da da da";

        }

        /// <summary>
        /// Return the name string that will be used to identify the npc in the files
        /// </summary>
        /// <param name="npcName"></param>
        /// <returns></returns>
        public static string NPC_NamesForFiles(string npcName)
        {
            return $"sqst{npcName}";
        }

        /// <summary>
        /// Return the name on the directory containing all the quest functions.
        /// </summary>
        /// <param name="forCallbackFunction">Set true if the directory is used to call a function and not write a new one.</param>
        /// <returns></returns>
        public static string ParentDirectoryForQuests(bool forCallbackFunction = false)
        {
            string folderName = "questy/";

            if (forCallbackFunction)
                return $"{folderName}";
            return $"/functions/{folderName}";
        }

        //public static string GetShortNameForNPC(string name)
        //{
        //    return name.Length > 6 ? name.Substring(0, 6) : name;
        //}
    }
}

