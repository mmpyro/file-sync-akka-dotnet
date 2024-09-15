using System;

namespace FileSync.Utils
{
    public class EpochDateTime
    {
        public static int UtcNow()
        {
            var t = DateTime.UtcNow - new DateTime(1970, 1, 1);
            return (int)t.TotalSeconds;
        }
    }
}
