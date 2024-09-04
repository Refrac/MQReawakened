﻿using Server.Reawakened.Database.Characters;
using Server.Reawakened.Players.Enums;
using Server.Reawakened.XMLs.Bundles.Base;

namespace Server.Reawakened.Players.Helpers;

public class NameGenSyllables(NameSyllables nameGen, Random random)
{
    private string GetRandomFromList(IReadOnlyList<string> names) =>
        names[random.Next(names.Count)];

    public static bool IsNameReserved(string[] names, CharacterHandler handler) =>
        handler.ContainsCharacterName(GetName(names));

    public static string GetName(string[] names) =>
        $"{names[0]}{names[1]}{names[2]}";

    public bool IsPossible(Gender gender, string[] names) =>
        nameGen.Syllables[gender][0].Contains(names[0]) &&
        nameGen.Syllables[gender][1].Contains(names[1]) &&
        nameGen.Syllables[gender][2].Contains(names[2]);

    public string[] GetRandomName(Gender gender, CharacterHandler handler)
    {
        while (true)
        {
            var names = new[]
            {
                GetRandomFromList(nameGen.Syllables[gender][0]),
                GetRandomFromList(nameGen.Syllables[gender][1]),
                GetRandomFromList(nameGen.Syllables[gender][2])
            };

            if (IsNameReserved(names, handler))
                continue;

            return names;
        }
    }
}
