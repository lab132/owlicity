using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using VelcroPhysics.Dynamics;
using VelcroPhysics.Factories;

namespace Owlicity
{
  public class GameObject : ISpatial
  {
    private static int _idGenerator;

    public readonly int ID = ++_idGenerator;

    public string Name;

    public List<ComponentBase> Components { get; } = new List<ComponentBase>();

    public SpatialComponent RootComponent;

    private SpatialData _spatial = new SpatialData();
    public SpatialData Spatial
    {
      get
      {
        SpatialData result;
        if(RootComponent != null)
        {
          result = RootComponent.Spatial;
        }
        else
        {
          result = _spatial;
        }

        return result;
      }
    }

    public GameLayer Layer = GameLayer.Default;

    public void AddComponent(ComponentBase newComponent)
    {
      Debug.Assert(!Components.Contains(newComponent));
      Components.Add(newComponent);
    }

    public T GetComponent<T>()
      where T : ComponentBase
    {
      return Components.OfType<T>().FirstOrDefault();
    }

    public IEnumerable<T> GetComponents<T>()
      where T : ComponentBase
    {
      return Components.OfType<T>();
    }

    public void Initialize()
    {
      ComponentBase[] toInit = Components.Where(c => c.IsInitializationEnabled).ToArray();
      foreach(ComponentBase component in toInit)
      {
        component.BeforeInitialize?.Invoke();
        component.Initialize();
      }

      foreach(ComponentBase component in toInit)
      {
        component.BeforePostInitialize?.Invoke();
        component.PostInitialize();
      }
    }

    public void PrePhysicsUpdate(float deltaSeconds)
    {
      foreach(ComponentBase component in Components.Where(c => c.IsPrePhysicsUpdateEnabled))
      {
        component.BeforePrePhysicsUpdate?.Invoke();
        component.PrePhysicsUpdate(deltaSeconds);
      }
    }

    public void Update(float deltaSeconds)
    {
      foreach(ComponentBase component in Components.Where(c => c.IsUpdateEnabled))
      {
        component.BeforeUpdate?.Invoke();
        component.Update(deltaSeconds);
      }
    }

    public void Draw(Renderer renderer)
    {
      foreach(ComponentBase component in Components.Where(c => c.IsDrawEnabled))
      {
        component.BeforeDraw?.Invoke();
        component.Draw(renderer);
      }
    }

    public void Destroy()
    {
      foreach(ComponentBase component in Components)
      {
        component.BeforeDestroy?.Invoke();
        component.Destroy();
      }
    }

    public override string ToString()
    {
      return $"{ID}: {Name} @ {Spatial}";
    }
  }

  public enum GameObjectType
  {
    Unknown,

    // Misc
    Camera,

    // Characters
    Owliver,

    Shop,

    // Mobs
    Slurp,

    // Bosses
    Tankton,

    // Particles
    DeathConfetti,

    Projectile,

    // Static stuff
    BackgroundScreen,
    Gate,
    Tree_Fir,
    Tree_FirAlt, // is "upside down"
    Tree_Conifer,
    Tree_ConiferAlt, // is "upside down"
    Tree_Oak,
    Tree_Orange,
    Bush,

    // Pickups
    Bonbon_Gold,
    Bonbon_Red,
    Key_Gold,

    ShopItem_FruitBowl,
    ShopItem_FishingRod,
    ShopItem_Stick,

    // Random groups
    Random_FirTree,
    Random_FirTreeAlt,
    Random_OakTree,
  }

  public static class GameObjectFactory
  {
    private static Random _random;
    private static int[] _knownCreationCount;

    public static void Initialize()
    {
      _random = new Random();
      _knownCreationCount = new int[Enum.GetNames(typeof(GameObjectType)).Length];
    }

    public static SquashComponent CreateOnHitSquasher(GameObject go, HealthComponent health)
    {
      var result = new SquashComponent(go);
      health.OnHit += (damage) =>
      {
        result.StartSequence();
      };

      return result;
    }

    public static BlinkingSequenceComponent CreateOnHitBlinkingSequence(GameObject go, HealthComponent health)
    {
      var result = new BlinkingSequenceComponent(go);
      health.OnHit += (damage) =>
      {
        result.StartSequence();
      };

      return result;
    }

    public static GameObject CreateKnown(GameObjectType type)
    {
      GameObject go = new GameObject();
      switch(type)
      {
        case GameObjectType.Camera:
        {
          var spc = new SpringArmComponent(go)
          {
            TargetInnerRange = 0.2f,
            TargetRange = float.MaxValue,

            SpeedFactor = 5,

            DebugDrawingEnabled = false,
          };
          go.RootComponent = spc;

          var cc = new CameraComponent(go);
          cc.AttachTo(spc);

#if DEBUG
          var mc = new MovementComponent(go)
          {
            MaxMovementSpeed = 5.0f,
          };
#endif
        }
        break;

        case GameObjectType.Owliver:
        {
          var bc = new BodyComponent(go)
          {
            InitMode = BodyComponentInitMode.Manual,
          };
          bc.BeforeInitialize += () =>
          {
            SpatialData s = go.GetWorldSpatialData();
            bc.Body = new Body(
              world: Global.Game.World,
              position: s.Position,
              rotation: s.Rotation.Radians,
              bodyType: BodyType.Dynamic,
              userdata: bc);

            float radius = Global.ToMeters(60) * Global.OwliverScale.X;
            float density = Global.OwliverDensity;
            FixtureFactory.AttachCircle(
              radius: radius,
              density: density,
              body: bc.Body,
              offset: Global.ToMeters(0, -60) * Global.OwliverScale,
              userData: bc);
            FixtureFactory.AttachCircle(
              radius: radius,
              density: density,
              body: bc.Body,
              offset: Global.ToMeters(0, -130) * Global.OwliverScale,
              userData: bc);

            bc.Body.FixedRotation = true;
            bc.Body.CollisionCategories = Global.OwliverCollisionCategory;
            bc.Body.CollidesWith = Global.LevelCollisionCategory | Global.EnemyCollisionCategory;
            bc.Body.SleepingAllowed = false;
            bc.Body.LinearDamping = 15.0f;
          };
          go.RootComponent = bc;

          var oc = new OwliverComponent(go)
          {
          };

          var mc = new MovementComponent(go)
          {
            ManualInputProcessing = true,
            MaxMovementSpeed = 2.5f,
          };

          var sa = new SpriteAnimationComponent(go)
          {
            AnimationTypes = new List<SpriteAnimationType>
            {
              SpriteAnimationType.Owliver_Idle_Stick_Left,
              SpriteAnimationType.Owliver_Idle_Stick_Right,
              SpriteAnimationType.Owliver_Walk_Stick_Left,
              SpriteAnimationType.Owliver_Walk_Stick_Right,
              SpriteAnimationType.Owliver_Attack_Stick_Left,
              SpriteAnimationType.Owliver_Attack_Stick_Right,
              SpriteAnimationType.Owliver_Idle_FishingRod_Left,
              SpriteAnimationType.Owliver_Idle_FishingRod_Right,
              SpriteAnimationType.Owliver_Walk_FishingRod_Left,
              SpriteAnimationType.Owliver_Walk_FishingRod_Right,
              SpriteAnimationType.Owliver_Attack_FishingRod_Left,
              SpriteAnimationType.Owliver_Attack_FishingRod_Right,
            },
          };
          sa.AttachTo(bc);

          var hc = new HealthComponent(go)
          {
            MaxHealth = 5,
          };
          hc.OnHit += (damage) =>
          {
            hc.MakeInvincible(oc.HitDuration);
          };

          CreateOnHitSquasher(go, hc).SetDefaultCurves(oc.HitDuration);

          CreateOnHitBlinkingSequence(go, hc).SetDefaultCurves(oc.HitDuration);

          var moc = new MoneyBagComponent(go)
          {
            InitialAmount = 0,
          };

          var kc = new KeyRingComponent(go);
        }
        break;

        case GameObjectType.Shop:
        {
          var bc = new BodyComponent(go)
          {
            InitMode = BodyComponentInitMode.Manual,
          };
          bc.BeforePostInitialize += () =>
          {
            SpatialData s = go.GetWorldSpatialData();
            bc.Body = BodyFactory.CreateBody(
              world: Global.Game.World,
              position: s.Position,
              rotation: s.Rotation.Radians,
              userData: bc);
            FixtureFactory.AttachRectangle(
              body: bc.Body,
              offset: Global.ToMeters(0, -50),
              width: Global.ToMeters(330),
              height: Global.ToMeters(100),
              density: Global.OwliverDensity,
              userData: bc);
            FixtureFactory.AttachRectangle(
              body: bc.Body,
              offset: Global.ToMeters(0, -160),
              width: Global.ToMeters(280),
              height: Global.ToMeters(150),
              density: Global.OwliverDensity,
              userData: bc);
          };
          go.RootComponent = bc;

          var sacShop = new SpriteAnimationComponent(go)
          {
            AnimationTypes = new List<SpriteAnimationType>
            {
              SpriteAnimationType.Shop,
            },
          };
          sacShop.AttachTo(bc);

          var sacShopkeeper = new SpriteAnimationComponent(go)
          {
            AnimationTypes = new List<SpriteAnimationType>
            {
              SpriteAnimationType.Shopkeeper_Idle_Front,
            },
          };
          sacShopkeeper.Spatial.Position.Y -= Global.ToMeters(100);
          sacShopkeeper.AttachTo(sacShop);
        }
        break;

        case GameObjectType.Slurp:
        {
          var bc = new BodyComponent(go)
          {
            InitMode = BodyComponentInitMode.Manual,
          };

          bc.BeforeInitialize += () =>
          {
            SpatialData s = go.GetWorldSpatialData();
            bc.Body = new Body(
              world: Global.Game.World,
              position: s.Position,
              rotation: s.Rotation.Radians,
              bodyType: BodyType.Dynamic,
              userdata: bc);

            float radius = Global.ToMeters(80 * Global.SlurpScale.X);
            float density = Global.OwliverDensity;
            FixtureFactory.AttachCircle(
              radius: radius,
              density: density,
              body: bc.Body,
              offset: Global.ToMeters(0, -25) * Global.SlurpScale,
              userData: bc);
            FixtureFactory.AttachCircle(
              radius: radius,
              density: density,
              body: bc.Body,
              offset: Global.ToMeters(0, 25) * Global.SlurpScale,
              userData: bc);

            bc.Body.FixedRotation = true;
            bc.Body.LinearDamping = 5.0f;
          };

          go.RootComponent = bc;

          var sa = new SpriteAnimationComponent(go)
          {
            AnimationTypes = new List<SpriteAnimationType>
            {
              SpriteAnimationType.Slurp_Idle_Left,
              SpriteAnimationType.Slurp_Idle_Right,
            }
          };
          sa.AttachTo(bc);

          var ec = new EnemyComponent(go)
          {
            AnimationType_Idle_Left = SpriteAnimationType.Slurp_Idle_Left,
            AnimationType_Idle_Right = SpriteAnimationType.Slurp_Idle_Right,
          };

          var hc = new HealthComponent(go)
          {
            MaxHealth = 3,
          };
          hc.OnHit += (damage) =>
          {
            hc.MakeInvincible(ec.HitDuration);
          };
          hc.OnDeath += (damage) =>
          {
            Global.Game.RemoveGameObject(go);

            var confetti = CreateKnown(GameObjectType.DeathConfetti);
            confetti.Spatial.CopyFrom(go.Spatial);
            confetti.GetComponent<AutoDestructComponent>().DestructionDelay = TimeSpan.FromSeconds(1.0f);
            Global.Game.AddGameObject(confetti);
          };

          var hdc = new HealthDisplayComponent(go)
          {
            InitialDisplayOrigin = HealthDisplayComponent.DisplayOrigin.Bottom,
            HealthIcon = SpriteAnimationFactory.CreateAnimationInstance(SpriteAnimationType.Cross),
          };
          hdc.AttachTo(sa);

          CreateOnHitSquasher(go, hc).SetDefaultCurves(ec.HitDuration);

          CreateOnHitBlinkingSequence(go, hc).SetDefaultCurves(ec.HitDuration);

          var hoc = new HomingComponent(go)
          {
            TargetRange = 3.0f,
            Speed = 0.5f,

            DebugDrawingEnabled = true,
          };
          hoc.BeforeInitialize += () =>
          {
            hoc.Target = Global.Game.Owliver;
          };
          hoc.AttachTo(bc);
        }
        break;

        case GameObjectType.Tankton:
        {
          var tankton = new TanktonComponent(go)
          {
            HitDuration = 0.25f,
          };

          var bc = new BodyComponent(go)
          {
            InitMode = BodyComponentInitMode.Manual,
          };
          bc.BeforeInitialize += () =>
          {
            SpatialData s = go.GetWorldSpatialData();
            Vector2 dim = Global.ToMeters(350, 400) * Global.TanktonScale;
            bc.Body = BodyFactory.CreateRoundedRectangle(
              world: Global.Game.World,
              position: s.Position,
              rotation: s.Rotation.Radians,
              bodyType: BodyType.Dynamic,
              userData: bc,
              width: dim.X,
              height: dim.Y,
              xRadius: 0.5f,
              yRadius: 0.5f,
              density: 2 * Global.OwliverDensity,
              segments: 0);
            bc.Body.FixedRotation = true;
            bc.Body.LinearDamping = 20.0f;
          };
          go.RootComponent = bc;

          var sa = new SpriteAnimationComponent(go)
          {
            AnimationTypes = new List<SpriteAnimationType>
            {
              SpriteAnimationType.Tankton_Idle_Left,
              SpriteAnimationType.Tankton_Idle_Right,
            },
          };
          sa.Spatial.Position.Y += Global.ToMeters(100);
          sa.AttachTo(bc);

          var hc = new HealthComponent(go)
          {
            MaxHealth = 20,
          };
          hc.OnHit += (damage) =>
          {
            hc.MakeInvincible(tankton.HitDuration);
          };
          hc.OnDeath += (damage) =>
          {
            Global.Game.RemoveGameObject(go);

            var confetti = CreateKnown(GameObjectType.DeathConfetti);
            confetti.Spatial.CopyFrom(go.Spatial);
            confetti.GetComponent<AutoDestructComponent>().DestructionDelay = TimeSpan.FromSeconds(5.0f);
            ParticleEmitterComponent deathEmitter = confetti.GetComponent<ParticleEmitterComponent>();
            deathEmitter.NumParticles = 4096;
            deathEmitter.BeforePostInitialize += () =>
            {
              // TODO(manu): Somehow, this doesn't work...
              deathEmitter.Emitter.MaxParticleSpeed = 200.0f;
            };
            Global.Game.AddGameObject(confetti);
          };

          var hdc = new HealthDisplayComponent(go)
          {
            HealthIcon = SpriteAnimationFactory.CreateAnimationInstance(SpriteAnimationType.Cross),
            InitialDisplayOrigin = HealthDisplayComponent.DisplayOrigin.Bottom,
            NumIconsPerRow = 5,
          };
          hdc.AttachTo(sa);

          CreateOnHitSquasher(go, hc).SetDefaultCurves(
            duration: tankton.HitDuration,
            extremeScale: new Vector2(0.9f, 1.1f));

          CreateOnHitBlinkingSequence(go, hc).SetDefaultCurves(tankton.HitDuration);
        }
        break;

        case GameObjectType.DeathConfetti:
        {
          var adc = new AutoDestructComponent(go)
          {
            DestructionDelay = TimeSpan.FromSeconds(1.0f),
          };

          var pec = new ParticleEmitterComponent(go)
          {
            NumParticles = 64,

            TextureContentNames = new[]
            {
              "confetti/confetti_01",
              "confetti/confetti_02",
              "confetti/confetti_03",
              "confetti/confetti_04",
              "confetti/confetti_05",
              "confetti/confetti_06",
              "confetti/confetti_07",
            },

            AvailableColors = Global.AllConfettiColors,
          };

          pec.BeforePostInitialize += delegate ()
          {
            pec.Emitter.MaxTTL = 0.8f * (float)adc.DestructionDelay.TotalSeconds;
            pec.Emitter.MaxParticleSpread = 0.05f;
            pec.Emitter.MaxParticleSpeed = 5f;
            pec.Emit(go.GetWorldSpatialData().Position);
          };
        }
        break;

        case GameObjectType.Projectile:
        {
          var bc = new BodyComponent(go)
          {
            InitMode = BodyComponentInitMode.Manual,
          };
          bc.BeforePostInitialize += () =>
          {
            SpatialData s = go.GetWorldSpatialData();
            bc.Body = BodyFactory.CreateCapsule(
              world: Global.Game.World,
              endRadius: Global.ToMeters(9),
              height: Global.ToMeters(50),
              userData: bc,
              position: s.Position,
              rotation: s.Rotation.Radians + new Angle { Degrees = 90.0f + new Random().NextBilateralFloat() * 2 }.Radians,
              density: 10 * Global.OwliverDensity,
              bodyType: BodyType.Dynamic);
            bc.Body.CollisionCategories = Global.OwliverWeaponCollisionCategory;
            bc.Body.CollidesWith = ~(Global.OwliverCollisionCategory | Global.OwliverWeaponCollisionCategory);
            bc.Body.IsBullet = true;
            bc.Body.OnCollision += (fixtureA, fixtureB, contact) =>
            {
              Global.HandleDefaultHit(fixtureB.Body, bc.Body.Position, 1, 0.1f);

              Global.Game.RemoveGameObject(go);

              var confetti = CreateKnown(GameObjectType.DeathConfetti);
              confetti.Spatial.CopyFrom(go.Spatial);

              confetti.GetComponent<AutoDestructComponent>().DestructionDelay = TimeSpan.FromSeconds(0.25f);

              var confettiPec = confetti.GetComponent<ParticleEmitterComponent>();
              confettiPec.NumParticles = 16;
              Vector2 g = -2.5f * bc.Body.LinearVelocity;
              confettiPec.BeforePostInitialize += () =>
              {
                confettiPec.Emitter.Gravity = g;
              };

              Global.Game.AddGameObject(confetti);
            };
          };
          go.RootComponent = bc;

          var sac = new SpriteAnimationComponent(go)
          {
            AnimationTypes = new List<SpriteAnimationType>
            {
              SpriteAnimationType.Bonbon_Red,
            },
          };
          sac.Spatial.Rotation.Degrees -= 45.0f;
          sac.AttachTo(bc);

          var adc = new AutoDestructComponent(go);
        }
        break;

        case GameObjectType.BackgroundScreen:
        {
          go.Layer = GameLayer.Background;

          var bc = new BodyComponent(go)
          {
            InitMode = BodyComponentInitMode.FromContent,
            BodyType = BodyType.Static,
          };
          bc.BeforePostInitialize += () =>
          {
            bc.Body.CollisionCategories = Global.LevelCollisionCategory;
          };
          go.RootComponent = bc;

          var sc = new SpriteComponent(go)
          {
            DepthReference = null, // Don't determine depth automatically
            RenderDepth = 1.0f,
          };
          sc.AttachTo(bc);
        }
        break;

        case GameObjectType.Gate:
        {
          float outerLeft = Global.ToMeters(310);
          float innerLeft = Global.ToMeters(79);
          float inner = Global.ToMeters(80);
          float innerRight = Global.ToMeters(79);
          float outerRight = Global.ToMeters(222);
          float width = Global.ToMeters(768);
          float height = Global.ToMeters(128);
          float barrierHeight = Global.ToMeters(20);
          float density = Global.OwliverDensity;

          var leftEdge = new SpatialComponent(go);
          {
            SpriteAnimationData anim = SpriteAnimationFactory.GetAnimation(SpriteAnimationType.Gate_Closed);
            Vector2 hotspot = anim.Config.Hotspot * anim.Config.Scale;
            float offset = Global.ToMeters(hotspot.X - 128);
            leftEdge.Spatial.Position.X -= offset;
          }
          leftEdge.AttachTo(go);

          var bcInner = new BodyComponent(go)
          {
            InitMode = BodyComponentInitMode.Manual,
          };
          bcInner.BeforePostInitialize += () =>
          {
            SpatialData s = bcInner.GetWorldSpatialData();
            bcInner.Body = BodyFactory.CreateRectangle(
              world: Global.Game.World,
              width: innerLeft + inner + innerRight,
              height: barrierHeight,
              density: density,
              position: s.Position + new Vector2(outerLeft + innerLeft + 0.5f * inner, 0.0f),
              rotation: s.Rotation.Radians,
              bodyType: BodyType.Static,
              userData: bcInner);
          };
          bcInner.AttachTo(leftEdge);

          var bcOuter = new BodyComponent(go)
          {
            InitMode = BodyComponentInitMode.Manual,
          };
          bcOuter.BeforePostInitialize += () =>
          {
            SpatialData s = bcOuter.GetWorldSpatialData();
            bcOuter.Body = BodyFactory.CreateBody(
              world: Global.Game.World,
              position: s.Position,
              rotation: s.Rotation.Radians,
              bodyType: BodyType.Static,
              userData: bcOuter);

            Vector2 offsetRight = Global.ToMeters(300, 0);
            FixtureFactory.AttachRectangle(
              body: bcOuter.Body,
              width: outerLeft,
              height: barrierHeight,
              offset: new Vector2(0.5f * outerLeft, 0.0f),
              density: density,
              userData: bcOuter);
            FixtureFactory.AttachRectangle(
              body: bcOuter.Body,
              width: outerRight,
              height: barrierHeight,
              offset: new Vector2(width - 0.5f * outerRight, 0.0f),
              density: density,
              userData: bcOuter);
          };
          bcOuter.AttachTo(leftEdge);

          var sac = new SpriteAnimationComponent(go)
          {
            AnimationTypes = new List<SpriteAnimationType>
            {
               SpriteAnimationType.Gate_Closed,
               SpriteAnimationType.Gate_Open,
            },
          };
          sac.AttachTo(go);

          var gac = new GateComponent(go)
          {
            Dimensions = Global.ToMeters(60, 130),
            UnlockableBlockade = bcInner,
          };
          gac.Spatial.Position.Y -= Global.ToMeters((0.5f * 128) - 20);
          gac.AttachTo(go);
        }
        break;

        case GameObjectType.Tree_Fir:
        case GameObjectType.Tree_FirAlt:
        case GameObjectType.Tree_Conifer:
        case GameObjectType.Tree_ConiferAlt:
        case GameObjectType.Tree_Oak:
        case GameObjectType.Tree_Orange:
        case GameObjectType.Bush:
        {
          List<SpriteAnimationType> animTypes = new List<SpriteAnimationType>();
          switch(type)
          {
            case GameObjectType.Tree_Fir: animTypes.Add(SpriteAnimationType.Fir_Idle); break;
            case GameObjectType.Tree_FirAlt: animTypes.Add(SpriteAnimationType.FirAlt_Idle); go.Layer = GameLayer.CloseToTheScreen; break;
            case GameObjectType.Tree_Conifer: animTypes.Add(SpriteAnimationType.Conifer_Idle); break;
            case GameObjectType.Tree_ConiferAlt: animTypes.Add(SpriteAnimationType.ConiferAlt_Idle); go.Layer = GameLayer.CloseToTheScreen; break;
            case GameObjectType.Tree_Oak: animTypes.Add(SpriteAnimationType.Oak_Idle); break;
            case GameObjectType.Tree_Orange: animTypes.Add(SpriteAnimationType.Orange_Idle); break;
            case GameObjectType.Bush: animTypes.Add(SpriteAnimationType.Bush_Idle); break;

            default: throw new InvalidProgramException();
          }

          var sa = new SpriteAnimationComponent(go)
          {
            AnimationTypes = animTypes,
            DepthReference = null, // Don't determine depth automatically
          };
          sa.BeforePostInitialize += () =>
          {
            sa.RenderDepth = Global.Game.CalcDepth(sa.GetWorldSpatialData(), go.Layer);
          };
          sa.AttachTo(go);
        }
        break;

        case GameObjectType.Bonbon_Gold:
        case GameObjectType.Bonbon_Red:
        {
          SpriteAnimationType animType;
          switch(type)
          {
            case GameObjectType.Bonbon_Gold: animType = SpriteAnimationType.Bonbon_Gold; break;
            case GameObjectType.Bonbon_Red: animType = SpriteAnimationType.Bonbon_Red; break;
            default: throw new InvalidProgramException();
          }

          var bc = new BodyComponent(go)
          {
            InitMode = BodyComponentInitMode.Manual,
          };

          bc.BeforeInitialize += () =>
          {
            SpatialData s = bc.GetWorldSpatialData();
            bc.Body = BodyFactory.CreateCircle(
              world: Global.Game.World,
              radius: 0.2f,
              density: 0.5f * Global.OwliverDensity,
              position: s.Position,
              userData: bc);
            bc.Body.IsSensor = true;
            bc.Body.CollidesWith = Global.OwliverCollisionCategory;
          };

          go.RootComponent = bc;

          var sac = new SpriteAnimationComponent(go)
          {
            AnimationTypes = new List<SpriteAnimationType>
            {
              animType,
            }
          };
          sac.AttachTo(bc);

          var mbc = new MoneyBagComponent(go)
          {
            InitialAmount = 10,
          };

          var puc = new PickupComponent(go);

          var hoc = new HomingComponent(go)
          {
            TargetRange = 1.0f,
            Speed = 3.0f,

            DebugDrawingEnabled = true,
          };
          hoc.BeforeInitialize += () =>
          {
            hoc.Target = Global.Game.Owliver;
          };
          hoc.AttachTo(bc);
        }
        break;

        case GameObjectType.Key_Gold:
        {
          var bc = new BodyComponent(go)
          {
            InitMode = BodyComponentInitMode.Manual,
          };

          bc.BeforeInitialize += () =>
          {
            SpatialData s = bc.GetWorldSpatialData();
            bc.Body = BodyFactory.CreateCircle(
              world: Global.Game.World,
              radius: 0.2f,
              density: 2 * Global.OwliverDensity,
              position: s.Position,
              userData: bc);
            bc.Body.IsSensor = true;
            bc.Body.CollidesWith = Global.OwliverCollisionCategory;
          };

          go.RootComponent = bc;

          var sac = new SpriteAnimationComponent(go)
          {
            AnimationTypes = new List<SpriteAnimationType>
            {
              SpriteAnimationType.Key_Gold,
            }
          };
          sac.AttachTo(bc);

          var krc = new KeyRingComponent(go);
          krc.InitialKeyAmounts[(int)KeyType.Gold] = 1;

          var puc = new PickupComponent(go);

          var hoc = new HomingComponent(go)
          {
            TargetRange = 1.0f,
            Speed = 3.0f,

            DebugDrawingEnabled = true,
          };
          hoc.BeforeInitialize += () =>
          {
            hoc.Target = Global.Game.Owliver;
          };
          hoc.AttachTo(bc);
        }
        break;

        case GameObjectType.ShopItem_FruitBowl:
        case GameObjectType.ShopItem_FishingRod:
        case GameObjectType.ShopItem_Stick:
        {
          var bc = new BodyComponent(go)
          {
            InitMode = BodyComponentInitMode.Manual,
          };
          bc.BeforeInitialize += () =>
          {
            SpatialData s = bc.GetWorldSpatialData();
            bc.Body = BodyFactory.CreateBody(
              world: Global.Game.World,
              position: s.Position,
              rotation: s.Rotation.Radians,
              bodyType: BodyType.Static,
              userData: bc);
            FixtureFactory.AttachRectangle(
              body: bc.Body,
              width: Global.ToMeters(100),
              height: Global.ToMeters(100),
              offset: Global.ToMeters(0, 100),
              density: Global.OwliverDensity,
              userData: bc);
            bc.Body.IsSensor = true;
            bc.Body.CollidesWith = Global.OwliverCollisionCategory;
          };
          go.RootComponent = bc;

          var sacItem = new SpriteAnimationComponent(go)
          {
            DepthReference = null,
            RenderDepth = 0.1f,
            AnimationTypes = new List<SpriteAnimationType>(),
          };
          sacItem.AttachTo(bc);

#if true
          var sacPriceTag = new SpriteAnimationComponent(go)
          {
            DepthReference = null,
            RenderDepth = 0.11f,
            AnimationTypes = new List<SpriteAnimationType>
            {
              SpriteAnimationType.PriceTag_20,
              SpriteAnimationType.PriceTag_100,
            },
          };
          sacPriceTag.Spatial.Position.Y -= Global.ToMeters(10);
          sacPriceTag.AttachTo(sacItem);

          var sic = new ShopItemComponent(go)
          {
            PriceTag = sacPriceTag,
          };

          switch(type)
          {
            case GameObjectType.ShopItem_FruitBowl:
            {
              sacItem.AnimationTypes.Add(SpriteAnimationType.FruitBowl);
              sic.ItemType = ShopItemType.FruitBowl;
            }
            break;

            case GameObjectType.ShopItem_FishingRod:
            {
              sacItem.AnimationTypes.Add(SpriteAnimationType.FishingRod_Left);
              sic.ItemType = ShopItemType.FishingRod;
            }
            break;

            case GameObjectType.ShopItem_Stick:
            {
              sacItem.AnimationTypes.Add(SpriteAnimationType.Stick_Left);
              sic.ItemType = ShopItemType.Stick;
            }
            break;

            default: throw new ArgumentException(nameof(type));
          }
#else
          switch(type)
          {
            case GameObjectType.ShopItem_FruitBowl:
            {
              sacItem.AnimationTypes.Add(SpriteAnimationType.FruitBowl);
            }
            break;

            case GameObjectType.ShopItem_FishingRod:
            {
              sacItem.AnimationTypes.Add(SpriteAnimationType.FishingRod_Left);
            }
            break;

            case GameObjectType.ShopItem_Stick:
            {
              sacItem.AnimationTypes.Add(SpriteAnimationType.Stick_Left);
            }
            break;

            default: throw new ArgumentException(nameof(type));
          }
#endif
        }
        break;

        case GameObjectType.Random_FirTree:
        {
          GameObjectType choice = _random.Choose(GameObjectType.Tree_Fir, GameObjectType.Tree_Conifer);
          go = CreateKnown(choice);
        }
        break;

        case GameObjectType.Random_FirTreeAlt:
        {
          GameObjectType choice = _random.Choose(GameObjectType.Tree_FirAlt, GameObjectType.Tree_ConiferAlt);
          go = CreateKnown(choice);
        }
        break;

        case GameObjectType.Random_OakTree:
        {
          GameObjectType choice = _random.Choose(GameObjectType.Tree_Oak, GameObjectType.Tree_Orange);
          go = CreateKnown(choice);
        }
        break;

        default:
        throw new ArgumentException("Unknown game object type.");
      }

      go.Name = $"{type}({_knownCreationCount[(int)type]++})";

      return go;
    }
  }
}
