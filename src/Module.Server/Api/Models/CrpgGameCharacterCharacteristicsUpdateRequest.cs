using Crpg.Module.Api.Models.Characters;
using Crpg.Module.Api.Models.Items;

namespace Crpg.Module.Api.Models;

internal class CrpgGameCharacterCharacteristicsUpdateRequest
{
    public CrpgCharacterCharacteristics Characteristics { get; set; } = new();
}
