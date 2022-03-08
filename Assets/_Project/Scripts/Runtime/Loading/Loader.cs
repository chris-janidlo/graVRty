using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace GraVRty.Loading
{
    public class Loader : MonoBehaviour
    {
        [SerializeField] float m_FadeInTime;
        [SerializeField] Ease m_FadeInEase;

        [SerializeField] List<Loadable> m_Loadables;
        [SerializeField] Renderer m_FadeHelmet;

        IEnumerator Start ()
        {
            // from https://stackoverflow.com/a/60239710/5931898
            foreach (var loadable in m_Loadables)
            {
                Coroutine routine = StartCoroutine(loadable.LoadRoutine());
                yield return routine;
            }

            m_FadeHelmet.material.DOFade(0, m_FadeInTime)
                .SetEase(m_FadeInEase)
                .OnKill(() => Destroy(gameObject));
        }

        void Update ()
        {
            float overallProgress = m_Loadables.Sum(l => l.LoadProgress) / m_Loadables.Count;

            // TODO: can show load percentage here
        }
    }
}
