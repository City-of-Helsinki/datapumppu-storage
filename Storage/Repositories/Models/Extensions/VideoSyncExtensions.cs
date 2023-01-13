namespace Storage.Repositories.Models.Extensions
{
    public static class VideoSyncExtensions
    {
        public static int GetVideoPosition(this VideoSync? videoSync, DateTime? startTime)
        {
            if (videoSync == null || videoSync?.Timestamp == null || videoSync?.VideoPosition == null)
            {
                return 0;
            }

            var timeDiff = (startTime - videoSync.Timestamp);
            if (timeDiff == null)
            {
                return 0;
            }

            return videoSync.VideoPosition.Value + (int)timeDiff.Value.TotalSeconds;
        }

    }
}
