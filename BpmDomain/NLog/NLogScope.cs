using Microsoft.Win32.SafeHandles;
using NLog;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.InteropServices;

namespace BpmDomain.NLog;

public sealed class NLogScope : IDisposable
{
    private readonly Logger _logger;
    private readonly string? currentMethodName;
    private bool disposed;
    private readonly SafeHandle handle;
    private readonly DateTime start;

    /// <summary>
    /// NLogScope
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="currentMethod"></param>
    public NLogScope(Logger logger, MethodBase? currentMethod)
    {
        var archivingLogEnabled = false;
        var archivingLogMegabyteLimit = 0;
        var archivingLogSelfCleaningEnabled = false;
        var archivingLogSelfCleaningAfterDays = 0;
        var mainDir = String.Empty;
        var mainLogFileName = String.Empty;
        var mainLogFullPath = String.Empty;

        handle = new SafeFileHandle(IntPtr.Zero, true);
        disposed = false;

        try
        {
            _ = bool.TryParse(logger.Factory?.Configuration?.Variables["ArchivingLog.Enabled"].ToString(), out archivingLogEnabled);
            _ = int.TryParse(logger.Factory?.Configuration?.Variables["ArchivingLog.MegabyteLimit"].ToString(), out archivingLogMegabyteLimit);
            _ = bool.TryParse(logger.Factory?.Configuration?.Variables["ArchivingLog.SelfCleaning.Enabled"].ToString(), out archivingLogSelfCleaningEnabled);
            _ = int.TryParse(logger.Factory?.Configuration?.Variables["ArchivingLog.SelfCleaning.AfterDays"].ToString(), out archivingLogSelfCleaningAfterDays);
            mainDir = logger.Factory?.Configuration?.Variables["mainDir"].ToString();
            mainLogFileName = logger.Factory?.Configuration?.Variables["mainLogFileName"].ToString();
            mainDir = mainDir?.Replace(@"\\", @"\");
            mainLogFileName = mainLogFileName?.Replace(@"\\", @"\");
            mainDir ??= String.Empty;
            mainLogFileName ??= String.Empty;
            mainLogFullPath = Path.Combine(mainDir, mainLogFileName);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "NLogScope: ");
        }

        if (archivingLogEnabled)
            ArchivingLogEnabled(mainLogFullPath, mainDir, mainLogFileName, logger, archivingLogMegabyteLimit);

        if (archivingLogSelfCleaningEnabled)
            ArchivingLogSelfCleaningEnabled(mainDir, archivingLogSelfCleaningAfterDays, logger);

        _logger = logger;
        currentMethodName = currentMethod?.Name;
        start = DateTime.Now;
        _logger.Info($"{currentMethodName} -> STARTED");
    }

    private static void ArchivingLogSelfCleaningEnabled(string? mainDir, int archivingLogSelfCleaningAfterDays, Logger logger)
    {
        if (Directory.Exists(mainDir))
        {
            var di = new DirectoryInfo(mainDir);

            foreach (FileInfo file in di.GetFiles())
            {
                var date1 = file.CreationTime;
                var date2 = DateTime.Now;
                var daysDiff = (date2 - date1).Days;

                if (file.Extension == ".zip" && daysDiff >= archivingLogSelfCleaningAfterDays)
                {
                    try
                    {
                        file.Delete();
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, "ArchivingLogSelfCleaningEnabled: ");
                    }
                }
            }
        }
    }

    private static void ArchivingLogEnabled(string mainLogFullPath, string? mainDir, string? mainLogFileName, Logger logger, int archivingLogMegabyteLimit)
    {
        if (File.Exists(mainLogFullPath))
        {
            var info = new FileInfo(mainLogFullPath);
            double size = info.Length / 1000000d;

            if (size > archivingLogMegabyteLimit)
            {
                var zipName = "backup_" + DateTime.Now.ToString("yyyyMMddHHmmss");

                try
                {
                    mainDir ??= String.Empty;
                    mainLogFileName ??= String.Empty;
                    using ZipArchive zip = ZipFile.Open(Path.Combine(mainDir, zipName + ".zip"), ZipArchiveMode.Create);
                    zip.CreateEntryFromFile(mainLogFullPath, Path.Combine(zipName, mainLogFileName));
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "ArchivingLogEnabled CreateEntryFromFile: ");
                }

                try
                {
                    File.WriteAllText(mainLogFullPath, String.Empty);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "ArchivingLogEnabled WriteAllText: ");
                }
            }
        }
    }

    /// <summary>
    /// Dispose
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);

        var end = DateTime.Now;
        TimeSpan span = end - start;

        var spanString = String.Format("{0} days and {1}:{2}:{3}.{4}",
            span.Days, span.Hours.ToString("00"), span.Minutes.ToString("00"), span.Seconds.ToString("00"), span.Milliseconds.ToString("0000"));

        _logger?.Info($"{currentMethodName} -> ENDED after {spanString}");
    }

    private void Dispose(bool disposing)
    {
        if (disposed)
            return;

        if (disposing)
        {
            handle.Dispose();
        }
        disposed = true;
    }

    /// <summary>
    /// Write Log how Error
    /// </summary>
    /// <param name="value"></param>
    public void Error(string value)
    {
        _logger?.Error(value);
    }

    /// <summary>
    /// Write Log how Warn
    /// </summary>
    /// <param name="value"></param>
    public void Warn(string value)
    {
        _logger?.Warn(value);
    }

    /// <summary>
    /// Write Log how Debug
    /// </summary>
    /// <param name="value"></param>
    public void Debug(string value)
    {
        _logger?.Debug(value);
    }

    /// <summary>
    /// Write Log how Info
    /// </summary>
    /// <param name="value"></param>
    public void Info(string value)
    {
        _logger?.Info(value);
    }

    /// <summary>
    /// Write Log how Trace
    /// </summary>
    /// <param name="value"></param>
    public void Trace(string value)
    {
        _logger?.Trace(value);
    }

    /// <summary>
    /// Write Log how Fatal
    /// </summary>
    /// <param name="value"></param>
    public void Fatal(string value)
    {
        _logger?.Fatal(value);
    }
}