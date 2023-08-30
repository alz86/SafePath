using Volo.Abp.Settings;

namespace SafePath.Settings;

public class SafePathSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        //Define your own settings here. Example:
        //context.Add(new SettingDefinition(SafePathSettings.MySetting1));
    }
}
