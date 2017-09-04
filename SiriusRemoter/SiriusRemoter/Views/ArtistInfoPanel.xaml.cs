using SiriusRemoter.ViewModels;
using System;
using System.Windows.Controls;

namespace SiriusRemoter.Views
{
    /// <summary>
    /// Interaction logic for ArtistInfoPanel.xaml
    /// </summary>
    public partial class ArtistInfoPanel : UserControl
    {
        public ArtistInfoPanel()
        {
            InitializeComponent();
        }

        public ArtistInfoViewModel ViewModel
        {
            get
            {
                return (DataContext as ArtistInfoViewModel);
            }
        }

        private void Previous_Image_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                ViewModel.SetNextImage(NextImageDirection.Previous);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void Next_Image_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                ViewModel.SetNextImage(NextImageDirection.Next);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void IsVisibleChangedHandler(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            //if turning visible
            if (((bool)e.NewValue))
            {
                ViewModel.Initialize();
            }
        }
    }
}
