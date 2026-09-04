using System.Runtime.CompilerServices;
using QuestPDF.Infrastructure;

namespace DairyManagementSystem.Tests
{
    internal static class TestStartup
    {
        [ModuleInitializer]
        internal static void Init()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }
    }
}
