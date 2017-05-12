// http://stackoverflow.com/a/1601670/6247478
// http://stackoverflow.com/a/9414495/6247478

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Shell32;

namespace _3launch
{
    class ShortcutReader
    {
        private static string GetShortcutTargetFile(string shortcutFilename)
        {
            string pathOnly = System.IO.Path.GetDirectoryName(shortcutFilename);
            string filenameOnly = System.IO.Path.GetFileName(shortcutFilename);

            Shell shell = new Shell();
            Folder folder = shell.NameSpace(pathOnly);
            FolderItem folderItem = folder.ParseName(filenameOnly);
            if (folderItem != null)
            {
                Shell32.ShellLinkObject link = (Shell32.ShellLinkObject) folderItem.GetLink;
                return link.Path;
            }

            return string.Empty;
        }

        public static Shortcut[] read()
        {
            var di = new DirectoryInfo("shortcuts");
            if (!di.Exists)
            {
                MessageBox.Show(
                    "Cannot find folder 'shortcuts'.\nPlease create this folder, put shortcut files (.lnk) in it, and run this app again.",
                    "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(-1);
                return null;
            }
            FileInfo[] fileInfos = di.GetFiles();
            List<Shortcut> results = new List<Shortcut>(fileInfos.Length);
            foreach (var fileInfo in fileInfos)
            {
                if ((fileInfo.Attributes & FileAttributes.Hidden) != 0)
                    continue;

                var shortcut = new Shortcut();
                shortcut.name = fileInfo.Name.Substring(0, fileInfo.Name.LastIndexOf(".")).ToUpper();
                shortcut.filePath = GetShortcutTargetFile(fileInfo.FullName);
                shortcut.fileName =
                    shortcut.filePath.Substring(shortcut.filePath.LastIndexOf(Path.DirectorySeparatorChar.ToString(),
                                                    StringComparison.Ordinal) + 1);
                if (Directory.Exists(shortcut.filePath))
                {
                    // Icon for directory
                    shortcut.icon = IconManager.GetFolderIcon(IconManager.IconSize.Large, IconManager.FolderType.Open);
                }
                else
                {
                    if (shortcut.filePath.ToLower().StartsWith("http"))
                    {
                        // Icon for URL
                        shortcut.icon = IconManager.FindIconForFilename(Util.GetSystemDefaultBrowser(), true);
                    }
                    else
                    {
                        // Icon for file
                        shortcut.icon = IconManager.FindIconForFilename(shortcut.filePath, true);
                    }
                }
                results.Add(shortcut);
            }

            return results.ToArray();
        }
    }
}