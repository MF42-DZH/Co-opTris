using System;
using Microsoft.Win32.SafeHandles;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ParticleEngine2D
{
    public class ParticleEngine : IDisposable
    {
        private Random random;
        public Vector2 EmitterLocation { get; set; }
        public List<Particle> particles;
        private Texture2D ParticleTexture;
        private Color ParticleColor;
        private bool IsFall;

        public ParticleEngine(Texture2D texture, Vector2 location, Color particleColor, Random randomiser, bool fall = true)
        {
            EmitterLocation = location;
            ParticleTexture = texture;
            particles = new List<Particle>();
            ParticleColor = particleColor;
            IsFall = fall;
            random = randomiser;
        }

        private Particle GenerateNewParticle()
        {
            Texture2D texture = ParticleTexture;
            Vector2 position = EmitterLocation + new Vector2(random.Next(1, 13) - 12, random.Next(1, 13) - 12);
            Vector2 velocity = new Vector2();
            if (IsFall)
            {
                velocity = new Vector2(
                    3f * (float)(random.NextDouble() * 2 - 1),
                    1.5f * (float)(random.NextDouble() + 0.5f));
            } else
            {
                velocity = new Vector2(
                    (float)(8f * (random.NextDouble() - 0.5f)),
                    (float)(8f * (random.NextDouble() - 0.5f)));
            }
            
            Color color = ParticleColor;
            int ttl = IsFall ? random.Next(10, 26) : random.Next(8, 16);
            float angle = IsFall ? 0f : (float)(random.NextDouble() * 360);

            return new Particle(texture, position, velocity, color, angle, ttl, IsFall);
        }

        public void AddParticles(int amt = 25)
        {
            int total = amt;

            for (int i = 0; i < total; i++)
            {
                particles.Add(GenerateNewParticle());
            }
        }

        public void Update()
        {
            for (int particle = 0; particle < particles.Count; particle++)
            {
                particles[particle].Update();
                if (particles[particle].TTL <= 0)
                {
                    particles[particle].Dispose();
                    particles.RemoveAt(particle);
                    particle--;
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            for (int index = 0; index < particles.Count; index++)
            {
                particles[index].Draw(spriteBatch);
            }
            spriteBatch.End();
        }

        // Flag: Has Dispose already been called?
        bool disposed = false;
        // Instantiate a SafeHandle instance.
        SafeHandle handle = new SafeFileHandle(IntPtr.Zero, true);

        // Public implementation of Dispose pattern callable by consumers.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                handle.Dispose();
                random = null;
                ParticleTexture = null;
            }

            disposed = true;
        }

        ~ParticleEngine()
        {
            Dispose(false);
        }
    }
}
