using System.Text.Json.Serialization;

namespace Crpg.Application.Common.Results;

public class Error(ErrorType type, ErrorCode code)
{
    /// <summary>
    /// The correlation ID provided by the gateway.
    /// </summary>
    [JsonRequired]
    public string? TraceId { get; set; }

    /// <summary>
    /// A machine-readable code specifying error category. This information is used on client side to focus on
    /// certain type of error, to either retry some processing or display only certain type of errors.
    /// </summary>
    public ErrorType Type { get; } = type;

    /// <summary>
    /// A unique machine-readable error code string.
    /// </summary>
    public ErrorCode Code { get; } = code;

    /// <summary>
    /// A short, human-readable summary of the problem type. It should not change from occurrence to occurrence
    /// of the problem, except for purposes of localization.
    /// </summary>
    [JsonRequired]
    public string? Title { get; init; }

    /// <summary>
    /// A human-readable explanation specific to this occurrence of the problem.
    /// </summary>
    [JsonRequired]
    public string? Detail { get; init; }

    /// <summary>
    /// A machine-readable structure to reference to the exact location causing the error.
    /// </summary>
    [JsonRequired]
    public ErrorSource? Source { get; init; }

    /// <summary>
    /// A human-readable stacktrace.
    /// </summary>
    [JsonRequired]
    public string? StackTrace { get; init; }

    public override string ToString() => Detail ?? Title ?? Code.ToString();
}
