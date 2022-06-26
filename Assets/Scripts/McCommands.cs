using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace McCommands
{
    public static class McCommands
    {
        #region All the values

        public enum Identifier
        {
            npcMarker, questTag, questProgress, ActiveDelivery, globalTimer1Sec
        }

        /// <summary>
        /// Contains the string associated with the score used for the quest
        /// </summary>
        private static Dictionary<Identifier, string> scoreDictionary = new Dictionary<Identifier, string>()
        {
            {Identifier.npcMarker, "npcMarker" },
            {Identifier.questTag, "qTag" },
            {Identifier.questProgress, "qProg_q" },
            {Identifier.ActiveDelivery, "activeDelivery" },
            {Identifier.globalTimer1Sec, "globalTimer1Sec" }
        };

        /// <summary>
        /// Return the name on the directory containing all the quest functions.
        /// </summary>
        /// <param name="forCallbackFunction">Set true if the directory is used to call a function and not write a new one.</param>
        /// <returns></returns>
        public static string GetParentDirectoryForQuests(bool forCallbackFunction = false)
        {
            string folderName = "questy/";

            if (forCallbackFunction)
                return $"{folderName}";
            return $"/functions/{folderName}";
        }

        //public static string[] transitionTypes =
        //{
        //    "DetailQuest", "Fetch", "GiveReward", "Custom"
        //};

        public enum Transitions
        {
            AcceptQuest,
            GiveItems,
            RewardPlayer,
            Custom
        }

        private static Dictionary<Transitions, string> transitionDictionary = new Dictionary<Transitions, string>()
        {
            {Transitions.AcceptQuest, "AcceptQuest" },
            {Transitions.GiveItems, "GiveItems" },
            {Transitions.RewardPlayer, "RewardPlayer" },
            {Transitions.Custom, "Custom" }
        };


        #endregion

        public static string GetScore(Identifier identifier, int extraValue = 0)
        {
            string value = "";

            scoreDictionary.TryGetValue(identifier, out value);

            if (identifier == Identifier.questProgress)
            {
                value += extraValue;
            }
            return value;
        }

        public static string GetScore(Identifier identifier, string extraString)
        {
            string value = "";

            scoreDictionary.TryGetValue(identifier, out value);

            if (identifier == Identifier.questTag)
            {
                value += extraString;
            }
            return value;
        }

        public static string GetTransition(Transitions transition)
        {
            string value;

            transitionDictionary.TryGetValue(transition, out value);

            return value;
        }


        public static string GetTransition(QuestType type)
        {
            string value;

            Transitions transition = Transitions.AcceptQuest;

            if(type == QuestType.Info)
            {
                transition = Transitions.AcceptQuest;
            }
            else if(type == QuestType.Fetch)
            {
                transition = Transitions.GiveItems;
            }
            else if(type == QuestType.Reward)
            {
                transition = Transitions.RewardPlayer;
            }
            else if(type == QuestType.Custom)
            {
                transition = Transitions.Custom;
            }

            transitionDictionary.TryGetValue(transition, out value);

            return value;
        }


        public enum ScoreboardType
        {
            Set, Add, Reset, Operation, Remove
        }
        /// <summary>
        /// Scoreboard function
        /// </summary>
        /// <param name="type">The type of the scoreboard to return</param>
        /// <param name="whoToApply">On what entity should this function run</param>
        /// <param name="whatScoreToUse">What is the scoreboard to use</param>
        /// <param name="howMuchShouldItBe">What is the value used by the function</param>
        /// <returns></returns>
        public static string Scoreboard(ScoreboardType type, string whoToApply, string whatScoreToUse, int howMuchShouldItBe)
        {
            switch (type)
            {
                case ScoreboardType.Set:
                    return $"scoreboard players set {whoToApply} {whatScoreToUse} {howMuchShouldItBe}";
                case ScoreboardType.Add:
                    return $"scoreboard players add {whoToApply} {whatScoreToUse} {howMuchShouldItBe}";
                case ScoreboardType.Reset:
                    return $"scoreboard players reset {whoToApply} {whatScoreToUse}";
                case ScoreboardType.Operation:
                    return $"scoreboard players operation ";
                case ScoreboardType.Remove:
                    return $"scoreboard players remove {whoToApply}  {whatScoreToUse} {howMuchShouldItBe}";
            }
            return "Esti prost sa moara Gibilan";
        }




        public static string Function(string location)
        {
            string function = "function " + GetParentDirectoryForQuests(true);

            function += location;

            return function;
        }




        public static string Execute(string whoToApply, string whereToApply, string command)
        {
            // De facut una separata si cu detect block
            string toExecute = "execute " + whoToApply + whereToApply + command;
            return toExecute;
        }




        public static string SetBlock(string pos, string itemId)
        {
            return $"setblock {pos} {itemId}";
        }




        public static string here()
        {
            return "~ ~ ~ ";
        }

        public static string thisEntity()
        {
            return "@s";
        }

        public static string masterEntity()
        {
            return "@e[scores = {ijk = 0}]";
        }

        public static string entity(string scoreToFind, int valueOfScore, bool orMore = false, bool onSelfInstedOnEveryEntity = false)
        {
            string entity;
            if(onSelfInstedOnEveryEntity)
                entity = "@s[scores = {" + scoreToFind + $" = {valueOfScore}";
            else
                entity = "@e[scores = {" + scoreToFind + $" = {valueOfScore}";

            if (orMore)
                entity += "..";

            entity += "}] ";
            return entity;
        }

        public static string entity(List<string> scoreToFind, List<int> valueOfScore)
        {
            string entityCall = "@e[scores = {";

            for(int i = 0; i < scoreToFind.Count; i++)
            {
                entityCall += scoreToFind[i] + $" = {valueOfScore[i]}";
                if (i != scoreToFind.Count - 1)
                    entityCall += ",";
            }

            entityCall += "}] ";

            return entityCall;
        }

        public static string GetDialogueValue(int conversation, int stage)
        {
            conversation++;
            stage++;

            return stage > 9 ? $"{conversation}{stage}" : $"{conversation}0{stage}";
        }


        public static string GetShortNameForNPC(string name)
        {
            return name.Length > 6 ? name.Substring(0, 6) : name;
        }

/// <summary>
        /// Return the name string that will be used to identify the npc in the files
        /// </summary>
        /// <param name="npcName"></param>
        /// <returns></returns>
        public static string GetNPC_NamesForFiles(string npcName)
        {
            return $"sqst{npcName}";
        }

        /// <summary>
        /// Return position the way minecraft wants it
        /// </summary>
        /// <param name="index"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public static string GetPositionFromVector(Vector3 p)
        {
            return $"{p.x} {p.y} {p.z}";

        }
    }
}