using MahApps.Metro.Controls;
using System.Windows;

namespace SiriusRemoter.Views
{
    /// <summary>
    /// Interaction logic for OptionsView.xaml
    /// </summary>
    public partial class OptionsView : MetroWindow
    {
        public OptionsView()
        {
            InitializeComponent();
        }

        public OptionsView(Window parent)
            : this()
        {
            this.Owner = parent;
        }
    }
}
