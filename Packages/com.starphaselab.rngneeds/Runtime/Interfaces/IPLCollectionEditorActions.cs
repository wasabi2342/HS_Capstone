using System;

namespace RNGNeeds
{
    [Obsolete("Used only by RNGNeeds PLCollection Drawer")]
    public interface IPLCollectionEditorActions
    {
        void AddList(string name = "");
        bool RemoveList(int index);
        void ClearCollection();
        public (bool ListFound, bool IsEmpty)  IsListEmpty(int index);
        bool MoveListUp(int index);
        bool MoveListDown(int index);
        Type ItemType();
    }
}