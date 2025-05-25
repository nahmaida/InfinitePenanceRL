namespace InfinitePenanceRL
{
    public abstract class Component
    {
        public Entity Owner { get; set; }
        public GameEngine Game => Owner?.Game;
        public virtual void Update() { }
    }
}