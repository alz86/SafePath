using SafePath.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace SafePath.Permissions;

public class SafePathPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(SafePathPermissions.GroupName);
        //Define your own permissions here. Example:
        //myGroup.AddPermission(SafePathPermissions.MyPermission1, L("Permission:MyPermission1"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<SafePathResource>(name);
    }
}
