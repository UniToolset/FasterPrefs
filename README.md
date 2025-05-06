# FasterPrefs
FasterPrefs is a high-performance alternative to Unity's PlayerPrefs that operates off the main thread. It provides a way to store and retrieve key-value pairs and saving it to file automatically without blocking the main game loop.

Comperison with Unity's PlayerPrefs (1000 operations, different values):

| Feature                | Unity PlayerPrefs | FasterPrefs         |
|------------------------|-------------------|---------------------|
| Set string value       | 273000 ticks      | 5619 ticks          |
| Has key                | 328000 ticks      | 5935 ticks          |
| Get string value       | 164899 ticks      | 4393 ticks          |

## Features
- Non-blocking operations (doesn't freeze the main thread)
- Fast in-memory access
- Automatic background saving
- Type-safe API
- Support for string, int, bool and float values
- Supports different type values for the same key

## Installation
1. Add the package to your Unity project through one of these methods:
   - Via Git URL in Package Manager: `https://github.com/yourusername/FasterPrefs.git`
   - Manual installation: Copy the `FasterPrefs/Runtime` folder into your project's Assets directory

## Quick Start

```csharp
// Initialize FasterPrefs with a storage file path
string filePath = Path.Combine(Application.persistentDataPath, "faster_prefs.dat");
var prefs = new FasterPrefs(filePath, Debug.LogError);

// Store values
prefs.SetValue("playerName", "John");
prefs.SetValue("playerScore", 100);
prefs.SetValue("playerHealth", 98.5f);

// Retrieve values (with optional default values)
string name = prefs.GetString("playerName", "Unknown");
int score = prefs.GetInt("playerScore", 0);
float health = prefs.GetFloat("playerHealth", 100f);

// Check if keys exist
bool hasName = prefs.HasKey<string>("playerName");
bool hasScore = prefs.HasKey<int>("playerScore");

// Delete specific keys
prefs.DeleteKey<string>("playerName");
prefs.DeleteKey<int>("playerScore");

// Delete all data
prefs.DeleteAll();
```

## Key Features Explained

### Automatic Background Saving
Unlike Unity's PlayerPrefs, FasterPrefs doesn't require explicit Save calls. All changes are automatically picked up by a background worker and saved to disk.

### Error Handling
FasterPrefs allows you to provide a custom logger for handling errors callback if you don't use default Debug.LogError:
```csharp
var prefs = new FasterPrefs(filePath, (error) => {
    MyLogger.LogError($"FasterPrefs error: {error}");
});
```

## Performance Considerations
- All disk operations happen on a background thread
- In-memory dictionary access provides fast read/write operations
- No synchronous disk I/O on the main thread

