namespace MainCore.Services
{
    public sealed record DefaultTemplatePaths(
        string AccountSettingsPath,
        string VillageSettingsPath,
        string BuildingListPath);

    public interface IDefaultTemplatePathStore
    {
        DefaultTemplatePaths Get();

        void SetAccountSettingsPath(string path);

        void SetVillageSettingsPath(string path);

        void SetBuildingListPath(string path);

        void ClearAccountSettingsPath();

        void ClearVillageSettingsPath();

        void ClearBuildingListPath();
    }
}
