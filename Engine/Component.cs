using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTKBase
{
    public class Component
    {
        private GameObject  _gameObject;
        public  bool        enable = true;

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
                if (!c.enable)
                {
                    Console.WriteLine("Awakening disabled component, not sure if this leads to issues later...");
                }
                c.Awake();
            }
        }
        static public void StartAll()
        {
            var toStart = componentsToStart;
            componentsToStart = new List<Component>();
            foreach (var c in toStart)
            {
                if (!c.enable)
                {
                    Console.WriteLine("Starting disabled component, not sure if this leads to issues later...");
                }
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

        public void Destroy(GameObject gameObject)
        {
            gameObject.Destroy();
        }

        static public T FindObjectOfType<T>() where T : Component
        {
            var scene = OpenTKApp.APP.mainScene;
            return scene.FindObjectOfType<T>();
        }
        
        public virtual void Awake() { }
        public virtual void Start() { }
        public virtual void Update() { }

    }
}
