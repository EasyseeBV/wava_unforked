using UnityEngine;
using System.Collections;
using VoxelBusters.CoreLibrary.NativePlugins;
using VoxelBusters.CoreLibrary;

namespace VoxelBusters.ScreenRecorderKit.VideoRecorderCore.Internal
{    
    internal class NativeRecorderStateChangeListener : IRecorderStateChangeListener
    {
        #region Delegates

        internal delegate void OnNewStateDelegate();

        #endregion

        #region Public callbacks

        public OnNewStateDelegate onPrepare;
        public OnNewStateDelegate onRecord;
        public OnNewStateDelegate onPause;
        public OnNewStateDelegate onStop;
        public OnNewStateDelegate onInvalid;

        #endregion

        public void OnPrepare()
        {
            onPrepare?.Invoke();
        }

        public void OnRecord()
        {
            onRecord?.Invoke();
        }

        public void OnPause()
        {
            onPause?.Invoke();
        }

        public void OnStop()
        {
            onStop?.Invoke();
        }

        public void OnInvalid()
        {
            onInvalid?.Invoke();
        }
    }
}