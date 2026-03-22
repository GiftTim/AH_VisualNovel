using UnityEngine;

namespace VISUALNOVEL
{
    public class VNDatabaseLinkSetup : MonoBehaviour
    {
        public void SetupExtrnalLinks()
        {

            VariableStore.CreateVariable("VN.mainCharacterName", "", 
                                            () => VNGameSave.activeFile.playerName, 
                                            value => VNGameSave.activeFile.playerName = value);


        }
    }
}
