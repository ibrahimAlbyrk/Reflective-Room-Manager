using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Room.Data
{
    [CreateAssetMenu(fileName = "New Room Management Data", menuName = "REFLECTIVE/Room/Room Management Data", order = 0)]
    public class RoomManagementData_SO : ScriptableObject
    {
        [field:SerializeField] public RoomData_SO DefaultRoomData { get; private set; }
    }
}