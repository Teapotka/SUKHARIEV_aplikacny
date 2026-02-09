using BA.Data;
using UnityEngine;

    public class MatchSocket : MonoBehaviour
    {
    [SerializeField] private Transform snapPoint;

    public int SocketIndex { get; set; }

    public ArtItemSO PlacedItem { get; private set; }
    public ArtCardView PlacedCard { get; private set; }

    public Transform SnapPoint => snapPoint != null ? snapPoint : transform;
    public bool IsOccupied => PlacedCard != null;

    [SerializeField] private Vector3 positionOffsetLocal = new Vector3(-0.1f, 0f, 0f);
    [SerializeField] private Vector3 rotationOffsetEuler = new Vector3(-90f, 180f, 0f);

    public bool TryPlace(ArtCardView card)
    {
        if (card == null) return false;
        if (IsOccupied) return false;

        PlacedCard = card;
        PlacedItem = card.Item;

        var sp = SnapPoint;

        card.transform.position = sp.TransformPoint(positionOffsetLocal);
        card.transform.rotation = sp.rotation * Quaternion.Euler(rotationOffsetEuler);

        return true;
    }

    public void Clear()
        {
            PlacedItem = null;
            PlacedCard = null;
        }
    }
