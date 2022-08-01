using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using NaughtyAttributes;
using GraVRty.GameFlow;

namespace GraVRty.GameFlow
{
    [CreateAssetMenu(menuName = "GraVRty/Pauser", fileName = "newPauser.asset")]
    public class Pauser : Loadable
    {
        [SerializeField] float m_PauseTime, m_UnpauseTime;
        [SerializeField] Ease m_PauseEase, m_UnpauseEase;

        [Foldout("Fog")]
        [SerializeField] AnimationCurve m_FogDensityOverFX;
        [Foldout("Fog")]
        [SerializeField] Color m_FogColor;
        [Foldout("Fog")]
        [SerializeField] FogMode m_FogMode;

        public bool Paused { get; private set; }

        Tween fxTweener;

        public override IEnumerator LoadRoutine ()
        {
            RenderSettings.fogColor = m_FogColor;
            RenderSettings.fogMode = m_FogMode;

            yield return null;
        }

        /// <summary>
        /// Pause the game. Doesn't do anything if the game is currently paused.
        /// </summary>
        public void Pause ()
        {
            if (!Paused) setState(true);
        }

        /// <summary>
        /// Unpause the game. Doesn't do anything if the game is not currently paused.
        /// </summary>
        public void Unpause ()
        {
            if (Paused) setState(false);
        }

        void setState (bool pausing)
        {
            Time.timeScale = pausing ? 0 : 1;

            fxTweener?.Kill();

            float
                start = pausing ? 0 : 1,
                end = pausing ? 1 : 0;

            float time = pausing ? m_PauseTime : m_UnpauseTime;
            Ease ease = pausing ? m_PauseEase : m_UnpauseEase;

            float tweenValue = start;
            fxTweener = DOTween.To(() => tweenValue, x => tweenValue = x, end, time)
                .SetUpdate(true) // sets tween timescale as independent from Time.timeScale
                .SetEase(ease)
                .OnStart(() => onFXStart(pausing))
                .OnUpdate(() => onFXUpdate(pausing, tweenValue))
                .OnKill(() => { fxTweener = null; onFXEnd(pausing); });

            Paused = pausing;
        }


        void onFXStart (bool pausing)
        {
            if (pausing) RenderSettings.fog = true;
        }

        void onFXUpdate (bool pausing, float time)
        {
            RenderSettings.fogDensity = m_FogDensityOverFX.Evaluate(time);
        }

        void onFXEnd (bool pausing)
        {
            if (!pausing) RenderSettings.fog = false;
        }
    }
}
