using System.Collections.Generic;

namespace OpenTKBase
{
    public class GameObject
    {
        private List<Component> _components;
        private Transform       _transform;

        public Transform transform
        {
            get { return _transform; }
        }

        public GameObject()
        {
            _components = new List<Component>();

            _transform = AddComponent<Transform>();

            OpenTKApp.APP?.mainScene?.Add(this);
        }

        public T AddComponent<T>() where T : Component, new()
        {
            T t = new T();
            t.gameObject = this;

            _components.Add(t);

            return t;
        }

        public T GetComponent<T>() where T : Component
        {
            foreach (var c in _components)
            {
                if (c is T) return c as T;
            }

            return null;
        }

        public List<T> GetComponents<T>() where T : Component
        {
            List<T> ret = new List<T>(); 
            foreach (var c in _components)
            {
                if (c is T) ret.Add(c as T);
            }

            return ret;
        }

        static List<GameObject> objectsToDestroy = new List<GameObject>();
        public void Destroy()
        {
            objectsToDestroy.Add(this);
        }

        static public void DestroyAllObjects()
        {
            if (objectsToDestroy.Count > 0)
            {
                var objectsToDestroyNow = new List<GameObject>(objectsToDestroy);
                objectsToDestroy.Clear();
                foreach (var obj in objectsToDestroyNow)
                {
                    OpenTKApp.APP?.mainScene?.Remove(obj);
                    // Destroy children
                    var children = new List<Transform>(obj.transform.GetChildren());
                    foreach (var t in children)
                    {
                        t.SetParent(null);
                        t.gameObject.Destroy();
                    }
                }
            }
        }

        public List<Component> GetAllComponents() => _components;
    }
}
