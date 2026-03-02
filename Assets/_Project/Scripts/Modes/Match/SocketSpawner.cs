using System.Collections.Generic;
using UnityEngine;

namespace BA.Modes.Match
{
    public class SocketSpawner : MonoBehaviour
    {
        [SerializeField] private MatchSocket prefab;
        [SerializeField] private Transform parent;
        [SerializeField] private Transform[] socketPoints;

        private readonly List<MatchSocket> sockets = new();
        public IReadOnlyList<MatchSocket> Sockets => sockets;

        public void BuildSockets(int count)
        {
            Clear();

            for (int i = 0; i < count; i++)
            {
                var p = socketPoints != null && socketPoints.Length > 0
                    ? socketPoints[i % socketPoints.Length]
                    : parent;

                var s = Instantiate(prefab, p.position, p.rotation, parent);
                s.SocketIndex = i;
                sockets.Add(s);
            }
        }

        private void Clear()
        {
            for (int i = 0; i < sockets.Count; i++)
                if (sockets[i]) Destroy(sockets[i].gameObject);
            sockets.Clear();
        }
    }
}