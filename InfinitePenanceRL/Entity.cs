using System.Collections.Generic;
using System.Linq;

namespace InfinitePenanceRL
{
    // класс для существ и обьектов
    public class Entity
    {
        public Vector2 Position { get; set; }
        public List<Component> Components { get; } = new List<Component>();
        public GameEngine Game { get; set; }

        public T GetComponent<T>() where T : Component
        {
            return Components.OfType<T>().FirstOrDefault();
        }

        public void AddComponent(Component component)
        {
            component.Owner = this;
            Components.Add(component);
        }
    }
}