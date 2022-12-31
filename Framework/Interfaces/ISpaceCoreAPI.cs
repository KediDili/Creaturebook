using System;

namespace Creaturebook.Framework.Interfaces
{
    public interface ISpaceCoreAPI
    {
         /// Must have [XmlType("Mods_SOMETHINGHERE")] attribute (required to start with "Mods_")
         void RegisterSerializerType(Type type);
    }
}
