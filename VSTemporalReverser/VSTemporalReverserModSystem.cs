using Vintagestory.API.Common;

namespace VSTemporalReverser;

public class VSTemporalReverserModSystem : ModSystem
{
    public override void Start(ICoreAPI api)
    {
        base.Start(api);
        api.RegisterItemClass("ItemTemporalReverser", typeof(ItemTemporalReverser));
        api.RegisterBlockClass("BlockRestoredCanopyBed", typeof(BlockRestoredCanopyBed));
    }
}
