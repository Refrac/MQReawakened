using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Rooms.Services;

namespace Protocols.External._B__PetBattleHandler;
public class InitBattle : ExternalProtocol
{
    public override string ProtocolName => "Bi";
    
    public WorldHandler WorldHandler { get; set; }

    public override void Run(string[] message)
    {
        var characterName = message[5];
        
        var model = Player.TempData.PetBattleModel;

        if (model == null)
        {
            WorldHandler.ChangePlayerRoom(Player, 47);
            return;
        }

        if (model.IsAI)
        {
            Player.SendXt("BI", "1", characterName, characterName);
        }
        else
        {
            var challenger = model.IsChallenger ? characterName : model.Enemy.Character.CharacterName;
            var challenged = !model.IsChallenger ? characterName : model.Enemy.Character.CharacterName;

            Player.SendXt("BI", model.IsChallenger ? "1" : "0", challenger, challenged);
        }
    }
}
