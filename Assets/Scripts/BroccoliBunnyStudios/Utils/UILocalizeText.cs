using BroccoliBunnyStudios.Managers;
using TMPro;
using UnityEngine;

namespace BroccoliBunnyStudios.Utils
{
    public class UILocalizeText : MonoBehaviour
    {
        [field: SerializeField]
        public string Key { get; set; }

        private void Start()
        {
            this.OnLocalizationChanged();

            LocalizationManager.Instance.OnLocalizationChanged += this.OnLocalizationChanged;
        }

        private void OnDestroy()
        {
            LocalizationManager.Instance.OnLocalizationChanged -= this.OnLocalizationChanged;
        }

        private void OnLocalizationChanged()
        {
            var tmp = this.GetComponent<TextMeshProUGUI>();
            if (tmp)
            {
                tmp.text = LocalizationManager.Instance[this.Key];
                tmp.SetAllDirty();
                tmp.ForceMeshUpdate();
            }
        }
    }
}