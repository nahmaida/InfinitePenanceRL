namespace InfinitePenanceRL
{
    // Базовый класс для всех компонентов - кирпичики, из которых собираются игровые объекты
    public abstract class Component
    {
        public Entity Owner { get; set; }  // Объект, которому принадлежит компонент
        public GameEngine Game => Owner?.Game;  // Быстрый доступ к игровому движку
        public virtual void Update() { }  // Обновление состояния компонента каждый кадр
    }
}