using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Uploader.Models;

namespace Uploader.Managers.Front
{
    public static class ProgressManager
    {
        private static ConcurrentDictionary<Guid, FileContainer> progresses = new ConcurrentDictionary<Guid, FileContainer>();

        public static void RegisterProgress(FileContainer fileContainer)
        {
            progresses.TryAdd(fileContainer.ProgressToken, fileContainer);

            // Supprimer le suivi progress après 1j
            Task taskClean = Task.Run(() =>
            {
                Thread.Sleep(24 * 60 * 60 * 1000); // 1j
                FileContainer thisFileContainer;
                progresses.TryRemove(fileContainer.ProgressToken, out thisFileContainer);
            });
        }

        public static FileContainer GetFileContainerByToken(Guid progressToken)
        {
            FileContainer fileContainer;
            progresses.TryGetValue(progressToken, out fileContainer);

            if(fileContainer != null)
                fileContainer.LastTimeProgressRequested = DateTime.UtcNow;

            return fileContainer;
        }

        public static FileContainer GetFileContainerBySourceHash(string sourceHash)
        {
            FileContainer fileContainer = progresses.Values
                .Where(s => s.SourceFileItem.IpfsHash == sourceHash)
                .OrderByDescending(s => s.NumInstance)
                .FirstOrDefault();

            if(fileContainer != null)
                fileContainer.LastTimeProgressRequested = DateTime.UtcNow;
            
            return fileContainer;
        }

        public static FileContainer GetFileContainerByChildHash(string hash)
        {
            FileContainer fileContainer = progresses.Values.Where(s =>
                    s?.OverlayFileItem?.IpfsHash == hash ||
                    s?.SpriteVideoFileItem?.IpfsHash == hash ||
                    (s?.EncodedFileItems.Any(v => v.IpfsHash == hash)??false)
                )
                .OrderByDescending(s => s.NumInstance)
                .FirstOrDefault();

            if(fileContainer != null)
                fileContainer.LastTimeProgressRequested = DateTime.UtcNow;
            
            return fileContainer;
        }
    }
}