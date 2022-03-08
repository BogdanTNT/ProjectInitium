using UnityEngine;

namespace Initium
{
    using ScriptableObjects;

    public class InitiumDialogue : MonoBehaviour
    {
        /* Dialogue Scriptable Objects */
        [SerializeField] private InitiumDialogueContainerSO dialogueContainer;
        [SerializeField] private InitiumDialogueGroupSO dialogueGroup;
        [SerializeField] private InitiumDialogueSO dialogue;

        /* Filters */
        [SerializeField] private bool groupedDialogues;
        [SerializeField] private bool startingDialoguesOnly;

        /* Indexes */
        [SerializeField] private int selectedDialogueGroupIndex;
        [SerializeField] private int selectedDialogueIndex;
    }
}