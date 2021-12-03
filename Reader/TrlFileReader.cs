using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using TmfLib.Pathable;

namespace TmfLib.Reader {
    public static class TrlFileReader {

        private static (BinaryReader TrlReader, int Version, int MapId) BeginTrailRead(byte[] trlData) {
            var trlDataStream = new MemoryStream(trlData);
            var trlReader     = new BinaryReader(trlDataStream, Encoding.ASCII);

            // If at end of stream, or if stream is 0 length, give up.
            if (trlReader.PeekChar() == -1) return (null, -1, -1);

            // First four bytes indicate the version of the format.
            int version = trlReader.ReadInt32();
            
            // Next four bytes indicate the map that the trail was recorded on.
            int mapId = trlReader.ReadInt32();

            return (trlReader, version, mapId);
        } 

        /// <summary>
        /// Peeks the trail's associated map ID in the trl's header without parsing the entire trail.
        /// </summary>
        public static int GetTrailMap(byte[] trlData) {
            return BeginTrailRead(trlData).MapId;
        }
        
        public static IEnumerable<ITrailSection> GetTrailSegments(byte[] trlData) {
            var (trlReader, version, mapId) = BeginTrailRead(trlData);

            if (version != 0) {
                // We only support (and are aware of) v0 of the format.
                yield break;
            }

            var trailPoints = new List<Vector3>();

            while (trlReader.PeekChar() != -1) {
                float x = trlReader.ReadSingle();
                float z = trlReader.ReadSingle();
                float y = trlReader.ReadSingle();

                if (z == 0 && x == 0 && y == 0) {
                    yield return new TrailSection(mapId, trailPoints);
                    trailPoints.Clear();
                } else {
                    trailPoints.Add(new Vector3(x, y, z));
                }
            }

            if (trailPoints.Any()) {
                // Record the last trail segment
                yield return new TrailSection(mapId, trailPoints);
            }

        }

    }
}
