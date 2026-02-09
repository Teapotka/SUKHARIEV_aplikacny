using System.Collections.Generic;
using BA.Data;
using UnityEngine;

    public class MatchAnswerTracker : MonoBehaviour
    {
        private SocketSpawner spawner;

        public void Bind(SocketSpawner socketSpawner) => spawner = socketSpawner;

        public void ClearPlacements()
        {
            if (spawner == null) return;
            var sockets = spawner.Sockets;
            for (int i = 0; i < sockets.Count; i++)
                sockets[i].Clear();
        }

        public List<ArtItemSO> GetPlacedItems()
        {
            var result = new List<ArtItemSO>();
            if (spawner == null) return result;

            var sockets = spawner.Sockets;
            for (int i = 0; i < sockets.Count; i++)
            {
                var item = sockets[i].PlacedItem;
                if (item != null) result.Add(item);
            }
            return result;
        }
    }
