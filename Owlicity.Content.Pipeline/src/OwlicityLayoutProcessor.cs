
#if DEBUG
#define DETECT_CLOSE_PIXELS
#endif

#define DIAGNOSTICS
#define PARALLEL

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Owlicity.Content.Pipeline
{
  /// <summary>
  /// This class will be instantiated by the XNA Framework Content Pipeline
  /// to apply custom processing to content data, converting an object of
  /// type TInput to TOutput. The input and output types may be the same if
  /// the processor wishes to alter data without changing its type.
  ///
  /// This should be part of a Content Pipeline Extension Library project.
  /// </summary>
  [ContentProcessor(DisplayName = "Layout Processor - Owlicity")]
  public class OwlicityLayoutProcessor : ContentProcessor<TextureContent, List<ScreenLayoutInfo>>
  {
    static Dictionary<Color, GameObjectType> _colorToTypeMap = new Dictionary<Color, GameObjectType>
    {
      { new Color(255,   0,   0), GameObjectType.Random_FirTree },
      { new Color(  0,   0, 255), GameObjectType.Random_OakTree },
      { new Color(255,   0, 255), GameObjectType.Random_FirTreeAlt },
      { new Color(255, 255,   0), GameObjectType.Bush },
    };

    struct JobInfo
    {
      public int rowOffset;
      public int rowCount;
      public List<ScreenLayoutInfo> infos;
      public List<string> warnings;
    }

    public override List<ScreenLayoutInfo> Process(TextureContent input, ContentProcessorContext context)
    {
      BitmapContent bitmap = input.Faces[0][0];
      byte[] bytes = bitmap.GetPixelData();

      Debug.Assert(bytes.Length % 4 == 0);

      int mapWidth = bitmap.Width;
      int mapHeight = bitmap.Height;
      int numJobs = Environment.ProcessorCount;
      int numLinesPerJob = mapHeight / numJobs;
      JobInfo[] jobInfos = new JobInfo[numJobs];
      for(int jobIndex = 0; jobIndex < numJobs; jobIndex++)
      {
        // Example: Assume numLinesPerJob = 10:
        jobInfos[jobIndex].rowOffset = jobIndex * numLinesPerJob; // 0, 10, 20, ...
        jobInfos[jobIndex].rowCount = numLinesPerJob; // 10, 10, 10, ...
        jobInfos[jobIndex].infos = new List<ScreenLayoutInfo>();
        jobInfos[jobIndex].warnings = new List<string>();
      }

      DateTime start = DateTime.Now;

#if PARALLEL
      Parallel.ForEach(Enumerable.Range(0, numJobs), (jobIndex) =>
#else
      for(int jobIndex = 0; jobIndex < numJobs; jobIndex++)
#endif
      {
        int numRowsToProcess = jobInfos[jobIndex].rowCount;
        for(int localRow = 0; localRow < numRowsToProcess; localRow++)
        {
          int row = localRow + jobInfos[jobIndex].rowOffset;
          ParseLayoutInfos(bytes, row, mapWidth, jobInfos[jobIndex].infos, jobInfos[jobIndex].warnings);
        }
      }
#if PARALLEL
      );
#endif

      int numRemainingJobs = mapHeight - (numLinesPerJob * numJobs);
      if(numRemainingJobs > 0)
      {
        Diagnostic(context.Logger, $"Parsing {numRemainingJobs} remaining job(s).");
        for(int row = mapHeight - numRemainingJobs; row < mapHeight; row++)
        {
          const int jobIndex = 0;
          ParseLayoutInfos(bytes, row, mapWidth, jobInfos[jobIndex].infos, jobInfos[jobIndex].warnings);
        }
      }

      TimeSpan duration = DateTime.Now - start;

      Diagnostic(context.Logger, $"Parsing pixel data took {duration.TotalSeconds.ToString("0.000")} seconds.");

      // For all following warnings.
      context.Logger.Indent();

      List<ScreenLayoutInfo> result = new List<ScreenLayoutInfo>();
      foreach(JobInfo jobResult in jobInfos)
      {
        result.AddRange(jobResult.infos);
        foreach(string warning in jobResult.warnings)
        {
          context.Logger.LogWarning(null, null, warning);
        }
      }

#if DETECT_CLOSE_PIXELS
      DetectClosePixels(context, result);
#endif

      context.Logger.Unindent();

      return result;
    }

    private void ParseLayoutInfos(byte[] bytes, int rowOffset, int width,
      List<ScreenLayoutInfo> infos, List<string> warnings)
    {
      int pixelOffset = rowOffset * width;
      uint[] data = new uint[width];
      for(int localPixelIndex = 0; localPixelIndex < data.Length; localPixelIndex++)
      {
        int x = localPixelIndex;
        int y = rowOffset;

        int pixelIndex = pixelOffset + localPixelIndex;
        int byteIndex = pixelIndex * 4;
        Color pixel = new Color(BitConverter.ToUInt32(bytes, byteIndex));

        if(pixel.A > 0)
        {
          uint hexColor = (uint)pixel.R << 24 | (uint)pixel.G << 16 | (uint)pixel.B << 8 | (uint)0xFF;
          GameObjectType objectType = GetGameObjectTypeFromColor(hexColor);
          if(objectType != GameObjectType.Unknown)
          {
            infos.Add(new ScreenLayoutInfo
            {
              ObjectType = objectType,
              Offset = new Vector2(x, y),
            });
          }
          else
          {
            warnings.Add($"Unknown layout color: {pixel} (hex: 0x{hexColor.ToString("X8")})");
          }
        }
      }
    }

    /// <param name="hexColor">Format: 0xRRGGBBAA</param>
    private GameObjectType GetGameObjectTypeFromColor(uint hexColor)
    {
      switch(hexColor)
      {
        case 0xFF0000FF: return GameObjectType.Random_FirTree;
        case 0x0000FFFF: return GameObjectType.Random_OakTree;
        case 0xFF00FFFF: return GameObjectType.Random_FirTreeAlt;
        case 0xFFFF00FF: return GameObjectType.Bush;
      }

      return GameObjectType.Unknown;
    }

    private void DetectClosePixels(ContentProcessorContext context, List<ScreenLayoutInfo> infos)
    {
      for(int infoIndex = 0; infoIndex < infos.Count; infoIndex++)
      {
        for(int otherIndex = infoIndex + 1; otherIndex < infos.Count; otherIndex++)
        {
          Vector2 a = infos[infoIndex].Offset;
          Vector2 b = infos[otherIndex].Offset;

          float distance = Vector2.Distance(a, b);
          if(distance <= 2)
          {
            context.Logger.LogWarning(null, null, $"Pixels are too close together (distance: {distance}): {a} <-> {b}");
          }
        }
      }
    }

    void Diagnostic(ContentBuildLogger logger, string message)
    {
#if DIAGNOSTICS
      logger.LogMessage(message);
#endif
    }
  }
}