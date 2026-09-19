using A2m.Server;
using Server.Reawakened.Database.Characters;

namespace Server.Reawakened.Players.Models.Character;

public class StatusEffectsModel(CharacterDbEntry entry)
{
    public Dictionary<ItemEffectType, StatusEffectModel> Effects => entry.StatusEffects;

    public void Add(ItemEffect effect, string prefabName)
    {
        var shouldReplaceEffect = true;

        if (Effects.TryGetValue(effect.Type, out var statusData))
            if (statusData.Value > effect.Value && statusData.Expiry > DateTime.UtcNow)
                shouldReplaceEffect = false;

        if (shouldReplaceEffect)
        {
            var duration = TimeSpan.FromSeconds(effect.Duration);
            Remove(effect.Type);
            Effects.Add(effect.Type, new StatusEffectModel(effect.Type, effect.Value, DateTime.UtcNow + duration, prefabName));
        }
    }

    public void Remove(ItemEffectType effect) => Effects.Remove(effect);

    public void UpdateStatus()
    {
        var effects = new Dictionary<ItemEffectType, StatusEffectModel>(Effects);
        
        foreach (var effect in effects)
        {
            if (effect.Value == null || effect.Value.Expiry <= DateTime.UtcNow)
                Remove(effect.Key);
        }
    }

    public float GetEffect(ItemEffectType effect)
    {
        var output = 0f;

        if (Effects.TryGetValue(effect, out var statusData))
            if (statusData.Effect == effect)
            {
                if (statusData.Expiry > DateTime.UtcNow)
                    output = statusData.Value;
                else
                    Remove(effect);
            }

        return output;
    }

    public bool HasEffect(ItemEffectType effect)
    {
        if (Effects.TryGetValue(effect, out var statusData))
            if (statusData.Effect == effect)
            {
                if (statusData.Expiry > DateTime.UtcNow)
                    return true;
                else
                    Remove(effect);
            }

        return false;
    }
}
