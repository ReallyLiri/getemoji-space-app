namespace GetEmoji.Services.Space;

public interface IPermissionRequestService
{
    Task RequestPermissionsAsync(string clientId);
}