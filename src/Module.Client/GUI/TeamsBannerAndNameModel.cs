using System;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.ImageIdentifiers;

namespace Crpg.Module;
public class TeamsBannerAndNameModel
{
    public BannerImageIdentifierVM Banner1;
    public BannerImageIdentifierVM Banner2;
    public string Team1Name;
    public string Team2Name;
    public TeamsBannerAndNameModel(BannerImageIdentifierVM banner1, BannerImageIdentifierVM banner2, string team1Name, string team2Name)
    {
        Banner1 = banner1;
        Banner2 = banner2;
        Team1Name = team1Name;
        Team2Name = team2Name;
    }
}
