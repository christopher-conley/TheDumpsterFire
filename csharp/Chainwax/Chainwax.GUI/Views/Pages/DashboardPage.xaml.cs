using Chainwax.GUI.ViewModels.Pages;
using Wpf.Ui.Controls;

namespace Chainwax.GUI.Views.Pages {
    public partial class DashboardPage : INavigableView<DashboardViewModel> {
        public DashboardViewModel ViewModel { get; }

        public DashboardPage(DashboardViewModel viewModel) {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }
    }
}
