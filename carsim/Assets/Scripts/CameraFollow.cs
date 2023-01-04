using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Vector3 offset;
    [SerializeField] private Transform target;
    [SerializeField] private float translateSpeed;
    [SerializeField] private float rotationSpeed;

    private void FixedUpdate()
    {
        HandleTranslation();
        HandleRotation();
    }
   
	public void settransform(Transform t){
		target = t;
	}
	
    private void HandleTranslation()
    {
        var targetPosition = target.TransformPoint(offset);
        transform.position = target.TransformPoint(offset); //Vector3.Lerp(transform.position, targetPosition, translateSpeed * Time.deltaTime);
    }
    private void HandleRotation()
    {
        //var direction = target.position - transform.position;
        //var rotation = Quaternion.LookRotation(direction, Vector3.up);
        transform.rotation = target.rotation;
    }
}
