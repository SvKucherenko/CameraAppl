using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using EDSDKLib;


namespace CameraDLL
{
     
    public class CameraDLL
    {
        public event EventHandler<EventArgs> PictureTaked;
        public event EventHandler<LivePictureUpdatedEventArgs> LivePictureUpdated;
        SDKHandler CameraHandler;
     List<int> AvList;
          List<int> TvList;
        List<int> ISOList;
        List<Camera> CamList;
        Bitmap Evf_Bmp;
        
        
        string SavePathText = "";
        protected virtual void OnPictureTaked(EventArgs e)
        {
            EventHandler<EventArgs> handler = PictureTaked;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        protected virtual void OnLivePictureUpdated(LivePictureUpdatedEventArgs e)
        {
            EventHandler<LivePictureUpdatedEventArgs> handler = LivePictureUpdated;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        public bool IsLiveViewOn()
        {
            if (CameraHandler == null) return false;
            return (CameraHandler.IsLiveViewOn);
        }
        public string ImageSaveDirectory
        {
            get   
            {
                if (CameraHandler != null)
                    return (CameraHandler.ImageSaveDirectory);
                else return null;
            }
            set 
            {
                if (CameraHandler != null)
                {
                    CameraHandler.ImageSaveDirectory = value;
                    if (!Directory.Exists(SavePathText)) Directory.CreateDirectory(SavePathText);
                }
            }
        }
        public bool Start()
        {
            try
            {
                CameraHandler = new SDKHandler();
                

              
                CameraHandler.PictureTaked+=new SDKHandler.PictureTakedHandler(CameraHandler_PictureTaked);
                CameraHandler.LiveViewUpdated += new SDKHandler.StreamUpdate(CameraHandler_LiveViewUpdated);
                SavePathText = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "RemotePhoto");
                if (!Directory.Exists(SavePathText)) Directory.CreateDirectory(SavePathText);
                CamList = CameraHandler.GetCameraList();
                if (CamList.Count != 0)
                {
                    CameraHandler.OpenSession(CamList[0]);
                    CameraHandler.ImageSaveDirectory = SavePathText;
                    CameraHandler.SetSetting(EDSDK.PropID_SaveTo, (uint)EDSDK.EdsSaveTo.Host);
                    CameraHandler.SetSetting(EDSDK.PropID_ImageQuality, (uint)EDSDK.ImageQuality.EdsImageQuality_LR);
                    CameraHandler.SetCapacity();
                }
                else return false; // no camera
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        void CameraHandler_LiveViewUpdated(Stream img)
        {
            LivePictureUpdatedEventArgs e= new LivePictureUpdatedEventArgs();
            e.img=img;
            OnLivePictureUpdated(e);
        }

        void CameraHandler_PictureTaked()
        {
            lock (CameraHandler)
            {
                OnPictureTaked(null);
            }
        }

        public List<string> GetAvList()
        {
            if (CameraHandler == null) return null;
            List<string> ret = new List<string>();
            foreach (int Av in CameraHandler.GetSettingsList((uint)EDSDK.PropID_Av))
            {
                ret.Add(CameraValues.AV((uint)Av));
            }
            return ret;
        }

        public List<string> GetTvList()
        {
            if (CameraHandler == null) return null;
            List<string> ret = new List<string>();
            foreach (int Tv in CameraHandler.GetSettingsList((uint)EDSDK.PropID_Tv))
            {
                ret.Add(CameraValues.TV((uint)Tv));
            }
            return ret;
        }
        public void SetAV(string Av)
        {
            if (CameraHandler != null)
            CameraHandler.SetSetting(EDSDK.PropID_Av, CameraValues.AV((string)Av));
        }
        public void SetTV(string Tv)
        {
            if (CameraHandler != null)
            CameraHandler.SetSetting(EDSDK.PropID_Tv, CameraValues.TV((string)Tv));
        }
        
        public void Stop()
        {
            if (CameraHandler != null)
            CameraHandler.Dispose();
        }
        public void StartLiveView()
        {
            if (CameraHandler != null)
                CameraHandler.StartLiveView();
        }
        public void StopLiveView()
        {
            if (CameraHandler != null)
                CameraHandler.StopLiveView();
        }
        public void TakePhoto()
        {
            if (CameraHandler != null)
                CameraHandler.TakePhoto();
        }

    }
    public class LivePictureUpdatedEventArgs : EventArgs
    {
        public Stream img { get; set; }
        
    }
}
