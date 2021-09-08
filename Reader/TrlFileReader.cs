using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using TmfLib.Pathable;

namespace TmfLib.Reader {
    public static class TrlFileReader {

        public static IEnumerable<ITrailSection> GetTrailSegments(byte[] trlData) {
            var trlDataStream = new MemoryStream(trlData);
            var trlReader     = new BinaryReader(trlDataStream, Encoding.ASCII);

            // If at end of stream, or if stream is 0 length, give up
            if (trlReader.PeekChar() == -1) yield break;

            // First four bytes are just 0000 to signify the first path section
            trlReader.ReadInt32();

            int mapId = trlReader.ReadInt32();

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
