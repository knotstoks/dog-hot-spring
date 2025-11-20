using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BroccoliBunnyStudios.Save
{
    public class SaveConfigDb
    {
        private readonly string _path;
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        private Dictionary<string, object> _dictionary;

        public SaveConfigDb(string path)
        {
            this._path = path;
            this._jsonSerializerSettings = new JsonSerializerSettings
            {
                DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind
            };
            this.Load();
        }

        public bool Load()
        {
            if (!string.IsNullOrEmpty(this._path))
            {
                if (File.Exists(this._path))
                {
                    StreamReader reader = null;
                    try
                    {
                        reader = new StreamReader(this._path, false);

                        var data = reader.ReadToEnd();
                        this.Deserialize(data);
                    }
                    catch (JsonReaderException)
                    {
                        this._dictionary = new Dictionary<string, object>();
                    }
                    finally
                    {
                        reader?.Close();
                    }
                }

                this._dictionary ??= new Dictionary<string, object>();
                return true;
            }

            return false;
        }

        public void Save()
        {
            var writer = new StreamWriter(this._path, false);
            writer.Write(JsonConvert.SerializeObject(this._dictionary, this._jsonSerializerSettings));
            writer.Close();
        }

        private void Deserialize(string data)
        {
            this._dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(data);
            this._dictionary ??= new Dictionary<string, object>();
        }

        #region Setters and Getters
        public void SetBool(string key, bool isOn)
        {
            this.SetValue(key, isOn);
        }

        public void SetInt(string key, int val)
        {
            this.SetValue(key, val);
        }

        public void SetFloat(string key, float val)
        {
            this.SetValue(key, val);
        }

        public void SetString(string key, string val)
        {
            this.SetValue(key, val);
        }

        public void SetLong(string key, long val)
        {
            this.SetValue(key, val);
        }

        public void SetDateTime(string key, DateTime val)
        {
            this.SetValue(key, val);
        }

        public void SetCollection<Tc, Ti>(string key, Tc val)
            where Tc : ICollection<Ti>
        {
            this.SetValue(key, val);
        }

        public void SetStruct<T>(string key, T val) where T : struct
        {
            this.SetValue<T>(key, val);
        }

        public bool GetBool(string key, bool defaultValue)
        {
            return Convert.ToBoolean(this.GetValue(key, defaultValue), CultureInfo.InvariantCulture);
        }

        public int GetInt(string key, int defaultValue)
        {
            return Convert.ToInt32(this.GetValue(key, defaultValue), CultureInfo.InvariantCulture);
        }

        public float GetFloat(string key, float defaultValue)
        {
            return Convert.ToSingle(this.GetValue(key, defaultValue), CultureInfo.InvariantCulture);
        }

        public string GetString(string key, string defaultValue)
        {
            return Convert.ToString(this.GetValue(key, defaultValue), CultureInfo.InvariantCulture);
        }

        public long GetLong(string key, long defaultValue)
        {
            return Convert.ToInt64(this.GetValue(key, defaultValue), CultureInfo.InvariantCulture);
        }

        public DateTime GetDateTime(string key, DateTime defaultValue)
        {
            return Convert.ToDateTime(this.GetValue(key, defaultValue), CultureInfo.InvariantCulture);
        }

        public Tc GetCollection<Tc, Ti>(string key, Tc defaultValue)
            where Tc : ICollection<Ti>
        {
            var obj = this.GetValue<Tc>(key, default);

            if (obj is JArray arr)
            {
                return arr.ToObject<Tc>();
            }

            if (obj?.GetType() == typeof(Tc))
            {
                return (Tc)obj;
            }

            return defaultValue;
        }

        public T GetStruct<T>(string key, T defaultValue) where T : struct
        {
            var obj = this.GetValue(key, defaultValue);

            if (obj is JObject jobj)
            {
                return jobj.ToObject<T>();
            }

            if (obj?.GetType() == typeof(T))
            {
                return (T)obj;
            }

            return defaultValue;
        }

        private void SetValue<T>(string key, T val)
        {
            if (!string.IsNullOrEmpty(this._path))
            {
                this._dictionary[key] = val;
                this.Save();
            }
        }

        private object GetValue<T>(string key, T defaultValue)
        {
            return this._dictionary.TryGetValue(key, out var value) ? value : defaultValue;
        }
        #endregion
    }
}