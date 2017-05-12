using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;


namespace _3launch
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<char> inputs;
        private Shortcut[] allShortcuts, filteredShortcuts;

        public MainWindow()
        {
            inputs = new List<char>();
            InitializeComponent();
            System.Windows.Input.InputMethod.SetIsInputMethodEnabled(this, false);
            InputMethod.Current.ImeState = InputMethodState.Off;

            allShortcuts = ShortcutReader.read();
            render();
            this.Hide();
            KeyInterceptor.mainWindow = this;
            KeyInterceptor.start();
            watchForShortcutsChange();
        }

        private void render()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var key in inputs)
            {
                sb.Append(key.ToString());
            }
            InputKeysBox.Text = sb.ToString();
            filteredShortcuts = allShortcuts.Where(item => item.name.StartsWith(InputKeysBox.Text)).ToArray();
            list.ItemsSource = filteredShortcuts;
        }

        private void OnKeyInput(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Back)
            {
                if (inputs.Count > 0)
                {
                    inputs.RemoveAt(inputs.Count - 1);
                    render();
                    return;
                }
            }
            if (e.Key == Key.Escape)
            {
                clearAndHide();
                return;
            }
            var keyCode = Util.keyToChar(e.Key);
            if (!Util.isKeyCodeValid(keyCode))
                return;
            inputs.Add(keyCode);
            render();
            if (filteredShortcuts.Length > 0)
            {
                var shortcuts = allShortcuts.Where(item => item.name == InputKeysBox.Text).ToArray();
                if (shortcuts.Length > 0)
                    launchShortcut(shortcuts[0]);
            }
        }

        private void launchShortcut(Shortcut shortcut)
        {
            try
            {
                Process.Start(shortcut.filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to start " + filteredShortcuts[0].filePath + "\n\n" + ex.ToString(),
                    "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            clearAndHide();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        public void clearAndHide()
        {
            inputs.Clear();
            render();
            this.Hide();
        }

        private void watchForShortcutsChange()
        {
            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = "shortcuts";
            watcher.NotifyFilter = NotifyFilters.Attributes |
                                   NotifyFilters.CreationTime |
                                   NotifyFilters.DirectoryName |
                                   NotifyFilters.FileName |
                                   NotifyFilters.LastAccess |
                                   NotifyFilters.LastWrite |
                                   NotifyFilters.Security |
                                   NotifyFilters.Size;
            watcher.Filter = "*.*";
            watcher.Changed += new FileSystemEventHandler(onShortcutsChanged);
            watcher.Created += new FileSystemEventHandler(onShortcutsChanged);
            watcher.Deleted += new FileSystemEventHandler(onShortcutsChanged);
            watcher.Renamed += new RenamedEventHandler(onShortcutsChanged);
            watcher.EnableRaisingEvents = true;
        }

        private void list_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (list.SelectedValue == null) return;
            launchShortcut(list.SelectedValue as Shortcut);
            list.UnselectAll();
        }

        private void onShortcutsChanged(object source, FileSystemEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                allShortcuts = ShortcutReader.read();
                render();
            }));
        }
    }
}