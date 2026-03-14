using Crpg.Module.Common.KeyBinder.Models;
using TaleWorlds.InputSystem;

namespace Crpg.Module.Common.KeyBinder;

public sealed class GameKeyBinderContext : GameKeyContext
{
    // Base ID for custom keys via KeyBinder, should be above the game's default, and gamekeys added via other methods than KeyBinder interface
    // currently 111 game keys, so starting from 125 to be safe for future additions.
    private const int BaseKeyId = 125;
    private readonly IEnumerable<BindedKey> keys;

    public GameKeyBinderContext(string categoryId, IEnumerable<BindedKey> keys)
        : base(categoryId, BaseKeyId + keys.Count())
    {
        this.keys = keys;
        RegisterGameKeys();
    }

    private void RegisterGameKeys()
    {
        // Be carefull with this BaseKeyId
        int i = BaseKeyId;
        foreach (BindedKey key in keys)
        {
            key.KeyId = i++;
            GameKey gameKey = new(key.KeyId, key.Id, GameKeyCategoryId, key.DefaultInputKey, key.DefaultControllerKey, GameKeyCategoryId);
            RegisterGameKey(gameKey);
        }
    }
}
