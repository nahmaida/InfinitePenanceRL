using System.Drawing;

namespace InfinitePenanceRL
{
    public class ColliderComponent : Component
    {
        private RectangleF _bounds;
        public bool IsSolid { get; set; } = true;

        public RectangleF Bounds
        {
            get
            {
                UpdateBounds();
                return _bounds;
            }
        }

        public override void Update()
        {
            UpdateBounds();
        }

        public void UpdateBounds()
        {
            if (Owner == null) return;

            var render = Owner.GetComponent<RenderComponent>();
            if (render != null)
            {
                _bounds = new RectangleF(
                    Owner.Position.X,
                    Owner.Position.Y,
                    render.Size.Width,
                    render.Size.Height);
            }
        }

        public bool CheckCollision(ColliderComponent other)
        {
            if (other == null || Owner == null || other.Owner == null)
                return false;

            return Bounds.IntersectsWith(other.Bounds);
        }
    }
}