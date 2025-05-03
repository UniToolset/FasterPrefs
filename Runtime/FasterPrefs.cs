using System;
using System.Collections.Generic;
using System.Globalization;

namespace FasterPrefs
{
    /// <summary>
    /// Provides an optimized alternative to Unity's PlayerPrefs for storing key-value pairs persistently.
    /// Supports string, int, and float data types with improved performance through in-memory dictionaries.
    /// It Does not require a Save method as a separate thread worker picks up the changes and writes them to the disk.
    /// </summary>
    public class FasterPrefs
    {
        private readonly Dictionary<string, string> _stringValues = new();
        private readonly Dictionary<string, int> _intValues = new();
        private readonly Dictionary<string, float> _floatValues = new();
        private readonly Storage _storage;

        /// <summary>
        /// Initializes a new instance of FasterPrefs with a specified storage file.
        /// </summary>
        /// <param name="filePath">A full path to the file where data will be stored.</param>
        /// <param name="onErrorLogAction">Callback action for error logging.</param>
        public FasterPrefs(string filePath, Action<string> onErrorLogAction)
        {
            _storage = new Storage(filePath, onErrorLogAction);
            var entries = _storage.GetEntries();
            foreach (var entry in entries)
            {
                switch (entry.DataType)
                {
                    case Storage.DataType.String:
                        _stringValues[entry.Key] = entry.Value;
                        break;
                    case Storage.DataType.Int:
                        _intValues[entry.Key] = int.Parse(entry.Value);
                        break;
                    case Storage.DataType.Float:
                        _floatValues[entry.Key] = float.Parse(entry.Value, CultureInfo.InvariantCulture);
                        break;
                }
            }
        }
        
        /// <summary>
        /// Stores a string value associated with the specified key.
        /// </summary>
        /// <param name="key">The key to identify the value.</param>
        /// <param name="value">The string value to store.</param>
        public void SetValue(string key, string value)
        {
            _stringValues[key] = value;
            _storage.SetValue(key, Storage.DataType.String, value);
        }

        /// <summary>
        /// Retrieves a stored string value by its key.
        /// </summary>
        /// <param name="key">The key of the value to retrieve.</param>
        /// <param name="defaultValue">The value to return if the key doesn't exist.</param>
        /// <returns>The stored string value or the default value if the key is not found.</returns>
        public string GetString(string key, string defaultValue = null)
        {
            if (!_stringValues.ContainsKey(key)) return defaultValue;
            return _stringValues[key];
        }

        /// <summary>
        /// Stores an integer value associated with the specified key.
        /// </summary>
        /// <param name="key">The key to identify the value.</param>
        /// <param name="value">The integer value to store.</param>
        public void SetValue(string key, int value)
        {
            _intValues[key] = value;
            _storage.SetValue(key, Storage.DataType.Int, value.ToString());
        }

        /// <summary>
        /// Retrieves a stored integer value by its key.
        /// </summary>
        /// <param name="key">The key of the value to retrieve.</param>
        /// <param name="defaultValue">The value to return if the key doesn't exist.</param>
        /// <returns>The stored integer value or the default value if the key is not found.</returns>
        public int GetInt(string key, int defaultValue = 0)
        {
            if (!_intValues.ContainsKey(key)) return defaultValue;
            return _intValues[key];
        }

        /// <summary>
        /// Stores a float value associated with the specified key.
        /// </summary>
        /// <param name="key">The key to identify the value.</param>
        /// <param name="value">The float value to store.</param>
        public void SetValue(string key, float value)
        {
            _floatValues[key] = value;
            _storage.SetValue(key, Storage.DataType.Float, value.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Retrieves a stored float value by its key.
        /// </summary>
        /// <param name="key">The key of the value to retrieve.</param>
        /// <param name="defaultValue">The value to return if the key doesn't exist.</param>
        /// <returns>The stored float value or the default value if the key is not found.</returns>
        public float GetFloat(string key, float defaultValue = 0f)
        {
            if (!_floatValues.ContainsKey(key)) return defaultValue;
            return _floatValues[key];
        }

        /// <summary>
        /// Deletes a key-value pair of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of value (string, int, or float).</typeparam>
        /// <param name="key">The key to delete.</param>
        /// <exception cref="ArgumentException">Thrown when T is not a supported type.</exception>
        public void DeleteKey<T>(string key)
        {
            if (typeof(T) == typeof(string))
            {
                _stringValues.Remove(key);
                return;
            }

            if (typeof(T) == typeof(int))
            {
                _intValues.Remove(key);
                return;
            }

            if (typeof(T) == typeof(float))
            {
                _floatValues.Remove(key);
                return;
            }
            
            throw new ArgumentException($"Unsupported type: {typeof(T).Name}");
        }

        /// <summary>
        /// Checks if a key exists for the specified type.
        /// </summary>
        /// <typeparam name="T">The type of value (string, int, or float).</typeparam>
        /// <param name="key">The key to check.</param>
        /// <returns>True if the key exists, false otherwise.</returns>
        /// <exception cref="ArgumentException">Thrown when T is not a supported type.</exception>
       public bool HasKey<T>(string key)
       {
           if (typeof(T) == typeof(string))
               return _stringValues.ContainsKey(key);
           if (typeof(T) == typeof(int))
               return _intValues.ContainsKey(key);
           if (typeof(T) == typeof(float))
               return _floatValues.ContainsKey(key);
           
           throw new ArgumentException($"Unsupported type: {typeof(T).Name}");
       }
        
        /// <summary>
        /// Deletes all stored key-value pairs across all data types.
        /// </summary>
        public void DeleteAll()
        {
            _stringValues.Clear();
            _intValues.Clear();
            _floatValues.Clear();
            _storage.DeleteAll();
        }
    }
}
