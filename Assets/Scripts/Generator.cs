using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
//using static McFunction.McFunctions;
using Initium.ScriptableObjects;
using System.Linq;
using static McCommands.McCommands;

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
        itemCounter = 0;
        foreach (NPC n in npcs)
        {
            GenerateTransitions(n);
        }

        GenerateInitialiserForScoreboards();
        GenerateMainExtenstion();

        AssetDatabase.Refresh();
    }

    /// <summary>
    /// Genereaza json
    /// </summary>
    public void GenerateJson()
    {
        // Sters jsonurile vechi (incearca cel putin)
        FileUtil.DeleteFileOrDirectory(Application.streamingAssetsPath + $"/dialogue/");
        Directory.CreateDirectory(Application.streamingAssetsPath + $"/dialogue/");

        // Resetat jsonu 
        json = new List<DialogueJSON>();

        // Genereaza json pt fiecare npc
        foreach (NPC n in npcs)
        {
            WriteDialogueJson(n);
        }

        AssetDatabase.Refresh();
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


        Directory.CreateDirectory(Application.streamingAssetsPath + $"/dialogue/");
        cachedLocation = Application.streamingAssetsPath + $"/dialogue/npc_{n.name.ToLower()}.json";
        Write(jsonStr);
    }


    /// <summary>
    /// O serie lucruri care trebuie facute inainte sa inceapa generarea fisierelor de mc
    /// </summary>
    void BeforeStartingGeneration()
    {
        // Resetat generare
        FileUtil.DeleteFileOrDirectory(Application.streamingAssetsPath + $"{GetParentDirectoryForQuests()}");

        // Creat foldere
        Directory.CreateDirectory(Application.streamingAssetsPath + $"{GetParentDirectoryForQuests()}");

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
    public void Write(string what)
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
        Directory.CreateDirectory(Application.streamingAssetsPath + $"{GetParentDirectoryForQuests()}npc/{GetNPC_NamesForFiles(n.name)}/ ");

        #region q000_onSummon

        cachedLocation = Application.streamingAssetsPath + $"{GetParentDirectoryForQuests()}npc/{GetNPC_NamesForFiles(n.name)}/q000_onSummon.mcfunction";
        //Create file
        Write($"# onSummon for {n.name}");

        // Set npcMarker
        Write(Scoreboard(ScoreboardType.Set, 
                         thisEntity(), 
                         GetScore(Identifier.npcMarker), 
                         npcs.IndexOf(n) + 1));

        Write("");
 
        // Set qProg_qn, daca are o singura conversatie atunci q1 devine 1, altfel doar ultimul q devine 1 si restul devin -1
        for (int i = 0; i < n.conversations.Count; i++)
        {
            int value = -1;
            if (n.conversations[i].lines.Any(dialog => dialog.isStartingDialogue == true))
                value = 1;
            Write(Scoreboard(ScoreboardType.Set, thisEntity(), GetScore(Identifier.questProgress, i + 1), value));
        }
        Write("\n");

        Write("# Implement changes");

        Write(Function($"npc/{GetNPC_NamesForFiles(n.name)}/q002_updateNpcQuests"));


        #endregion


        #region q001_checkQuestStages

        cachedLocation = Application.streamingAssetsPath + $"{GetParentDirectoryForQuests()}npc/{GetNPC_NamesForFiles(n.name)}/q001_checkQuestStages.mcfunction";
        //Create file
        Write($"# Check Quest Stages for {n.name}");


        // Teoretic, original era @s in loc de @e la primu execute, dar merge
        // 
        for(int i = 0; i < n.conversations.Count - 1; i++)
        {
            Write(Execute(entity(GetScore(Identifier.questProgress, i + 1), n.conversations[i].lines.Count), 
                          here(), 
                          Execute(entity(GetScore(Identifier.npcMarker), n.index), 
                                  here(), 
                                  Scoreboard(ScoreboardType.Set, 
                                             thisEntity(), 
                                             GetScore(Identifier.questProgress, i + 1), 
                                             -1))));
            Write("");

        }

        Write("");


        // This portion is used to change the quest that the npc is displaying
        // The quest number is XYZ, where x is the questline number, the YZ is the individual dialogue line in the questline
        // This part is also usefull when the npc is out of range and the main function has to wait until the npc is loaded
        for (int i = 0; i < n.conversations.Count; i++)
        {
            
            for (int j = 0; j < n.conversations[i].lines.Count; j++)
            {
                //Write(Execute(ExecuteType.OnqTag, n, i, j));
                Write(Execute(entity(GetScore(Identifier.questTag, n.name), int.Parse(GetDialogueValue(i, j))),
                              here(),
                              Execute(entity(GetScore(Identifier.npcMarker), n.index),
                                      here(),
                                      Scoreboard(ScoreboardType.Set,
                                                 thisEntity(),
                                                 GetScore(Identifier.questProgress, i + 1), 
                                                 j + 1))));
            }
            Write("\n");
        }
        Write("\n");

        // Dupa ce entitatea este incarcata si ii atribuie noul quest atunci reseteaza master sa nu il mai caute
        Write("# Reset trigger objective");
        //Write($"{ScoreboardCommand(0)} {entities(0)} {scores(scoreType.qTag, n)} -1");
        Write(Scoreboard(ScoreboardType.Set, masterEntity(), GetScore(Identifier.questTag, n.name), -1));

        Write("# Implement changes");
        Write(Function($"npc/{GetNPC_NamesForFiles(n.name)}/q002_updateNpcQuests"));

        #endregion


        #region q002_updateNpcQuests

        cachedLocation = Application.streamingAssetsPath + $"{GetParentDirectoryForQuests()}npc/{GetNPC_NamesForFiles(n.name)}/q002_updateNpcQuests.mcfunction";
        //Create file
        Write($"# Dialogue and particle change for {n.name}");

        for(int i = n.conversations.Count - 1; i >= 0; i--)
        {
            Write($"# Update for quest about {n.conversations[i].descriereConversatie}");
            for(int j = 0; j < n.conversations[i].lines.Count; j++)
            {
                Write($"{Execute(entity(GetScore(Identifier.questProgress, i + 1), j + 1, false, true), here(), "")} dialogue change @s \"{n.name.ToLower()}_q{i + 1}s{j + 1}\"");

                switch (n.conversations[i].lines[j].particle)
                {
                    case ParticleForQuest.BeginQuest:
                        Write($"{Execute(entity(GetScore(Identifier.questProgress, i + 1), j + 1, false, true), here(), "")}  event entity @s scaiquest:ev_quest_info");
                        break;
                    case ParticleForQuest.WaitsForQuests:
                        Write($"{Execute(entity(GetScore(Identifier.questProgress, i + 1), j + 1, false, true), here(), "")} event entity @s scaiquest:ev_quest_in_progress");
                        break;
                    case ParticleForQuest.QuestInfo:
                        Write($"{Execute(entity(GetScore(Identifier.questProgress, i + 1), j + 1, false, true), here(), "")} event entity @s scaiquest:ev_quest_info");
                        break;
                    case ParticleForQuest.Idle:
                        Write($"{Execute(entity(GetScore(Identifier.questProgress, i + 1), j + 1, false, true), here(), "")} event entity @s scaiquest:ev_quest_off");
                        break;
                }

                Write("");
            }
            Write("");
        }

        #endregion
    }


    /// <summary>
    /// Counts the number of item present in the whole story
    /// </summary>
    int itemCounter;
    void GenerateFetch(NPC n)
    {
        Directory.CreateDirectory(Application.streamingAssetsPath + $"{GetParentDirectoryForQuests()}npc/{GetNPC_NamesForFiles(n.name)}/mat/");

        // Datorita limitei de caractere in scoreboarduri, cachedName scrie mereu doar primele 6 litere din numele npcurilor
        string cachedName = GetShortNameForNPC(n.name);


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

                        // Score for item tracked in the quest
                        string cachedCounter = $"rlcnt{itemCounter}" + (cachedItem.id.Length > 8 ? cachedItem.id.Substring(0, 8) : cachedItem.id);

                        // Verifica Daca A Luat Toate Itemele - VDALTI
                        string cachedVDALTI = $"VDALTI{cachedName}{m}";

                        itemScoreboard.Add(cachedCounter);
                        itemQuantity.Add(cachedItem.quantity);

                        #endregion


                        #region Generare StartGiving

                        // This file is called by the button pressed by the player on the dialogue window of the npc.
                        // The file starts the command block chain to start taking items from the player to complete the quest

                        //cachedLocation = Application.streamingAssetsPath + $"{GetParentDirectoryForQuests()}npc/{GetNPC_NamesForFiles(n.name)}/mat/StartGiving{cachedItem.id}.mcfunction";

                        //Write("# Activeaza command blocku");

                        //Write(SetBlock(GetPositionFromVector(cachedItem.redstoneBlock), "minecraft:redstone_block"));

                        //Write("# Trigger check pt a scoate blocul de redstone");

                        ////entities(Entities.NoDelivery)
                        //Write(Scoreboard(ScoreboardType.Add, entity(GetScore(Identifier.ActiveDelivery), -1, true), GetScore(Identifier.ActiveDelivery), 1));

                        //Write(Scoreboard(ScoreboardType.Set, masterEntity(), cachedVDALTI, 0));

                        #endregion


                        #region Generare additem

                        // This file is called by the command block each time the player gives an item to the quest
                        // The function increments the sidebar progress fake-player but also checks if the quest is completed

                        cachedLocation = Application.streamingAssetsPath + $"{GetParentDirectoryForQuests()}npc/{GetNPC_NamesForFiles(n.name)}/mat/add{cachedItem.id}.mcfunction";

                        Write("# Incrementeaza counteru");

                        Write(Scoreboard(ScoreboardType.Add, masterEntity(), cachedCounter, 1));

                        Write("# Actualizeaza sidebar");

                        Write(Scoreboard(ScoreboardType.Operation, "", "", 0) + $"\"Bring {n.alternativeName} {cachedItem.quantity} {cachedItem.alternativeName}\" sidebarQprog = {masterEntity()} {cachedCounter}");

                        Write("# Verifica daca conditiile questului au fost indeplinite");
                        
                        //Write($"{Function(1, n, i, j)}");
                        Write(Function($"npc/{GetNPC_NamesForFiles(n.name)}/mat/checkQuestComplete_q{i + 1}s{j + 1}"));

                        #endregion


                        #region Generare intreruptItem

                        // This function is called when the command block finished taking the items from the player. 
                        // This can either happen when the player runs out of items to give or he has finished giving the items


                        cachedLocation = Application.streamingAssetsPath + $"{GetParentDirectoryForQuests()}npc/{GetNPC_NamesForFiles(n.name)}/mat/intrerupt{cachedItem.id}.mcfunction";
                        Write("# Intrerupt command block chain");

                        Write(SetBlock(GetPositionFromVector(cachedItem.redstoneBlock), "air"));

                        Write(Scoreboard(ScoreboardType.Reset, masterEntity(), cachedVDALTI, 0));

                        //..entities(Entities.HasDelivery)
                        Write(Scoreboard(ScoreboardType.Remove, entity(GetScore(Identifier.ActiveDelivery), 0, true), "activeDelivery", 1));

                        Write(Scoreboard(ScoreboardType.Reset,
                                         $"\"Bring {n.alternativeName} {cachedItem.quantity} {cachedItem.alternativeName}\"",
                                         "sidebarQprog",
                                         0));

                        #endregion


                        itemCounter++;
                    }
                }

                #region Create Quest Complete


                if (itemScoreboard.Count > 0)
                {
                    cachedLocation = Application.streamingAssetsPath + $"{GetParentDirectoryForQuests()}npc/{GetNPC_NamesForFiles(n.name)}/mat/questComplete_q{i + 1}s{j + 1}.mcfunction";

                    Write($"# Things to do after the quest was completed");
                    Write($"# Set the scoreboard for every character after the quest is completed");

                    string convSB = GetDialogueValue(i, j + 1);
                    Write(Scoreboard(ScoreboardType.Set, thisEntity(), GetScore(Identifier.questTag, n.name), int.Parse(convSB)));

                }

                #endregion


                #region Create Check Quest Completion

                for (int b = 0; b < itemScoreboard.Count; b++)
                {
                    cachedLocation = Application.streamingAssetsPath + $"{GetParentDirectoryForQuests()}npc/{GetNPC_NamesForFiles(n.name)}/mat/checkQuestComplete_q{i + 1}s{j + 1}.mcfunction";
                    Write($"# Check if the quest took all the item for the quest from the inventory");

                    List<string> scItem = new List<string>();
                    scItem.Add(itemScoreboard[b]);
                    List<int> qItem = new List<int>();
                    qItem.Add(itemQuantity[b]);

                    Write(Execute(entity(scItem, qItem), here(), Function($"npc/{GetNPC_NamesForFiles(n.name)}/mat/intrerupt{n.conversations[i].lines[j].FetchItems[b].id}")));

                }


                if (itemScoreboard.Count > 0)
                {
                    Write(Execute(entity(itemScoreboard, itemQuantity), here(), Function($"npc/{GetNPC_NamesForFiles(n.name)}/mat/questComplete_q{i + 1}s{j + 1}")));

                }

                #endregion
            }
        }

    }


    
    /// <summary>
    /// Generates qx0y_ceva cu care face tranzitia intre questuri
    /// </summary>
    /// <param name="n"></param>
    void GenerateTransitions(NPC n)
    {
        string cachedName = GetShortNameForNPC(n.name);


        for(int i = 0; i < n.conversations.Count; i++)
        {
            for(int j = 0; j < n.conversations[i].lines.Count; j++)
            {
                cachedLocation = Application.streamingAssetsPath + $"{GetParentDirectoryForQuests()}npc/{GetNPC_NamesForFiles(n.name)}/q{GetDialogueValue(i, j)}_{GetTransition(n.conversations[i].lines[j].questType)}.mcfunction";

                string desc = n.conversations[i].descriereConversatie;

                switch (n.conversations[i].lines[j].questType)
                {
                    case QuestType.Info:
                        Write($"# Gives information about quest {desc} / Accepts quest");
                        break;
                    case QuestType.Fetch:
                        Write($"# Start trying to give items to NPC");
                        break;
                    case QuestType.Reward:
                        Write($"# Rewards player for his/her work");
                        break;
                    case QuestType.Custom:
                        Write($"# Custom quest");
                        break;
                }

                if(n.conversations[i].lines[j].questType != QuestType.Fetch)
                {
                    foreach(string s in n.conversations[i].lines[j].nextDialogues)
                    {
                        foreach(NPC npc in npcs)
                        {
                            foreach(Conversation conv in npc.conversations)
                            {
                                foreach(Dialogue line in conv.lines)
                                {
                                    if (line.dialogueId == s)
                                    {
                                        Write("# Obiective progress questuri. Se tin pe fiecare NPC care participa la quest");

                                        string convSB = GetDialogueValue(npc.conversations.IndexOf(conv), 
                                                                         npc.conversations[npc.conversations.IndexOf(conv)].lines.IndexOf(line));

                                        Write(Scoreboard(ScoreboardType.Set, masterEntity(), GetScore(Identifier.questTag, npc.name), int.Parse(convSB)));
                                    }
                                }
                            }
                        }
                    }

                    

                    if(n.conversations[i].lines.Count > j + 1)
                    {
                        if(n.conversations[i].lines[j + 1].questType == QuestType.Fetch)
                        {
                            Write("\n");

                            Write("### Pregateste scoreboards pentru urmatoru quest care o sa fie fetch ###");
                            Write("# incrementarea se face prin command blocuri");


                            for (int m = 0; m < n.conversations[i].lines[j + 1].FetchItems.Count; m++)
                            {
                                Item cachedItem = n.conversations[i].lines[j + 1].FetchItems[m];
                                
                                string cachedCounter = $"rlcnt{itemCounter}" + (cachedItem.id.Length > 8 ? cachedItem.id.Substring(0, 8) : cachedItem.id);

                                Write(Scoreboard(ScoreboardType.Add, masterEntity(), cachedCounter, 0));

                                Write(Scoreboard(ScoreboardType.Operation, "", "", 0) + $"\"Bring {n.alternativeName} {cachedItem.quantity} {cachedItem.alternativeName}\" sidebarQprog = {masterEntity()} {cachedCounter}");

                                itemCounter++;
                            }
                        }
                    }
                }
                else
                {
                    for (int m = 0; m < n.conversations[i].lines[j].FetchItems.Count; m++)
                    {

                        //Write(Function($"npc/{GetNPC_NamesForFiles(n.name)}/mat/StartGiving{item.id}"));

                        Item cachedItem = n.conversations[i].lines[j].FetchItems[m];

                        // Verifica Daca A Luat Toate Itemele - VDALTI
                        string cachedVDALTI = $"VDALTI{cachedName}{m}";

                        Write("# Activeaza command blocku");

                        Write(SetBlock(GetPositionFromVector(cachedItem.redstoneBlock), "minecraft:redstone_block"));

                        Write("# Trigger check pt a scoate blocul de redstone");

                        //entities(Entities.NoDelivery)
                        Write(Scoreboard(ScoreboardType.Add, entity(GetScore(Identifier.ActiveDelivery), -1, true), GetScore(Identifier.ActiveDelivery), 1));

                        Write("# initializare timer de scos bloc de redstone pt coal.");
                        Write("# scadem din el in activeDelivery.mcfunction");
                        Write("# Il prelungim/resetam in add" + cachedItem + ".mcfunction");

                        Write(Scoreboard(ScoreboardType.Set, masterEntity(), cachedVDALTI, 0));

                        Write("");
                    }
                }

                Write("");
                Write(Function($"npc/{GetNPC_NamesForFiles(n.name)}/q002_updateNpcQuests"));
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


    /// <summary>
    /// Transforma din dialogue group contrainer in list de npc care sa fie dupa aceea tradus in minecraft functions
    /// </summary>
    public void TransformDialogueContainerToList()
    {
        npcs.Clear();


        for (int i = 0; i < dialogueContainer.DialogueGroups.Count; i++)
        {
            List<InitiumDialogueSO> conversatie = new List<InitiumDialogueSO>();
            conversatie = dialogueContainer.DialogueGroups.Values.ToList()[i];
            conversatie.Sort(delegate(InitiumDialogueSO x, InitiumDialogueSO y)
            {
                return x.name.CompareTo(y.name);
            });

            for(int j = 0; j < conversatie.Count; j++)
            {
                // daca nu gaseste speakeru, il creaza,
                if (!npcs.Any(npc => npc.name == conversatie[j].Speaker))
                {
                    npcs.Add(new NPC(conversatie[j].Speaker));
                }

                var n = npcs.First(npc => npc.name == conversatie[j].Speaker);
                n.alternativeName = conversatie[j].AltSpeaker;

                // daca speakeru nu are o conversatie cu indexul (i) in conversatiea lui de conversatii, o creaza
                if (!n.conversations.Any(conv => conv.priority == i))
                {
                    n.conversations.Add(new Conversation(i, dialogueContainer.DialogueGroups.Keys.ToList()[i].GroupName));
                }

                var con = n.conversations.First(conv => conv.priority == i);
                // Trebuie schimabta incat in functie de quest sa adauge diverse lucruri
                // Adica daca e un fetch quest sa ii adauge si iteme, dar doar atunci sa le adauge, nu si daca e info quest

                Dialogue dialogue = new Dialogue(conversatie[j]);
                //Aici cauta primul npc cu numele vorbitului care spune dialogul cu indexul j din conversatie, dupa aceea cauta conversatia npcului care are legatura cu "conversatie", dupa aceea adauga o noua linie de dialog 
                con.lines.Add(dialogue);
            }
        }
    }


    private void GenerateInitialiserForScoreboards()
    {
        Directory.CreateDirectory(Application.streamingAssetsPath + $"{GetParentDirectoryForQuests()}scoreboardsAddon/");
        cachedLocation = Application.streamingAssetsPath + $"{GetParentDirectoryForQuests()}scoreboardsAddon/quest.mcfunction";

        Write("# Every scoreboard used by the quest will be initialised here");

        Write("scoreboard objectives remove activeDelivery");
        Write("scoreboard objectives add activeDelivery dummy");

        Write("scoreboard objectives remove sidebarQprog");
        Write("scoreboard objectives add sidebarQprog dummy");

        //Write($"{ScoreboardCommand(0)} @s activeDelivery -1");
        Write(Scoreboard(ScoreboardType.Set, thisEntity(), GetScore(Identifier.ActiveDelivery), -1));

        Write("\n");

        int itemCounter = 0;
        foreach (NPC n in npcs)
        {
            Write($"# ---- Scoreboards for {n.name}/{n.alternativeName} ----");

            //scores(scoreType.qTag, n)
            Write($"scoreboard objectives remove {GetScore(Identifier.questTag, n.index)}");
            Write($"scoreboard objectives add {GetScore(Identifier.questTag, n.index)} dummy");
            Write("");


            string cachedName = n.name.Length > 6 ? n.name.Substring(0, 6) : n.name;

            foreach (Conversation c in n.conversations)
            {
                Write($"scoreboard objectives remove questProgress_q{n.conversations.IndexOf(c) + 1}");
                Write($"scoreboard objectives add questProgress_q{n.conversations.IndexOf(c) + 1} dummy");
                Write("");

                foreach (Dialogue d in c.lines)
                {
                    if (d.questType == QuestType.Fetch)
                    {
                        foreach (Item i in d.FetchItems)
                        {
                            string cachedCounter = $"rlcnt{itemCounter}" + (i.id.Length > 8 ? i.id.Substring(0, 8) : i.id);

                            string cachedVDALTI = $"VDALTI{cachedName}{d.FetchItems.IndexOf(i)}";

                            Write($"scoreboard objectives remove {cachedCounter}");
                            Write($"scoreboard objectives add {cachedCounter} dummy");

                            Write($"scoreboard objectives remove {cachedVDALTI}");
                            Write($"scoreboard objectives add {cachedVDALTI} dummy");


                            //Write($"{ScoreboardCommand(0)} @s {cachedCounter} 0");
                            Write(Scoreboard(ScoreboardType.Set, thisEntity(), cachedCounter, 0));

                            itemCounter++;
                            Write("");

                        }
                    }
                }

            }

            Write("\n");

        }

    }

    private void GenerateMainExtenstion()
    {
        Directory.CreateDirectory(Application.streamingAssetsPath + $"{GetParentDirectoryForQuests()}scoreboardsAddon/");
        cachedLocation = Application.streamingAssetsPath + $"{GetParentDirectoryForQuests()}scoreboardsAddon/mainExtenstion.mcfunction";

        Write("# Main function extension, usually for items");

        //Write("execute @e[scores={activeDelivery=0..}] ~~~ function triggers/activeDelivery\n");

        Write(Execute(entity(GetScore(Identifier.ActiveDelivery), 0, true), here(), Function("triggers/activeDelivery")));
        Write("");

        for(int i = 0; i < npcs.Count; i++)
        {
            //Write("execute @e[scores = {globalTimer1Sec = " + );

        }

        foreach (NPC n in npcs)
        {
            //Write("execute @e[scores = {globalTimer1Sec = " + n.index + $", {scores(scoreType.qTag, n)} = 0.." + "}] ~ ~ ~ " + $"{Execute(ExecuteType.OnNpcMarker, n)} {Function(2, n)}");
            Write(Execute("@e[scores = {" + $"{GetScore(Identifier.globalTimer1Sec)} = {n.index}, " +
                          $"{GetScore(Identifier.questTag, n.name)} = 0.." + "}] ", 
                          here(), 
                          Execute(entity(GetScore(Identifier.npcMarker), n.index), 
                                  here(), 
                                  Function($"npc/{GetNPC_NamesForFiles(n.name)}/q001_checkQuestStages"))));
        }

        Write("\n");


        Directory.CreateDirectory(Application.streamingAssetsPath + $"{GetParentDirectoryForQuests()}triggers/");
        cachedLocation = Application.streamingAssetsPath + $"{GetParentDirectoryForQuests()}triggers/activeDelivery.mcfunction";


        foreach (NPC n in npcs)
        {
            string cachedName = GetShortNameForNPC(n.name);

            foreach (Conversation c in n.conversations)
            {
                foreach (Dialogue d in c.lines)
                {
                    if(d.questType == QuestType.Fetch)
                    {
                        foreach (Item i in d.FetchItems)
                        {
                            string cachedVDALTI = $"VDALTI{cachedName}{d.FetchItems.IndexOf(i)}";

                            //Write($"{ScoreboardCommand(ScoreboardCommands.Add)} @e[scores = " + "{" + cachedVDALTI + " = 0..}] " + cachedVDALTI + " 1");
                            Write(Scoreboard(ScoreboardType.Add, $"@e[scores = " + "{" + cachedVDALTI + " = 0..}] ", cachedVDALTI, 1));

                            //Write($"execute @e[scores = " + 
                            //    "{" + 
                            //    cachedVDALTI + 
                            //    " = " + 
                            //    (i.quantity + 5).ToString() + 
                            //    "..}] ~ ~ ~ function npc/sqst" + 
                            //    $"{n.name}/mat/intrerupt{i.id}");

                            Write(Execute("@e[scores = " + "{" + cachedVDALTI + " = " + (i.quantity + 5).ToString() + "..}]",
                                          here(),
                                          Function(GetParentDirectoryForQuests(true) + GetNPC_NamesForFiles(n.name) + "/mat/intrerupt" + i.id)));
                        }
                    }
                    
                }

            }

            Write("");
        }
    }
}

