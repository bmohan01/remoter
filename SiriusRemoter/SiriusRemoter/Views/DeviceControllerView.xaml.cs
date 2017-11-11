namespace SiriusRemoter.Views
{
    using SiriusRemoter.Models;
    using SiriusRemoter.ViewModels;
    using System;
    using System.ComponentModel;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Input;

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

        private void FilterText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (navItems.ItemsSource != null)
            {
                //setup filtering
                ViewModel.ExecuteFilter();
            }
        }
    }
}
