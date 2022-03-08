using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using static McFunction.McFunctions;
using Initium.ScriptableObjects;
using System.Linq;

public class Generator : MonoBehaviour
{
    public List<NPC> npcs;

    public List<NPC> saveNpc;

    [Tooltip("Vorba aia, for debugging purposes only!!!!1!")]
    public List<DialogueJSON> json;

    private string cachedLocation;

    public InitiumDialogueContainerSO dialogueContainer;

    /// <summary>
    /// Genereaza mcfunction
    /// </summary>
    public void Generate()
    {
        Debug.ClearDeveloperConsole();

        // Init
        BeforeStartingGeneration();

        // Genereaza mcfunction pt fiecare npc
        foreach(NPC n in npcs)
        {
            Generateq00(n);
            GenerateFetch(n);
        }
    }

    /// <summary>
    /// Genereaza json
    /// </summary>
    public void GenerateJson()
    {
        // Sters jsonurile vechi (incearca cel putin)
        FileUtil.DeleteFileOrDirectory(Application.streamingAssetsPath + $"/dialogue/");

        // Resetat jsonu 
        json = new List<DialogueJSON>();

        // Genereaza json pt fiecare npc
        foreach (NPC n in npcs)
        {
            WriteDialogueJson(n);
        }
    }


    /// <summary>
    /// Genereaza jsonurile cu dialogul - mai trebuie sa fac sa adauge comenzile unde trebuie
    /// </summary>
    /// <param name="n"></param>
    void WriteDialogueJson(NPC n)
    {
        json.Add(new DialogueJSON(n));

        string jsonStr = JsonUtility.ToJson(json[json.Count - 1], true);
        //Debug.Log(jsonStr);
        jsonStr = jsonStr.Replace(@"\\n", @"\n");
        jsonStr = jsonStr.Replace(@"npc_dialogue", @"minecraft:npc_dialogue");
        //Debug.Log(jsonStr);


        cachedLocation = cachedLocation = Application.streamingAssetsPath + $"/dialogue/npc_{n.name}.json";
        Write(jsonStr);
    }


    /// <summary>
    /// O serie lucruri care trebuie facute inainte sa inceapa generarea fisierelor de mc
    /// </summary>
    void BeforeStartingGeneration()
    {
        // Resetat generare
        FileUtil.DeleteFileOrDirectory(Application.streamingAssetsPath + $"/functions/");
        FileUtil.DeleteFileOrDirectory(Application.streamingAssetsPath + $"/dialogue/");
        FileUtil.DeleteFileOrDirectory(Application.streamingAssetsPath);

        // Creat foldere
        Directory.CreateDirectory(Application.streamingAssetsPath + $"/dialogue/");

        // Adaugat un index la npc pentru a sti care-i care
        for (int i = 0; i < npcs.Count; i++)
            npcs[i].index = i + 1;
    
        // Reseteaza cate iteme o sa fie date la diverse questuri
        itemCounter = 0;

    }


    /// <summary>
    /// Scrie in fisierul din cachedLocation
    /// </summary>
    /// <param name="what"></param>
    /// <param name="npc"></param>
    public void Write(string what, NPC npc = null)
    {
        if (!File.Exists(cachedLocation))
        {
            File.WriteAllText(cachedLocation, what + "\n");
            return;
        }

        File.AppendAllText(cachedLocation, what + "\n");
    }


    /// <summary>
    /// Genereaza q000, q001 si mai tre sa fac si q002
    /// </summary>
    /// <param name="n"></param>
    void Generateq00(NPC n)
    {
        Directory.CreateDirectory(Application.streamingAssetsPath + $"/functions/npc/sqst{n.name}/");

        #region q000_onSummon
        cachedLocation = Application.streamingAssetsPath + $"/functions/npc/sqst{n.name}/q000_onSummon.mcfunction";
        //Create file
        Write($"# onSummon for {n.name}");

        // Set npcMarker
        Write($"{SB(0)} @s {sc(0)} {(npcs.IndexOf(n) + 1).ToString()}", n);

        // Set qProg_qn, daca are o singura conversatie atunci q1 devine 1, altfel doar ultimul q devine 1 si restul devin -1
        for (int i = 0; i < n.conversations.Count; i++)
        {
            string value = i + 1 == n.conversations.Count ? 1.ToString() : (-1).ToString();
            Write($"{SB(0)} @s qProg_q{i + 1} {value}");
        }
        Write("\n");

        Write("# Implement changes");
        Write($"{f(0, n)}");

        #endregion


        #region q001_checkQuestStages
        cachedLocation = Application.streamingAssetsPath + $"/functions/npc/sqst{n.name}/q001_checkQuestStages.mcfunction";
        //Create file
        Write($"# Check Quest Stages for {n.name}");

        for (int i = 0; i < n.conversations.Count; i++)
        {
            for (int j = 0; j < n.conversations[i].lines.Count; j++)
            {
                Write(exe(1, n, i, j));
            }
            Write("\n");
        }
        Write("\n");

        // Dupa ce entitatea este incarcata si ii atribuie noul quest atunci reseteaza master sa nu il mai caute
        Write("# Reset trigger objective");
        Write($"{SB(0)} {e(0)} {sc(1, n)} -1");

        Write("# Implement changes");
        Write(f(0, n));

        #endregion


        #region q002_updateNpcQuests
        cachedLocation = Application.streamingAssetsPath + $"/functions/npc/sqst{n.name}/q002_updateNpcQuests.mcfunction";
        //Create file
        Write($"# Dialogue and particle change for {n.name}");

        //for (int i = 0; i < n.conversations.Count; i++)
        //{
        //    for (int j = 0; j < n.conversations[i].lines.Count; j++)
        //    {
        //        Write(updateNpc, )
        //        Write(checkQuest, "\n");
        //    }
        //}

        #endregion
    }


    /// <summary>
    /// Counts the number of item present in the whole story
    /// </summary>
    int itemCounter;
    void GenerateFetch(NPC n)
    {
        Directory.CreateDirectory(Application.streamingAssetsPath + $"/functions/npc/sqst{n.name}/mat/");

        // Datorita limitei de caractere in scoreboarduri, cachedName scrie mereu doar primele 6 litere din numele npcurilor
        string cachedName = n.name.Length > 6 ? n.name.Substring(0, 6) : n.name;


        // Genereaza fisiere noi pt fiecare dialog in care trebuie sa dea iteme (checkQuestCompletion), dar si pentru fiecare item care trebuie dat (give, add, intrerupt)
        for (int i = 0; i < n.conversations.Count; i++)
        {
            for(int j = 0; j < n.conversations[i].lines.Count; j++)
            {
                // Lista cu itemele de care o sa fie nevoie pentru a completa questul 
                List<string> itemScoreboard = new List<string>();
                List<int> itemQuantity = new List<int>();
                
                if (n.conversations[i].lines[j].questType == QuestType.Fetch)
                {
                    for(int m = 0; m < n.conversations[i].lines[j].FetchItems.Count; m++)
                    {
                        #region Cache some things before generation

                        Item cachedItem = n.conversations[i].lines[j].FetchItems[m];

                        // Scoreboard for item tracked in the quest
                        string cachedCounter = $"rlcnt{itemCounter}" + (cachedItem.id.Length > 8 ? cachedItem.id.Substring(0, 8) : cachedItem.id);

                        string cachedVDALTI = $"VDALTI{cachedName}{m}";

                        itemScoreboard.Add(cachedCounter);
                        itemQuantity.Add(cachedItem.quantity);

                        #endregion


                        #region Generare giveItem

                        cachedLocation = Application.streamingAssetsPath + $"/functions/npc/sqst{n.name}/mat/give{cachedItem.id}.mcfunction";
                        Write("# Activeaza command blocku");

                        Write($"setblock {pos(0, cachedItem.redstoneBlock)} minecraft:redstone_block");

                        Write("# Trigger check pt a scoate blocul de redstone");

                        Write($"{SB(1)} {e(1)} activeDelivery 1");

                        #endregion


                        #region Generare additem

                        cachedLocation = Application.streamingAssetsPath + $"/functions/npc/sqst{n.name}/mat/add{cachedItem.id}.mcfunction";
                        Write("# Incrementeaza counteru");

                        Write($"{SB(1)} {e(0)} {cachedCounter} 1");

                        Write("# Actualizeaza sidebar");

                        Write($"{SB(3)} \"Bring {n.alternativeName} {cachedItem.quantity} {cachedItem.alternativeName}\" sidebarQprog = {e(0)} {cachedCounter}");

                        Write("# verifica daca conditiile questului au fost indeplinite");

                        Write($"{f(1, n)}");

                        #endregion


                        #region Generare intreruptItem

                        cachedLocation = Application.streamingAssetsPath + $"/functions/npc/sqst{n.name}/mat/intrerupt{cachedItem.id}.mcfunction";
                        Write("# Intrerupt command block chain");

                        Write($"setblock {pos(0, cachedItem.redstoneBlock)} air");

                        Write($"{SB(2)} {e(0)} {cachedVDALTI}");

                        Write($"{SB(4)} {e(2)} activeDelivery 1");

                        Write($"{SB(2)} \"Bring {n.alternativeName} {cachedItem.quantity} {cachedItem.alternativeName}\" sidebarQprog");

                        #endregion


                        itemCounter++;
                    }
                }


                #region Create Check Quest Completion

                for (int b = 0; b < itemScoreboard.Count; b++)
                {
                    cachedLocation = Application.streamingAssetsPath + $"/functions/npc/sqst{n.name}/mat/checkQuestComplete_q{i + 1}s{j + 1}.mcfunction";
                    Write($"# Check if the quest took all the item for the quest from the inventory");

                    List<string> scItem = new List<string>();
                    scItem.Add(itemScoreboard[b]);
                    List<int> qItem = new List<int>();
                    qItem.Add(itemQuantity[b]);

                    Write($"execute {e(3, scItem, qItem)} ~ ~ ~ function npc/sqst{n.name}/intrerupt{n.conversations[i].lines[j].FetchItems[b].id}");

                }

                if(itemScoreboard.Count > 0)
                {
                    string convSB = j + 2 > 9 ? $"{i + 1}{j + 2}" : $"{i + 1}0{j + 2}";
                    Write($"execute {e(3, itemScoreboard, itemQuantity)} ~ ~ ~ {SB(0)} @s {sc(1, n)} {convSB}");

                }

                #endregion
            }
        }
    }


    public void SaveList()
    {
        saveNpc = new List<NPC>(npcs);
    }

    public void LoadList()
    {
        npcs = new List<NPC>(saveNpc);
    }


    public void TransformDialogueContainerToList()
    {
        npcs.Clear();

        for(int i = 0; i < dialogueContainer.DialogueGroups.Count; i++)
        {
            List<InitiumDialogueSO> conversatie = new List<InitiumDialogueSO>();
            conversatie = dialogueContainer.DialogueGroups.Values.ToList()[i];

            for(int j = 0; j < conversatie.Count; j++)
            {
                // daca nu gaseste speakeru, il creaza,
                if (!npcs.Any(npc => npc.name == conversatie[j].Speaker))
                {
                    npcs.Add(new NPC(conversatie[j].Speaker));
                }

                // daca speakeru nu are o conversatie cu indexul (i) in conversatiea lui de conversatii, o creaza
                if(!npcs.First(npc => npc.name == conversatie[j].Speaker).conversations.Any(conv => conv.priority == i))
                {
                    npcs.First(npc => npc.name == conversatie[j].Speaker).conversations.Add(new Conversation(i, dialogueContainer.DialogueGroups.Keys.ToList()[i].GroupName));
                }

                // Trebuie schimabta incat in functie de quest sa adauge diverse lucruri
                // Adica daca e un fetch quest sa ii adauge si iteme, dar doar atunci sa le adauge, nu si daca e info quest

                //Aici cauta primul npc cu numele vorbitului care spune dialogul cu indexul j din conversatie, dupa aceea cauta conversatia npcului care are legatura cu "conversatie", dupa aceea adauga o noua linie de dialog 
                npcs.First(npc => npc.name == conversatie[j].Speaker).conversations.First(conv => conv.priority == i).lines.Add(new Dialogue(conversatie[j].DialogueName, conversatie[j].Text));
            }
        }
    }

}

