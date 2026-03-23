using UnityEngine;

public class SocketPuzzleReward : MonoBehaviour
{
    [Header("Color Sockets")]
    [SerializeField] private ColorSocket redSocket;
    [SerializeField] private ColorSocket greenSocket;
    [SerializeField] private ColorSocket blueSocket;

    [Header("Reward")]
    [SerializeField] private GameObject rewardObject;

    private void Update()
    {
        if (!rewardObject) return;

        bool allSocketsCorrect =
            redSocket && greenSocket && blueSocket &&
            redSocket.IsCorrectOccupied() &&
            greenSocket.IsCorrectOccupied() &&
            blueSocket.IsCorrectOccupied();

        rewardObject.SetActive(allSocketsCorrect);
    }
}
