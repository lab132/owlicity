﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Owlicity
{
  public struct Particle
  {
    public Vector2 Position { get; set; }
    public float Rotation { get; set; }
    public Vector2 Velocity { get; set; }
    public float AngularVelocity { get; set; }
    public Color Color { get; set; }
    public Texture2D Texture { get; set; }
    public float TTL { get; set; }
  }
}
