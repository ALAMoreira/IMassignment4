using System.Windows;

namespace secondModality
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private SecondMod _sm;
        public MainWindow()
        {
            //InitializeComponent();

            _sm = new SecondMod();
        
        }

        private void B1_OnClick(object sender, RoutedEventArgs e)
        {
            _sm.sendToFusion("RED");

        }

        private void B2_OnClick(object sender, RoutedEventArgs e)
        {
            _sm.sendToFusion("BLUE");

        }
        private void B3_OnClick(object sender, RoutedEventArgs e)
        {
            _sm.sendToFusion("YELLOW");

        }

        private void B4_OnClick(object sender, RoutedEventArgs e)
        {
            _sm.sendToFusion("SQUARE","YELLOW");

        }



        /*
        private void _sm_Recognized(object sender, SpeechEventArg e)
        {
            result.Text = e.Text;
            confidence.Text = e.Confidence+"";
            if (e.Final) result.FontWeight = FontWeights.Bold;
            else result.FontWeight = FontWeights.Normal;
        }

    */

    }
}
