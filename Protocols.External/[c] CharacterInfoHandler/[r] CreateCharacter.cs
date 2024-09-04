using A2m.Server;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Database.Characters;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Enums;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Services;
using Server.Reawakened.XMLs.Bundles.Base;

namespace Protocols.External._c__CharacterInfoHandler;

public class CreateCharacter : ExternalProtocol
{
    public override string ProtocolName => "cr";

    public CharacterHandler CharacterHandler { get; set; }
    public NameGenSyllables NameGenSyllables { get; set; }
    public ServerRConfig ServerConfig { get; set; }
    public WorldGraph WorldGraph { get; set; }
    public WorldHandler WorldHandler { get; set; }
    public EventPrefabs EventPrefabs { get; set; }

    public override void Run(string[] message)
    {
        var firstName = message[5];
        var middleName = message[6];
        var lastName = message[7];

        var gender = (Gender)int.Parse(message[8]);
        var characterEntry = new CharacterDbEntry(message[9]);

        var tribe = TribeType.Invalid;

        var names = new[] { firstName, middleName, lastName };

        if (NameGenSyllables.IsNameReserved(names, CharacterHandler))
        {
            var suggestion = NameGenSyllables.GetRandomName(gender, CharacterHandler);

            SendXt("cr", 0, suggestion[0], suggestion[1], suggestion[2]);
        }
        else if (Player.UserInfo.CharacterIds.Count > ServerConfig.MaxCharacterCount)
        {
            SendXt("cr", 1);
        }
        else
        {
            characterEntry.Allegiance = tribe;
            characterEntry.CharacterName = string.Join(string.Empty, names);
            characterEntry.UserUuid = Player.UserId;
            
            characterEntry.Registered = true;

            characterEntry.LevelId = WorldGraph.NewbZone;
            characterEntry.SpawnPointId = string.Empty;

            CharacterHandler.Add(characterEntry);

            var characterData = CharacterHandler.GetCharacterFromName(characterEntry.CharacterName);

            characterData.SetLevelXp(1, ServerConfig);

            Player.AddCharacter(characterData);

            var levelInfo = WorldHandler.GetLevelInfo(characterData.LevelId);

            Player.SendStartPlay(characterData, levelInfo, EventPrefabs);
        }
    }
}
