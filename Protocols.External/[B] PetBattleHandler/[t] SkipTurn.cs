using Server.Reawakened.Network.Protocols;

namespace Protocols.External._B__PetBattleHandler;
public class SkipTurn : ExternalProtocol
{
    public override string ProtocolName => "Bt";

    public override void Run(string[] message)
    {
        var characterName = message[5];

        var model = Player.TempData.PetBattleModel;
        
        if (model.BattleOver)
            return;
        
        model.MyTurn = !model.MyTurn;
        
        SendXt("BT", model.MyTurn ? "0" : "1");
    }
}
