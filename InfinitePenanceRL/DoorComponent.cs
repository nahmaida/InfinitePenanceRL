using System.Drawing;

namespace InfinitePenanceRL
{
    public class DoorComponent : Component
    {
        public bool IsOpen { get; private set; }
        public bool IsHovered { get; set; } = false;
        
        public DoorComponent(bool isOpen = false)
        {
            IsOpen = isOpen;
        }
        
        public void Toggle()
        {
            IsOpen = !IsOpen;
            
            // обновляем спрайт двери
            var render = Owner.GetComponent<RenderComponent>();
            if (render != null)
            {
                render.SpriteName = IsOpen ? "door_open" : "door_closed";
            }
            
            // обновляем коллайдер - открытые двери проходимы
            var collider = Owner.GetComponent<ColliderComponent>();
            if (collider != null)
            {
                collider.IsSolid = !IsOpen;
            }
        }
        
        public void SetOpen(bool open)
        {
            if (IsOpen != open)
            {
                IsOpen = open;
                
                // обновляем спрайт двери
                var render = Owner.GetComponent<RenderComponent>();
                if (render != null)
                {
                    render.SpriteName = IsOpen ? "door_open" : "door_closed";
                }
                
                // обновляем коллайдер
                var collider = Owner.GetComponent<ColliderComponent>();
                if (collider != null)
                {
                    collider.IsSolid = !IsOpen;
                }
            }
        }
    }
} 