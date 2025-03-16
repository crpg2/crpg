using Crpg.GameServerManager.Api.Models;

namespace Crpg.GameServerManager.Api.Exceptions;

internal class CrpgClientException : Exception
{
    private readonly CrpgResult _crpgResult;

    public CrpgClientException(CrpgResult crpgResult)
    {
        _crpgResult = crpgResult;
    }

    public string ErrorCode => _crpgResult.Errors![0].Code;

    public override string ToString()
    {
        return ErrorCode;
    }
}
