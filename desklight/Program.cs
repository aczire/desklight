using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Drawing;
using TaskScheduler;
using System.Windows.Forms;

namespace desklight
{
    class Program
    {

        // Reference https://docs.microsoft.com/en-us/windows/configuration/windows-spotlight

        private const int MIN_SIZE_BYTES = 256000; // 250KB
        private const string SPOTLIGHT_DIR = "Packages\\Microsoft.Windows.ContentDeliveryManager_cw5n1h2txyewy\\LocalState\\Assets";

        static void Main(string[] args)
        {
            bool installMode = args.Contains("install");
            bool silentMode = args.Contains("silent");
            try
            {
                if (installMode)
                {
                    if (silentMode)
                    {
                        CreateScheduledTask(true);
                    }
                    else
                    {
                        const string text = "Auto update wallpaper with lockscreen image?";
                        const string caption = "Enable Autorun";
                        if (MessageBox.Show(text, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
                        {
                            CreateScheduledTask(false);
                        }
                    }
                }

                List<string> spotLightImages = extractSpotlightImages();

                if (spotLightImages.Count > 0)
                {
                    Random rand = new Random();
                    int spotIndex = rand.Next(spotLightImages.Count - 1);
                    Wallpaper.Set(Image.FromFile(spotLightImages[spotIndex]), Wallpaper.Style.Centered);
                }
            }
            catch (Exception ex)
            {
                if (silentMode)
                {
                    Console.WriteLine(ex.ToString());
                }
                else
                {
                    const string caption = "Failed to run desklight!!!";
                    MessageBox.Show(ex.Message.ToString(), caption, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                }
            }
        }

        private static void CreateScheduledTask(bool silent)
        {
            //create task service instance
            TaskScheduler.TaskScheduler taskService = new TaskScheduler.TaskScheduler();
            taskService.Connect();
            ITaskDefinition taskDefinition = taskService.NewTask(0);
            taskDefinition.Settings.Enabled = true;
            taskDefinition.RegistrationInfo.Author = "Desklight";
            taskDefinition.Principal.RunLevel = _TASK_RUNLEVEL.TASK_RUNLEVEL_LUA;
            taskDefinition.Settings.AllowDemandStart = true;
            taskDefinition.Settings.StartWhenAvailable = true;
            taskDefinition.Settings.DisallowStartIfOnBatteries = false;
            taskDefinition.Settings.Compatibility = _TASK_COMPATIBILITY.TASK_COMPATIBILITY_V2_4;

            //create trigger for task creation.
            ITriggerCollection _iTriggerCollection = taskDefinition.Triggers;
            ISessionStateChangeTrigger sessionStateChangeTrigger = (ISessionStateChangeTrigger)_iTriggerCollection.Create(_TASK_TRIGGER_TYPE2.TASK_TRIGGER_SESSION_STATE_CHANGE);
            sessionStateChangeTrigger.Id = "UnlockTrigger";
            sessionStateChangeTrigger.StateChange = _TASK_SESSION_STATE_CHANGE_TYPE.TASK_SESSION_UNLOCK;
            sessionStateChangeTrigger.UserId = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            sessionStateChangeTrigger.Enabled = true;

            ///get actions.
            IActionCollection actions = taskDefinition.Actions;
            _TASK_ACTION_TYPE actionType = _TASK_ACTION_TYPE.TASK_ACTION_EXEC;

            //create new action
            IAction action = actions.Create(actionType);
            IExecAction execAction = action as IExecAction;
            execAction.Path = getExecutable(silent);
            ITaskFolder rootFolder = taskService.GetFolder(@"\");

            //register task.
            const int TASK_CREATE_OR_UPDATE = 6;
            string taskName = "Desklight-" + Environment.UserName;
            rootFolder.RegisterTaskDefinition(taskName, taskDefinition, TASK_CREATE_OR_UPDATE, null, null, _TASK_LOGON_TYPE.TASK_LOGON_NONE, null);
        }

        private static string getExecutable(bool silent)
        {
            string destinationExecutable = null;
            try
            {
                string currentExecutable = System.Reflection.Assembly.GetEntryAssembly().Location.ToString();
                string applicationDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string desklightDir = Path.Combine(applicationDataDir, "Desklight");
                destinationExecutable = Path.Combine(desklightDir, "desklight.exe");
                Directory.CreateDirectory(desklightDir);
                File.Copy(currentExecutable, destinationExecutable, true);
            }
            catch (Exception ex)
            {
                if (silent)
                {
                    Console.WriteLine(ex.ToString());
                }
                else
                {
                    const string caption = "Failed to install desklight!!!";
                    MessageBox.Show(ex.Message.ToString(), caption, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                }
            }

            return destinationExecutable;
        }

        private static List<string> extractSpotlightImages()
        {
            string appdataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string spotlightFolder = SPOTLIGHT_DIR;

            string spotlightFolderPath = Path.Combine(appdataPath, spotlightFolder);

            List<String> spotLightImages = new List<string>();

            foreach (string spotfile in Directory.GetFiles(spotlightFolderPath))
            {
                if (new FileInfo(spotfile).Length > MIN_SIZE_BYTES)
                {
                    // Select only landscape mode images. Not the portrait one.
                    if (Image.FromFile(spotfile).Width > Image.FromFile(spotfile).Height)
                    {
                        spotLightImages.Add(spotfile);
                    }
                }
            }

            return spotLightImages;
        }
    }
}
