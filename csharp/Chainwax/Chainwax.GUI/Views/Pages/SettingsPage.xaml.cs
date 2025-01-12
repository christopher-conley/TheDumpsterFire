using Chainwax.GUI.ViewModels.Pages;
using Wpf.Ui.Controls;

namespace Chainwax.GUI.Views.Pages {
    public partial class SettingsPage : INavigableView<SettingsViewModel> {
        public SettingsViewModel ViewModel { get; }

        public SettingsPage(SettingsViewModel viewModel) {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }
    }
}
