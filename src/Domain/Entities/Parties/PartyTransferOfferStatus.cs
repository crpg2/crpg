using Crpg.Domain.Entities.Parties;

namespace Crpg.Domain.Entities.Parties;

/// <summary>
/// Status of a <see cref="PartyTransferOffer"/>.
/// </summary>
public enum PartyTransferOfferStatus
{
    /// <summary>
    /// TODO:
    /// </summary>
    Intent,

    /// <summary>
    /// </summary>
    Pending,

    /// <summary>
    /// TODO:
    /// </summary>
    Declined,

    /// <summary>
    /// TODO:
    /// </summary>
    Accepted,
}
