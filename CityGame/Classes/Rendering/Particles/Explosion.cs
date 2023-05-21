using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CityGame.Classes.Rendering.Shaders;
using CityGame.Classes.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OrpticonGameHelper;
using OrpticonGameHelper.Classes.Particles;
using OrpticonGameHelper.Classes.Utils;
using static CityGame.MainWindow;

namespace CityGame.Classes.Rendering.Particles;

public sealed class Explosion : ParticleEmitter, IParticleHandler
{
    internal const string SHADER_PATH = "CityGame.Classes.Rendering.Particles.explosion.cfx";
    private const float PIXEL_TO_TILE_RATIO = 1 / 2f;
    
    private static Effect? _explosionShader = null;

    public Color MinColor = Color.Yellow;
    public Color MaxColor = Color.OrangeRed;
    public Vector2 DirectionTendency = Vector2.UnitX;
    public float DirectionVariance = 180;
    public int ParticleCountMin = 8;
    public int ParticleCountMax = 8;
    
    public Explosion()
        : base(Explosion.GetShader())
    {
        this.ParticleHandler = this;
    }

    private static Effect GetShader()
    {
        if (Explosion._explosionShader == null)
        {
            Explosion._explosionShader = ShaderLoader.Get(Explosion.SHADER_PATH);
        }

        return Explosion._explosionShader;
    }

    public ICollection<IParticle> Generate()
    {
        List<IParticle> particles = new List<IParticle>();
        for (int i = 0; i < Random.Shared.Next(ParticleCountMin, ParticleCountMax + 1); i++)
        {
            ExplosionParticle particle = new ExplosionParticle();
            particle.OriginalPosition = particle.Position = Vector2.Zero;

            float variance = ((float)Random.Shared.NextSingle() * 2 - 1) * DirectionVariance;
            Vector2 direction = DirectionTendency.RotateBy(variance);
            particle.Direction = direction * Random.Shared.NextSingle();

            particle.color = Color.Lerp(MinColor, MaxColor, Random.Shared.NextSingle());

            particle.LifeTime = EmissionTime;
            particle.LifeTimeScale = Random.Shared.NextSingle(1, 1.5f);
            
            particle.UVOffset = new Vector2(Random.Shared.NextSingle(1), Random.Shared.NextSingle(1));

            particles.Add(particle);
        }

        return particles;
    }
    public bool EaseMovement { get; set; } = true;
    public void Move(ICollection<IParticle> particles, float deltatime, ParticleEmitter emitter)
    {

        foreach (IParticle particle in particles)
        {
            float time = emitter.NormalizedTime;
            if (particle is ExplosionParticle explParticle) 
            {
                explParticle.ReduceLifeTime(deltatime);
                time = ParticleEmitter.CalculateNormalizedReversedTime(explParticle.LifeTime, this.EmissionTime);
            }
            
            float distanceCalc = EaseMovement ? Easing.Easing.OutExpo(time) : time;
            float distance = distanceCalc * emitter.MaxParticleDistance;
            particle.Position = particle.OriginalPosition + particle.Direction * distance;

        }
    }

    public void Draw(ref ParticleDrawContext context)
    {
        context.Effect.Parameters["view_projection"].SetValue(context.View * context.Projection);
        context.Effect.Parameters["color"].SetValue(this.MinColor.ToVector4());


        context.SpriteBatch.Begin(
            effect: Effect, 
            blendState: BlendState.NonPremultiplied,
            sortMode: SpriteSortMode.Immediate,
            samplerState: SamplerState.LinearWrap
        );

        float pixelSize = MainWindow.TileSize * Explosion.PIXEL_TO_TILE_RATIO;
        float sizeToTileRatio = (float)context.Size / MainWindow.TileSize;
        int resultingPixelSize = (int)Math.Ceiling(pixelSize * sizeToTileRatio);

        context.Effect.Parameters["size"].SetValue(context.Size / 2);
        context.Effect.Parameters["pixelSize"].SetValue(resultingPixelSize / 2);
        foreach (IParticle particle in context.Particles)
        {
            if (!particle.IsActive)
            {
                continue;
            }

            float normTime = NormalizedTime;
            if (particle is ExplosionParticle explParticle)
            {
                normTime = ParticleEmitter.CalculateNormalizedReversedTime(explParticle.LifeTime, this.EmissionTime);
                context.Effect.Parameters["color"].SetValue(explParticle.color.ToVector4());
                context.Effect.Parameters["noiseOffset"].SetValue(explParticle.UVOffset);
            }
            float time = Easing.Easing.OutExpo(normTime);
        
            context.Effect.Parameters["timing"].SetValue(time);
            context.Effect.Parameters["position"].SetValue(context.EmitterCenterPosition + particle.Position);
            int x = (int)(context.EmitterRenderPosition.X + particle.Position.X);
            int y = (int)(context.EmitterRenderPosition.Y + particle.Position.Y);

            context.SpriteBatch.Draw(
                Window._textures[Environment.CurrentDirectory + "\\Resources\\NoiseTexture.png"],
                new Rectangle(x, y, context.Size, context.Size),
                Color.White
            );
        }
        
        context.SpriteBatch.End();
    }
}