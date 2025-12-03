using UnityEngine;

[System.Serializable]
public class DialogueLine
{
    public string speakerName;
    [TextArea(3, 10)]
    public string dialogueText;
    
    public DialogueLine(string speaker, string text)
    {
        speakerName = speaker;
        dialogueText = text;
    }
}

[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue System/Dialogue")]
public class DialogueData : ScriptableObject
{
    public DialogueLine[] dialogueLines;
    public bool canSkip = true;
    public bool pauseGame = false; // Whether to pause the game during dialogue
}