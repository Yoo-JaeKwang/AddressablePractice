public struct DownloadProgressStatus
{
    public long TotalBytes;
    public long DownloadedBytes;
    public long RemainedBytes;
    public float TotalProgress; // 0 ~ 1

    public DownloadProgressStatus(long totalBytes, long downloadedBytes, long remainedBytes, float totalProgress)
    {
        TotalBytes = totalBytes;
        DownloadedBytes = downloadedBytes;
        RemainedBytes = remainedBytes;
        TotalProgress = totalProgress;
    }
}