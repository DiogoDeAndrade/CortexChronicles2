using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTKBase
{
    public class Component
    {
        private GameObject _gameObject;

        public GameObject gameObject
        {
            get { return _gameObject; }
            set { _gameObject = value; }
        }

        static List<Component> componentsToAwake = new List<Component>();
        static List<Component> componentsToStart = new List<Component>();
        static public bool needToAwake => componentsToAwake.Count > 0;
        static public bool needToStart => componentsToStart.Count > 0;

        static public void AwakeAll()
        {
            var toAwake = componentsToAwake;
            componentsToAwake = new List<Component>();
            foreach (var c in toAwake)
            {
                c.Awake();
            }
        }
        static public void StartAll()
        {
            var toStart = componentsToStart;
            componentsToStart = new List<Component>();
            foreach (var c in toStart)
            {
                c.Start();
            }
        }

        public Component()
        {
            componentsToAwake.Add(this);
            componentsToStart.Add(this);
        }

        public Transform transform => _gameObject.transform;

        public T GetComponent<T>() where T : Component => gameObject?.GetComponent<T>();

        public virtual void Awake() { }
        public virtual void Start() { }
        public virtual void Update() { }
    }
}
