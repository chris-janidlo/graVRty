using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace GraVRty.Loading
{
    [CreateAssetMenu(menuName = "GraVRty/Loadables/Semi-Eager GameObject Pool")]
    public class LoadableSemiEagerGameObjectPool : Loadable, IObjectPool<GameObject>
    {

        [SerializeField] GameObject m_Prefab;
        [SerializeField] Vector3 m_InstantiationLocation = new(1000, 1000, 1000);
        [SerializeField] int m_EagerLoadCount, m_MaxInstanceCount;

        public int CountInactive
        {
            get
            {
                assertLoaded(nameof(CountInactive));
                return nativePool.CountInactive;
            }
        }

        ObjectPool<GameObject> nativePool;

        public override IEnumerator LoadRoutine ()
        {
            nativePool = new ObjectPool<GameObject>(create, onGet, onRelease, onDestroy, true, m_EagerLoadCount, m_MaxInstanceCount);

            for (int i = 0; i < m_EagerLoadCount; i++)
            {
                GameObject instance = nativePool.Get();
                yield return null;
                nativePool.Release(instance);

                LoadProgress = i / m_EagerLoadCount;
            }

            LoadProgress = 1;
            FinishedLoading = true;
        }

        public void Clear ()
        {
            assertLoaded(nameof(Clear));
            nativePool.Clear();
        }

        public GameObject Get ()
        {
            assertLoaded(nameof(Get));
            return nativePool.Get();
        }

        public PooledObject<GameObject> Get (out GameObject v)
        {
            assertLoaded(nameof(Get));
            return nativePool.Get(out v);
        }

        public void Release (GameObject element)
        {
            assertLoaded(nameof(Release));
            nativePool.Release(element);
        }

        public void Release (MonoBehaviour component)
        {
            Release(component.gameObject);
        }

        public GameObject Get (Transform parent)
        {
            GameObject instance = Get();
            setParent(instance, parent);
            return instance;
        }

        public GameObject Get (Vector3 position, Quaternion rotation)
        {
            GameObject instance = Get();
            instance.transform.SetPositionAndRotation(position, rotation);
            return instance;
        }

        public GameObject Get (Vector3 position, Quaternion rotation, Transform parent)
        {
            GameObject instance = Get();
            setParent(instance, parent);
            instance.transform.SetPositionAndRotation(position, rotation);
            return instance;
        }

        public T Get<T> () where T : MonoBehaviour
        {
            return getComponentOnInstance<T>(Get());
        }

        public T Get<T> (Transform parent) where T : MonoBehaviour
        {
            return getComponentOnInstance<T>(Get(parent));
        }

        public T Get<T> (Vector3 position, Quaternion rotation) where T : MonoBehaviour
        {
            return getComponentOnInstance<T>(Get(position, rotation));
        }

        public T Get<T> (Vector3 position, Quaternion rotation, Transform parent) where T : MonoBehaviour
        {
            return getComponentOnInstance<T>(Get(position, rotation, parent));
        }

        GameObject create ()
        {
            return Instantiate(m_Prefab, m_InstantiationLocation, Quaternion.identity);
        }

        void onGet (GameObject instance)
        {
            instance.SetActive(true);
        }

        void onRelease (GameObject instance)
        {
            instance.SetActive(false);
            instance.transform.SetParent(null);
        }

        void onDestroy (GameObject instance)
        {
            Destroy(instance);
        }

        void setParent (GameObject instance, Transform parent)
        {
            instance.transform.SetParent(parent);
            instance.transform.SetPositionAndRotation(parent.position, parent.rotation);
            instance.transform.localScale = Vector3.one;
        }

        T getComponentOnInstance<T> (GameObject instance) where T : MonoBehaviour
        {
            T component = instance.GetComponent<T>();

            if (component == null)
            {
                throw new MissingComponentException($"Cannot load component '{typeof(T).Name}' from prefab '{instance}' through pool '{name}'. Did you use the right pool?");
            }

            return component;
        }
    }
}
