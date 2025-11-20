using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace BroccoliBunnyStudios.Extensions
{
    public static class ExtensionsClick
    {
        public static void OnClick(this Button btn, UnityAction func)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                using (btn.Using())
                {
                    func();
                }
            });
        }

        public static void OnClickAsync(this Button btn, Func<UniTask> func)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                Func().Forget();
            });

            async UniTask Func()
            {
                using (btn.Using())
                {
                    await func();
                }
            }
        }

        public static void OnClickCoroutine(this Button btn, Func<IEnumerator> ieFunc)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => { btn.StartCoroutine(new ButtonWrapperForIEnumerator(btn, ieFunc())); });
        }

        private static ButtonWrapper Using(this Button btn)
        {
            return new ButtonWrapper(btn);
        }

        private sealed class ButtonWrapperForIEnumerator : CustomYieldInstruction
        {
            private readonly Button _btn;
            private readonly IEnumerator _runner;

            public ButtonWrapperForIEnumerator(Button btn, IEnumerator runner)
            {
                btn.enabled = false;
                this._btn = btn;
                this._runner = runner;
            }

            public override bool keepWaiting
            {
                get
                {
                    var isWaiting = this._runner.MoveNext();
                    if (!isWaiting)
                    {
                        this._btn.enabled = true;
                    }

                    return isWaiting;
                }
            }
        }

        private sealed class ButtonWrapper : IDisposable
        {
            private Button _btn;

            public ButtonWrapper(Button btn)
            {
                btn.enabled = false;
                this._btn = btn;

                switch (this._btn.transition)
                {
                    case Selectable.Transition.ColorTint:
                        this._btn.targetGraphic.color = this._btn.colors.pressedColor;
                        break;
                    case Selectable.Transition.SpriteSwap:
                        this._btn.image.overrideSprite = this._btn.spriteState.pressedSprite;
                        break;
                }
            }

            public void Dispose()
            {
                if (this._btn != null)
                {
                    if (this._btn.gameObject.activeInHierarchy)
                    {
                        this._btn.StartCoroutine(DelayedEnable(this._btn));
                    }
                    else
                    {
                        Enable(this._btn);
                    }

                    this._btn = null;
                }
            }

            private static IEnumerator DelayedEnable(Button button)
            {
                if (button.gameObject.activeInHierarchy)
                {
                    var colors = button.colors;
                    yield return new WaitForSecondsRealtime(colors.fadeDuration);
                    button.enabled = true;
                    switch (button.transition)
                    {
                        case Selectable.Transition.ColorTint:
                            button.targetGraphic.color = colors.normalColor;
                            break;
                        case Selectable.Transition.SpriteSwap:
                            button.image.overrideSprite = null;
                            break;
                    }
                }
                else
                {
                    Enable(button);
                }
            }

            private static void Enable(Button button)
            {
                var colors = button.colors;
                button.enabled = true;
                switch (button.transition)
                {
                    case Selectable.Transition.ColorTint:
                        button.targetGraphic.color = colors.normalColor;
                        break;
                    case Selectable.Transition.SpriteSwap:
                        button.image.overrideSprite = null;
                        break;
                }
            }
        }
    }
}