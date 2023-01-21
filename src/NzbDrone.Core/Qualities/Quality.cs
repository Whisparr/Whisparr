using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Qualities
{
    public class Quality : IEmbeddedDocument, IEquatable<Quality>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public QualitySource Source { get; set; }
        public int Resolution { get; set; }

        public Quality()
        {
        }

        private Quality(int id, string name, QualitySource source, int resolution)
        {
            Id = id;
            Name = name;
            Source = source;
            Resolution = resolution;
        }

        public override string ToString()
        {
            return Name;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public bool Equals(Quality other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Id.Equals(other.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return Equals(obj as Quality);
        }

        public static bool operator ==(Quality left, Quality right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Quality left, Quality right)
        {
            return !Equals(left, right);
        }

        // Unable to determine
        public static Quality Unknown => new Quality(0, "Unknown", QualitySource.Unknown, 0);

        // SD
        public static Quality SDTV => new Quality(1, "SDTV", QualitySource.Television, 480);
        public static Quality DVD => new Quality(2, "DVD", QualitySource.DVD, 0);
        public static Quality DVDR => new Quality(23, "DVD-R", QualitySource.DVDRaw, 480); // new

        // HDTV
        public static Quality HDTV720p => new Quality(4, "HDTV-720p", QualitySource.Television, 720);
        public static Quality HDTV1080p => new Quality(9, "HDTV-1080p", QualitySource.Television, 1080);
        public static Quality HDTV2160p => new Quality(16, "HDTV-2160p", QualitySource.Television, 2160);

        // Web-DL
        public static Quality WEBDL480p => new Quality(8, "WEBDL-480p", QualitySource.Web, 480);
        public static Quality WEBDL720p => new Quality(5, "WEBDL-720p", QualitySource.Web, 720);
        public static Quality WEBDL1080p => new Quality(3, "WEBDL-1080p", QualitySource.Web, 1080);
        public static Quality WEBDL2160p => new Quality(18, "WEBDL-2160p", QualitySource.Web, 2160);

        // Bluray
        public static Quality Bluray480p => new Quality(20, "Bluray-480p", QualitySource.Bluray, 480); // new
        public static Quality Bluray576p => new Quality(21, "Bluray-576p", QualitySource.Bluray, 576); // new
        public static Quality Bluray720p => new Quality(6, "Bluray-720p", QualitySource.Bluray, 720);
        public static Quality Bluray1080p => new Quality(7, "Bluray-1080p", QualitySource.Bluray, 1080);
        public static Quality Bluray2160p => new Quality(19, "Bluray-2160p", QualitySource.Bluray, 2160);

        public static Quality Remux1080p => new Quality(30, "Remux-1080p", QualitySource.BlurayRaw, 1080);
        public static Quality Remux2160p => new Quality(31, "Remux-2160p", QualitySource.BlurayRaw, 2160);

        public static Quality BRDISK => new Quality(22, "BR-DISK", QualitySource.BlurayDisk, 1080); // new

        // Others
        public static Quality RAWHD => new Quality(10, "Raw-HD", QualitySource.TelevisionRaw, 1080);

        public static Quality WEBRip480p => new Quality(12, "WEBRip-480p", QualitySource.WebRip, 480);
        public static Quality WEBRip720p => new Quality(14, "WEBRip-720p", QualitySource.WebRip, 720);
        public static Quality WEBRip1080p => new Quality(15, "WEBRip-1080p", QualitySource.WebRip, 1080);
        public static Quality WEBRip2160p => new Quality(17, "WEBRip-2160p", QualitySource.WebRip, 2160);

        public static Quality VR => new Quality(22, "VR", QualitySource.VR, 1080);

        static Quality()
        {
            All = new List<Quality>
            {
                Unknown,
                SDTV,
                DVD,
                DVDR,
                HDTV720p,
                HDTV1080p,
                HDTV2160p,
                WEBDL480p,
                WEBDL720p,
                WEBDL1080p,
                WEBDL2160p,
                WEBRip480p,
                WEBRip720p,
                WEBRip1080p,
                WEBRip2160p,
                Bluray480p,
                Bluray576p,
                Bluray720p,
                Bluray1080p,
                Bluray2160p,
                Remux1080p,
                Remux2160p,
                BRDISK,
                RAWHD,
                VR
            };

            AllLookup = new Quality[All.Select(v => v.Id).Max() + 1];
            foreach (var quality in All)
            {
                AllLookup[quality.Id] = quality;
            }

            DefaultQualityDefinitions = new HashSet<QualityDefinition>
            {
                new QualityDefinition(Quality.Unknown)     { Weight = 1,  MinSize = 0, MaxSize = 100, PreferredSize = 95 },
                new QualityDefinition(Quality.SDTV)        { Weight = 8,  MinSize = 0, MaxSize = 100, PreferredSize = 95 },
                new QualityDefinition(Quality.DVD)         { Weight = 9,  MinSize = 0, MaxSize = 100, PreferredSize = 95 },
                new QualityDefinition(Quality.DVDR)        { Weight = 10,  MinSize = 0, MaxSize = 100, PreferredSize = 95 },

                new QualityDefinition(Quality.WEBDL480p)   { Weight = 11, MinSize = 0, MaxSize = 100, PreferredSize = 95, GroupName = "WEB 480p" },
                new QualityDefinition(Quality.WEBRip480p)   { Weight = 11, MinSize = 0, MaxSize = 100, PreferredSize = 95, GroupName = "WEB 480p" },
                new QualityDefinition(Quality.Bluray480p)  { Weight = 12, MinSize = 0, MaxSize = 100, PreferredSize = 95 },
                new QualityDefinition(Quality.Bluray576p)  { Weight = 13, MinSize = 0, MaxSize = 100, PreferredSize = 95 },

                new QualityDefinition(Quality.HDTV720p)    { Weight = 14, MinSize = 0, MaxSize = 100, PreferredSize = 95 },
                new QualityDefinition(Quality.WEBDL720p)   { Weight = 15, MinSize = 0, MaxSize = 100, PreferredSize = 95, GroupName = "WEB 720p" },
                new QualityDefinition(Quality.WEBRip720p)   { Weight = 15, MinSize = 0, MaxSize = 100, PreferredSize = 95, GroupName = "WEB 720p" },
                new QualityDefinition(Quality.Bluray720p)  { Weight = 16, MinSize = 0, MaxSize = 100, PreferredSize = 95 },

                new QualityDefinition(Quality.HDTV1080p)   { Weight = 17, MinSize = 0, MaxSize = 100, PreferredSize = 95 },
                new QualityDefinition(Quality.WEBDL1080p)  { Weight = 18, MinSize = 0, MaxSize = 100, PreferredSize = 95, GroupName = "WEB 1080p" },
                new QualityDefinition(Quality.WEBRip1080p)   { Weight = 18, MinSize = 0, MaxSize = 100, PreferredSize = 95, GroupName = "WEB 1080p" },
                new QualityDefinition(Quality.Bluray1080p) { Weight = 19, MinSize = 0, MaxSize = null, PreferredSize = null },
                new QualityDefinition(Quality.Remux1080p)  { Weight = 20, MinSize = 0, MaxSize = null, PreferredSize = null },

                new QualityDefinition(Quality.HDTV2160p)   { Weight = 21, MinSize = 0, MaxSize = null, PreferredSize = null },
                new QualityDefinition(Quality.WEBDL2160p)  { Weight = 22, MinSize = 0, MaxSize = null, PreferredSize = null, GroupName = "WEB 2160p" },
                new QualityDefinition(Quality.WEBRip2160p)  { Weight = 22, MinSize = 0, MaxSize = null, PreferredSize = null, GroupName = "WEB 2160p" },
                new QualityDefinition(Quality.Bluray2160p) { Weight = 23, MinSize = 0, MaxSize = null, PreferredSize = null },
                new QualityDefinition(Quality.Remux2160p)  { Weight = 24, MinSize = 0, MaxSize = null, PreferredSize = null },

                new QualityDefinition(Quality.BRDISK)      { Weight = 25, MinSize = 0, MaxSize = null, PreferredSize = null },
                new QualityDefinition(Quality.RAWHD)       { Weight = 26, MinSize = 0, MaxSize = null, PreferredSize = null },
                new QualityDefinition(Quality.VR)          { Weight = 27, MinSize = 4, MaxSize = null, PreferredSize = null },
            };
        }

        public static readonly List<Quality> All;

        public static readonly Quality[] AllLookup;

        public static readonly HashSet<QualityDefinition> DefaultQualityDefinitions;
        public static Quality FindById(int id)
        {
            if (id == 0)
            {
                return Unknown;
            }

            var quality = AllLookup[id];

            if (quality == null)
            {
                throw new ArgumentException("ID does not match a known quality", "id");
            }

            return quality;
        }

        public static explicit operator Quality(int id)
        {
            return FindById(id);
        }

        public static explicit operator int(Quality quality)
        {
            return quality.Id;
        }
    }
}
