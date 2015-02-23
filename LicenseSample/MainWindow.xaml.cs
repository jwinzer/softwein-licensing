using System;
using System.Windows;

namespace LicenseSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const uint PRODUCT_ID = 0x30;
        private const uint MIN_KEYPADS = 5;

        public int KeypadLicense { get; private set; }

        public MainWindow()
        {
            InitializeComponent();

            UpdateLicense(Properties.Settings.Default.License);
        }

        private bool UpdateLicense(string licenseKey)
        {
            // Default license: expires 2019/12/31, 5 keypads
            Softwein.Secure.License license =
                new Softwein.Secure.License(PRODUCT_ID, new DateTime(2019, 12, 31), MIN_KEYPADS);

            if (licenseKey.Length > 0)
            {
                bool ok = license.WriteKey(licenseKey);
                // Use default license if no valid license has been applied yet.
                if (!ok && labelLicense.Content != null)
                {
                    MessageBox.Show("The license key is invalid.", 
                        Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return false;
                }
            }

            if (license.ExpirationDate < DateTime.Now)
            {
                MessageBox.Show("Sorry, your license is expired.\nYou can still use the program with a restricted number of keypads.",
                    Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                KeypadLicense = (int)MIN_KEYPADS;
            }
            else
            {
                KeypadLicense = (int)license.Value;
            }

            labelLicense.Content =
                "License:" +
                "\nSerial Number: " + license.SerialNumber + 
                "\nExpiration Date: " +
                license.ExpirationDate.ToShortDateString() + 
                "\nDongle: " + license.Dongle +
                "\nKeypads: " +  KeypadLicense.ToString();

            // save license key
            Properties.Settings.Default.License = license.ReadKey();
            return true;
        }

        private void textBoxLicenseKey_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Return)
            {
                e.Handled = true;
                if (UpdateLicense(textBoxLicenseKey.Text))
                    textBoxLicenseKey.Text = string.Empty;
            }
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
        }
    }
}
