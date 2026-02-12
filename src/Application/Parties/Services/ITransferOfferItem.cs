namespace Crpg.Application.Parties.Services;

/// <summary>
/// Represents an item in a transfer offer.
/// </summary>
internal interface ITransferOfferItem
{
    string ItemId { get; }
    int Count { get; }
}
