namespace InfinitePenanceRL
{
    public static class EntityFactory
    {
        public static Entity CreatePlayer(GameEngine game)
        {
            var player = new Entity
            {
                Position = new Vector2(100, 100),
                Game = game
            };

            player.AddComponent(new MovementComponent());
            player.AddComponent(new RenderComponent
            {
                Color = Color.Blue,
                Size = new Size(32, 32)
            });

            return player;
        }
    }
}