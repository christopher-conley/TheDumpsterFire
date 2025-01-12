using Chainwax.ShellExtension.Helpers;
using SharpShell.Attributes;
using SharpShell.SharpContextMenu;
using System;
using System.Linq;
using System.Resources;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Reflection;

namespace Chainwax.ShellExtension {



    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.None)]
    [COMServerAssociation(AssociationType.AllFilesAndFolders)]
    [COMServerAssociation(AssociationType.Directory)]
    [COMServerAssociation(AssociationType.DirectoryBackground)]
    [Guid("15EF9399-6875-4825-9F75-33F879BB059B")]
    public class Chainwax : SharpContextMenu {

        private string _folderPath;
        private string[]? _selectedItems;
        private int _selectedItemsCount;
        private string _actionItemText = String.Empty;
        private string _singleItemName;
        private Dictionary<string, string> _itemTypes = [];
        protected override bool CanShowMenu() {
            if ((null == FolderPath) || (null == SelectedItemPaths) || (SelectedItemPaths.Count() == 0)) {
                return false;
            }
            _folderPath = this.FolderPath;
            _selectedItems = this.SelectedItemPaths.ToArray();
            _selectedItemsCount = this._selectedItems.Length;

            if (_selectedItemsCount == 1) {
                if (File.Exists(_selectedItems[0])) {
                    _itemTypes.Add(_selectedItems[0], "file");
                }
                else if (Directory.Exists(_selectedItems[0])) {
                    _itemTypes.Add(_selectedItems[0], "directory");
                }
                else {
                    return false;
                }
                _singleItemName = Path.GetFileName(_selectedItems[0]);
                _actionItemText = $"Create symlink to {_singleItemName} here";
            }
            else {
                foreach (string item in _selectedItems) {
                    if (File.Exists(item)) {
                        _itemTypes.Add(item, "file");
                    }
                    else if (Directory.Exists(item)) {
                        _itemTypes.Add(item, "directory");
                    }
                    else {
                        continue;
                    }

                    if (_itemTypes.Count() == 0) {
                        return false;
                    }
                }
                _actionItemText = $"Create symlinks to {_selectedItemsCount} items here";
            }
            return true;
        }

        protected override ContextMenuStrip CreateMenu() {
            var menu = new ContextMenuStrip();

            var mainItem = new ToolStripMenuItem {
                Text = "Chainwax",
                Image = new Bitmap(16, 16)
            };

            var actionItem = new ToolStripMenuItem {
                Text = _actionItemText,
            };

            using (var stream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream($"Chainwax.ShellExtension.Assets.chainwax_16x16.png"))
            using (new StreamReader(stream)) {
                mainItem.Image = new Bitmap(stream);
            }



            //actionItem.Click += (sender, e) => MessageBox.Show("Action performed!");

            //actionItem.Click += (sender, e) => Chainwax.Helpers.SymlinkHelper.CreateSymlink(sender, e);

            // working
            actionItem.Click += (sender, e) => ShellExSymlinkHelper.CreateSymlink(_itemTypes, _selectedItemsCount, FolderPath);
            mainItem.DropDownItems.Add(actionItem);

            mainItem.DropDownItems.Add(actionItem);
            menu.Items.Add(mainItem);

            //actionItem.PerformClick();

            return menu;
        }

        [ComRegisterFunction]
        public static void Register(Type t) {
            //RegisterContextMenu(t, @"*\shellex\ContextMenuHandlers\");
            RegisterContextMenu(t, @"*\shellex\DragDropHandlers\");
            //RegisterContextMenu(t, @"Directory\shellex\ContextMenuHandlers\");
            RegisterContextMenu(t, @"Directory\shellex\DragDropHandlers\");
            //RegisterContextMenu(t, @"Directory\Background\shellex\ContextMenuHandlers\");
        }

        private static void RegisterContextMenu(Type t, string basePath) {
            string keyPath = basePath + "Chainwax";
            using (var key = Microsoft.Win32.Registry.ClassesRoot.CreateSubKey(keyPath)) {
                if (key == null) {
                    Console.WriteLine($"Failed to create registry key: {keyPath}");
                }
                else {
                    key.SetValue(null, t.GUID.ToString("B"));
                    Console.WriteLine($"Registered Chainwax COM server as context menu handler at {keyPath}");
                }
            }
        }

        [ComUnregisterFunction]
        public static void Unregister(Type t) {
            //UnregisterContextMenu(@"*\shellex\ContextMenuHandlers\");
            UnregisterContextMenu(@"*\shellex\DragDropHandlers\");
            //UnregisterContextMenu(@"Directory\shellex\ContextMenuHandlers\");
            UnregisterContextMenu(@"Directory\shellex\DragDropHandlers\");
            //UnregisterContextMenu(@"Directory\Background\shellex\ContextMenuHandlers\");
            Console.WriteLine($"Unregistered Chainwax COM server {t.FullName} as context menu handler");
        }

        private static void UnregisterContextMenu(string basePath) {
            string keyPath = basePath + "Chainwax";
            try {
                Microsoft.Win32.Registry.ClassesRoot.DeleteSubKeyTree(keyPath, false);
            }
            catch (Exception ex) {
                Console.WriteLine($"Error unregistering COM server from {keyPath}: {ex.Message}");
            }
        }
    }
}
