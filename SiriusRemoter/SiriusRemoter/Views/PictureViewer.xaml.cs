using System.Windows.Controls;
using SiriusRemoter.ViewModels;

namespace SiriusRemoter.Views
{
    /// <summary>
    /// Interaction logic for PictureViewer.xaml
    /// </summary>
    public partial class PictureViewer : UserControl
    {
        public PictureViewer()
        {
            InitializeComponent();

            //DataContext = new PictureViewerViewModel();
        }
    }
}
