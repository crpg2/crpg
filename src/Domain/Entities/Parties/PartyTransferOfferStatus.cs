namespace Crpg.Domain.Entities.Parties;

/// <summary>
/// Status of a <see cref="PartyTransferOffer"/>.
/// </summary>
public enum PartyTransferOfferStatus
{
    /// <summary>
    /// Initial state when the offer is created.
    /// </summary>
    Intent,

    /// <summary>
    /// Pending response from the target party.
    /// </summary>
    Pending,
}
