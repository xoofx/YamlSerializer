using System;

namespace YamlSerializer
{
    interface IRehashableKey
    {
        event EventHandler Changed;
    }
}