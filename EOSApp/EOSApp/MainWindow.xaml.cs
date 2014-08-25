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
using System.IO;
using System.Threading;
using System.Windows.Threading;
using System.Windows.Forms;

namespace EOSApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        CameraDLL.CameraDLL cam = new CameraDLL.CameraDLL();
        public MainWindow()
        {
            InitializeComponent();
        }
        bool bDisconnecting = false;
        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            if (btnConnect.Content.ToString() == "Connect")
            {
                txtSavePatchTextBox.Text = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "RemotePhoto").ToString();
                if (cam.Start())
                {
                    btnConnect.Content = "Disconnect";
                    List<string> l = cam.GetAvList();
                    foreach (string c in l)
                    {
                        cboAppertures.Items.Add(c);
                    }
                    l = cam.GetTvList();
                    foreach (string c in l)
                    {
                        cboShutterTimes.Items.Add(c);
                    }
                    cam.LivePictureUpdated += new EventHandler<CameraDLL.LivePictureUpdatedEventArgs>(cam_LivePictureUpdated);
                    cam.PictureTaked += new EventHandler<EventArgs>(cam_PictureTaked);
                }
            }
            else
            {
                bDisconnecting = true;
                cboAppertures.Items.Clear();
                cboShutterTimes.Items.Clear();
                cam.Stop();
               
                btnConnect.Content = "Connect";
                bDisconnecting = false;
            }
        }

        void cam_PictureTaked(object sender, EventArgs e)
        {
            System.Windows.MessageBox.Show("Picture taked");
        }
        private  void SetBitmap(BitmapImage b)
        {
            imgPicture.Source = b;
        }

        void cam_LivePictureUpdated(object sender, CameraDLL.LivePictureUpdatedEventArgs e)
        {
            using (var stream = e.img)
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = stream;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();

                Thread t = new Thread(new ThreadStart(
        delegate
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action<BitmapImage>(SetBitmap), bitmap);
        }
        ));
                t.Start();
                
            }
        }

        private void cboAppertures_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!bDisconnecting)
            cam.SetAV(cboAppertures.SelectedItem.ToString());
        }

        private void cboShutterTimes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!bDisconnecting)
            cam.SetTV(cboShutterTimes.SelectedItem.ToString());
        }

        private void btnLiveView_Click(object sender, RoutedEventArgs e)
        {
            if (!cam.IsLiveViewOn())
            { btnLiveView.Content = "Stop LV";
            cam.StartLiveView();
            }
            else
            { btnLiveView.Content = "Start LV";
            cam.StopLiveView();
            }
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
               System.Windows.Forms.FolderBrowserDialog SaveFolderBrowser= new System.Windows.Forms.FolderBrowserDialog() ;
               if (SaveFolderBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
               {
                   txtSavePatchTextBox.Text = SaveFolderBrowser.SelectedPath;
                   cam.ImageSaveDirectory = SaveFolderBrowser.SelectedPath;
               }
        }

        private void btnTakePicture_Click(object sender, RoutedEventArgs e)
        {
            cam.TakePhoto();
        }
    }
}
