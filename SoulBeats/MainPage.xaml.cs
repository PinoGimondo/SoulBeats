using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace SoulBeats
{
    public sealed partial class MainPage : Page
    {
        private SoundManager sm;
        private float diff=0;
        private bool changingDiff = false;
        private Button[] fbut= new Button[10];
        private int minFreq = 30;
        private int maxFreq = 2000;

        public MainPage()
        {
            this.InitializeComponent();
            sm = new SoundManager();
            sm.init();
            lFreq.Value = sm.l_freq * 10;
            rFreq.Value = sm.r_freq * 10;
            slVolume.Value = sm.amplitude * 100;

            fbut[0] = f0;
            fbut[1] = f1;
            fbut[2] = f2;
            fbut[3] = f3;
            fbut[4] = f4;
            fbut[5] = f5;
            fbut[6] = f6;
            fbut[7] = f7;
            fbut[8] = f8;
            fbut[9] = f9;

            updateDisplay();
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (sm.playing())
            {
                sm.stop();
            }
            else {
                sm.start();
            }
            updateDisplay();
        }

        private void lFreq_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (sm != null && !changingDiff) {
                sm.l_freq = (float)lFreq.Value / 10;
                if (flock.IsChecked==true) {
                   rFreq.Value= (sm.l_freq + diff)*10;
                }

                updateDisplay();
            }
        }

        private void rFreq_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (sm != null )
            {
                sm.r_freq = (float)rFreq.Value / 10;
                updateDisplay();
            }
        }
        private void slVolume_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (sm != null)
            {
                sm.amplitude = (float)slVolume.Value / 100;
                updateDisplay();
            }
        }

        private void updateDisplay() {
            tbLeft.Text = sm.l_freq.ToString();
            tbRight.Text = sm.r_freq.ToString();
            diff = sm.r_freq - sm.l_freq;
            tbDiff.Text = Math.Abs(diff).ToString("#.##");
            if (sm.playing())
            {
                btnStart.Content = "STOP";
            }
            else {
                btnStart.Content = "Generate Audio";
            }

            if (fbut[0] != null) {

                for (int i = 0; i < 10; i++)
                {
                    fbut[i].Visibility = Visibility.Collapsed;
                }

                float ottava = Math.Abs(diff);

                if (ottava > 2)
                {
                    int bott = 0;
                    while (true)
                    {
                        ottava = ottava * 2;
                        if (ottava > minFreq && ottava < maxFreq)
                        {
                            Button b = fbut[bott++];
                            b.Visibility = Visibility.Visible;
                            b.Content = ottava.ToString();
                        }
                        if (ottava > maxFreq || bott >= 9)
                        {
                            break;
                        }
                    }
                }
            }
        }

        private void tbDiff_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!changingDiff)
            {
                double val = 0;
                try
                {
                    val = double.Parse(tbDiff.Text) ;
                    if (val > 50) val = 50;
                    if (val < 0) val = 0;
                }
                catch (Exception)
                {

                }
                changingDiff = true;
                rFreq.Value = ((double)lFreq.Value) + val * 10;
                changingDiff = false;
            }

        }

        private void f_Click(object sender, RoutedEventArgs e)
        {
            Button b = (Button)sender;
            lFreq.Value = (float.Parse(b.Content.ToString())) * 10;
        }
    }
}
