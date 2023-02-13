using Rubrics.Handlers.Items;
using Rubrics.Worker.Main.Settings;

namespace Rubrics.Worker.Main.ConfigurationProvider
{
    public class MigrationSettingsProvider : IProvideMigrationSetting
    {
        private readonly AppSettings _appsettings;

        public MigrationSettingsProvider(AppSettings appSettings)
        {
            _appsettings = appSettings;
        }

        //public bool IsMigrationEnabled => bool.Parse(_appsettings.MigrationEnabled);
        public bool IsMigrationEnabled { get; }
    }
}
