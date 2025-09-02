
namespace DIALOGUE
{
    public class DIALOGUE_LINE
    {
        public DL_SPEAKER_DATA speaker;
        public DL_DIALOGUE_DATA dialogue;
        public string commands;

        public bool hasSpeaker => speaker != null;
        public bool hasDialogue => dialogue.hasDialogue;
        public bool hasCommands => commands != string.Empty;

        public DIALOGUE_LINE(string speaker, string dialogue, string commands)
        {
            this.speaker    = (string.IsNullOrWhiteSpace(speaker) ? null : new DL_SPEAKER_DATA(speaker));
            this.dialogue   = new DL_DIALOGUE_DATA(dialogue);
            this.commands = commands;
        }
    }
}


