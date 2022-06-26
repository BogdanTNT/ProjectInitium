using Initium.ScriptableObjects;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static McFunction.McFunctions;
using static McCommands.McCommands;


[System.Serializable]
public class NPC
{
    [Tooltip("Numele la general")]
    public string name;

    [Tooltip("Deobicei numele din buletin.")]
    public string alternativeName;

    [HideInInspector]
    public int index;

    [Tooltip("Fiecare npc poate participa in diferite conversatii pe anumite subiecte")]
    public List<Conversation> conversations;

    public NPC(string speaker)
    {
        name = speaker;

        conversations = new List<Conversation>();
    }

}

/// <summary>
/// O conversatie este totalitatea linilor de dialog despre un anumit subiect 
/// <para>Sa zicem ca o conversatie este un Quest Line</para>
/// </summary>
[System.Serializable]
public class Conversation
{
    [Tooltip("Per total, despre ce e conversatia?")]
    public string descriereConversatie;

    [Tooltip("Fiecare conversatie poate avea mai multe linii de dialog")]
    public List<Dialogue> lines;
    public int priority;

    public Conversation(int index, string descriere)
    {
        priority = index;

        descriereConversatie = descriere;

        lines = new List<Dialogue>();
    }

    public Conversation(Conversation c)
    {
        descriereConversatie = c.descriereConversatie;
        lines = new List<Dialogue>(c.lines);
        priority = c.priority;
    }
}

// O linie de dialog reprezinta un quest efectiv, dar o linie de dialog poate fi si doar informativa 
[System.Serializable]
public class Dialogue
{
    [Tooltip("Codul de dialog pe care il spune npcul (lista de coduri este in en_US.lang)")]
    public string dialogueId;

    [Tooltip("Tipul de particula pentru linia de dialog")]
    public ParticleForQuest particle;

    [Tooltip("Tipul de quest efectiv (info, fetch, custom)")]
    public QuestType questType;
    public List<Item> FetchItems;

    public string dialogueText;

    //[HideInInspector]
    public bool isStartingDialogue;

    public List<string> nextDialogues;

    public Dialogue(string id, string text, ParticleForQuest overhead = ParticleForQuest.BeginQuest, QuestType type = QuestType.Info, List<Item> items = null, bool starter = false, List<string> choices = null)
    {
        dialogueId = id;
        dialogueText = text;

        particle = overhead;
        questType = type;

        FetchItems = new List<Item>();
        if(items != null)
            FetchItems = items;

        isStartingDialogue = starter;

        if(nextDialogues != null)
            nextDialogues = new List<string>(choices);
    }

    public Dialogue(InitiumDialogueSO dialogue)
    {
        dialogueId = dialogue.DialogueName;
        particle = dialogue.Particle;
        questType = dialogue.Type;
        FetchItems = new List<Item>(dialogue.FetchItems);
        dialogueText = dialogue.Text;
        isStartingDialogue = dialogue.IsStartingDialogue;
        nextDialogues = new List<string>();

        if (dialogue.Choices.Count != 0)
        {
            foreach(Initium.Data.InitiumDialogueChoiceData data in dialogue.Choices)
            {
                if(data != null)
                    if(data.NextDialogue != null)
                        nextDialogues.Add(data.NextDialogue.DialogueName);
            }
        }
            //nextDialogues = new List<string>(dialogue.Choices.ConvertAll(new System.Converter<Initium.Data.InitiumDialogueChoiceData, string>(n => n.NextDialogue.DialogueName)));
        
    }
}

// Info = ev_quest_info
// GivesQuest = ev_quest_pending
// WaitsQuest = ev_quest_in_progress
// Idle = ev_quest_off
[System.Serializable]
public enum ParticleForQuest
{
    BeginQuest, WaitsForQuests, QuestInfo, Idle 
}

[System.Serializable]
public enum QuestType
{
    Info, Fetch, Reward, Custom
}



/// <summary>
/// Reprezinta itemul care o sa fie dat de catre player in fetch quest
/// </summary>
[System.Serializable]
public class Item
{
    

    [Tooltip("Cate iteme tre sa aduca playeru?")]
    public int quantity;

    [Tooltip("Idul itemului pt mc")]
    public string id;

    [Tooltip("Numele mai usor de citit de catre player")]
    public string alternativeName;

    [Tooltip("Unde trebuie plasat redstone blocku ca sa poate sistemul sa ia itemul din inventarul playerului")]
    public Vector3 redstoneBlock;

    public Item(Item item)
    {
        quantity = item.quantity;
        id = item.id;
        alternativeName = item.alternativeName;
        redstoneBlock = item.redstoneBlock;
    }

    public Item()
    {
        quantity = 0;
        id = "Item Id";
        alternativeName = "Human Item Name";
        redstoneBlock = Vector3.zero;
    }
}


/// <summary>
/// Entire JSON class for the dialogue.json
/// </summary>
[System.Serializable]
public class DialogueJSON
{
    public string format_version = "1.17";

    [System.Serializable]
    public class NpcDialogue
    {
        [System.Serializable]
        public class Scene
        {
            public string scene_tag;

            /// <summary>
            /// Type of text used by the json to refrence things like dialogue and button
            /// </summary>
            [System.Serializable]
            public class RawText
            {
                public string translate;
                public List<string> with;

                public RawText(string whatToTranslate)
                {
                    translate = whatToTranslate.ToLower();
                    with = new List<string>();
                    with.Add(@"\n");
                }
            }
            
            /// <summary>
            /// Text dialogue for the line of conversation that the npc will tell
            /// </summary>
            [System.Serializable]
            public class Text
            {
                public List<RawText> rawtext;

                public Text(string s)
                {
                    rawtext = new List<RawText>();
                    rawtext.Add(new RawText(s.ToLower()));
                }
            }

            /// <summary>
            /// The Button text
            /// </summary>
            [System.Serializable]
            public class ButtonName
            {
                public List<ButtonRawText> rawtext;

                public ButtonName(string buttonName)
                {
                    rawtext = new List<ButtonRawText>();
                    rawtext.Add(new ButtonRawText(buttonName));
                }
            }
            /// <summary>
            /// The type of button to show
            /// </summary>
            [System.Serializable]
            public class ButtonRawText
            {
                public string translate;

                public ButtonRawText(string whatToTranslate)
                {
                    translate = whatToTranslate.ToLower();
                }
            }
            /// <summary>
            /// Button For Dialogue Window
            /// Also what command to execute when the button is pressed
            /// </summary>
            [System.Serializable]
            public class Button
            {
                public ButtonName name;
                public List<string> commands;

                public Button(string buttonName, string command)
                {
                    name = new ButtonName(buttonName);
                    commands = new List<string>();
                    commands.Add(command);
                }
            }

            public Text npc_name;

            public Text text;

            public List<Button> buttons;

            public Scene(Dialogue d, NPC n, int conv, int line)
            {

                scene_tag = $"{n.name}_q{conv}s{line}".ToLower();
                npc_name = new Text($"dlg_npc_{n.name}.name".ToLower());
                text = new Text($"dlg_npc_{n.name}.q{conv}s{line}");

                buttons = new List<Button>();

                if(n.conversations[conv - 1].lines[line - 1].questType == QuestType.Fetch)
                {
                    string functionCommand = line > 9 ? $"{conv}{line}" : $"{conv}0{line}";

                    buttons.Add(new Button($"dlg_npc_{n.name}.q{conv}s{line}.butt1", 
                                           $"/function {GetParentDirectoryForQuests(true)}npc/{GetNPC_NamesForFiles(n.name)}/q{functionCommand}_{GetTransition(n.conversations[conv - 1].lines[line - 1].questType)}"));
                }
                else if (n.conversations[conv - 1].lines[line - 1].questType == QuestType.Info)
                {
                    if(n.conversations[conv - 1].lines[line - 1].particle == ParticleForQuest.BeginQuest)
                    {
                        string functionCommand = line > 9 ? $"{conv}{line}" : $"{conv}0{line}";

                        buttons.Add(new Button($"dlg_npc_{n.name}.q{conv}s{line}.butt1", 
                                               $"/function {GetParentDirectoryForQuests(true)}npc/{GetNPC_NamesForFiles(n.name)}/q{functionCommand}_{GetTransition(n.conversations[conv - 1].lines[line - 1].questType)}"));
                    }
                }
                else if(n.conversations[conv - 1].lines[line - 1].questType == QuestType.Reward)
                {
                    string functionCommand = line > 9 ? $"{conv}{line}" : $"{conv}0{line}";

                    buttons.Add(new Button($"dlg_npc_{n.name}.q{conv}s{line}.butt1", 
                                           $"/function {GetParentDirectoryForQuests(true)}npc/{GetNPC_NamesForFiles(n.name)}/q{functionCommand}_{GetTransition(n.conversations[conv - 1].lines[line - 1].questType)}"));
                }
                else
                {
                    buttons = null;
                }
            }
        }

        public List<Scene> scenes;

        public NpcDialogue(NPC n)
        {
            scenes = new List<Scene>();
            for(int i = 0; i < n.conversations.Count; i++)
            {
                for(int j = 0; j < n.conversations[i].lines.Count; j++)
                {
                    scenes.Add(new Scene(n.conversations[i].lines[j], n, i + 1, j + 1));
                }
            }
        }
    }

    public NpcDialogue npc_dialogue;

    public DialogueJSON(NPC n)
    {
        npc_dialogue = new NpcDialogue(n);
    }


}

