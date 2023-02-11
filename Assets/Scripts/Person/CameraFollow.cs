using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Vector3 CameraOffset = new Vector3(0, 2, -1);
    public Quaternion CameraRotationOffset = new Quaternion(3, 0, 0, 5);
    public float CameraSpeed = 0.5f;

    private Transform _player;
    private Transform _transform;

    // Start is called before the first frame update
    void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player").transform;
        _transform = gameObject.transform;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        _transform.position = _player.position + CameraOffset;
        _transform.rotation = CameraRotationOffset;
    }
}
