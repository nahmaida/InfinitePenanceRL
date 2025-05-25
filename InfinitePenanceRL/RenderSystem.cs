using System.Collections.Generic;
using System.Drawing;

namespace InfinitePenanceRL
{
    public class RenderSystem
    {
        public void Render(Graphics g, IEnumerable<Entity> entities)
        {
            foreach (var entity in entities)
            {
                var render = entity.GetComponent<RenderComponent>();
                render?.Draw(g);
            }
        }
    }
}