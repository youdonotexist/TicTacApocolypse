using UnityEngine;
using System.Collections;

public class Unit : MonoBehaviour
{
	public enum Team {
		RED,
		BLUE,
		NEUTRAL
	};
	
	public Team team;
	public HomeBase homeBase;
	
	public string AnimationPrefix = "Red";
	exSpriteAnimation animation = null;
	exSprite unitSprite = null;
	
	Path path;
	bool shouldFollowPath;
	
	Node goToPoint;
	Node fromPoint;
	
	public bool destroyNextTurn = false;
	
	public float moveDuration = 20.0f;
	public float moveElapsed = 0.0f;
	
	public float speed = 2.0f;
	
	public float normalSpeed = 4.0f;
	public float slowSpeed = 2.0f;
	public float fastSpeed = 5.0f;
	
	public AudioClip DeathAudio;
	public AudioClip HitAudio;
	public AudioClip SlowDownAudio;
	
	Vector3 lastDirection;
	
	// Use this for initialization
	void Start ()
	{
		speed = normalSpeed;
		animation = GetComponent<exSpriteAnimation>();
		unitSprite = GetComponent<exSprite>();
	}
	
	void FixedUpdate ()
	{
		if (destroyNextTurn && gameObject != null) {
			//Destroy (gameObject);
			return;
		}
		
		if (shouldFollowPath) {
			if (goToPoint == null && path.AtEnd() == false) {
				goToPoint = path.Begin();
				transform.position = goToPoint.transform.position;
				
				fromPoint = goToPoint;
				goToPoint = path.Next();
			}
			
			if (goToPoint != null) { //Follow to the next node
				float dist = Vector3.Distance(transform.position, goToPoint.transform.position);
				if (dist >= 1.1f) {
					lastDirection = (goToPoint.transform.position - transform.position).normalized;
					transform.position =  transform.position + (lastDirection * speed * Time.fixedDeltaTime);
					
					//For visibility
					Vector3 pos = transform.position;
					pos.z = -1.0f;
					transform.position = pos;
					
					UpdateAnimation();
				}
				else {
					fromPoint = goToPoint;
					goToPoint = path.Next();
					moveElapsed = 0.0f;
					
					if (path.AtEnd()) {
						Vector3 pos = transform.position;
						pos.z = -1.0f;
						transform.position = pos;
						
						//StartCoroutine(MoveToCell());
					}
				}
			}
			else {
				if (path.AtEnd()) {
					Vector3 pos = transform.position;
						pos.z = -1.0f;
						transform.position = pos;
					
					StartCoroutine(MoveToCell());
				}	
			}
			moveElapsed += Time.deltaTime;
		}
	}
	
	public void FollowPath(Path p) {
		path = p;
		shouldFollowPath = true;
	}
	
	IEnumerator MoveToCell() {
		collider.enabled = false;
		
		float moveTime = 1.0f;
		float moveElapsed = 0.0f;
		Transform t = transform;
		
		Vector3 startPos = t.position;
		Vector3 endPos = path.cell.transform.position;
		
		exSprite sprite = GetComponent<exSprite>();
		Color startColor = sprite.color;
		Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0.0f);
		
		while (moveElapsed < moveTime) {
			t.position = Vector3.Lerp (startPos, endPos, moveElapsed/moveTime);
			sprite.color = Color.Lerp(startColor, endColor, moveElapsed/moveTime);
			
			moveElapsed += Time.deltaTime;
			yield return null;
		}
		
		path.cell.AddUnit(this);
		Destroy(gameObject);
	}
	
	public void SlowDown() {
		//audio.PlayOneShot(SlowDownAudio);
		speed = slowSpeed;
	}
	
	public void NormalSpeed () {
		speed = normalSpeed;
	}
	
	void OnCollisionEnter(Collision c) {
		Unit unit = c.collider.GetComponent<Unit>();
		if (unit) {
			c.collider.enabled = false;
			collider.enabled = false;
			
			unit.destroyNextTurn = true;
			destroyNextTurn = true;
			
			UpdateAnimation();
		}
	}
	
	void UpdateAnimation() {
		Vector3 direction = lastDirection;
		string animName = "";
		
		if (destroyNextTurn) {
			if (Mathf.Abs (direction.x) > Mathf.Abs (direction.y)) {
				animName = AnimationPrefix + "ShootAndDieSide";
				if (direction.x > 0.0f) {
					Vector3 scale = transform.localScale;
					scale.x = AnimationPrefix == "Red" ? -1.0f : 1.0f;
					transform.localScale = scale;
				}
				else {
					Vector3 scale = transform.localScale;
					scale.x = AnimationPrefix == "Red" ? 1.0f : -1.0f;
					transform.localScale = scale;	
				}
			}
			else if (Mathf.Abs (direction.y) > Mathf.Abs (direction.x)) {
				if (direction.y > 0.0f) animName = AnimationPrefix + "ShootAndDieUp";
				else animName = AnimationPrefix + "ShootAndDieDown";
			}
			
			if (!animation.IsPlaying(animName)) {
				animation.Play(animName);	
				
				if (team == Team.BLUE) {
					audio.PlayOneShot(HitAudio);
					audio.PlayOneShot(DeathAudio, 1.0f);
				}
				
			}
			
			return;
		}
		
		if (Mathf.Abs (direction.x) > Mathf.Abs (direction.y)) {
			animName = AnimationPrefix + "WalkSide";
			if (!animation.IsPlaying(animName)) {
				animation.Play(animName);	
			}
			
			if (direction.x > 0.0f) {
				Vector3 scale = transform.localScale;
				scale.x = AnimationPrefix == "Red" ? -1.0f : 1.0f;
				transform.localScale = scale;
			}
			else {
				Vector3 scale = transform.localScale;
				scale.x = AnimationPrefix == "Red" ? 1.0f : -1.0f;
				transform.localScale = scale;	
			}
		}
		else if (Mathf.Abs (direction.y) > Mathf.Abs (direction.x)) {
			if (direction.y > 0.0f) {
				animName = AnimationPrefix + "WalkUp";
				if (!animation.IsPlaying(animName)) {
					animation.Play(animName);	
				}
			}
			else if (direction.y < 0.0f){
				animName = AnimationPrefix + "WalkDown";
				if (!animation.IsPlaying(animName)) {
					animation.Play(animName);	
				}
			}
		}
	}
}

