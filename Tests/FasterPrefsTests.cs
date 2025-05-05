using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace FasterPrefs.Tests
{
    [TestFixture]
    public class FasterPrefsTests
    {
        private string _testFilePath;
        private FasterPrefs _prefs;

        [SetUp]
        public void Setup()
        {
            _testFilePath = TestHelper.GetTemporaryFilePath();
            _prefs = new FasterPrefs(_testFilePath, (error) => System.Console.WriteLine(error));
        }

        [TearDown]
        public void Cleanup()
        {
            _prefs.DeleteAll();
            TestHelper.DeleteFileIfExists(_testFilePath);
        }

        [Test]
        public async Task FileExists()
        {
            const string key = "test_string";
            const string value = "Hello World";
            
            _prefs.SetValue(key, value);
            await Task.Delay(200); 
            
            Assert.That(File.Exists(_testFilePath), Is.True);
        }

        [Test]
        public async Task MultipleWrites_LastValueInFile()
        {
            const string key = "test_string";
            const string value1 = "Hello World";
            const string value2 = "Hello World 2";
            const string value3 = "Hello World 3";
            
            _prefs.SetValue(key, value1);
            _prefs.SetValue(key, value2);
            _prefs.SetValue(key, value3);
            
            await Task.Delay(200);

            var testValue = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(value3));
            var lines = File.ReadAllLines(_testFilePath);
            
            Assert.That(lines.Count(x=>x.Contains(testValue)), Is.EqualTo(1));
        }

        [Test]
        public void StringValue_SetAndGet_ReturnsCorrectValue()
        {
            const string key = "test_string";
            const string value = "Hello World";
            
            _prefs.SetValue(key, value);
            string result = _prefs.GetString(key);
            
            Assert.That(result, Is.EqualTo(value));
        }

        [Test]
        public void IntValue_SetAndGet_ReturnsCorrectValue()
        {
            const string key = "test_int";
            const int value = 42;
            
            _prefs.SetValue(key, value);
            int result = _prefs.GetInt(key);
            
            Assert.That(result, Is.EqualTo(value));
        }

        [Test]
        public void FloatValue_SetAndGet_ReturnsCorrectValue()
        {
            const string key = "test_float";
            const float value = 3.14f;
            
            _prefs.SetValue(key, value);
            float result = _prefs.GetFloat(key);
            
            Assert.That(result, Is.EqualTo(value));
        }

        [Test]
        public void BoolValue_SetAndGet_ReturnsCorrectValue()
        {
            const string key = "test_bool";
            const bool value = true;
            
            _prefs.SetValue(key, value);
            bool result = _prefs.GetBool(key);
            
            Assert.That(result, Is.EqualTo(value));
        }

        [Test]
        public void DeleteKey_RemovesValue()
        {
            const string key = "test_delete";
            _prefs.SetValue(key, "test");
            
            _prefs.DeleteKey<string>(key);
            string result = _prefs.GetString(key, "default");
            
            Assert.That(result, Is.EqualTo("default"));
        }

        [Test]
        public void HasKey_ReturnsCorrectValue()
        {
            const string key = "test_exists";
            _prefs.SetValue(key, "test");
            
            bool exists = _prefs.HasKey<string>(key);
            bool notExists = _prefs.HasKey<string>("nonexistent");
            
            Assert.That(exists, Is.True);
            Assert.That(notExists, Is.False);
        }

        [Test]
        public void DeleteAll_RemovesAllValues()
        {
            _prefs.SetValue("str", "test");
            _prefs.SetValue("int", 1);
            _prefs.SetValue("float", 1.0f);
            _prefs.SetValue("bool", true);
            
            _prefs.DeleteAll();
            
            Assert.Multiple(() =>
            {
                Assert.That(_prefs.HasKey<string>("str"), Is.False);
                Assert.That(_prefs.HasKey<int>("int"), Is.False);
                Assert.That(_prefs.HasKey<float>("float"), Is.False);
                Assert.That(_prefs.HasKey<bool>("bool"), Is.False);
            });
        }

        [Test]
        public void GetDefaultValue_WhenKeyNotExists_ReturnsDefault()
        {
            Assert.Multiple(() =>
            {
                Assert.That(_prefs.GetString("nonexistent", "default"), Is.EqualTo("default"));
                Assert.That(_prefs.GetInt("nonexistent", 42), Is.EqualTo(42));
                Assert.That(_prefs.GetFloat("nonexistent", 3.14f), Is.EqualTo(3.14f));
                Assert.That(_prefs.GetBool("nonexistent", true), Is.True);
            });
        }
    }
}
