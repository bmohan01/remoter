using SiriusRemoter.ViewModels;
using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace SiriusRemoter.Views
{
    /// <summary>
    /// Interaction logic for DeviceSetupView.xaml
    /// </summary>
    public partial class DeviceControllerView : UserControl
    {
        public DeviceControllerView()
        {
            InitializeComponent();
        }

        public DeviceControllerViewModel ViewModel
        {
            get
            {
                return DataContext as DeviceControllerViewModel;
            }
        }

        public void NavigationItemClickHandler(object sender, MouseButtonEventArgs e)
        {
            try
            {
                ViewModel.PlayerController.ActivateNavigationItem();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
