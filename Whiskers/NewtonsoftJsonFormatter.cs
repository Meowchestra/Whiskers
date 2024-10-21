/*
 * Copyright(c) 2024 HavenDV
 * Licensed under the MIT license. See https://github.com/HavenDV/H.Pipes/blob/master/LICENSE.txt for full license information.
 */

using System.Text;
using H.Formatters;
using Newtonsoft.Json;

namespace Whiskers;

public class NewtonsoftJsonFormatter : FormatterBase
{
    /// <summary>
    /// A formatter that uses <see cref="JsonConvert"/> inside for serialization/deserialization
    /// </summary>
    public Encoding Encoding { get; set; } = Encoding.UTF8;

    protected override byte[] SerializeInternal(object obj)
    {
        var json = JsonConvert.SerializeObject(obj);
        var bytes = Encoding.GetBytes(json);

        return bytes;
    }

    protected override T DeserializeInternal<T>(byte[] bytes)
    {
        var json = Encoding.GetString(bytes);
        var obj =
            JsonConvert.DeserializeObject<T>(json) ??
            throw new InvalidOperationException("Deserialized object is null.");

        return obj;
    }
}