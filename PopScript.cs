using UnityEngine;
using System.Collections;

public class PopScript : MonoBehaviour 
{
	// Private
	private bool inMotion;
	private float timer;
	private float moveSpeed;
	private float rotationSpeed;
	private Vector3 startPosition;
	private EDirection direction;
	private Vector3 rotationVector;
	private float h;
	private float k;
	
	
	void Awake()
	{
		inMotion = false;
		timer = 0f;
		moveSpeed = Random.Range(CONSTANTS.MIN_POP_MOVE_SPEED, CONSTANTS.MAX_POP_MOVE_SPEED);
		rotationSpeed = Random.Range(CONSTANTS.MIN_POP_ROTATION_SPEED, CONSTANTS.MAX_POP_ROTATION_SPEED); 
		startPosition = transform.position;
		rotationVector = Random.onUnitSphere;
		h = Random.Range(CONSTANTS.MIN_POP_HK * Screen.width, CONSTANTS.MAX_POP_HK * Screen.width);
		k = Random.Range(CONSTANTS.MIN_POP_HK * Screen.height, CONSTANTS.MAX_POP_HK * Screen.height);
	}
	
	
	void Start() 
	{
		
	}
	
	
	void Update() 
	{
		if (inMotion)
			UpdatePop();	
	}
	
	
	public void ActivatePop(EDirection d)
	{
		inMotion = true;
		direction = d;
		int sign;
		
		if (Random.Range(0, 2) == 0)
			sign = -1;
		else
			sign = 1;
		
		if (direction == EDirection.Right)
		{
			h *= -1;
			k *= sign;
		}
		else if (direction == EDirection.Left)
		{
			//h *= 1;
			k *= sign;
		}
		else if (direction == EDirection.Up)
		{
			h *= sign;
			k *= -1;
		}
		else if (direction == EDirection.Down)
		{
			h *= sign; 
			//k *= 1;
		}
	}
	
	
	private void UpdatePop()
	{
		timer += Time.deltaTime * moveSpeed;
		
		float x = 0f;
		float y = 0f;
		
		if (direction == EDirection.Up || direction == EDirection.Down)
		{
			x = timer;
			
			if (h < 0)
				x *= -1;
			
			y = (-k / (h * h)) * (x - h) * (x - h) + k;
		}
		
		if (direction == EDirection.Left || direction == EDirection.Right)
		{
			y = timer;
			
			if (k < 0)
				y *= -1;
			
			x = (-h / (k * k)) * (y - k) * (y - k) + h;
		}
		
		transform.position = startPosition + new Vector3(x, y, 0f);
		transform.RotateAround(rotationVector, Time.deltaTime * rotationSpeed); // obsolete but changing it makes popped cubes not rotate.
		
		// See if the transform is offscreen and can be destroyed from the hierarchy.
		if (OffScreen())
        	Destroy(this.gameObject);
	}
	


	private bool OffScreen()
	{
		return Mathf.Abs(transform.position.x) > Screen.width || Mathf.Abs(transform.position.y) > Screen.height;
	}
	

	
	
}
