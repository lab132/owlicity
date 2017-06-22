using Microsoft.Xna.Framework;

namespace Owlicity
{
  // Note(manu): Everything in here is in pixel-space,
  // i.e. there is no need to convert to meters!
  public class OwlHud
  {
    public Owliver Owliver;

    public SpatialData HealthIconAnchor = new SpatialData();
    public SpriteAnimationInstance HealthIconAnimation;

    public Color FullHealthTint = Color.White;
    public Color NoHealthTint = new Color(60, 60, 60);

    public SpatialData MoneyBagIconAnchor = new SpatialData();
    public SpriteAnimationInstance MoneyBagIconAnimation;

    public SpriteAnimationInstance CrossAnimation;
    public SpriteAnimationInstance[] DigitAnimations;

    public SpatialData KeyRingAnchor = new SpatialData();

    public SpriteAnimationInstance[] KeyAnimations;

    public void ResetLayout(Rectangle bounds)
    {
      Rectangle margin = new Rectangle { X = 8, Y = 8, Width = bounds.Width - 16, Height = bounds.Bottom - 16 };

      HealthIconAnchor.Position = new Vector2(margin.Left, margin.Top) + 0.5f * HealthIconAnimation.ScaledDim;

      {
        Vector2 offset = MoneyBagIconAnimation.ScaledDim;
        offset.X = -offset.X;
        offset.Y = 0.6f * offset.Y;
        MoneyBagIconAnchor.Position = new Vector2(bounds.Right, bounds.Top) + offset;
      }
    }

    public void Initialize()
    {
      HealthIconAnimation = SpriteAnimationFactory.CreateAnimationInstance(SpriteAnimationType.OwlHealthIcon);

      MoneyBagIconAnimation = SpriteAnimationFactory.CreateAnimationInstance(SpriteAnimationType.Bonbon_Gold);

      Owliver = Global.Game.Owliver;

      CrossAnimation = SpriteAnimationFactory.CreateAnimationInstance(SpriteAnimationType.Cross);

      DigitAnimations = new SpriteAnimationInstance[10];
      for(int digit = 0; digit < DigitAnimations.Length; digit++)
      {
        SpriteAnimationType animType = SpriteAnimationType.Digit0 + digit;
        DigitAnimations[digit] = SpriteAnimationFactory.CreateAnimationInstance(animType);
      }

      KeyRingAnchor.AttachTo(HealthIconAnchor);
      KeyRingAnchor.Position.Y += 64;

      KeyAnimations = new SpriteAnimationInstance[(int)KeyType.COUNT];
      for(int keyIndex = 0; keyIndex < KeyAnimations.Length; keyIndex++)
      {
        SpriteAnimationType animType = SpriteAnimationType.Key_Gold + keyIndex;
        KeyAnimations[keyIndex] = SpriteAnimationFactory.CreateAnimationInstance(animType);
      }
    }

    public void Draw(Renderer renderer, float deltaSeconds)
    {
      CrossAnimation.Update(deltaSeconds);
      foreach(SpriteAnimationInstance anim in DigitAnimations)
      {
        anim.Update(deltaSeconds);
      }

      foreach(SpriteAnimationInstance anim in KeyAnimations)
      {
        anim.Update(deltaSeconds);
      }

      if(HealthIconAnimation != null)
      {
        HealthIconAnimation.Update(deltaSeconds);
        int hp = Owliver.Health.MaxHealth;
        SpatialData spatial = HealthIconAnchor.GetWorldSpatialData();
        const float spacing = 3;
        for(int healthIndex = 0; healthIndex < hp; healthIndex++)
        {
          Color tint = healthIndex < Owliver.Health.CurrentHealth ? FullHealthTint : NoHealthTint;
          HealthIconAnimation.Draw(renderer, spatial.GetWorldSpatialData(), tint: tint);
          spatial.Position.X += HealthIconAnimation.ScaledDim.X + spacing;
        }
      }

      if(Owliver.MoneyBag != null)
      {
        MoneyBagIconAnimation.Update(deltaSeconds);

        SpatialData spatial = MoneyBagIconAnchor.GetWorldSpatialData();
        MoneyBagIconAnimation.Draw(renderer, MoneyBagIconAnchor);

        const float spacing = 3;
        float previousAnimWidth = MoneyBagIconAnimation.ScaledDim.X;

        spatial.Position.X -= 0.5f * CrossAnimation.ScaledDim.X + 0.5f * previousAnimWidth + spacing;
        CrossAnimation.Draw(renderer, spatial);
        previousAnimWidth = CrossAnimation.ScaledDim.X;

        int value = Owliver.MoneyBag.CurrentAmount;
        while(true)
        {
          int digit = value % 10;
          SpriteAnimationInstance digitAnim = DigitAnimations[digit];

          spatial.Position.X -= 0.5f * previousAnimWidth + 0.5f * digitAnim.ScaledDim.X + spacing;
          digitAnim.Draw(renderer, spatial);

          value /= 10;
          if(value == 0)
            break;

          previousAnimWidth = digitAnim.ScaledDim.X;
        }
      }

      if(Owliver.KeyRing != null)
      {
        SpatialData anchor = KeyRingAnchor.GetWorldSpatialData();
        for(int keyTypeIndex = 0; keyTypeIndex < Owliver.KeyRing.CurrentKeyAmounts.Length; keyTypeIndex++)
        {
          KeyType keyType = (KeyType)keyTypeIndex;
          int keyAmount = Owliver.KeyRing.CurrentKeyAmounts[keyTypeIndex];
          SpriteAnimationInstance keyAnim = KeyAnimations[keyTypeIndex];
          SpatialData spatial = anchor.GetWorldSpatialData();
          for(int keyIndex = 0; keyIndex < keyAmount; keyIndex++)
          {
            keyAnim.Draw(renderer, spatial);
            spatial.Position.X += 0.6f * keyAnim.ScaledDim.X;
          }

          anchor.Position.Y += 0.8f * keyAnim.ScaledDim.Y;
        }
      }
    }
  }
}
