using UnityEditor;
using UnityEngine;

namespace CleanStateMachine
{
    public interface IContextMenuProvider
    {
        void AddItemsToMenu(GenericMenu menu, Vector2 graphMousePosition);
    }
}
