using UnityEngine;

namespace BroccoliBunnyStudios.Panel
{
    public class BasePanel : MonoBehaviour
    {
        public delegate void Handler(BasePanel dlg);
        public event Handler EvtCloseOrdered;

        /// <summary>
        /// Any logic should be done before base.Close is called
        /// </summary>
        public virtual void Close()
        {
            this.EvtCloseOrdered?.Invoke(this);

            PanelManager.Instance.OnClose(this);
        }
    }
}