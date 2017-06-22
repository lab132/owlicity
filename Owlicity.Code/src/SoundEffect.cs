using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Owlicity
{
  public enum SoundEffectType
  {
    Invalid,
    OwliverWalking,
  }

  public static class SoundEffectFactory
  {
    private static SoundEffect[] _known;
    private static ContentManager _content;

    public static void Initialize(ContentManager content)
    {
      if (content != null)
      {
        _content = content;
        int numSoundEffects = Enum.GetNames(typeof(SoundEffectType)).Length;
        _known = new SoundEffect[numSoundEffects];
      }
    }

    public static void PreLoadSoundContent(List<SoundEffectType> soundEffects)
    {
      foreach (SoundEffectType effect in soundEffects)
      {
        GetSoundEffect(effect);
      }
    }
    
    public static SoundEffect GetSoundEffect(SoundEffectType effectType)
    {
      Debug.Assert(_content != null, "Not initialized.");

      SoundEffect result = _known[(int)effectType];
      if (result == null)
      {
        string soundEffectName;
        switch (effectType)
        {
          case SoundEffectType.OwliverWalking:
            soundEffectName = "snd/walking_grass";
          break;
          default: throw new ArgumentException("Unknown audio effect type.");
        }

        result = _content.Load<SoundEffect>(soundEffectName);
        _known[(int)effectType] = result;
      }
      return result;
    }


  }

}
