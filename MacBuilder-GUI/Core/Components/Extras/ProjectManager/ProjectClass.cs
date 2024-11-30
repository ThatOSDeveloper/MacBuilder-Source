using MacBuilder.Core.Global;
using MacBuilder.Core.Logger;
using MacBuilder_GUI.Core.Components.Assets.Dialogs;
using MacBuilder_GUI.Core.Components.Extras.Updater;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MacBuilder_GUI.Core.Components.Extras.ProjectManager
{
    public class ProjectClass
    {
        private static readonly string projectDirectory = Path.Combine(Global.DownloadPath, "Projects");

        public async static void GenerateProject(string projectName, string version)
        {
            if (CheckForExistingProject(projectName, version))
            {
                Logger.Log($"Project {projectName} with version {version} already exists.", Logger.LogLevel.Warning);
                return;
            }

            Logger.Log($"Generating project {projectName} with version {version}.", Logger.LogLevel.Info);

            string projectPath = Path.Combine(projectDirectory, $"{projectName}_{version}");
            Directory.CreateDirectory(projectPath);
            Logger.Log($"Project {projectName} created at {projectPath}.", Logger.LogLevel.Info);

            if (await UpdaterClass.IsUpdateAvailableAsync())
            {
                Logger.Log("An update is available for this project.", Logger.LogLevel.Info);
            }
        }

        public static bool CheckForExistingProject(string projectName, string version)
        {
            string projectPath = Path.Combine(projectDirectory, $"{projectName}_{version}");
            bool exists = Directory.Exists(projectPath);
            Logger.Log(exists
                ? $"Project {projectName} with version {version} already exists."
                : $"Project {projectName} with version {version} does not exist.", Logger.LogLevel.Info);
            return exists;
        }

        public static void LoadProject(string projectName, string version)
        {
            string projectPath = Path.Combine(projectDirectory, $"{projectName}_{version}");
            if (!Directory.Exists(projectPath))
            {
                Logger.Log($"Project {projectName} with version {version} does not exist.", Logger.LogLevel.Error);
                return;
            }

            Logger.Log($"Loading project {projectName} with version {version}.", Logger.LogLevel.Info);
            // TODO
        }

        public static void DeleteProject(string projectName, string version)
        {
            string projectPath = Path.Combine(projectDirectory, $"{projectName}_{version}");
            if (Directory.Exists(projectPath))
            {
                Directory.Delete(projectPath, true);
                Logger.Log($"Deleted project {projectName} with version {version}.", Logger.LogLevel.Warning);
            }
            else
            {
                Logger.Log($"Project {projectName} with version {version} does not exist.", Logger.LogLevel.Error);
            }
        }

        public static void DeleteAllProjects()
        {
            if (Directory.Exists(projectDirectory))
            {
                Directory.Delete(projectDirectory, true);
                Logger.Log("Deleted all projects.", Logger.LogLevel.Warning);
                Directory.CreateDirectory(projectDirectory);
            }
            else
            {
                Logger.Log("No projects directory found to delete.", Logger.LogLevel.Warning);
            }
        }

        public async Task<bool> IsProjectOutOfDate(string projectName, string version)
        {
            if (await UpdaterClass.IsUpdateAvailableAsync())
            {
                Logger.Log($"Project {projectName} version {version} is outdated.", Logger.LogLevel.Error);
                bool shouldDelete = await DialogClass.ShowYesNoDialogAsync("Outdated Project", $"Project {projectName} version {version} is outdated. Do you want to delete it?");
                if (shouldDelete)
                {
                    DeleteProject(projectName, version);
                }
                return true;
            }
            return false;
        }
    }
}
