using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Eurovision.Dataset.Scraping;

internal class PolymorphicTypeResolver : DefaultJsonTypeInfoResolver
{
    private Assembly CurrentAssembly { get; set; }

    public PolymorphicTypeResolver()
    {
        CurrentAssembly = GetType().Assembly;
    }

    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        JsonTypeInfo jsonTypeInfo = base.GetTypeInfo(type, options);

        if (CurrentAssembly == type.Assembly)
        {
            IEnumerable<Type> derivedTypes = GetAllSubclassOf(type);

            if (derivedTypes.Any())
            {
                jsonTypeInfo.PolymorphismOptions = new JsonPolymorphismOptions
                {
                    IgnoreUnrecognizedTypeDiscriminators = true,
                    UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization
                };

                foreach (Type derivedType in derivedTypes)
                    jsonTypeInfo.PolymorphismOptions.DerivedTypes.Add(new JsonDerivedType(derivedType));
            }
        }

        return jsonTypeInfo;
    }

    public IEnumerable<Type> GetAllSubclassOf(Type parent)
    {
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            foreach (Type type in assembly.GetTypes())
                if (type.IsSubclassOf(parent))
                    yield return type;
    }
}
