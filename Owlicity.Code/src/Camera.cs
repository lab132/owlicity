using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Owlicity
{
  public class Camera
  {
    public Viewport Viewport;

    // Note(manu): This is where the view, projection, world matrices are stored in.
    public BasicEffect Effect;

    // We need to change the basis ("world") to compensate
    // for inconsistent coordinate systems with XNA and DirectX.
    // See: https://gamedev.stackexchange.com/a/69757
    private static readonly Matrix WorldMatrix = Matrix.CreateWorld(
      position: Vector3.Zero,
      forward: new Vector3(0, 0, -1),
      up: new Vector3(0, 1, 0));

    public void Reset(GraphicsDevice device, ref Matrix projection, ref Matrix view)
    {
      Viewport = device.Viewport;
      Effect = new BasicEffect(device)
      {
        Projection = projection,
        View = view,
        World = WorldMatrix,

        VertexColorEnabled = true,
        TextureEnabled = true,
      };
    }
  }
}
