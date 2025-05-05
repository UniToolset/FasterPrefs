using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FasterPrefs
{
    internal class Storage
    {
        private const string Separator = "|";
        private readonly List<Entry> _allEntries = new ();
        private readonly ConcurrentQueue<Entry> _cache = new ();
        private readonly string _mainFile;
        private readonly Action<string> _errorLogCallback;
        private readonly string _changesFile;
        
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public Storage(string storagePath, Action<string> errorLogCallback = null)
        {
            var dirPath = storagePath.Substring(0, storagePath.LastIndexOf(Path.DirectorySeparatorChar));
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            _mainFile = storagePath;
            _errorLogCallback = errorLogCallback;
            _changesFile = storagePath + ".changes";
            _allEntries = GetEntriesInternal();
            Task.Run(SaveChangesWorker);
        }
        
        //todo add Funcs to constructor to encode/decode data

        internal List<Entry> GetEntries()
        {
            return _allEntries;
        }

        private List<Entry> GetEntriesInternal()
        {
            if (!File.Exists(_mainFile))
            {
                return new List<Entry>();
            }
            using(TextReader reader = new StreamReader(_mainFile))
            {
                string line;
                var entries = new List<Entry>();
                while ((line = reader.ReadLine()) != null)
                {
                    var parts = line.Split(Separator);
                    if (parts.Length != 3) continue;

                    var entry = new Entry
                    {
                        Key = parts[0],
                        DataType = (DataType)Enum.Parse(typeof(DataType), parts[1]),
                        Value = parts[2]
                    };
                    if(entry.DataType == DataType.String)
                    {
                        DecodeString(ref entry.Value);
                    }
                    entries.Add(entry);
                }
                return entries;
            }
        }

        internal void SetValue(string key, DataType type, object value)
        {
            _cache.Enqueue(new Entry()
            {
                Key = key,
                DataType = type,
                Value = value.ToString()
            });
        }
        
        internal void DeleteKey(string key, DataType type)
        {
            _allEntries.RemoveAll(e => e.Key == key && e.DataType == type);
            _cache.Enqueue(new Entry()
            {
                Key = key,
                DataType = type,
                Value = null
            });
        }

        private async void SaveChangesWorker()
        {
            try
            {
                while (true)
                {
                    if (_cache.Count > 0)
                    {
                        await _semaphore.WaitAsync();
                        if(_cache.Count == 0) continue; //if DeleteAll was called
                        
                        while (_cache.TryDequeue(out Entry entry))
                        {
                            if(entry.Value == null) continue; //key deletion operation. Needed in entry to start file writing.
                            
                            var existingEntry = _allEntries.Find(e => e.Key == entry.Key);
                            if (existingEntry != null)
                            {
                                existingEntry.Value = entry.Value;
                                existingEntry.DataType = entry.DataType;
                            }
                            else
                            {
                                _allEntries.Add(entry);
                            }
                        }

                        using (var writer = new StreamWriter(_changesFile, true))
                        {
                            foreach (var entry in _allEntries)
                            {
                                if (entry.DataType == DataType.String)
                                {
                                    EncodeString(ref entry.Value);
                                }
                                await writer.WriteLineAsync(
                                    $"{entry.Key}{Separator}{entry.DataType}{Separator}{entry.Value}");
                            }
                        }

                        File.Delete(_mainFile);
                        File.Move(_changesFile, _mainFile);
                        File.Delete(_changesFile);
                        _semaphore.Release();
                    }

                    await Task.Delay(100);
                }
            }
            catch (Exception e)
            {
                _semaphore.Release();
                _errorLogCallback?.Invoke($"Error FasterPrefs during data save: {e.Message}");
            }
        }

        private void EncodeString(ref string entryValue)
        {
            if (string.IsNullOrEmpty(entryValue)) return;
            entryValue = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(entryValue));
        }
        
        private void DecodeString(ref string entryValue)
        {
            if (string.IsNullOrEmpty(entryValue)) return;
            entryValue = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(entryValue));
        }

        internal enum DataType
        {
            String,
            Int,
            Float,
            Bool
        }

        internal class Entry
        {
            public string Key;
            public string Value;
            public DataType DataType;
        }

        public async void DeleteAll()
        {
            await _semaphore.WaitAsync();
            try
            {
                _cache.Clear();
                _allEntries.Clear();
                File.Delete(_mainFile);
                File.Delete(_changesFile);
            }
            catch (Exception e)
            {
                _errorLogCallback?.Invoke($"Error FasterPrefs during data delete: {e.Message}");
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}