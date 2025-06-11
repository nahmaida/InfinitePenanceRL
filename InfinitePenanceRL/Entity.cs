using System.Collections.Generic;
using System.Linq;

namespace InfinitePenanceRL
{
    // Базовый класс для всех игровых объектов (игрок, враги, стены и т.д.)
    public class Entity
    {
        public Vector2 Position { get; set; }  // Позиция объекта в игровом мире
        public List<Component> Components { get; } = new List<Component>();  // Список компонентов объекта
        public GameEngine Game { get; set; }  // Ссылка на игровой движок

        // Ищем компонент нужного типа (например, для отрисовки или физики)
        public T GetComponent<T>() where T : Component
        {
            return Components.OfType<T>().FirstOrDefault();
        }

        // Добавляем новый компонент к объекту
        public void AddComponent(Component component)
        {
            component.Owner = this;
            Components.Add(component);
        }
    }
}