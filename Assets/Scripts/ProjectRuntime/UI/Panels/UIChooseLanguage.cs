using System.Collections.Generic;
using BroccoliBunnyStudios.Extensions;
using BroccoliBunnyStudios.Managers;
using BroccoliBunnyStudios.Sound;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectRuntime.UI.Panels
{
    public class UIChooseLanguage : MonoBehaviour
    {
        [field: SerializeField, Header("Scene References")]
        private Button Button { get; set; }

        [field: SerializeField]
        private TextMeshProUGUI ChooseLanguageTMP { get; set; }

        [field: SerializeField, Header("Button Sprites")]
        private Sprite ActiveButtonSprite { get; set; }

        [field: SerializeField]
        private Sprite InactiveButtonSprite { get; set; }

        [field: SerializeField, Header("Sfxes")]
        private AudioPlaybackInfo ButtonClickSfx { get; set; }

        private EnumLanguage _enumLanguage;
        private readonly static Dictionary<EnumLanguage, string> LanguageNames = new()
        {
            { EnumLanguage.EN, "English" },
            { EnumLanguage.ZH_HANS, "简体中文" },
            { EnumLanguage.JP, "日本語" },
        };

        private void Awake()
        {
            this.Button.OnClick(() => this.OnButtonClick().Forget());
            LocalizationManager.Instance.OnLocalizationChanged += this.OnLocalizationChanged;
        }

        private void OnDestroy()
        {
            LocalizationManager.Instance.OnLocalizationChanged -= this.OnLocalizationChanged;
        }

        public void Init(EnumLanguage enumLanguage)
        {
            this._enumLanguage = enumLanguage;
            this.ChooseLanguageTMP.text = LanguageNames[this._enumLanguage];
            this.OnLocalizationChanged();
        }

        private async UniTaskVoid OnButtonClick()
        {
            SoundManager.Instance.PlayAudioPlaybackInfoAsync(this.ButtonClickSfx, false, Vector3.zero).Forget();

            LocalizationManager.Instance.SetLanguage(this._enumLanguage);

            await UniTask.CompletedTask;
        }

        private void OnLocalizationChanged()
        {
            this.Button.image.sprite = this._enumLanguage == SaveManager.Instance.DisplayLangauge
                ? this.ActiveButtonSprite
                : this.InactiveButtonSprite;
        }
    }
}