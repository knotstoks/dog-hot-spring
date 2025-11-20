using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace BroccoliBunnyStudios.Panel
{
    public class UISceneTransitionMask : MonoBehaviour
    {
        [field: SerializeField]
        public float StartScale { get; private set; } = 20f;

        [field: SerializeField]
        public float EndScale { get; private set; } = 0.01f;

        [field: SerializeField]
        public Image MaskImage { get; private set; }

        public async UniTask FadeToBlackAsync(float duration = 1f)
        {
            if (duration <= 0f)
            {
                this.gameObject.SetActive(true);
                this.MaskImage.transform.localScale = new Vector3(this.EndScale, this.EndScale, this.EndScale);
                return;
            }

            this.gameObject.SetActive(true);
            this.MaskImage.transform.localScale = new Vector3(this.StartScale, this.StartScale, this.StartScale);
            await this.MaskImage.transform.DOScale(new Vector3(this.EndScale, this.EndScale, this.EndScale), duration)
                .SetEase(Ease.InQuart)
                .SetUpdate(true)
                .ToUniTask();
            this.MaskImage.transform.localScale = new Vector3(this.EndScale, this.EndScale, this.EndScale);
        }

        public async UniTask FadeFromBlack(float duration = 1f)
        {
            if (duration <= 0f)
            {
                this.MaskImage.transform.localScale = new Vector3(this.StartScale, this.StartScale, this.StartScale);
                this.gameObject.SetActive(false);
                return;
            }

            this.MaskImage.transform.localScale = new Vector3(this.EndScale, this.EndScale, this.EndScale);
            await this.MaskImage.transform.DOScale(new Vector3(this.StartScale, this.StartScale, this.StartScale), duration)
                .SetEase(Ease.InQuart)
                .SetUpdate(true)
                .ToUniTask();
            this.MaskImage.transform.localScale = new Vector3(this.StartScale, this.StartScale, this.StartScale);
            this.gameObject.SetActive(false);
        }
    }
}