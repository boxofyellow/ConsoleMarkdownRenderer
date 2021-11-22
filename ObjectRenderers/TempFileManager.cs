using System;
using System.Collections.Generic;
using System.IO;

namespace ConsoleMarkdownRenderer.ObjectRenderers
{
    /// <summary>
    /// Little class for collecting temp files, upon disposing the the temp files are deleted
    /// </summary>
    public class TempFileManager : IDisposable
    {
        public string GetTempFile()
        {
            var result = Path.GetTempFileName();
            m_files.Add(result);
            return result;
        }

        // internal for Testing
        internal int Count => m_files.Count;

        // infernal for testing
        internal bool Contains(string path) => m_files.Contains(path);

        public void Dispose()
        {
            foreach (var file in m_files)
            {
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
            }
            m_files.Clear();
        }

        private readonly HashSet<string> m_files = new();
    }
}