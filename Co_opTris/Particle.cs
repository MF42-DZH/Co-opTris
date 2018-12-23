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
    public class Particle : IDisposable
    {
        public Texture2D Texture { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public Color Colour { get; set; }
        public int TTL { get; set; }
        public float Angle { get; set; }
        private int CurrentTime = 0;
        private bool IsFall;

        public Particle(Texture2D texture, Vector2 position, Vector2 velocity, Color color, float angle, int ttl, bool fall = true)
        {
            Texture = texture;
            Position = position;
            Velocity = velocity;
            Colour = color;
            TTL = ttl;
            IsFall = fall;
            Angle = angle;
        }

        public void Update()
        {
            TTL--;
            CurrentTime++;
            Position += Velocity;
            Velocity += IsFall ? new Vector2(0, 0.035f * (float)Math.Pow(CurrentTime, 1.5)) : new Vector2(0, 0.025f * (float)Math.Pow(CurrentTime, 1.5));  // Gravity
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Rectangle destinationRectangle = new Rectangle((int)Position.X - (Texture.Width / 2), (int)Position.Y - (Texture.Height / 2), Texture.Width, Texture.Height);
            Rectangle sourceRectangle = new Rectangle(0, 0, Texture.Width, Texture.Height);
            Vector2 origin = new Vector2(Texture.Width / 2, Texture.Height / 2);

            spriteBatch.Draw(Texture, destinationRectangle, sourceRectangle, Colour, Angle, origin, SpriteEffects.None, 0f);
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
            }

            disposed = true;
        }

        ~Particle()
        {
            Dispose(false);
        }
    }
}
