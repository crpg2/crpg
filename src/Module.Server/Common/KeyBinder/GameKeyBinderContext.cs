using System.Collections.Generic;
using System.Linq;
using Crpg.Module.Common.KeyBinder.Models;
using TaleWorlds.InputSystem;

public sealed class GameKeyBinderContext : GameKeyContext
{
    private readonly IEnumerable<BindedKey> keys;

    public GameKeyBinderContext(string categoryId, IEnumerable<BindedKey> keys)
        : base(categoryId, 109 + keys.Count())
    {
        this.keys = keys;
        // RegisterHotKeys();
        RegisterGameKeys();
        // RegisterGameAxisKeys();
    }

    private void RegisterHotKeys()
    {
        // TODO GameKeyBinderContext.RegisterHotKeys()
    }

    private void RegisterGameKeys()
    {
        // Be carefull with this 109
        int i = 109;
        foreach (BindedKey key in keys)
        {
            key.KeyId = i++;
            GameKey gameKey = new(key.KeyId, key.Id, GameKeyCategoryId, key.DefaultInputKey, key.DefaultControllerKey, GameKeyCategoryId);
            RegisterGameKey(gameKey);
        }
    }

    private void RegisterGameAxisKeys()
    {
        // TODO GameKeyBinderContext.RegisterGameAxisKeys()
    }
}
