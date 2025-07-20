using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;
using System;
using System.Collections.Generic;
using System.Linq;

public class ItemDisplayVM : ViewModel
{
    public ImageIdentifierVM ImageVM { get; }
    public string Name { get; }

    public ItemDisplayVM(ItemObject item)
    {
        ImageVM = new ImageIdentifierVM(item);
        Name = item.Name.ToString();
    }

    // Expose the image properties for binding convenience
    [DataSourceProperty]
    public string Id => ImageVM.Id;

    [DataSourceProperty]
    public string AdditionalArgs => ImageVM.AdditionalArgs;

    [DataSourceProperty]
    public int ImageTypeCode => ImageVM.ImageTypeCode;

    [DataSourceProperty]
    public string DisplayName => Name;
}
