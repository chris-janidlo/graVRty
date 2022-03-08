using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GraVRty.Loading
{
    public abstract class LoadableEagerCache<T> : Loadable
        where T : MonoBehaviour
    {
        [SerializeField] List<T> m_PrefabssToInstantiate;
        [SerializeField] Vector3 m_InstantiationLocation;

        public override float LoadProgress { get => (float) loadedPrefabs.Count / m_PrefabssToInstantiate.Count; }

        protected Dictionary<T, T> loadedPrefabs;

        public override IEnumerator LoadRoutine ()
        {
            FinishedLoading = false;

            loadedPrefabs = new Dictionary<T, T>();

            foreach (var prefab in m_PrefabssToInstantiate)
            {
                T instantiatedPrefab = instantiatePrefab(prefab);
                yield return null;
                loadedPrefabs[prefab] = instantiatedPrefab;
                instantiatedPrefab.gameObject.SetActive(false);
            }

            FinishedLoading = true;
        }

        public T InstantiateFromCache (T prefab, Transform parent)
        {
            assertLoaded(nameof(InstantiateFromCache));

            if (!loadedPrefabs.TryGetValue(prefab, out T instance))
            {
                throw new InvalidOperationException($"cannot instantiate {prefab.name} from pool because it was never loaded into the pool");
            }

            if (instance.gameObject.activeSelf)
            {
                throw new InvalidOperationException($"cannot instantiate {prefab.name} from pool because it is already in use");
            }

            instance.transform.parent = parent;
            instance.transform.SetPositionAndRotation(parent.position, parent.rotation);
            instance.transform.localScale = Vector3.one;

            instance.gameObject.SetActive(true);

            return instance;
        }

        public void ReleaseToCache (T prefab)
        {
            assertLoaded(nameof(ReleaseToCache));

            if (!loadedPrefabs.TryGetValue(prefab, out T instance))
            {
                throw new InvalidOperationException($"cannot release {prefab.name} back to the pool because it was never loaded into the pool");
            }

            if (!instance.gameObject.activeSelf)
            {
                throw new InvalidOperationException($"cannot release {prefab.name} back to the pool because it is not currently in use");
            }

            instance.transform.parent = null;
            instance.gameObject.SetActive(false);
        }

        protected virtual T instantiatePrefab (T prefab)
        {
            return Instantiate(prefab, m_InstantiationLocation, Quaternion.identity);
        }
    }
}