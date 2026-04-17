using NLog;
using NLog.Targets;
using System.Diagnostics;
using System.Reflection;

namespace BpmDomain.NLog
{
    public static class NLogUtility
    {
        public static MethodBase? GetMethodToNLog(MethodBase? method)
        {
            MethodBase? _method = null;

            try
            {
                var memberType = method?.DeclaringType?.MemberType;
                var methodName = String.Empty;
                var fullClassName = String.Empty;
                var start = 0;
                var end = 0;

                if (memberType == MemberTypes.NestedType)
                {
                    var fullName = method?.DeclaringType?.FullName;

                    start = 0;
                    fullName ??= String.Empty;
                    end = fullName.IndexOf('+');
                    fullClassName = fullName[start..end];

                    start = fullName.IndexOf('<') + 1;
                    end = fullName.IndexOf('>');
                    methodName = fullName[start..end];

                    _method = new StackTrace()
                            .GetFrames()
                            .Select(frame => frame.GetMethod())
                            .FirstOrDefault(_ => _?.Name == methodName && _.DeclaringType?.FullName == fullClassName);
                }

                if (memberType != MemberTypes.NestedType || _method == null)
                {
                    _method = method;
                }
            }
            catch (Exception)
            {
                _method = method;
            }

            return _method;
        }

        /// <summary>
        /// Clear NLog log file
        /// </summary>
        /// <param name="targetName"></param>
        /// <param name="maxSizeInMB"></param>
        public static void ClearNLogFile(string targetName, int maxSizeInMB)
        {
            var fileTarget = (FileTarget?)LogManager.Configuration?.FindTargetByName(targetName);
            var logEventInfo = new LogEventInfo { TimeStamp = DateTime.Now };
            fileTarget ??= new FileTarget();
            string filePath = fileTarget.FileName.Render(logEventInfo).Replace(@"\\", @"\");
            if (File.Exists(filePath))
            {
                long length = new FileInfo(filePath).Length;

                if (ConvertBytesToMegabytes(length) > maxSizeInMB)
                {
                    var lines = new List<string>() { };
                    var _lines = new List<string>() { };
                    string? line;

                    var file = new StreamReader(filePath);

                    while ((line = file.ReadLine()) != null)
                    {
                        lines.Add(line);
                    }

                    file.Close();

                    int counter = lines.Count;

                    for (int i = counter / 2; i < counter; i++)
                    {
                        _lines.Add(lines.ToArray()[i]);
                    }

                    File.WriteAllLines(filePath, [.. _lines]);
                }
            }
        }

        public static double ConvertBytesToMegabytes(long bytes)
        {
            return (bytes / 1024f) / 1024f;
        }
    }
}