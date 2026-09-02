using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace PuzzleFramework.Progression
{
    /// <summary>Single-writer local JSON persistence. Replacement failure preserves the old file.</summary>
    public sealed class JsonProgressSaveLoadService : IProgressSaveLoadService
    {
        public ProgressLoadResult Load(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return new ProgressLoadResult(ProgressLoadStatus.IoFailure, null, "Progress path is required.");
            string json;
            try { json = File.ReadAllText(path); }
            catch (FileNotFoundException) { return new ProgressLoadResult(ProgressLoadStatus.NotFound, null); }
            catch (DirectoryNotFoundException) { return new ProgressLoadResult(ProgressLoadStatus.NotFound, null); }
            catch (Exception e) { return new ProgressLoadResult(ProgressLoadStatus.IoFailure, null, e.Message); }

            try
            {
                if (string.IsNullOrWhiteSpace(json))
                    return new ProgressLoadResult(ProgressLoadStatus.InvalidData, null, "Progress file is empty.");
                PlayerProgressSnapshot snapshot = JsonUtility.FromJson<PlayerProgressSnapshot>(json);
                if (snapshot != null && snapshot.FormatVersion > 0 &&
                    snapshot.FormatVersion != PlayerProgressData.CurrentFormatVersion)
                    return new ProgressLoadResult(ProgressLoadStatus.UnsupportedVersion, null,
                        $"Unsupported progress version {snapshot.FormatVersion}.");
                if (!PlayerProgressData.TryRestore(snapshot, out PlayerProgressData progress, out string reason))
                    return new ProgressLoadResult(ProgressLoadStatus.InvalidData, null, reason);
                return new ProgressLoadResult(ProgressLoadStatus.Loaded, progress);
            }
            catch (Exception e) { return new ProgressLoadResult(ProgressLoadStatus.InvalidData, null, e.Message); }
        }

        public ProgressSaveResult Save(PlayerProgressData progress, string path)
        {
            if (progress == null || string.IsNullOrWhiteSpace(path))
                return new ProgressSaveResult(false, "Progress and a save path are required.");
            string temporaryPath = null;
            try
            {
                string fullPath = Path.GetFullPath(path);
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                temporaryPath = fullPath + "." + Guid.NewGuid().ToString("N") + ".tmp";
                byte[] json = Encoding.UTF8.GetBytes(JsonUtility.ToJson(progress.CaptureSnapshot(), true));
                using (FileStream stream = new(temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    stream.Write(json, 0, json.Length);
                    stream.Flush(flushToDisk: true);
                }
                // Never delete the destination first: an interrupted/failed write must retain it.
                if (File.Exists(fullPath)) File.Replace(temporaryPath, fullPath, null);
                else File.Move(temporaryPath, fullPath);
                return new ProgressSaveResult(true);
            }
            catch (Exception e) { return new ProgressSaveResult(false, $"Progress save failed: {e.Message}"); }
            finally
            {
                if (temporaryPath != null)
                {
                    try { File.Delete(temporaryPath); }
                    catch (IOException) { }
                    catch (UnauthorizedAccessException) { }
                }
            }
        }
    }
}
