namespace SettingsUI.Helpers;

#nullable enable

public class ApplicationDataContainerHelper
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationDataContainerHelper"/> class.
    /// </summary>
    /// <param name="appData">The data store to interact with.</param>
    public ApplicationDataContainerHelper(ApplicationData appData)
    {
        AppData = appData ?? throw new ArgumentNullException(nameof(appData));
    }

    /// <summary>
    /// Gets the settings container.
    /// </summary>
    public ApplicationDataContainer Settings => AppData.LocalSettings;

    /// <summary>
    /// Gets the storage host.
    /// </summary>
    protected ApplicationData AppData { get; }

    /// <summary>
    /// Get a new instance using ApplicationData.Current.
    /// </summary>
    /// <returns>A new instance of ApplicationDataContainerHelper.</returns>
    public static ApplicationDataContainerHelper GetCurrent()
    {
        var appData = ApplicationData.Current;
        return new ApplicationDataContainerHelper(appData);
    }

    /// <summary>
    /// Determines whether a setting already exists.
    /// </summary>
    /// <param name="key">Key of the setting (that contains object).</param>
    /// <returns>True if a value exists.</returns>
    public bool KeyExists(string key)
    {
        return Settings.Values.ContainsKey(key);
    }

    /// <summary>
    ///  Save a setting locally on the device
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void Save<T>(string key, T? value)
    {
        Settings.Values[key] = value;
    }

    /// <summary>
    /// Retrieves a single item by its key.
    /// </summary>
    /// <typeparam name="T">Type of object retrieved.</typeparam>
    /// <param name="key">Key of the object.</param>
    /// <param name="default">Default value of the object.</param>
    /// <returns>The TValue object.</returns>
    public T? Read<T>(string key, T? @default = default)
    {
        if (Settings.Values.TryGetValue(key, out var valueObj))
        {
            return (T)Convert.ChangeType(valueObj, typeof(T));
        }

        return @default;
    }

    public bool TryRead<T>(string key, out T? value)
    {
        if (Settings.Values.TryGetValue(key, out var valueObj))
        {
            value = (T)Convert.ChangeType(valueObj, typeof(T));
            return true;
        }

        value = default;
        return false;
    }

    public bool TryDelete(string key)
    {
        return Settings.Values.Remove(key);
    }

    public void Clear()
    {
        Settings.Values.Clear();
    }

    /// <summary>
    /// Determines whether a setting already exists in composite.
    /// </summary>
    /// <param name="compositeKey">Key of the composite (that contains settings).</param>
    /// <param name="key">Key of the setting (that contains object).</param>
    /// <returns>True if a value exists.</returns>
    public bool KeyExists(string compositeKey, string key)
    {
        if (TryRead(compositeKey, out ApplicationDataCompositeValue? composite) && composite != null)
        {
            return composite.ContainsKey(key);
        }

        return false;
    }

    /// <summary>
    /// Attempts to retrieve a single item by its key in composite.
    /// </summary>
    /// <typeparam name="T">Type of object retrieved.</typeparam>
    /// <param name="compositeKey">Key of the composite (that contains settings).</param>
    /// <param name="key">Key of the object.</param>
    /// <param name="value">The value of the object retrieved.</param>
    /// <returns>The T object.</returns>
    public bool TryRead<T>(string compositeKey, string key, out T? value)
    {
        if (TryRead(compositeKey, out ApplicationDataCompositeValue? composite) && composite != null)
        {
            var compositeValue = (string)composite[key];
            if (compositeValue != null)
            {
                value = (T)Convert.ChangeType(compositeValue, typeof(T));
                return true;
            }
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Retrieves a single item by its key in composite.
    /// </summary>
    /// <typeparam name="T">Type of object retrieved.</typeparam>
    /// <param name="compositeKey">Key of the composite (that contains settings).</param>
    /// <param name="key">Key of the object.</param>
    /// <param name="default">Default value of the object.</param>
    /// <returns>The T object.</returns>
    public T? Read<T>(string compositeKey, string key, T? @default = default)
    {
        if (TryRead(compositeKey, out ApplicationDataCompositeValue? composite) && composite != null)
        {
            if (composite.TryGetValue(key, out var valueObj))
            {
                return (T)Convert.ChangeType(valueObj, typeof(T));
            }
        }

        return @default;
    }

    /// <summary>
    /// Saves a group of items by its key in a composite.
    /// This method should be considered for objects that do not exceed 8k bytes during the lifetime of the application
    /// and for groups of settings which need to be treated in an atomic way.
    /// </summary>
    /// <typeparam name="T">Type of object saved.</typeparam>
    /// <param name="compositeKey">Key of the composite (that contains settings).</param>
    /// <param name="values">Objects to save.</param>
    public void Save<T>(string compositeKey, IDictionary<string, T> values)
    {
        if (TryRead(compositeKey, out ApplicationDataCompositeValue? composite) && composite != null)
        {
            foreach (var setting in values)
            {
                if (composite.ContainsKey(setting.Key))
                {
                    composite[setting.Key] = setting.Value;
                }
                else
                {
                    composite.Add(setting.Key, setting.Value);
                }
            }
        }
        else
        {
            composite = new ApplicationDataCompositeValue();
            foreach (var setting in values)
            {
                composite.Add(setting.Key, setting.Value);
            }

            Settings.Values[compositeKey] = composite;
        }
    }

    /// <summary>
    /// Deletes a single item by its key in composite.
    /// </summary>
    /// <param name="compositeKey">Key of the composite (that contains settings).</param>
    /// <param name="key">Key of the object.</param>
    /// <returns>A boolean indicator of success.</returns>
    public bool TryDelete(string compositeKey, string key)
    {
        if (TryRead(compositeKey, out ApplicationDataCompositeValue? composite) && composite != null)
        {
            return composite.Remove(key);
        }

        return false;
    }
}
