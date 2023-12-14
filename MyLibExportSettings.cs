using Playnite.SDK;
using Playnite.SDK.Data;
using MyLibExport.Services;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLibExport
{
    public class MyLibExportSettings : ObservableObject
    {
        public bool keepInSync = true;
        public bool showProgress = true;
        public bool showErrors = true;

        public bool KeepInSync { get => keepInSync; set => SetValue(ref keepInSync, value); }
        public bool ShowProgress { get => showProgress; set => SetValue(ref showProgress, value); }
        public bool ShowErrors { get => showErrors; set => SetValue(ref showErrors, value); }
        // Playnite serializes settings object to a JSON object and saves it as text file.
        // If you want to exclude some property from being saved then use `JsonDontSerialize` ignore attribute.
        //[DontSerialize]
        //public bool OptionThatWontBeSaved { get => optionThatWontBeSaved; set => SetValue(ref optionThatWontBeSaved, value); }
    }

    public class MyLibExportSettingsViewModel : ObservableObject, ISettings
    {
        private static ILogger logger = LogManager.GetLogger();
        private readonly MyLibExport plugin;
        private readonly IPlayniteAPI api;
        private MyLibExportSettings editingClone { get; set; }
        private MyLibExportSettings settings;
        private MyLibExportAccountClient clientApi;
        public bool IsUserLoggedIn
        {
            get
            {
                try
                {
                    clientApi.CheckAuthentication().GetAwaiter().GetResult();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }
        private void Login()
        {
            try
            {
                clientApi.Login();
                OnPropertyChanged(nameof(IsUserLoggedIn));
            }
            catch (Exception e) when (!Debugger.IsAttached)
            {
                logger.Error(e, "Failed to authenticate user.");
            }
        }
        public MyLibExportSettings Settings
        {
            get => settings;
            set
            {
                settings = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand<object> LoginCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                Login();
            });
        }

        public MyLibExportSettingsViewModel(MyLibExport plugin, IPlayniteAPI api)
        {
            // Injecting your plugin instance is required for Save/Load method because Playnite saves data to a location based on what plugin requested the operation.
            this.plugin = plugin;
            this.api = api;
            clientApi = new MyLibExportAccountClient(plugin, api);

            // Load saved settings.
            var savedSettings = plugin.LoadPluginSettings<MyLibExportSettings>();

            // LoadPluginSettings returns null if not saved data is available.
            if (savedSettings != null)
            {
                Settings = savedSettings;
            }
            else
            {
                Settings = new MyLibExportSettings();
            }
        }

        public void BeginEdit()
        {
            // Code executed when settings view is opened and user starts editing values.
            editingClone = Serialization.GetClone(Settings);
        }

        public void CancelEdit()
        {
            // Code executed when user decides to cancel any changes made since BeginEdit was called.
            // This method should revert any changes made to Option1 and Option2.
            Settings = editingClone;
        }

        public void EndEdit()
        {
            // Code executed when user decides to confirm changes made since BeginEdit was called.
            // This method should save settings made to Option1 and Option2.
            plugin.SavePluginSettings(Settings);
        }

        public bool VerifySettings(out List<string> errors)
        {
            // Code execute when user decides to confirm changes made since BeginEdit was called.
            // Executed before EndEdit is called and EndEdit is not called if false is returned.
            // List of errors is presented to user if verification fails.
            errors = new List<string>();
            return true;
        }
    }
}