using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Windows.Forms;

namespace POBR {
    public partial class MainWindow: Window {
        public Picture picture;

        public MainWindow() {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            var dialog = new System.Windows.Forms.OpenFileDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();

            if( result == System.Windows.Forms.DialogResult.OK ) {
                picture = new Picture(dialog.FileName);
                this.Image.Source = picture.Image;

                this.Button.Visibility = Visibility.Collapsed;
                this.Image.Visibility = Visibility.Visible;
            }
        }
    }
}
