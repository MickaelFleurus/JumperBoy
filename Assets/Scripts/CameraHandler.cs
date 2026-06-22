using UnityEngine;
using Unity.Cinemachine;
using System;
using Unity.VisualScripting;

public class CameraHandler : MonoBehaviour
{
    CinemachineCamera mainCamera;
    CinemachineFollow cameraFollow;
    [SerializeField] CameraTarget toFollow;
    [SerializeField] Player player;
    [SerializeField] float offset = 5.0f;

    float direction;
    void Awake()
    {
        mainCamera = GetComponent<CinemachineCamera>();
        cameraFollow = GetComponent<CinemachineFollow>();
        mainCamera.Target = toFollow;


        cameraFollow.FollowOffset.x = 0;
    }

    void Start()
    {
        PlayerInputs.Instance.inGameActions.Move += OnMove;
    }

    void Update()
    {
        float delta = offset * 0.5f * Time.deltaTime;
        cameraFollow.FollowOffset.x = Mathf.MoveTowards(cameraFollow.FollowOffset.x, direction, delta);
    }

    void OnMove(Vector2 move)
    {
        direction = offset * Math.Sign(move.x);
    }

}
