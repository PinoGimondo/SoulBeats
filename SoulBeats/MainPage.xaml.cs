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
        public MainPage()
        {
            this.InitializeComponent();
            sm = new SoundManager();
            sm.init();
            lFreq.Value = sm.l_freq * 10;
            rFreq.Value = sm.r_freq * 10;
            slVolume.Value = sm.amplitude * 100;
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
    }
}
