using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace McFunction
{
    public class McFunctions
    {
        /// <summary>
        /// Scoreboard for 0:set, 1:add, 2:reset, 3:operation, 4:remove
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static string SB(int index)
        {
            switch (index)
            {
                case 0:
                    return "scoreboard players set";
                case 1:
                    return "scoreboard players add";
                case 2:
                    return "scoreboard players reset";
                case 3:
                    return "scoreboard players operation";
                case 4:
                    return "scoreboard players remove";
            }
            return "Esti prost sa moara Gibilan";
        }

        /// <summary>
        /// Score for 0:npcMarker, 1+npc:qTagNume
        /// </summary>
        /// <param name="index"></param>
        /// <param name="npc"></param>
        /// <returns></returns>
        public static string sc(int index, NPC npc = null)
        {
            switch (index)
            {
                case 0:
                    return "npcMarker";
                case 1:
                    return $"qTag{npc.name}";
                //case 2:
                //    return 
            }
            return "Iar esti prost sa moara Gibilan";
        }

        /// <summary>
        /// Functions for 0:q002, 1:checkQuestComplete
        /// </summary>
        /// <param name="index"></param>
        /// <param name="npc"></param>
        /// <returns></returns>
        public static string f(int index, NPC npc = null)
        {
            switch (index)
            {
                case 0:
                    return $"function npc/sqst{npc.name}/q002_updateNpcQuests";
                case 1:
                    return $"function npc/sqst{npc.name}/mat/checkQuestComplete";
            }
            return "Inca o data esti prost sa moara Gibilan";
        }

        /// <summary>
        /// Execute for 0:master, 1:qTag, 2:npcMarker
        /// </summary>
        /// <param name="index"></param>
        /// <param name="npc"></param>
        /// <param name="convesation"></param>
        /// <param name="stage"></param>
        /// <returns></returns>
        public static string exe(int index, NPC npc = null, int convesation = 0, int stage = 0)
        {
            string b = "execute ";
            string t = " ~ ~ ~";
            convesation++;
            stage++;

            // qTagNume in functie de stagiul liniei de dialog la care a ajuns
            // Daca e peste 9 sa fie xy, altfel de fie x0y.
            string convSB = stage > 9 ? $"{convesation}{stage}" : $"{convesation}0{stage}";

            switch (index)
            {
                case 0:
                    return b + 
                        "@e[scores = {ijk = 0}]" + 
                        t;
                case 1:
                    return b + 
                        "@e[scores = {" + 
                        $"qTag{npc.name} = {convSB}" + "}]" + 
                        t + 
                        " " + 
                        exe(2, npc) + 
                        $" {SB(0)} @s qProg_q{convesation} {stage}";
                case 2:
                    return b +
                        "@e[scores = {npcMarker = " + npc.index + "}]" +
                        t;
                //case 3:
                //    return b +
                //        "@s[scores = {" +

            }
            return "Da tu chiar nu te opresti din a fi prost";
        }

        /// <summary>
        /// Tipul de entitate 0:master 1:activeDelivery=-1.., 2:activeDelivery=0.., 3:custom + scoreboards
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static string e(int index, List<string> scItem = null, List<int> scValue = null)
        {
            switch (index)
            {
                case 0:
                    return "@e[scores = {ijk = 0}]";
                case 1:
                    return "@e[scores = {activeDelivery = -1..}]";
                case 2:
                    return "@e[scores = {activeDelivery = 0..}]";
                case 3:
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

            return "Ba da ba da da da";

        }
    }
}

