using Microsoft.Xna.Framework;
using OrpticonGameHelper.Classes.Particles;

namespace CityGame.Classes.Rendering.Particles;

public sealed class ExplosionParticle: Particle
{
    public float LifeTimeScale = 1;
    public float LifeTime = 1;
    public Vector2 UVOffset = Vector2.Zero;

    public void ReduceLifeTime(float deltatime)
    {
        this.LifeTime -= deltatime * this.LifeTimeScale;

        if (this.LifeTime <= 0)
        {
            this.IsActive = false;
        }
    }
}